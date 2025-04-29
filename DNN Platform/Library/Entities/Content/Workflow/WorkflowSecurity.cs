// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow;

using System;
using System.Linq;

using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

public class WorkflowSecurity : ServiceLocator<IWorkflowSecurity, WorkflowSecurity>, IWorkflowSecurity
{
    private const string ReviewPermissionKey = "REVIEW";
    private const string ReviewPermissionCode = "SYSTEM_CONTENTWORKFLOWSTATE";
    private readonly IUserController userController = UserController.Instance;
    private readonly IWorkflowManager workflowManager = WorkflowManager.Instance;
    private readonly IWorkflowStatePermissionsRepository statePermissionsRepository = WorkflowStatePermissionsRepository.Instance;

    /// <inheritdoc/>
    public bool HasStateReviewerPermission(PortalSettings settings, UserInfo user, int stateId)
    {
        var permissions = this.statePermissionsRepository.GetWorkflowStatePermissionByState(stateId);

        return user.IsSuperUser ||
               PortalSecurity.IsInRoles(user, settings, settings.AdministratorRoleName) ||
               PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), ReviewPermissionKey));
    }

    /// <inheritdoc/>
    public bool HasStateReviewerPermission(int portalId, int userId, int stateId)
    {
        var user = this.userController.GetUserById(portalId, userId);
        var portalSettings = new PortalSettings(portalId);
        return this.HasStateReviewerPermission(portalSettings, user, stateId);
    }

    /// <inheritdoc/>
    public bool HasStateReviewerPermission(int stateId)
    {
        var user = this.userController.GetCurrentUserInfo();
        return this.HasStateReviewerPermission(PortalSettings.Current, user, stateId);
    }

    /// <inheritdoc/>
    public bool IsWorkflowReviewer(int workflowId, int userId)
    {
        var workflow = this.workflowManager.GetWorkflow(workflowId);
        return workflow.States.Any(contentWorkflowState => this.HasStateReviewerPermission(workflow.PortalID, userId, contentWorkflowState.StateID));
    }

    /// <inheritdoc/>
    public PermissionInfo GetStateReviewPermission()
    {
        return (PermissionInfo)new PermissionController().GetPermissionByCodeAndKey(ReviewPermissionCode, ReviewPermissionKey)[0];
    }

    /// <inheritdoc/>
    protected override Func<IWorkflowSecurity> GetFactory()
    {
        return () => new WorkflowSecurity();
    }
}
