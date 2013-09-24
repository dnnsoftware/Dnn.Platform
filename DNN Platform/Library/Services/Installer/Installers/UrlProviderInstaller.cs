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
using System.Linq;
using System.Xml.XPath;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Urls;

namespace DotNetNuke.Services.Installer.Installers
{
    class UrlProviderInstaller : ComponentInstallerBase
    {
        private ExtensionUrlProviderInfo _extensionUrlProvider;
        private ExtensionUrlProviderInfo _installedExtensionUrlProvider;
        private string _desktopModuleName;

        #region "Public Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List)
        /// </summary>
        /// <value>A String</value>
        /// <history>
        /// 	[cnurse]	03/28/2008  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }

        #endregion


        private void DeleteProvider()
        {
            try
            {
                //Attempt to get the Desktop Module
                //Attempt to get the Desktop Module
                DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByPackageID(Package.PackageID);

                ExtensionUrlProviderInfo tempUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger)
                                            .SingleOrDefault(p => p.DesktopModuleId == desktopModule.DesktopModuleID);
                if (tempUrlProvider != null)
                {
                    ExtensionUrlProviderController.DeleteProvider(tempUrlProvider);

                    Log.AddInfo(string.Format(Util.URLPROVIDER_UnRegistered, tempUrlProvider.ProviderName));
                }
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        #region Public Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the UrlProvider component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                //Ensure DesktopModule Cache is cleared
                DataCache.RemoveCache(String.Format(DataCache.DesktopModuleCacheKey, Null.NullInteger));

                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(_desktopModuleName, Null.NullInteger);
                if (desktopModule != null)
                {
                    _extensionUrlProvider.DesktopModuleId = desktopModule.DesktopModuleID;
                }

                //Attempt to get the Desktop Module
                _installedExtensionUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger)
                                            .SingleOrDefault(p => p.ProviderType == _extensionUrlProvider.ProviderType);

                if (_installedExtensionUrlProvider != null)
                {
                    _extensionUrlProvider.ExtensionUrlProviderId = _installedExtensionUrlProvider.ExtensionUrlProviderId;
                }

                ExtensionUrlProviderController.SaveProvider(_extensionUrlProvider);

                Completed = true;
                Log.AddInfo(string.Format(Util.URLPROVIDER_Registered, _extensionUrlProvider.ProviderName));
            }
            catch (Exception ex)
            {
                Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            _extensionUrlProvider = new ExtensionUrlProviderInfo
                {
                    ProviderName = Util.ReadElement(manifestNav, "urlProvider/name", Log, Util.URLPROVIDER_NameMissing),
                    ProviderType = Util.ReadElement(manifestNav, "urlProvider/type", Log, Util.URLPROVIDER_TypeMissing),
                    SettingsControlSrc = Util.ReadElement(manifestNav, "urlProvider/settingsControlSrc"),
                    IsActive = true,
                    RedirectAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/redirectAllUrls", "false")),
                    ReplaceAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/replaceAllUrls", "false")),
                    RewriteAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/rewriteAllUrls", "false"))
                };

            _desktopModuleName = Util.ReadElement(manifestNav, "urlProvider/desktopModule");
            if (Log.Valid)
            {
                Log.AddInfo(Util.URLPROVIDER_ReadSuccess);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event 
        /// that one of the other components fails
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            //If Temp Provider exists then we need to update the DataStore with this 
            if (_installedExtensionUrlProvider == null)
            {
                //No Temp Provider - Delete newly added module
                DeleteProvider();
            }
            else
            {
                //Temp Provider - Rollback to Temp
                ExtensionUrlProviderController.SaveProvider(_installedExtensionUrlProvider);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the component
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            DeleteProvider();
        }

        #endregion
    }
}
