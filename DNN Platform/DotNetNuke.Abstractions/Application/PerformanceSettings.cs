// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application;

/// <summary>Enumeration of site performance setting, say by another way that means how to set the cache.</summary>
/// <remarks>
/// <para>Using cache will speed up the application to a great degree, we recommend to use cache for whole modules,
/// but sometimes cache also make confuse for user, if we didn't take care of how to make cache expired when needed,
/// such as if a data has already been deleted but the cache aren't clear, it will cause un expected errors.
/// so you should choose a correct performance setting type when you're trying to cache some stuff, and always remember
/// update cache immediately after the data changed.</para>
/// <para>default cache policy in core api will use cache timeout multiple Host Performance setting's value as cache time(unit: minutes):</para>
/// <list type="bullet">
/// <item>HostSettingsCacheTimeOut: 20</item>
/// <item>PortalAliasCacheTimeOut: 200</item>
/// <item>PortalSettingsCacheTimeOut: 20</item>
/// <item>More cache timeout definitions see <c>DotNetNuke.Common.Utilities.DataCache</c></item>
/// </list>
/// </remarks>
public enum PerformanceSettings
{
    /// <summary>No Caching.</summary>
    NoCaching = 0,

    /// <summary>Caching for a short time.</summary>
    LightCaching = 1,

    /// <summary>Caching for moderate time.</summary>
    ModerateCaching = 3,

    /// <summary>Caching for a long time.</summary>
    HeavyCaching = 6,
}
