using System.Threading;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    public class RequireHostAttribute : AuthorizeAttributeBase
    {
        /// <summary>
        /// Tests if the request passes the authorization requirements
        /// </summary>
        /// <param name="context">The auth filter context</param>
        /// <returns>True when authorization is succesful</returns>
        public override bool IsAuthorized(AuthFilterContext context)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }

            var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
            return currentUser.IsSuperUser;
        }
    }
}
