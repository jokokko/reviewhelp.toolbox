using System.Collections.Generic;
using System.Linq;
using Colorful;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public sealed class StructureMapWiringConsoleReporter : IStructureMapWiringReporter
	{
		private readonly Theme theme;

		public StructureMapWiringConsoleReporter(Theme theme)
		{
			this.theme = theme;
		}

		private void WriteLocations(StructureMapWiringCtx item)
		{
			Console.WriteLine($"\t\t{item.Invocation.GetLocation()}");
		}

		public void WriteWirings(IEnumerable<StructureMapWiringCtx> wirings, string header = null)
		{
			var wiringsGrouped =
				from w in wirings
				group w by w.Lifecycle
				into g
				select g;

			if (header != null)
			{
				Console.WriteLine(header, theme.WiringHeader);
			}

			foreach (var items in wiringsGrouped.OrderBy(x => x.Key.Name))
			{
				foreach (var s in items.OrderBy(x => x.Plugin.Name))
				{
					Console.WriteLine($"\t{s.Plugin.ToDisplayString()} -> {s.ConcretePlugin?.ToDisplayString()}", theme.Plugin);
					WriteLocations(s);
				}
			}
		}

	}
}