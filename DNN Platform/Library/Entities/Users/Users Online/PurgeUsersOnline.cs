// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Entities.Users
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Users
    /// Class:      PurgeUsersOnline
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The PurgeUsersOnline class provides a Scheduler for purging the Users Online
    /// data
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
    public class PurgeUsersOnline : SchedulerClient
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Constructs a PurgeUsesOnline SchedulerClient
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objScheduleHistoryItem">A SchedulerHistiryItem</param>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public PurgeUsersOnline(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateUsersOnline updates the Users Online information
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        private void UpdateUsersOnline()
        {
            var objUserOnlineController = new UserOnlineController();

            //Is Users Online Enabled?
            if ((objUserOnlineController.IsEnabled()))
            {
                //Update the Users Online records from Cache
                this.Status = "Updating Users Online";
                objUserOnlineController.UpdateUsersOnline();
                this.Status = "Update Users Online Successfully";
                this.ScheduleHistoryItem.Succeeded = true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DoWork does th4 Scheduler work
        /// </summary>
        /// -----------------------------------------------------------------------------
        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public override void DoWork()
        {
            try
            {
                //notification that the event is progressing
                this.Progressing(); //OPTIONAL
                this.UpdateUsersOnline();
                this.ScheduleHistoryItem.Succeeded = true; //REQUIRED
                this.ScheduleHistoryItem.AddLogNote("UsersOnline purge completed.");
            }
            catch (Exception exc) //REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; //REQUIRED
                this.ScheduleHistoryItem.AddLogNote("UsersOnline purge failed." + exc);

                //notification that we have errored
                this.Errored(ref exc);

                //log the exception
                Exceptions.LogException(exc);
            }
        }
    }
}
