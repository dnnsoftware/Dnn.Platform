// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

#endregion

namespace DotNetNuke.Services.Installer.Packages
{
    ///-----------------------------------------------------------------------------
    /// Project		: DotNetNuke
    /// Namespace   : DotNetNuke.Services.Installer.Packages
    /// Class		: PackageCreatedEventArgs
    ///-----------------------------------------------------------------------------
    /// <summary>
    /// PackageCreatedEventArgs provides a custom EventArgs class for a
    /// Package Created Event.
    /// </summary>
    ///-----------------------------------------------------------------------------
    public class PackageCreatedEventArgs : EventArgs
    {
        private readonly PackageInfo _Package;

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Builds a new PackageCreatedEventArgs
        /// </summary>
        /// <param name="package">The package associated with this event</param>
        /// <remarks></remarks>
        ///-----------------------------------------------------------------------------
        public PackageCreatedEventArgs(PackageInfo package)
        {
            _Package = package;
        }

        ///-----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Package associated with this event
        /// </summary>
        ///-----------------------------------------------------------------------------
        public PackageInfo Package
        {
            get
            {
                return _Package;
            }
        }
    }
}
