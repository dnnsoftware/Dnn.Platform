using DotNetNuke.Entities.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dnn.Modules.Html.Components
{
	public class HtmlServices : IUpgradeable
	{
		public string UpgradeModule(string Version)
		{
			switch (Version)
			{
				case "07.05.00":
					break;
			}

			return string.Empty;
		}

	}
}