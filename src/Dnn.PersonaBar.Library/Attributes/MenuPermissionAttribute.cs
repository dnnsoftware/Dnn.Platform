#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
                if (isAdmin)
                    return true;

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

                return hasPermission;
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
