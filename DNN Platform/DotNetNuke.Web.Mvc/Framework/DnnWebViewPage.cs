// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework
{
    using System.Web.Mvc;

    using DotNetNuke.Web.Mvc.Helpers;

    public abstract class DnnWebViewPage : WebViewPage
    {
        public DnnHelper<object> Dnn { get; set; }

        public new DnnHtmlHelper<object> Html { get; set; }

        public new DnnUrlHelper Url { get; set; }

        public override void InitHelpers()
        {
            this.Ajax = new AjaxHelper<object>(this.ViewContext, this);
            this.Html = new DnnHtmlHelper<object>(this.ViewContext, this);
            this.Url = new DnnUrlHelper(this.ViewContext);
            this.Dnn = new DnnHelper<object>(this.ViewContext, this);
        }
    }
}
