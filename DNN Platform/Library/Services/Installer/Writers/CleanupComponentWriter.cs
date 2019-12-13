// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;
using System.IO;
using System.Xml;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The CleanupComponentWriter class handles creating the manifest for Cleanup
    /// Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class CleanupComponentWriter
    {
		#region "Private Members"
		
        private readonly SortedList<string, InstallFile> _Files;
        private string _BasePath;

		#endregion

		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the ContainerComponentWriter
        /// </summary>
        /// <param name="basePath">Base Path.</param>
        /// <param name="files">A Dictionary of files</param>
        /// -----------------------------------------------------------------------------
        public CleanupComponentWriter(string basePath, SortedList<string, InstallFile> files)
        {
            _Files = files;
            _BasePath = basePath;
        }
		
		#endregion

		#region "Public Methods"
		
        public virtual void WriteManifest(XmlWriter writer)
        {
            foreach (KeyValuePair<string, InstallFile> kvp in _Files)
            {
				//Start component Element
                writer.WriteStartElement("component");
                writer.WriteAttributeString("type", "Cleanup");
                writer.WriteAttributeString("fileName", kvp.Value.Name);
                writer.WriteAttributeString("version", Path.GetFileNameWithoutExtension(kvp.Value.Name));

                //End component Element
                writer.WriteEndElement();
            }
        }
		
		#endregion
    }
}
