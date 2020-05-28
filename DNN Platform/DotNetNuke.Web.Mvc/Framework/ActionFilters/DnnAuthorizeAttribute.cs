// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    public class DnnAuthorizeAttribute : AuthorizeAttributeBase
    {
        private string _staticRoles;
        private string[] _staticRolesSplit = new string[0];

        private string _denyRoles;
        private string[] _denyRolesSplit = new string[0];

        /// <summary>
        /// Gets or sets the authorized roles (separated by comma) 
        /// </summary>
        public string StaticRoles
        {
            get { return _staticRoles; }
            set
            {
                _staticRoles = value;
                _staticRolesSplit = SplitString(_staticRoles);
            }
        }

        /// <summary>
        /// Gets or sets the denied roles (separated by comma)
        /// </summary>
        public string DenyRoles
        {
            get { return _denyRoles; }
            set
            {
                _denyRoles = value;
                _denyRolesSplit = SplitString(_denyRoles);
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
            if (!IsAuthenticated())
            {
                return false;
            }

            if (_denyRolesSplit.Any())
            {
                var currentUser = GetCurrentUser();
                if (!currentUser.IsSuperUser && _denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (_staticRolesSplit.Any())
            {
                var currentUser = GetCurrentUser();
                if (!_staticRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            return true;
        }

        private string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            IEnumerable<string> split = from piece in original.Split(',')
                                        let trimmed = piece.Trim()
                                        where !String.IsNullOrEmpty(trimmed)
                                        select trimmed;
            return split.ToArray();
        }

    }
}
