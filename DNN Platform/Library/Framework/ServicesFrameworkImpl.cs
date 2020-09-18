// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework
{
    using System;
    using System.Globalization;
    using System.Web.Helpers;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    internal class ServicesFrameworkImpl : IServicesFramework, IServiceFrameworkInternals
    {
        private const string AntiForgeryKey = "dnnAntiForgeryRequested";
        private const string ScriptKey = "dnnSFAjaxScriptRequested";

        public bool IsAjaxAntiForgerySupportRequired
        {
            get { return CheckKey(AntiForgeryKey); }
        }

        public bool IsAjaxScriptSupportRequired
        {
            get { return CheckKey(ScriptKey); }
        }

        public void RequestAjaxAntiForgerySupport()
        {
            this.RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
        }

        public void RegisterAjaxAntiForgery(Page page)
        {
            var ctl = page.FindControl("ClientResourcesFormBottom");
            if (ctl != null)
            {
                ctl.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
            }
        }

        public void RequestAjaxScriptSupport()
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            SetKey(ScriptKey);
        }

        public void RegisterAjaxScript(Page page)
        {
            var path = ServicesFramework.GetServiceFrameworkRoot();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            JavaScript.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
            ClientAPI.RegisterClientVariable(page, "sf_siteRoot", path, /*overwrite*/ true);
            ClientAPI.RegisterClientVariable(page, "sf_tabId", PortalSettings.Current.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture), /*overwrite*/ true);

            string scriptPath;
            if (HttpContextSource.Current.IsDebuggingEnabled)
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
