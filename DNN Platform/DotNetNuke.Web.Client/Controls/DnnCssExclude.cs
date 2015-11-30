using DotNetNuke.Web.Client.Controls;

namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    public class DnnCssExclude : ClientResourceExclude
    {
        public DnnCssExclude()
        {
            DependencyType = ClientDependency.Core.ClientDependencyType.Css;
        }
    }
}
