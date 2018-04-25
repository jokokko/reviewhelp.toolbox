using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReviewHelp.Toolbox.Model
{
    public sealed class StructureMapWiringCtx
    {
        public StructureMapWiringCtx(InvocationExpressionSyntax invocation, ITypeSymbol plugin, StructureMapLifecycle lifecycle, IAssemblySymbol assembly, ITypeSymbol concretePlugin = null)
        {
            Invocation = invocation;            
	        Plugin = plugin;
	        Lifecycle = lifecycle;
	        Assembly = assembly;
	        ConcretePlugin = concretePlugin;
        }

        public InvocationExpressionSyntax Invocation { get; }        
	    public ITypeSymbol Plugin { get; }
	    public StructureMapLifecycle Lifecycle { get; }
	    public IAssemblySymbol Assembly { get; }
	    public ITypeSymbol ConcretePlugin { get; }
    }
}