using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReviewHelp.Toolbox.Model
{
    public sealed class WiringCtx
    {
        public WiringCtx(InvocationExpressionSyntax invocation, ITypeSymbol plugin, Lifecycle lifecycle, ITypeSymbol concretePlugin = null)
        {
            Invocation = invocation;            
	        Plugin = plugin;
	        Lifecycle = lifecycle;
	        ConcretePlugin = concretePlugin;
        }

        public InvocationExpressionSyntax Invocation { get; }        
	    public ITypeSymbol Plugin { get; }
	    public Lifecycle Lifecycle { get; }
	    public ITypeSymbol ConcretePlugin { get; }
    }
}