// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow;

// ReSharper enable CheckNamespace
using System.Collections.Generic;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Security.Roles;

[DnnDeprecated(7, 4, 0, "Use IWorkflowEngine", RemovalVersion = 10)]
public partial interface IContentWorkflowController
{
    void StartWorkflow(int workflowID, int itemID, int userID);

    void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID);

    void CompleteState(int itemID, string subject, string body, string comment, int portalID, int userID, string source, params string[] parameters);

    void DiscardState(int itemID, string subject, string body, string comment, int portalID, int userID);

    bool IsWorkflowCompleted(int itemID);

    bool IsWorkflowOnDraft(int itemID);

    void SendWorkflowNotification(bool sendEmail, bool sendMessage, PortalSettings settings, IEnumerable<RoleInfo> roles, IEnumerable<UserInfo> users, string subject, string body, string comment, int userID);

    void DiscardWorkflow(int contentItemId, string comment, int portalId, int userId);

    void CompleteWorkflow(int contentItemId, string comment, int portalId, int userId);

    string ReplaceNotificationTokens(string text, ContentWorkflow workflow, ContentItem item, ContentWorkflowState state, int portalID, int userID, string comment = "");

    ContentWorkflowSource GetWorkflowSource(int workflowId, string sourceName);

    IEnumerable<ContentWorkflow> GetWorkflows(int portalID);

    ContentWorkflow GetDefaultWorkflow(int portalID);

    ContentWorkflow GetWorkflowByID(int workflowID);

    ContentWorkflow GetWorkflow(ContentItem item);

    void AddWorkflow(ContentWorkflow workflow);

    void UpdateWorkflow(ContentWorkflow workflow);

    IEnumerable<ContentWorkflowLog> GetWorkflowLogs(int workflowId, int contentItemId);

    void DeleteWorkflowLogs(int workflowID, int contentItemID);

    IEnumerable<ContentWorkflowState> GetWorkflowStates(int workflowID);

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
}
