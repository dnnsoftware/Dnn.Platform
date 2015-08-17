// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
