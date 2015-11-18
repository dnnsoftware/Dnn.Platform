using DotNetNuke.Web.Client.Controls;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    public class DnnJsRemove : ClientResourceRemove
    {
        public DnnJsRemove()
        {
            DependencyType = ClientDependency.Core.ClientDependencyType.Javascript;
        }
    }
}
