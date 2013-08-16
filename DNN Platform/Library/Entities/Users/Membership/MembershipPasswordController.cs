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

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Entities.Users.Membership
{
    public class MembershipPasswordController
    {
        private static readonly DataProvider Provider = DataProvider.Instance();

        public List<PasswordHistory> GetPasswordHistory()
        {
            List<PasswordHistory> history = CBO.FillCollection<PasswordHistory>(Provider.GetPasswordHistory(UserController.GetCurrentUserInfo().UserID));
            return history;
        }


        public bool IsValidToken(int userId, Guid resetToken)
        {
            if (UserController.GetCurrentUserInfo().PasswordResetToken == resetToken && UserController.GetCurrentUserInfo().PasswordResetExpiration <= DateTime.Now)
            {
                return true;
            }
            return false;
        }

        public bool FoundBannedPassword(string inputString)
        {
            const string listName = "BannedPasswords";

            var listController = new ListController();
            PortalSettings settings = PortalController.GetCurrentPortalSettings();

            IEnumerable<ListEntryInfo> listEntryHostInfos = listController.GetListEntryInfoItems(listName, "", Null.NullInteger);
            IEnumerable<ListEntryInfo> listEntryPortalInfos = listController.GetListEntryInfoItems(listName + "-" + settings.PortalId, "", settings.PortalId);
            
            IEnumerable<ListEntryInfo> query2 = listEntryHostInfos.Where(test => test.Text == inputString);
            IEnumerable<ListEntryInfo> query3 = listEntryPortalInfos.Where(test => test.Text == inputString);
            
            if (query2.Any() || query3.Any())
            {
                return true;
            }

            return false;
        }
    }
}