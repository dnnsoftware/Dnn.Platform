// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Groups.Components
{
    using System;
    using System.Globalization;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Journal;

    public class GroupUtilities
    {
        public static void CreateJournalEntry(RoleInfo roleInfo, UserInfo createdBy)
        {
            var journalController = JournalController.Instance;
            var journalItem = new JournalItem();

            string url = string.Empty;

            if (roleInfo.Settings.ContainsKey("URL"))
            {
                url = roleInfo.Settings["URL"];
            }

            journalItem.PortalId = roleInfo.PortalID;
            journalItem.ProfileId = createdBy.UserID;
            journalItem.UserId = createdBy.UserID;
            journalItem.Title = roleInfo.RoleName;
            journalItem.ItemData = new ItemData { Url = url };
            journalItem.SocialGroupId = roleInfo.RoleID;
            journalItem.Summary = roleInfo.Description;
            journalItem.Body = null;
            journalItem.JournalTypeId = journalController.GetJournalType("groupcreate").JournalTypeId;
            journalItem.ObjectKey = string.Format("groupcreate:{0}:{1}", roleInfo.RoleID.ToString(CultureInfo.InvariantCulture), createdBy.UserID.ToString(CultureInfo.InvariantCulture));

            if (journalController.GetJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey) != null)
            {
                journalController.DeleteJournalItemByKey(roleInfo.PortalID, journalItem.ObjectKey);
            }

            journalItem.SecuritySet = string.Empty;

            if (roleInfo.IsPublic)
            {
                journalItem.SecuritySet += "E,";
            }

            journalController.SaveJournalItem(journalItem, null);
        }
    }
}
