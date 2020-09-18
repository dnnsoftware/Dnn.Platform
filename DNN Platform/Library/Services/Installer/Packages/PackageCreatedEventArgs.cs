// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Packages
{
    using System;

    /// -----------------------------------------------------------------------------
    /// Project     : DotNetNuke
    /// Namespace   : DotNetNuke.Services.Installer.Packages
    /// Class       : PackageCreatedEventArgs
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// PackageCreatedEventArgs provides a custom EventArgs class for a
    /// Package Created Event.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class PackageCreatedEventArgs : EventArgs
    {
        private readonly PackageInfo _Package;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageCreatedEventArgs"/> class.
        /// Builds a new PackageCreatedEventArgs.
        /// </summary>
        /// <param name="package">The package associated with this event.</param>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public PackageCreatedEventArgs(PackageInfo package)
        {
            this._Package = package;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Package associated with this event.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public PackageInfo Package
        {
            get
            {
                return this._Package;
            }
        }
    }
}
