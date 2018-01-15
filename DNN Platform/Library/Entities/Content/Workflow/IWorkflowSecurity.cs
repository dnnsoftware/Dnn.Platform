#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Entities.Content.Workflow
{
    /// <summary>
    /// This class is responsible of provide information around Workflow Review permission
    /// </summary>
    public interface IWorkflowSecurity
    {
        /// <summary>
        /// This method returns true if the user has review permission on the specified state
        /// </summary>
        /// <param name="portalSettings">Portal settings</param>
        /// <param name="user">User entity</param>
        /// <param name="stateId">State Id</param>
        /// <returns>True if the user has review permission, false otherwise</returns>
        bool HasStateReviewerPermission(PortalSettings portalSettings, UserInfo user, int stateId);

        /// <summary>
        /// This method returns true if the user has review permission on the specified state
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="userId">User Id</param>
        /// <param name="stateId">State Id</param>
        /// <returns>True if the user has review permission, false otherwise</returns>
        bool HasStateReviewerPermission(int portalId, int userId, int stateId);

        /// <summary>
        /// This method returns true if the current user has review permission on the specified state
        /// </summary>
        /// <param name="stateId">State Id</param>
        /// <returns>True if the user has review permission, false otherwise</returns>
        bool HasStateReviewerPermission(int stateId);

        /// <summary>
        /// This method returns true if the user has review permission on at least one workflow state
        /// </summary>
        /// <param name="workflowId">Workflow Id</param>
        /// <param name="userId">User Id</param>
        /// <returns>True if the user has review permission on at least on workflow state, false otherwise</returns>
        bool IsWorkflowReviewer(int workflowId, int userId);

        /// <summary>
        /// This method gets the PermissionInfo of the State Review permission
        /// </summary>
        /// <returns>PermissionInfo of the State Review permission</returns>
        PermissionInfo GetStateReviewPermission();
    }
}
