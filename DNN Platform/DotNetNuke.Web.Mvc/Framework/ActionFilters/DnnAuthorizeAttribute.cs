// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Web;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;

    public class DnnAuthorizeAttribute : AuthorizeAttributeBase
    {
        private string staticRoles;
        private string[] staticRolesSplit = new string[0];

        private string denyRoles;
        private string[] denyRolesSplit = new string[0];

        /// <summary>Gets or sets the authorized roles (separated by comma).</summary>
        public string StaticRoles
        {
            get
            {
                return this.staticRoles;
            }

            set
            {
                this.staticRoles = value;
                this.staticRolesSplit = this.SplitString(this.staticRoles);
            }
        }

        /// <summary>Gets or sets the denied roles (separated by comma).</summary>
        public string DenyRoles
        {
            get
            {
                return this.denyRoles;
            }

            set
            {
                this.denyRoles = value;
                this.denyRolesSplit = this.SplitString(this.denyRoles);
            }
        }

        protected virtual bool IsAuthenticated()
        {
            return Thread.CurrentPrincipal.Identity.IsAuthenticated;
        }

        protected virtual UserInfo GetCurrentUser()
        {
            return PortalController.Instance.GetCurrentPortalSettings().UserInfo;
        }

        /// <inheritdoc/>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!this.IsAuthenticated())
            {
                return false;
            }

            if (this.denyRolesSplit.Any())
            {
                var currentUser = this.GetCurrentUser();
                if (!currentUser.IsSuperUser && this.denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (this.staticRolesSplit.Any())
            {
                var currentUser = this.GetCurrentUser();
                if (!this.staticRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            return true;
        }

        private string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            IEnumerable<string> split = from piece in original.Split(',')
                                        let trimmed = piece.Trim()
                                        where !string.IsNullOrEmpty(trimmed)
                                        select trimmed;
            return split.ToArray();
        }
    }
}
