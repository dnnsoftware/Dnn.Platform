// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System;

#endregion


namespace DotNetNuke.Services.Users
{
    public class PurgeDeletedUsers : SchedulerClient
    {

        public PurgeDeletedUsers(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                foreach (PortalInfo portal in new PortalController().GetPortals())
                {
                    var settings = new PortalSettings(portal.PortalID);
                    if (settings.DataConsentActive)
                    {
                        if (settings.DataConsentUserDeleteAction == PortalSettings.UserDeleteAction.DelayedHardDelete)
                        {
                            var thresholdDate = DateTime.Now;
                            switch (settings.DataConsentDelayMeasurement)
                            {
                                case "h":
                                    thresholdDate = DateTime.Now.AddHours(-1 * settings.DataConsentDelay);
                                    break;
                                case "d":
                                    thresholdDate = DateTime.Now.AddDays(-1 * settings.DataConsentDelay);
                                    break;
                                case "w":
                                    thresholdDate = DateTime.Now.AddDays(-7 * settings.DataConsentDelay);
                                    break;
                            }
                            var deletedUsers = UserController.GetDeletedUsers(portal.PortalID);
                            foreach (UserInfo user in deletedUsers)
                            {
                                if (user.LastModifiedOnDate < thresholdDate && user.RequestsRemoval)
                                {
                                    UserController.RemoveUser(user);
                                    ScheduleHistoryItem.AddLogNote(string.Format("Removed user {0}{1}", user.Username, Environment.NewLine));
                                }
                            }
                        }
                    }
                }
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
                ScheduleHistoryItem.AddLogNote("Purging deleted users task completed");
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging deleted users task failed: {0}.", exc.ToString()));

                //notification that we have errored
                Errored(ref exc); //REQUIRED

                //log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
