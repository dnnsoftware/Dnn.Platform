// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Common.Utils;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>Controller to manage IP Filters.</summary>
    public partial class IPFilterController : ComponentBase<IIPFilterController, IPFilterController>, IIPFilterController
    {
        /// <summary>Initializes a new instance of the <see cref="IPFilterController"/> class.</summary>
        internal IPFilterController()
        {
        }

        private enum FilterType
        {
            Allow = 1,
            Deny = 2,
        }

        /// <inheritdoc/>
        public int AddIPFilter(IPFilterInfo ipFilter)
        {
            Requires.NotNull("ipFilter", ipFilter);
            AssertValidIPFilter(ipFilter);

            int id = DataProvider.Instance().AddIPFilter(
                ipFilter.IPAddress,
                ipFilter.SubnetMask,
                ipFilter.RuleType,
                UserController.Instance.GetCurrentUserInfo().UserID,
                ipFilter.Notes);
            return id;
        }

        /// <inheritdoc/>
        public void UpdateIPFilter(IPFilterInfo ipFilter)
        {
            Requires.NotNull("ipFilter", ipFilter);
            AssertValidIPFilter(ipFilter);

            DataProvider.Instance().UpdateIPFilter(
                ipFilter.IPFilterID,
                ipFilter.IPAddress,
                ipFilter.SubnetMask,
                ipFilter.RuleType,
                UserController.Instance.GetCurrentUserInfo().UserID,
                ipFilter.Notes);
        }

        /// <inheritdoc/>
        public void DeleteIPFilter(IPFilterInfo ipFilter)
        {
            Requires.PropertyNotNegative("ipFilter", "ipFilter.IPFilterID", ipFilter.IPFilterID);
            DataProvider.Instance().DeleteIPFilter(ipFilter.IPFilterID);
        }

        /// <inheritdoc/>
        public IPFilterInfo GetIPFilter(int ipFilter)
        {
            return CBO.FillObject<IPFilterInfo>(DataProvider.Instance().GetIPFilter(ipFilter));
        }

        /// <inheritdoc/>
        public bool IsIPBanned(string ipAddress)
        {
            return CheckIfBannedIPAddress(ipAddress);
        }

        /// <inheritdoc/>
        public bool CanIPStillAccess(string myip, IList<IPFilterInfo> filterList)
        {
            var allowAllIPs = false;
            var globalAllow = (from p in filterList
                               where p.IPAddress == "*"
                               select p).ToList();

            if (globalAllow.Count > 0)
            {
                allowAllIPs = true;
            }

            var allowRules = (from p in filterList
                              where p.RuleType == (int)FilterType.Allow
                              select p).ToList();

            var denyRules = (from p in filterList
                             where p.RuleType == (int)FilterType.Deny
                             select p).ToList();

            // if global allow and no deny
            if (allowAllIPs & denyRules.Count == 0)
            {
                return true;
            }

            // if global allow, check if a deny rule would override
            if (allowAllIPs & denyRules.Count > 0)
            {
                if (denyRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return false;
                }
            }

            // if no global allow, check if a deny rule would apply
            if (allowAllIPs == false & denyRules.Count > 0)
            {
                if (denyRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return false;
                }
            }

            // if no global allow, and no deny rules check if an allow rule would apply
            if (allowAllIPs == false & denyRules.Count == 0)
            {
                if (allowRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public bool IsAllowableDeny(string currentIP, IPFilterInfo ipFilter)
        {
            if (ipFilter.RuleType == (int)FilterType.Allow)
            {
                return true;
            }

            if (NetworkUtils.IsIPInRange(currentIP, ipFilter.IPAddress, ipFilter.SubnetMask))
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        IList<IPFilterInfo> IIPFilterController.GetIPFilters()
        {
            return CBO.FillCollection<IPFilterInfo>(DataProvider.Instance().GetIPFilters());
        }

        private static void AssertValidIPFilter(IPFilterInfo ipFilter)
        {
            IPAddress parsed;
            if (IPAddress.TryParse(ipFilter.IPAddress, out parsed) == false)
            {
                throw new ArgumentException(Localization.GetExceptionMessage("IPAddressIncorrect", "IP address is not in correct format"));
            }

            bool isIPRange = string.IsNullOrEmpty(ipFilter.SubnetMask) == false;
            if (isIPRange && IPAddress.TryParse(ipFilter.SubnetMask, out parsed) == false)
            {
                throw new ArgumentException(Localization.GetExceptionMessage("SubnetMaskIncorrect", "Subnet mask is not in correct format"));
            }
        }

        private static bool CheckIfBannedIPAddress(string ipAddress)
        {
            IList<IPFilterInfo> filterList = Instance.GetIPFilters();
            bool ipAllowed = true;
            foreach (var ipFilterInfo in filterList)
            {
                // if a single deny exists, this win's
                if (ipFilterInfo.RuleType == (int)FilterType.Deny)
                {
                    if (NetworkUtils.IsIPInRange(ipAddress, ipFilterInfo.IPAddress, ipFilterInfo.SubnetMask))
                    {
                        // log
                        LogBannedIPAttempt(ipAddress);
                        return true;
                    }
                }

                // check any allows - if one exists set flag but let processing continue to verify no deny overrides
                if (ipFilterInfo.RuleType == (int)FilterType.Allow)
                {
                    if (ipFilterInfo.IPAddress == "*" || NetworkUtils.IsIPInRange(ipAddress, ipFilterInfo.IPAddress, ipFilterInfo.SubnetMask))
                    {
                        ipAllowed = false;
                    }
                }
            }

            return ipAllowed;
        }

        private static void LogBannedIPAttempt(string ipAddress)
        {
            var log = new LogInfo
            {
                LogTypeKey = nameof(DotNetNuke.Abstractions.Logging.EventLogType.IP_LOGIN_BANNED),
            };
            log.LogProperties.Add(new LogDetailInfo("HostAddress", ipAddress));
            LogController.Instance.AddLog(log);
        }
    }
}
