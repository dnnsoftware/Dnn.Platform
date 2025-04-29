// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.RequestFilter;

public enum RequestFilterRuleType
{
    Redirect = 0,
    PermanentRedirect = 1,
    NotFound = 2,
}

public enum RequestFilterOperatorType
{
    Equal = 0,
    NotEqual = 1,
    Regex = 2,
}
