// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.Mvc;
using System.Web.Routing;
// ReSharper disable ConvertPropertyToExpressionBody

namespace DotNetNuke.Web.Mvc.Helpers
{
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
            get { return ((HtmlHelper<TModel>)HtmlHelper).ViewData; }
        }
    }
}
