#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Controllers
{
    /// <summary>
    /// Search Result Controller for Tab Indexer
    /// </summary>
    /// <remarks></remarks>
    public class UserResultController : BaseResultController
    {
        #region Abstract Class Implmentation

        public override bool HasViewPermission(SearchResult searchResult)
        {
            var userInSearchResult = UserController.GetUserById(PortalController.GetCurrentPortalSettings().PortalId, searchResult.AuthorUserId);
            if (userInSearchResult.IsDeleted || userInSearchResult.UserID == Null.NullInteger)
            {
                return false;
            }
            if (UserController.GetCurrentUserInfo().IsSuperUser || UserController.GetCurrentUserInfo().IsInRole("Administrators"))
            {
                if (searchResult.UniqueKey.Contains("AdminOnly"))
                {
                    return true;
                }
            }

            if (userInSearchResult.Social.Friend != null)
            {
                if (searchResult.UniqueKey.Contains("FriendsAndGroups"))
                {
                    return true;
                }
            }
            if (UserController.GetCurrentUserInfo().UserID != Null.NullInteger)
            {
                if (searchResult.UniqueKey.Contains("MembersOnly"))
                {
                    return true;
                }
            }
            if (UserController.GetCurrentUserInfo().UserID == Null.NullInteger)
            {
                if (searchResult.UniqueKey.Contains("AllUsers"))
                {
                    return true;
                }
            }
            return false;
        }

        public override string GetDocUrl(SearchResult searchResult)
        {
            var url = Globals.NavigateURL(PortalController.GetCurrentPortalSettings().UserTabId, string.Empty, "userid=" + searchResult.AuthorUserId);
            return url;
        }

        #endregion
    }
}