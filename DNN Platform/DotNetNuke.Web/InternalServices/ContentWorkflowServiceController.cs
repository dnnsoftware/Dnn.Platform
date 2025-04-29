// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

[DnnAuthorize]
public partial class ContentWorkflowServiceController : DnnApiController
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

    [DnnDeprecated(7, 4, 0, "No replacement")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public partial HttpResponseMessage Review(NotificationDTO postData)
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

                var source = notification.Context;
                string[] parameters = null;
                if (notification.Context.Contains(";"))
                {
                    parameters = notification.Context.Split(';');
                    source = parameters[0];
                    parameters = parameters.ToList().Skip(1).ToArray();
                }

                var workflow = ContentWorkflowController.Instance.GetDefaultWorkflow(this.PortalSettings.PortalId);
                var workflowSource = ContentWorkflowController.Instance.GetWorkflowSource(workflow.WorkflowID, source);
                if (workflowSource == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                var sourceAction = Reflection.CreateInstance(Reflection.CreateType(workflowSource.SourceType)) as IContentWorkflowAction;
                if (sourceAction == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Link = sourceAction.GetAction(parameters) });
            }
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }
}
