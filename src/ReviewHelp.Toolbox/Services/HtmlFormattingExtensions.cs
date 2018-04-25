using HtmlAgilityPack;

namespace ReviewHelp.Toolbox.Services
{
	internal static class HtmlFormattingExtensions
	{
		public static string Enc(this string value)
		{
			return HtmlDocument.HtmlEncode(value);
		}
	}
}