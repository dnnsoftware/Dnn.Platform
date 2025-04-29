// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Logging;

using System;

/// <summary>Log type configuration info.</summary>
public interface ILogTypeConfigInfo : ILogTypeInfo
{
    /// <summary>Gets the start <see cref="DateTime"/>.</summary>
    DateTime StartDateTime { get; }

    /// <summary>Gets or sets a value indicating whether an email notifiaction is active.</summary>
    bool EmailNotificationIsActive { get; set; }

    /// <summary>Gets or sets the email from address.</summary>
    string MailFromAddress { get; set; }

    /// <summary>Gets or sets the email to address.</summary>
    string MailToAddress { get; set; }

    /// <summary>Gets or sets the notification threshold.</summary>
    int NotificationThreshold { get; set; }

    /// <summary>Gets or sets the notification threshold time.</summary>
    int NotificationThresholdTime { get; set; }

    /// <summary>Gets or sets the notification threshold time.</summary>
    NotificationThresholdTimeType NotificationThresholdTimeType { get; set; }

    /// <summary>Gets or sets the Id.</summary>
    string Id { get; set; }

    /// <summary>Gets or sets a value indicating whether if logging is active.</summary>
    bool LoggingIsActive { get; set; }

    /// <summary>Gets or sets the log file name.</summary>
    string LogFileName { get; set; }

    /// <summary>Gets or sets the log file name with path.</summary>
    string LogFileNameWithPath { get; set; }

    /// <summary>Gets or sets the log type portal Id.</summary>
    string LogTypePortalId { get; set; }

    /// <summary>Gets or sets the keep most recent.</summary>
    string KeepMostRecent { get; set; }
}
