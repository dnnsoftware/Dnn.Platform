// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Host;

using System;
using System.Collections.Generic;

/// <summary>
/// Do not implement.  This interface is meant for reference and unit test purposes only.
/// There is no guarantee that this interface will not change.
/// </summary>
public interface IIPFilterController
{
    /// <summary>Adds an IP filter.</summary>
    /// <param name="ipFilter">The informations about the IP filter to add.</param>
    /// <returns>The ID of the newly created integer.</returns>
    int AddIPFilter(IPFilterInfo ipFilter);

    /// <summary>Updates an existing IP filter.</summary>
    /// <param name="ipFilter">The informations about the IP filter to update.</param>
    void UpdateIPFilter(IPFilterInfo ipFilter);

    /// <summary>Deletes an IP filter.</summary>
    /// <param name="ipFilter">The informations about the IP filter to delete.</param>
    void DeleteIPFilter(IPFilterInfo ipFilter);

    /// <summary>Gets a single IP filter.</summary>
    /// <param name="ipFilter">The ID of the IP filter to get.</param>
    /// <returns><see cref="IPFilterInfo"/>.</returns>
    IPFilterInfo GetIPFilter(int ipFilter);

    /// <summary>Gets all the IP filters.</summary>
    /// <returns>A collection of <see cref="IPFilterInfo"/>.</returns>
    IList<IPFilterInfo> GetIPFilters();

    /// <summary>Gets a value indicating whether a given IP address is banned.</summary>
    /// <param name="ipAddress">A string representation of an IP address.</param>
    [Obsolete("Deprecated in DotNetNuke 7.1.0. Please use IsIPBanned instead. Scheduled removal in v10.0.0.")]
    void IsIPAddressBanned(string ipAddress);

    /// <summary>Gets a value indicating whether a given IP address is banned.</summary>
    /// <param name="ipAddress">A string representation of an IP address.</param>
    /// <returns>A value indicating wheter the provided IP address is banned.</returns>
    bool IsIPBanned(string ipAddress);

    /// <summary>Gets a value indicating whether an IP address can be denied.</summary>
    /// <param name="ipAddress">The IP address to check.</param>
    /// <param name="ipFilter">The IP filter against which to check.</param>
    /// <returns>A value indicating whether the IP address can be denied.</returns>
    bool IsAllowableDeny(string ipAddress, IPFilterInfo ipFilter);

    /// <summary>Gets a value indicating whether an IP address has access.</summary>
    /// <param name="myip">The IP address to check.</param>
    /// <param name="filterList">A list of filters to check against.</param>
    /// <returns>A value indicating whether the IP is authorized.</returns>
    bool CanIPStillAccess(string myip, IList<IPFilterInfo> filterList);
}
