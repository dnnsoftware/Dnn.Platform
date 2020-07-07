// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Installers
{
    using System;
    using System.Xml.XPath;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguageInstaller installs Language Packs to a DotNetNuke site.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class LanguageInstaller : FileInstaller
    {
        private readonly LanguagePackType LanguagePackType;
        private LanguagePackInfo InstalledLanguagePack;
        private Locale Language;
        private LanguagePackInfo LanguagePack;
        private Locale TempLanguage;

        public LanguageInstaller(LanguagePackType type)
        {
            this.LanguagePackType = type;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a list of allowable file extensions (in addition to the Host's List).
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        public override string AllowableFiles
        {
            get { return "resx, xml, tdf,template"; }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("languageFiles").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "languageFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("languageFile").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "languageFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Commit method finalises the Install and commits any pending changes.
        /// </summary>
        /// <remarks>In the case of Modules this is not neccessary.</remarks>
        /// -----------------------------------------------------------------------------
        public override void Commit()
        {
            if (this.LanguagePackType == LanguagePackType.Core || this.LanguagePack.DependentPackageID > 0)
            {
                base.Commit();
            }
            else
            {
                this.Completed = true;
                this.Skipped = true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The Install method installs the language component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void Install()
        {
            if (this.LanguagePackType == LanguagePackType.Core || this.LanguagePack.DependentPackageID > 0)
            {
                try
                {
                    // Attempt to get the LanguagePack
                    this.InstalledLanguagePack = LanguagePackController.GetLanguagePackByPackage(this.Package.PackageID);
                    if (this.InstalledLanguagePack != null)
                    {
                        this.LanguagePack.LanguagePackID = this.InstalledLanguagePack.LanguagePackID;
                    }

                    // Attempt to get the Locale
                    this.TempLanguage = LocaleController.Instance.GetLocale(this.Language.Code);
                    if (this.TempLanguage != null)
                    {
                        this.Language.LanguageId = this.TempLanguage.LanguageId;
                    }

                    if (this.LanguagePack.PackageType == LanguagePackType.Core)
                    {
                        // Update language
                        Localization.SaveLanguage(this.Language);
                    }

                    // Set properties for Language Pack
                    this.LanguagePack.PackageID = this.Package.PackageID;
                    this.LanguagePack.LanguageID = this.Language.LanguageId;

                    // Update LanguagePack
                    LanguagePackController.SaveLanguagePack(this.LanguagePack);

                    this.Log.AddInfo(string.Format(Util.LANGUAGE_Registered, this.Language.Text));

                    // install (copy the files) by calling the base class
                    base.Install();
                }
                catch (Exception ex)
                {
                    this.Log.AddFailure(ex);
                }
            }
            else
            {
                this.Completed = true;
                this.Skipped = true;
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
            // If Temp Language exists then we need to update the DataStore with this
            if (this.TempLanguage == null)
            {
                // No Temp Language - Delete newly added Language
                this.DeleteLanguage();
            }
            else
            {
                // Temp Language - Rollback to Temp
                Localization.SaveLanguage(this.TempLanguage);
            }

            // Call base class to prcoess files
            base.Rollback();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The UnInstall method uninstalls the language component.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void UnInstall()
        {
            this.DeleteLanguage();

            // Call base class to process files
            base.UnInstall();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The ReadCustomManifest method reads the custom manifest items.
        /// </summary>
        /// <param name="nav">The XPathNavigator representing the node.</param>
        /// -----------------------------------------------------------------------------
        protected override void ReadCustomManifest(XPathNavigator nav)
        {
            this.Language = new Locale();
            this.LanguagePack = new LanguagePackInfo();

            // Get the Skin name
            this.Language.Code = Util.ReadElement(nav, "code");
            this.Language.Text = Util.ReadElement(nav, "displayName");
            this.Language.Fallback = Util.ReadElement(nav, "fallback");

            if (this.LanguagePackType == LanguagePackType.Core)
            {
                this.LanguagePack.DependentPackageID = -2;
            }
            else
            {
                string packageName = Util.ReadElement(nav, "package");
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name.Equals(packageName, StringComparison.OrdinalIgnoreCase));
                if (package != null)
                {
                    this.LanguagePack.DependentPackageID = package.PackageID;
                }
            }

            // Call base class
            base.ReadCustomManifest(nav);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The DeleteLanguage method deletes the Language
        /// from the data Store.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void DeleteLanguage()
        {
            try
            {
                // Attempt to get the LanguagePack
                LanguagePackInfo tempLanguagePack = LanguagePackController.GetLanguagePackByPackage(this.Package.PackageID);

                // Attempt to get the Locale
                Locale language = LocaleController.Instance.GetLocale(tempLanguagePack.LanguageID);
                if (tempLanguagePack != null)
                {
                    LanguagePackController.DeleteLanguagePack(tempLanguagePack);
                }

                // fix DNN-26330     Removing a language pack extension removes the language
                // we should not delete language when deleting language pack, as there is just a loose relationship
                // if (language != null && tempLanguagePack.PackageType == LanguagePackType.Core)
                // {
                //    Localization.DeleteLanguage(language);
                // }
                this.Log.AddInfo(string.Format(Util.LANGUAGE_UnRegistered, language.Text));
            }
            catch (Exception ex)
            {
                this.Log.AddFailure(ex);
            }
        }
    }
}
