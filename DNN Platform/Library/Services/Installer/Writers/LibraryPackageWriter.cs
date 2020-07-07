// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LibraryPackageWriter class.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class LibraryPackageWriter : PackageWriterBase
    {
        public LibraryPackageWriter(PackageInfo package)
            : base(package)
        {
            this.BasePath = "DesktopModules\\Libraries";
            this.AssemblyPath = "bin";
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            base.GetFiles(includeSource, false);
        }
    }
}
