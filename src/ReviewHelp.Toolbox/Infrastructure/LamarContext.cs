using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReviewHelp.Toolbox.Infrastructure
{
	public sealed class LamarContext
	{        
		public readonly Version Version;
		public LamarContext(Compilation compilation)
		{            
			Version = compilation.ReferencedAssemblyNames
				.FirstOrDefault(a => a.Name.Equals(Constants.LamarAssembly, StringComparison.OrdinalIgnoreCase))
				?.Version;            
		}
	}
}