// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Helpers;

using System.Web.Mvc;
using System.Web.Routing;

public class DnnHtmlHelper<TModel> : DnnHtmlHelper
{
    /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
    /// <param name="viewContext">The view context.</param>
    /// <param name="viewDataContainer">The ViewData container.</param>
    public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
        : this(viewContext, viewDataContainer, RouteTable.Routes)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DnnHtmlHelper{TModel}"/> class.</summary>
    /// <param name="viewContext">The view context.</param>
    /// <param name="viewDataContainer">The ViewData container.</param>
    /// <param name="routeCollection">The route collection.</param>
    public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
        : base(new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection))
    {
    }

    public new object ViewBag => this.HtmlHelper.ViewBag;

    public new ViewDataDictionary<TModel> ViewData => this.HtmlHelper.ViewData;

    internal new HtmlHelper<TModel> HtmlHelper => (HtmlHelper<TModel>)base.HtmlHelper;
}
