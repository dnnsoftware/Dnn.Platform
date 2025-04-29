// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Cookies;

using System;

using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;

/// <summary>Scheduled task to remove old cookies.</summary>
public class PurgeAuthCookiesTask : SchedulerClient
{
    /// <summary>Initializes a new instance of the <see cref="PurgeAuthCookiesTask"/> class.</summary>
    /// <param name="objScheduleHistoryItem">Object that will get added to the database as a record of this run.</param>
    public PurgeAuthCookiesTask(ScheduleHistoryItem objScheduleHistoryItem)
    {
        this.ScheduleHistoryItem = objScheduleHistoryItem;
    }

    /// <inheritdoc/>
    public override void DoWork()
    {
        try
        {
            AuthCookieController.Instance.DeleteExpired(DateTime.UtcNow);
            this.ScheduleHistoryItem.Succeeded = true;
            this.ScheduleHistoryItem.AddLogNote("Purging auth cookies completed");
        }
        catch (Exception exc)
        {
            this.ScheduleHistoryItem.Succeeded = false;
            this.ScheduleHistoryItem.AddLogNote(string.Format("Purging auth cookies task failed: {0}.", exc.ToString()));
            this.Errored(ref exc);
            Exceptions.LogException(exc);
        }
    }
}
