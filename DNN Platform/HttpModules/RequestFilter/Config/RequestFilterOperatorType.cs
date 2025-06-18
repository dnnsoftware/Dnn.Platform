// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.RequestFilter;

/// <summary>The operator for a filter.</summary>
public enum RequestFilterOperatorType
{
    /// <summary>Equal.</summary>
    Equal = 0,

    /// <summary>Not equal.</summary>
    NotEqual = 1,

    /// <summary>Regular expression.</summary>
    Regex = 2,
}
