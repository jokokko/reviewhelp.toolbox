using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using ReviewHelp.Toolbox.Analyzers;
using ReviewHelp.Toolbox.Model;
using Console = Colorful.Console;

namespace ReviewHelp.Toolbox.Services
{
    public sealed class StructureMapWiringCataloger
    {
	    private class Theme
	    {
			public static readonly Theme Current = new Theme();

		    private Theme()
		    {
			    Plugin = Color.Yellow;
			    WiringHeader = Color.Green;			    
		    }

		    public Color Plugin { get; }
		    public Color WiringHeader { get; }		    
	    }

        public async Task BuildCatalog(IEnumerable<string> solutions, Dictionary<string, string> solutionProperties = null)
        {
            if (solutions == null)
            {
                throw new ArgumentNullException(nameof(solutions));
            }

            var solutionsToAnalyze = solutions as string[] ?? solutions.ToArray();

            if (!solutionsToAnalyze.Any())
            {
                return;
            }

            var collector = new StructureMapWiringAnalyzer();

            await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, collector, solutionProperties)).ToArray())
                .ConfigureAwait(false);

            void WriteLocations(WiringCtx item)
            {
				Console.WriteLine($"\t\t{item.Invocation.GetLocation()}");
			}

	        var theme = Theme.Current;

            void WriteWirings(IGrouping<Lifecycle, WiringCtx> items, string header = null)
            {
                if (header != null)
                {
                    Console.WriteLine(header, theme.WiringHeader);
                }

                foreach (var s in items.OrderBy(x => x.Plugin.Name))
                {	                
                    Console.WriteLine($"\t{s.Plugin.ToDisplayString()} -> {s.ConcretePlugin?.ToDisplayString()}", theme.Plugin);
                    WriteLocations(s);
                }
            }

	        var wirings = collector.GetWirings();

	        var wiringsGrouped  =
		        from w in wirings
		        group w by w.Lifecycle into g
		        select g;
		
            foreach (var p in wiringsGrouped.OrderBy(x => x.Key.Name))
            {
                WriteWirings(p, $"Wired as `{p.Key.Name}`");
            }
        }

        private static async Task AnalyzeSolution(string solutionPath, DiagnosticAnalyzer analyzer, Dictionary<string, string> workspaceProperties = null)
        {			
            var solution = await MSBuildWorkspace.Create(workspaceProperties ?? new Dictionary<string, string>()).OpenSolutionAsync(solutionPath).ConfigureAwait(false);
            var analyzers = ImmutableArray.Create(analyzer);

            foreach (var s in solution.Projects)
            {
                var compilation = await s.GetCompilationAsync().ConfigureAwait(false);
                await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
            }
        }
    }
}