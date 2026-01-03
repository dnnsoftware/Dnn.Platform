// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.MvcPipeline.Controllers;
    using DotNetNuke.Web.MvcPipeline.Framework;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// HTML helper extensions for rendering MVC pipeline module controls and related utilities.
    /// </summary>
    public static partial class HtmlHelpers
    {
        /// <summary>
        /// Renders an MVC module control and configures page contributions when supported.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <param name="moduleControl">The MVC module control to render.</param>
        /// <returns>An HTML string representing the rendered module control.</returns>
        public static IHtmlString Control(this HtmlHelper htmlHelper, IMvcModuleControl moduleControl)
        {
            if (moduleControl is IPageContributor)
            {
                var pageContributor = (IPageContributor)moduleControl;
                pageContributor.ConfigurePage(new PageConfigurationContext(Common.Globals.GetCurrentServiceProvider()));
            }

            return moduleControl.Html(htmlHelper);
        }

        /// <summary>
        /// Creates and renders an MVC module control for the specified module and control source.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <param name="controlSrc">The control source path.</param>
        /// <param name="module">The module configuration.</param>
        /// <returns>An HTML string representing the rendered module control.</returns>
        public static IHtmlString Control(this HtmlHelper htmlHelper, string controlSrc, ModuleInfo module)
        {
            var moduleControl = ModuleControlFactory.CreateModuleControl(module, controlSrc);
            if (moduleControl is IPageContributor)
            {
                var pageContributor = (IPageContributor)moduleControl;
                pageContributor.ConfigurePage(new PageConfigurationContext(Common.Globals.GetCurrentServiceProvider()));
            }

            return moduleControl.Html(htmlHelper);
        }

        /// <summary>
        /// Creates and renders the default MVC module control for the specified module.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <param name="module">The module configuration.</param>
        /// <returns>An HTML string representing the rendered module control.</returns>
        public static IHtmlString Control(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            return htmlHelper.Control(module.ModuleControl.ControlSrc, module);
        }

        /// <summary>
        /// Renders the Content Security Policy nonce for the current request, when available.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The CSP nonce wrapped as an HTML string, or an empty string when CSP is not configured.</returns>
        public static IHtmlString CspNonce(this HtmlHelper htmlHelper)
        {
            // TODO: CSP - implement nonce support
            // return new MvcHtmlString(htmlHelper.ViewContext.HttpContext.Items["CSP-NONCE"].ToString());
            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Registers the MVC AJAX support script when required by the services framework.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>An empty HTML string.</returns>
        public static IHtmlString RegisterAjaxScriptIfRequired(this HtmlHelper htmlHelper)
        {
            if (MvcServicesFrameworkInternal.Instance.IsAjaxScriptSupportRequired)
            {
                MvcServicesFrameworkInternal.Instance.RegisterAjaxScript();
            }

            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Renders an anti-forgery token when AJAX anti-forgery support is required.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>
        /// The rendered anti-forgery token, or an empty HTML string when anti-forgery support is not required.
        /// </returns>
        public static IHtmlString AntiForgeryIfRequired(this HtmlHelper htmlHelper)
        {
            if (ServicesFrameworkInternal.Instance.IsAjaxAntiForgerySupportRequired)
            {
                return AntiForgery.GetHtml();
            }

            return new MvcHtmlString(string.Empty);
        }

        /// <summary>
        /// Gets the dependency injection service provider from the current <see cref="DnnPageController"/>.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The service provider associated with the current controller.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the current controller is not a <see cref="DnnPageController"/>.
        /// </exception>
        internal static IServiceProvider GetDependencyProvider(HtmlHelper htmlHelper)
        {
            var controller = htmlHelper.ViewContext.Controller as DnnPageController;

            if (controller == null)
            {
                throw new InvalidOperationException("The HtmlHelper can only be used from DnnPageController");
            }

            return controller.DependencyProvider;
        }

        /// <summary>
        /// Gets the client resource controller from the current dependency injection provider.
        /// </summary>
        /// <param name="htmlHelper">The MVC HTML helper.</param>
        /// <returns>The client resource controller instance.</returns>
        internal static IClientResourceController GetClientResourcesController(HtmlHelper htmlHelper)
        {
            return GetDependencyProvider(htmlHelper).GetRequiredService<IClientResourceController>();
        }
    }
}
