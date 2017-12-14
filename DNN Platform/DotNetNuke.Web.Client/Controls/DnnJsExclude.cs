using DotNetNuke.Web.Client.Controls;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    public class DnnJsExclude : ClientResourceExclude
    {
        public DnnJsExclude()
        {
            DependencyType = ClientDependency.Core.ClientDependencyType.Javascript;
        }
    }
}
