// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;

    public class DnnHelper<TModel> : DnnHelper
    {
        /// <summary>Initializes a new instance of the <see cref="DnnHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(null, null, null, viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper{TModel}"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        public DnnHelper(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(clientResourceController, appStatus, eventLogger, viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper{TModel}"/> class.</summary>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection))
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnHelper{TModel}"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="viewContext">The view context.</param>
        /// <param name="viewDataContainer">The ViewData container.</param>
        /// <param name="routeCollection">The route collection.</param>
        public DnnHelper(IClientResourceController clientResourceController, IApplicationStatusInfo appStatus, IEventLogger eventLogger, ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(clientResourceController, appStatus, eventLogger, new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection))
        {
        }

        public new ViewDataDictionary<TModel> ViewData => ((HtmlHelper<TModel>)this.HtmlHelper).ViewData;
    }
}
