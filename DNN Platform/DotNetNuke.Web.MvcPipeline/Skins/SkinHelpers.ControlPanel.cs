// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString ControlPanel(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject")
        {
            var lblControlPanel = new TagBuilder("span");

            if (!string.IsNullOrEmpty(cssClass))
            {
                lblControlPanel.AddCssClass(cssClass);
            }

            // lblControlPanel.SetInnerText(Localization.GetString("ControlPanel", Localization.GetResourceFile(helper.ViewContext.Controller, "ControlPanel.ascx")));
            return new MvcHtmlString(lblControlPanel.ToString());
        }
    }
}
