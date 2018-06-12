// // DotNetNuke® - http://www.dotnetnuke.com
// // Copyright (c) 2002-2018
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Web.Helpers;
using System.Web.UI;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Framework
{
    internal class ServicesFrameworkImpl : IServicesFramework, IServiceFrameworkInternals
    {
        private const string AntiForgeryKey = "dnnAntiForgeryRequested";
        private const string ScriptKey = "dnnSFAjaxScriptRequested";

        public void RequestAjaxAntiForgerySupport()
        {
            RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
        }

        public bool IsAjaxAntiForgerySupportRequired
        {
            get { return CheckKey(AntiForgeryKey); }
        }

        public void RegisterAjaxAntiForgery(Page page)
        {
            var ctl = page.FindControl("ClientResourcesFormBottom");
            if(ctl != null)
            {
                ctl.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
            }
        }

        public bool IsAjaxScriptSupportRequired
        {
            get{ return CheckKey(ScriptKey); }
        }

        public void RequestAjaxScriptSupport()
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            SetKey(ScriptKey);
        }

        public void RegisterAjaxScript(Page page)
        {
            var path = ServicesFramework.GetServiceFrameworkRoot();
            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            JavaScript.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
            ClientAPI.RegisterClientVariable(page, "sf_siteRoot", path, /*overwrite*/ true);
            ClientAPI.RegisterClientVariable(page, "sf_tabId", PortalSettings.Current.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture), /*overwrite*/ true);
                        
            string scriptPath;
            if(HttpContextSource.Current.IsDebuggingEnabled)
            {
                scriptPath = "~/js/Debug/dnn.servicesframework.js";
            }
            else
            {
                scriptPath = "~/js/dnn.servicesframework.js";
            }
            
            ClientResourceManager.RegisterScript(page, scriptPath);
        }

        private static void SetKey(string key)
        {
            HttpContextSource.Current.Items[key] = true;
        }

        private static bool CheckKey(string antiForgeryKey)
        {
            return HttpContextSource.Current.Items.Contains(antiForgeryKey);
        }
    }
}