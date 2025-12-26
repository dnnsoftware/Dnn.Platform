// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Skin helper methods for rendering the current date in the user's local time.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders the current date for the logged-in user, using an optional custom format.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="cssClass">Optional CSS class applied to the span.</param>
        /// <param name="dateFormat">Optional date format string; when empty, uses the long date pattern.</param>
        /// <returns>An HTML string representing the current date.</returns>
        public static IHtmlString CurrentDate(this HtmlHelper<PageModel> helper, string cssClass = "SkinObject", string dateFormat = "")
        {
            var lblDate = new TagBuilder("span");

            if (!string.IsNullOrEmpty(cssClass))
            {
                lblDate.AddCssClass(cssClass);
            }

            var user = UserController.Instance.GetCurrentUserInfo();
            lblDate.SetInnerText(!string.IsNullOrEmpty(dateFormat) ? user.LocalTime().ToString(dateFormat) : user.LocalTime().ToLongDateString());

            return new MvcHtmlString(lblDate.ToString());
        }
    }
}
