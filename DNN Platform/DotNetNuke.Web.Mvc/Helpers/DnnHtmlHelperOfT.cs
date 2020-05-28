// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Web.Mvc;
using System.Web.Routing;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnHtmlHelper<TModel> : DnnHtmlHelper
    {
        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer) 
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public DnnHtmlHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection))
 
        {
        }

        internal new HtmlHelper<TModel> HtmlHelper => (HtmlHelper<TModel>)base.HtmlHelper;

        public new object ViewBag => HtmlHelper.ViewBag;

        public new ViewDataDictionary<TModel> ViewData => HtmlHelper.ViewData;
    }
}
