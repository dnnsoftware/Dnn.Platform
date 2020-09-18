// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;

    public partial class LogController : ServiceLocator<ILogController, LogController>, ILogController
    {
        private const int WriterLockTimeout = 10000; // milliseconds
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LogController));
        private static readonly ReaderWriterLockSlim LockLog = new ReaderWriterLockSlim();

        public void AddLog(LogInfo logInfo)
        {
            if (Globals.Status == Globals.UpgradeStatus.Install)
            {
                Logger.Info(logInfo);
            }
            else
            {
                try
                {
                    logInfo.LogCreateDate = DateTime.Now;
                    logInfo.LogServerName = Globals.ServerName;
                    if (string.IsNullOrEmpty(logInfo.LogServerName))
                    {
                        logInfo.LogServerName = "NA";
                    }

                    if (string.IsNullOrEmpty(logInfo.LogUserName))
                    {
                        if (HttpContext.Current != null)
                        {
                            if (HttpContext.Current.Request.IsAuthenticated)
                            {
                                logInfo.LogUserName = UserController.Instance.GetCurrentUserInfo().Username;
                            }
                        }
                    }

                    // Get portal name if name isn't set
                    if (logInfo.LogPortalID != Null.NullInteger && string.IsNullOrEmpty(logInfo.LogPortalName))
                    {
                        logInfo.LogPortalName = PortalController.Instance.GetPortal(logInfo.LogPortalID).PortalName;
                    }

                    // Check if Log Type exists
                    if (!this.GetLogTypeInfoDictionary().ContainsKey(logInfo.LogTypeKey))
                    {
                        // Add new Log Type
                        var logType = new LogTypeInfo()
                        {
                            LogTypeKey = logInfo.LogTypeKey,
                            LogTypeFriendlyName = logInfo.LogTypeKey,
                            LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                            LogTypeCSSClass = "GeneralAdminOperation",
                            LogTypeDescription = string.Empty,
                        };
                        this.AddLogType(logType);

                        var logTypeConfigInfo = new LogTypeConfigInfo()
                        {
                            LogTypeKey = logInfo.LogTypeKey,
                            LogTypePortalID = "*",
                            LoggingIsActive = false,
                            KeepMostRecent = "-1",
                            EmailNotificationIsActive = false,
                            NotificationThreshold = 1,
                            NotificationThresholdTime = 1,
                            NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                            MailFromAddress = string.Empty,
                            MailToAddress = string.Empty,
                        };
                        this.AddLogTypeConfigInfo(logTypeConfigInfo);
                    }

                    if (LoggingProvider.Instance() != null)
                    {
                        try
                        {
                            LoggingProvider.Instance().AddLog(logInfo);
                        }
                        catch (Exception)
                        {
                            if (Globals.Status != Globals.UpgradeStatus.Upgrade) // this may caught exception during upgrade because old logging provider has problem in it.
                            {
                                throw;
                            }
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);

                    AddLogToFile(logInfo);
                }
            }
        }

        public void AddLogType(string configFile, string fallbackConfigFile)
        {
            var xmlDoc = new XmlDocument { XmlResolver = null };
            try
            {
                xmlDoc.Load(configFile);
            }
            catch (FileNotFoundException exc)
            {
                Logger.Debug(exc);
                xmlDoc.Load(fallbackConfigFile);
            }

            var logType = xmlDoc.SelectNodes("/LogConfig/LogTypes/LogType");
            if (logType != null)
            {
                foreach (XmlNode typeInfo in logType)
                {
                    if (typeInfo.Attributes != null)
                    {
                        var objLogTypeInfo = new LogTypeInfo
                        {
                            LogTypeKey = typeInfo.Attributes["LogTypeKey"].Value,
                            LogTypeFriendlyName = typeInfo.Attributes["LogTypeFriendlyName"].Value,
                            LogTypeDescription = typeInfo.Attributes["LogTypeDescription"].Value,
                            LogTypeCSSClass = typeInfo.Attributes["LogTypeCSSClass"].Value,
                            LogTypeOwner = typeInfo.Attributes["LogTypeOwner"].Value,
                        };
                        this.AddLogType(objLogTypeInfo);
                    }
                }
            }

            var logTypeConfig = xmlDoc.SelectNodes("/LogConfig/LogTypeConfig");
            if (logTypeConfig != null)
            {
                foreach (XmlNode typeConfigInfo in logTypeConfig)
                {
                    if (typeConfigInfo.Attributes != null)
                    {
                        var logTypeConfigInfo = new LogTypeConfigInfo
                        {
                            EmailNotificationIsActive = typeConfigInfo.Attributes["EmailNotificationStatus"].Value == "On",
                            KeepMostRecent = typeConfigInfo.Attributes["KeepMostRecent"].Value,
                            LoggingIsActive = typeConfigInfo.Attributes["LoggingStatus"].Value == "On",
                            LogTypeKey = typeConfigInfo.Attributes["LogTypeKey"].Value,
                            LogTypePortalID = typeConfigInfo.Attributes["LogTypePortalID"].Value,
                            MailFromAddress = typeConfigInfo.Attributes["MailFromAddress"].Value,
                            MailToAddress = typeConfigInfo.Attributes["MailToAddress"].Value,
                            NotificationThreshold = Convert.ToInt32(typeConfigInfo.Attributes["NotificationThreshold"].Value),
                            NotificationThresholdTime = Convert.ToInt32(typeConfigInfo.Attributes["NotificationThresholdTime"].Value),
                            NotificationThresholdTimeType =
                                                            (LogTypeConfigInfo.NotificationThresholdTimeTypes)Enum.Parse(typeof(LogTypeConfigInfo.NotificationThresholdTimeTypes), typeConfigInfo.Attributes["NotificationThresholdTimeType"].Value),
                        };
                        this.AddLogTypeConfigInfo(logTypeConfigInfo);
                    }
                }
            }
        }

        public void AddLogType(LogTypeInfo logType)
        {
            LoggingProvider.Instance().AddLogType(logType.LogTypeKey, logType.LogTypeFriendlyName, logType.LogTypeDescription, logType.LogTypeCSSClass, logType.LogTypeOwner);
        }

        public void AddLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LoggingProvider.Instance().AddLogTypeConfigInfo(
                logTypeConfig.ID,
                logTypeConfig.LoggingIsActive,
                logTypeConfig.LogTypeKey,
                logTypeConfig.LogTypePortalID,
                logTypeConfig.KeepMostRecent,
                logTypeConfig.LogFileName,
                logTypeConfig.EmailNotificationIsActive,
                Convert.ToString(logTypeConfig.NotificationThreshold),
                Convert.ToString(logTypeConfig.NotificationThresholdTime),
                Convert.ToString((int)logTypeConfig.NotificationThresholdTimeType),
                logTypeConfig.MailFromAddress,
                logTypeConfig.MailToAddress);
        }

        public void ClearLog()
        {
            LoggingProvider.Instance().ClearLog();
        }

        public void DeleteLog(LogInfo logInfo)
        {
            LoggingProvider.Instance().DeleteLog(logInfo);
        }

        public virtual void DeleteLogType(LogTypeInfo logType)
        {
            LoggingProvider.Instance().DeleteLogType(logType.LogTypeKey);
        }

        public virtual void DeleteLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LoggingProvider.Instance().DeleteLogTypeConfigInfo(logTypeConfig.ID);
        }

        public virtual List<LogInfo> GetLogs(int portalID, string logType, int pageSize, int pageIndex, ref int totalRecords)
        {
            return LoggingProvider.Instance().GetLogs(portalID, logType, pageSize, pageIndex, ref totalRecords);
        }

        public virtual ArrayList GetLogTypeConfigInfo()
        {
            return LoggingProvider.Instance().GetLogTypeConfigInfo();
        }

        public virtual LogTypeConfigInfo GetLogTypeConfigInfoByID(string id)
        {
            return LoggingProvider.Instance().GetLogTypeConfigInfoByID(id);
        }

        public virtual Dictionary<string, LogTypeInfo> GetLogTypeInfoDictionary()
        {
            return LoggingProvider.Instance().GetLogTypeInfo().Cast<LogTypeInfo>().ToDictionary(logTypeInfo => logTypeInfo.LogTypeKey);
        }

        public virtual object GetSingleLog(LogInfo log, LoggingProvider.ReturnType returnType)
        {
            return LoggingProvider.Instance().GetSingleLog(log, returnType);
        }

        public void PurgeLogBuffer()
        {
            LoggingProvider.Instance().PurgeLogBuffer();
        }

        public virtual void UpdateLogTypeConfigInfo(LogTypeConfigInfo logTypeConfig)
        {
            LoggingProvider.Instance().UpdateLogTypeConfigInfo(
                logTypeConfig.ID,
                logTypeConfig.LoggingIsActive,
                logTypeConfig.LogTypeKey,
                logTypeConfig.LogTypePortalID,
                logTypeConfig.KeepMostRecent,
                logTypeConfig.LogFileName,
                logTypeConfig.EmailNotificationIsActive,
                Convert.ToString(logTypeConfig.NotificationThreshold),
                Convert.ToString(logTypeConfig.NotificationThresholdTime),
                Convert.ToString((int)logTypeConfig.NotificationThresholdTimeType),
                logTypeConfig.MailFromAddress,
                logTypeConfig.MailToAddress);
        }

        public virtual void UpdateLogType(LogTypeInfo logType)
        {
            LoggingProvider.Instance().UpdateLogType(logType.LogTypeKey, logType.LogTypeFriendlyName, logType.LogTypeDescription, logType.LogTypeCSSClass, logType.LogTypeOwner);
        }

        protected override Func<ILogController> GetFactory()
        {
            return () => new LogController();
        }

        private static void AddLogToFile(LogInfo logInfo)
        {
            try
            {
                var f = Globals.HostMapPath + "\\Logs\\LogFailures.xml.resources";
                WriteLog(f, logInfo.Serialize());
            }

            // ReSharper disable EmptyGeneralCatchClause
            catch (Exception exc) // ReSharper restore EmptyGeneralCatchClause
            {
                Logger.Error(exc);
            }
        }

        private static void RaiseError(string filePath, string header, string message)
        {
            Logger.ErrorFormat("filePath={0}, header={1}, message={2}", filePath, header, message);

            if (HttpContext.Current != null)
            {
                HttpResponse response = HttpContext.Current.Response;
                HtmlUtils.WriteHeader(response, header);
                HtmlUtils.WriteError(response, filePath, message);
                HtmlUtils.WriteFooter(response);
                response.End();
            }
        }

        private static void WriteToStreamWriter(FileStream fs, string message)
        {
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                var fileLength = fs.Length;
                if (fileLength > 0)
                {
                    fs.Position = fileLength - 9;
                }
                else
                {
                    message = "<logs>" + message;
                }

                sw.WriteLine(message + "</logs>");
                sw.Flush();
            }
        }

        private static void WriteLog(string filePath, string message)
        {
            FileStream fs = null;
            if (!LockLog.TryEnterWriteLock(WriterLockTimeout))
            {
                return;
            }

            try
            {
                var intAttempts = 0;
                while (fs == null && intAttempts < 100)
                {
                    intAttempts += 1;
                    try
                    {
                        fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    }
                    catch (IOException exc)
                    {
                        Logger.Debug(exc);
                        Thread.Sleep(1);
                    }
                }

                if (fs == null)
                {
                    if (HttpContext.Current != null)
                    {
                        HttpContext.Current.Response.Write("An error has occurred writing to the exception log.");
                        HttpContext.Current.Response.End();
                    }
                }
                else
                {
                    WriteToStreamWriter(fs, message);
                }
            }
            catch (UnauthorizedAccessException)
            {
                RaiseError(filePath, "Unauthorized Access Error", "The Windows User Account listed below must have Read/Write Privileges for the website path.");
            }
            catch (DirectoryNotFoundException exc)
            {
                RaiseError(filePath, "Directory Not Found Error", exc.Message);
            }
            catch (PathTooLongException exc)
            {
                RaiseError(filePath, "Path Too Long Error", exc.Message);
            }
            catch (IOException exc)
            {
                RaiseError(filePath, "IO Error", exc.Message);
            }
            catch (SqlException exc)
            {
                RaiseError(filePath, "SQL Exception", SqlUtils.TranslateSQLException(exc));
            }
            catch (Exception exc)
            {
                RaiseError(filePath, "Unhandled Error", exc.Message);
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }

                LockLog.ExitWriteLock();
            }
        }
    }
}
