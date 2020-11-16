// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;

    [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
    public interface IContentWorkflowController
    {
        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void StartWorkflow(int workflowID, int itemID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        bool IsWorkflowCompleted(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        bool IsWorkflowOnDraft(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
        void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment,
                              int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowEngine. Scheduled removal in v10.0.0.")]
        void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId);

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "");

        [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
        ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        IEnumerable<ContentWorkflow> GetWorkflows(int portalID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager. Scheduled removal in v10.0.0.")]
        ContentWorkflow GetDefaultWorkflow(int portalID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        ContentWorkflow GetWorkflowByID(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        ContentWorkflow GetWorkflow(ContentItem item);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        void AddWorkflow(ContentWorkflow workflow);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowManager. Scheduled removal in v10.0.0.")]
        void UpdateWorkflow(ContentWorkflow workflow);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger. Scheduled removal in v10.0.0.")]
        IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int workflowId, int contentItemId);

        [Obsolete("Deprecated in Platform 7.4.0.. Scheduled removal in v10.0.0.")]
        void DeleteWorkflowLogs(int workflowID, int contentItemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        ContentWorkflowState GetWorkflowStateByID(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        void AddWorkflowState(ContentWorkflowState state);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        void UpdateWorkflowState(ContentWorkflowState state);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        IEnumerable<ContentWorkflowStatePermission> GetWorkflowStatePermissionByState(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        void AddWorkflowStatePermission(ContentWorkflowStatePermission permission, int lastModifiedByUserID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        void UpdateWorkflowStatePermission(ContentWorkflowStatePermission permission, int lasModifiedByUserId);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowStateManager. Scheduled removal in v10.0.0.")]
        void DeleteWorkflowStatePermission(int workflowStatePermissionID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsAnyReviewer(int portalID, int userID, int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsAnyReviewer(int workflowID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsCurrentReviewer(int portalId, int userID, int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsCurrentReviewer(int itemID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsReviewer(int portalId, int userID, int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowSecurity. Scheduled removal in v10.0.0.")]
        bool IsReviewer(int stateID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger. Scheduled removal in v10.0.0.")]
        void AddWorkflowLog(ContentItem item, string action, string comment, int userID);

        [Obsolete("Deprecated in Platform 7.4.0. Use instead ISystemWorkflowManager. Scheduled removal in v10.0.0.")]
        void CreateDefaultWorkflows(int portalId);
    }
}
