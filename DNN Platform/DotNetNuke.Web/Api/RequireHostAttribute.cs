// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
