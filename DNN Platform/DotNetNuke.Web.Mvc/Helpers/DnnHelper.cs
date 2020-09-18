// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class DnnHelper
    {
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(new HtmlHelper(viewContext, viewDataContainer, routeCollection))
        {
        }

        protected DnnHelper(HtmlHelper htmlHelper)
        {
            this.HtmlHelper = htmlHelper;

            var controller = htmlHelper.ViewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            this.DnnPage = controller.DnnPage;

            this.ModuleContext = controller.ModuleContext;
            this.LocalResourceFile = controller.LocalResourceFile;
        }

        public ModuleInfo ActiveModule
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.ActiveTab; }
        }

        public PortalSettings PortalSettings
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.PortalSettings; }
        }

        public RouteCollection RouteCollection
        {
            get { return this.HtmlHelper.RouteCollection; }
        }

        public UserInfo User
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.UserInfo; }
        }

        public dynamic ViewBag
        {
            get { return this.HtmlHelper.ViewBag; }
        }

        public ViewContext ViewContext
        {
            get { return this.HtmlHelper.ViewContext; }
        }

        public ViewDataDictionary ViewData
        {
            get { return this.HtmlHelper.ViewData; }
        }

        public IViewDataContainer ViewDataContainer
        {
            get { return this.HtmlHelper.ViewDataContainer; }
        }

        public Page DnnPage { get; set; }

        public string LocalResourceFile { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        internal HtmlHelper HtmlHelper { get; set; }

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        public void DnnCssInclude(string filePath)
        {
            ClientResourceManager.RegisterStyleSheet(this.DnnPage, filePath);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.
        /// </summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnCssInclude(string filePath, int priority)
        {
            ClientResourceManager.RegisterStyleSheet(this.DnnPage, filePath, priority);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Defaults to rendering in the page header.
        /// </summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnCssInclude(string filePath, FileOrder.Css priority)
        {
            ClientResourceManager.RegisterStyleSheet(this.DnnPage, filePath, priority);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.
        /// </summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        public void DnnCssInclude(string filePath, int priority, string provider)
        {
            ClientResourceManager.RegisterStyleSheet(this.DnnPage, filePath, priority, provider);
        }

        /// <summary>
        /// Requests that a CSS file be registered on the client browser. Allows for overriding the default provider.
        /// </summary>
        /// <param name="filePath">The relative file path to the CSS resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The provider name to be used to render the css file on the page.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public void DnnCssInclude(string filePath, int priority, string provider, string name, string version)
        {
            ClientResourceManager.RegisterStyleSheet(this.DnnPage, filePath, priority, provider, name, version);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        public void DnnJsInclude(string filePath)
        {
            ClientResourceManager.RegisterScript(this.DnnPage, filePath);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnJsInclude(string filePath, int priority)
        {
            ClientResourceManager.RegisterScript(this.DnnPage, filePath, priority);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        public void DnnJsInclude(string filePath, FileOrder.Js priority)
        {
            ClientResourceManager.RegisterScript(this.DnnPage, filePath, priority);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        public void DnnJsInclude(string filePath, int priority, string provider)
        {
            ClientResourceManager.RegisterScript(this.DnnPage, filePath, priority, provider);
        }

        /// <summary>
        /// Requests that a JavaScript file be registered on the client browser.
        /// </summary>
        /// <param name="filePath">The relative file path to the JavaScript resource.</param>
        /// <param name="priority">The relative priority in which the file should be loaded.</param>
        /// <param name="provider">The name of the provider responsible for rendering the script output.</param>
        /// <param name="name">Name of framework like Bootstrap, Angular, etc.</param>
        /// <param name="version">Version nr of framework.</param>
        public void DnnJsInclude(string filePath, int priority, string provider, string name, string version)
        {
            ClientResourceManager.RegisterScript(this.DnnPage, filePath, priority, provider, name, version);
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        public void JavaScriptLibraryInclude(string name)
        {
            JavaScript.RequestRegistration(name);
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        /// <param name="version">The library's version.</param>
        public void JavaScriptLibraryInclude(string name, Version version)
        {
            JavaScript.RequestRegistration(name, version);
        }

        /// <summary>Requests that a script from a JavaScript Library be registered on the client browser.</summary>
        /// <param name="name">The name of the JavaScript Library.</param>
        /// <param name="version">The library's version.</param>
        /// <param name="specificVersion">
        /// How much of the <paramref name="version"/> to pay attention to.
        /// When <see cref="SpecificVersion.Latest"/> is passed, ignore the <paramref name="version"/>.
        /// When <see cref="SpecificVersion.LatestMajor"/> is passed, match the major version.
        /// When <see cref="SpecificVersion.LatestMinor"/> is passed, match the major and minor versions.
        /// When <see cref="SpecificVersion.Exact"/> is passed, match all parts of the version.
        /// </param>
        public void JavaScriptLibraryInclude(string name, Version version, SpecificVersion specificVersion)
        {
            JavaScript.RequestRegistration(name, version, specificVersion);
        }
    }
}
