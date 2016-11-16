using System;
using System.Threading;
using Dnn.PersonaBar.Library.PersonaBar.Controllers;
using Dnn.PersonaBar.Library.PersonaBar.Model;
using Dnn.PersonaBar.Library.PersonaBar.Permissions;
using Dnn.PersonaBar.Library.PersonaBar.Repository;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class MenuPermissionAttribute : AuthorizeAttributeBase
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
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
            {
                return false;
            }

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
