// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.ComponentModel;

#endregion

namespace DotNetNuke.Security.Profile
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Profile
    /// Class:      ProfileProviderConfig
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileProviderConfig class provides a wrapper to the Profile providers
    /// configuration
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ProfileProviderConfig
    {
        private static readonly ProfileProvider profileProvider = ProfileProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether the Provider Properties can be edited
        /// </summary>
        /// <returns>A Boolean</returns>
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
