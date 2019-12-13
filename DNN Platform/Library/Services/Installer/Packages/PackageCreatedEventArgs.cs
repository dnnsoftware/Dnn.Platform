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
