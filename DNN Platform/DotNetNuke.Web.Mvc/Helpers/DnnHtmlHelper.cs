// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Framework;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

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
            this.HtmlHelper = htmlHelper;

            var controller = htmlHelper.ViewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnHtmlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            this.ModuleContext = controller.ModuleContext;
        }

        public RouteCollection RouteCollection => this.HtmlHelper.RouteCollection;

        public dynamic ViewBag => this.HtmlHelper.ViewBag;

        public ViewContext ViewContext => this.HtmlHelper.ViewContext;

        public ViewDataDictionary ViewData => this.HtmlHelper.ViewData;

        public IViewDataContainer ViewDataContainer => this.HtmlHelper.ViewDataContainer;

        public ModuleInstanceContext ModuleContext { get; set; }

        internal HtmlHelper HtmlHelper { get; set; }

        public MvcHtmlString AntiForgeryToken()
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            return new MvcHtmlString(string.Empty);
        }

        public string AttributeEncode(string value) => this.HtmlHelper.AttributeEncode(value);

        public string AttributeEncode(object value) => this.HtmlHelper.AttributeEncode(value);

        public string Encode(string value) => this.HtmlHelper.Encode(value);

        public string Encode(object value) => this.HtmlHelper.Encode(value);

        public string FormatValue(object value, string format) => this.HtmlHelper.FormatValue(value, format);

        public MvcHtmlString HttpMethodOverride(HttpVerbs httpVerb) => this.HtmlHelper.HttpMethodOverride(httpVerb);

        public MvcHtmlString HttpMethodOverride(string httpVerb) => this.HtmlHelper.HttpMethodOverride(httpVerb);

        public IHtmlString Raw(string value) => this.HtmlHelper.Raw(value);

        public IHtmlString Raw(object value) => this.HtmlHelper.Raw(value);

        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name) => this.HtmlHelper.GetUnobtrusiveValidationAttributes(name);

        public IDictionary<string, object> GetUnobtrusiveValidationAttributes(string name, ModelMetadata metadata) => this.HtmlHelper.GetUnobtrusiveValidationAttributes(name, metadata);

        public void EnableClientValidation() => this.HtmlHelper.EnableClientValidation();

        public void EnableClientValidation(bool enabled) => this.HtmlHelper.EnableClientValidation(enabled);

        public void EnableUnobtrusiveJavaScript() => this.HtmlHelper.EnableUnobtrusiveJavaScript();

        public void EnableUnobtrusiveJavaScript(bool enabled) => this.HtmlHelper.EnableUnobtrusiveJavaScript(enabled);
    }
}
