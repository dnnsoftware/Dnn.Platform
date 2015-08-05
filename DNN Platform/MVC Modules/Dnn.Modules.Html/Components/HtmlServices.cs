using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.Modules.Html.Components
{
	/// <summary>
	/// Html Module Upgradable Controller.
	/// </summary>
	public class HtmlServices : IUpgradeable
	{
		/// <summary>
		/// Upgrade Module.
		/// </summary>
		/// <param name="version"></param>
		/// <returns></returns>
		public string UpgradeModule(string version)
		{
			switch (version)
			{
				case "07.05.00":
					break;
			}

			return string.Empty;
		}

	}
}