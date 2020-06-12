// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.DDRMenu
{
    public class ModuleBase : PortalModuleBase
    {
        protected string GetStringSetting(string name, string defaultValue)
        {
            var result = this.Request.QueryString[name];
            if (string.IsNullOrEmpty(result))
            {
                result = (string)this.Settings[name];
            }
            if (string.IsNullOrEmpty(result))
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

        protected string GetStringSetting(string name)
        {
            return this.GetStringSetting(name, "");
        }

        protected int GetIntSetting(string name, int defaultValue)
        {
            return Convert.ToInt32(this.GetStringSetting(name, defaultValue.ToString()));
        }

        protected int GetIntSetting(string name)
        {
            return this.GetIntSetting(name, 0);
        }

        protected bool GetBoolSetting(string name, bool defaultValue)
        {
            var result = this.GetStringSetting(name);
            return (result == "") ? defaultValue : Convert.ToBoolean(result);
        }

        protected bool GetBoolSetting(string name)
        {
            return this.GetBoolSetting(name, false);
        }
    }
}
