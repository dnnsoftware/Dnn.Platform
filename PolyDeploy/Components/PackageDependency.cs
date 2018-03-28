using DotNetNuke.Services.Installer.Dependencies;
using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class PackageDependency
    {
        private static readonly ISet<string> PackageTypes = new HashSet<string>(new [] { "PACKAGE", "MANAGEDPACKAGE" }, StringComparer.OrdinalIgnoreCase);

        public bool IsPackageDependency { get; set; }
        public string PackageName { get; set; }
        internal bool DnnMet { get; set; }
        internal bool DeployMet { get; set; }

        internal bool IsMet
        {
            get
            {
                return DnnMet || DeployMet;
            }
        }

        public PackageDependency(XPathNavigator dependencyRoot)
        {
            IsPackageDependency = PackageTypes.Contains(dependencyRoot.GetAttribute("type", ""));
            PackageName = dependencyRoot.Value;
            DnnMet = false;
            DeployMet = false;

            IDependency dep = DependencyFactory.GetDependency(dependencyRoot);

            DnnMet = dep.IsValid;
        }
    }
}
