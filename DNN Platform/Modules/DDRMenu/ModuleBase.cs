// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.DDRMenu
{
	public class ModuleBase : PortalModuleBase
	{
		protected String GetStringSetting(String name, String defaultValue)
		{
			var result = this.Request.QueryString[name];
			if (String.IsNullOrEmpty(result))
			{
				result = (String)this.Settings[name];
			}
			if (String.IsNullOrEmpty(result))
			{
				result = defaultValue;
			}

			if (result != null)
			{
				result = result.Trim();
			}
			if (result == "-")
			{
				result = "";
			}

			return result;
		}

		protected String GetStringSetting(String name)
		{
			return this.GetStringSetting(name, "");
		}

		protected Int32 GetIntSetting(String name, Int32 defaultValue)
		{
			return Convert.ToInt32(this.GetStringSetting(name, defaultValue.ToString()));
		}

		protected Int32 GetIntSetting(String name)
		{
			return this.GetIntSetting(name, 0);
		}

		protected Boolean GetBoolSetting(String name, Boolean defaultValue)
		{
			var result = this.GetStringSetting(name);
			return (result == "") ? defaultValue : Convert.ToBoolean(result);
		}

		protected Boolean GetBoolSetting(String name)
		{
			return this.GetBoolSetting(name, false);
		}
	}
}
