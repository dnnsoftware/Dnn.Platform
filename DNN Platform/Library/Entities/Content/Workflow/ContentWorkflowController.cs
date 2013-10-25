#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

namespace DotNetNuke.Entities.Content.Workflow
{
    public class ContentWorkflowController : ComponentBase<IContentWorkflowController, ContentWorkflowController>, IContentWorkflowController
    {
        private readonly ContentController contentController;
        private const string ContentWorkflowNotificationType = "ContentWorkflowNotification";

        private ContentWorkflowController()
        {            
            contentController = new ContentController();
        }

        #region Public Methods
        public IEnumerable<ContentWorkflow> GetWorkflows(int portalID)
        {
            return CBO.FillCollection<ContentWorkflow>(DataProvider.Instance().GetContentWorkflows(portalID));
        }

        public void StartWorkflow(int workflowID, int itemID, int userID)
        {
            var item = contentController.GetContentItem(itemID);
            var workflow = GetWorkflow(item);

            //If already exists a started workflow
            if (workflow != null && !IsWorkflowCompleted(workflow, item))
            {
                //TODO; Study if is need to throw an exception
                return;
            }
            if(workflow == null)
                workflow = GetWorkflowByID(workflowID);

            //Delete previous logs
            DataProvider.Instance().DeleteContentWorkflowLogs(itemID, workflowID);
            var newStateID = GetFirstWorkflowStateID(workflow);
            SetWorkflowState(newStateID, item);
            AddWorkflowLog(item, ContentWorkflowLogType.WorkflowStarted, userID);            
            AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);
        }
        
