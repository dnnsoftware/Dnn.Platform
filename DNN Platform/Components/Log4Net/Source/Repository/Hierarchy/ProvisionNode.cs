using System.Collections;

namespace log4net.Repository.Hierarchy
{
	internal sealed class ProvisionNode : ArrayList
	{
		internal ProvisionNode(Logger log)
		{
			this.Add(log);
		}
	}
}