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

using System.Collections.Generic;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;

namespace DotNetNuke.Entities.Content.Workflow
{
    public interface IContentWorkflowController
    {
        IEnumerable<ContentWorkflow> GetWorkflows(int portalID);

        void StartWorkflow(int workflowID, int itemID, int userID);

        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID);

        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters);

        void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID);

        bool IsWorkflowCompleted(int itemID);

        ContentWorkflow GetDefaultWorkflow(int portalID);

        bool IsWorkflowOnDraft(int itemID);

        ContentWorkflow GetWorkflowByID(int workflowID);

        ContentWorkflow GetWorkflow(ContentItem item);

        void AddWorkflow(ContentWorkflow workflow);

        void UpdateWorkflow(ContentWorkflow workflow);

        IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int workflowId, int contentItemId);

        void DeleteWorkflowLogs(int workflowID, int contentItemID);

        IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID);

        ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName);

        ContentWorkflowState GetWorkflowStateByID(int stateID);

        void AddWorkflowState(ContentWorkflowState state);

        void UpdateWorkflowState(ContentWorkflowState state);

        IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID);

        void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID);

        void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lasModifiedByUserId);

        void DeleteWorkflowStatePermission(int workflowStatePermissionID);

        bool IsAnyReviewer(int portalID, int userID, int workflowID);

        bool IsAnyReviewer(int workflowID);

        bool IsCurrentReviewer(int portalId, int userID, int itemID);

        bool IsCurrentReviewer(int itemID);

        bool IsReviewer(int portalId, int userID, int stateID);

        bool IsReviewer(int stateID);

        void AddWorkflowLog(ContentItem item, string action, string comment, int userID);

        void CreateDefaultWorkflows(int portalId);

        void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment,
                              int userID);

        void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId);

        void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId);

        string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "");
    }
}