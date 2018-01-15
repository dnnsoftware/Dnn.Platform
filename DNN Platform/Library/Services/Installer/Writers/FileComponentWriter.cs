#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
#region Usings

using System.Collections.Generic;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The FileComponentWriter class handles creating the manifest for File Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class FileComponentWriter
    {
		#region "Private Members"

        private readonly string _BasePath;
        private readonly Dictionary<string, InstallFile> _Files;
        private readonly PackageInfo _Package;
        private int _InstallOrder = Null.NullInteger;
        private int _UnInstallOrder = Null.NullInteger;

		#endregion

		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the FileComponentWriter
        /// </summary>
        /// <param name="basePath">The Base Path for the files</param>
        /// <param name="files">A Dictionary of files</param>
        /// <param name="package">Package Info.</param>
        /// -----------------------------------------------------------------------------
        public FileComponentWriter(string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
        {
            _Files = files;
            _BasePath = basePath;
            _Package = package;
        }
		
		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("files")
        /// </summary>
        /// <value>A String</value>
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
        /// Gets the name of the Component Type ("File")
        /// </summary>
        /// <value>A String</value>
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
        /// Gets the name of the Item Node ("file")
        /// </summary>
        /// <value>A String</value>
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
        /// Gets the Logger
        /// </summary>
        /// <value>A Logger</value>
        /// -----------------------------------------------------------------------------
        protected virtual Logger Log
        {
            get
            {
                return _Package.Log;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Package
        /// </summary>
        /// <value>A PackageInfo</value>
        /// -----------------------------------------------------------------------------
        protected virtual PackageInfo Package
        {
            get
            {
                return _Package;
            }
        }
		
		#endregion

		#region "Public Properties"

        public int InstallOrder
        {
            get
            {
                return _InstallOrder;
            }
            set
            {
                _InstallOrder = value;
            }
        }

        public int UnInstallOrder
        {
            get
            {
                return _UnInstallOrder;
            }
            set
            {
                _UnInstallOrder = value;
            }
        }
		
		#endregion

		#region "Protected Methods"


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// The WriteCustomManifest method writes the custom manifest items (that subclasses
        /// of FileComponentWriter may need)
        /// </summary>
        /// <param name="writer">The Xmlwriter to use</param>
        /// -----------------------------------------------------------------------------
        protected virtual void WriteCustomManifest(XmlWriter writer)
        {
        }

        protected virtual void WriteFileElement(XmlWriter writer, InstallFile file)
        {
            Log.AddInfo(string.Format(Util.WRITER_AddFileToManifest, file.Name));

            //Start file Element
            writer.WriteStartElement(ItemNodeName);

            //Write path
            if (!string.IsNullOrEmpty(file.Path))
            {
                string path = file.Path;
                if (!string.IsNullOrEmpty(_BasePath))
                {
                    if (file.Path.ToLowerInvariant().Contains(_BasePath.ToLowerInvariant()))
                    {
                        path = file.Path.ToLowerInvariant().Replace(_BasePath.ToLowerInvariant() + "\\", "");
                    }
                }
                writer.WriteElementString("path", path);
            }
			
            //Write name
            writer.WriteElementString("name", file.Name);

            //Write sourceFileName
            if (!string.IsNullOrEmpty(file.SourceFileName))
            {
                writer.WriteElementString("sourceFileName", file.SourceFileName);
            }
			
            //Close file Element
            writer.WriteEndElement();
        }
		
		#endregion

		#region "Public Methods"

        public virtual void WriteManifest(XmlWriter writer)
        {
			//Start component Element
            writer.WriteStartElement("component");
            writer.WriteAttributeString("type", ComponentType);
            if (InstallOrder > Null.NullInteger)
            {
                writer.WriteAttributeString("installOrder", InstallOrder.ToString());
            }
            if (UnInstallOrder > Null.NullInteger)
            {
                writer.WriteAttributeString("unInstallOrder", UnInstallOrder.ToString());
            }
			
            //Start files element
            writer.WriteStartElement(CollectionNodeName);

            //Write custom manifest items
            WriteCustomManifest(writer);

            //Write basePath Element
            if (!string.IsNullOrEmpty(_BasePath))
            {
                writer.WriteElementString("basePath", _BasePath);
            }
            foreach (InstallFile file in _Files.Values)
            {
                WriteFileElement(writer, file);
            }
			
            //End files Element
            writer.WriteEndElement();

            //End component Element
            writer.WriteEndElement();
        }
		
		#endregion
    }
}
