// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Scheduling;

    public class DBLoggingProvider : LoggingProvider
    {
        public const string LogTypeCacheKey = "LogTypes";
        public const string LogTypeInfoCacheKey = "GetLogTypeConfigInfo";
        public const string LogTypeInfoByKeyCacheKey = "GetLogTypeConfigInfoByKey";

        private const int ReaderLockTimeout = 10000;
        private const int WriterLockTimeout = 10000;
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DBLoggingProvider));
        private static readonly IList<LogQueueItem> LogQueue = new List<LogQueueItem>();
        private static readonly ReaderWriterLockSlim LockNotif = new ReaderWriterLockSlim();
        private static readonly ReaderWriterLockSlim LockQueueLog = new ReaderWriterLockSlim();

        public override void AddLog(LogInfo logInfo)
        {
            string configPortalID = logInfo.LogPortalID != Null.NullInteger
                                        ? logInfo.LogPortalID.ToString()
                                        : "*";
            var logTypeConfigInfo = this.GetLogTypeConfigInfoByKey(logInfo.LogTypeKey, configPortalID);
            if (logTypeConfigInfo == null || logTypeConfigInfo.LoggingIsActive == false)
            {
                return;
            }

            logInfo.LogConfigID = logTypeConfigInfo.ID;
            var logQueueItem = new LogQueueItem { LogInfo = logInfo, LogTypeConfigInfo = logTypeConfigInfo };
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

        // ReSharper disable once InconsistentNaming
        public override void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
            DataProvider.Instance().AddLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataCache.RemoveCache(LogTypeCacheKey);
        }

        public override void AddLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive,
                                                  string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
            int intThreshold = -1;
            int intThresholdTime = -1;
            int intThresholdTimeType = -1;
            int intKeepMostRecent = -1;
            if (Globals.NumberMatchRegex.IsMatch(threshold))
            {
                intThreshold = Convert.ToInt32(threshold);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTime))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTimeType))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType);
            }

            if (Globals.NumberMatchRegex.IsMatch(keepMostRecent))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent);
            }

            DataProvider.Instance().AddLogTypeConfigInfo(
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
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
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
            DataCache.RemoveCache(LogTypeCacheKey);
        }

        public override void DeleteLogTypeConfigInfo(string id)
        {
            DataProvider.Instance().DeleteLogTypeConfigInfo(id);
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
        }

        public override List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            var logs = new List<LogInfo>();
            FillLogs(DataProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex), logs, ref totalRecords);
            return logs;
        }

        public override ArrayList GetLogTypeConfigInfo()
        {
            var list = (ArrayList)DataCache.GetCache(LogTypeInfoCacheKey);
            if (list == null)
            {
                IDataReader dr = null;
                try
                {
                    dr = DataProvider.Instance().GetLogTypeConfigInfo();
                    list = CBO.FillCollection(dr, typeof(LogTypeConfigInfo));
                    DataCache.SetCache(LogTypeInfoCacheKey, list);
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
            return CBO.FillObject<LogTypeConfigInfo>(DataProvider.Instance().GetLogTypeConfigInfoByID(Convert.ToInt32(id)));
        }

        public override ArrayList GetLogTypeInfo()
        {
            return CBO.GetCachedObject<ArrayList>(
                new CacheItemArgs(LogTypeCacheKey, 20, CacheItemPriority.Normal),
                c => CBO.FillCollection(DataProvider.Instance().GetLogTypeInfo(), typeof(LogTypeInfo)));
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

            var xmlDoc = new XmlDocument { XmlResolver = null };
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

            LogTypeConfigInfo configInfo = this.GetLogTypeConfigInfoByKey(logType, configPortalID);
            if (configInfo == null)
            {
                return false;
            }

            return configInfo.LoggingIsActive;
        }

        public override void PurgeLogBuffer()
        {
            if (!LockQueueLog.TryEnterWriteLock(WriterLockTimeout))
            {
                return;
            }

            try
            {
                for (int i = LogQueue.Count - 1; i >= 0; i += -1)
                {
                    LogQueueItem logQueueItem = LogQueue[i];

                    // in case the log was removed
                    // by another thread simultaneously
                    if (logQueueItem != null)
                    {
                        WriteLog(logQueueItem);
                        LogQueue.Remove(logQueueItem);
                    }
                }
            }
            finally
            {
                LockQueueLog.ExitWriteLock();
            }

            DataProvider.Instance().PurgeLog();
        }

        public override void SendLogNotifications()
        {
            List<LogTypeConfigInfo> configInfos = CBO.FillCollection<LogTypeConfigInfo>(DataProvider.Instance().GetEventLogPendingNotifConfig());
            foreach (LogTypeConfigInfo typeConfigInfo in configInfos)
            {
                IDataReader dr = DataProvider.Instance().GetEventLogPendingNotif(Convert.ToInt32(typeConfigInfo.ID));
                string log = string.Empty;
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

        // ReSharper disable once InconsistentNaming
        public override void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription, string logTypeCSSClass, string logTypeOwner)
        {
            DataProvider.Instance().UpdateLogType(logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeCSSClass, logTypeOwner);
            DataCache.RemoveCache(LogTypeCacheKey);
        }

        public override void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey, string logTypePortalID, string keepMostRecent, string logFileName, bool emailNotificationIsActive, string threshold, string thresholdTime, string thresholdTimeType, string mailFromAddress, string mailToAddress)
        {
            var intThreshold = -1;
            var intThresholdTime = -1;
            var intThresholdTimeType = -1;
            var intKeepMostRecent = -1;
            if (Globals.NumberMatchRegex.IsMatch(threshold))
            {
                intThreshold = Convert.ToInt32(threshold);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTime))
            {
                intThresholdTime = Convert.ToInt32(thresholdTime);
            }

            if (Globals.NumberMatchRegex.IsMatch(thresholdTimeType))
            {
                intThresholdTimeType = Convert.ToInt32(thresholdTimeType);
            }

            if (Globals.NumberMatchRegex.IsMatch(keepMostRecent))
            {
                intKeepMostRecent = Convert.ToInt32(keepMostRecent);
            }

            DataProvider.Instance().UpdateLogTypeConfigInfo(
                id,
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
            DataCache.RemoveCache(LogTypeInfoCacheKey);
            DataCache.RemoveCache(LogTypeInfoByKeyCacheKey);
        }

        private static Hashtable FillLogTypeConfigInfoByKey(ArrayList arr)
        {
            var ht = new Hashtable();
            int i;
            for (i = 0; i <= arr.Count - 1; i++)
            {
                var logTypeConfigInfo = (LogTypeConfigInfo)arr[i];
                if (string.IsNullOrEmpty(logTypeConfigInfo.LogTypeKey))
                {
                    logTypeConfigInfo.LogTypeKey = "*";
                }

                if (string.IsNullOrEmpty(logTypeConfigInfo.LogTypePortalID))
                {
                    logTypeConfigInfo.LogTypePortalID = "*";
                }

                ht.Add(logTypeConfigInfo.LogTypeKey + "|" + logTypeConfigInfo.LogTypePortalID, logTypeConfigInfo);
            }

            DataCache.SetCache(LogTypeInfoByKeyCacheKey, ht);
            return ht;
        }

        private static LogInfo FillLogInfo(IDataReader dr)
        {
            var obj = new LogInfo();
            try
            {
                obj.LogCreateDate = Convert.ToDateTime(dr["LogCreateDate"]);
                obj.LogGUID = Convert.ToString(dr["LogGUID"]);
                obj.LogPortalID = Convert.ToInt32(Null.SetNull(dr["LogPortalID"], obj.LogPortalID));
                obj.LogPortalName = Convert.ToString(Null.SetNull(dr["LogPortalName"], obj.LogPortalName));
                obj.LogServerName = Convert.ToString(Null.SetNull(dr["LogServerName"], obj.LogServerName));
                obj.LogUserID = Convert.ToInt32(Null.SetNull(dr["LogUserID"], obj.LogUserID));
                obj.LogEventID = Convert.ToInt32(Null.SetNull(dr["LogEventID"], obj.LogEventID));
                obj.LogTypeKey = Convert.ToString(dr["LogTypeKey"]);
                obj.LogUserName = Convert.ToString(dr["LogUserName"]);
                obj.LogConfigID = Convert.ToString(dr["LogConfigID"]);
                obj.LogProperties.Deserialize(Convert.ToString(dr["LogProperties"]));
                obj.Exception.AssemblyVersion = Convert.ToString(Null.SetNull(dr["AssemblyVersion"], obj.Exception.AssemblyVersion));
                obj.Exception.PortalId = Convert.ToInt32(Null.SetNull(dr["PortalId"], obj.Exception.PortalId));
                obj.Exception.UserId = Convert.ToInt32(Null.SetNull(dr["UserId"], obj.Exception.UserId));
                obj.Exception.TabId = Convert.ToInt32(Null.SetNull(dr["TabId"], obj.Exception.TabId));
                obj.Exception.RawUrl = Convert.ToString(Null.SetNull(dr["RawUrl"], obj.Exception.RawUrl));
                obj.Exception.Referrer = Convert.ToString(Null.SetNull(dr["Referrer"], obj.Exception.Referrer));
                obj.Exception.UserAgent = Convert.ToString(Null.SetNull(dr["UserAgent"], obj.Exception.UserAgent));
                obj.Exception.ExceptionHash = Convert.ToString(Null.SetNull(dr["ExceptionHash"], obj.Exception.ExceptionHash));
                obj.Exception.Message = Convert.ToString(Null.SetNull(dr["Message"], obj.Exception.Message));
                obj.Exception.StackTrace = Convert.ToString(Null.SetNull(dr["StackTrace"], obj.Exception.StackTrace));
                obj.Exception.InnerMessage = Convert.ToString(Null.SetNull(dr["InnerMessage"], obj.Exception.InnerMessage));
                obj.Exception.InnerStackTrace = Convert.ToString(Null.SetNull(dr["InnerStackTrace"], obj.Exception.InnerStackTrace));
                obj.Exception.Source = Convert.ToString(Null.SetNull(dr["Source"], obj.Exception.Source));
                /* DNN-6218 + DNN-6242: DB logging provider throws errors
                // the view "vw_EventLog" doesn't have these fields or any table in the database
                obj.Exception.FileName = Convert.ToString(Null.SetNull(dr["FileName"], obj.Exception.FileName));
                obj.Exception.FileLineNumber = Convert.ToInt32(Null.SetNull(dr["FileLineNumber"], obj.Exception.FileLineNumber));
                obj.Exception.FileColumnNumber = Convert.ToInt32(Null.SetNull(dr["FileColumnNumber"], obj.Exception.FileColumnNumber));
                obj.Exception.Method = Convert.ToString(Null.SetNull(dr["Method"], obj.Exception.Method));
                 */
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
                    DataProvider.Instance().AddLog(
                        objLogInfo.LogGUID,
                        objLogInfo.LogTypeKey,
                        objLogInfo.LogUserID,
                        objLogInfo.LogUserName,
                        objLogInfo.LogPortalID,
                        objLogInfo.LogPortalName,
                        objLogInfo.LogCreateDate,
                        objLogInfo.LogServerName,
                        logProperties,
                        Convert.ToInt32(objLogInfo.LogConfigID),
                        objLogInfo.Exception,
                        logTypeConfigInfo.EmailNotificationIsActive);
                    if (logTypeConfigInfo.EmailNotificationIsActive)
                    {
                        if (LockNotif.TryEnterWriteLock(ReaderLockTimeout))
                        {
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
                                LockNotif.ExitWriteLock();
                            }
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

        private LogTypeConfigInfo GetLogTypeConfigInfoByKey(string logTypeKey, string logTypePortalID)
        {
            var configInfoByKey = (Hashtable)DataCache.GetCache(LogTypeInfoByKeyCacheKey) ?? FillLogTypeConfigInfoByKey(this.GetLogTypeConfigInfo());
            var logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey[logTypeKey + "|" + logTypePortalID];
            if (logTypeConfigInfo == null)
            {
                logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey["*|" + logTypePortalID];
                if (logTypeConfigInfo == null)
                {
                    logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey[logTypeKey + "|*"];
                    if (logTypeConfigInfo == null)
                    {
                        logTypeConfigInfo = (LogTypeConfigInfo)configInfoByKey["*|*"];
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
    }
}
