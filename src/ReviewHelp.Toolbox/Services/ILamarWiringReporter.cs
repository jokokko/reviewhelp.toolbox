using System.Collections.Generic;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public interface ILamarWiringReporter
	{
		void WriteWirings(IEnumerable<LamarWiringCtx> items, string header = null);
	}
}