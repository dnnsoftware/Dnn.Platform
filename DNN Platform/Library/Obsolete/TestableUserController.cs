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

#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;

#endregion

namespace DotNetNuke.Entities.Users.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use UserController instead")]
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