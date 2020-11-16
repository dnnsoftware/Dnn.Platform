// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;

    public class ExceptionLogController : LogController
    {
        public enum ExceptionLogType
        {
            GENERAL_EXCEPTION,
            MODULE_LOAD_EXCEPTION,
            PAGE_LOAD_EXCEPTION,
            SCHEDULER_EXCEPTION,
            SECURITY_EXCEPTION,
            SEARCH_INDEXER_EXCEPTION,
            DATA_EXCEPTION,
        }

        public void AddLog(Exception objException)
        {
            this.AddLog(objException, ExceptionLogType.GENERAL_EXCEPTION);
        }

        public void AddLog(BasePortalException objBasePortalException)
        {
            var log = new LogInfo
            {
                Exception = Exceptions.GetExceptionInfo(objBasePortalException),
            };
            log.Exception.AssemblyVersion = objBasePortalException.AssemblyVersion;
            log.Exception.PortalId = objBasePortalException.PortalID;
            log.Exception.UserId = objBasePortalException.UserID;
            log.Exception.TabId = objBasePortalException.ActiveTabID;
            log.Exception.RawUrl = objBasePortalException.RawURL;
            log.Exception.Referrer = objBasePortalException.AbsoluteURLReferrer;
            log.Exception.UserAgent = objBasePortalException.UserAgent;
            if (objBasePortalException.GetType().Name == "ModuleLoadException")
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.MODULE_LOAD_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "PageLoadException")
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.PAGE_LOAD_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SchedulerException")
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.SCHEDULER_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SecurityException")
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.SECURITY_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SearchException")
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.SEARCH_INDEXER_EXCEPTION);
            }
            else
            {
                this.AddLog(objBasePortalException, log, ExceptionLogType.GENERAL_EXCEPTION);
            }
        }

        public void AddLog(Exception objException, ExceptionLogType logType)
        {
            var log = new LogInfo { Exception = new ExceptionInfo(objException) };
            this.AddLog(objException, log, logType);
        }

        public void AddLog(Exception objException, LogInfo log, ExceptionLogType logType)
        {
            log.LogTypeKey = logType.ToString();
            if (logType == ExceptionLogType.SEARCH_INDEXER_EXCEPTION)
            {
                // Add SearchException Properties
                var objSearchException = (SearchException)objException;
                log.LogProperties.Add(new LogDetailInfo("ModuleId", objSearchException.SearchItem.ModuleId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("SearchItemId", objSearchException.SearchItem.SearchItemId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("Title", objSearchException.SearchItem.Title));
                log.LogProperties.Add(new LogDetailInfo("SearchKey", objSearchException.SearchItem.SearchKey));
                log.LogProperties.Add(new LogDetailInfo("GUID", objSearchException.SearchItem.GUID));
            }
            else if (logType == ExceptionLogType.MODULE_LOAD_EXCEPTION)
            {
                // Add ModuleLoadException Properties
                var objModuleLoadException = (ModuleLoadException)objException;
                log.LogProperties.Add(new LogDetailInfo("ModuleId", objModuleLoadException.ModuleId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("ModuleDefId", objModuleLoadException.ModuleDefId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("FriendlyName", objModuleLoadException.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("ModuleControlSource", objModuleLoadException.ModuleControlSource));
            }
            else if (logType == ExceptionLogType.SECURITY_EXCEPTION)
            {
                // Add SecurityException Properties
                var objSecurityException = (SecurityException)objException;
                log.LogProperties.Add(new LogDetailInfo("Querystring", objSecurityException.Querystring));
                log.LogProperties.Add(new LogDetailInfo("IP", objSecurityException.IP));
            }

            // Add BasePortalException Properties
            var objBasePortalException = new BasePortalException(objException.ToString(), objException);
            log.LogProperties.Add(new LogDetailInfo("AbsoluteURL", objBasePortalException.AbsoluteURL));
            log.LogProperties.Add(new LogDetailInfo("DefaultDataProvider", objBasePortalException.DefaultDataProvider));
            log.LogProperties.Add(new LogDetailInfo("ExceptionGUID", objBasePortalException.ExceptionGUID));
            log.LogPortalID = objBasePortalException.PortalID;

            var sqlException = objException as SqlException;
            if (sqlException != null && (uint)sqlException.ErrorCode == 0x80131904 && sqlException.Number == 4060)
            {
                // This is to avoid stack-overflow exception when a database connection exception occurs
                // bercause the logger will try to write to the database and goes in a loop of failures.
                Trace.TraceError(log.Serialize());
            }
            else
            {
                Instance.AddLog(log);
            }

            // when current user is host user and exception is PageLoadException, try to log the log guid into cookies.
            // so that this log can be picked and do more action on it later.
            if (logType == ExceptionLogType.PAGE_LOAD_EXCEPTION && HttpContext.Current != null && UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                HttpContext.Current.Response.Cookies.Add(
                    new HttpCookie("LogGUID", log.LogGUID) { HttpOnly = false, Path = !string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/" });
            }
        }
    }
}
