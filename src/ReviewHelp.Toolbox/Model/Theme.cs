using System.Drawing;

namespace ReviewHelp.Toolbox.Model
{
	public sealed class Theme
	{
		public static readonly Theme Default = new Theme();

		private Theme()
		{
			Plugin = Color.Yellow;
			WiringHeader = Color.Green;
		}

		public Color Plugin { get; }
		public Color WiringHeader { get; }
	}
}