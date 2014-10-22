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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotNetNuke.Common.Utilities;
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

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowEngine : ServiceLocator<IWorkflowEngine, WorkflowEngine>, IWorkflowEngine
    {
        #region Constants
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";
        private const string ContentWorkflowNotificationNoActionType = "ContentWorkflowNoActionNotification";
        #endregion

        #region Members
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
        #endregion

        #region Constructor
        public WorkflowEngine()
        {
            _contentController = new ContentController();
            _workflowRepository = WorkflowRepository.Instance;
            _workflowStateRepository = WorkflowStateRepository.Instance;
            _workflowStatePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
            _workflowLogRepository = WorkflowLogRepository.Instance;
            _workflowActionManager = WorkflowActionManager.Instance;
            _workflowSecurity = WorkflowSecurity.Instance;
            _userController = UserController.Instance;
            _notificationsController = NotificationsController.Instance;
            _workflowManager = WorkflowManager.Instance;
            _workflowLogger = WorkflowLogger.Instance;
        }
        #endregion

        #region Private Methods
        private void PerformWorkflowActionOnStateChanged(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = GetWorkflowActionInstance(contentItem, actionType);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanged(stateTransaction);
            }
        }

        private void PerformWorkflowActionOnStateChanging(StateTransaction stateTransaction, WorkflowActionTypes actionType)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflowAction = GetWorkflowActionInstance(contentItem, actionType);
            if (workflowAction != null)
            {
                workflowAction.DoActionOnStateChanging(stateTransaction);
            }
        }

        private IWorkflowAction GetWorkflowActionInstance(ContentItem contentItem, WorkflowActionTypes actionType)
        {
            return _workflowActionManager.GetWorkflowActionInstance(contentItem.ContentTypeId, actionType);
        }

        private void UpdateContentItemWorkflowState(int stateId, ContentItem item)
        {
            item.StateID = stateId;
            _contentController.UpdateContentItem(item);
        }

        private UserInfo GetUserThatHaveSubmittedDraftState(Entities.Workflow workflow, int contentItemId)
        {
            var logs = _workflowLogRepository.GetWorkflowLogs(contentItemId, workflow.WorkflowID);

            var logDraftCompleted = logs
                .OrderByDescending(l => l.Date)
                .FirstOrDefault(l => l.Type == (int)WorkflowLogType.DraftCompleted); 

            if (logDraftCompleted != null && logDraftCompleted.User != Null.NullInteger)
            {
                return _userController.GetUserById(workflow.PortalID, logDraftCompleted.User);
            }
            return null;
        }

        #region Notification utilities
        private string GetWorkflowNotificationContext(ContentItem contentItem, WorkflowState state)
        {
            return string.Format("{0}:{1}:{2}", contentItem.ContentItemId, state.WorkflowID, state.StateID);
        }

        private void DeleteWorkflowNotifications(ContentItem contentItem, WorkflowState state)
        {
            var context = GetWorkflowNotificationContext(contentItem, state);
            var notificationTypeId = _notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId;
            var notifications = _notificationsController.GetNotificationByContext(notificationTypeId, context);
            foreach (var notification in notifications)
            {
                _notificationsController.DeleteAllNotificationRecipients(notification.NotificationID);
            }
        }

        private void SendNotificationToAuthor(StateTransaction stateTransaction, WorkflowState state, Entities.Workflow workflow, ContentItem contentItem, WorkflowActionTypes workflowActionType)
        {
            var user = GetUserThatHaveSubmittedDraftState(workflow, contentItem.ContentItemId);
            if (user == null)
            {
                Services.Exceptions.Exceptions.LogException(new WorkflowException(Localization.GetExceptionMessage("WorkflowAuthorNotFound", "Author cannot be found. Notification won't be sent")));
                return;
            }

            if (user.UserID == stateTransaction.UserId)
            {
                return;
            }

            var workflowAction = GetWorkflowActionInstance(contentItem, workflowActionType);
            if (workflowAction == null)
            {
                return;
            }

            var message = workflowAction.GetActionMessage(stateTransaction, state);

            var notification = new Notification
            {
                NotificationTypeID = _notificationsController.GetNotificationType(ContentWorkflowNotificationNoActionType).NotificationTypeId,
                Subject = message.Subject,
                Body = message.Body,
                IncludeDismissAction = true,
                SenderUserID = stateTransaction.UserId
            };

            _notificationsController.SendNotification(notification, workflow.PortalID, null, new []{ user });
        }

        private void SendNotificationsToReviewers(ContentItem contentItem, WorkflowState state, StateTransaction stateTransaction, WorkflowActionTypes workflowActionType, PortalSettings portalSettings)
        {
            if (!state.SendNotification)
            {
                return;
            }

            var permissions = _workflowStatePermissionsRepository.GetWorkflowStatePermissionByState(state.StateID).ToArray();
            var users = GetUsersFromPermissions(portalSettings, permissions, state.SendNotificationToAdministrators).ToArray();
            var roles = GetRolesFromPermissions(portalSettings, permissions, state.SendNotificationToAdministrators).ToArray();

            roles = roles.ToArray();
            users = users.ToArray();

            if (!roles.Any() && !users.Any())
            {
                return; // If there are no receivers, the notification is avoided
            }

            var workflowAction = GetWorkflowActionInstance(contentItem, workflowActionType);
            if (workflowAction == null)
            {
                return;
            }

            var message = workflowAction.GetActionMessage(stateTransaction, state);

            var notification = new Notification
            {
                NotificationTypeID = _notificationsController.GetNotificationType(ContentWorkflowNotificationType).NotificationTypeId,
                Subject = message.Subject,
                Body = message.Body,
                IncludeDismissAction = true,
                SenderUserID = stateTransaction.UserId,
                Context = GetWorkflowNotificationContext(contentItem, state)
            };

            _notificationsController.SendNotification(notification, portalSettings.PortalId, roles.ToList(), users.ToList());
        }
        
        private static IEnumerable<RoleInfo> GetRolesFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions, bool includeAdministrators)
        {
            var roles = (from permission in permissions 
                         where permission.AllowAccess && permission.RoleID > Null.NullInteger 
                         select RoleController.Instance.GetRoleById(settings.PortalId, permission.RoleID)).ToList();

            if (!includeAdministrators)
            {
                return roles;
            }

            if (IsAdministratorRoleAlreadyIncluded(settings, roles))
            {
                return roles;
            }

            var adminRole = RoleController.Instance.GetRoleByName(settings.PortalId, settings.AdministratorRoleName);
            roles.Add(adminRole);
            return roles;
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
        }

        private static IEnumerable<UserInfo> GetUsersFromPermissions(PortalSettings settings, IEnumerable<WorkflowStatePermission> permissions, bool includeAdministrators)
        {
            var users = (from permission in permissions 
                         where permission.AllowAccess && permission.UserID > Null.NullInteger 
                         select UserController.GetUserById(settings.PortalId, permission.UserID)).ToList();

            return includeAdministrators ? IncludeSuperUsers(users) : users;
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
        #endregion

        #region Log Workflow utilities
        private void AddWorkflowCommentLog(ContentItem contentItem, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }
            var state = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }
        private void AddWorkflowCommentLog(ContentItem contentItem, WorkflowState state, int userId, string userComment)
        {
            if (string.IsNullOrEmpty(userComment))
            {
                return;
            }
            AddWorkflowLog(contentItem, state, WorkflowLogType.CommentProvided, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowLogType logType, int userId, string userComment = null)
        {
            var state = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            AddWorkflowLog(contentItem, state, logType, userId, userComment);
        }

        private void AddWorkflowLog(ContentItem contentItem, WorkflowState state, WorkflowLogType logType, int userId, string userComment = null)
        {
            var workflow = _workflowManager.GetWorkflow(contentItem);
            var logTypeText = GetWorkflowActionComment(logType);
            
            var logComment = ReplaceNotificationTokens(logTypeText, workflow, contentItem, state, userId, userComment);

            _workflowLogger.AddWorkflowLog(contentItem.ContentItemId, workflow.WorkflowID, logType, logComment, userId);
        }
        
        private static string GetWorkflowActionComment(WorkflowLogType logType)
        {
            var logName = Enum.GetName(typeof(WorkflowLogType), logType);
            return Localization.GetString(logName + ".Comment");
        }

        private string ReplaceNotificationTokens(string text, Entities.Workflow workflow, ContentItem item, WorkflowState state, int userId, string comment = "")
        {
            var user = _userController.GetUserById(workflow.PortalID, userId);
            var datetime = DateTime.UtcNow;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : "");
            result = result.Replace("[DATE]", datetime.ToString("F", CultureInfo.CurrentCulture));
            result = result.Replace("[STATE]", state != null ? state.StateName : "");
            result = result.Replace("[WORKFLOW]", workflow.WorkflowName);
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : "");
            result = result.Replace("[COMMENT]", !String.IsNullOrEmpty(comment) ? comment : "");
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
        #endregion

        #endregion

        #region Public Methods
        public void StartWorkflow(int workflowId, int contentItemId, int userId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);
            var workflow = _workflowManager.GetWorkflow(contentItem);

            //If already exists a started workflow
            if (workflow != null && !IsWorkflowCompleted(contentItem))
            {
                throw new WorkflowInvalidOperationException(Localization.GetExceptionMessage("WorkflowAlreadyStarted", "Workflow cannot get started for this Content Item. It already has a started workflow."));
            }

            if (workflow == null || workflow.WorkflowID != workflowId)
            {
                workflow = _workflowRepository.GetWorkflow(workflowId);
            }

            UpdateContentItemWorkflowState(workflow.FirstState.StateID, contentItem);

            // Delete previous logs
            _workflowLogRepository.DeleteWorkflowLogs(contentItemId, workflowId);

            // Add logs
            AddWorkflowLog(contentItem, WorkflowLogType.WorkflowStarted, userId);
            AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, userId);
        }

        public void CompleteState(StateTransaction stateTransaction)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = _workflowManager.GetWorkflow(contentItem);
            if (workflow == null || IsWorkflowCompleted(contentItem))
            {
                return;
            }

            var isFirstState = workflow.FirstState.StateID == contentItem.StateID;

            if (!isFirstState && !_workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            var nextState = GetNextWorkflowState(workflow, contentItem.StateID);
            if (nextState.StateID == workflow.LastState.StateID)
            {
                CompleteWorkflow(stateTransaction);
                return;
            }

            // before-change action
            PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteState);
            UpdateContentItemWorkflowState(nextState.StateID, contentItem);

            // Add logs
            AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            AddWorkflowLog(contentItem, currentState,
                currentState.StateID == workflow.FirstState.StateID
                    ? WorkflowLogType.DraftCompleted
                    : WorkflowLogType.StateCompleted, stateTransaction.UserId);
            AddWorkflowLog(contentItem,
                nextState.StateID == workflow.LastState.StateID
                    ? WorkflowLogType.WorkflowApproved
                    : WorkflowLogType.StateInitiated, stateTransaction.UserId);

            // after-change action
            PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteState);
            
            SendNotificationsToReviewers(contentItem, nextState, stateTransaction, WorkflowActionTypes.CompleteState, new PortalSettings(workflow.PortalID));
            
            DeleteWorkflowNotifications(contentItem, currentState);
        }

        public void DiscardState(StateTransaction stateTransaction)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);
            var workflow = _workflowManager.GetWorkflow(contentItem);
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

            if (!isFirstState && !_workflowSecurity.HasStateReviewerPermission(workflow.PortalID, stateTransaction.UserId, contentItem.StateID))
            {
                throw new WorkflowSecurityException(Localization.GetExceptionMessage("UserCannotReviewWorkflowState", "User cannot review the workflow state"));
            }

            var currentState = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            var previousState = GetPreviousWorkflowState(workflow, contentItem.StateID);
            if (previousState.StateID == workflow.LastState.StateID)
            {
                DiscardWorkflow(stateTransaction);
                return;
            }

            // before-change action
            PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardState);

            UpdateContentItemWorkflowState(previousState.StateID, contentItem);

            // Add logs
            AddWorkflowCommentLog(contentItem, currentState, stateTransaction.UserId, stateTransaction.Message.UserComment);
            AddWorkflowLog(contentItem, currentState, WorkflowLogType.StateDiscarded, stateTransaction.UserId);
            AddWorkflowLog(contentItem, WorkflowLogType.StateInitiated, stateTransaction.UserId);

            // after-change action
            PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardState);
            
            if (previousState.StateID == workflow.FirstState.StateID)
            {
                // Send to author - workflow comes back to draft state
                SendNotificationToAuthor(stateTransaction, previousState, workflow, contentItem, WorkflowActionTypes.DiscardState);
            }
            else
            {
                // TODO: replace reviewers notifications
                SendNotificationsToReviewers(contentItem, previousState, stateTransaction, WorkflowActionTypes.DiscardState, new PortalSettings(workflow.PortalID));
            }

            DeleteWorkflowNotifications(contentItem, currentState);
        }
        
        public bool IsWorkflowCompleted(int contentItemId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId);
            return IsWorkflowCompleted(contentItem);
        }

        public bool IsWorkflowCompleted(ContentItem contentItem)
        {
            var workflow = _workflowManager.GetWorkflow(contentItem);
            if (workflow == null) return true; // If item has not workflow, then it is considered as completed

            return contentItem.StateID == Null.NullInteger || workflow.LastState.StateID == contentItem.StateID;
        }

        public bool IsWorkflowOnDraft(int contentItemId)
        {
            var contentItem = _contentController.GetContentItem(contentItemId); //Ensure DB values
            return IsWorkflowOnDraft(contentItem);
        }

        public bool IsWorkflowOnDraft(ContentItem contentItem)
        {
            var workflow = _workflowManager.GetWorkflow(contentItem);
            if (workflow == null) return false; // If item has not workflow, then it is not on Draft
            return contentItem.StateID == workflow.FirstState.StateID;
        }

        public void DiscardWorkflow(StateTransaction stateTransaction)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);

            var currentState = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

            var workflow =_workflowManager.GetWorkflow(contentItem);
            UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            AddWorkflowLog(contentItem, WorkflowLogType.WorkflowDiscarded, stateTransaction.UserId);

            // after-change action
            PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.DiscardWorkflow);

            // Notifications
            SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.DiscardWorkflow);
            DeleteWorkflowNotifications(contentItem, currentState);
        }

        public void CompleteWorkflow(StateTransaction stateTransaction)
        {
            var contentItem = _contentController.GetContentItem(stateTransaction.ContentItemId);

            var currentState = _workflowStateRepository.GetWorkflowStateByID(contentItem.StateID);
            if (currentState.StateID != stateTransaction.CurrentStateId)
            {
                throw new WorkflowConcurrencyException();
            }

            // before-change action
            PerformWorkflowActionOnStateChanging(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

            var workflow = _workflowManager.GetWorkflow(contentItem);
            UpdateContentItemWorkflowState(workflow.LastState.StateID, contentItem);

            // Logs
            AddWorkflowCommentLog(contentItem, stateTransaction.UserId, stateTransaction.Message.UserComment);
            AddWorkflowLog(contentItem, WorkflowLogType.WorkflowApproved, stateTransaction.UserId);

            // after-change action
            PerformWorkflowActionOnStateChanged(stateTransaction, WorkflowActionTypes.CompleteWorkflow);

            // Notifications
            SendNotificationToAuthor(stateTransaction, workflow.LastState, workflow, contentItem, WorkflowActionTypes.CompleteWorkflow);
            DeleteWorkflowNotifications(contentItem, currentState);
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowEngine> GetFactory()
        {
            return () => new WorkflowEngine();
        }
        #endregion
    }
}
