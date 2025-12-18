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

    /// <summary>Provides Dnn specific details authorization filter.</summary>
    public sealed class DnnAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        private static readonly List<string> DefaultAuthTypes = new List<string>();

        private static readonly string[] EmptyArray = new string[0];

        private string staticRoles;
        private string[] staticRolesSplit = new string[0];

        private string denyRoles;
        private string[] denyRolesSplit = new string[0];

        private string authTypes;
        private string[] authTypesSplit = new string[0];

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
                this.staticRolesSplit = SplitString(this.staticRoles);
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
                this.denyRolesSplit = SplitString(this.denyRoles);
            }
        }

        /// <summary>Gets or sets the allowed authentication types (separated by comma).</summary>
        public string AuthTypes
        {
            get
            {
                return this.authTypes;
            }

            set
            {
                this.authTypes = value;
                this.authTypesSplit = SplitString(this.authTypes);
            }
        }

        /// <inheritdoc/>
        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            var identity = Thread.CurrentPrincipal.Identity;
            if (!identity.IsAuthenticated)
            {
                return false;
            }

            if (this.denyRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!currentUser.IsSuperUser && this.denyRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            if (this.staticRolesSplit.Any())
            {
                var currentUser = PortalController.Instance.GetCurrentPortalSettings().UserInfo;
                if (!this.staticRolesSplit.Any(currentUser.IsInRole))
                {
                    return false;
                }
            }

            // if the attribute opted in explicitly for specific authentication types, then
            // use it; otherwise use the defaults according to settings in the web.config.
            var currentAuthType = (identity.AuthenticationType ?? string.Empty).Trim();
            if (currentAuthType.Length > 0)
            {
                if (this.authTypesSplit.Any())
                {
                    return this.authTypesSplit.Contains(currentAuthType);
                }

                return DefaultAuthTypes.Contains(currentAuthType);
            }

            return true;
        }

        /// <summary>Adds an authentication type to the default authentication types.</summary>
        /// <param name="authType">The name of the authentication type to add.</param>
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
