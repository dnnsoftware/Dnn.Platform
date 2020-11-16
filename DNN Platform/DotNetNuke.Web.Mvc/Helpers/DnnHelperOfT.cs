// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable ConvertPropertyToExpressionBody
namespace DotNetNuke.Web.Mvc.Helpers
{
    using System.Web.Mvc;
    using System.Web.Routing;

    public class DnnHelper<TModel> : DnnHelper
    {
        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer)
            : this(viewContext, viewDataContainer, RouteTable.Routes)
        {
        }

        public DnnHelper(ViewContext viewContext, IViewDataContainer viewDataContainer, RouteCollection routeCollection)
            : base(new HtmlHelper<TModel>(viewContext, viewDataContainer, routeCollection))
        {
        }

        public new ViewDataDictionary<TModel> ViewData
        {
            get { return ((HtmlHelper<TModel>)this.HtmlHelper).ViewData; }
        }
    }
}
