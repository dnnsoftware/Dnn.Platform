#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
