#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

#region Usings

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
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Host
{
    public class IPFilterController : ComponentBase<IIPFilterController, IPFilterController>, IIPFilterController
    {
        #region Private

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (IPFilterController));
        
        private enum FilterType
        {
            Allow=1,
            Deny=2
        }
        #endregion

        #region Constructor

        internal IPFilterController()
        {
        }

        #endregion

        #region IIPFilterController Members

        /// <summary>
        /// add a new IP filter
        /// </summary>
        /// <param name="ipFilter">filter details</param>
        /// <returns>filter id</returns>
        public int AddIPFilter(IPFilterInfo ipFilter)
        {
            Requires.NotNull("ipFilter", ipFilter);
            AssertValidIPFilter(ipFilter);

            int id = DataProvider.Instance().AddIPFilter(ipFilter.IPAddress, ipFilter.SubnetMask, ipFilter.RuleType, UserController.Instance.GetCurrentUserInfo().UserID);
            return id;
        }

        /// <summary>
        /// update an existing IP filter
        /// </summary>
        /// <param name="ipFilter">filter details</param>
        public void UpdateIPFilter(IPFilterInfo ipFilter)
        {
            Requires.NotNull("ipFilter", ipFilter);
            AssertValidIPFilter(ipFilter);

            DataProvider.Instance().UpdateIPFilter(ipFilter.IPFilterID, ipFilter.IPAddress, ipFilter.SubnetMask, ipFilter.RuleType, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        public void DeleteIPFilter(IPFilterInfo ipFilter)
        {
            Requires.PropertyNotNegative("ipFilter", "ipFilter.IPFilterID", ipFilter.IPFilterID);
            DataProvider.Instance().DeleteIPFilter(ipFilter.IPFilterID);
        }

        /// <summary>
        /// get an IP filter 
        /// </summary>
        /// <param name="ipFilter">filter details</param>
        /// <returns>the selected IP filter</returns>
        public IPFilterInfo GetIPFilter(int ipFilter)
        {
            return CBO.FillObject<IPFilterInfo>(DataProvider.Instance().GetIPFilter(ipFilter));       
        }
    
        /// <summary>
        /// get the list of IP filters
        /// </summary>
        /// <returns>list of IP filters</returns>
        IList<IPFilterInfo> IIPFilterController.GetIPFilters()
        {
            return CBO.FillCollection<IPFilterInfo>(DataProvider.Instance().GetIPFilters());
        }

        [Obsolete("deprecated with 7.1.0 - please use IsIPBanned instead to return the value and apply your own logic. Scheduled removal in v10.0.0.")]
        public void IsIPAddressBanned(string ipAddress)
        {
            if (CheckIfBannedIPAddress(ipAddress))
            {//should throw 403.6
            throw new HttpException(403, "");
            }
        }

        /// <summary>
        /// Check the set of rules to see if an IP address is banned (used on login)
        /// </summary>
        /// <param name="ipAddress">IP address</param>
        /// <returns>true if banned</returns>
        public bool IsIPBanned(string ipAddress)
        {

            return CheckIfBannedIPAddress(ipAddress);
        }

        private bool CheckIfBannedIPAddress(string ipAddress)
        {
            IList<IPFilterInfo> filterList = Instance.GetIPFilters();
            bool ipAllowed = true;
            foreach (var ipFilterInfo in filterList)
            {
                //if a single deny exists, this win's
                if (ipFilterInfo.RuleType == (int)FilterType.Deny)
                {
                    if (NetworkUtils.IsIPInRange(ipAddress, ipFilterInfo.IPAddress, ipFilterInfo.SubnetMask))
                    {
                        //log
                        LogBannedIPAttempt(ipAddress);
                        return true;

                    }
                }
                //check any allows - if one exists set flag but let processing continue to verify no deny overrides
                if (ipFilterInfo.RuleType == (int)FilterType.Allow)
                {
                    if (ipFilterInfo.IPAddress=="*" || NetworkUtils.IsIPInRange(ipAddress, ipFilterInfo.IPAddress, ipFilterInfo.SubnetMask))
                    {
                        ipAllowed = false;

                    }
                }
            }
            return ipAllowed;
        }

        

        private void LogBannedIPAttempt(string ipAddress)
        {
            var log = new LogInfo
            {
                LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString()
            };
            log.LogProperties.Add(new LogDetailInfo("HostAddress", ipAddress));
            LogController.Instance.AddLog(log);
        }

        /// <summary>
        /// Check if an IP address range can still access based on a set of rules
        /// note: this set is typically the list of IP filter rules minus a proposed delete
        /// </summary>
        /// <param name="myip">IP address</param>
        /// <param name="filterList">list of IP filters</param>
        /// <returns>true if IP can access, false otherwise</returns>
        public bool CanIPStillAccess(string myip,IList<IPFilterInfo> filterList)
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
            //if global allow and no deny
            if (allowAllIPs & denyRules.Count==0)
            {
                return true;
            }

            //if global allow, check if a deny rule would override
            if (allowAllIPs & denyRules.Count>0)
            {
                if (denyRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return false;
                }
            }

            //if no global allow, check if a deny rule would apply
            if (allowAllIPs==false & denyRules.Count > 0)
            {
                if (denyRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return false;
                }
            }

            //if no global allow, and no deny rules check if an allow rule would apply
            if (allowAllIPs == false & denyRules.Count == 0)
            {
                if (allowRules.Any(ipf => NetworkUtils.IsIPInRange(myip, ipf.IPAddress, ipf.SubnetMask)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a new rule would block the existing IP address
        /// </summary>
        /// <param name="currentIP">current IP address</param>
        /// <param name="ipFilter">new propose rule</param>
        /// <returns>true if rule would not block current IP, false otherwise</returns>
        public bool IsAllowableDeny(string currentIP, IPFilterInfo ipFilter)
        {
            if (ipFilter.RuleType==(int)FilterType.Allow)
            {
                return true;
            }

            if (NetworkUtils.IsIPInRange(currentIP, ipFilter.IPAddress, ipFilter.SubnetMask))
            {
                return false;
            }
            return true;
        }

        #endregion

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
    }
}
