// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers.Models
{
    /// <summary>A view model with information about an upgrade indicator.</summary>
    public class UpgradeIndicatorViewModel
    {
        /// <summary>Gets or sets the ID.</summary>
        public string ID { get; set; }

        /// <summary>Gets or sets the image URL.</summary>
        public string ImageUrl { get; set; }

        /// <summary>Gets or sets the web action.</summary>
        public string WebAction { get; set; }

        /// <summary>Gets or sets the alternate text of the image.</summary>
        public string AltText { get; set; }

        /// <summary>Gets or sets the tooltip text.</summary>
        public string ToolTip { get; set; }

        /// <summary>Gets or sets the CSS class.</summary>
        public string CssClass { get; set; }
    }
}
