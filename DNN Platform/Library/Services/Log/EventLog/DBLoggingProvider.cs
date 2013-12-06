#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;

#endregion


namespace DotNetNuke.Services.Log.EventLog
{
    public class DBLoggingProvider : LoggingProvider
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (DBLoggingProvider));
        private const int ReaderLockTimeout = 10000;
        private const int WriterLockTimeout = 10000;
        private static readonly IList<LogQueueItem> LogQueue = new List<LogQueueItem>();
        private static readonly ReaderWriterLock LockNotif = new ReaderWriterLock();
        private static readonly ReaderWriterLock LockQueueLog = new ReaderWriterLock();

        private static Hashtable FillLogTypeConfigInfoByKey(ArrayList arr)
        {
            var ht = new Hashtable();
            int i;
            for (i = 0; i <= arr.Count - 1; i++)
            {
                var logTypeConfigInfo = (LogTypeConfigInfo) arr[i];
                if (String.IsNullOrEmpty(logTypeConfigInfo.LogTypeKey))
                {
                    logTypeConfigInfo.LogTypeKey = "*";
                }
                if (String.IsNullOrEmpty(logTypeConfigInfo.LogTypePortalID))
                {
                    logTypeConfigInfo.LogTypePortalID = "*";
                }
                ht.Add(logTypeConfigInfo.LogTypeKey + "|" + logTypeConfigInfo.LogTypePortalID, logTypeConfigInfo);
            }
            DataCache.SetCache("GetLogTypeConfigInfoByKey", ht);
            return ht;
        }

        private LogTypeConfigInfo GetLogTypeConfigInfoByKey(string logTypeKey, string logTypePortalID)
        {
            var configInfoByKey = (Hashtable) DataCache.GetCache("GetLogTypeConfigInfoByKey") ?? FillLogTypeConfigInfoByKey(GetLogTypeConfigInfo());
            var logTypeConfigInfo = (LogTypeConfigInfo) configInfoByKey[logTypeKey + "|" + logTypePortalID];
            if (logTypeConfigInfo == null)
            {
                logTypeConfigInfo = (LogTypeConfigInfo) configInfoByKey["*|" + logTypePortalID];
                if (logTypeConfigInfo == null)
                {
                    logTypeConfigInfo = (LogTypeConfigInfo) configInfoByKey[logTypeKey + "|*"];
                    if (logTypeConfigInfo == null)
                    {
                        logTypeConfigInfo = (LogTypeConfigInfo) configInfoByKey["*|*"];
                    }
                    else
                    {
                        return logTypeConfigInfo;
                    }
                }
                else
                {
                    return logTypeConfigInfo;
                }
            }
            else
            {
                return logTypeConfigInfo;
            }
            return logTypeConfigInfo;
        }

        private static LogInfo FillLogInfo(IDataReader dr)
        {
            var obj = new LogInfo();
            try
            {
                obj.LogCreateDate = Convert.ToDateTime(dr["LogCreateDate"]);
                obj.LogGUID = Convert.ToString(dr["LogGUID"]);
                if (dr["LogPortalID"] != DBNull.Value)
                {
                    obj.LogPortalID = Convert.ToInt32(dr["LogPortalID"]);
                }
                if (dr["LogPortalName"] != DBNull.Value)
                {
                    obj.LogPortalName = Convert.ToString(dr["LogPortalName"]);
                }
                if (dr["LogServerName"] != DBNull.Value)
                {
                    obj.LogServerName = Convert.ToString(dr["LogServerName"]);
                }
                if (dr["LogUserID"] != DBNull.Value)
                {
                    obj.LogUserID = Convert.ToInt32(dr["LogUserID"]);
                }
                obj.LogTypeKey = Convert.ToString(dr["LogTypeKey"]);
                obj.LogUserName = Convert.ToString(dr["LogUserName"]);
                obj.LogConfigID = Convert.ToString(dr["LogConfigID"]);
                obj.LogProperties.Deserialize(Convert.ToString(dr["LogProperties"]));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            return obj;
        }

        private static void FillLogs(IDataReader dr, IList logs, ref int totalRecords)
        {
            try
            {
                while (dr.Read())
                {
                    LogInfo logInfo = FillLogInfo(dr);
                    logs.Add(logInfo);
                }
                dr.NextResult();
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"]);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
        }

        private static void WriteError(LogTypeConfigInfo logTypeConfigInfo, Exception exc, string header, string message)
        {
            if (HttpContext.Current != null)
            {
                if (HttpContext.Current.IsCustomErrorEnabled)
                {
                    HttpContext.Current.AddError(exc);
                }
                else
                {
                    HttpResponse response = HttpContext.Current.Response;
                    response.StatusCode = 500;
                    HtmlUtils.WriteHeader(response, header);

                    if (logTypeConfigInfo != null)
                    {
                        HtmlUtils.WriteError(response, logTypeConfigInfo.LogFileNameWithPath, message);
                    }
                    HtmlUtils.WriteFooter(response);
                    response.End();
                }
            }
            
        }

        private static void WriteLog(LogQueueItem logQueueItem)
        {
            LogTypeConfigInfo logTypeConfigInfo = null;
            try
            {
                logTypeConfigInfo = logQueueItem.LogTypeConfigInfo;
                if (logTypeConfigInfo != null)
                {
                    LogInfo objLogInfo = logQueueItem.LogInfo;
                    string logProperties = objLogInfo.LogProperties.Serialize();
                    DataProvider.Instance().AddLog(objLogInfo.LogGUID,
                                                   objLogInfo.LogTypeKey,
                                                   objLogInfo.LogUserID,
                                                   objLogInfo.LogUserName,
                                                   objLogInfo.LogPortalID,
                                                   objLogInfo.LogPortalName,
                                                   objLogInfo.LogCreateDate,
                                                   objLogInfo.LogServerName,
                                                   logProperties,
                                                   Convert.ToInt32(objLogInfo.LogConfigID));
                    if (logTypeConfigInfo.EmailNotificationIsActive)
                    {
                        LockNotif.AcquireWriterLock(ReaderLockTimeout);
                        try
                        {
                            if (logTypeConfigInfo.NotificationThreshold == 0)
                            {
                                string str = logQueueItem.LogInfo.Serialize();
                                Mail.Mail.SendEmail(logTypeConfigInfo.MailFromAddress, logTypeConfigInfo.MailToAddress, "Event Notification", string.Format("<pre>{0}</pre>", HttpUtility.HtmlEncode(str)));
                            }
                        }
                        finally
                        {
                            LockNotif.ReleaseWriterLock();
                        }
                    }
                }
            }
            catch (SqlException exc)
            {
                Logger.Error(exc);
                WriteError(logTypeConfigInfo, exc, "SQL Exception", SqlUtils.TranslateSQLException(exc));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                WriteError(logTypeConfigInfo, exc, "Unhandled Error", exc.Message);
            }
        }

        public override void AddLog(LogInfo logInfo)
        {
            string configPortalID = logInfo.LogPortalID != Null.NullInteger 
                                        ? logInfo.LogPortalID.ToString() 
                                        : "*";
            var logTypeConfigInfo = GetLogTypeConfigInfoByKey(logInfo.LogTypeKey, configPortalID);
            if (logTypeConfigInfo == null || logTypeConfigInfo.LoggingIsActive == false)
            {
                return;
            }
            logInfo.LogConfigID = logTypeConfigInfo.ID;
            var logQueueItem = new LogQueueItem {LogInfo = logInfo, LogTypeConfigInfo = logTypeConfigInfo};
            SchedulingProvider scheduler = SchedulingProvider.Instance();
            if (scheduler == null || logInfo.BypassBuffering || SchedulingProvider.Enabled == false 
                || scheduler.GetScheduleStatus() == ScheduleStatus.STOPPED || !Host.EventLogBuffer)
            {
                WriteLog(logQueueItem);
            }
            else
            {
                LogQueue.Add(logQueueItem);
            }
        }

        public override void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
            DataProvider.Instance().AddLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        public override void AddLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive,
                                                  string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
            int intThreshold = -1;
            int intThresholdTime = -1;
            int intThresholdTimeType = -1;
            int intKeepMostRecent = -1;
            if (Regex.IsMatch(threshold, "^\\d+$"))
            {
                intThreshold = Convert.ToInt32(threshold);
            }
            if (Regex.IsMatch(thresholdTime, "^\\d+$"))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime);
            }
            if (Regex.IsMatch(thresholdTimeType, "^\\d+$"))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType);
            }
            if (Regex.IsMatch(keepMostRecent, "^\\d+$"))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent);
            }
            DataProvider.Instance().AddLogTypeConfigInfo(loggingIsActive,
                                                         logTypeKey,
                                                         logTypePortalID,
                                                         intKeepMostRecent,
                                                         emailNotificationIsActive,
                                                         intThreshold,
                                                         intThresholdTime,
                                                         intThresholdTimeType,
                                                         mailFromAddress,
                                                         mailToAddress);
            DataCache.RemoveCache("GetLogTypeConfigInfo");
            DataCache.RemoveCache("GetLogTypeConfigInfoByKey");
        }

        public override void ClearLog()
        {
            DataProvider.Instance().ClearLog();
        }

        public override void DeleteLog(LogInfo logInfo)
        {
            DataProvider.Instance().DeleteLog(logInfo.LogGUID);
        }

        public override void DeleteLogType(string logTypeKey)
        {
            DataProvider.Instance().DeleteLogType(logTypeKey);
        }

        public override void DeleteLogTypeConfigInfo(string id)
        {
            DataProvider.Instance().DeleteLogTypeConfigInfo(id);
            DataCache.RemoveCache("GetLogTypeConfigInfo");
            DataCache.RemoveCache("GetLogTypeConfigInfoByKey");
        }

        public override List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new List<LogInfo>();
            FillLogs(DataProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

        public override ArrayList GetLogTypeConfigInfo()
        {
            var list = (ArrayList) DataCache.GetCache("GetLogTypeConfigInfo");
            if (list == null)
            {
                IDataReader dr = null;
                try
                {
                    dr = DataProvider.Instance().GetLogTypeConfigInfo();
                    list = CBO.FillCollection(dr, typeof (LogTypeConfigInfo));
                    DataCache.SetCache("GetLogTypeConfigInfo", list);
                    FillLogTypeConfigInfoByKey(list);
                }
                finally
                {
                    if (dr == null)
                    {
                        list = new ArrayList();
                    }
                    else
                    {
                        CBO.CloseDataReader(dr, true);
                    }
                }
            }
            return list;
        }

        public override LogTypeConfigInfo GetLogTypeConfigInfoByID(string id)
        {
            return (LogTypeConfigInfo) CBO.FillObject(DataProvider.Instance().GetLogTypeConfigInfoByID(Convert.ToInt32(id)), typeof (LogTypeConfigInfo));
        }

        public override ArrayList GetLogTypeInfo()
        {
            return CBO.FillCollection(DataProvider.Instance().GetLogTypeInfo(), typeof (LogTypeInfo));
        }

        public override object GetSingleLog(LogInfo logInfo, ReturnType returnType)
        {
            IDataReader dr = DataProvider.Instance().GetSingleLog(logInfo.LogGUID);
            LogInfo log = null;
            try
            {
                if (dr != null)
                {
                    dr.Read();
                    log = FillLogInfo(dr);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }
            if (returnType == ReturnType.LogInfoObjects)
            {
                return log;
            }
            var xmlDoc = new XmlDocument();
            if (log != null)
            {
                xmlDoc.LoadXml(log.Serialize());
            }
            return xmlDoc.DocumentElement;
        }

        public override bool LoggingIsEnabled(string logType, int portalID)
        {
            string configPortalID = portalID.ToString();
            if (portalID == -1)
            {
                configPortalID = "*";
            }
            LogTypeConfigInfo configInfo = GetLogTypeConfigInfoByKey(logType, configPortalID);
            if (configInfo == null)
            {
                return false;
            }
            return configInfo.LoggingIsActive;
        }

        public override void PurgeLogBuffer()
        {
            LockQueueLog.AcquireWriterLock(WriterLockTimeout);
            try
            {
                for (int i = LogQueue.Count - 1; i >= 0; i += -1)
                {
                    LogQueueItem logQueueItem = LogQueue[i];
                    //in case the log was removed
                    //by another thread simultaneously
                    if (logQueueItem != null)
                    {
                        WriteLog(logQueueItem);
                        LogQueue.Remove(logQueueItem);
                    }
                }
            }
            finally
            {
                LockQueueLog.ReleaseWriterLock();
            }
            DataProvider.Instance().PurgeLog();
        }

        public override void SendLogNotifications()
        {
            List<LogTypeConfigInfo> configInfos = CBO.FillCollection<LogTypeConfigInfo>(DataProvider.Instance().GetEventLogPendingNotifConfig());
            foreach (LogTypeConfigInfo typeConfigInfo in configInfos)
            {
                IDataReader dr = DataProvider.Instance().GetEventLogPendingNotif(Convert.ToInt32(typeConfigInfo.ID));
                string log = "";
                try
                {
                    while (dr.Read())
                    {
                        LogInfo logInfo = FillLogInfo(dr);
                        log += logInfo.Serialize() + Environment.NewLine + Environment.NewLine;
                    }
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
                Mail.Mail.SendEmail(typeConfigInfo.MailFromAddress, typeConfigInfo.MailToAddress, "Event Notification", string.Format("<pre>{0}</pre>", HttpUtility.HtmlEncode(log)));
                DataProvider.Instance().UpdateEventLogPendingNotif(Convert.ToInt32(typeConfigInfo.ID));
            }
        }

        public override bool SupportsEmailNotification()
        {
            return true;
        }

        public override bool SupportsInternalViewer()
        {
            return true;
        }

        public override bool SupportsSendToCoreTeam()
        {
            return false;
        }

        public override bool SupportsSendViaEmail()
        {
            return true;
        }

        public override void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
            DataProvider.Instance().UpdateLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
        }

        public override void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
            int intThreshold = -1;
            int intThresholdTime = -1;
            int intThresholdTimeType = -1;
            int intKeepMostRecent = -1;
            if (Regex.IsMatch(threshold, "^\\d+$"))
            {
                intThreshold = Convert.ToInt32(threshold);
            }
            if (Regex.IsMatch(thresholdTime, "^\\d+$"))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime);
            }
            if (Regex.IsMatch(thresholdTimeType, "^\\d+$"))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType);
            }
            if (Regex.IsMatch(keepMostRecent, "^\\d+$"))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent);
            }
            DataProvider.Instance().UpdateLogTypeConfigInfo(id,
                                                            loggingIsActive,
                                                            logTypeKey,
                                                            logTypePortalID,
                                                            intKeepMostRecent,
                                                            emailNotificationIsActive,
                                                            intThreshold,
                                                            intThresholdTime,
                                                            intThresholdTimeType,
                                                            mailFromAddress,
                                                            mailToAddress);
            DataCache.RemoveCache("GetLogTypeConfigInfo");
            DataCache.RemoveCache("GetLogTypeConfigInfoByKey");
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog()
        {
            var logs = new LogInfoArray();
            int totalRecords = 0; 
            FillLogs(DataProvider.Instance().GetLogs(Null.NullInteger, Null.NullString, 10, 0), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(string logType)
        {
            var logs = new LogInfoArray();
            int totalRecords = 0;
            FillLogs(DataProvider.Instance().GetLogs(Null.NullInteger, logType, 10, 0), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(int portalID)
        {
            var logs = new LogInfoArray();
            int totalRecords = 0;
            FillLogs(DataProvider.Instance().GetLogs(portalID, Null.NullString, 10, 0), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(int portalID, string logType)
        {
            var logs = new LogInfoArray();
            int totalRecords = 0;
            FillLogs(DataProvider.Instance().GetLogs(portalID, logType, 10, 0), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new LogInfoArray();
            FillLogs(DataProvider.Instance().GetLogs(Null.NullInteger, Null.NullString, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new LogInfoArray();
            FillLogs(DataProvider.Instance().GetLogs(Null.NullInteger, logType, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(int portalID, int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new LogInfoArray();
            FillLogs(DataProvider.Instance().GetLogs(portalID, Null.NullString, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

        [Obsolete("Deprecated in 6.0. Replaced by GetLogs().")]
        public override LogInfoArray GetLog(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new LogInfoArray();
            FillLogs(DataProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

    }
}