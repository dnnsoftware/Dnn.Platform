﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Entities.Users.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use UserController instead. Scheduled removal in v10.0.0.")]
    public class TestableUserController : ServiceLocator<IUserController, TestableUserController>, IUserController
    {
        protected override Func<IUserController> GetFactory()
        {
            return () => new TestableUserController();
        }

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
    }
}
