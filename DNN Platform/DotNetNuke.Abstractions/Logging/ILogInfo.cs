// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

using System;

/// <summary>The log info.</summary>
public interface ILogInfo
{
    /// <summary>Gets or sets the log GUID.</summary>
    string LogGuid { get; set; }

    /// <summary>Gets or sets the log file ID.</summary>
    string LogFileId { get; set; }

    /// <summary>Gets or sets the log type key.</summary>
    string LogTypeKey { get; set; }

    /// <summary>Gets or sets the log user id.</summary>
    int LogUserId { get; set; }

    /// <summary>Gets or sets the log event id.</summary>
    int LogEventId { get; set; }

    /// <summary>Gets or sets the log username.</summary>
    string LogUserName { get; set; }

    /// <summary>Gets or sets the log portal id.</summary>
    int LogPortalId { get; set; }

    /// <summary>Gets or sets the log portal name.</summary>
    string LogPortalName { get; set; }

    /// <summary>Gets or sets the log create date.</summary>
    DateTime LogCreateDate { get; set; }

    /// <summary>Gets or sets the log create date number.</summary>
    long LogCreateDateNum { get; set; }

    /// <summary>Gets or sets the log properties.</summary>
    ILogProperties LogProperties { get; set; }

    /// <summary>Gets or sets a value indicating whether or not the log can bypass the buffer.</summary>
    bool BypassBuffering { get; set; }

    /// <summary>Gets or sets the log server name.</summary>
    string LogServerName { get; set; }

    /// <summary>Gets or sets the log config id.</summary>
    string LogConfigId { get; set; }

    /// <summary>Gets or sets the exception info.</summary>
    IExceptionInfo Exception { get; set; }

    /// <summary>Adds a property to the log.</summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value.</param>
    void AddProperty(string name, string value);
}
