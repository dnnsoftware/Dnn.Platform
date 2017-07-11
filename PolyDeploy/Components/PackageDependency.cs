using DotNetNuke.Services.Installer.Dependencies;
using System.Xml.XPath;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class PackageDependency
    {
        public string Type { get; set; }
        public string Value { get; set; }
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
            Type = dependencyRoot.GetAttribute("type", "");
            Value = dependencyRoot.Value;
            DnnMet = false;
            DeployMet = false;

            IDependency dep = DependencyFactory.GetDependency(dependencyRoot);

            DnnMet = dep.IsValid;
        }
    }
}
