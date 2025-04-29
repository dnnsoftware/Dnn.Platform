// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;

using DotNetNuke.Abstractions.Logging;

/// <inheritdoc />
[Serializable]
public partial class LogTypeInfo : ILogTypeInfo
{
    /// <inheritdoc />
    string ILogTypeInfo.LogTypeCssClass { get; set; }

    /// <inheritdoc />
    public string LogTypeDescription { get; set; }

    /// <inheritdoc />
    public string LogTypeFriendlyName { get; set; }

    /// <inheritdoc />
    public string LogTypeKey { get; set; }

    /// <inheritdoc />
    public string LogTypeOwner { get; set; }
}
