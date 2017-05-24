using System;
using System.Threading;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MenuPermissionAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        /// <summary>
        /// The default service scope when <see cref="MenuName"/> is not defined.
        /// </summary>
        public ServiceScope Scope { get; set; }

        /// <summary>
        /// The menu identifier which decide the api whether can requested.
        /// For example, if this value set to "Pages", the user who have access to pages module can request api.
        /// Users who don't have permissions to Pages module, will not available to request the api.
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// The Roles which need exclude from permissions, when user in the role will receive 401 exception.
        /// If need set multiple roles, put semicolon(;) between role names. 
        /// </summary>
        public string Exclude { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var authenticated = Thread.CurrentPrincipal.Identity.IsAuthenticated;
            var portalSettings = PortalSettings.Current;
            var currentUser = UserController.Instance.GetCurrentUserInfo();

            var administratorRoleName = Constants.AdminsRoleName;
            if (portalSettings != null)
            {
                administratorRoleName = portalSettings.AdministratorRoleName;
            }

            var isHost = currentUser.IsSuperUser;
            var isAdmin = currentUser.IsInRole(administratorRoleName);
            var isRegular = currentUser.UserID > 0;

            if (authenticated && isHost)
            {
                return true;
            }

            //when there have excluded roles defined, and current user in the role. the service call will failed.
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

            //if menu identifier defined, then will check the menu permission, multiple identifier should split with ",".
            if (!string.IsNullOrEmpty(MenuName))
            {
                var hasPermission = false;
                MenuName.Split(',').ForEach(menuName =>
                {
                    if (!hasPermission)
                    {
                        var menuItem = GetMenuByIdentifier(menuName);
                        if (menuItem != null && portalSettings != null)
                        {
                            hasPermission = PersonaBarController.Instance.IsVisible(portalSettings, portalSettings.UserInfo, menuItem);
                        }
                    }
                });

                if (hasPermission)
                {
                    return true;
                }
            }
            

            //when menu identifier not defined, will check the service scope permission.
            switch (Scope)
            {
                case ServiceScope.Admin:
                    return authenticated && isAdmin;
                case ServiceScope.Regular:
                    if (portalSettings != null)
                    {
                        //if user have ability on any persona bar menus, then need allow to request api.
                        return PersonaBarController.Instance.GetMenu(portalSettings, portalSettings.UserInfo).AllItems.Count > 0;
                    }
                    
                    return isAdmin || isRegular;
                default:
                    return false;
            }
        }

        private MenuItem GetMenuByIdentifier(string menuName)
        {
            if (string.IsNullOrEmpty(menuName))
            {
                return null;
            }

            return PersonaBarRepository.Instance.GetMenuItem(menuName.Trim());
        }
    }
}
