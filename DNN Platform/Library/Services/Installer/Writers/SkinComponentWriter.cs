// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this._SkinName = skinName;
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
            writer.WriteElementString(this.SkinNameNodeName, this._SkinName);
        }
        
        #endregion
    }
}
