#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.AdminLogs.Services.Dto;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Api;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.AdminLogs.Services
{
    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class AdminLogsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AdminLogsController));
        private readonly Components.AdminLogsController _controller = new Components.AdminLogsController();

        /// GET: api/AdminLogs/GetLogTypes
        /// <summary>
        /// Gets log types
        /// </summary>
        /// <param></param>
        /// <returns>List of log types</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView + "," + Components.Constants.LogSettingsEdit)]
        public HttpResponseMessage GetLogTypes()
        {
            try
            {
                var logTypes =
                    LogController.Instance.GetLogTypeInfoDictionary()
                        .Values.OrderBy(t => t.LogTypeFriendlyName)
                        .ToList();

                logTypes.Insert(0, new LogTypeInfo
                {
                    LogTypeFriendlyName = Localization.GetString("AllTypes", Components.Constants.LocalResourcesFile),
                    LogTypeKey = "*"
                });

                var types = logTypes.Select(v => new
                {
                    v.LogTypeFriendlyName,
                    v.LogTypeKey
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #region Event Viewer

        /// GET: api/AdminLogs/GetLogItems
        /// <summary>
        /// Gets log items
        /// </summary>
        /// <param></param>
        /// <param name="logType"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns>List of log items</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView)]
        public HttpResponseMessage GetLogItems(string logType, int pageSize, int pageIndex)
        {
            try
            {
                var portalId = UserInfo.IsSuperUser ? Request.GetQueryStringAsInteger("portalid") : PortalId;
                var totalRecords = 0;
                var logItems = LogController.Instance.GetLogs(portalId,
                    logType == "*" ? string.Empty : logType,
                    pageSize, pageIndex, ref totalRecords);

                var items = logItems.Select(v => new
                {
                    v.LogGUID,
                    v.LogFileID,
                    _controller.GetMyLogType(v.LogTypeKey).LogTypeCSSClass,
                    _controller.GetMyLogType(v.LogTypeKey).LogTypeFriendlyName,
                    v.LogUserName,
                    v.LogPortalName,
                    LogCreateDate = v.LogCreateDate.ToString("G", CultureInfo.InvariantCulture),
                    v.LogProperties.Summary,
                    LogProperties = _controller.GetPropertiesText(v)
                });

                var response = new
                {
                    Success = true,
                    Results = items,
                    TotalResults = totalRecords
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogItems
        /// <summary>
        /// Deletes log items
        /// </summary>
        /// <param name="logIds">List of log items to be deleted</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage DeleteLogItems(IEnumerable<string> logIds)
        {
            try
            {
                foreach (var logId in logIds)
                {
                    var objLogInfo = new LogInfo { LogGUID = logId };
                    LogController.Instance.DeleteLog(objLogInfo);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/EmailLogItems
        /// <summary>
        /// Sends log items via email
        /// </summary>
        /// <param name="request">send email request</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView + "&" + Components.Constants.AdminLogsEdit)]
        public HttpResponseMessage EmailLogItems(EmailLogItemsRequest request)
        {
            try
            {
                if (!UserInfo.IsSuperUser && request.LogIds.Any(
                    x =>
                        ((LogInfo)
                            LogController.Instance.GetSingleLog(new LogInfo { LogGUID = x },
                                LoggingProvider.ReturnType.LogInfoObjects))?.LogPortalID != PortalId))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized,
                        Localization.GetString("UnAuthorizedToSendLog", Components.Constants.LocalResourcesFile));
                }
                string error;
                var subject = request.Subject;
                var strFromEmailAddress = !string.IsNullOrEmpty(UserInfo.Email) ? UserInfo.Email : PortalSettings.Email;

                if (string.IsNullOrEmpty(subject))
                {
                    subject = PortalSettings.PortalName + @" Exceptions";
                }

                string returnMsg = _controller.EmailLogItems(subject, strFromEmailAddress, request.Email,
                    request.Message, request.LogIds, out error);
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = string.IsNullOrEmpty(returnMsg) ? true : false,
                    ErrorMessage = error,
                    ReturnMessage = returnMsg
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/ClearLog
        /// <summary>
        /// Clears all log items
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ClearLog()
        {
            try
            {
                _controller.ClearLog();
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Log Settings

        /// GET: api/AdminLogs/GetKeepMostRecentOptions
        /// <summary>
        /// Gets options of Keep Most Recent
        /// </summary>
        /// <param></param>
        /// <returns>Option list of Keep Most Recent</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetKeepMostRecentOptions()
        {
            try
            {
                var options = _controller.GetKeepMostRecentOptions().ToList();

                var response = new
                {
                    Success = true,
                    Results = options,
                    TotalResults = options.Count
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetOccurenceThresholdOptions
        /// <summary>
        /// Gets options of Occurence Threshold
        /// </summary>
        /// <param></param>
        /// <returns>Option list of Occurence Threshold</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetOccurrenceOptions()
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        Thresholds = _controller.GetOccurenceThresholds().ToList(),
                        NotificationTimes = _controller.GetOccurenceThresholdNotificationTimes().ToList(),
                        NotificationTimeTypes = _controller.GetOccurenceThresholdNotificationTimeTypes().ToList()
                    },
                    TotalResults = 1
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>
        /// Gets log settings
        /// </summary>
        /// <param></param>
        /// <returns>List of log settings</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSettings()
        {
            try
            {
                var logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>();

                int portalId;
                var types = logTypes
                    .Where(x => UserInfo.IsSuperUser || (int.TryParse(x.LogTypePortalID, out portalId) && portalId == PortalId))
                    .Select(v => new
                    {
                        v.LogTypeFriendlyName,
                        v.LogTypeKey,
                        v.LogTypePortalID,
                        LogTypePortalName =
                            int.TryParse(v.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                        v.LoggingIsActive,
                        v.LogFileName,
                        v.ID
                    }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>
        /// Gets log type settings
        /// </summary>
        /// <param name="logTypeConfigId"></param>
        /// <returns></returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSetting(string logTypeConfigId)
        {
            try
            {
                var configInfo = _controller.GetLogTypeConfig(logTypeConfigId);
                int portalId;
                if (!UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != PortalId))
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    configInfo.ID,
                    configInfo.LoggingIsActive,
                    configInfo.LogTypeFriendlyName,
                    configInfo.LogTypeKey,
                    LogTypePortalID =
                            int.TryParse(configInfo.LogTypePortalID, out portalId) ? portalId.ToString() : "*",
                    LogTypePortalName =
                            int.TryParse(configInfo.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                    configInfo.KeepMostRecent,
                    configInfo.EmailNotificationIsActive,
                    configInfo.NotificationThreshold,
                    configInfo.NotificationThresholdTime,
                    configInfo.NotificationThresholdTimeType,
                    configInfo.MailFromAddress,
                    configInfo.MailToAddress
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/AddLogSettings
        /// <summary>
        /// Adds a new log settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage AddLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            try
            {
                var isAdmin = UserInfo.Roles.Contains(PortalSettings.AdministratorRoleName);
                if (isAdmin)
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                request.LogTypePortalID = UserInfo.IsSuperUser ? request.LogTypePortalID : PortalId.ToString();

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                _controller.AddLogTypeConfig(logTypeConfigInfo);
                return GetLatestLogSetting();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/UpdateLogSettings
        /// <summary>
        /// Updates an existing log settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage UpdateLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            try
            {
                request.LogTypePortalID = UserInfo.IsSuperUser ? request.LogTypePortalID : PortalId.ToString();

                int requestPortalId;
                int settingPortalId;
                var configInfo = _controller.GetLogTypeConfig(request.ID);
                if (!UserInfo.IsSuperUser &&
                    (!int.TryParse(configInfo.LogTypePortalID, out settingPortalId) ||
                     !int.TryParse(request.LogTypePortalID, out requestPortalId) || requestPortalId != settingPortalId))
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                _controller.UpdateLogTypeConfig(logTypeConfigInfo);
                return GetLogSetting(request.ID);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogSettings
        /// <summary>
        /// Deletes an existing log settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage DeleteLogSetting(DeleteLogSettingsRequest request)
        {
            try
            {
                var configInfo = _controller.GetLogTypeConfig(request.LogTypeConfigId);
                int portalId;
                if (!UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != PortalId))
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);

                _controller.DeleteLogTypeConfig(request.LogTypeConfigId);
                return Request.CreateResponse(HttpStatusCode.OK,
                    new { Success = true, LogSettingId = request.LogTypeConfigId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region Private Methods

        /// GET: api/AdminLogs/GetLatestLogSetting
        /// <summary>
        /// Gets latest log setting
        /// </summary>
        /// <param></param>
        /// <returns>Log Setting</returns>
        private HttpResponseMessage GetLatestLogSetting()
        {
            try
            {
                var logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>()
                    .OrderByDescending(l => Convert.ToInt32(l.ID)).ToList();
                var configInfo = logTypes.FirstOrDefault();

                if (configInfo != null)
                {
                    int portalId;
                    return Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        configInfo.ID,
                        configInfo.LoggingIsActive,
                        configInfo.LogTypeFriendlyName,
                        configInfo.LogTypeKey,
                        LogTypePortalID =
                            int.TryParse(configInfo.LogTypePortalID, out portalId) ? portalId.ToString() : "*",
                        LogTypePortalName =
                            int.TryParse(configInfo.LogTypePortalID, out portalId)
                                ? PortalController.Instance.GetPortal(portalId).PortalName
                                : "*",
                        configInfo.KeepMostRecent,
                        configInfo.EmailNotificationIsActive,
                        configInfo.NotificationThreshold,
                        configInfo.NotificationThresholdTime,
                        configInfo.NotificationThresholdTimeType,
                        configInfo.MailFromAddress,
                        configInfo.MailToAddress
                    });
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion
    }
}