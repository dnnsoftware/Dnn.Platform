// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Users
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Scheduling;

    public class PurgeDeletedUsers : SchedulerClient
    {
        /// <summary>Initializes a new instance of the <see cref="PurgeDeletedUsers"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeDeletedUsers(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// <inheritdoc/>
        public override void DoWork()
        {
            try
            {
                foreach (IPortalInfo portal in new PortalController().GetPortals())
                {
                    var settings = new PortalSettings(portal.PortalId);
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

                            var deletedUsers = UserController.GetDeletedUsers(portal.PortalId);
                            foreach (UserInfo user in deletedUsers)
                            {
                                if (user.LastModifiedOnDate < thresholdDate && user.RequestsRemoval)
                                {
                                    UserController.RemoveUser(user);
                                    this.ScheduleHistoryItem.AddLogNote($"Removed user {user.Username}{Environment.NewLine}");
                                }
                            }
                        }
                    }
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging deleted users task completed");
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging deleted users task failed: {exc}.");

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
