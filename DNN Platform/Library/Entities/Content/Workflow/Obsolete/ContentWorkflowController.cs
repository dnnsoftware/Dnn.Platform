// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Social.Notifications;

    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowController : ComponentBase<IContentWorkflowController, ContentWorkflowController>, IContentWorkflowController
    {
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";
        private readonly ContentController contentController;

        private ContentWorkflowController()
        {
            this.contentController = new ContentController();
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId)
        {
            var item = this.contentController.GetContentItem(contentItemId);
            var workflow = this.GetWorkflow(item);
            var stateId = this.GetLastWorkflowStateID(workflow);
            this.AddWorkflowCommentLog(item, comment, userId);
            this.AddWorkflowLog(item, ContentWorkflowLogType.WorkflowDiscarded, userId);
            this.SetWorkflowState(stateId, item);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId)
        {
            var item = this.contentController.GetContentItem(contentItemId);
            var workflow = this.GetWorkflow(item);
            var lastStateId = this.GetLastWorkflowStateID(workflow);
            this.AddWorkflowCommentLog(item, comment, userId);
            this.AddWorkflowLog(item, ContentWorkflowLogType.WorkflowApproved, userId);
            this.SetWorkflowState(lastStateId, item);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "")
        {
            var user = UserController.GetUserById(portalID, userID);
            var datetime = DateTime.Now;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : string.Empty);
            result = result.Replace("[DATE]", datetime.ToString("d-MMM-yyyy hh:mm") + datetime.ToString("tt").ToLowerInvariant());
            result = result.Replace("[STATE]", state != null ? state.StateName : string.Empty);
            result = result.Replace("[WORKFLOW]", workflow != null ? workflow.WorkflowName : string.Empty);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : string.Empty);
            result = result.Replace("[COMMENT]", !string.IsNullOrEmpty(comment) ? comment : string.Empty);

            return result;
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID)
        {
            this.CompleteState(itemID, subject, body, comment, portalID, userID, string.Empty);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public void StartWorkflow(int workflowID, int itemID, int userID)
        {
            var item = this.contentController.GetContentItem(itemID);
            var workflow = this.GetWorkflow(item);

            // If already exists a started workflow
            if (workflow != null && !this.IsWorkflowCompleted(workflow, item))
            {
                return;
            }

            if (workflow == null || workflow.WorkflowID != workflowID)
            {
                workflow = this.GetWorkflowByID(workflowID);
            }

            // Delete previous logs
            DataProvider.Instance().DeleteContentWorkflowLogs(itemID, workflowID);
            var newStateID = this.GetFirstWorkflowStateID(workflow);
            this.SetWorkflowState(newStateID, item);
            this.AddWorkflowLog(item, ContentWorkflowLogType.WorkflowStarted, userID);
            this.AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters)
        {
            var item = this.contentController.GetContentItem(itemID);
            var workflow = this.GetWorkflow(item);
            if (workflow == null)
            {
                return;
            }

            if (!this.IsWorkflowCompleted(workflow, item))
            {
                var currentState = this.GetWorkflowStateByID(item.StateID);
                if (!string.IsNullOrEmpty(comment))
                {
                    this.AddWorkflowCommentLog(item, comment, userID);
                }

                this.AddWorkflowLog(item, currentState.StateID == this.GetFirstWorkflowStateID(workflow) ? ContentWorkflowLogType.DraftCompleted : ContentWorkflowLogType.StateCompleted, userID);

                var endStateID = this.GetNextWorkflowStateID(workflow, item.StateID);
                this.SetWorkflowState(endStateID, item);
                if (endStateID == this.GetLastWorkflowStateID(workflow))
                {
                    this.AddWorkflowLog(item, ContentWorkflowLogType.WorkflowApproved, userID);
                }
                else
                {
                    this.AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);
                }

                this.SendNotification(new PortalSettings(portalID), workflow, item, currentState, subject, body, comment, endStateID, userID, source, parameters);
            }
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID)
        {
            var item = this.contentController.GetContentItem(itemID);
            var workflow = this.GetWorkflow(item);
            if (workflow == null)
            {
                return;
            }

            var currentState = this.GetWorkflowStateByID(item.StateID);
            if ((this.GetFirstWorkflowStateID(workflow) != currentState.StateID) && (this.GetLastWorkflowStateID(workflow) != currentState.StateID))
            {
                if (!string.IsNullOrEmpty(comment))
                {
                    this.AddWorkflowCommentLog(item, comment, userID);
                }

                this.AddWorkflowLog(item, ContentWorkflowLogType.StateDiscarded, userID);
                int previousStateID = this.GetPreviousWorkflowStateID(workflow, item.StateID);
                this.SetWorkflowState(previousStateID, item);
                this.AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);
                this.SendNotification(new PortalSettings(portalID), workflow, item, currentState, subject, body, comment, previousStateID, userID, null, null);
            }
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public bool IsWorkflowCompleted(int itemID)
        {
            var item = this.contentController.GetContentItem(itemID); // Ensure DB values
            var workflow = this.GetWorkflow(item);
            if (workflow == null)
            {
                return true; // If item has not workflow, then it is considered as completed
            }

            return this.IsWorkflowCompleted(workflow, item);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        public bool IsWorkflowOnDraft(int itemID)
        {
            var item = this.contentController.GetContentItem(itemID); // Ensure DB values
            var workflow = this.GetWorkflow(item);
            if (workflow == null)
            {
                return false; // If item has not workflow, then it is not on Draft
            }

            return item.StateID == this.GetFirstWorkflowStateID(workflow);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger. Scheduled removal in v10.0.0.")]
        public void AddWorkflowLog(ContentItem item, string action, string comment, int userID)
        {
            var workflow = this.GetWorkflow(item);

            this.AddWorkflowLog(workflow != null ? workflow.WorkflowID : Null.NullInteger, item, action, comment, userID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger. Scheduled removal in v10.0.0.")]
        public IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {
            return CBO.FillCollection<ContentWorkflowLog>(DataProvider.Instance().GetContentWorkflowLogs(contentItemId, workflowId));
        }

        [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
        public void DeleteWorkflowLogs(int contentItemID, int workflowID)
        {
            DataProvider.Instance().DeleteContentWorkflowLogs(contentItemID, workflowID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID)
        {
            return CBO.FillCollection<ContentWorkflowStatePermission>(DataProvider.Instance().GetContentWorkflowStatePermissionsByStateID(stateID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID)
        {
            DataProvider.Instance().AddContentWorkflowStatePermission(
                permission.StateID,
                permission.PermissionID,
                permission.RoleID,
                permission.AllowAccess,
                permission.UserID,
                lastModifiedByUserID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID)
        {
            DataProvider.Instance().UpdateContentWorkflowStatePermission(
                permission.WorkflowStatePermissionID,
                permission.StateID,
                permission.PermissionID,
                permission.RoleID,
                permission.AllowAccess,
                permission.UserID,
                lastModifiedByUserID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public void DeleteWorkflowStatePermission(int workflowStatePermissionID)
        {
            DataProvider.Instance().DeleteContentWorkflowStatePermission(workflowStatePermissionID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public ContentWorkflowState GetWorkflowStateByID(int stateID)
        {
            return CBO.FillObject<ContentWorkflowState>(DataProvider.Instance().GetContentWorkflowState(stateID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public void AddWorkflowState(ContentWorkflowState state)
        {
            var id = DataProvider.Instance().AddContentWorkflowState(
                state.WorkflowID,
                state.StateName,
                state.Order,
                state.IsActive,
                state.SendEmail,
                state.SendMessage,
                state.IsDisposalState,
                state.OnCompleteMessageSubject,
                state.OnCompleteMessageBody,
                state.OnDiscardMessageSubject,
                state.OnDiscardMessageBody);
            state.StateID = id;
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public void UpdateWorkflowState(ContentWorkflowState state)
        {
            DataProvider.Instance().UpdateContentWorkflowState(
                state.StateID,
                state.StateName,
                state.Order,
                state.IsActive,
                state.SendEmail,
                state.SendMessage,
                state.IsDisposalState,
                state.OnCompleteMessageSubject,
                state.OnCompleteMessageBody,
                state.OnDiscardMessageSubject,
                state.OnDiscardMessageBody);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        public IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID)
        {
            return CBO.FillCollection<ContentWorkflowState>(DataProvider.Instance().GetContentWorkflowStates(workflowID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        public IEnumerable<ContentWorkflow> GetWorkflows(int portalID)
        {
            return CBO.FillCollection<ContentWorkflow>(DataProvider.Instance().GetContentWorkflows(portalID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        public ContentWorkflow GetWorkflow(ContentItem item)
        {
            var state = this.GetWorkflowStateByID(item.StateID);
            if (state == null)
            {
                return null;
            }

            return this.GetWorkflowByID(state.WorkflowID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        public void AddWorkflow(ContentWorkflow workflow)
        {
            var id = DataProvider.Instance().AddContentWorkflow(workflow.PortalID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
            workflow.WorkflowID = id;
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        public void UpdateWorkflow(ContentWorkflow workflow)
        {
            DataProvider.Instance().UpdateContentWorkflow(workflow.WorkflowID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        public ContentWorkflow GetWorkflowByID(int workflowID)
        {
            var workflow = CBO.FillObject<ContentWorkflow>(DataProvider.Instance().GetContentWorkflow(workflowID));
            if (workflow != null)
            {
                workflow.States = this.GetWorkflowStates(workflowID);
                return workflow;
            }

            return null;
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager. Scheduled removal in v10.0.0.")]
        public void CreateDefaultWorkflows(int portalId)
        {
            if (this.GetWorkflows(portalId).Any(w => w.WorkflowName == Localization.GetString("DefaultWorkflowName")))
            {
                return;
            }

            var worflow = new ContentWorkflow
            {
                PortalID = portalId,
                WorkflowName = Localization.GetString("DefaultWorkflowName"),
                Description = Localization.GetString("DefaultWorkflowDescription"),
                IsDeleted = false,
                StartAfterCreating = false,
                StartAfterEditing = true,
                DispositionEnabled = false,
                States = new List<ContentWorkflowState>
                                               {
                                                   new ContentWorkflowState
                                                       {
                                                           StateName =
                                                               Localization.GetString("DefaultWorkflowState1.StateName"),
                                                           Order = 1,
                                                           IsActive = true,
                                                           SendEmail = false,
                                                           SendMessage = true,
                                                           IsDisposalState = false,
                                                           OnCompleteMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState1.OnCompleteMessageSubject"),
                                                           OnCompleteMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState1.OnCompleteMessageBody"),
                                                           OnDiscardMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState1.OnDiscardMessageSubject"),
                                                           OnDiscardMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState1.OnDiscardMessageBody")
                                                       },
                                                   new ContentWorkflowState
                                                       {
                                                           StateName =
                                                               Localization.GetString("DefaultWorkflowState2.StateName"),
                                                           Order = 2,
                                                           IsActive = true,
                                                           SendEmail = false,
                                                           SendMessage = true,
                                                           IsDisposalState = false,
                                                           OnCompleteMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState2.OnCompleteMessageSubject"),
                                                           OnCompleteMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState2.OnCompleteMessageBody"),
                                                           OnDiscardMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState2.OnDiscardMessageSubject"),
                                                           OnDiscardMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState2.OnDiscardMessageBody")
                                                       },
                                                   new ContentWorkflowState
                                                       {
                                                           StateName =
                                                               Localization.GetString("DefaultWorkflowState3.StateName"),
                                                           Order = 3,
                                                           IsActive = true,
                                                           SendEmail = false,
                                                           SendMessage = true,
                                                           IsDisposalState = false,
                                                           OnCompleteMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState3.OnCompleteMessageSubject"),
                                                           OnCompleteMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState3.OnCompleteMessageBody"),
                                                           OnDiscardMessageSubject =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState3.OnDiscardMessageSubject"),
                                                           OnDiscardMessageBody =
                                                               Localization.GetString(
                                                                   "DefaultWorkflowState3.OnDiscardMessageBody")
                                                       }
                                               },
            };

            this.AddWorkflow(worflow);
            foreach (var state in worflow.States)
            {
                state.WorkflowID = worflow.WorkflowID;
                this.AddWorkflowState(state);
            }
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager. Scheduled removal in v10.0.0.")]
        public ContentWorkflow GetDefaultWorkflow(int portalID)
        {
            var wf = this.GetWorkflows(portalID).First(); // We assume there is only 1 Workflow. This needs to be changed for other scenarios
            wf.States = this.GetWorkflowStates(wf.WorkflowID);
            return wf;
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsAnyReviewer(int workflowID)
        {
            var workflow = this.GetWorkflowByID(workflowID);
            return workflow.States.Any(contentWorkflowState => this.IsReviewer(contentWorkflowState.StateID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsAnyReviewer(int portalID, int userID, int workflowID)
        {
            var workflow = this.GetWorkflowByID(workflowID);
            return workflow.States.Any(contentWorkflowState => this.IsReviewer(portalID, userID, contentWorkflowState.StateID));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsReviewer(int stateID)
        {
            var permissions = this.GetWorkflowStatePermissionByState(stateID);
            var user = UserController.Instance.GetCurrentUserInfo();
            return this.IsReviewer(user, PortalSettings.Current, permissions);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsReviewer(int portalID, int userID, int stateID)
        {
            var permissions = this.GetWorkflowStatePermissionByState(stateID);
            var user = UserController.GetUserById(portalID, userID);
            var portalSettings = new PortalSettings(portalID);

            return this.IsReviewer(user, portalSettings, permissions);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsCurrentReviewer(int portalID, int userID, int itemID)
        {
            var item = this.contentController.GetContentItem(itemID);
            return this.IsReviewer(portalID, userID, item.StateID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        public bool IsCurrentReviewer(int itemID)
        {
            var item = this.contentController.GetContentItem(itemID);
            return this.IsReviewer(item.StateID);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName)
        {
            return CBO.FillObject<ContentWorkflowSource>(DataProvider.Instance().GetContentWorkflowSource(workflowId, sourceName));
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body,
                                             string comment, int userID)
        {
            var replacedSubject = this.ReplaceNotificationTokens(subject, null, null, null, settings.PortalId, userID);
            var replacedBody = this.ReplaceNotificationTokens(body, null, null, null, settings.PortalId, userID);
            this.SendNotification(sendEmail, sendMessage, settings, roles, users, replacedSubject, replacedBody, comment, userID, null, null);
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
        }

        private static IEnumerable<UserInfo> IncludeSuperUsers(ICollection<UserInfo> users)
        {
            var superUsers = UserController.GetUsers(false, true, Null.NullInteger);
            foreach (UserInfo superUser in superUsers)
            {
                if (IsSuperUserNotIncluded(users, superUser))
                {
                    users.Add(superUser);
                }
            }

            return users;
        }

        private static bool IsSuperUserNotIncluded(IEnumerable<UserInfo> users, UserInfo superUser)
        {
            return users.All(u => u.UserID != superUser.UserID);
        }

        private void AddWorkflowCommentLog(ContentItem item, string userComment, int userID)
        {
            var workflow = this.GetWorkflow(item);

            var logComment = this.ReplaceNotificationTokens(this.GetWorkflowActionComment(ContentWorkflowLogType.CommentProvided), workflow, item, workflow.States.FirstOrDefault(s => s.StateID == item.StateID), workflow.PortalID, userID, userComment);
            this.AddWorkflowLog(workflow.WorkflowID, item, this.GetWorkflowActionText(ContentWorkflowLogType.CommentProvided), logComment, userID);
        }

        private void SendNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment, int userID, string source, string[] parameters)
        {
            if (sendEmail)
            {
                this.SendEmailNotifications(settings, roles, users, subject, body, comment);
            }

            if (sendMessage)
            {
                this.SendMessageNotifications(settings, roles, users, subject, body, comment, userID, source, parameters);
            }
        }

        private void SendNotification(PortalSettings settings, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, string subject, string body, string comment, int destinationStateID, int actionUserID, string source, string[] parameters)
        {
            var permissions = this.GetWorkflowStatePermissionByState(destinationStateID);
            var users = this.GetUsersFromPermissions(settings, permissions);
            var roles = this.GetRolesFromPermissions(settings, permissions);
            var replacedSubject = this.ReplaceNotificationTokens(subject, workflow, item, this.GetWorkflowStateByID(destinationStateID), settings.PortalId, actionUserID);
            var replacedBody = this.ReplaceNotificationTokens(body, workflow, item, this.GetWorkflowStateByID(destinationStateID), settings.PortalId, actionUserID);

            this.SendNotification(state.SendEmail, state.SendMessage, settings, roles, users, replacedSubject, replacedBody, comment, actionUserID, source, parameters);
        }

        private void SendMessageNotifications(PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment, int actionUserID, string source, string[] parameters)
        {
            var fullbody = this.GetFullBody(body, comment);

            if (!roles.Any() && !users.Any())
            {
                return; // If there are no receivers, the notification is avoided
            }

            var notification = new Notification
            {
                NotificationTypeID = NotificationsController.Instance.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId,
                Subject = subject,
                Body = fullbody,
                IncludeDismissAction = true,
                SenderUserID = actionUserID,
            };

            // append the context
            if (!string.IsNullOrEmpty(source))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    source = string.Format("{0};{1}", source, string.Join(";", parameters));
                }

                notification.Context = source;
            }

            NotificationsController.Instance.SendNotification(notification, settings.PortalId, roles.ToList(), users.ToList());
        }

        private string GetFullBody(string body, string comment)
        {
            return body + "<br><br>" + comment;
        }

        private void SendEmailNotifications(PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment)
        {
            var fullbody = this.GetFullBody(body, comment);
            var emailUsers = users.ToList();
            foreach (var role in roles)
            {
                var roleUsers = RoleController.Instance.GetUsersByRole(settings.PortalId, role.RoleName);
                emailUsers.AddRange(from UserInfo user in roleUsers select user);
            }

            foreach (var userMail in emailUsers.Select(u => u.Email).Distinct())
            {
                Mail.SendEmail(settings.Email, userMail, subject, fullbody);
            }
        }

        private IEnumerable<RoleInfo> GetRolesFromPermissions(PortalSettings settings, IEnumerable<ContentWorkflowStatePermission> permissions)
        {
            var roles = new List<RoleInfo>();

            foreach (var permission in permissions)
            {
                if (permission.AllowAccess && permission.RoleID > Null.NullInteger)
                {
                    roles.Add(RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleID));
                }
            }

            if (!IsAdministratorRoleAlreadyIncluded(settings, roles))
            {
                var adminRole = RoleController.Instance.GetRoleByName(settings.PortalId, settings.AdministratorRoleName);
                roles.Add(adminRole);
            }

            return roles;
        }

        private IEnumerable<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<ContentWorkflowStatePermission> permissions)
        {
            var users = new List<UserInfo>();
            foreach (var permission in permissions)
            {
                if (permission.AllowAccess && permission.UserID > Null.NullInteger)
                {
                    users.Add(UserController.GetUserById(settings.PortalId, permission.UserID));
                }
            }

            return IncludeSuperUsers(users);
        }

        private bool IsReviewer(UserInfo user, PortalSettings settings, IEnumerable<ContentWorkflowStatePermission> permissions)
        {
            var administratorRoleName = settings.AdministratorRoleName;
            return user.IsSuperUser || PortalSecurity.IsInRoles(user, settings, administratorRoleName) || PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), "REVIEW"));
        }

        private void AddWorkflowLog(ContentItem item, ContentWorkflowLogType logType, int userID)
        {
            var workflow = this.GetWorkflow(item);

            var comment = this.ReplaceNotificationTokens(this.GetWorkflowActionComment(logType), workflow, item, workflow.States.FirstOrDefault(s => s.StateID == item.StateID), workflow.PortalID, userID);

            this.AddWorkflowLog(workflow.WorkflowID, item, this.GetWorkflowActionText(logType), comment, userID);
        }

        private void AddWorkflowLog(int workflowID, ContentItem item, string action, string comment, int userID)
        {
            DataProvider.Instance().AddContentWorkflowLog(action, comment, userID, workflowID, item.ContentItemId);
        }

        private string GetWorkflowActionText(ContentWorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(ContentWorkflowLogType), logType);
            return Localization.GetString(logName + ".Action");
        }

        private string GetWorkflowActionComment(ContentWorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(ContentWorkflowLogType), logType);
            return Localization.GetString(logName + ".Comment");
        }

        private int GetFirstWorkflowStateID(ContentWorkflow workflow)
        {
            int intStateID = -1;
            var states = workflow.States;
            if (states.Any())
            {
                intStateID = states.OrderBy(s => s.Order).FirstOrDefault().StateID;
            }

            return intStateID;
        }

        private int GetLastWorkflowStateID(ContentWorkflow workflow)
        {
            int intStateID = -1;
            var states = workflow.States;
            if (states.Any())
            {
                intStateID = states.OrderBy(s => s.Order).LastOrDefault().StateID;
            }

            return intStateID;
        }

        private int GetPreviousWorkflowStateID(ContentWorkflow workflow, int stateID)
        {
            int intPreviousStateID = -1;

            var states = workflow.States.OrderBy(s => s.Order);
            int intItem = 0;

            // locate the current state
            var initState = states.SingleOrDefault(s => s.StateID == stateID);
            if (initState != null)
            {
                intPreviousStateID = initState.StateID;
            }

            for (int i = 0; i < states.Count(); i++)
            {
                if (states.ElementAt(i).StateID == stateID)
                {
                    intPreviousStateID = stateID;
                    intItem = i;
                }
            }

            // get previous active state
            if (intPreviousStateID == stateID)
            {
                intItem = intItem - 1;
                while (intItem >= 0)
                {
                    if (states.ElementAt(intItem).IsActive)
                    {
                        intPreviousStateID = states.ElementAt(intItem).StateID;
                        break;
                    }

                    intItem = intItem - 1;
                }
            }

            // if none found then reset to first state
            if (intPreviousStateID == -1)
            {
                intPreviousStateID = this.GetFirstWorkflowStateID(workflow);
            }

            return intPreviousStateID;
        }

        private int GetNextWorkflowStateID(ContentWorkflow workflow, int stateID)
        {
            int intNextStateID = -1;
            var states = workflow.States.OrderBy(s => s.Order);
            int intItem = 0;

            // locate the current state
            for (intItem = 0; intItem < states.Count(); intItem++)
            {
                if (states.ElementAt(intItem).StateID == stateID)
                {
                    intNextStateID = stateID;
                    break;
                }
            }

            // get next active state
            if (intNextStateID == stateID)
            {
                intItem = intItem + 1;
                while (intItem < states.Count())
                {
                    if (states.ElementAt(intItem).IsActive)
                    {
                        intNextStateID = states.ElementAt(intItem).StateID;
                        break;
                    }

                    intItem = intItem + 1;
                }
            }

            // if none found then reset to first state
            if (intNextStateID == -1)
            {
                intNextStateID = this.GetFirstWorkflowStateID(workflow);
            }

            return intNextStateID;
        }

        private void SetWorkflowState(int stateID, ContentItem item)
        {
            item.StateID = stateID;
            this.contentController.UpdateContentItem(item);
        }

        private bool IsWorkflowCompleted(ContentWorkflow workflow, ContentItem item)
        {
            var endStateID = this.GetLastWorkflowStateID(workflow);

            return item.StateID == Null.NullInteger || endStateID == item.StateID;
        }
    }
}
