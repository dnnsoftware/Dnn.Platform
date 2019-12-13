// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.IO;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework.JavaScriptLibraries;

namespace DotNetNuke.Services.Installer.Installers
{
    public class JavaScriptLibraryInstaller : ComponentInstallerBase
    {
        private JavaScriptLibrary _library;
        private JavaScriptLibrary _installedLibrary;

        #region Public Methods

        private void DeleteLibrary()
        {
            try
            {
                //Attempt to get the Library
                var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.PackageID == Package.PackageID);

                if (library != null)
                {
                    JavaScriptLibraryController.Instance.DeleteLibrary(library);

                    Log.AddInfo(string.Format(Util.LIBRARY_UnRegistered, library.LibraryName));
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        public override void Commit()
        {
        }

        public override void Install()
        {
            try
            {
                //Attempt to get the JavaScript Library
                _installedLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == _library.LibraryName && l.Version == _library.Version);

                if (_installedLibrary != null)
                {
                    _library.JavaScriptLibraryID = _installedLibrary.JavaScriptLibraryID;
                }
                //Save JavaScript Library  to database
                _library.PackageID = Package.PackageID;
                JavaScriptLibraryController.Instance.SaveLibrary(_library);

                Completed = true;
                Log.AddInfo(string.Format(Util.LIBRARY_Registered, _library.LibraryName));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        public override void ReadManifest(XPathNavigator manifestNav)
        {
            //Load the JavaScript Library from the manifest
            _library = CBO.DeserializeObject<JavaScriptLibrary>(new StringReader(manifestNav.InnerXml));
            _library.Version = Package.Version;

            if (Log.Valid)
            {
                Log.AddInfo(Util.LIBRARY_ReadSuccess);
            }
        }

        public override void Rollback()
        {
            //If Temp Library exists then we need to update the DataStore with this 
            if (_installedLibrary == null)
            {
                //No Temp Library - Delete newly added library
                DeleteLibrary();
            }
            else
            {
                //Temp Library - Rollback to Temp
                JavaScriptLibraryController.Instance.SaveLibrary(_installedLibrary);
            }
        }

        public override void UnInstall()
        {
            DeleteLibrary();
        }

        #endregion

    }
}
