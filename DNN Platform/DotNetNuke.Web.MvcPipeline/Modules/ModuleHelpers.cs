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
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// HTML helpers for working with DNN MVC modules (localization, edit URLs, and partial views).
    /// </summary>
    public static partial class ModuleHelpers
    {
        /// <summary>
        /// Gets a localized string for the specified key and resource file.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="key">The localization key.</param>
        /// <param name="localResourceFile">The resource file path.</param>
        /// <returns>The localized string as an HTML string.</returns>
        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key, string localResourceFile)
        {
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }

        /// <summary>
        /// Gets a localized string for the specified key using the <c>LocalResourceFile</c> in <see cref="ViewDataDictionary"/>.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="key">The localization key.</param>
        /// <returns>The localized string as an HTML string.</returns>
        public static IHtmlString LocalizeString(this HtmlHelper htmlHelper, string key)
        {
            if (htmlHelper.ViewContext.ViewData["LocalResourceFile"] == null)
            {
                throw new InvalidOperationException("The LocalResourceFile must be set in the ViewData to use this helper.");
            }

            var localResourceFile = (string)htmlHelper.ViewContext.ViewData["LocalResourceFile"];
            return MvcHtmlString.Create(Localization.GetString(key, localResourceFile));
        }

        /// <summary>
        /// Builds an edit URL for the current module.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <returns>The edit URL as an HTML string.</returns>
        public static IHtmlString EditUrl(this HtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }

            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl());
        }

        /// <summary>
        /// Builds an edit URL for the current module and specified control key.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="controlKey">The control key.</param>
        /// <returns>The edit URL as an HTML string.</returns>
        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string controlKey)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }

            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(controlKey));
        }

        /// <summary>
        /// Builds an edit URL for the current module with a single key/value route parameter.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <returns>The edit URL as an HTML string.</returns>
        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }

            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue));
        }

        /// <summary>
        /// Builds an edit URL for the current module with a key/value parameter and control key.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <param name="controlKey">The control key.</param>
        /// <returns>The edit URL as an HTML string.</returns>
        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue, string controlKey)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }

            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue, controlKey));
        }

        /// <summary>
        /// Builds an edit URL for the current module with a key/value parameter, control key, and additional route parameters.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="keyName">The query string key.</param>
        /// <param name="keyValue">The query string value.</param>
        /// <param name="controlKey">The control key.</param>
        /// <param name="additionalParameters">Additional route parameters.</param>
        /// <returns>The edit URL as an HTML string.</returns>
        public static IHtmlString EditUrl(this HtmlHelper htmlHelper, string keyName, string keyValue, string controlKey, params string[] additionalParameters)
        {
            if (htmlHelper.ViewContext.ViewData["ModuleContext"] == null)
            {
                throw new InvalidOperationException("The ModuleContext must be set in the ViewData to use this helper.");
            }

            var moduleContext = (ModuleInstanceContext)htmlHelper.ViewContext.ViewData["ModuleContext"];
            return MvcHtmlString.Create(moduleContext.EditUrl(keyName, keyValue, controlKey, additionalParameters));
        }

        /// <summary>
        /// Renders a partial view for the current module using the module's folder path.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="partialViewName">The name of the partial view.</param>
        /// <param name="model">The model to pass to the partial view.</param>
        /// <returns>The partial view HTML.</returns>
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

        /// <summary>
        /// Renders a partial view for the current module using the module's folder path with explicit view data.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="partialViewName">The name of the partial view.</param>
        /// <param name="model">The model to pass to the partial view.</param>
        /// <param name="dic">The view data dictionary.</param>
        /// <returns>The partial view HTML.</returns>
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
    }
}
