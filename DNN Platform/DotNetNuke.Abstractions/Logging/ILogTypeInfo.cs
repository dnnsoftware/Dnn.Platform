// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

/// <summary>Log type info.</summary>
public interface ILogTypeInfo
{
    /// <summary>Gets or sets log type css class.</summary>
    string LogTypeCssClass { get; set; }

    /// <summary>Gets or sets the log type description.</summary>
    string LogTypeDescription { get; set; }

    /// <summary>Gets or sets the log type friendly name.</summary>
    string LogTypeFriendlyName { get; set; }

    /// <summary>Gets or sets log type key.</summary>
    string LogTypeKey { get; set; }

    /// <summary>Gets or sets the log type owner.</summary>
    string LogTypeOwner { get; set; }
}
