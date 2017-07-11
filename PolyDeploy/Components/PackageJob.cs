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

        public bool DidInstall { get; set; }

        internal List<PackageDependency> Dependencies { get; set; }
        
        private PackageInstaller PackageInstaller { get; set; }

        public string Version
        {
            get
            {
                if (version == null)
                {
                    return null;
                }

                return version.ToString();
            }
        }

        private bool Installable
        {
            get
            {
                var canInstall = true;

                foreach (PackageDependency packDep in Dependencies)
                {
                    if (!(packDep.DnnFulfilled || packDep.WillFulfill))
                    {
                        canInstall = false;
                        break;
                    }
                }

                return canInstall;
            }
        }

        protected Version version { get; set; }

        public PackageJob(PackageInstaller packageInstaller)
        {
            Name = packageInstaller.Package.Name;
            version = packageInstaller.Package.Version;
            Dependencies = new List<PackageDependency>();
            DidInstall = false;

            PackageInstaller = packageInstaller;

            XPathDocument document = new XPathDocument(new StringReader(packageInstaller.Package.Manifest));

            XPathNavigator rootNav = document.CreateNavigator();

            rootNav.MoveToFirstChild();

            foreach (XPathNavigator nav in rootNav.Select("dependencies/dependency"))
            {
                Dependencies.Add(new PackageDependency(nav));
            }
        }

        public void Install()
        {
            if (Installable)
            {
                PackageInstaller.Install();

                DidInstall = true;
            }
        }
    }
}
