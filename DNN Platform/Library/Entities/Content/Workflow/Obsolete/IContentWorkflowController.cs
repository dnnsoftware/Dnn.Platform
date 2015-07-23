﻿#region Copyright
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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    [Obsolete("Deprecated in Platform 7.4.0.")]
    public interface IContentWorkflowController
    {
        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void StartWorkflow(int workflowID, int itemID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        bool IsWorkflowCompleted(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        bool IsWorkflowOnDraft(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0.")]
        void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment,
                              int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine")]
        void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId);

        [Obsolete("Deprecated in Platform 7.4.0")]
        string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "");

        [Obsolete("Deprecated in Platform 7.4.0.")]
        ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager")]
        IEnumerable<ContentWorkflow> GetWorkflows(int portalID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager")]
        ContentWorkflow GetDefaultWorkflow(int portalID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager")]
        ContentWorkflow GetWorkflowByID(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager")]
        ContentWorkflow GetWorkflow(ContentItem item);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager")]
        void AddWorkflow(ContentWorkflow workflow);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager")]
        void UpdateWorkflow(ContentWorkflow workflow);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger")]
        IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int workflowId, int contentItemId);

        [Obsolete("Deprecated in Platform 7.4.0.")]
        void DeleteWorkflowLogs(int workflowID, int contentItemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        ContentWorkflowState GetWorkflowStateByID(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        void AddWorkflowState(ContentWorkflowState state);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        void UpdateWorkflowState(ContentWorkflowState state);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lasModifiedByUserId);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager")]
        void DeleteWorkflowStatePermission(int workflowStatePermissionID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsAnyReviewer(int portalID, int userID, int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsAnyReviewer(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsCurrentReviewer(int portalId, int userID, int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsCurrentReviewer(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsReviewer(int portalId, int userID, int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity")]
        bool IsReviewer(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger")]
        void AddWorkflowLog(ContentItem item, string action, string comment, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager")]
        void CreateDefaultWorkflows(int portalId);
    }
}