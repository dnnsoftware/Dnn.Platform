// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Abstractions.ClientResources;

namespace DotNetNuke.Web.MvcPipeline.Models
{
    /// <summary>
    /// Represents a client script registered by the MVC pipeline.
    /// </summary>
    public class RegisteredScript
    {
        /// <summary>
        /// Gets or sets the script path or content.
        /// </summary>
        public string Script { get; set; }

        /// <summary>
        /// Gets or sets the load order for the script.
        /// </summary>
        public FileOrder.Js FileOrder { get; set; } = DotNetNuke.Abstractions.ClientResources.FileOrder.Js.DefaultPriority;
    }
}
