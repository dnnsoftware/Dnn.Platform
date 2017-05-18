#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The SkinComponentWriter class handles creating the manifest for Skin Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SkinComponentWriter : FileComponentWriter
    {
		#region "Private Members"

        private readonly string _SkinName;

		#endregion

		#region "Constructors"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the SkinComponentWriter
        /// </summary>
        /// <param name="skinName">The name of the Skin</param>
        /// <param name="basePath">The Base Path for the files</param>
        /// <param name="files">A Dictionary of files</param>
        /// <param name="package">Package Info.</param>
        /// -----------------------------------------------------------------------------
        public SkinComponentWriter(string skinName, string basePath, Dictionary<string, InstallFile> files, PackageInfo package) : base(basePath, files, package)
        {
            _SkinName = skinName;
        }
		
		#endregion

		#region "Protected Properties"

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("skinFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "skinFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Component Type ("Skin")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                return "Skin";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("skinFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "skinFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the SkinName Node ("skinName")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected virtual string SkinNameNodeName
        {
            get
            {
                return "skinName";
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
        protected override void WriteCustomManifest(XmlWriter writer)
        {
            writer.WriteElementString(SkinNameNodeName, _SkinName);
        }
		
		#endregion
    }
}
