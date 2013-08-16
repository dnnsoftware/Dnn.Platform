using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientDependency.Core
{
	public interface IClientDependencyPath
	{

		string Name { get; set; }
		string Path { get; set; }
		bool ForceBundle { get; set; }

	}
}
