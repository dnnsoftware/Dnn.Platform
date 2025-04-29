// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Host;
using Microsoft.VisualBasic;

public class DNNScheduler : SchedulingProvider
{
    /// <inheritdoc/>
    public override Dictionary<string, string> Settings
    {
        get
        {
            return ComponentFactory.GetComponentSettings<DNNScheduler>() as Dictionary<string, string>;
        }
    }

    /// <inheritdoc/>
    public override int AddSchedule(ScheduleItem scheduleItem)
    {
        // Remove item from queue
        Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);

        // save item
        scheduleItem.ScheduleID = SchedulingController.AddSchedule(
            scheduleItem.TypeFullName,
            scheduleItem.TimeLapse,
            scheduleItem.TimeLapseMeasurement,
            scheduleItem.RetryTimeLapse,
            scheduleItem.RetryTimeLapseMeasurement,
            scheduleItem.RetainHistoryNum,
            scheduleItem.AttachToEvent,
            scheduleItem.CatchUpEnabled,
            scheduleItem.Enabled,
            scheduleItem.ObjectDependencies,
            scheduleItem.Servers,
            scheduleItem.FriendlyName,
            scheduleItem.ScheduleStartDate);

        // Add schedule to queue
        this.RunScheduleItemNow(scheduleItem);

