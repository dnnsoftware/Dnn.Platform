// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            var menuItem = this.GetMenuByIdentifier();
            var portalSettings = PortalSettings.Current;

            if (menuItem == null || portalSettings == null)
            {
                return false;
            }
            if (!this.CheckPermissionForAdmin && PortalSecurity.IsInRole(Constants.AdminsRoleName))
            {
                return true;
            }
            //Permissions seperated by & should be treated with AND operand.
            //Permissions seperated by , are internally treated with OR operand.
            var allPermissionGroups = this.Permission.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            return allPermissionGroups.All(allPermissions => MenuPermissionController.HasMenuPermission(portalSettings.PortalId, menuItem, allPermissions));
        }

        private MenuItem GetMenuByIdentifier()
        {
            if (string.IsNullOrEmpty(this.MenuName))
            {
                return null;
            }

            return PersonaBarRepository.Instance.GetMenuItem(this.MenuName);
        }
    }
}
