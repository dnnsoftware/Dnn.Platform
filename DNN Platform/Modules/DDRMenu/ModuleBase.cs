using System;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.DDRMenu
{
	public class ModuleBase : PortalModuleBase
	{
		protected String GetStringSetting(String name, String defaultValue)
		{
			var result = Request.QueryString[name];
			if (String.IsNullOrEmpty(result))
			{
				result = (String)Settings[name];
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
			return GetStringSetting(name, "");
		}

		protected Int32 GetIntSetting(String name, Int32 defaultValue)
		{
			return Convert.ToInt32(GetStringSetting(name, defaultValue.ToString()));
		}

		protected Int32 GetIntSetting(String name)
		{
			return GetIntSetting(name, 0);
		}

		protected Boolean GetBoolSetting(String name, Boolean defaultValue)
		{
			var result = GetStringSetting(name);
			return (result == "") ? defaultValue : Convert.ToBoolean(result);
		}

		protected Boolean GetBoolSetting(String name)
		{
			return GetBoolSetting(name, false);
		}
	}
}