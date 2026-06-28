// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Common;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Content.Workflow.Dto;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;

/// <summary>An API controller for managing content moving through its workflow.</summary>
/// <param name="contentController">The content controller.</param>
/// <param name="workflowEngine">The workflow engine.</param>
/// <param name="tabController">The tab controller.</param>
[DnnAuthorize]
public partial class ContentWorkflowServiceController(IContentController contentController, IWorkflowEngine workflowEngine, ITabController tabController)
    : DnnApiController
{
    private readonly IContentController contentController = contentController ?? ContentController.Instance;
    private readonly IWorkflowEngine workflowEngine = workflowEngine ?? WorkflowEngine.Instance;
    private readonly ITabController tabController = tabController ?? TabController.Instance;

    /// <summary>Initializes a new instance of the <see cref="ContentWorkflowServiceController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IContentController. Scheduled removal in v12.0.0.")]
    public ContentWorkflowServiceController()
        : this(null, null, null)
    {
    }

    /// <summary>Rejects a workflow.</summary>
    /// <param name="postData">The workflow notification to reject.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnDeprecated(10, 3, 3, "Use overload taking NotificationRequest")]
    public partial HttpResponseMessage Reject(NotificationDTO postData)
        => this.Reject(postData?.ToNotificationRequest());

    /// <summary>Rejects a workflow.</summary>
    /// <param name="requestBody">The workflow notification to reject.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage Reject(NotificationRequest requestBody)
    {
        try
        {
            var notification = NotificationsController.Instance.GetNotification(requestBody.NotificationId);
            if (notification != null)
            {
                if (string.IsNullOrEmpty(notification.Context))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }

                var parameters = notification.Context.Split(':');

                var stateTransaction = new StateTransaction
                {
                    ContentItemId = int.Parse(parameters[0], CultureInfo.InvariantCulture),
                    CurrentStateId = int.Parse(parameters[2], CultureInfo.InvariantCulture),
                    Message = new StateTransactionMessage(),
                    UserId = this.UserInfo.UserID,
                };
                this.workflowEngine.DiscardState(stateTransaction);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Approves a workflow.</summary>
    /// <param name="postData">The workflow notification to approve.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnDeprecated(10, 3, 3, "Use overload taking NotificationRequest")]
    public partial HttpResponseMessage Approve(NotificationDTO postData)
        => this.Approve(postData?.ToNotificationRequest());

    /// <summary>Approves a workflow.</summary>
    /// <param name="requestBody">The workflow notification to approve.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage Approve(NotificationRequest requestBody)
    {
        try
        {
            var notification = NotificationsController.Instance.GetNotification(requestBody.NotificationId);
            if (notification != null)
            {
                if (string.IsNullOrEmpty(notification.Context))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }

                string[] parameters = notification.Context.Split(':');

                var stateTransaction = new StateTransaction
                {
                    ContentItemId = int.Parse(parameters[0], CultureInfo.InvariantCulture),
                    CurrentStateId = int.Parse(parameters[2], CultureInfo.InvariantCulture),
                    Message = new StateTransactionMessage(),
                    UserId = this.UserInfo.UserID,
                };
                this.workflowEngine.CompleteState(stateTransaction);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Complete a workflow state for the current page.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage CompleteState()
    {
        try
        {
            this.workflowEngine.CompleteState(this.BuildStateTransaction());
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Discards a workflow state for the current page.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage DiscardState()
    {
        try
        {
            this.workflowEngine.DiscardState(this.BuildStateTransaction());
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Complete a workflow for the current page.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage CompleteWorkflow()
    {
        try
        {
            this.workflowEngine.CompleteWorkflow(this.BuildStateTransaction());
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Discards a workflow for the current page.</summary>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage DiscardWorkflow()
    {
        try
        {
            this.workflowEngine.DiscardWorkflow(this.BuildStateTransaction());
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }
        catch (Exception exc)
        {
            Exceptions.LogException(exc);
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    private StateTransaction BuildStateTransaction()
    {
        var portalId = this.PortalSettings.PortalId;
        var tabId = this.Request.FindTabId();
        var currentPage = this.tabController.GetTab(tabId, portalId);
        var contentItemId = currentPage.ContentItemId;
        var contentItem = this.contentController.GetContentItem(contentItemId);
        var stateTransaction = new StateTransaction
        {
            ContentItemId = contentItem.ContentItemId,
            CurrentStateId = contentItem.StateID,
            Message = new StateTransactionMessage(),
            UserId = this.UserInfo.UserID,
        };
        return stateTransaction;
    }
}
