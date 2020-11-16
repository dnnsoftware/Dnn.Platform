// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Installer.Writers
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Installer.Packages;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ContainerComponentWriter class handles creating the manifest for Container
    /// Component(s).
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class ContainerComponentWriter : SkinComponentWriter
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerComponentWriter"/> class.
        /// Constructs the ContainerComponentWriter.
        /// </summary>
        /// <param name="containerName">The name of the Container.</param>
        /// <param name="basePath">The Base Path for the files.</param>
        /// <param name="files">A Dictionary of files.</param>
        /// <param name="package"></param>
        /// -----------------------------------------------------------------------------
        public ContainerComponentWriter(string containerName, string basePath, Dictionary<string, InstallFile> files, PackageInfo package)
            : base(containerName, basePath, files, package)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("containerFiles").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "containerFiles";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Component Type ("Skin").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                return "Container";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("containerFile").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "containerFile";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the SkinName Node ("containerName").
        /// </summary>
        /// <value>A String.</value>
        /// -----------------------------------------------------------------------------
        protected override string SkinNameNodeName
        {
            get
            {
                return "containerName";
            }
        }
    }
}
