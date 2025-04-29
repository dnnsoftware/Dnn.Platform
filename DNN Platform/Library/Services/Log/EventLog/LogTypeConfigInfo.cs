// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;

using DotNetNuke.Abstractions.Logging;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;

/// <inheritdoc />
[Serializable]
public partial class LogTypeConfigInfo : LogTypeInfo, ILogTypeConfigInfo
{
    private string mailFromAddress;

    public enum NotificationThresholdTimeTypes
    {
        None = 0,
        Seconds = 1,
        Minutes = 2,
        Hours = 3,
        Days = 4,
    }

    /// <inheritdoc />
    public DateTime StartDateTime
    {
        get
        {
            switch (this.NotificationThresholdTimeType)
            {
                case NotificationThresholdTimeTypes.Seconds:
                    return DateTime.Now.AddSeconds((int)((ILogTypeConfigInfo)this).NotificationThresholdTimeType * -1);
                case NotificationThresholdTimeTypes.Minutes:
                    return DateTime.Now.AddMinutes((int)((ILogTypeConfigInfo)this).NotificationThresholdTimeType * -1);
                case NotificationThresholdTimeTypes.Hours:
                    return DateTime.Now.AddHours((int)((ILogTypeConfigInfo)this).NotificationThresholdTimeType * -1);
                case NotificationThresholdTimeTypes.Days:
                    return DateTime.Now.AddDays((int)((ILogTypeConfigInfo)this).NotificationThresholdTimeType * -1);
                default:
                    return Null.NullDate;
            }
        }
    }

    /// <inheritdoc />
    public bool EmailNotificationIsActive { get; set; }

    /// <inheritdoc />
    public string MailFromAddress
    {
        get
        {
            var portalSettings = Globals.GetPortalSettings();
            return
                string.IsNullOrWhiteSpace(this.mailFromAddress)
                    ? (portalSettings == null ? string.Empty : portalSettings.Email)
                    : this.mailFromAddress;
        }

        set
        {
            this.mailFromAddress = value;
        }
    }

    /// <inheritdoc />
    public string MailToAddress { get; set; }

    /// <inheritdoc />
    public int NotificationThreshold { get; set; }

    /// <inheritdoc />
    public int NotificationThresholdTime { get; set; }

    public NotificationThresholdTimeTypes NotificationThresholdTimeType
    {
        get => (NotificationThresholdTimeTypes)((ILogTypeConfigInfo)this).NotificationThresholdTimeType;
        set => ((ILogTypeConfigInfo)this).NotificationThresholdTimeType = (Abstractions.Logging.NotificationThresholdTimeType)value;
    }

    /// <inheritdoc />
    NotificationThresholdTimeType ILogTypeConfigInfo.NotificationThresholdTimeType { get; set; }

    /// <inheritdoc />
    string ILogTypeConfigInfo.Id { get; set; }

    /// <inheritdoc />
    public bool LoggingIsActive { get; set; }

    /// <inheritdoc />
    public string LogFileName { get; set; }

    /// <inheritdoc />
    public string LogFileNameWithPath { get; set; }

    /// <inheritdoc />
    string ILogTypeConfigInfo.LogTypePortalId { get; set; }

    /// <inheritdoc />
    public string KeepMostRecent { get; set; }
}
