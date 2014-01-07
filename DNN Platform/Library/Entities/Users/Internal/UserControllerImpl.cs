#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Membership;

namespace DotNetNuke.Entities.Users.Internal
{
    internal class UserControllerImpl : IUserController
    {
        public UserInfo GetUserByDisplayname(int portalId, string displayName)
        {
            return MembershipProvider.Instance().GetUserByDisplayName(PortalController.GetEffectivePortalId(portalId), displayName);
        }

        public UserInfo GetUserById(int portalId, int userId)
        {
            return UserController.GetUserById(PortalController.GetEffectivePortalId(portalId), userId);
        }

        public IList<UserInfo> GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int filterRoleId, int relationshipTypeId,
            bool isAdmin, int pageIndex, int pageSize, string sortColumn,
            bool sortAscending, string propertyNames, string propertyValues)
        {
            return MembershipProvider.Instance().GetUsersAdvancedSearch(PortalController.GetEffectivePortalId(portalId), userId, filterUserId, filterRoleId, relationshipTypeId,
                                                       isAdmin, pageIndex, pageSize, sortColumn,
                                                       sortAscending, propertyNames, propertyValues);
        }

        public IList<UserInfo> GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn, bool sortAscending, string propertyName, string propertyValue)
        {
            return MembershipProvider.Instance().GetUsersBasicSearch(PortalController.GetEffectivePortalId(portalId), pageIndex, pageSize, sortColumn,
                                                       sortAscending, propertyName, propertyValue);
        }

    }
}