// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Framework;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;

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

        public RouteCollection RouteCollection => HtmlHelper.RouteCollection;

        public dynamic ViewBag => HtmlHelper.ViewBag;

        public ViewContext ViewContext => HtmlHelper.ViewContext;

        public ViewDataDictionary ViewData => HtmlHelper.ViewData;

        public IViewDataContainer ViewDataContainer => HtmlHelper.ViewDataContainer;

        public string AttributeEncode(string value) => HtmlHelper.AttributeEncode(value);

        public string AttributeEncode(object value) => HtmlHelper.AttributeEncode(value);

        public string Encode(string value) => HtmlHelper.Encode(value);

        public string Encode(object value) => HtmlHelper.Encode(value);

        public string FormatValue(object value, string format) => HtmlHelper.FormatValue(value, format);

        public MvcHtmlString HttpMethodOverride(HttpVerbs httpVerb) => HtmlHelper.HttpMethodOverride(httpVerb);

        public MvcHtmlString HttpMethodOverride(string httpVerb) => HtmlHelper.HttpMethodOverride(httpVerb);

        public IHtmlString Raw(string value) => HtmlHelper.Raw(value);

        public IHtmlString Raw(object value) => HtmlHelper.Raw(value);

        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name) => HtmlHelper.GetUnobtrusiveValidationAttributes(name);

        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name, ModelMetadata metadata) => HtmlHelper.GetUnobtrusiveValidationAttributes(name, metadata);

        public void EnableClientValidation() => HtmlHelper.EnableClientValidation();

        public void EnableClientValidation(bool enabled) => HtmlHelper.EnableClientValidation(enabled);

        public void EnableUnobtrusiveJavaScript() => HtmlHelper.EnableUnobtrusiveJavaScript();

        public void EnableUnobtrusiveJavaScript(bool enabled) => HtmlHelper.EnableUnobtrusiveJavaScript(enabled);

    }
}
