// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Urls;

    internal class UrlProviderInstaller : ComponentInstallerBase
    {
        private ExtensionUrlProviderInfo _extensionUrlProvider;
        private ExtensionUrlProviderInfo _installedExtensionUrlProvider;
        private string _desktopModuleName;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        public override string AllowableFiles
        {
            get
            {
                return "ashx, aspx, ascx, vb, cs, resx, css, js, resources, config, vbproj, csproj, sln, htm, html";
            }
        }

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
        /// The Install method installs the UrlProvider component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            try
            {
                // Ensure DesktopModule Cache is cleared
                DataCache.RemoveCache(string.Format(DataCache.DesktopModuleCacheKey, Null.NullInteger));

                var desktopModule = DesktopModuleController.GetDesktopModuleByModuleName(this._desktopModuleName, Null.NullInteger);
                if (desktopModule != null)
                {
                    this._extensionUrlProvider.DesktopModuleId = desktopModule.DesktopModuleID;
                }

                // Attempt to get the Desktop Module
                this._installedExtensionUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger)
                                            .SingleOrDefault(p => p.ProviderType == this._extensionUrlProvider.ProviderType);

                if (this._installedExtensionUrlProvider != null)
                {
                    this._extensionUrlProvider.ExtensionUrlProviderId = this._installedExtensionUrlProvider.ExtensionUrlProviderId;
                }

                ExtensionUrlProviderController.SaveProvider(this._extensionUrlProvider);

                this.Completed = true;
                this.Log.AddInfo(string.Format(Util.URLPROVIDER_Registered, this._extensionUrlProvider.ProviderName));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadManifest method reads the manifest file for the compoent.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void ReadManifest(XPathNavigator manifestNav)
        {
            this._extensionUrlProvider = new ExtensionUrlProviderInfo
            {
                ProviderName = Util.ReadElement(manifestNav, "urlProvider/name", this.Log, Util.URLPROVIDER_NameMissing),
                ProviderType = Util.ReadElement(manifestNav, "urlProvider/type", this.Log, Util.URLPROVIDER_TypeMissing),
                SettingsControlSrc = Util.ReadElement(manifestNav, "urlProvider/settingsControlSrc"),
                IsActive = true,
                RedirectAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/redirectAllUrls", "false")),
                ReplaceAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/replaceAllUrls", "false")),
                RewriteAllUrls = Convert.ToBoolean(Util.ReadElement(manifestNav, "urlProvider/rewriteAllUrls", "false")),
            };

            this._desktopModuleName = Util.ReadElement(manifestNav, "urlProvider/desktopModule");
            if (this.Log.Valid)
            {
                this.Log.AddInfo(Util.URLPROVIDER_ReadSuccess);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Rollback method undoes the installation of the component in the event
        /// that one of the other components fails.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Rollback()
        {
            // If Temp Provider exists then we need to update the DataStore with this
            if (this._installedExtensionUrlProvider == null)
            {
                // No Temp Provider - Delete newly added module
                this.DeleteProvider();
            }
            else
            {
                // Temp Provider - Rollback to Temp
                ExtensionUrlProviderController.SaveProvider(this._installedExtensionUrlProvider);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.DeleteProvider();
        }

        private void DeleteProvider()
        {
            try
            {
                ExtensionUrlProviderInfo tempUrlProvider = ExtensionUrlProviderController.GetProviders(Null.NullInteger).Where(p => p.ProviderName == this._extensionUrlProvider.ProviderName && p.ProviderType == this._extensionUrlProvider.ProviderType).FirstOrDefault();
                if (tempUrlProvider != null)
                {
                    ExtensionUrlProviderController.DeleteProvider(tempUrlProvider);

                    this.Log.AddInfo(string.Format(Util.URLPROVIDER_UnRegistered, tempUrlProvider.ProviderName));
                }
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
