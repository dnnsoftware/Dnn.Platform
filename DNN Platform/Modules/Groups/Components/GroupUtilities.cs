﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
