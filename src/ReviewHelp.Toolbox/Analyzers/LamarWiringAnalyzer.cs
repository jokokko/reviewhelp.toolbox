using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReviewHelp.Toolbox.Infrastructure;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class LamarWiringAnalyzer : LamarAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.Lamar1000WiredPluginsAndLifecycles);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext ctx, LamarContext lamarCtx)
		{
			var analyzer = new Analyzer(this);
			ctx.RegisterSyntaxNodeAction(analyzer.AnalyzeInvocation, SyntaxKind.InvocationExpression);
			ctx.RegisterSyntaxNodeAction(analyzer.AnalyzeAssignment, SyntaxKind.SimpleAssignmentExpression);			
			ctx.RegisterCompilationEndAction(analyzer.End);
		}

		private readonly ConcurrentBag<LamarWiringCtx> wirings = new ConcurrentBag<LamarWiringCtx>();		

		public IEnumerable<LamarWiringCtx> GetWirings()
		{
			return wirings;
		}

		private sealed class Analyzer : IOnMethodInvocation
		{
			private readonly LamarWiringAnalyzer host;
			
			public HashSet<string> OnMethods => new HashSet<string>(new[]
			{
				"ServiceRegistry.For",
				"ServiceRegistry.ForSingletonOf",
			});
			
			public void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
			{                
				var node = (InvocationExpressionSyntax)context.Node;
				var symbol = context.SemanticModel.GetSymbolInfo(node);

				if (symbol.Symbol?.Kind != SymbolKind.Method)
				{
					return;
				}

				var method = (IMethodSymbol)symbol.Symbol;
				if (this.MatchInvocation(method))
				{
					AnalyzeInvocation(context, node, method);
				}
			}
						
			public Analyzer(LamarWiringAnalyzer host)
			{
				this.host = host;								
			}

			private static readonly Dictionary<string, LamarLifecycle> LifecycleMap = new Dictionary<string, LamarLifecycle>
			{
				{ "Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton", LamarLifecycle.Singleton },
				{ "Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped", LamarLifecycle.Scoped },
				{ "Microsoft.Extensions.DependencyInjection.ServiceLifetime.Transient", LamarLifecycle.Transient },
			};

			private sealed class StructureMapConfigurationInvocation
			{
				public IMethodSymbol MethodSymbol { get; }
				public InvocationExpressionSyntax Invocation { get; }

				public StructureMapConfigurationInvocation(IMethodSymbol methodSymbol, InvocationExpressionSyntax invocation)
				{
					MethodSymbol = methodSymbol;
					Invocation = invocation;
				}
			}

			private void AnalyzeInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax node, IMethodSymbol method)
			{
				var invocationNodes = node.Parent.Ancestors().OfType<InvocationExpressionSyntax>().ToArray();

				var invocations = (from i in invocationNodes
								   let sym = context.SemanticModel.GetSymbolInfo(i)
								   where sym.Symbol?.Kind == SymbolKind.Method
								   let methodSymbol = (IMethodSymbol)sym.Symbol
								   select new StructureMapConfigurationInvocation(methodSymbol, i)).ToArray();

				ITypeSymbol plugin;
				LamarLifecycle lifecycle = null;

				if (method.Name.Equals("ForSingletonOf"))
				{
					lifecycle = LamarLifecycle.Singleton;
				}

				var assignment = node.Ancestors().OfType<AssignmentExpressionSyntax>().FirstOrDefault();

				ISymbol assignedTo = null;

				if (assignment != null)
				{
					assignedTo = context.SemanticModel.GetSymbolInfo(assignment.Left).Symbol;
				}

				var variableDeclaration = node.Ancestors().OfType<VariableDeclarationSyntax>().FirstOrDefault();

				if (variableDeclaration != null)
				{
					assignedTo = context.SemanticModel.GetDeclaredSymbol(variableDeclaration.Variables[0]);					
				}

				ITypeSymbol PluginFromArgument(ExpressionSyntax expr)
				{
					// ReSharper disable once ConvertIfStatementToSwitchStatement
					if (expr is TypeOfExpressionSyntax toe)
					{
						var typeInfo = context.SemanticModel.GetTypeInfo(toe.Type);
						return typeInfo.Type;
					}

					// ReSharper disable once InvertIf
					if (expr is InvocationExpressionSyntax ies && ies.Expression is MemberAccessExpressionSyntax mes)
					{
						var sym = context.SemanticModel.GetSymbolInfo(ies);
						// ReSharper disable once InvertIf
						if (sym.Symbol != null && sym.Symbol.Name.Equals("GetType"))
						{
							var typeInfo = context.SemanticModel.GetTypeInfo(mes.Expression);
							return typeInfo.Type;
						}
					}

					if (expr is IdentifierNameSyntax ins)
					{
						var sym = context.SemanticModel.GetTypeInfo(ins);
						return sym.Type;
					}

					return null;
				}

				LamarLifecycle LifecycleFromArgument(ExpressionSyntax expr)
				{
					var lifecycleType = context.SemanticModel.GetTypeInfo(expr).Type;
					return LifecycleMap[lifecycleType.ToDisplayString()];
				}

				if (method.IsGenericMethod)
				{
					plugin = method.TypeArguments[0];

					if (node.ArgumentList.Arguments.Count > 0)
					{
						lifecycle = LifecycleFromArgument(node.ArgumentList.Arguments[0].Expression);
					}
				}
				else
				{
					var expr = node.ArgumentList.Arguments[0].Expression;

					plugin = PluginFromArgument(expr);					
				}

				if (plugin == null)
				{
					return;
				}

				LamarLifecycle FromInvocation(StructureMapConfigurationInvocation i)
				{
					switch (i.MethodSymbol.Name)
					{						
						case "Singleton":
							{
								return LamarLifecycle.Singleton;
							}
						case "Scoped":
							{
								return LamarLifecycle.Scoped;
							}
						case "Transient":
							{
								return LamarLifecycle.Transient;
							}
						default:
							{
								return null;
							}
					}
				}

				lifecycle = lifecycle ?? invocations.Select(FromInvocation).FirstOrDefault(x => x != null) ?? LamarLifecycle.TransientImplicit;

				var concretesPluggedBy = new[] { "Use", "Add" };

				var concretePluginInvocation =
					invocations.FirstOrDefault(x => concretesPluggedBy.Any(s => s.Equals(x.MethodSymbol.Name, StringComparison.Ordinal)));

				var concretePlugin = plugin;

				ITypeSymbol ConcretePluginFrom(StructureMapConfigurationInvocation invocation)
				{
					if (invocation.MethodSymbol.IsGenericMethod)
					{
						return invocation.MethodSymbol.TypeArguments[0];
					}

					var expr = invocation.Invocation.ArgumentList.Arguments[invocation.Invocation.ArgumentList.Arguments.Count - 1].Expression;
					var pluginToReturn = PluginFromArgument(expr);

					if (pluginToReturn == null)
					{
						if (expr is LambdaExpressionSyntax les)
						{
							var bodyType = context.SemanticModel.GetTypeInfo(les.Body);
							return bodyType.Type ?? (context.SemanticModel.GetSymbolInfo(expr).Symbol as IMethodSymbol)?.ReturnType;
						}
					}

					return pluginToReturn;
				}

				if (concretePluginInvocation != null)
				{
					concretePlugin = ConcretePluginFrom(concretePluginInvocation);
				}

				var assembly = context.Compilation.Assembly;

				var wiringCtx = new LamarWiringCtx(node, plugin, lifecycle, assembly, concretePlugin, assignedTo);

				host.wirings.Add(wiringCtx);
			}

			public void AnalyzeAssignment(SyntaxNodeAnalysisContext ctx)
			{
				var assignment = (AssignmentExpressionSyntax)ctx.Node;

				var sym = ctx.SemanticModel.GetSymbolInfo(assignment.Left);

				if (sym.Symbol == null)
				{
					return;
				}

				if (!sym.Symbol.Name.Equals("Lifetime", StringComparison.CurrentCulture) ||
				    !sym.Symbol.ContainingType.ToDisplayString().Equals("Lamar.IoC.Instances.Instance"))
				{
					return;
				}
				
				var lifecycleSym = ctx.SemanticModel.GetSymbolInfo(assignment.Right);

				if (lifecycleSym.Symbol == null)
				{
					return;
				}

				if (!(assignment.Left is MemberAccessExpressionSyntax memberAccess))
				{
					return;
				}

				var registration = ctx.SemanticModel.GetSymbolInfo(memberAccess.Expression);

				if (registration.Symbol != null && LifecycleMap.TryGetValue(lifecycleSym.Symbol.ToDisplayString(), out var value))
				{
					lifecycles.Add(new LifecyclePerSymbol
					{
						Symbol = registration.Symbol,
						Lifecycle = value
					});
				}
			}

			private sealed class LifecyclePerSymbol
			{
				public ISymbol Symbol;
				public LamarLifecycle Lifecycle;
			}

			private readonly ConcurrentBag<LifecyclePerSymbol> lifecycles = new ConcurrentBag<LifecyclePerSymbol>();
			
			public void End(CompilationAnalysisContext ctx)
			{
				foreach (var item in lifecycles.Join(host.wirings, s => s.Symbol, s => s.AssignedTo, (lf, wiringCtx) => new { symbol = lf, wiringCtx }))
				{
					item.wiringCtx.Lifecycle = item.symbol.Lifecycle;					
				}
			}
		}
	}	
}