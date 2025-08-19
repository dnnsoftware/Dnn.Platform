// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.AdminLogs.Services
{
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
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;
    using Newtonsoft.Json.Linq;

    [MenuPermission(MenuName = Components.Constants.MenuName)]
    public class AdminLogsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AdminLogsController));
        private readonly Components.AdminLogsController controller = new Components.AdminLogsController();

        /// GET: api/AdminLogs/GetLogTypes
        /// <summary>Gets log types.</summary>
        /// <returns>List of log types.</returns>
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
                    LogTypeKey = "*",
                });

                var types = logTypes.Select(v => new
                {
                    v.LogTypeFriendlyName,
                    v.LogTypeKey,
                }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogItems
        /// <summary>Gets log items.</summary>
        /// <param name="logType">The log type.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="pageIndex">The page index.</param>
        /// <returns>List of log items.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView)]
        public HttpResponseMessage GetLogItems(string logType, int pageSize, int pageIndex)
        {
            try
            {
                var portalId = this.UserInfo.IsSuperUser ? this.Request.GetQueryStringAsInteger("portalid") : this.PortalId;
                var totalRecords = 0;
                var logItems = LogController.Instance.GetLogs(
                    portalId,
                    logType == "*" ? string.Empty : logType,
                    pageSize,
                    pageIndex,
                    ref totalRecords);

                var items = logItems.Select(v => new
                {
                    v.LogGUID,
                    v.LogFileID,
                    this.controller.GetMyLogType(v.LogTypeKey).LogTypeCSSClass,
                    this.controller.GetMyLogType(v.LogTypeKey).LogTypeFriendlyName,
                    v.LogUserName,
                    v.LogPortalName,
                    LogCreateDate = v.LogCreateDate.ToString("G", CultureInfo.InvariantCulture),
                    v.LogProperties.Summary,
                    LogProperties = this.controller.GetPropertiesText(v),
                });

                var response = new
                {
                    Success = true,
                    Results = items,
                    TotalResults = totalRecords,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogItems
        /// <summary>Deletes log items.</summary>
        /// <param name="logIds">List of log items to be deleted.</param>
        /// <returns>A response indicating success.</returns>
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

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/EmailLogItems
        /// <summary>Sends log items via email.</summary>
        /// <param name="request">send email request.</param>
        /// <returns>A response with result details.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.AdminLogsView + "&" + Components.Constants.AdminLogsEdit)]
        public HttpResponseMessage EmailLogItems(EmailLogItemsRequest request)
        {
            try
            {
                if (!this.UserInfo.IsSuperUser && request.LogIds.Any(logGuid => (LogController.Instance.GetLog(logGuid)?.LogPortalId != this.PortalId)))
                {
                    return this.Request.CreateErrorResponse(
                        HttpStatusCode.Unauthorized,
                        Localization.GetString("UnAuthorizedToSendLog", Components.Constants.LocalResourcesFile));
                }

                string error;
                var subject = request.Subject;
                var strFromEmailAddress = !string.IsNullOrEmpty(this.UserInfo.Email) ? this.UserInfo.Email : this.PortalSettings.Email;

                if (string.IsNullOrEmpty(subject))
                {
                    subject = this.PortalSettings.PortalName + @" Exceptions";
                }

                string returnMsg = this.controller.EmailLogItems(subject, strFromEmailAddress, request.Email, request.Message, request.LogIds, out error);
                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Success = string.IsNullOrEmpty(returnMsg) ? true : false,
                    ErrorMessage = error,
                    ReturnMessage = returnMsg,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/ClearLog
        /// <summary>Clears all log items.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequireHost]
        public HttpResponseMessage ClearLog()
        {
            try
            {
                this.controller.ClearLog();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetKeepMostRecentOptions
        /// <summary>Gets options of Keep Most Recent.</summary>
        /// <returns>Option list of Keep Most Recent.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetKeepMostRecentOptions()
        {
            try
            {
                var options = this.controller.GetKeepMostRecentOptions().ToList();

                var response = new
                {
                    Success = true,
                    Results = options,
                    TotalResults = options.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetOccurrenceOptions
        /// <summary>Gets options of occurrence threshold.</summary>
        /// <returns>Option list of occurrence threshold.</returns>
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
                        Thresholds = this.controller.GetOccurenceThresholds().ToList(),
                        NotificationTimes = this.controller.GetOccurenceThresholdNotificationTimes().ToList(),
                        NotificationTimeTypes = this.controller.GetOccurenceThresholdNotificationTimeTypes().ToList(),
                    },
                    TotalResults = 1,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>Gets log settings.</summary>
        /// <returns>List of log settings.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSettings()
        {
            try
            {
                var logTypes = LogController.Instance.GetLogTypeConfigInfo().Cast<LogTypeConfigInfo>();

                int portalId;
                var types = logTypes
                    .Where(x => this.UserInfo.IsSuperUser || (int.TryParse(x.LogTypePortalID, out portalId) && portalId == this.PortalId))
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
                        v.ID,
                    }).ToList();

                var response = new
                {
                    Success = true,
                    Results = types,
                    TotalResults = types.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLogSettings
        /// <summary>Gets log type settings.</summary>
        /// <param name="logTypeConfigId">The log type config ID.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpGet]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView, CheckPermissionForAdmin = true)]
        public HttpResponseMessage GetLogSetting(string logTypeConfigId)
        {
            try
            {
                var configInfo = this.controller.GetLogTypeConfig(logTypeConfigId);
                int portalId;
                if (!this.UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != this.PortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
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
                    configInfo.MailToAddress,
                });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/AddLogSettings
        /// <summary>Adds a new log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage AddLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            try
            {
                if (!this.UserInfo.IsSuperUser)
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                this.controller.AddLogTypeConfig(logTypeConfigInfo);
                return this.GetLatestLogSetting();
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/UpdateLogSettings
        /// <summary>Updates an existing log settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response with the log setting info.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage UpdateLogSetting([FromBody] UpdateLogSettingsRequest request)
        {
            try
            {
                request.LogTypePortalID = this.UserInfo.IsSuperUser ? request.LogTypePortalID : this.PortalId.ToString();

                int requestPortalId;
                int settingPortalId;
                var configInfo = this.controller.GetLogTypeConfig(request.ID);
                if (!this.UserInfo.IsSuperUser &&
                    (!int.TryParse(configInfo.LogTypePortalID, out settingPortalId) ||
                     !int.TryParse(request.LogTypePortalID, out requestPortalId) || requestPortalId != settingPortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                var logTypeConfigInfo = JObject.FromObject(request).ToObject<LogTypeConfigInfo>();
                this.controller.UpdateLogTypeConfig(logTypeConfigInfo);
                return this.GetLogSetting(request.ID);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/AdminLogs/DeleteLogSettings
        /// <summary>Deletes an existing log settings.</summary>
        /// <param name="request">The delete request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdvancedPermission(MenuName = Components.Constants.MenuName, Permission = Components.Constants.LogSettingsView + "&" + Components.Constants.LogSettingsEdit, CheckPermissionForAdmin = true)]
        public HttpResponseMessage DeleteLogSetting(DeleteLogSettingsRequest request)
        {
            try
            {
                var configInfo = this.controller.GetLogTypeConfig(request.LogTypeConfigId);
                int portalId;
                if (!this.UserInfo.IsSuperUser && (!int.TryParse(configInfo.LogTypePortalID, out portalId) || portalId != this.PortalId))
                {
                    return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                this.controller.DeleteLogTypeConfig(request.LogTypeConfigId);
                return this.Request.CreateResponse(
                    HttpStatusCode.OK,
                    new { Success = true, LogSettingId = request.LogTypeConfigId });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/AdminLogs/GetLatestLogSetting
        /// <summary>Gets latest log setting.</summary>
        /// <returns>Log Setting.</returns>
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
                    return this.Request.CreateResponse(HttpStatusCode.OK, new
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
                        configInfo.MailToAddress,
                    });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
