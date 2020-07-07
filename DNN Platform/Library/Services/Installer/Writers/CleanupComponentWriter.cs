// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CleanupComponentWriter class handles creating the manifest for Cleanup
    /// Component(s).
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class CleanupComponentWriter
    {
        private readonly SortedList<string, InstallFile> _Files;
        private string _BasePath;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupComponentWriter"/> class.
        /// Constructs the ContainerComponentWriter.
        /// </summary>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// -----------------------------------------------------------------------------
        public CleanupComponentWriter(string basePath, SortedList<string, InstallFile> files)
        {
            this._Files = files;
            this._BasePath = basePath;
        }

        public virtual void WriteManifest(XmlWriter writer)
        {
            foreach (KeyValuePair<string, InstallFile> kvp in this._Files)
            {
                // Start component Element
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", "Cleanup");
                writer.WriteAttributeString("fileName", kvp.Value.Name);
                writer.WriteAttributeString("version", Path.GetFileNameWithoutExtension(kvp.Value.Name));

                // End component Element
                writer.WriteEndElement();
            }
        }
    }
}
