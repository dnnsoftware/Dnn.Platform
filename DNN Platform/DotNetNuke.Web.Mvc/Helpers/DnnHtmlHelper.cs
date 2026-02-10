// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    using Microsoft.Extensions.DependencyInjection;

    public class DnnHtmlHelper
    {
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, (IServicesFramework)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, IServicesFramework servicesFramework)
            : this(viewContext, viewDataContainer, RouteTable.Routes, servicesFramework)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(viewContext, viewDataContainer, routeCollection, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection, IServicesFramework servicesFramework)
            : this(new HtmlHelper(viewContext, viewDataContainer, routeCollection), servicesFramework)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="htmlHelper">The HtmlHelper to wrap.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        protected DnnHtmlHelper(HtmlHelper htmlHelper)
            : this(htmlHelper, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper"/> class.</summary>
        /// <param name="htmlHelper">The HtmlHelper to wrap.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        protected DnnHtmlHelper(HtmlHelper htmlHelper, IServicesFramework servicesFramework)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
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
            this.servicesFramework.RequestAjaxAntiForgerySupport();
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
