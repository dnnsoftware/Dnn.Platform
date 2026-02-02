// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Models
{
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Represents a stylesheet registered by the MVC pipeline.
    /// </summary>
    public class RegisteredStylesheet
    {
        /// <summary>
        /// Gets or sets the stylesheet path.
        /// </summary>
        public string Stylesheet { get; set; }

        /// <summary>
        /// Gets or sets the load order for the stylesheet.
        /// </summary>
        public FileOrder.Css FileOrder { get; set; }
    }
}