        // Return Id
        return scheduleItem.ScheduleID;
    }

    /// <inheritdoc/>
    public override void AddScheduleItemSetting(int scheduleID, string name, string value)
    {
        SchedulingController.AddScheduleItemSetting(scheduleID, name, value);
    }

    /// <inheritdoc/>
    public override void DeleteSchedule(ScheduleItem scheduleItem)
    {
        SchedulingController.DeleteSchedule(scheduleItem.ScheduleID);
        Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);
        DataCache.RemoveCache("ScheduleLastPolled");
    }

    /// <inheritdoc/>
    public override void ExecuteTasks()
    {
        if (Enabled)
        {
            Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
            Scheduler.CoreScheduler.KeepRunning = true;
            Scheduler.CoreScheduler.KeepThreadAlive = false;
            Scheduler.CoreScheduler.Start();
        }
    }

    /// <inheritdoc/>
    public override int GetActiveThreadCount()
    {
        return SchedulingController.GetActiveThreadCount();
    }

    /// <inheritdoc/>
    public override int GetFreeThreadCount()
    {
        return SchedulingController.GetFreeThreadCount();
    }

    /// <inheritdoc/>
    public override int GetMaxThreadCount()
    {
        return SchedulingController.GetMaxThreadCount();
    }

    /// <inheritdoc/>
    public override ScheduleItem GetNextScheduledTask(string server)
    {
        return SchedulingController.GetNextScheduledTask(server);
    }

    /// <inheritdoc/>
    public override ArrayList GetSchedule()
    {
        return new ArrayList(SchedulingController.GetSchedule().ToArray());
    }

    /// <inheritdoc/>
    public override ArrayList GetSchedule(string server)
    {
        return new ArrayList(SchedulingController.GetSchedule(server).ToArray());
    }

    /// <inheritdoc/>
    public override ScheduleItem GetSchedule(int scheduleID)
    {
        return SchedulingController.GetSchedule(scheduleID);
    }

    /// <inheritdoc/>
    public override ScheduleItem GetSchedule(string typeFullName, string server)
    {
        return SchedulingController.GetSchedule(typeFullName, server);
    }

    /// <inheritdoc/>
    public override ArrayList GetScheduleHistory(int scheduleID)
    {
        return new ArrayList(SchedulingController.GetScheduleHistory(scheduleID).ToArray());
    }

    /// <inheritdoc/>
    public override Hashtable GetScheduleItemSettings(int scheduleID)
    {
        return SchedulingController.GetScheduleItemSettings(scheduleID);
    }

    /// <inheritdoc/>
    public override Collection GetScheduleProcessing()
    {
        return SchedulingController.GetScheduleProcessing();
    }

    /// <inheritdoc/>
    public override Collection GetScheduleQueue()
    {
        return SchedulingController.GetScheduleQueue();
    }

    /// <inheritdoc/>
    public override ScheduleStatus GetScheduleStatus()
    {
        return SchedulingController.GetScheduleStatus();
    }

    /// <inheritdoc/>
    public override void Halt(string sourceOfHalt)
    {
        Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
        Scheduler.CoreScheduler.Halt(sourceOfHalt);
        Scheduler.CoreScheduler.KeepRunning = false;
    }

    /// <inheritdoc/>
    public override void PurgeScheduleHistory()
    {
        Scheduler.CoreScheduler.InitializeThreadPool(false, MaxThreads);
        Scheduler.CoreScheduler.PurgeScheduleHistory();
    }

    /// <inheritdoc/>
    public override void ReStart(string sourceOfRestart)
    {
        this.Halt(sourceOfRestart);
        this.StartAndWaitForResponse();
    }

    /// <inheritdoc/>
    public override void RunEventSchedule(EventName eventName)
    {
        if (Enabled)
        {
            Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
            Scheduler.CoreScheduler.RunEventSchedule(eventName);
        }
    }

    /// <inheritdoc/>
    public override void RunScheduleItemNow(ScheduleItem scheduleItem, bool runNow)
    {
        // If the validation for the server failed, then the server was updated. The scheduled item will be picked up on the next check, so we can exit the request for this instance to run.
        if (!Scheduler.CoreScheduler.ValidateServersAreActiveForScheduledItem(scheduleItem))
        {
            return;
        }

        // Remove item from queue
        Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);
        var scheduleHistoryItem = new ScheduleHistoryItem(scheduleItem) { NextStart = runNow ? DateTime.Now : (scheduleItem.ScheduleStartDate != Null.NullDate ? scheduleItem.ScheduleStartDate : DateTime.Now) };

        if (scheduleHistoryItem.TimeLapse != Null.NullInteger
            && scheduleHistoryItem.TimeLapseMeasurement != Null.NullString
            && scheduleHistoryItem.Enabled
            && SchedulingController.CanRunOnThisServer(scheduleItem.Servers))
        {
            scheduleHistoryItem.ScheduleSource = ScheduleSource.STARTED_FROM_SCHEDULE_CHANGE;
            Scheduler.CoreScheduler.AddToScheduleQueue(scheduleHistoryItem);
        }

        DataCache.RemoveCache("ScheduleLastPolled");
    }

    /// <inheritdoc/>
    public override void RunScheduleItemNow(ScheduleItem scheduleItem)
    {
        this.RunScheduleItemNow(scheduleItem, false);
    }

    /// <inheritdoc/>
    public override void Start()
    {
        if (Enabled)
        {
            Scheduler.CoreScheduler.InitializeThreadPool(Debug, MaxThreads);
            Scheduler.CoreScheduler.KeepRunning = true;
            Scheduler.CoreScheduler.KeepThreadAlive = true;
            Scheduler.CoreScheduler.Start();
        }
    }

    /// <inheritdoc/>
    public override void StartAndWaitForResponse()
    {
        if (Enabled)
        {
            var newThread = new Thread(this.Start) { IsBackground = true };
            newThread.Start();

            // wait for up to 30 seconds for thread
            // to start up
            for (int i = 0; i <= 30; i++)
            {
                if (this.GetScheduleStatus() != ScheduleStatus.STOPPED)
                {
                    return;
                }

                Thread.Sleep(1000);
            }
        }
    }

    /// <inheritdoc/>
    public override void UpdateScheduleWithoutExecution(ScheduleItem scheduleItem)
    {
        SchedulingController.UpdateSchedule(scheduleItem);
    }

    /// <inheritdoc/>
    public override void UpdateSchedule(ScheduleItem scheduleItem)
    {
        // Remove item from queue
        Scheduler.CoreScheduler.RemoveFromScheduleQueue(scheduleItem);

        // save item
        SchedulingController.UpdateSchedule(scheduleItem);

        // Update items that are already scheduled
        var futureHistory = this.GetScheduleHistory(scheduleItem.ScheduleID).Cast<ScheduleHistoryItem>().Where(h => h.NextStart > DateTime.Now);

        var scheduleItemStart = scheduleItem.ScheduleStartDate > DateTime.Now
            ? scheduleItem.ScheduleStartDate
            : scheduleItem.NextStart;
        foreach (var scheduleHistoryItem in futureHistory)
        {
            scheduleHistoryItem.NextStart = scheduleItemStart;
            SchedulingController.UpdateScheduleHistory(scheduleHistoryItem);
        }

        // Add schedule to queue
        this.RunScheduleItemNow(scheduleItem);
    }

    /// <inheritdoc/>
    public override void RemoveFromScheduleInProgress(ScheduleItem scheduleItem)
    {
        // get ScheduleHistoryItem of the running task
        var runningscheduleHistoryItem = this.GetScheduleHistory(scheduleItem.ScheduleID).Cast<ScheduleHistoryItem>().ElementAtOrDefault(0);
        Scheduler.CoreScheduler.StopScheduleInProgress(scheduleItem, runningscheduleHistoryItem);
    }
}
