using System;
using System.Web.UI;
using DotNetNuke.Framework;

namespace DotNetNuke.Common.Utilities
{
    public class JavaScriptUtils: ServiceLocator<IJavaScriptUtils,JavaScriptUtils>, IJavaScriptUtils
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
