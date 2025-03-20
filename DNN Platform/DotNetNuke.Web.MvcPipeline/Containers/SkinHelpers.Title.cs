// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Containers
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Mvc.Html;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Title(this HtmlHelper<ContainerModel> htmlHelper, string cssClass = "")
        {
            var model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                throw new InvalidOperationException("The model need to be present.");
            }

            var labelDiv = new TagBuilder("div");
            labelDiv.InnerHtml = model.ModuleConfiguration.ModuleTitle;
            if (!string.IsNullOrEmpty(cssClass))
            {
                labelDiv.AddCssClass(cssClass);
            }

            return MvcHtmlString.Create(labelDiv.ToString());
        }
    }
}
