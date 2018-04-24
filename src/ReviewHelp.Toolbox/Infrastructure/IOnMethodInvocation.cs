using System.Collections.Generic;

namespace ReviewHelp.Toolbox.Infrastructure
{
	public interface IOnMethodInvocation
	{
		HashSet<string> OnMethods { get; }
	}
}