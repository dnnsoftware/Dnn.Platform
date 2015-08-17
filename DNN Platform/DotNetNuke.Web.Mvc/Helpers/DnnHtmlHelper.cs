// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Framework;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;

// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnHtmlHelper
    {
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer) 
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(new HtmlHelper(viewContext, viewDataContainer, routeCollection))
        {
        }

        protected DnnHtmlHelper(HtmlHelper htmlHelper)
        {
            HtmlHelper = htmlHelper;

            var controller = htmlHelper.ViewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHtmlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            ModuleContext = controller.ModuleContext;
        }

        public MvcHtmlString AntiForgeryToken()
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            return new MvcHtmlString(String.Empty);
        }

        internal HtmlHelper HtmlHelper { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public RouteCollection RouteCollection { get { return HtmlHelper.RouteCollection; } }

        public dynamic ViewBag { get { return HtmlHelper.ViewBag; } }

        public ViewContext ViewContext { get { return HtmlHelper.ViewContext; } }

        public ViewDataDictionary ViewData { get { return HtmlHelper.ViewData; } }

        public IViewDataContainer ViewDataContainer { get { return HtmlHelper.ViewDataContainer; } }
    }
}
