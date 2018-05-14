using System.Collections.Generic;
using System.Linq;
using Colorful;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public sealed class LamarWiringConsoleReporter : ILamarWiringReporter
	{
		private readonly Theme theme;

		public LamarWiringConsoleReporter(Theme theme)
		{
			this.theme = theme;
		}

		private static void WriteLocations(LamarWiringCtx item)
		{
			Console.WriteLine($"\t\t{item.Invocation.GetLocation()}");
		}

		public void WriteWirings(IEnumerable<LamarWiringCtx> wirings, string header = null)
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