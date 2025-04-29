// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

/// <summary>The log details info.</summary>
public interface ILogDetailInfo
{
    /// <summary>Gets or sets the property name.</summary>
    string PropertyName { get; set; }

    /// <summary>Gets or sets the property value.</summary>
    string PropertyValue { get; set; }
}
