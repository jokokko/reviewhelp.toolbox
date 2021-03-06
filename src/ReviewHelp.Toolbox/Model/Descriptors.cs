﻿using Microsoft.CodeAnalysis;
using ReviewHelp.Toolbox.Infrastructure;

namespace ReviewHelp.Toolbox.Model
{
    internal static class Descriptors
    {
        private static DiagnosticDescriptor Rule(string id, string title, RuleCategory category, DiagnosticSeverity defaultSeverity, string messageFormat, string description = null)
        {            
            return new DiagnosticDescriptor(id, title, messageFormat, category.Name, defaultSeverity, true, description);
        }
        
	    internal static readonly DiagnosticDescriptor StructureMap1000WiredPluginsAndLifecycles = Rule("SM1000", "Wired plugin and lifecycle", RuleCategory.Usage, DiagnosticSeverity.Info, "lifecycle");
	    internal static readonly DiagnosticDescriptor Lamar1000WiredPluginsAndLifecycles = Rule("LM1000", "Wired plugin and lifecycle", RuleCategory.Usage, DiagnosticSeverity.Info, "lifecycle");
	}
}