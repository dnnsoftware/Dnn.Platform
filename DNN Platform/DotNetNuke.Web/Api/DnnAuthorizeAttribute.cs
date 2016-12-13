#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
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
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    public sealed class DnnAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        private string _staticRoles;
        private string[] _staticRolesSplit = new string[0];

        private string _denyRoles;
        private string[] _denyRolesSplit = new string[0];

        private string _authTypes;
        private string[] _authTypesSplit = new string[0];

        private static readonly List<string> DefaultAuthTypes = new List<string>();

        internal static void AppendToDefaultAuthTypes(string authType)
        {
            if (!string.IsNullOrEmpty(authType))
            {
                DefaultAuthTypes.Add(authType.Trim());
            }
        }

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

        /// <summary>
        /// Gets or sets the allowed authentication types (separated by comma)
        /// </summary>
        public string AuthTypes
        {
            get { return _authTypes; }
            set 
            {
                _authTypes = value;
                _authTypesSplit = SplitString(_authTypes);
            }
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            var identity = Thread.CurrentPrincipal.Identity;
            if(!identity.IsAuthenticated)
            {
                return false;
            }

            if(_denyRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!currentUser.IsSuperUser && _denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (_staticRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!_staticRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            // if the attribute opted in explicitly for specific authentication types, then 
            // use it; otherwise use the defaults according to settings in the web.config.
            var currentAuthType = (identity.AuthenticationType ?? "").Trim();
            if (currentAuthType.Length > 0)
            {
                if (_authTypesSplit.Any())
                {
                    return _authTypesSplit.Contains(currentAuthType);
                }

                return DefaultAuthTypes.Contains(currentAuthType);
            }

            return true;
        }

        private static readonly string[] EmptyArray = new string[0];
        private static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return EmptyArray;
            }

            var split = original.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
            return split.ToArray();
        }
    }
}