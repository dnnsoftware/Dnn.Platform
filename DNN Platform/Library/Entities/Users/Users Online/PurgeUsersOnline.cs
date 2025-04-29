// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users;

using System;

using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

/// Project:    DotNetNuke
/// Namespace:  DotNetNuke.Entities.Users
/// Class:      PurgeUsersOnline
/// <summary>
/// The PurgeUsersOnline class provides a Scheduler for purging the Users Online
/// data.
/// </summary>
[DnnDeprecated(8, 0, 0, "Other solutions exist outside of the DNN Platform", RemovalVersion = 10)]
public partial class PurgeUsersOnline : SchedulerClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PurgeUsersOnline"/> class.
    /// Constructs a PurgeUsesOnline SchedulerClient.
    /// </summary>
    /// <param name="objScheduleHistoryItem">A SchedulerHistiryItem.</param>
    public PurgeUsersOnline(ScheduleHistoryItem objScheduleHistoryItem)
    {
        this.ScheduleHistoryItem = objScheduleHistoryItem;
    }

    /// <summary>DoWork does th4 Scheduler work.</summary>
    public override void DoWork()
    {
        try
        {
            // notification that the event is progressing
            this.Progressing(); // OPTIONAL
            this.UpdateUsersOnline();
            this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
            this.ScheduleHistoryItem.AddLogNote("UsersOnline purge completed.");
        }
        catch (Exception exc)
        {
            this.ScheduleHistoryItem.Succeeded = false; // REQUIRED
            this.ScheduleHistoryItem.AddLogNote("UsersOnline purge failed." + exc);

            // notification that we have errored
            this.Errored(ref exc);

            // log the exception
            Exceptions.LogException(exc);
        }
    }

    /// <summary>UpdateUsersOnline updates the Users Online information.</summary>
    private void UpdateUsersOnline()
    {
        var objUserOnlineController = new UserOnlineController();

        // Is Users Online Enabled?
        if (objUserOnlineController.IsEnabled())
        {
            // Update the Users Online records from Cache
            this.Status = "Updating Users Online";
            objUserOnlineController.UpdateUsersOnline();
            this.Status = "Update Users Online Successfully";
            this.ScheduleHistoryItem.Succeeded = true;
        }
    }
}
