using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReviewHelp.Toolbox.Model
{	
	public sealed class LamarWiringCtx
	{
		public LamarWiringCtx(InvocationExpressionSyntax invocation, ITypeSymbol plugin, LamarLifecycle lifecycle,
			IAssemblySymbol assembly, ITypeSymbol concretePlugin = null, ISymbol assignedTo = null)
		{
			Invocation = invocation;            
			Plugin = plugin;
			Lifecycle = lifecycle;
			Assembly = assembly;
			ConcretePlugin = concretePlugin;
			AssignedTo = assignedTo;
		}

		public InvocationExpressionSyntax Invocation { get; }        
		public ITypeSymbol Plugin { get; }
		public LamarLifecycle Lifecycle { get; internal set; }
		public IAssemblySymbol Assembly { get; }
		public ITypeSymbol ConcretePlugin { get; }
		public ISymbol AssignedTo { get; }
	}
}