using DotNetNuke.Web.Client.Controls;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    public class DnnCssRemove : ClientResourceRemove
    {
        public DnnCssRemove()
        {
            DependencyType = ClientDependency.Core.ClientDependencyType.Css;
        }
    }
}
