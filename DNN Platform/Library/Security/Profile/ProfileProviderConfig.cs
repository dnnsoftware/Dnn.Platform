// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Profile
{
    using System.ComponentModel;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Profile
    /// Class:      ProfileProviderConfig
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileProviderConfig class provides a wrapper to the Profile providers
    /// configuration.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfileProviderConfig
    {
        private static readonly ProfileProvider profileProvider = ProfileProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether the Provider Properties can be edited.
        /// </summary>
        /// <returns>A Boolean.</returns>
        /// -----------------------------------------------------------------------------
        [Browsable(false)]
        public static bool CanEditProviderProperties
        {
            get
            {
                return profileProvider.CanEditProviderProperties;
            }
        }
    }
}
