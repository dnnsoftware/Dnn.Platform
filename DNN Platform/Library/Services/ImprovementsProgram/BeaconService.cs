// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.ImprovementsProgram
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Web;

    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;

    public class BeaconService : ServiceLocator<IBeaconService, BeaconService>, IBeaconService
    {
        private static readonly bool IsAlphaMode = DotNetNukeContext.Current?.Application?.Status == ReleaseMode.Alpha;

        private string _beaconEndpoint;

        public string GetBeaconEndpoint()
        {
            if (string.IsNullOrEmpty(this._beaconEndpoint))
            {
                var ep = ConfigurationManager.AppSettings["ImprovementProgram.Endpoint"];
#if DEBUG
                this._beaconEndpoint = string.IsNullOrEmpty(ep)
                    ? "https://dev.dnnapi.com/beacon"
                    : ep;
#else
                _beaconEndpoint = string.IsNullOrEmpty(ep)
                    ? "https://dnnapi.com/beacon"
                    : ep;
#endif
            }

            return this._beaconEndpoint;
        }

        public bool IsBeaconEnabledForControlBar(UserInfo user)
        {
            // check for Update Service Opt-in
            // check if a host or admin
            // check if currently on a host/admin page
            var enabled = false;

            if (Host.ParticipateInImprovementProg && !IsAlphaMode)
            {
                var roles = GetUserRolesBitValues(user);
                var tabPath = TabController.CurrentPage.TabPath;
                enabled = (roles & (RolesEnum.Host | RolesEnum.Admin)) != 0 &&
                          (tabPath.StartsWith("//Admin") || tabPath.StartsWith("//Host"));
            }

            return enabled;
        }

        public bool IsBeaconEnabledForPersonaBar()
        {
            return Host.ParticipateInImprovementProg && !IsAlphaMode;
        }

        public string GetBeaconQuery(UserInfo user, string filePath = null)
        {
            var roles = 0;
            if (user.UserID >= 0)
            {
                if (user.IsSuperUser)
                {
                    roles |= (int)RolesEnum.Host;
                }

                if (user.IsInRole("Administrators"))
                {
                    roles |= (int)RolesEnum.Admin;
                }

                if (user.IsInRole("Content Managers"))
                {
                    roles |= (int)RolesEnum.ContentManager;
                }

                if (user.IsInRole("Content Editors"))
                {
                    roles |= (int)RolesEnum.ContentEditor;
                }

                if (user.IsInRole("Community Manager"))
                {
                    roles |= (int)RolesEnum.CommunityManager;
                }
            }

            // h: Host GUID - hashed
            // p: Portal GUID - hashed
            // a: Portal Alias - hashed
            // r: Role(s) - bitmask - see RolesEnum
            // u: User ID - hashed
            // f: page name / tab path
            // n: Product Edition - hashed
            // v: Version - hashed
            var uid = user.UserID.ToString("D") + user.CreatedOnDate.ToString("O");
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var qparams = new Dictionary<string, string>
            {
                // Remember to URL ENCODE values that can be ambigious
                { "h", HttpUtility.UrlEncode(this.GetHash(Host.GUID)) },
                { "p", HttpUtility.UrlEncode(this.GetHash(portalSettings.GUID.ToString())) },
                { "a", HttpUtility.UrlEncode(this.GetHash(portalSettings.PortalAlias.HTTPAlias)) },
                { "u", HttpUtility.UrlEncode(this.GetHash(uid)) },
                { "r", roles.ToString("D") },
            };

            if (!string.IsNullOrEmpty(filePath))
            {
                qparams["f"] = HttpUtility.UrlEncode(filePath);
            }

            // add package and version to context of request
            string packageName = DotNetNukeContext.Current.Application.Name;
            string installVersion = Common.Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, "00", 3, string.Empty);
            if (!string.IsNullOrEmpty(packageName))
            {
                qparams["n"] = HttpUtility.UrlEncode(this.GetHash(packageName));
            }

            if (!string.IsNullOrEmpty(installVersion))
            {
                qparams["v"] = HttpUtility.UrlEncode(this.GetHash(installVersion));
            }

            return "?" + string.Join("&", qparams.Select(kpv => kpv.Key + "=" + kpv.Value));
        }

        public string GetBeaconUrl(UserInfo user, string filePath = null)
        {
            return this.GetBeaconEndpoint() + this.GetBeaconQuery(user, filePath);
        }

        protected override Func<IBeaconService> GetFactory()
        {
            return () => new BeaconService();
        }

        private static RolesEnum GetUserRolesBitValues(UserInfo user)
        {
            var roles = RolesEnum.None;
            if (user.UserID >= 0)
            {
                if (user.IsSuperUser)
                {
                    roles |= RolesEnum.Host;
                }

                if (user.IsInRole("Administrators"))
                {
                    roles |= RolesEnum.Admin;
                }

                if (user.IsInRole("Content Managers"))
                {
                    roles |= RolesEnum.ContentManager;
                }

                if (user.IsInRole("Content Editors"))
                {
                    roles |= RolesEnum.ContentEditor;
                }

                if (user.IsInRole("Community Manager"))
                {
                    roles |= RolesEnum.CommunityManager;
                }
            }

            return roles;
        }

        private string GetHash(string data)
        {
            using (var sha256 = CryptographyUtils.CreateSHA256())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
