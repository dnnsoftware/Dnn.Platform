// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.Xml;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The FileComponentWriter class handles creating the manifest for File Component(s).
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class FileComponentWriter
    {
        private readonly string _BasePath;
        private readonly Dictionary<string, InstallFile> _Files;
        private readonly PackageInfo _Package;
        private int _InstallOrder = Null.NullInteger;
        private int _UnInstallOrder = Null.NullInteger;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="FileComponentWriter"/> class.
        /// Constructs the FileComponentWriter.
        /// </summary>
        /// <param name="basePath">The Base Path for the files.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package">Package Info.</param>
        /// -----------------------------------------------------------------------------
        public FileComponentWriter(string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
        {
            this._Files = files;
            this._BasePath = basePath;
            this._Package = package;
        }

        public int InstallOrder
        {
            get
            {
                return this._InstallOrder;
            }

            set
            {
                this._InstallOrder = value;
            }
        }

        public int UnInstallOrder
        {
            get
            {
                return this._UnInstallOrder;
            }

            set
            {
                this._UnInstallOrder = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("files").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected virtual string CollectionNodeName
        {
            get
            {
                return "files";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Component Type ("File").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected virtual string ComponentType
        {
            get
            {
                return "File";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("file").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected virtual string ItemNodeName
        {
            get
            {
                return "file";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Logger.
        /// </summary>
        /// <value>A Logger.</value>
        /// -----------------------------------------------------------------------------
        protected virtual Logger Log
        {
            get
            {
                return this._Package.Log;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Package.
        /// </summary>
        /// <value>A PackageInfo.</value>
        /// -----------------------------------------------------------------------------
        protected virtual PackageInfo Package
        {
            get
            {
                return this._Package;
            }
        }

        public virtual void WriteManifest(XmlWriter writer)
        {
            // Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", this.ComponentType);
            if (this.InstallOrder > Null.NullInteger)
            {
                writer.WriteAttributeString("installOrder", this.InstallOrder.ToString());
            }

            if (this.UnInstallOrder > Null.NullInteger)
            {
                writer.WriteAttributeString("unInstallOrder", this.UnInstallOrder.ToString());
            }

            // Start files element
            writer.WriteStartElement(this.CollectionNodeName);

            // Write custom manifest items
            this.WriteCustomManifest(writer);

            // Write basePath Element
            if (!string.IsNullOrEmpty(this._BasePath))
            {
                writer.WriteElementString("basePath", this._BasePath);
            }

            foreach (InstallFile file in this._Files.Values)
            {
                this.WriteFileElement(writer, file);
            }

            // End files Element
            writer.WriteEndElement();

            // End component Element
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The WriteCustomManifest method writes the custom manifest items (that subclasses
        /// of FileComponentWriter may need).
        /// </summary>
        /// <param name="writer">The Xmlwriter to use.</param>
        /// -----------------------------------------------------------------------------
        protected virtual void WriteCustomManifest(XmlWriter writer)
        {
        }

        protected virtual void WriteFileElement(XmlWriter writer, InstallFile file)
        {
            this.Log.AddInfo(string.Format(Util.WRITER_AddFileToManifest, file.Name));

            // Start file Element
            writer.WriteStartElement(this.ItemNodeName);

            // Write path
            if (!string.IsNullOrEmpty(file.Path))
            {
                string path = file.Path;
                if (!string.IsNullOrEmpty(this._BasePath))
                {
                    if (file.Path.ToLowerInvariant().Contains(this._BasePath.ToLowerInvariant()))
                    {
                        path = file.Path.ToLowerInvariant().Replace(this._BasePath.ToLowerInvariant() + "\\", string.Empty);
                    }
                }

                writer.WriteElementString("path", path);
            }

            // Write name
            writer.WriteElementString("name", file.Name);

            // Write sourceFileName
            if (!string.IsNullOrEmpty(file.SourceFileName))
            {
                writer.WriteElementString("sourceFileName", file.SourceFileName);
            }

            // Close file Element
            writer.WriteEndElement();
        }
    }
}
