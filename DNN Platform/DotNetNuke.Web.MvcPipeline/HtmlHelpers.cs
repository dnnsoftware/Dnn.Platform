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
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;
    using DotNetNuke.Web.MvcPipeline.Utils;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class HtmlHelpers
    {
        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, ModuleInfo module)
        {
            var clientResourcesController = GetClientResourcesController();
            var moduleControl = MvcUtils.GetModuleControl(module, controlSrc);
            // moduleControl.ViewContext = htmlHelper.ViewContext;
            if (moduleControl is IResourcable)
            {
                var resourcable = (IResourcable)moduleControl;
                if (resourcable.ModuleResources.StyleSheets != null)
                {
                    foreach (var styleSheet in resourcable.ModuleResources.StyleSheets)
                    {
                        clientResourcesController
                            .RegisterStylesheet(styleSheet.FilePath, styleSheet.Priority);
                    }
                }
                if (resourcable.ModuleResources.Scripts != null)
                {
                    foreach (var javaScript in resourcable.ModuleResources.Scripts)
                    {
                        clientResourcesController
                            .RegisterScript(javaScript.FilePath, javaScript.Priority);
                    }
                }
                if (resourcable.ModuleResources.Libraries != null)
                {
                    foreach (var lib in resourcable.ModuleResources.Libraries)
                    {
                        MvcJavaScript.RequestRegistration(lib);
                    }
                }
                if (resourcable.ModuleResources.AjaxScript)
                {
                    ServicesFramework.Instance.RequestAjaxScriptSupport();
                }
                if (resourcable.ModuleResources.AjaxAntiForgery)
                {
                    ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
                }
            }
            return moduleControl.Html(htmlHelper);
        }

        public static IHtmlString Control(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            return htmlHelper.Control(module.ModuleControl.ControlSrc, module);
        }

        public static IHtmlString CspNonce(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString(htmlHelper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
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
