// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Users
{
    using System;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Scheduling;

    public class PurgeDeletedUsers : SchedulerClient
    {
        public PurgeDeletedUsers(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
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
                                    this.ScheduleHistoryItem.AddLogNote(string.Format("Removed user {0}{1}", user.Username, Environment.NewLine));
                                }
                            }
                        }
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging deleted users task completed");
            }
            catch (Exception exc) // REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote(string.Format("Purging deleted users task failed: {0}.", exc.ToString()));

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
