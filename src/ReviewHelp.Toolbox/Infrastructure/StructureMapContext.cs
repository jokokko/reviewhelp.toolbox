using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReviewHelp.Toolbox.Infrastructure
{
    public sealed class StructureMapContext
    {        
        public readonly Version Version;
        public StructureMapContext(Compilation compilation)
        {            
            Version = compilation.ReferencedAssemblyNames
                .FirstOrDefault(a => a.Name.Equals(Constants.StructureMapAssembly, StringComparison.OrdinalIgnoreCase))
                ?.Version;            
        }
    }
}