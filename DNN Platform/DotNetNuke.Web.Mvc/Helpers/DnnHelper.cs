// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;

// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.Web.Mvc.Helpers
{
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
            HtmlHelper = htmlHelper;

            var controller = htmlHelper.ViewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            DnnPage = controller.DnnPage;

            ModuleContext = controller.ModuleContext;
            LocalResourceFile = controller.LocalResourceFile;
        }

        public ModuleInfo ActiveModule
        {
            get { return (ModuleContext == null) ? null : ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (PortalSettings == null) ? null : PortalSettings.ActiveTab; }
        }

        public Page DnnPage { get; set; }

        internal HtmlHelper HtmlHelper { get; set; }

        public string LocalResourceFile { get; set; }

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        public PortalSettings PortalSettings
        {
            get { return (ModuleContext == null) ? null : ModuleContext.PortalSettings; }
        }

        public RouteCollection RouteCollection { get { return HtmlHelper.RouteCollection; } }

        public UserInfo User
        {
            get { return (PortalSettings == null) ? null : PortalSettings.UserInfo; }
        }

        public dynamic ViewBag { get { return HtmlHelper.ViewBag; } }

        public ViewContext ViewContext { get { return HtmlHelper.ViewContext; } }

        public ViewDataDictionary ViewData { get { return HtmlHelper.ViewData; } }

        public IViewDataContainer ViewDataContainer { get { return HtmlHelper.ViewDataContainer; } }
    }
}