        public void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID)
        {
            var item = contentController.GetContentItem(itemID);
            var workflow = GetWorkflow(item);
            if (workflow == null)            
                return;          
  
            if (!IsWorkflowCompleted(workflow, item))
            {
                var currentState = GetWorkflowStateByID(item.StateID);
                if (!String.IsNullOrEmpty(comment))
                {
                    AddWorkflowCommentLog(item, comment, userID);                    
                }
                AddWorkflowLog(item, currentState.StateID == GetFirstWorkflowStateID(workflow) ? ContentWorkflowLogType.DraftCompleted : ContentWorkflowLogType.StateCompleted, userID);

                var endStateID = GetNextWorkflowStateID(workflow, item.StateID);
                SetWorkflowState(endStateID, item);
                if (endStateID == GetLastWorkflowStateID(workflow))
                {
                    AddWorkflowLog(item, ContentWorkflowLogType.WorkflowApproved, userID);
                }
                else
                {
                    AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);                    
                }
            
                SendNotification(new PortalSettings(portalID), workflow, item, currentState, subject, body, comment, endStateID, userID);
            }
        }

        public void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID)
        {
            var item = contentController.GetContentItem(itemID);
            var workflow = GetWorkflow(item);
            if (workflow == null)            
                return;   
         
            
            var currentState = GetWorkflowStateByID(item.StateID);
            if ((GetFirstWorkflowStateID(workflow) != currentState.StateID) && (GetLastWorkflowStateID(workflow) != currentState.StateID))
            {
                if (!String.IsNullOrEmpty(comment))
                {
                    AddWorkflowCommentLog(item, comment, userID);                                    
                }
                AddWorkflowLog(item, ContentWorkflowLogType.StateDiscarded, userID);
                int previousStateID = GetPreviousWorkflowStateID(workflow, item.StateID);
                SetWorkflowState(previousStateID, item);
                AddWorkflowLog(item, ContentWorkflowLogType.StateInitiated, userID);                
                SendNotification(new PortalSettings(portalID), workflow, item, currentState, subject, body, comment, previousStateID, userID);
            }
        }

        public bool IsWorkflowCompleted(int itemID)
        {
            var item = contentController.GetContentItem(itemID); //Ensure DB values
            var workflow = GetWorkflow(item);
            if (workflow == null) return true; // If item has not workflow, then it is considered as completed
            return IsWorkflowCompleted(workflow, item);
        }

        public ContentWorkflow GetDefaultWorkflow(int portalID)
        {
            var wf = GetWorkflows(portalID).First(); // We assume there is only 1 Workflow. This needs to be changed for other scenarios
            wf.States = GetWorkflowStates(wf.WorkflowID);
            return wf;
        }
            
        public bool IsWorkflowOnDraft(int itemID)
        {
            var item = contentController.GetContentItem(itemID); //Ensure DB values
            var workflow = GetWorkflow(item);
            if (workflow == null) return false; // If item has not workflow, then it is not on Draft
            return item.StateID == GetFirstWorkflowStateID(workflow);
        }

        public ContentWorkflow GetWorkflowByID(int workflowID)
        {            
            var workflow = CBO.FillObject<ContentWorkflow>(DataProvider.Instance().GetContentWorkflow(workflowID));
            if (workflow != null)
            {
                workflow.States = GetWorkflowStates(workflowID);
                return workflow;
            }
            return null;
        }

        public ContentWorkflow GetWorkflow(ContentItem item)
        {
            var state = GetWorkflowStateByID(item.StateID);
            if (state == null) return null;
            return GetWorkflowByID(state.WorkflowID);
        }

        public void AddWorkflow(ContentWorkflow workflow)
        {
            var id = DataProvider.Instance().AddContentWorkflow(workflow.PortalID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
            workflow.WorkflowID = id;
        }

        public void UpdateWorkflow(ContentWorkflow workflow)
        {
            DataProvider.Instance().UpdateContentWorkflow(workflow.WorkflowID, workflow.WorkflowName, workflow.Description, workflow.IsDeleted, workflow.StartAfterCreating, workflow.StartAfterEditing, workflow.DispositionEnabled);
        }

        public IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId)
        {            
            return CBO.FillCollection<ContentWorkflowLog>(DataProvider.Instance().GetContentWorkflowLogs(contentItemId, workflowId));
        }

        public void DeleteWorkflowLogs(int contentItemID, int workflowID)
        {
            DataProvider.Instance().DeleteContentWorkflowLogs(contentItemID, workflowID);
        }

        public IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID)
        {
            return CBO.FillCollection<ContentWorkflowState>(DataProvider.Instance().GetContentWorkflowStates(workflowID));            
        }

        public void AddWorkflowState(ContentWorkflowState state)
        {
            var id = DataProvider.Instance().AddContentWorkflowState(state.WorkflowID,
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

        public void UpdateWorkflowState(ContentWorkflowState state)
        {
            DataProvider.Instance().UpdateContentWorkflowState(state.StateID, 
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

        public IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID)
        {
            return CBO.FillCollection<ContentWorkflowStatePermission>(DataProvider.Instance().GetContentWorkflowStatePermissionsByStateID(stateID));
        }

        public void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID)
        {
            DataProvider.Instance().AddContentWorkflowStatePermission(permission.StateID,
                                                                       permission.PermissionID,
                                                                       permission.RoleID,
                                                                       permission.AllowAccess,
                                                                       permission.UserID,
                                                                       lastModifiedByUserID);
        }

        public void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID)
        {            
            DataProvider.Instance().UpdateContentWorkflowStatePermission(permission.WorkflowStatePermissionID, 
                                                                            permission.StateID,
                                                                            permission.PermissionID,
                                                                            permission.RoleID,
                                                                            permission.AllowAccess,
                                                                            permission.UserID,
                                                                            lastModifiedByUserID);       
        }

        public void DeleteWorkflowStatePermission(int workflowStatePermissionID)
        {
            DataProvider.Instance().DeleteContentWorkflowStatePermission(workflowStatePermissionID);
        }

        public bool IsAnyReviewer(int workflowID)
        {
            var workflow = GetWorkflowByID(workflowID);
            return workflow.States.Any(contentWorkflowState => IsReviewer(contentWorkflowState.StateID) );
        }

        public bool IsAnyReviewer(int portalID, int userID, int workflowID)
        {
            var workflow = GetWorkflowByID(workflowID);
            return workflow.States.Any(contentWorkflowState => IsReviewer(portalID, userID, contentWorkflowState.StateID) );
        }
        
        public bool IsReviewer(int stateID)
        {
            var permissions = GetWorkflowStatePermissionByState(stateID);
            var user = UserController.GetCurrentUserInfo();
            return IsReviewer(user, PortalSettings.Current, permissions);
        }

        public bool IsReviewer(int portalID, int userID, int stateID)
        {
            var permissions = GetWorkflowStatePermissionByState(stateID);
            var user = UserController.GetUserById(portalID, userID);
            var portalSettings = new PortalSettings(portalID);

            return IsReviewer(user, portalSettings, permissions);
        }

        public bool IsCurrentReviewer(int portalID, int userID, int itemID)
        {
            var item = contentController.GetContentItem(itemID);
            return IsReviewer(portalID, userID, item.StateID);
        }

        public bool IsCurrentReviewer(int itemID)
        {
            var item = contentController.GetContentItem(itemID);
            return IsReviewer(item.StateID);
        }

        public ContentWorkflowState GetWorkflowStateByID(int stateID)
        {            
            return CBO.FillObject<ContentWorkflowState>(DataProvider.Instance().GetContentWorkflowState(stateID));
        }

        public void AddWorkflowLog(ContentItem item, string action, string comment, int userID)
        {
            var workflow = GetWorkflow(item);
            
            AddWorkflowLog(workflow != null ? workflow.WorkflowID: Null.NullInteger, item, action, comment, userID);
        }

        public void CreateDefaultWorkflows(int portalId)
        {
            if(GetWorkflows(portalId).Any(w => w.WorkflowName == Localization.GetString("DefaultWorkflowName")))
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
                                               }
                              };

            AddWorkflow(worflow);
            foreach (var state in worflow.States)
            {
                state.WorkflowID = worflow.WorkflowID;
                AddWorkflowState(state);
            }
        }

        public void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body,
                                             string comment, int userID)
        {
            var replacedSubject = ReplaceNotificationTokens(subject, null, null, null, settings.PortalId, userID);
            var replacedBody = ReplaceNotificationTokens(body, null, null, null, settings.PortalId, userID);
            SendNotification(sendEmail, sendMessage, settings, roles, users, replacedSubject, replacedBody, comment, userID);
        }

        public void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId)
        {
            var item = contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);
            var stateId = GetLastWorkflowStateID(workflow);
            AddWorkflowCommentLog(item, comment, userId);
            AddWorkflowLog(item, ContentWorkflowLogType.WorkflowDiscarded, userId);         
            SetWorkflowState(stateId, item);
        }

        public void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId)
        {
            var item = contentController.GetContentItem(contentItemId);
            var workflow = GetWorkflow(item);            
            var lastStateId = GetLastWorkflowStateID(workflow);
            AddWorkflowCommentLog(item, comment, userId);
            AddWorkflowLog(item, ContentWorkflowLogType.WorkflowApproved, userId);
            SetWorkflowState(lastStateId, item);            
        }

        public string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "")
        {
            var user = UserController.GetUserById(portalID, userID);
            var datetime = DateTime.Now;
            var result = text.Replace("[USER]", user != null ? user.DisplayName : "");
            result = result.Replace("[DATE]", datetime.ToString("d-MMM-yyyy hh:mm") + datetime.ToString("tt").ToLower());
            result = result.Replace("[STATE]", state != null ? state.StateName : "");
            result = result.Replace("[WORKFLOW]", workflow != null ? workflow.WorkflowName : "");
            result = result.Replace("[CONTENT]", item != null ? item.ContentTitle : "");
            result = result.Replace("[COMMENT]", !String.IsNullOrEmpty(comment) ? comment : "");

            return result;
        }
        #endregion

        #region Private Methods
        private void AddWorkflowCommentLog(ContentItem item, string userComment, int userID)
        {
            var workflow = GetWorkflow(item);

            var logComment = ReplaceNotificationTokens(GetWorkflowActionComment(ContentWorkflowLogType.CommentProvided), workflow, item, workflow.States.FirstOrDefault(s => s.StateID == item.StateID), workflow.PortalID, userID, userComment);            
            AddWorkflowLog(workflow.WorkflowID, item, GetWorkflowActionText(ContentWorkflowLogType.CommentProvided), logComment, userID);
        }

        private void SendNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment, int userID)
        {
            if (sendEmail)
            {
                SendEmailNotifications(settings, roles, users, subject, body, comment);
            }
            if (sendMessage)
            {
                SendMessageNotifications(settings, roles, users, subject, body, comment, userID);
            }
        }

        private void SendNotification(PortalSettings settings, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, string subject, string body, string comment, int destinationStateID, int actionUserID)
        {
            var permissions = GetWorkflowStatePermissionByState(destinationStateID);
            var users = GetUsersFromPermissions(settings, permissions);
            var roles = GetRolesFromPermissions(settings, permissions);
            var replacedSubject = ReplaceNotificationTokens(subject, workflow, item, GetWorkflowStateByID(destinationStateID), settings.PortalId, actionUserID);
            var replacedBody = ReplaceNotificationTokens(body, workflow, item, GetWorkflowStateByID(destinationStateID), settings.PortalId, actionUserID);
            
            SendNotification(state.SendEmail, state.SendMessage, settings, roles, users, replacedSubject, replacedBody, comment, actionUserID);
        }

        private void SendMessageNotifications(PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment, int actionUserID)
        {
            //TODO: Confirm the final body and comment format
            var fullbody = GetFullBody(body, comment);

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
                SenderUserID = actionUserID
            };

            NotificationsController.Instance.SendNotification(notification, settings.PortalId, roles.ToList(), users.ToList());
        }

        private string GetFullBody(string body, string comment)
        {
            return body + "<br><br>" + comment;
        }

        private void SendEmailNotifications(PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment)
        {
            //TODO: Confirm the final body and comment format
            var fullbody = GetFullBody(body, comment);
            var roleController = new RoleController();
            var emailUsers = users.ToList();
            foreach (var role in roles)
            {
                var roleUsers = roleController.GetUsersByRoleName(settings.PortalId, role.RoleName);
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
            var roleController = new RoleController();
            
            foreach (var permission in permissions)
            {
                if (permission.AllowAccess && permission.RoleID > Null.NullInteger)
                {
                    roles.Add(roleController.GetRole(permission.RoleID, settings.PortalId));
                }
            }

            if(!IsAdministratorRoleAlreadyIncluded(settings, roles))
            {
                var adminRole = roleController.GetRoleByName(settings.PortalId, settings.AdministratorRoleName);
                roles.Add(adminRole);
            }
            return roles;
        }

        private static bool IsAdministratorRoleAlreadyIncluded(PortalSettings settings, IEnumerable<RoleInfo> roles)
        {
            return roles.Any(r => r.RoleName == settings.AdministratorRoleName);
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

        private static IEnumerable<UserInfo> IncludeSuperUsers(ICollection<UserInfo> users)
        {
            var superUsers = UserController.GetUsers(false, true, Null.NullInteger);
            foreach (UserInfo superUser in superUsers)
            {
                if(IsSuperUserNotIncluded(users, superUser))
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

        private bool IsReviewer(UserInfo user, PortalSettings settings, IEnumerable<ContentWorkflowStatePermission> permissions)
        {
            var administratorRoleName = settings.AdministratorRoleName;
            return user.IsSuperUser || PortalSecurity.IsInRoles(user, settings, administratorRoleName) || PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), "REVIEW"));
        }

        private void AddWorkflowLog(ContentItem item, ContentWorkflowLogType logType, int userID)
        {
            var workflow = GetWorkflow(item);

            var comment = ReplaceNotificationTokens(GetWorkflowActionComment(logType), workflow, item, workflow.States.FirstOrDefault(s => s.StateID == item.StateID), workflow.PortalID, userID);

            AddWorkflowLog(workflow.WorkflowID, item, GetWorkflowActionText(logType), comment, userID);
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
                intPreviousStateID = GetFirstWorkflowStateID(workflow);
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
                intNextStateID = GetFirstWorkflowStateID(workflow);
            }

            return intNextStateID;
        }

        private void SetWorkflowState(int stateID, ContentItem item)
        {
            item.StateID = stateID;
            contentController.UpdateContentItem(item);
        }

        private bool IsWorkflowCompleted(ContentWorkflow workflow, ContentItem item)
        {
            var endStateID = GetLastWorkflowStateID(workflow);

            return item.StateID == Null.NullInteger || endStateID == item.StateID;
        }

        #endregion
    }
}
