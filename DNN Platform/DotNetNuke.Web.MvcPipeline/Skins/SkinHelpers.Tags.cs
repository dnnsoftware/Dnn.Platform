// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.MvcPipeline.Models;

    public static partial class SkinHelpers
    {
        public static IHtmlString Tags(this HtmlHelper<PageModel> helper, string cssClass = "", string addImageUrl = "", string cancelImageUrl = "", string saveImageUrl = "", bool allowTagging = true, bool showCategories = true, bool showTags = true, string separator = ",&nbsp;", string objectType = "Page", string repeatDirection = "Horizontal")
        {
            var portalSettings = PortalSettings.Current;
            var tagsControl = new TagBuilder("dnn:tags");
            tagsControl.Attributes.Add("id", "tagsControl");
            tagsControl.Attributes.Add("runat", "server");
            tagsControl.Attributes.Add("CssClass", cssClass);
            tagsControl.Attributes.Add("AddImageUrl", addImageUrl);
            tagsControl.Attributes.Add("CancelImageUrl", cancelImageUrl);
            tagsControl.Attributes.Add("SaveImageUrl", saveImageUrl);
            tagsControl.Attributes.Add("AllowTagging", allowTagging.ToString());
            tagsControl.Attributes.Add("ShowCategories", showCategories.ToString());
            tagsControl.Attributes.Add("ShowTags", showTags.ToString());
            tagsControl.Attributes.Add("Separator", separator);
            tagsControl.Attributes.Add("ObjectType", objectType);
            tagsControl.Attributes.Add("RepeatDirection", repeatDirection);

            return new MvcHtmlString(tagsControl.ToString());
        }
    }
}
