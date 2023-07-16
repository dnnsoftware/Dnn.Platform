// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
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

    [DnnAuthorize]
    public class ContentWorkflowServiceController : DnnApiController
    {
        private readonly IWorkflowEngine workflowEngine;

        /// <summary>Initializes a new instance of the <see cref="ContentWorkflowServiceController"/> class.</summary>
        public ContentWorkflowServiceController()
        {
            this.workflowEngine = WorkflowEngine.Instance;
        }

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
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransiction = new StateTransaction
                    {
                        ContentItemId = int.Parse(parameters[0]),
                        CurrentStateId = int.Parse(parameters[2]),
                        Message = new StateTransactionMessage(),
                        UserId = this.UserInfo.UserID,
                    };
                    this.workflowEngine.DiscardState(stateTransiction);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
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
                        return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    string[] parameters = notification.Context.Split(':');

                    var stateTransiction = new StateTransaction
                    {
                        ContentItemId = int.Parse(parameters[0]),
                        CurrentStateId = int.Parse(parameters[2]),
                        Message = new StateTransactionMessage(),
                        UserId = this.UserInfo.UserID,
                    };
                    this.workflowEngine.CompleteState(stateTransiction);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }
    }
}
