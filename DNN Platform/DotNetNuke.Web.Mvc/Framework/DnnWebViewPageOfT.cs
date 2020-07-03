// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework
{
    using System.Web.Mvc;

    using DotNetNuke.Web.Mvc.Helpers;

    public abstract class DnnWebViewPage<TModel> : WebViewPage<TModel>
    {
        public DnnHelper<TModel> Dnn { get; set; }

        public new DnnHtmlHelper<TModel> Html { get; set; }

        public new DnnUrlHelper Url { get; set; }

        public override void InitHelpers()
        {
            this.Ajax = new AjaxHelper<TModel>(this.ViewContext, this);
            this.Html = new DnnHtmlHelper<TModel>(this.ViewContext, this);
            this.Url = new DnnUrlHelper(this.ViewContext);
            this.Dnn = new DnnHelper<TModel>(this.ViewContext, this);
        }
    }
}
