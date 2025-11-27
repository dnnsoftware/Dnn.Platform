// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Web.MvcPipeline.Models;
    using Microsoft.Extensions.DependencyInjection;

    public static partial class SkinHelpers
    {
        public static IHtmlString DotNetNuke(this HtmlHelper<PageModel> helper, string cssClass = "")
        {
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();
            if (!hostSettingsService.GetBoolean("Copyright", true))
            {
                return MvcHtmlString.Empty;
            }

            var link = new TagBuilder("a");
            link.Attributes.Add("href", DotNetNukeContext.Current.Application.Url);
            if (!string.IsNullOrEmpty(cssClass))
            {
                link.AddCssClass(cssClass);
            }

            link.SetInnerText(DotNetNukeContext.Current.Application.LegalCopyright);

            return new MvcHtmlString(link.ToString());
        }
    }
}
