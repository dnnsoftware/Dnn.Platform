using System;
using System.Threading;
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
        /// <summary>
        /// The default service scope when <see cref="SupportExtension"/> is not defined.
        /// </summary>
        public ServiceScope Scope { get; set; }

        /// <summary>
        /// The menu identifier which decide the api whether can requested.
        /// For example, if this value set to "Pages", the user who have access to pages module can request api.
        /// Users who don't have permissions to Pages module, will not available to request the api.
        /// </summary>
        public string SupportExtension { get; set; }

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
            var isRegular = currentUser.UserID > 0;

            if (isHost)
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

            var menuItem = GetMenuByIdentifier();
            if (menuItem != null && portalSettings != null)
            {
                //if supported extension defined, then will check the menu permission.
                return PersonaBarController.Instance.IsVisible(portalSettings, portalSettings.UserInfo, menuItem);
            }

            //when menu identifier not defined, will check the service scope permission.
            switch (Scope)
            {
                case ServiceScope.Admin:
                    return isAdmin;
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

        private MenuItem GetMenuByIdentifier()
        {
            if (string.IsNullOrEmpty(SupportExtension))
            {
                return null;
            }

            return PersonaBarRepository.Instance.GetMenuItem(SupportExtension);
        }
    }
}
