// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Collections.Generic;

using DotNetNuke.Services.Installer.Packages;

#endregion

namespace DotNetNuke.Services.Installer.Writers
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ResourceFileComponentWriter class handles creating the manifest for Resource
    /// File Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ResourceFileComponentWriter : FileComponentWriter
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs the ResourceFileComponentWriter
        /// </summary>
        /// <param name="basePath">The Base Path for the files</param>
        /// <param name="files">A Dictionary of files</param>
        /// <param name="package"></param>
        /// -----------------------------------------------------------------------------
        public ResourceFileComponentWriter(string basePath, Dictionary<string, InstallFile> files, PackageInfo package) : base(basePath, files, package)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("resourceFiles")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "resourceFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Component Type ("ResourceFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                return "ResourceFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("resourceFile")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "resourceFile";
            }
        }
    }
}
