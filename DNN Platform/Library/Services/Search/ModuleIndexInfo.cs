#region Usings

using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace DotNetNuke.Services.Search
{
	internal class ModuleIndexInfo
	{
		public ModuleInfo ModuleInfo { get; set; }

		public bool SupportSearch { get; set; }
	}
}
