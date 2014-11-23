#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Web;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Utilities;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    public class ExceptionLogController : LogController
    {
        #region ExceptionLogType enum

        public enum ExceptionLogType
        {
            GENERAL_EXCEPTION,
            MODULE_LOAD_EXCEPTION,
            PAGE_LOAD_EXCEPTION,
            SCHEDULER_EXCEPTION,
            SECURITY_EXCEPTION,
            SEARCH_INDEXER_EXCEPTION,
            DATA_EXCEPTION
        }

        #endregion

        public void AddLog(Exception objException)
        {
            AddLog(objException, ExceptionLogType.GENERAL_EXCEPTION);
        }

        public void AddLog(BasePortalException objBasePortalException)
        {
            if (objBasePortalException.GetType().Name == "ModuleLoadException")
            {
                AddLog(objBasePortalException, ExceptionLogType.MODULE_LOAD_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "PageLoadException")
            {
                AddLog(objBasePortalException, ExceptionLogType.PAGE_LOAD_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SchedulerException")
            {
                AddLog(objBasePortalException, ExceptionLogType.SCHEDULER_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SecurityException")
            {
                AddLog(objBasePortalException, ExceptionLogType.SECURITY_EXCEPTION);
            }
            else if (objBasePortalException.GetType().Name == "SearchException")
            {
                AddLog(objBasePortalException, ExceptionLogType.SEARCH_INDEXER_EXCEPTION);
            }
            else
            {
                AddLog(objBasePortalException, ExceptionLogType.GENERAL_EXCEPTION);
            }
        }

        public void AddLog(Exception objException, ExceptionLogType logType)
        {
            var log = new LogInfo {LogTypeKey = logType.ToString()};
            if (logType == ExceptionLogType.SEARCH_INDEXER_EXCEPTION)
            {
				//Add SearchException Properties
                var objSearchException = (SearchException) objException;
                log.LogProperties.Add(new LogDetailInfo("ModuleId", objSearchException.SearchItem.ModuleId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("SearchItemId", objSearchException.SearchItem.SearchItemId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("Title", objSearchException.SearchItem.Title));
                log.LogProperties.Add(new LogDetailInfo("SearchKey", objSearchException.SearchItem.SearchKey));
                log.LogProperties.Add(new LogDetailInfo("GUID", objSearchException.SearchItem.GUID));
            }
            else if (logType == ExceptionLogType.MODULE_LOAD_EXCEPTION)
            {
				//Add ModuleLoadException Properties
                var objModuleLoadException = (ModuleLoadException) objException;
                log.LogProperties.Add(new LogDetailInfo("ModuleId", objModuleLoadException.ModuleId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("ModuleDefId", objModuleLoadException.ModuleDefId.ToString()));
                log.LogProperties.Add(new LogDetailInfo("FriendlyName", objModuleLoadException.FriendlyName));
                log.LogProperties.Add(new LogDetailInfo("ModuleControlSource", objModuleLoadException.ModuleControlSource));
            }
            else if (logType == ExceptionLogType.SECURITY_EXCEPTION)
            {
				//Add SecurityException Properties
                var objSecurityException = (SecurityException) objException;
                log.LogProperties.Add(new LogDetailInfo("Querystring", objSecurityException.Querystring));
                log.LogProperties.Add(new LogDetailInfo("IP", objSecurityException.IP));
            }
			
			//Add BasePortalException Properties
            var objBasePortalException = new BasePortalException(objException.ToString(), objException);
            log.LogProperties.Add(new LogDetailInfo("AssemblyVersion", objBasePortalException.AssemblyVersion));
            log.LogProperties.Add(new LogDetailInfo("PortalID", objBasePortalException.PortalID.ToString()));
            log.LogProperties.Add(new LogDetailInfo("PortalName", objBasePortalException.PortalName));
            log.LogProperties.Add(new LogDetailInfo("UserID", objBasePortalException.UserID.ToString()));
            log.LogProperties.Add(new LogDetailInfo("UserName", objBasePortalException.UserName));
            log.LogProperties.Add(new LogDetailInfo("ActiveTabID", objBasePortalException.ActiveTabID.ToString()));
            log.LogProperties.Add(new LogDetailInfo("ActiveTabName", objBasePortalException.ActiveTabName));
            log.LogProperties.Add(new LogDetailInfo("RawURL", objBasePortalException.RawURL));
            log.LogProperties.Add(new LogDetailInfo("AbsoluteURL", objBasePortalException.AbsoluteURL));
            log.LogProperties.Add(new LogDetailInfo("AbsoluteURLReferrer", objBasePortalException.AbsoluteURLReferrer));
            log.LogProperties.Add(new LogDetailInfo("UserAgent", objBasePortalException.UserAgent));
            log.LogProperties.Add(new LogDetailInfo("DefaultDataProvider", objBasePortalException.DefaultDataProvider));
            log.LogProperties.Add(new LogDetailInfo("ExceptionGUID", objBasePortalException.ExceptionGUID));
            log.LogProperties.Add(new LogDetailInfo("InnerException", objBasePortalException.InnerException.Message));
            log.LogProperties.Add(new LogDetailInfo("FileName", objBasePortalException.FileName));
            log.LogProperties.Add(new LogDetailInfo("FileLineNumber", objBasePortalException.FileLineNumber.ToString()));
            log.LogProperties.Add(new LogDetailInfo("FileColumnNumber", objBasePortalException.FileColumnNumber.ToString()));
            log.LogProperties.Add(new LogDetailInfo("Method", objBasePortalException.Method));
            log.LogProperties.Add(new LogDetailInfo("StackTrace", objBasePortalException.StackTrace));
            log.LogProperties.Add(new LogDetailInfo("Message", objBasePortalException.Message));
            log.LogProperties.Add(new LogDetailInfo("Source", objBasePortalException.Source));
            log.LogPortalID = objBasePortalException.PortalID;
            LogController.Instance.AddLog(log);

            //when current user is host user and exception is PageLoadException, try to log the log guid into cookies.
            //so that this log can be picked and do more action on it later.
            if (logType == ExceptionLogType.PAGE_LOAD_EXCEPTION && HttpContext.Current != null && UserController.Instance.GetCurrentUserInfo().IsSuperUser)
            {
                HttpContext.Current.Response.Cookies.Add(new HttpCookie("LogGUID", log.LogGUID){HttpOnly = false});
            }
        }
    }
}