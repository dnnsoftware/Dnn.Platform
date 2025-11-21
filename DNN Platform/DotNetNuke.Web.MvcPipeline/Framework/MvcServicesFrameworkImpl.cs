namespace DotNetNuke.Web.MvcPipeline.Framework
{
    using System.Globalization;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using System.Web.UI;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    internal class MvcServicesFrameworkImpl : IMvcServicesFramework, IMvcServiceFrameworkInternals
    {
        private const string AntiForgeryKey = "dnnAntiForgeryRequested";
        private const string ScriptKey = "dnnSFAjaxScriptRequested";

        /// <inheritdoc/>
        public bool IsAjaxAntiForgerySupportRequired
        {
            get { return CheckKey(AntiForgeryKey); }
        }

        /// <inheritdoc/>
        public bool IsAjaxScriptSupportRequired
        {
            get { return CheckKey(ScriptKey); }
        }

        /// <inheritdoc/>
        public void RequestAjaxAntiForgerySupport()
        {
            this.RequestAjaxScriptSupport();
            SetKey(AntiForgeryKey);
        }

        /// <inheritdoc/>
        public void RegisterAjaxAntiForgery(Page page)
        {
            var ctl = page.FindControl("ClientResourcesFormBottom");
            if (ctl != null)
            {
                ctl.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
            }
        }

        /// <inheritdoc/>
        public void RequestAjaxScriptSupport()
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);
            SetKey(ScriptKey);
        }

        /// <inheritdoc/>
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

            var controller = GetClientResourcesController();
            controller.RegisterScript(scriptPath);
        }

        public void RegisterAjaxScript(ControllerContext page)
        {
            var path = ServicesFramework.GetServiceFrameworkRoot();
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            MvcJavaScript.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
            MvcClientAPI.RegisterClientVariable("sf_siteRoot", path, /*overwrite*/ true);
            MvcClientAPI.RegisterClientVariable("sf_tabId", PortalSettings.Current.ActiveTab.TabID.ToString(CultureInfo.InvariantCulture), /*overwrite*/ true);

            string scriptPath;
            if (HttpContextSource.Current.IsDebuggingEnabled)
            {
                scriptPath = "~/js/Debug/dnn.servicesframework.js";
            }
            else
            {
                scriptPath = "~/js/dnn.servicesframework.js";
            }

            var controller = GetClientResourcesController();
            controller.RegisterScript(scriptPath);
        }

        private static void SetKey(string key)
        {
            HttpContextSource.Current.Items[key] = true;
        }

        private static bool CheckKey(string antiForgeryKey)
        {
            return HttpContextSource.Current.Items.Contains(antiForgeryKey);
        }

        public void RegisterAjaxAntiForgery(ControllerContext page)
        {
            throw new System.NotImplementedException();
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = DotNetNuke.Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
