﻿using System;
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
	public sealed class StructureMapWiringAnalyzer : StructureMapAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptors.StructureMap1000WiredPluginsAndLifecycles);

		protected override void AnalyzeCompilation(CompilationStartAnalysisContext ctx, StructureMapContext structureMapCtx)
		{
			var analyzer = new Analyzer(this);
			ctx.RegisterSyntaxNodeAction(analyzer.Analyze, SyntaxKind.InvocationExpression);
		}

		private readonly ConcurrentBag<StructureMapWiringCtx> wirings = new ConcurrentBag<StructureMapWiringCtx>();

		public IEnumerable<StructureMapWiringCtx> GetWirings()
		{
			return wirings;
		}

		private sealed class Analyzer : IOnMethodInvocation
		{
			private readonly StructureMapWiringAnalyzer host;
			
			public HashSet<string> OnMethods => new HashSet<string>(new[]
			{
				"Registry.For",
				"Registry.ForSingletonOf",
			});
			
			public void Analyze(SyntaxNodeAnalysisContext context)
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
						
			public Analyzer(StructureMapWiringAnalyzer host)
			{
				this.host = host;								
			}

			private static readonly Dictionary<string, StructureMapLifecycle> LifecycleMap = new Dictionary<string, StructureMapLifecycle>
			{
				{ "StructureMap.Pipeline.SingletonLifecycle", StructureMapLifecycle.Singleton },
				{ "StructureMap.Pipeline.ContainerLifecycle", StructureMapLifecycle.Container },
				{ "StructureMap.Pipeline.ThreadLocalStorageLifecycle", StructureMapLifecycle.ThreadLocal },
				{ "StructureMap.Pipeline.TransientLifecycle", StructureMapLifecycle.Transient },
				{ "StructureMap.Pipeline.UniquePerRequestLifecycle", StructureMapLifecycle.Unique },
				{ "StructureMap.Pipeline.ChildContainerSingletonLifecycle", StructureMapLifecycle.ChildContainerSingleton }
				// ObjectLifecycle SM for injected instances
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
				StructureMapLifecycle lifecycle = null;

				if (method.Name.Equals("ForSingletonOf"))
				{
					lifecycle = StructureMapLifecycle.Singleton;
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

				StructureMapLifecycle LifecycleFromArgument(ExpressionSyntax expr)
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

					if (node.ArgumentList.Arguments.Count > 1)
					{
						lifecycle = LifecycleFromArgument(node.ArgumentList.Arguments[1].Expression);
					}
				}
	
				if (plugin == null)
				{
					return;
				}

				StructureMapLifecycle FromInvocation(StructureMapConfigurationInvocation i)
				{
					switch (i.MethodSymbol.Name)
					{
						case "LifecycleIs":
						{
							if (i.MethodSymbol.IsGenericMethod)
							{
								var lifecycleType = i.MethodSymbol.TypeArguments[0];
								return LifecycleMap[lifecycleType.ToDisplayString()];
							}
							else
							{
								var lifecycleType = context.SemanticModel.GetTypeInfo(i.Invocation.ArgumentList.Arguments[0].Expression).Type;
								return LifecycleMap[lifecycleType.ToDisplayString()];
							}
						}
						case "Singleton":
						{
							return StructureMapLifecycle.Singleton;							
						}
						case "AlwaysUnique":
						{
							return StructureMapLifecycle.Unique;							
						}
						case "Transient":
						{
							return StructureMapLifecycle.Transient;							
						}
						default:
						{
							return null;
						}
					}					
				}

				lifecycle = lifecycle ?? invocations.Select(FromInvocation).FirstOrDefault(x => x != null) ?? StructureMapLifecycle.TransientImplicit;

				var concretesPluggedBy = new[] {"Use", "Add"};

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

				var wiringCtx = new StructureMapWiringCtx(node, plugin, lifecycle, assembly, concretePlugin);
				
				host.wirings.Add(wiringCtx);
			}
		}
	}
}