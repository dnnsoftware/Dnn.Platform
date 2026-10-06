// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using System.Xml.XPath;

    using DotNetNuke.Services.Installer.Installers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>Information about a package to be installed.</summary>
    internal sealed class PackageJob
    {
        /// <summary>Initializes a new instance of the <see cref="PackageJob"/> class.</summary>
        /// <param name="serviceProvider">The DI container.</param>
        /// <param name="packageInstaller">The installer.</param>
        public PackageJob(IServiceProvider serviceProvider, PackageInstaller packageInstaller)
        {
            this.Name = packageInstaller.Package.Name;
            this.Version = packageInstaller.Package.Version;
            this.Dependencies = new List<PackageDependency>();

            using var stringReader = new StringReader(packageInstaller.Package.Manifest);
            using var xmlReader = XmlReader.Create(stringReader, new XmlReaderSettings { XmlResolver = null, });
            XPathDocument document = new XPathDocument(xmlReader);

            XPathNavigator rootNav = document.CreateNavigator();

            rootNav.MoveToFirstChild();

            foreach (XPathNavigator nav in rootNav.Select("dependencies/dependency"))
            {
                this.Dependencies.Add(new PackageDependency(serviceProvider, nav));
            }
        }

        [JsonConstructor]
        private PackageJob()
        {
        }

        /// <summary>Gets or sets the package name.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the package dependencies.</summary>
        public List<PackageDependency> Dependencies { get; set; }

        /// <summary>Gets the version as a <see cref="string"/>.</summary>
        public string VersionStr => this.Version.ToString();

        /// <summary>Gets a value indicating whether this package can be installed (i.e. if all of its <see cref="Dependencies"/> have been met).</summary>
        public bool CanInstall
        {
            get
            {
                foreach (PackageDependency dependency in this.Dependencies)
                {
                    if (!dependency.IsMet)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        [JsonProperty]
        [JsonConverter(typeof(VersionConverter))]
        private Version Version { get; set; }
    }
}
