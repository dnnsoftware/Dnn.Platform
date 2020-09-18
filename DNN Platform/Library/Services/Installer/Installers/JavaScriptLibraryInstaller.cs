// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.IO;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.JavaScriptLibraries;

    public class JavaScriptLibraryInstaller : ComponentInstallerBase
    {
        private JavaScriptLibrary _library;
        private JavaScriptLibrary _installedLibrary;

        public override void Commit()
        {
        }

        public override void Install()
        {
            try
            {
                // Attempt to get the JavaScript Library
                this._installedLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == this._library.LibraryName && l.Version == this._library.Version);

                if (this._installedLibrary != null)
                {
                    this._library.JavaScriptLibraryID = this._installedLibrary.JavaScriptLibraryID;
                }

                // Save JavaScript Library  to database
                this._library.PackageID = this.Package.PackageID;
                JavaScriptLibraryController.Instance.SaveLibrary(this._library);

                this.Completed = true;
                this.Log.AddInfo(string.Format(Util.LIBRARY_Registered, this._library.LibraryName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        public override void ReadManifest(XPathNavigator manifestNav)
        {
            // Load the JavaScript Library from the manifest
            this._library = CBO.DeserializeObject<JavaScriptLibrary>(new StringReader(manifestNav.InnerXml));
            this._library.Version = this.Package.Version;

            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.LIBRARY_ReadSuccess);
            }
        }

        public override void Rollback()
        {
            // If Temp Library exists then we need to update the DataStore with this
            if (this._installedLibrary == null)
            {
                // No Temp Library - Delete newly added library
                this.DeleteLibrary();
            }
            else
            {
                // Temp Library - Rollback to Temp
                JavaScriptLibraryController.Instance.SaveLibrary(this._installedLibrary);
            }
        }

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

                    this.Log.AddInfo(string.Format(Util.LIBRARY_UnRegistered, library.LibraryName));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
