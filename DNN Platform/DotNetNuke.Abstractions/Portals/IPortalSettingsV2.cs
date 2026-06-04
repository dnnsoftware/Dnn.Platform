// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Portals
{
    using DotNetNuke.Abstractions.Framework;

    /// <summary>
    /// The PortalSettings class encapsulates all of the settings for the Portal,
    /// as well as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    public interface IPortalSettingsV2 : IPortalSettings
    {
        /// <summary>Gets the pipeline type for the portal.</summary>
        PagePipeline.PortalRenderingPipeline PagePipeline { get; }
    }
}
