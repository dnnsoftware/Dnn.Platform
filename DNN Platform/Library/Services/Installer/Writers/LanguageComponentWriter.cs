// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LanguageComponentWriter class handles creating the manifest for Language
    /// Component(s).
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class LanguageComponentWriter : FileComponentWriter
    {
        private readonly int _DependentPackageID;
        private readonly Locale _Language;
        private readonly LanguagePackType _PackageType;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageComponentWriter"/> class.
        /// Constructs the LanguageComponentWriter.
        /// </summary>
        /// <param name="language">Language Info.</param>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package">Package Info.</param>
        /// -----------------------------------------------------------------------------
        public LanguageComponentWriter(Locale language, string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(basePath, files, package)
        {
            this._Language = language;
            this._PackageType = LanguagePackType.Core;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageComponentWriter"/> class.
        /// Constructs the LanguageComponentWriter.
        /// </summary>
        /// <param name="languagePack">Language Package info.</param>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package">Package Info.</param>
        /// -----------------------------------------------------------------------------
        public LanguageComponentWriter(LanguagePackInfo languagePack, string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(basePath, files, package)
        {
            this._Language = LocaleController.Instance.GetLocale(languagePack.LanguageID);
            this._PackageType = languagePack.PackageType;
            this._DependentPackageID = languagePack.DependentPackageID;
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
        /// Gets the name of the Component Type ("CoreLanguage/ExtensionLanguage").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                if (this._PackageType == LanguagePackType.Core)
                {
                    return "CoreLanguage";
                }
                else
                {
                    return "ExtensionLanguage";
                }
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
        /// The WriteCustomManifest method writes the custom manifest items.
        /// </summary>
        /// <param name="writer">The Xmlwriter to use.</param>
        /// -----------------------------------------------------------------------------
        protected override void WriteCustomManifest(XmlWriter writer)
        {
            // Write language Elements
            writer.WriteElementString("code", this._Language.Code);
            if (this._PackageType == LanguagePackType.Core)
            {
                writer.WriteElementString("displayName", this._Language.Text);
                writer.WriteElementString("fallback", this._Language.Fallback);
            }
            else
            {
                PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == this._DependentPackageID);
                writer.WriteElementString("package", package.Name);
            }
        }
    }
}
