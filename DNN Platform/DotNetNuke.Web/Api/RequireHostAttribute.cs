// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Threading;

    using DotNetNuke.Entities.Portals;

    public class RequireHostAttribute : AuthorizeAttributeBase
    {
        /// <summary>
        /// Tests if the request passes the authorization requirements.
        /// </summary>
        /// <param name="context">The auth filter context.</param>
        /// <returns>True when authorization is succesful.</returns>
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
