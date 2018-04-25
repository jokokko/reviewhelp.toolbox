using System.Collections.Generic;
using ReviewHelp.Toolbox.Model;

namespace ReviewHelp.Toolbox.Services
{
	public interface IStructureMapWiringReporter
	{
		void WriteWirings(IEnumerable<StructureMapWiringCtx> items, string header = null);
	}
}