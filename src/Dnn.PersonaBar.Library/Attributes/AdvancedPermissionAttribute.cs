using System;
using System.Threading;
using Dnn.PersonaBar.Library.Model;
using Dnn.PersonaBar.Library.Permissions;
using Dnn.PersonaBar.Library.Repository;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AdvancedPermissionAttribute : AuthorizeAttributeBase
    {
        /// <summary>
        /// The menu identifier.
        /// </summary>
        public string SupportExtension { get; set; }

        /// <summary>
        /// The permission key. 
        /// </summary>
        public string Permission { get; set; }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            var menuItem = GetMenuByIdentifier();
            var portalSettings = PortalSettings.Current;

            if (menuItem == null || portalSettings == null)
            {
                return false;
            }

            return MenuPermissionController.HasMenuPermission(portalSettings.PortalId, menuItem, Permission);
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
