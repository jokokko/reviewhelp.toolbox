using System.Collections.Generic;
using System.IO;
using System.Linq;
using Colorful;
using HtmlAgilityPack;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public sealed class LamarWiringHtmlReporter : ILamarWiringReporter
	{
		public void WriteWirings(IEnumerable<LamarWiringCtx> items, string header = null)
		{
			var doc = new HtmlDocument();
			var body = HtmlNode.CreateNode("<html><head></head><body></body></html>");
			doc.DocumentNode.AppendChild(body);

			var table = doc.CreateElement("table");

			var tableHeader = HtmlNode.CreateNode( $@"<thead><tr><th>Plugin</th><th>Concrete Plugin</th><th>Lifecycle</th><th>Assembly</th><th>Source Location</th></thead>");
			var tableBody = doc.CreateElement("tbody");

			tableBody = items.OrderBy(x => x.Assembly?.Name).ThenBy(x => x.Lifecycle.Name).Aggregate(tableBody, (node, w) =>
			{
				var row = HtmlNode.CreateNode($@"<tr><td><pre>{w.Plugin.ToDisplayString().Enc()}</pre></td><td><pre>{w.ConcretePlugin?.ToDisplayString().Enc()}</pre></td><td><pre>{w.Lifecycle.Name.Enc()}</pre></td><td><pre>{w.Assembly?.Name.Enc()}</pre></td><td><pre>{w.Invocation.GetLocation().ToString().Enc()}</pre></td></tr>");

				tableBody.AppendChild(row);

				return tableBody;
			});

			table.AppendChild(tableHeader);
			table.AppendChild(tableBody);
			
			doc.DocumentNode.SelectSingleNode("//body").AppendChild(table);

			using (var sb = new StringWriter())
			{
				doc.Save(sb);
				Console.Write(sb);
			}
		}
	}
}