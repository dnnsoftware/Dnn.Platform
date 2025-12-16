// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Packages;

    /// <summary>
    /// The ScriptComponentWriter class handles creating the manifest for Script
    /// Component(s).
    /// </summary>
    public class ScriptComponentWriter : FileComponentWriter
    {
        /// <summary>Initializes a new instance of the <see cref="ScriptComponentWriter"/> class.</summary>
        /// <param name="basePath">The script files base path.</param>
        /// <param name="scripts">The script files.</param>
        /// <param name="package">The package info.</param>
        public ScriptComponentWriter(string basePath, Dictionary<string, InstallFile> scripts, PackageInfo package)
            : base(basePath, scripts, package)
        {
        }

        /// <summary>Gets the name of the Collection Node ("scripts").</summary>
        /// <value>A String.</value>
        protected override string CollectionNodeName
        {
            get
            {
                return "scripts";
            }
        }

        /// <summary>Gets the name of the Component Type ("Script").</summary>
        /// <value>A String.</value>
        protected override string ComponentType
        {
            get
            {
                return "Script";
            }
        }

        /// <summary>Gets the name of the Item Node ("script").</summary>
        /// <value>A String.</value>
        protected override string ItemNodeName
        {
            get
            {
                return "script";
            }
        }

        /// <inheritdoc/>
        protected override void WriteFileElement(XmlWriter writer, InstallFile file)
        {
            this.Log.AddInfo(string.Format(CultureInfo.InvariantCulture, Util.WRITER_AddFileToManifest, file.Name));
            string type = "Install";
            string version = Null.NullString;
            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (fileName.Equals("uninstall", StringComparison.OrdinalIgnoreCase))
            {
                // UnInstall.SqlDataprovider
                type = "UnInstall";
                version = this.Package.Version.ToString(3);
            }
            else if (fileName.Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                // Install.SqlDataprovider
                type = "Install";
                version = new Version(0, 0, 0).ToString(3);
            }
            else if (fileName.StartsWith("Install"))
            {
                // Install.xx.xx.xx.SqlDataprovider
                type = "Install";
                version = fileName.Replace("Install.", string.Empty);
            }
            else
            {
                // xx.xx.xx.SqlDataprovider
                type = "Install";
                version = fileName;
            }

            // Start file Element
            writer.WriteStartElement(this.ItemNodeName);
            writer.WriteAttributeString("type", type);

            // Write path
            if (!string.IsNullOrEmpty(file.Path))
            {
                writer.WriteElementString("path", file.Path);
            }

            // Write name
            writer.WriteElementString("name", file.Name);

            // 'Write sourceFileName
            if (!string.IsNullOrEmpty(file.SourceFileName))
            {
                writer.WriteElementString("sourceFileName", file.SourceFileName);
            }

            // Write Version
            writer.WriteElementString("version", version);

            // Close file Element
            writer.WriteEndElement();
        }
    }
}
