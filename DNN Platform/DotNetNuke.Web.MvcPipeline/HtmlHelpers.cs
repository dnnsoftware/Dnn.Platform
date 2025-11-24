// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using DotNetNuke.Web.MvcPipeline.Utils;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class HtmlHelpers
    {
        public static IHtmlString Control(this HtmlHelper htmlHelper, IMvcModuleControl moduleControl)
        {
            if (moduleControl is IPageContributor)
            {
                var pageContributor = (IPageContributor)moduleControl;
                pageContributor.ConfigurePage(new PageConfigurationContext(Common.Globals.GetCurrentServiceProvider()));
            }
            return moduleControl.Html(htmlHelper);
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, ModuleInfo module)
        {
            var moduleControl = MvcUtils.GetModuleControl(module, controlSrc);
            if (moduleControl is IPageContributor)
            {
                var pageContributor = (IPageContributor)moduleControl;
                pageContributor.ConfigurePage(new PageConfigurationContext(Common.Globals.GetCurrentServiceProvider()));
            }
            return moduleControl.Html(htmlHelper);
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            return htmlHelper.Control(module.ModuleControl.ControlSrc, module);
        }

        public static IHtmlString CspNonce(this HtmlHelper htmlHelper)
        {
            //todo CSP - implement nonce support
            //return new MvcHtmlString(htmlHelper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString RegisterAjaxScriptIfRequired(this HtmlHelper htmlHelper)
        {
            if (MvcServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                MvcServicesFrameworkInternal.Instance.RegisterAjaxScript(htmlHelper.ViewContext.Controller.ControllerContext);
            }

            return new MvcHtmlString(string.Empty);
        }

        public static IHtmlString AntiForgeryIfRequired(this HtmlHelper htmlHelper)
        {
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                return AntiForgery.GetHtml();
            }

            return new MvcHtmlString(string.Empty);
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
