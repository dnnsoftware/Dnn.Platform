// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Workflow.Actions;
    using DotNetNuke.Entities.Content.Workflow.Dto;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Content.Workflow.Exceptions;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Notifications;

    public class WorkflowEngine : ServiceLocator<IWorkflowEngine, WorkflowEngine>, IWorkflowEngine
    {
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";
        private const string ContentWorkflowNotificationNoActionType = "ContentWorkflowNoActionNotification";
        private const string ContentWorkflowNotificatioStartWorkflowType = "ContentWorkflowStartWorkflowNotification";
        private readonly IContentController _contentController;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowStateRepository _workflowStateRepository;
        private readonly IWorkflowStatePermissionsRepository _workflowStatePermissionsRepository;
        private readonly IWorkflowLogRepository _workflowLogRepository;
        private readonly IWorkflowActionManager _workflowActionManager;
        private readonly IUserController _userController;
        private readonly IWorkflowSecurity _workflowSecurity;
        private readonly INotificationsController _notificationsController;
        private readonly IWorkflowManager _workflowManager;
        private readonly IWorkflowLogger _workflowLogger;
        private readonly ISystemWorkflowManager _systemWorkflowManager;

        public WorkflowEngine()
        {
            this._contentController = Util.GetContentController();
            this._workflowRepository = WorkflowRepository.Instance;
            this._workflowStateRepository = WorkflowStateRepository.Instance;
            this._workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
            this._workflowLogRepository = WorkflowLogRepository.Instance;
            this._workflowActionManager = WorkflowActionManager.Instance;
            this._workflowSecurity = WorkflowSecurity.Instance;
            this._userController = UserController.Instance;
            this._notificationsController = NotificationsController.Instance;
            this._workflowManager = WorkflowManager.Instance;
            this._workflowLogger = WorkflowLogger.Instance;
            this._systemWorkflowManager = SystemWorkflowManager.Instance;
        }

        public UserInfo GetStartedDraftStateUser(ContentItem contentItem)
        {
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.WorkflowStarted);
        }

        public UserInfo GetSubmittedDraftStateUser(ContentItem contentItem)
        {
            return this.GetUserByWorkflowLogType(contentItem, WorkflowLogType.DraftCompleted);
        }

        public void StartWorkflow(int workflowId, int contentItemId, int userId)
        {
            Requires.NotNegative("workflowId", workflowId);

            var contentItem = this._contentController.GetContentItem(contentItemId);
            var workflow = this._workflowManager.GetWorkflow(contentItem);

            // If already exists a started workflow
            if (workflow != null && !this.IsWorkflowCompleted(contentItem))
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowAlreadyStarted", "Workflow cannot get started for this Content Item. It already has a started workflow."));
            }

            if (workflow == null || workflow.WorkflowID != workflowId)
            {
                workflow = this._workflowRepository.GetWorkflow(workflowId);
            }

            var initialTransaction = this.CreateInitialTransaction(contentItemId, workflow.FirstState.StateID, userId);

            // Perform action before starting workflow
            this.PerformWorkflowActionOnStateChanging(initialTransaction, WorkflowActionTypes.StartWorkflow);
            this.UpdateContentItemWorkflowState(workflow.FirstState.StateID, contentItem);

            // Send notifications to stater
            if (workflow.WorkflowID != this._systemWorkflowManager.GetDirectPublishWorkflow(workflow.PortalID).WorkflowID) // This notification is not sent in Direct Publish WF
            {
                this.SendNotificationToWorkflowStarter(initialTransaction, workflow, contentItem, userId, WorkflowActionTypes.StartWorkflow);
            }

            // Delete previous logs
            this._workflowLogRepository.DeleteWorkflowLogs(contentItemId, workflowId);

            // Add logs
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowStarted, userId);
            this.AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, userId);

            // Perform action after starting workflow
            this.PerformWorkflowActionOnStateChanged(initialTransaction, WorkflowActionTypes.StartWorkflow);
        }

        public void CompleteState(StateTransaction stateTransaction)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this._workflowManager.GetWorkflow(contentItem);

            if (workflow == null)
            {
                return;
            }

            if (this.IsWorkflowCompleted(contentItem)
                        && !(workflow.IsSystem && workflow.States.Count() == 1))
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowSystemWorkflowStateCannotComplete", "System workflow state cannot be completed."));
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;

            if (!isFirstState && !this._workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            var nextState = this.GetNextWorkflowState(workflow, contentItem.StateID);
            if (nextState.StateID == workflow.LastState.StateID)
            {
                this.CompleteWorkflow(stateTransaction);
                return;
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteState);
            this.UpdateContentItemWorkflowState(nextState.StateID, contentItem);

            // Add logs
            this.AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, currentState,
                currentState.StateID == workflow.FirstState.StateID
                    ? WorkflowLogType.DraftCompleted
                    : WorkflowLogType.StateCompleted, stateTransaction.UserId);
            this.AddWorkflowLog(
                contentItem,
                nextState.StateID == workflow.LastState.StateID
                    ? WorkflowLogType.WorkflowApproved
                    : WorkflowLogType.StateInitiated, stateTransaction.UserId);

            this.SendNotificationsToReviewers(contentItem, nextState, stateTransaction, WorkflowActionTypes.CompleteState, new PortalSettings(workflow.PortalID));

            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteState);
        }

        public void DiscardState(StateTransaction stateTransaction)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = this._workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return;
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;
            var isLastState = workflow.LastState.StateID == contentItem.StateID;

            if (isLastState)
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowCannotDiscard", "Cannot discard on last workflow state"));
            }

            if (!isFirstState && !this._workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            var previousState = this.GetPreviousWorkflowState(workflow, contentItem.StateID);
            if (previousState.StateID == workflow.LastState.StateID)
            {
                this.DiscardWorkflow(stateTransaction);
                return;
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardState);

            this.UpdateContentItemWorkflowState(previousState.StateID, contentItem);

            // Add logs
            this.AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, currentState, WorkflowLogType.StateDiscarded, stateTransaction.UserId);
            this.AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, stateTransaction.UserId);

            if (previousState.StateID == workflow.FirstState.StateID)
            {
                // Send to author - workflow comes back to draft state
                this.SendNotificationToAuthor(stateTransaction, previousState, workflow, contentItem, WorkflowActionTypes.DiscardState);
            }
            else
            {
                this.SendNotificationsToReviewers(contentItem, previousState, stateTransaction, WorkflowActionTypes.DiscardState, new PortalSettings(workflow.PortalID));
            }

            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardState);
        }

        public bool IsWorkflowCompleted(int contentItemId)
        {
            var contentItem = this._contentController.GetContentItem(contentItemId);
            return this.IsWorkflowCompleted(contentItem);
        }

        public bool IsWorkflowCompleted(ContentItem contentItem)
        {
            var workflow = this._workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return true; // If item has not workflow, then it is considered as completed
            }

            return contentItem.StateID == Null.NullInteger || workflow.LastState.StateID == contentItem.StateID;
        }

        public bool IsWorkflowOnDraft(int contentItemId)
        {
            var contentItem = this._contentController.GetContentItem(contentItemId); // Ensure DB values
            return this.IsWorkflowOnDraft(contentItem);
        }

        public bool IsWorkflowOnDraft(ContentItem contentItem)
        {
            var workflow = this._workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return false; // If item has not workflow, then it is not on Draft
            }

            return contentItem.StateID == workflow.FirstState.StateID;
        }

        public void DiscardWorkflow(StateTransaction stateTransaction)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);

            var currentState = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

            var workflow = this._workflowManager.GetWorkflow(contentItem);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowDiscarded, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.DiscardWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardWorkflow);
        }

        public void CompleteWorkflow(StateTransaction stateTransaction)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);

            var currentState = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            this.PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

            var workflow = this._workflowManager.GetWorkflow(contentItem);
            this.UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            this.AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            this.AddWorkflowLog(contentItem, WorkflowLogType.WorkflowApproved, stateTransaction.UserId);

            // Notifications
            this.SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.CompleteWorkflow);
            this.DeleteWorkflowNotifications(contentItem, currentState);

            // after-change action
            this.PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteWorkflow);
        }

        protected override Func<IWorkflowEngine> GetFactory()
        {
            return () => new WorkflowEngine();
        }

        private static List<RoleInfo> GetRolesFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
            return (from permission in permissions
                    where permission.AllowAccess && permission.RoleID > Null.NullInteger
                    select RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleID)).ToList();
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
        }

        private static List<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions)
        {
            return (from permission in permissions
                    where permission.AllowAccess && permission.UserID > Null.NullInteger
                    select UserController.GetUserById(settings.PortalId, permission.UserID)).ToList();
        }

        private static List<UserInfo> IncludeSuperUsers(ICollection<UserInfo> users)
        {
            var superUsers = UserController.GetUsers(false, true, Null.NullInteger);
            foreach (UserInfo superUser in superUsers)
            {
                if (IsSuperUserNotIncluded(users, superUser))
                {
                    users.Add(superUser);
                }
            }

            return users.ToList();
        }

        private static bool IsSuperUserNotIncluded(IEnumerable<UserInfo> users, UserInfo superUser)
        {
            return users.All(u => u.UserID != superUser.UserID);
        }

        private static string GetWorkflowActionComment(WorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
            return Localization.GetString(logName + ".Comment");
        }

        private StateTransaction CreateInitialTransaction(int contentItemId, int stateId, int userId)
        {
            return new StateTransaction
            {
                ContentItemId = contentItemId,
                CurrentStateId = stateId,
                UserId = userId,
                Message = new StateTransactionMessage(),
            };
        }

        private void PerformWorkflowActionOnStateChanged(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanged(stateTransaction);
            }
        }

        private void PerformWorkflowActionOnStateChanging(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
            var contentItem = this._contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = this.GetWorkflowActionInstance(contentItem, actionType);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanging(stateTransaction);
            }
        }

        private IWorkflowAction GetWorkflowActionInstance(ContentItem contentItem, WorkflowActionTypes actionType)
        {
            return this._workflowActionManager.GetWorkflowActionInstance(contentItem.ContentTypeId, actionType);
        }

        private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
        {
            item.StateID = stateId;
            this._contentController.UpdateContentItem(item);
        }

        private UserInfo GetUserThatHaveStartedOrSubmittedDraftState(Entities.Workflow workflow, ContentItem contentItem, StateTransaction stateTransaction)
        {
            bool wasDraftSubmitted = this.WasDraftSubmitted(workflow, stateTransaction.CurrentStateId);
            if (wasDraftSubmitted)
            {
                return this.GetSubmittedDraftStateUser(contentItem);
            }

            return this.GetStartedDraftStateUser(contentItem);
        }

        private UserInfo GetUserByWorkflowLogType(ContentItem contentItem, WorkflowLogType type)
        {
            var workflow = this._workflowManager.GetWorkflow(contentItem);
            if (workflow == null)
            {
                return null;
            }

            var logs = this._workflowLogRepository.GetWorkflowLogs(contentItem.ContentItemId, workflow.WorkflowID);

            var logDraftCompleted = logs
                .OrderByDescending(l => l.Date)
                .FirstOrDefault(l => l.Type == (int)type);

            if (logDraftCompleted != null && logDraftCompleted.User != Null.NullInteger)
            {
                return this._userController.GetUserById(workflow.PortalID, logDraftCompleted.User);
            }

            return null;
        }

        private bool WasDraftSubmitted(Entities.Workflow workflow, int currentStateId)
        {
            var isDirectPublishWorkflow = workflow.IsSystem && workflow.States.Count() == 1;
            var draftSubmitted = workflow.FirstState.StateID != currentStateId;
            return !(isDirectPublishWorkflow || !draftSubmitted);
        }

        private string GetWorkflowNotificationContext(ContentItem contentItem, WorkflowState state)
        {
            return string.Format("{0}:{1}:{2}", contentItem.ContentItemId, state.WorkflowID, state.StateID);
        }

        private void DeleteWorkflowNotifications(ContentItem contentItem, WorkflowState state)
        {
            var context = this.GetWorkflowNotificationContext(contentItem, state);
            var notificationTypeId = this._notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId;
            this.DeleteNotificationsByType(notificationTypeId, context);
            notificationTypeId = this._notificationsController.GetNotificationType(ContentWorkflowNotificatioStartWorkflowType).NotificationTypeId;
            this.DeleteNotificationsByType(notificationTypeId, context);
        }

        private void DeleteNotificationsByType(int notificationTypeId, string context)
        {
            var notifications = this._notificationsController.GetNotificationByContext(notificationTypeId, context);
            foreach (var notification in notifications)
            {
                this._notificationsController.DeleteAllNotificationRecipients(notification.NotificationID);
            }
        }

        private void SendNotificationToAuthor(StateTransaction stateTransaction, WorkflowState state, Entities.Workflow workflow, ContentItem contentItem, WorkflowActionTypes workflowActionType)
        {
            try
            {
                if (!state.SendNotification)
                {
                    return;
                }

                var user = this.GetUserThatHaveStartedOrSubmittedDraftState(workflow, contentItem, stateTransaction);
                if (user == null)
                {
                    // Services.Exceptions.Exceptions.LogException(new WorkflowException(Localization.GetExceptionMessage("WorkflowAuthorNotFound", "Author cannot be found. Notification won't be sent")));
                    return;
                }

                if (user.UserID == stateTransaction.UserId)
                {
                    return;
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);

                var notification = this.GetNotification(this.GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationNoActionType);

                this._notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void SendNotificationToWorkflowStarter(StateTransaction stateTransaction, Entities.Workflow workflow, ContentItem contentItem, int starterUserId, WorkflowActionTypes workflowActionType)
        {
            try
            {
                if (!workflow.FirstState.SendNotification)
                {
                    return;
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
                if (workflowAction == null)
                {
                    return;
                }

                var user = this._userController.GetUser(workflow.PortalID, starterUserId);

                var message = workflowAction.GetActionMessage(stateTransaction, workflow.FirstState);

                var notification = this.GetNotification(this.GetWorkflowNotificationContext(contentItem, workflow.FirstState), stateTransaction, message, ContentWorkflowNotificatioStartWorkflowType);

                this._notificationsController.SendNotification(notification, workflow.PortalID, null, new[] { user });
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void SendNotificationsToReviewers(ContentItem contentItem, WorkflowState state, StateTransaction stateTransaction, WorkflowActionTypes workflowActionType, PortalSettings portalSettings)
        {
            try
            {
                if (!state.SendNotification && !state.SendNotificationToAdministrators)
                {
                    return;
                }

                var reviewers = this.GetUserAndRolesForStateReviewers(portalSettings, state);

                if (!reviewers.Roles.Any() && !reviewers.Users.Any())
                {
                    return; // If there are no receivers, the notification is avoided
                }

                var workflowAction = this.GetWorkflowActionInstance(contentItem, workflowActionType);
                if (workflowAction == null)
                {
                    return;
                }

                var message = workflowAction.GetActionMessage(stateTransaction, state);

                var notification = this.GetNotification(this.GetWorkflowNotificationContext(contentItem, state), stateTransaction, message, ContentWorkflowNotificationType);

                this._notificationsController.SendNotification(notification, portalSettings.PortalId, reviewers.Roles.ToList(), reviewers.Users.ToList());
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private Notification GetNotification(string workflowContext, StateTransaction stateTransaction,
            ActionMessage message, string notificationType)
        {
            var notification = new Notification
            {
                NotificationTypeID =
                    this._notificationsController.GetNotificationType(notificationType).NotificationTypeId,
                Subject = message.Subject,
                Body = message.Body,
                IncludeDismissAction = true,
                SenderUserID = stateTransaction.UserId,
                Context = workflowContext,
                SendToast = message.SendToast,
            };
            return notification;
        }

        private ReviewersDto GetUserAndRolesForStateReviewers(PortalSettings portalSettings, WorkflowState state)
        {
            var reviewers = new ReviewersDto
            {
                Roles = new List<RoleInfo>(),
                Users = new List<UserInfo>(),
            };
            if (state.SendNotification)
            {
                var permissions = this._workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(state.StateID).ToArray();
                reviewers.Users = GetUsersFromPermissions(portalSettings, permissions);
                reviewers.Roles = GetRolesFromPermissions(portalSettings, permissions);
            }

            if (state.SendNotificationToAdministrators)
            {
                if (!IsAdministratorRoleAlreadyIncluded(portalSettings, reviewers.Roles))
                {
                    var adminRole = RoleController.Instance.GetRoleByName(portalSettings.PortalId, portalSettings.AdministratorRoleName);
                    reviewers.Roles.Add(adminRole);
                }

                reviewers.Users = IncludeSuperUsers(reviewers.Users);
            }

            return reviewers;
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

            var state = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowCommentLog(ContentItem contentItem, WorkflowState state, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }

            this.AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowLogType logType, int userId, string userComment = null)
        {
            var state = this._workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            this.AddWorkflowLog(contentItem, state, logType, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment = null)
        {
            try
            {
                this.TryAddWorkflowLog(contentItem, state, logType, userId, userComment);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        private void TryAddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment)
        {
            var workflow = this._workflowManager.GetWorkflow(contentItem);
            var logTypeText = GetWorkflowActionComment(logType);
            var logComment = this.ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);
            this._workflowLogger.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, logType, logComment, userId);
        }

        private string ReplaceNotificationTokens(string text, Entities.Workflow workflow, ContentItem item, WorkflowState state, int userId, string comment = "")
        {
            var user = this._userController.GetUserById(workflow.PortalID, userId);
            var datetime = DateTime.UtcNow;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : string.Empty);
            result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
            result = result.Replace("[STATE]", state != null ? state.StateName : string.Empty);
            result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : string.Empty);
            result = result.Replace("[COMMENT]", !string.IsNullOrEmpty(comment) ? comment : string.Empty);
            return result;
        }

        private WorkflowState GetNextWorkflowState(Entities.Workflow workflow, int stateId)
        {
            WorkflowState nextState = null;
            var states = workflow.States.OrderBy(s => s.Order);
            int index;

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    break;
                }
            }

            index = index + 1;
            if (index < states.Count())
            {
                nextState = states.ElementAt(index);
            }

            return nextState ?? workflow.FirstState;
        }

        private WorkflowState GetPreviousWorkflowState(Entities.Workflow workflow, int stateId)
        {
            WorkflowState previousState = null;
            var states = workflow.States.OrderBy(s => s.Order);
            int index;

            if (workflow.FirstState.StateID == stateId)
            {
                return workflow.LastState;
            }

            // locate the current state
            for (index = 0; index < states.Count(); index++)
            {
                if (states.ElementAt(index).StateID == stateId)
                {
                    previousState = states.ElementAt(index - 1);
                    break;
                }
            }

            return previousState ?? workflow.LastState;
        }

        private class ReviewersDto
        {
            public List<RoleInfo> Roles { get; set; }

            public List<UserInfo> Users { get; set; }
        }
    }
}
