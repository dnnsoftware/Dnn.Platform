// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.RequestFilter;

/// <summary>The type of response for a rule.</summary>
public enum RequestFilterRuleType
{
    /// <summary>Temporary redirect.</summary>
    Redirect = 0,

    /// <summary>Permanent redirect.</summary>
    PermanentRedirect = 1,

    /// <summary>Not found response.</summary>
    NotFound = 2,
}
