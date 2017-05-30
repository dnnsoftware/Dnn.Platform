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

        public bool ForceAdminPermission { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var menuItem = GetMenuByIdentifier();
            var portalSettings = PortalSettings.Current;

            if (menuItem == null || portalSettings == null)
            {
                return false;
            }
            if (ForceAdminPermission && PortalSecurity.IsInRole(Constants.AdminsRoleName))
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
