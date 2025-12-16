// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;

    public class JavaScriptLibraryInstaller : ComponentInstallerBase
    {
        private JavaScriptLibrary library;
        private JavaScriptLibrary installedLibrary;

        /// <inheritdoc/>
        public override void Commit()
        {
        }

        /// <inheritdoc/>
        public override void Install()
        {
            try
            {
                // Attempt to get the JavaScript Library
                this.installedLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == this.library.LibraryName && l.Version == this.library.Version);

                if (this.installedLibrary != null)
                {
                    this.library.JavaScriptLibraryID = this.installedLibrary.JavaScriptLibraryID;
                }

                // Save JavaScript Library  to database
                this.library.PackageID = this.Package.PackageID;
                JavaScriptLibraryController.Instance.SaveLibrary(this.library);

                this.Completed = true;
                this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.LIBRARY_Registered, this.library.LibraryName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// <inheritdoc/>
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Load the JavaScript Library from the manifest
            this.library = CBO.DeserializeObject<JavaScriptLibrary>(new StringReader(manifestNav.InnerXml));
            this.library.Version = this.Package.Version;

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.LIBRARY_ReadSuccess);
            }
        }

        /// <inheritdoc/>
        public override void Rollback()
        {
            // If Temp Library exists then we need to update the DataStore with this
            if (this.installedLibrary == null)
            {
                // No Temp Library - Delete newly added library
                this.DeleteLibrary();
            }
            else
            {
                // Temp Library - Rollback to Temp
                JavaScriptLibraryController.Instance.SaveLibrary(this.installedLibrary);
            }
        }

        /// <inheritdoc/>
        public override void UnInstall()
        {
            this.DeleteLibrary();
        }

        private void DeleteLibrary()
        {
            try
            {
                // Attempt to get the Library
                var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == this.Package.PackageID);

                if (library != null)
                {
                    JavaScriptLibraryController.Instance.DeleteLibrary(library);

                    this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.LIBRARY_UnRegistered, library.LibraryName));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
