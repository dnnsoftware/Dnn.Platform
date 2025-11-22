// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;

    using Microsoft.Extensions.DependencyInjection;

    public static partial class ModuleHelpers
    {
        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key, string localResourceFile)
        {
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }

        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key)
        {
            if (htmlHelper.ViewContext.ViewData["LocalResourceFile"] == null)
            {
                throw new InvalidOperationException("The LocalResourceFile must be set in the ViewData to use this helper.");
            }
            var localResourceFile = (string)htmlHelper.ViewContext.ViewData["LocalResourceFile"];
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }

        public static IHtmlString EditUrl(this HtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl());
        }

        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string controlKey)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl( controlKey));
        }

        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue));
        }

        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue, string controlKey)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue, controlKey));
        }

        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters));
        }

        public static MvcHtmlString ModulePartial(this HtmlHelper htmlHelper, string partialViewName, object model = null)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];

            var viewPath = string.Format("~/DesktopModules/{0}/Views/{1}.cshtml", moduleContext.Configuration.DesktopModule.FolderName, partialViewName);

            return htmlHelper.Partial(viewPath, model);
        }

        public static MvcHtmlString ModulePartial(this HtmlHelper htmlHelper, string partialViewName, object model, ViewDataDictionary dic)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }
            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];

            var viewPath = string.Format("~/DesktopModules/{0}/Views/{1}.cshtml", moduleContext.Configuration.DesktopModule.FolderName, partialViewName);

            return htmlHelper.Partial(viewPath, model, dic);
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
