// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;

    [Serializable]
    public class LogTypeConfigInfo : LogTypeInfo
    {
        private string _mailFromAddress;

        public enum NotificationThresholdTimeTypes
        {
            None = 0,
            Seconds = 1,
            Minutes = 2,
            Hours = 3,
            Days = 4,
        }

        public DateTime StartDateTime
        {
            get
            {
                switch (this.NotificationThresholdTimeType)
                {
                    case NotificationThresholdTimeTypes.Seconds:
                        return DateTime.Now.AddSeconds(this.NotificationThresholdTime * -1);
                    case NotificationThresholdTimeTypes.Minutes:
                        return DateTime.Now.AddMinutes(this.NotificationThresholdTime * -1);
                    case NotificationThresholdTimeTypes.Hours:
                        return DateTime.Now.AddHours(this.NotificationThresholdTime * -1);
                    case NotificationThresholdTimeTypes.Days:
                        return DateTime.Now.AddDays(this.NotificationThresholdTime * -1);
                    default:
                        return Null.NullDate;
                }
            }
        }

        public bool EmailNotificationIsActive { get; set; }

        public string MailFromAddress
        {
            get
            {
                var portalSettings = Globals.GetPortalSettings();
                return
                    string.IsNullOrWhiteSpace(this._mailFromAddress)
                    ? (portalSettings == null ? string.Empty : portalSettings.Email)
                    : this._mailFromAddress;
            }

            set { this._mailFromAddress = value; }
        }

        public string MailToAddress { get; set; }

        public int NotificationThreshold { get; set; }

        public int NotificationThresholdTime { get; set; }

        public NotificationThresholdTimeTypes NotificationThresholdTimeType { get; set; }

        public string ID { get; set; }

        public bool LoggingIsActive { get; set; }

        public string LogFileName { get; set; }

        public string LogFileNameWithPath { get; set; }

        public string LogTypePortalID { get; set; }

        public string KeepMostRecent { get; set; }
    }
}
