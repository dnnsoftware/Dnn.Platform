using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dnn.PersonaBar.Library.Containers;
using Dnn.PersonaBar.Library.PersonaBar.Controllers;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using Dnn.PersonaBar.Library.PersonaBar.Repository;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ServiceScopeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        public ServiceScope Scope { get; set; }

        public string Identifier { get; set; }

        /// <summary>
        /// The Roles which need exclude from permissions, when user in the role will receive 401 exception.
        /// If need set multiple roles, put semicolon(;) between role names. 
        /// </summary>
        public string Exclude { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return false;
            }

            var portalSettings = PortalSettings.Current;
            var currentUser = UserController.Instance.GetCurrentUserInfo();

            var administratorRoleName = Constants.AdminsRoleName;
            if (portalSettings != null)
            {
                administratorRoleName = portalSettings.AdministratorRoleName;
            }

            var isHost = currentUser.IsSuperUser;
            var isAdmin = currentUser.IsInRole(administratorRoleName);
            var isRegular = PersonaBarContainer.Instance.EditorRoles.Any(r => currentUser.IsInRole(r));

            if (isHost)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(Exclude))
            {
                foreach (var roleName in Exclude.Split(';'))
                {
                    var cleanRoleName = roleName.Trim();
                    if (!string.IsNullOrEmpty(cleanRoleName))
                    {
                        if (currentUser.IsInRole(cleanRoleName))
                        {
                            return false;
                        }
                    }
                }
            }

            //if menu identifier defined, then will check the menu permission.
            var menuItem = GetMenu();
            if (menuItem != null && portalSettings != null)
            {
                return PersonaBarController.Instance.IsVisible(portalSettings, portalSettings.UserInfo, menuItem);
            }

            //when menu identifier not defined, will check the service scope permission.
            switch (Scope)
            {
                case ServiceScope.Admin:
                case ServiceScope.AdminHost:
                    return isAdmin;
                case ServiceScope.Regular:
                case ServiceScope.Common:
                    if (portalSettings != null)
                    {
                        return PersonaBarController.Instance.GetMenu(portalSettings, portalSettings.UserInfo).AllItems.Count > 0;
                    }
                    
                    return isAdmin || isRegular;
                default:
                    return false;
            }
        }

        private MenuItem GetMenu()
        {
            if (string.IsNullOrEmpty(Identifier))
            {
                return null;
            }

            return PersonaBarRepository.Instance.GetMenuItem(Identifier);
        }
    }
}
