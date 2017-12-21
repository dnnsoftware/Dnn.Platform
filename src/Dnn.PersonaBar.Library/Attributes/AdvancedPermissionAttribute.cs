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
using System.Linq;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Library.Permissions;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Collections;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdvancedPermissionAttribute : AuthorizeAttributeBase
    {
        /// <summary>
        /// The menu identifier.
        /// </summary>
        public string MenuName { get; set; }

        /// <summary>
        /// The permission key. 
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// When true, it will force admin to have explicit Permission. When false, admin is passed without checking the Permission.
        /// </summary>
        public bool CheckPermissionForAdmin { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var menuItem = GetMenuByIdentifier();
            var portalSettings = PortalSettings.Current;

            if (menuItem == null || portalSettings == null)
            {
                return false;
            }
            if (!CheckPermissionForAdmin && PortalSecurity.IsInRole(Constants.AdminsRoleName))
            {
                return true;
            }
            //Permissions seperated by & should be treated with AND operand.
            //Permissions seperated by , are internally treated with OR operand.
            var allPermissionGroups = Permission.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            return allPermissionGroups.All(allPermissions => MenuPermissionController.HasMenuPermission(portalSettings.PortalId, menuItem, allPermissions));
        }

        private MenuItem GetMenuByIdentifier()
        {
            if (string.IsNullOrEmpty(MenuName))
            {
                return null;
            }

            return PersonaBarRepository.Instance.GetMenuItem(MenuName);
        }
    }
}
