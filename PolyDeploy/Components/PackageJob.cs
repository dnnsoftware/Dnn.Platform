using DotNetNuke.Services.Installer.Installers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class PackageJob
    {
        public string Name { get; set; }
        public List<PackageDependency> Dependencies { get; set; }
        
        public string VersionStr
        {
            get
            {
                return Version.ToString();
            }
        }

        public bool CanInstall
        {
            get
            {
                foreach (PackageDependency dependency in Dependencies)
                {
                    if (!dependency.IsMet)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private Version Version { get; set; }

        public PackageJob(PackageInstaller packageInstaller)
        {
            Name = packageInstaller.Package.Name;
            Version = packageInstaller.Package.Version;
            Dependencies = new List<PackageDependency>();

            XPathDocument document = new XPathDocument(new StringReader(packageInstaller.Package.Manifest));

            XPathNavigator rootNav = document.CreateNavigator();

            rootNav.MoveToFirstChild();

            foreach (XPathNavigator nav in rootNav.Select("dependencies/dependency"))
            {
                Dependencies.Add(new PackageDependency(nav));
            }
        }
    }
}
