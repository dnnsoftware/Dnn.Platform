// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Web.Mvc.Helpers;

    using Microsoft.Extensions.DependencyInjection;

    public abstract class DnnWebViewPage : WebViewPage
    {
        private readonly IServicesFramework servicesFramework;

        /// <summary>Initializes a new instance of the <see cref="DnnWebViewPage"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IServicesFramework. Scheduled removal in v12.0.0.")]
        protected DnnWebViewPage()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnWebViewPage"/> class.</summary>
        /// <param name="servicesFramework">The web API service framework.</param>
        protected DnnWebViewPage(IServicesFramework servicesFramework)
        {
            this.servicesFramework = servicesFramework ?? Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>();
        }

        public DnnHelper<object> Dnn { get; set; }

        public new DnnHtmlHelper<object> Html { get; set; }

        public new DnnUrlHelper Url { get; set; }

        /// <inheritdoc />
        public override void InitHelpers()
        {
            this.Ajax = new AjaxHelper<object>(this.ViewContext, this);
            this.Html = new DnnHtmlHelper<object>(this.ViewContext, this, this.servicesFramework);
            this.Url = new DnnUrlHelper(this.ViewContext);
            this.Dnn = ActivatorUtilities.CreateInstance<DnnHelper<object>>(Globals.GetCurrentServiceProvider(), this.ViewContext, this);
        }
    }
}
