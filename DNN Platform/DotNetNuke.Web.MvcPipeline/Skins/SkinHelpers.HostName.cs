// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
        public static IHtmlString HostName(this HtmlHelper<PageModel> helper, string cssClass = "")
        {
            var hostSettings = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

            var link = new TagBuilder("a");
            link.Attributes.Add("href", Globals.AddHTTP(hostSettings.HostUrl));
            
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            link.SetInnerText(hostSettings.HostTitle);

            return new MvcHtmlString(link.ToString());
        }
    }
}
