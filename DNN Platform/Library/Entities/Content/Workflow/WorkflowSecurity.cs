// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security.Permissions;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Content.Workflow.Repositories;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    using Microsoft.Extensions.DependencyInjection;

    public class WorkflowSecurity(IPermissionDefinitionService permissionDefinitionService, IUserController userController, IWorkflowManager workflowManager)
        : ServiceLocator<IWorkflowSecurity, WorkflowSecurity>, IWorkflowSecurity
    {
        private const string ReviewPermissionKey = "REVIEW";
        private const string ReviewPermissionCode = "SYSTEM_CONTENTWORKFLOWSTATE";
        private const string ContentManagers = "Content Managers";
        private readonly IPermissionDefinitionService permissionDefinitionService = permissionDefinitionService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPermissionDefinitionService>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IWorkflowManager workflowManager = workflowManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IWorkflowManager>();
        private readonly IWorkflowStatePermissionsRepository statePermissionsRepository = WorkflowStatePermissionsRepository.Instance;

        /// <summary>Initializes a new instance of the <see cref="WorkflowSecurity"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IPermissionDefinitionService. Scheduled removal in v12.0.0.")]
        public WorkflowSecurity()
            : this(null, UserController.Instance, WorkflowManager.Instance)
        {
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", Justification = "Breaking change")]
        public bool HasStateReviewerPermission(PortalSettings settings, UserInfo user, int stateId)
            => this.HasStateReviewerPermission((IPortalSettings)settings, user, stateId);

        /// <inheritdoc cref="IWorkflowSecurity.HasStateReviewerPermission(PortalSettings,UserInfo,int)" />
        public bool HasStateReviewerPermission(IPortalSettings settings, UserInfo user, int stateId)
        {
            var permissions = this.statePermissionsRepository.GetWorkflowStatePermissionByState(stateId);

            return user.IsSuperUser ||
                PortalSecurity.IsInRoles(user, settings, settings.AdministratorRoleName) ||
                PortalSecurity.IsInRoles(user, settings, ContentManagers) ||
                PortalSecurity.IsInRoles(user, settings, PermissionController.BuildPermissions(permissions.ToList(), ReviewPermissionKey));
        }

        /// <inheritdoc />
        public bool HasStateReviewerPermission(int portalId, int userId, int stateId)
        {
            var user = this.userController.GetUserById(portalId, userId);
            var portalSettings = new PortalSettings(portalId);
            return this.HasStateReviewerPermission(portalSettings, user, stateId);
        }

        /// <inheritdoc />
        public bool HasStateReviewerPermission(int stateId)
        {
            var user = this.userController.GetCurrentUserInfo();
            return this.HasStateReviewerPermission(PortalSettings.Current, user, stateId);
        }

        /// <inheritdoc />
        public bool IsWorkflowReviewer(int workflowId, int userId)
        {
            var workflow = this.workflowManager.GetWorkflow(workflowId);
            return workflow.States.Any(contentWorkflowState => this.HasStateReviewerPermission(workflow.PortalID, userId, contentWorkflowState.StateID));
        }

        /// <inheritdoc />
        public PermissionInfo GetStateReviewPermission()
        {
            return this.permissionDefinitionService.GetDefinitionsByCodeAndKey(ReviewPermissionCode, ReviewPermissionKey).OfType<PermissionInfo>().FirstOrDefault();
        }

        /// <inheritdoc />
        protected override Func<IWorkflowSecurity> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<IWorkflowSecurity>;
        }
    }
}
