using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Oakton;
using ReviewHelp.Toolbox.Model;
using ReviewHelp.Toolbox.Services;

namespace ReviewHelp.Toolbox.Commands
{
	[Description("Analyze wired plugins and their lifecycles in Lamar configurations", Name = "lamar-wirings")]
	[UsedImplicitly]
	public sealed class AnalyzeLamarWirings : OaktonAsyncCommand<SolutionInput>
	{
		public override async Task<bool> Execute(SolutionInput input)
		{
			var service = new LamarWiringCataloger();

			var solutionProperties = new Dictionary<string, string>();

			if (!string.IsNullOrEmpty(input.TargetFrameworkFlag))
			{
				solutionProperties["TargetFramework"] = input.TargetFrameworkFlag;
			}

			var theme = Theme.Default;

			var reporter = input.HtmlFlag
				? (ILamarWiringReporter)new LamarWiringHtmlReporter()
				: new LamarWiringConsoleReporter(theme);

			await service.BuildCatalog(input.Solutions.Where(File.Exists), reporter, solutionProperties);

			return true;
		}
	}
}