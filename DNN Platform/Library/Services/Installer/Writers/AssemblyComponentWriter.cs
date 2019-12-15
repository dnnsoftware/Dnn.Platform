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
    /// The AssemblyComponentWriter class handles creating the manifest for Assembly
    /// Component(s)
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class AssemblyComponentWriter : FileComponentWriter
    {
        public AssemblyComponentWriter(string basePath, Dictionary<string, InstallFile> assemblies, PackageInfo package) : base(basePath, assemblies, package)
        {
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Collection Node ("assemblies")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string CollectionNodeName
        {
            get
            {
                return "assemblies";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Component Type ("Assembly")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ComponentType
        {
            get
            {
                return "Assembly";
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the name of the Item Node ("assembly")
        /// </summary>
        /// <value>A String</value>
        /// -----------------------------------------------------------------------------
        protected override string ItemNodeName
        {
            get
            {
                return "assembly";
            }
        }
    }
}
