// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Framework;

    public class DnnHtmlHelper<TModel> : DnnHtmlHelper
    {
        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, (IServicesFramework)null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, IServicesFramework servicesFramework)
            : this(viewContext, viewDataContainer, RouteTable.Routes, servicesFramework)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : this(viewContext, viewDataContainer, routeCollection, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection, IServicesFramework servicesFramework)
            : base(new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection), servicesFramework)
        {
        }

        public new object ViewBag => this.HtmlHelper.ViewBag;

        public new ViewDataDictionary<TModel> ViewData => this.HtmlHelper.ViewData;

        internal new HtmlHelper<TModel> HtmlHelper => (HtmlHelper<TModel>)base.HtmlHelper;
    }
}
