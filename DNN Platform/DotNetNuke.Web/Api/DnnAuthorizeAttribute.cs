// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;

    public sealed class DnnAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        private static readonly List<string> DefaultAuthTypes = new List<string>();

        private static readonly string[] EmptyArray = new string[0];

        private string _staticRoles;
        private string[] _staticRolesSplit = new string[0];

        private string _denyRoles;
        private string[] _denyRolesSplit = new string[0];

        private string _authTypes;
        private string[] _authTypesSplit = new string[0];

        /// <summary>
        /// Gets or sets the authorized roles (separated by comma).
        /// </summary>
        public string StaticRoles
        {
            get { return this._staticRoles; }

            set
            {
                this._staticRoles = value;
                this._staticRolesSplit = SplitString(this._staticRoles);
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
                this._denyRolesSplit = SplitString(this._denyRoles);
            }
        }

        /// <summary>
        /// Gets or sets the allowed authentication types (separated by comma).
        /// </summary>
        public string AuthTypes
        {
            get { return this._authTypes; }

            set
            {
                this._authTypes = value;
                this._authTypesSplit = SplitString(this._authTypes);
            }
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            var identity = Thread.CurrentPrincipal.Identity;
            if (!identity.IsAuthenticated)
            {
                return false;
            }

            if (this._denyRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!currentUser.IsSuperUser && this._denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (this._staticRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!this._staticRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            // if the attribute opted in explicitly for specific authentication types, then
            // use it; otherwise use the defaults according to settings in the web.config.
            var currentAuthType = (identity.AuthenticationType ?? string.Empty).Trim();
            if (currentAuthType.Length > 0)
            {
                if (this._authTypesSplit.Any())
                {
                    return this._authTypesSplit.Contains(currentAuthType);
                }

                return DefaultAuthTypes.Contains(currentAuthType);
            }

            return true;
        }

        internal static void AppendToDefaultAuthTypes(string authType)
        {
            if (!string.IsNullOrEmpty(authType))
            {
                DefaultAuthTypes.Add(authType.Trim());
            }
        }

        private static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return EmptyArray;
            }

            var split = original.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
            return split.ToArray();
        }
    }
}
