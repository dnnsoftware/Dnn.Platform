// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc.Framework
{
    public abstract class DnnWebViewPage<TModel> : WebViewPage<TModel>
    {
        public DnnHelper<TModel> Dnn { get; set; }

        public new DnnHtmlHelper<TModel> Html { get; set; }

        public new DnnUrlHelper Url { get; set; }

        public override void InitHelpers() 
        {
            Ajax = new AjaxHelper<TModel>(ViewContext, this);
            Html = new DnnHtmlHelper<TModel>(ViewContext, this);
            Url = new DnnUrlHelper(ViewContext);
            Dnn = new DnnHelper<TModel>(ViewContext, this);
        }
    }
}