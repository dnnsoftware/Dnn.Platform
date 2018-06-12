#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
