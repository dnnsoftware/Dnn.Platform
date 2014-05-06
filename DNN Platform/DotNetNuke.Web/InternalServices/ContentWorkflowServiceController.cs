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

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class ContentWorkflowServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentWorkflowServiceController));
        
        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Review(NotificationDTO postData)
        {
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);

                if (notification != null)
                {
                    //Dismiss the notification
                    NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, UserInfo.UserID);

                    if (string.IsNullOrEmpty(notification.Context))
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success"});
                    }

                    var source = notification.Context;
                    string[] parameters = null;
                    if (notification.Context.Contains(";"))
                    {
                        parameters = notification.Context.Split(';');
                        source = parameters[0];
                        parameters = parameters.ToList().Skip(1).ToArray();
                    }

                    var workflow = ContentWorkflowController.Instance.GetDefaultWorkflow(PortalSettings.PortalId);
                    var workflowSource = ContentWorkflowController.Instance.GetWorkflowSource(workflow.WorkflowID, source);
                    if (workflowSource == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    var sourceAction = Reflection.CreateInstance(Reflection.CreateType(workflowSource.SourceType)) as IContentWorkflowAction;
                    if (sourceAction == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Link = sourceAction.GetAction(parameters) }); 
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");

        }
    }
}