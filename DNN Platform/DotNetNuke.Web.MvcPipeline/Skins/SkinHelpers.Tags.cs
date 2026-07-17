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

    /// <summary>
    /// Skin helper methods for rendering the legacy DNN tags control.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders the legacy DNN tags control for a given object type.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Optional CSS class applied to the control.</param>
        /// <param name="addImageUrl">Image URL for the add button.</param>
        /// <param name="cancelImageUrl">Image URL for the cancel button.</param>
        /// <param name="saveImageUrl">Image URL for the save button.</param>
        /// <param name="allowTagging">If set to <c>true</c>, allows adding new tags.</param>
        /// <param name="showCategories">If set to <c>true</c>, shows categories.</param>
        /// <param name="showTags">If set to <c>true</c>, shows tags.</param>
        /// <param name="separator">Separator between tags.</param>
        /// <param name="objectType">The DNN object type the tags apply to (for example, "Page").</param>
        /// <param name="repeatDirection">The repeat direction (for example, "Horizontal").</param>
        /// <returns>An HTML string representing the tags control.</returns>
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
