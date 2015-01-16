using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{
	public class BasicClientDependencyFile : IClientDependencyFile
	{
		public BasicClientDependencyFile(ClientDependencyType type)
		{
			DependencyType = type;
		}

		#region IClientDependencyFile Members

		public string FilePath { get; set; }
		public ClientDependencyType DependencyType { get; private set; }
		public int Priority { get; set; }
		public string PathNameAlias { get; set; }
		/// <summary>
		/// This can be empty and will use default provider
		/// </summary>
		public string ForceProvider { get; set; }

		#endregion
	}
}
