// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using DotNetNuke.Abstractions.ClientResources;

    /// <summary>
    /// Base class for link resource types.
    /// </summary>
    public abstract class LinkResource : ResourceBase, ILinkResource
    {
        /// <inheritdoc />
        public bool Preload { get; set; } = false;
    }
}
