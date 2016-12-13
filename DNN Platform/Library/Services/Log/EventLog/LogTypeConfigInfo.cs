#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using DotNetNuke.Common.Utilities;

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

        public string MailFromAddress { get; set; }

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