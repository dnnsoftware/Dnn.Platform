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
        private string _staticRoles;
        private string[] _staticRolesSplit = new string[0];

        private string _denyRoles;
        private string[] _denyRolesSplit = new string[0];

        /// <summary>
        /// Gets or sets the authorized roles (separated by comma).
        /// </summary>
        public string StaticRoles
        {
            get { return this._staticRoles; }

            set
            {
                this._staticRoles = value;
                this._staticRolesSplit = this.SplitString(this._staticRoles);
            }
        }

        /// <summary>
        /// Gets or sets the denied roles (separated by comma).
        /// </summary>
        public string DenyRoles
        {
            get { return this._denyRoles; }

            set
            {
                this._denyRoles = value;
                this._denyRolesSplit = this.SplitString(this._denyRoles);
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

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!this.IsAuthenticated())
            {
                return false;
            }

            if (this._denyRolesSplit.Any())
            {
                var currentUser = this.GetCurrentUser();
                if (!currentUser.IsSuperUser && this._denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (this._staticRolesSplit.Any())
            {
                var currentUser = this.GetCurrentUser();
                if (!this._staticRolesSplit.Any(currentUser.IsInRole))
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
