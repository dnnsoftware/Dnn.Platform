// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;

    /// <summary>
    /// This class is responsible of provide information around Workflow Review permission.
    /// </summary>
    public interface IWorkflowSecurity
    {
        /// <summary>
        /// This method returns true if the user has review permission on the specified state.
        /// </summary>
        /// <param name="portalSettings">Portal settings.</param>
        /// <param name="user">User entity.</param>
        /// <param name="stateId">State Id.</param>
        /// <returns>True if the user has review permission, false otherwise.</returns>
        bool HasStateReviewerPermission(PortalSettings portalSettings, UserInfo user, int stateId);

        /// <summary>
        /// This method returns true if the user has review permission on the specified state.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="userId">User Id.</param>
        /// <param name="stateId">State Id.</param>
        /// <returns>True if the user has review permission, false otherwise.</returns>
        bool HasStateReviewerPermission(int portalId, int userId, int stateId);

        /// <summary>
        /// This method returns true if the current user has review permission on the specified state.
        /// </summary>
        /// <param name="stateId">State Id.</param>
        /// <returns>True if the user has review permission, false otherwise.</returns>
        bool HasStateReviewerPermission(int stateId);

        /// <summary>
        /// This method returns true if the user has review permission on at least one workflow state.
        /// </summary>
        /// <param name="workflowId">Workflow Id.</param>
        /// <param name="userId">User Id.</param>
        /// <returns>True if the user has review permission on at least on workflow state, false otherwise.</returns>
        bool IsWorkflowReviewer(int workflowId, int userId);

        /// <summary>
        /// This method gets the PermissionInfo of the State Review permission.
        /// </summary>
        /// <returns>PermissionInfo of the State Review permission.</returns>
        PermissionInfo GetStateReviewPermission();
    }
}
