// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    [Serializable]
    public class LogTypeConfigInfo : LogTypeInfo
    {
        #region NotificationThresholdTimeTypes enum

        public enum NotificationThresholdTimeTypes
        {
            None = 0,
            Seconds = 1,
            Minutes = 2,
            Hours = 3,
            Days = 4
        }

        #endregion

        private string _mailFromAddress;

        public DateTime StartDateTime
        {
            get
            {
                switch (NotificationThresholdTimeType)
                {
                    case NotificationThresholdTimeTypes.Seconds:
                        return DateTime.Now.AddSeconds(NotificationThresholdTime*-1);
                    case NotificationThresholdTimeTypes.Minutes:
                        return DateTime.Now.AddMinutes(NotificationThresholdTime*-1);
                    case NotificationThresholdTimeTypes.Hours:
                        return DateTime.Now.AddHours(NotificationThresholdTime*-1);
                    case NotificationThresholdTimeTypes.Days:
                        return DateTime.Now.AddDays(NotificationThresholdTime*-1);
                    default:
                        return Null.NullDate;
                }
            }
        }

        public bool EmailNotificationIsActive { get; set; }

        public string MailFromAddress
        {
            get {                
                var portalSettings = Globals.GetPortalSettings();
                return 
                    string.IsNullOrWhiteSpace(_mailFromAddress) 
                    ? (portalSettings == null ? string.Empty : portalSettings.Email) 
                    : _mailFromAddress; }
            set { _mailFromAddress = value; }
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
