// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ResourceFileComponentWriter class handles creating the manifest for Resource
    /// File Component(s).
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ResourceFileComponentWriter : FileComponentWriter
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceFileComponentWriter"/> class.
        /// Constructs the ResourceFileComponentWriter.
        /// </summary>
        /// <param name="basePath">The Base Path for the files.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package"></param>
        /// -----------------------------------------------------------------------------
        public ResourceFileComponentWriter(string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(basePath, files, package)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("resourceFiles").
        /// </summary>
        /// <value>A String.</value>
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
        /// Gets the name of the Component Type ("ResourceFile").
        /// </summary>
        /// <value>A String.</value>
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
        /// Gets the name of the Item Node ("resourceFile").
        /// </summary>
        /// <value>A String.</value>
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
