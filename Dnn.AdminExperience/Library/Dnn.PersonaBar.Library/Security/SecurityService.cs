using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace Dnn.PersonaBar.Library.Security
{
    public class SecurityService : ISecurityService
    {
        public virtual bool IsPagesAdminUser()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }
        public static ISecurityService Instance
        {
            get
            {
                var controller = ComponentFactory.GetComponent<ISecurityService>("SecurityService");
                if (controller == null)
                {
                    ComponentFactory.RegisterComponent<ISecurityService, SecurityService>("SecurityService");
                }

                return ComponentFactory.GetComponent<ISecurityService>("SecurityService");
            }
        }
    }
}
