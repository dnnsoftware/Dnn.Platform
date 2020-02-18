// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.HttpModules.RequestFilter
{
    public enum RequestFilterRuleType
    {
        Redirect,
        PermanentRedirect,
        NotFound
    }

    public enum RequestFilterOperatorType
    {
        Equal,
        NotEqual,
        Regex
    }
}
