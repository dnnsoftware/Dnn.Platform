// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    /// <summary>An API controller for managing content moving through its workflow.</summary>
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
            var currentPage = TabController.Instance.GetTab(tabId, portalId);
            var contentItemId = currentPage.ContentItemId;
            var contentController = Util.GetContentController();
            var contentItem = contentController.GetContentItem(contentItemId);
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
}
