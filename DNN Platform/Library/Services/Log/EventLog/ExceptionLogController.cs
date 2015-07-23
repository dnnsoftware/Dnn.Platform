#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;

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
			var log = new LogInfo
			{
				Exception = Exceptions.Exceptions.GetExceptionInfo(objBasePortalException),
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
				AddLog(objBasePortalException, log, ExceptionLogType.MODULE_LOAD_EXCEPTION);
			}
			else if (objBasePortalException.GetType().Name == "PageLoadException")
			{
				AddLog(objBasePortalException, log, ExceptionLogType.PAGE_LOAD_EXCEPTION);
			}
			else if (objBasePortalException.GetType().Name == "SchedulerException")
			{
				AddLog(objBasePortalException, log, ExceptionLogType.SCHEDULER_EXCEPTION);
			}
			else if (objBasePortalException.GetType().Name == "SecurityException")
			{
				AddLog(objBasePortalException, log, ExceptionLogType.SECURITY_EXCEPTION);
			}
			else if (objBasePortalException.GetType().Name == "SearchException")
			{
				AddLog(objBasePortalException, log, ExceptionLogType.SEARCH_INDEXER_EXCEPTION);
			}
			else
			{
				AddLog(objBasePortalException, log, ExceptionLogType.GENERAL_EXCEPTION);
			}
		}

		public void AddLog(Exception objException, ExceptionLogType logType)
		{
			var log = new LogInfo { Exception = new ExceptionInfo(objException) };
			AddLog(objException, log, logType);
		}

		public void AddLog(Exception objException, LogInfo log, ExceptionLogType logType)
		{
			log.LogTypeKey = logType.ToString();
			if (logType == ExceptionLogType.SEARCH_INDEXER_EXCEPTION)
			{
				//Add SearchException Properties
				var objSearchException = (SearchException)objException;
				log.LogProperties.Add(new LogDetailInfo("ModuleId", objSearchException.SearchItem.ModuleId.ToString()));
				log.LogProperties.Add(new LogDetailInfo("SearchItemId", objSearchException.SearchItem.SearchItemId.ToString()));
				log.LogProperties.Add(new LogDetailInfo("Title", objSearchException.SearchItem.Title));
				log.LogProperties.Add(new LogDetailInfo("SearchKey", objSearchException.SearchItem.SearchKey));
				log.LogProperties.Add(new LogDetailInfo("GUID", objSearchException.SearchItem.GUID));
			}
			else if (logType == ExceptionLogType.MODULE_LOAD_EXCEPTION)
			{
				//Add ModuleLoadException Properties
				var objModuleLoadException = (ModuleLoadException)objException;
				log.LogProperties.Add(new LogDetailInfo("ModuleId", objModuleLoadException.ModuleId.ToString()));
				log.LogProperties.Add(new LogDetailInfo("ModuleDefId", objModuleLoadException.ModuleDefId.ToString()));
				log.LogProperties.Add(new LogDetailInfo("FriendlyName", objModuleLoadException.FriendlyName));
				log.LogProperties.Add(new LogDetailInfo("ModuleControlSource", objModuleLoadException.ModuleControlSource));
			}
			else if (logType == ExceptionLogType.SECURITY_EXCEPTION)
			{
				//Add SecurityException Properties
				var objSecurityException = (SecurityException)objException;
				log.LogProperties.Add(new LogDetailInfo("Querystring", objSecurityException.Querystring));
				log.LogProperties.Add(new LogDetailInfo("IP", objSecurityException.IP));
			}

			//Add BasePortalException Properties
			var objBasePortalException = new BasePortalException(objException.ToString(), objException);
			log.LogProperties.Add(new LogDetailInfo("AbsoluteURL", objBasePortalException.AbsoluteURL));
			log.LogProperties.Add(new LogDetailInfo("DefaultDataProvider", objBasePortalException.DefaultDataProvider));
			log.LogProperties.Add(new LogDetailInfo("ExceptionGUID", objBasePortalException.ExceptionGUID));
			log.LogPortalID = objBasePortalException.PortalID;
			LogController.Instance.AddLog(log);

			//when current user is host user and exception is PageLoadException, try to log the log guid into cookies.
			//so that this log can be picked and do more action on it later.
			if (logType == ExceptionLogType.PAGE_LOAD_EXCEPTION && HttpContext.Current != null && UserController.Instance.GetCurrentUserInfo().IsSuperUser)
			{
                HttpContext.Current.Response.Cookies.Add(
                    new HttpCookie("LogGUID", log.LogGUID) { HttpOnly = false, Path = (!string.IsNullOrEmpty(Globals.ApplicationPath) ? Globals.ApplicationPath : "/") });
			}
		}
	}
}