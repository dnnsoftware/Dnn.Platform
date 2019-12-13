// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Linq;
using DotNetNuke.Entities.Content.Workflow.Repositories;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Entities.Content.Workflow
{
    public class WorkflowSecurity : ServiceLocator<IWorkflowSecurity, WorkflowSecurity>, IWorkflowSecurity
    {
        #region Constants
        private const string ReviewPermissionKey = "REVIEW";
        private const string ReviewPermissionCode = "SYSTEM_CONTENTWORKFLOWSTATE";
        #endregion

        #region Members
        private readonly IUserController _userController = UserController.Instance;
        private readonly IWorkflowManager _workflowManager = WorkflowManager.Instance;
        private readonly IWorkflowStatePermissionsRepository _statePermissionsRepository = WorkflowStatePermissionsRepository.Instance;
        #endregion

        #region Public Methods
        public bool HasStateReviewerPermission(PortalSettings settings, UserInfo user, int stateId)
        {
            var permissions = _statePermissionsRepository.GetWorkflowStatePermissionByState(stateId);

            return user.IsSuperUser ||
                PortalSecurity.IsInRoles(user, settings, settings.AdministratorRoleName) ||
                PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), ReviewPermissionKey));
        }

        public bool HasStateReviewerPermission(int portalId, int userId, int stateId)
        {
            var user = _userController.GetUserById(portalId, userId);
            var portalSettings = new PortalSettings(portalId);
            return HasStateReviewerPermission(portalSettings, user, stateId);
        }

        public bool HasStateReviewerPermission(int stateId)
        {
            var user = _userController.GetCurrentUserInfo();
            return HasStateReviewerPermission(PortalSettings.Current, user, stateId);
        }

        public bool IsWorkflowReviewer(int workflowId, int userId)
        {
            var workflow = _workflowManager.GetWorkflow(workflowId);
            return workflow.States.Any(contentWorkflowState => HasStateReviewerPermission(workflow.PortalID, userId, contentWorkflowState.StateID));
        }

        public PermissionInfo GetStateReviewPermission()
        {
            return (PermissionInfo)new PermissionController().GetPermissionByCodeAndKey(ReviewPermissionCode, ReviewPermissionKey)[0];
        }
        #endregion

        #region Service Locator
        protected override Func<IWorkflowSecurity> GetFactory()
        {
            return () => new WorkflowSecurity();
        }
        #endregion
    }
}
