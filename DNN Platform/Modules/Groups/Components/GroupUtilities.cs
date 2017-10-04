#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Globalization;

using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Journal;

#endregion

namespace DotNetNuke.Modules.Groups.Components
{
    public class GroupUtilities
    {
        public static void CreateJournalEntry(RoleInfo roleInfo, UserInfo createdBy)
        {
            var journalController = JournalController.Instance;
            var journalItem = new JournalItem();
            
            string url = "";
            
            if (roleInfo.Settings.ContainsKey("URL"))
            {
                url = roleInfo.Settings["URL"];
            }

            journalItem.PortalId = roleInfo.PortalID;
            journalItem.ProfileId = createdBy.UserID;
            journalItem.UserId = createdBy.UserID;
            journalItem.Title = roleInfo.RoleName;
            journalItem.ItemData = new ItemData {Url = url};
            journalItem.SocialGroupId = roleInfo.RoleID;
            journalItem.Summary = roleInfo.Description;
            journalItem.Body = null;
            journalItem.JournalTypeId = journalController.GetJournalType("groupcreate").JournalTypeId;
            journalItem.ObjectKey = string.Format("groupcreate:{0}:{1}", roleInfo.RoleID.ToString(CultureInfo.InvariantCulture), createdBy.UserID.ToString(CultureInfo.InvariantCulture));
            
            if (journalController.GetJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey) != null)
                journalController.DeleteJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey);
            
            
            journalItem.SecuritySet = string.Empty;
            
            if (roleInfo.IsPublic)
                journalItem.SecuritySet += "E,";
            
            
            journalController.SaveJournalItem(journalItem, null);
        }
    }
}