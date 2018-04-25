using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using ReviewHelp.Toolbox.Analyzers;

namespace ReviewHelp.Toolbox.Services
{
	public sealed class StructureMapWiringCataloger
    {
        public async Task BuildCatalog(IEnumerable<string> solutions, IStructureMapWiringReporter reporter, Dictionary<string, string> solutionProperties = null)
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
	        
	        var wirings = collector.GetWirings();

	        reporter.WriteWirings(wirings);
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