using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReviewHelp.Toolbox.Infrastructure
{
	public static class OnInvocationMixins
	{
		public static bool MatchInvocation(this IOnMethodInvocation instance, IMethodSymbol methodSymbol)
		{
			return instance.OnMethods.Contains($"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}");
		}

		public static bool MatchInvocation(this IOnMethodInvocation instance, IMethodSymbol methodSymbol, IEnumerable<string> methods)
		{
			return methods.Contains($"{methodSymbol.ContainingType.Name}.{methodSymbol.Name}", StringComparer.Ordinal);
		}
	}
}