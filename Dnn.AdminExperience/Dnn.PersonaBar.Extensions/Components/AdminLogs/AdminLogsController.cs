// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.AdminLogs.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Net.Mime;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;

    public class AdminLogsController
    {
        private Dictionary<string, LogTypeInfo> _logTypeDictionary;

        private PortalSettings _portalSettings;

        protected Dictionary<string, LogTypeInfo> LogTypeDictionary
        {
            get
            {
                this._logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();
                return this._logTypeDictionary;
            }
        }

        private PortalSettings PortalSettings
        {
            get
            {
                this._portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                return this._portalSettings;
            }
        }

        public LogTypeInfo GetMyLogType(string logTypeKey)
        {
            LogTypeInfo logType;
            this.LogTypeDictionary.TryGetValue(logTypeKey, out logType);

            if (logType == null)
            {
                logType = new LogTypeInfo();
            }
            return logType;
        }

        public string GetPropertiesText(object obj)
        {
            var str = new StringBuilder();
            var objLogInfo = (LogInfo)obj;
            if (objLogInfo != null)
            {
                var objLogProperties = objLogInfo.LogProperties;
                int i;
                for (i = 0; i <= objLogProperties.Count - 1; i++)
                {
                    //display the values in the Panel child controls.
                    var ldi = (LogDetailInfo)objLogProperties[i];
                    if (ldi.PropertyName == "Message")
                    {
                        str.Append("<p><strong>" + ldi.PropertyName + "</strong>:</br><pre>" +
                                   HttpUtility.HtmlEncode(ldi.PropertyValue) + "</pre></p>");
                    }
                    else
                    {
                        str.Append("<p><strong>" + ldi.PropertyName + "</strong>:" +
                                   HttpUtility.HtmlEncode(ldi.PropertyValue) + "</p>");
                    }
                }
                if (!string.IsNullOrEmpty(objLogInfo.Exception.ExceptionHash))
                {
                    str.Append(objLogInfo.Exception);
                }
                str.Append("<p>" + Localization.GetString("ServerName", Constants.LocalResourcesFile) +
                           HttpUtility.HtmlEncode(objLogInfo.LogServerName) + "</p>");
            }
            return str.ToString();
        }

        public void ClearLog()
        {
            LogController.Instance.ClearLog();

            //add entry to log recording it was cleared
            EventLogController.Instance.AddLog(Localization.GetString("LogCleared", Constants.LocalResourcesFile),
                               Localization.GetString("Username", Constants.LocalResourcesFile) + ":" + UserController.Instance.GetCurrentUserInfo().Username,
                               this.PortalSettings,
                               -1,
                               EventLogController.EventLogType.ADMIN_ALERT);
        }

        public Dictionary<string, string> GetKeepMostRecentOptions()
        {
            Dictionary<string, string> options = new Dictionary<string, string>();
            int[] items = { 1, 2, 3, 4, 5, 10, 25, 100, 250, 500 };
            options.Add(Localization.GetString("All", Constants.LocalResourcesFile), "*");
            foreach (int item in items)
            {
                if (item == 1)
                {
                    options.Add(item + Localization.GetString("LogEntry", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    options.Add(item + Localization.GetString("LogEntries", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }
            return options;
        }

        public Dictionary<string, string> GetOccurenceThresholdOptions()
        {
            Dictionary<string, string> thresholds = new Dictionary<string, string>();
            int[] items = { 1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000 };
            foreach (int item in items)
            {
                if (item == 1)
                {
                    thresholds.Add(item + Localization.GetString("Occurence", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    thresholds.Add(item + Localization.GetString("Occurences", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }
            return thresholds;
        }

        public Dictionary<string, string> GetOccurenceThresholds()
        {
            Dictionary<string, string> thresholds = new Dictionary<string, string>();
            int[] items = { 1, 2, 3, 4, 5, 10, 25, 100, 250, 500, 1000 };
            foreach (int item in items)
            {
                if (item == 1)
                {
                    thresholds.Add(item + Localization.GetString("Occurence", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    thresholds.Add(item + Localization.GetString("Occurences", Constants.LocalResourcesFile),
                        item.ToString(CultureInfo.InvariantCulture));
                }
            }

            return thresholds;
        }

        public Dictionary<string, string> GetOccurenceThresholdNotificationTimes()
        {
            Dictionary<string, string> notificationTimes = new Dictionary<string, string>();
            foreach (int item in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 60, 90, 120 })
            {
                notificationTimes.Add(item.ToString(CultureInfo.InvariantCulture), item.ToString(CultureInfo.InvariantCulture));
            }

            return notificationTimes;
        }

        public Dictionary<string, string> GetOccurenceThresholdNotificationTimeTypes()
        {
            Dictionary<string, string> notificationTimeTypes = new Dictionary<string, string>();
            foreach (int item in new[] { 1, 2, 3, 4 })
            {
                notificationTimeTypes.Add(Localization.GetString(string.Format("TimeType_{0}", item), Constants.LocalResourcesFile), item.ToString(CultureInfo.InvariantCulture));
            }

            return notificationTimeTypes;
        }

        public void UpdateLogTypeConfig(LogTypeConfigInfo logTypeConfigInfo)
        {
            LogController.Instance.UpdateLogTypeConfigInfo(logTypeConfigInfo);
        }

        public void AddLogTypeConfig(LogTypeConfigInfo logTypeConfigInfo)
        {
            LogController.Instance.AddLogTypeConfigInfo(logTypeConfigInfo);
        }

        public void DeleteLogTypeConfig(string logTypeConfigId)
        {
            var logTypeConfigInfo = new LogTypeConfigInfo();
            logTypeConfigInfo.ID = logTypeConfigId;
            LogController.Instance.DeleteLogTypeConfigInfo(logTypeConfigInfo);
        }

        public LogTypeConfigInfo GetLogTypeConfig(string logTypeConfigId)
        {
            return LogController.Instance.GetLogTypeConfigInfoByID(logTypeConfigId);
        }

        public string EmailLogItems(string subject, string fromEmailAddress, string toEmailAddress, string message, IEnumerable<string> logItemIds, out string error)
        {
            if (string.IsNullOrEmpty(subject))
            {
                subject = this.PortalSettings.PortalName + @" Exceptions";
            }

            string returnMsg;
            if (Globals.EmailValidatorRegex.IsMatch(fromEmailAddress))
            {
                const string tempFileName = "errorlog.xml";
                var filePath = this.PortalSettings.HomeDirectoryMapPath + tempFileName;
                var xmlDoc = this.GetExceptions(logItemIds);
                xmlDoc.Save(filePath);

                var attachments = new List<Attachment>();
                var ct = new ContentType { MediaType = MediaTypeNames.Text.Xml, Name = tempFileName };

                using (var attachment = new Attachment(filePath, ct))
                {
                    attachments.Add(attachment);

                    returnMsg = Mail.SendEmail(fromEmailAddress, fromEmailAddress, toEmailAddress, subject, message, attachments);
                }

                FileSystemUtils.DeleteFile(filePath);

                if (string.IsNullOrEmpty(returnMsg))
                {
                    error = Localization.GetString("EmailSuccess", Constants.LocalResourcesFile);
                }
                else
                {
                    error = Localization.GetString("EmailFailure", Constants.LocalResourcesFile);
                }
            }
            else
            {
                returnMsg = string.Format(Localization.GetString("InavlidEmailAddress", Constants.LocalResourcesFile), fromEmailAddress);
                error = Localization.GetString("EmailFailure", Constants.LocalResourcesFile);
            }
            return returnMsg;
        }

        private XmlDocument GetExceptions(IEnumerable<string> logIds)
        {
            var objXml = new XmlDocument { XmlResolver = null };
            objXml.LoadXml("<LogEntries></LogEntries>");
            foreach (var logId in logIds)
            {
                var objLogInfo = new LogInfo { LogGUID = logId };
                var objNode = objXml.ImportNode((XmlNode)LogController.Instance.GetSingleLog(objLogInfo, LoggingProvider.ReturnType.XML), true);
                if (objXml.DocumentElement != null)
                {
                    objXml.DocumentElement.AppendChild(objNode);
                }
            }
            return objXml;
        }
    }
}
