using DotNetNuke.Services.Installer.Dependencies;
using System.Xml.XPath;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class PackageDependency
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool DnnFulfilled { get; set; }
        public bool WillFulfill { get; set; }

        public PackageDependency(XPathNavigator dependencyRoot)
        {
            Type = dependencyRoot.GetAttribute("type", "");
            Value = dependencyRoot.Value;
            DnnFulfilled = false;
            WillFulfill = false;

            IDependency dep = DependencyFactory.GetDependency(dependencyRoot);

            DnnFulfilled = dep.IsValid;
        }
    }
}
