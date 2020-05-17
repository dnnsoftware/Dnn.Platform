// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Entities.Host
{
    /// <summary>
    /// Do not implement.  This interface is meant for reference and unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IIPFilterController
    {
        int AddIPFilter(IPFilterInfo ipFilter);

        void UpdateIPFilter(IPFilterInfo ipFilter);

        void DeleteIPFilter(IPFilterInfo ipFilter);

        IPFilterInfo GetIPFilter(int ipFilter);

        IList<IPFilterInfo> GetIPFilters();

        bool IsIPBanned(string ipAddress);

        bool IsAllowableDeny(string ipAddress, IPFilterInfo ipFilter);

        bool CanIPStillAccess(string myip, IList<IPFilterInfo> filterList);
    }
}
