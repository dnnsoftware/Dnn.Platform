#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.UI.Components.Contracts;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.UI.Services.PersonaBar
{
    [DnnAuthorize(StaticRoles = Constants.AllMajorRoles)]
    public class TasksController : PersonaBarApiController
    {
        #region Public API methods
        
        /// <summary>
        /// Return a list of tasks for cmx
        /// </summary>
        [HttpGet]
        public HttpResponseMessage GetTasks(int pageIndex, int pageSize, int afterNotificationId)
        {
            try
            {
                int totalTasks;
                int totalRollups;
                var portalId = PortalController.GetEffectivePortalId(PortalSettings.PortalId);
                var contract = new GetTasksContract
                {
                    PortalId = portalId,
                    UserId = PortalSettings.UserId,
                    PageSize = pageSize,
                    PageIndex = pageIndex,
                    AfterNotificationId = afterNotificationId
                };
                var results = Components.Controllers.TasksController.Instance.GetTasks(contract, out totalRollups, out totalTasks);
                var response = new
                {
                    Success = true,
                    Results = results,
                    TotalRollups = totalRollups,
                    TotalTasks = totalTasks,
                };

                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        /// <summary>
        /// Get details about a Task.
        /// </summary>
        /// <param name="notificationId">NotificationId from the previous call of GetTasks</param>
        /// <param name="indexId">1 based index, e.g. first page = 1</param>
        [HttpGet]
        public HttpResponseMessage GetTask(int notificationId, int indexId)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(PortalSettings.PortalId);
                var contract = new GetTaskContract
                {
                    PortalId = portalId,
                    UserId = PortalSettings.UserId,
                    NotificationId = notificationId,
                    IndexId = indexId
                };


                var response = new
                {
                    Success = true,
                    UserDetail = Components.Controllers.TasksController.Instance.GetTask(contract)
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        #endregion
    }
}
