#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
        public PurgeUsersOnline(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpdateUsersOnline updates the Users Online information
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void UpdateUsersOnline()
        {
            var objUserOnlineController = new UserOnlineController();

            //Is Users Online Enabled?
            if ((objUserOnlineController.IsEnabled()))
            {
                //Update the Users Online records from Cache
                Status = "Updating Users Online";
                objUserOnlineController.UpdateUsersOnline();
                Status = "Update Users Online Successfully";
                ScheduleHistoryItem.Succeeded = true;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DoWork does th4 Scheduler work
        /// </summary>
        /// -----------------------------------------------------------------------------
        public override void DoWork()
        {
            try
            {
                //notification that the event is progressing
                Progressing(); //OPTIONAL
                UpdateUsersOnline();
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
                ScheduleHistoryItem.AddLogNote("UsersOnline purge completed.");
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED
                ScheduleHistoryItem.AddLogNote("UsersOnline purge failed." + exc);

                //notification that we have errored
                Errored(ref exc);

                //log the exception
                Exceptions.LogException(exc);
            }
        }
    }
}
