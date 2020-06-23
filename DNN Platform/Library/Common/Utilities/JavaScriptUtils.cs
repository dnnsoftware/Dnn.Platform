// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Framework;

    public class JavaScriptUtils : ServiceLocator<IJavaScriptUtils, JavaScriptUtils>, IJavaScriptUtils
    {
        public void RegisterJavascriptVariable(string variableName, object value, Page page, Type type)
        {
            var valueAsJson = Json.Serialize(value);

            var script = string.Format("var {0} = {1};", variableName, valueAsJson);

            if (ScriptManager.GetCurrent(page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(page, type, variableName, script, true);
            }
            else
            {
                page.ClientScript.RegisterStartupScript(type, variableName, script, true);
            }
        }

        protected override Func<IJavaScriptUtils> GetFactory()
        {
            return () => new JavaScriptUtils();
        }
    }
}
