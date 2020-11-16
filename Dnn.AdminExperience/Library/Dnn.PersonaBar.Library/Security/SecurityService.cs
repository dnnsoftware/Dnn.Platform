// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Security
{
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public class SecurityService : ISecurityService
    {
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

        public virtual bool IsPagesAdminUser()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }
    }
}
