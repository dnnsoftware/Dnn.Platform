// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals.Extensions
{
    using DotNetNuke.Abstractions.Portals;

    /// <summary>
    /// Extends the <see cref="IPortalSettings"/> interface.
    /// </summary>
    public static class IPortalSettingsExtensions
    {
        /// <summary>
        /// Gets the styles for the portal.
        /// </summary>
        /// <param name="portalSettings">The portal settings to the the styles from.</param>
        /// <returns><see cref="IPortalStyles"/>.</returns>
        public static IPortalStyles GetStyles(this IPortalSettings portalSettings)
        {
            var repo = new PortalStylesRepository();
            return repo.GetSettings(portalSettings.PortalId);
        }
    }
}
