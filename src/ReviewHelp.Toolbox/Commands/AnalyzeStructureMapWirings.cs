using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Oakton;
using ReviewHelp.Toolbox.Model;
using ReviewHelp.Toolbox.Services;

namespace ReviewHelp.Toolbox.Commands
{
    [Description("Analyze wired plugins and their lifecycles in StructureMap configurations", Name = "structuremap-wirings")]
    public sealed class AnalyzeStructureMapWirings : OaktonAsyncCommand<SolutionInput>
    {
        public override async Task<bool> Execute(SolutionInput input)
        {
            var service = new StructureMapWiringCataloger();

	        var solutionProperties = new Dictionary<string, string>();

	        if (!string.IsNullOrEmpty(input.TargetFrameworkFlag))
	        {
		        solutionProperties["TargetFramework"] = input.TargetFrameworkFlag;
	        }

	        var theme = Theme.Default;

	        var reporter = input.HtmlFlag
		        ? (IStructureMapWiringReporter)new StructureMapWiringHtmlReporter()
		        : new StructureMapWiringConsoleReporter(theme);

            await service.BuildCatalog(input.Solutions.Where(File.Exists), reporter, solutionProperties);

            return true;
        }
    }
}