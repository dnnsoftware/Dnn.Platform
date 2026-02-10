// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers.Models
{
    /// <summary>A view model for a menu item.</summary>
    public class MenuItemViewModel
    {
        /// <summary>Gets or sets the ID.</summary>
        public string ID { get; set; }

        /// <summary>Gets or sets the text.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the source user control.</summary>
        public string Source { get; set; }

        /// <summary>Gets or sets the order.</summary>
        public int Order { get; set; }
    }
}
