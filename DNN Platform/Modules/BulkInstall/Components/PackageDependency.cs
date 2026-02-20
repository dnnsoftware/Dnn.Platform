// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;
    using System.Collections.Generic;
    using System.Xml.XPath;

    using DotNetNuke.Services.Installer.Dependencies;

    using Newtonsoft.Json;

    /// <summary>A dependency of a package.</summary>
    internal sealed class PackageDependency
    {
        private static readonly HashSet<string> PackageTypes = new HashSet<string>(["PACKAGE", "MANAGEDPACKAGE",], StringComparer.OrdinalIgnoreCase);

        /// <summary>Initializes a new instance of the <see cref="PackageDependency"/> class.</summary>
        /// <param name="dependencyRoot">The root of the <c>dependency</c> element.</param>
        public PackageDependency(XPathNavigator dependencyRoot)
        {
            this.IsPackageDependency = PackageTypes.Contains(dependencyRoot.GetAttribute("type", string.Empty));
            this.PackageName = dependencyRoot.Value;
            this.DependencyVersion = dependencyRoot.GetAttribute("version", string.Empty);
            this.DnnMet = false;
            this.DeployMet = false;

            IDependency dep = DependencyFactory.GetDependency(dependencyRoot);

            this.DnnMet = dep.IsValid;
        }

        [JsonConstructor]
        private PackageDependency()
        {
        }

        /// <summary>Gets or sets a value indicating whether the dependency is for another package.</summary>
        public bool IsPackageDependency { get; set; }

        /// <summary>Gets or sets the package name.</summary>
        public string PackageName { get; set; }

        /// <summary>Gets or sets the dependency version.</summary>
        public string DependencyVersion { get; set; }

        /// <summary>Gets or sets a value indicating whether DNN already meets this dependency.</summary>
        internal bool DnnMet { get; set; }

        /// <summary>Gets or sets a value indicating whether the deployment meets this dependency.</summary>
        internal bool DeployMet { get; set; }

        /// <summary>Gets a value indicating whether the dependency is met.</summary>
        internal bool IsMet => this.DnnMet || this.DeployMet;
    }
}
