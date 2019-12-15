// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LibraryPackageWriter class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class LibraryPackageWriter : PackageWriterBase
    {
        public LibraryPackageWriter(PackageInfo package) : base(package)
        {
            BasePath = "DesktopModules\\Libraries";
            AssemblyPath = "bin";
        }

        protected override void GetFiles(bool includeSource, bool includeAppCode)
        {
            base.GetFiles(includeSource, false);
        }
    }
}
