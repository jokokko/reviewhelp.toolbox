using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;
using ReviewHelp.Toolbox.Analyzers;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public sealed class LamarWiringCataloger
	{
		public async Task BuildCatalog(IEnumerable<string> solutions, ILamarWiringReporter reporter, Dictionary<string, string> solutionProperties = null)
		{
			if (solutions == null)
			{
				throw new ArgumentNullException(nameof(solutions));
			}

			if (reporter == null)
			{
				throw new ArgumentNullException(nameof(reporter));
			}

			var solutionsToAnalyze = solutions as string[] ?? solutions.ToArray();

			if (!solutionsToAnalyze.Any())
			{
				return;
			}
			
			var results = await Task.WhenAll(solutionsToAnalyze.Select(x => AnalyzeSolution(x, solutionProperties)).ToArray())
				.ConfigureAwait(false);

			var wirings = results.SelectMany(x => x);

			reporter.WriteWirings(wirings);
		}

		private static async Task<IEnumerable<LamarWiringCtx>> AnalyzeSolution(string solutionPath, Dictionary<string, string> workspaceProperties = null)
		{			
			var analyzer = new LamarWiringAnalyzer();

			var solution = await MSBuildWorkspace.Create(workspaceProperties ?? new Dictionary<string, string>()).OpenSolutionAsync(solutionPath).ConfigureAwait(false);
			var analyzers = ImmutableArray.Create((DiagnosticAnalyzer)analyzer);

			foreach (var s in solution.Projects)
			{
				var compilation = await s.GetCompilationAsync().ConfigureAwait(false);
				await compilation.WithAnalyzers(analyzers).GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);				
			}

			return analyzer.GetWirings();
		}
	}
}