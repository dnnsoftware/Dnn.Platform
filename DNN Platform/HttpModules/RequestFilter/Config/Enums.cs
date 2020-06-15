// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.RequestFilter
{
    public enum RequestFilterRuleType
    {
        Redirect,
        PermanentRedirect,
        NotFound,
    }

    public enum RequestFilterOperatorType
    {
        Equal,
        NotEqual,
        Regex,
    }
}
