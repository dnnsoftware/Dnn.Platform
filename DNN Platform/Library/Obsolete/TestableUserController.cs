// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using DotNetNuke.Framework;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use UserController instead. Scheduled removal in v10.0.0.")]
    public class TestableUserController : ServiceLocator<IUserController, TestableUserController>, IUserController
    {
        public UserInfo GetUserByDisplayname(int portalId, string displayName)
        {
            return UserController.Instance.GetUserByDisplayname(portalId, displayName);
        }

        public UserInfo GetUserById(int portalId, int userId)
        {
            return UserController.Instance.GetUserById(portalId, userId);
        }

        public IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationTypeId,
            bool isAdmin, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyNames,
            string propertyValues)
        {
            return UserController.Instance.GetUsersAdvancedSearch(portalId, userId, filterUserId, filterRoleId,
                relationTypeId, isAdmin, pageIndex, pageSize, sortColumn, sortAscending, propertyNames, propertyValues);
        }

        public IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending,
            string propertyName, string propertyValue)
        {
            return UserController.Instance.GetUsersBasicSearch(portalId, pageIndex, pageSize, sortColumn, sortAscending, propertyName, propertyValue);
        }

        protected override Func<IUserController> GetFactory()
        {
            return () => new TestableUserController();
        }
    }
}
