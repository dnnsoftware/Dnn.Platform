// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class ContentWorkflowServiceController : DnnApiController
    {
        #region Members
        private readonly IWorkflowEngine _workflowEngine;
        #endregion

        #region Constructor
        public ContentWorkflowServiceController()
        {
            _workflowEngine = WorkflowEngine.Instance;
        }
        #endregion

        #region Web Methods
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reject(NotificationDTO postData)
        {
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                if (notification != null)
                {
                    if (string.IsNullOrEmpty(notification.Context))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransiction = new StateTransaction
                                           {
                                               ContentItemId = int.Parse(parameters[0]),
                                               CurrentStateId = int.Parse(parameters[2]),
                                               Message = new StateTransactionMessage (),
                                               UserId = UserInfo.UserID
                                           };
                    _workflowEngine.DiscardState(stateTransiction);

                    return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Approve(NotificationDTO postData)
        {
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                if (notification != null)
                {
                    if (string.IsNullOrEmpty(notification.Context))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransiction = new StateTransaction
                                            {
                                                ContentItemId = int.Parse(parameters[0]),
                                                CurrentStateId = int.Parse(parameters[2]),
                                                Message = new StateTransactionMessage(),
                                                UserId = UserInfo.UserID
                                            };
                    _workflowEngine.CompleteState(stateTransiction);

                    return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");

        }

        #endregion
    }
}
