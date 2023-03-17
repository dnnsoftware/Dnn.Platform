// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using Microsoft.VisualBasic;

    using Globals = DotNetNuke.Common.Globals;

    // ReSharper restore InconsistentNaming

    // set up our delegates so we can track and react to events of the scheduler clients
    public delegate void WorkStarted(SchedulerClient objSchedulerClient);

    public delegate void WorkProgressing(SchedulerClient objSchedulerClient);

    public delegate void WorkCompleted(SchedulerClient objSchedulerClient);

    public delegate void WorkErrored(SchedulerClient objSchedulerClient, Exception objException);

    // ReSharper disable InconsistentNaming
    public enum EventName
    {
        // do not add APPLICATION_END
        // it will not reliably complete
        APPLICATION_START = 0,
    }

    public enum ScheduleSource
    {
        NOT_SET = 0,
        STARTED_FROM_SCHEDULE_CHANGE = 1,
        STARTED_FROM_EVENT = 2,
        STARTED_FROM_TIMER = 3,
        STARTED_FROM_BEGIN_REQUEST = 4,
    }

    public enum ScheduleStatus
    {
        NOT_SET = 0,
        WAITING_FOR_OPEN_THREAD = 1,
        RUNNING_EVENT_SCHEDULE = 2,
        RUNNING_TIMER_SCHEDULE = 3,
        RUNNING_REQUEST_SCHEDULE = 4,
        WAITING_FOR_REQUEST = 5,
        SHUTTING_DOWN = 6,
        STOPPED = 7,
    }

    public enum SchedulerMode
    {
        DISABLED = 0,
        TIMER_METHOD = 1,
        REQUEST_METHOD = 2,
    }

    public abstract class SchedulingProvider
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        public EventName EventName;

        /// <summary>Initializes a new instance of the <see cref="SchedulingProvider"/> class.</summary>
        protected SchedulingProvider()
        {
            var settings = this.Settings;
            if (settings != null)
            {
                this.ProviderPath = settings["providerPath"];

                string str;
                bool dbg;

                if (settings.TryGetValue("debug", out str) && bool.TryParse(str, out dbg))
                {
                    Debug = dbg;
                }

                int value;
                if (!settings.TryGetValue("maxThreads", out str) || !int.TryParse(str, out value))
                {
                    value = 1;
                }

                MaxThreads = value;

                // if (!settings.TryGetValue("delayAtAppStart", out str) || !int.TryParse(str, out value))
                // {
                //    value = 60;
                // }
                if (DotNetNuke.Common.Globals.Status != Globals.UpgradeStatus.Install)
                {
                    DelayAtAppStart = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1) * 60;
                }
                else
                {
                    DelayAtAppStart = 60;
                }
            }
            else
            {
                MaxThreads = 1;
                if (DotNetNuke.Common.Globals.Status != Globals.UpgradeStatus.Install)
                {
                    DelayAtAppStart = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1) * 60;
                }
                else
                {
                    DelayAtAppStart = 60;
                }
            }
        }

        public static bool Enabled
        {
            get
            {
                return SchedulerMode != SchedulerMode.DISABLED;
            }
        }

        public static bool ReadyForPoll
        {
            get
            {
                return DataCache.GetCache("ScheduleLastPolled") == null;
            }
        }

        public static SchedulerMode SchedulerMode
        {
            get
            {
                return Host.SchedulerMode;
            }
        }

        /// <summary>
        /// Gets the number of seconds since application start where no timer-initiated
        /// schedulers are allowed to run before. This safeguards against ovelapped
        /// application re-starts. See "Disable Ovelapped Recycling" under Recycling
        /// of IIS Manager Application Pool's Advanced Settings.
        /// </summary>
        public static int DelayAtAppStart { get; private set; }

        public static bool Debug { get; private set; }

        public static int MaxThreads { get; private set; }

        public static DateTime ScheduleLastPolled
        {
            get
            {
                return DataCache.GetCache("ScheduleLastPolled") != null
                    ? (DateTime)DataCache.GetCache("ScheduleLastPolled") : DateTime.MinValue;
            }

            set
            {
                var nextScheduledTask = Instance().GetNextScheduledTask(ServerController.GetExecutingServerName());
                var nextStart = nextScheduledTask != null && nextScheduledTask.NextStart > DateTime.Now
                                         ? nextScheduledTask.NextStart : DateTime.Now.AddMinutes(1);
                DataCache.SetCache("ScheduleLastPolled", value, nextStart);
            }
        }

        public virtual Dictionary<string, string> Settings
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }

        public string ProviderPath { get; private set; }

        public static SchedulingProvider Instance()
        {
            return ComponentFactory.GetComponent<SchedulingProvider>();
        }

        public abstract void Start();

        public abstract void ExecuteTasks();

        public abstract void ReStart(string sourceOfRestart);

        public abstract void StartAndWaitForResponse();

        public abstract void Halt(string sourceOfHalt);

        public abstract void PurgeScheduleHistory();

        public abstract void RunEventSchedule(EventName eventName);

        public abstract ArrayList GetSchedule();

        public abstract ArrayList GetSchedule(string server);

        public abstract ScheduleItem GetSchedule(int scheduleID);

        public abstract ScheduleItem GetSchedule(string typeFullName, string server);

        public abstract ScheduleItem GetNextScheduledTask(string server);

        public abstract ArrayList GetScheduleHistory(int scheduleID);

        public abstract Hashtable GetScheduleItemSettings(int scheduleID);

        public abstract void AddScheduleItemSetting(int scheduleID, string name, string value);

        public abstract Collection GetScheduleQueue();

        public abstract Collection GetScheduleProcessing();

        public abstract int GetFreeThreadCount();

        public abstract int GetActiveThreadCount();

        public abstract int GetMaxThreadCount();

        public abstract ScheduleStatus GetScheduleStatus();

        public abstract int AddSchedule(ScheduleItem scheduleItem);

        public abstract void UpdateSchedule(ScheduleItem scheduleItem);

        public abstract void UpdateScheduleWithoutExecution(ScheduleItem scheduleItem);

        public abstract void DeleteSchedule(ScheduleItem scheduleItem);

        public virtual void RunScheduleItemNow(ScheduleItem scheduleItem)
        {
            // Do Nothing
        }

        public virtual void RunScheduleItemNow(ScheduleItem scheduleItem, bool runNow)
        {
            // Do Nothing
        }

        public abstract void RemoveFromScheduleInProgress(ScheduleItem scheduleItem);

        /// <summary>
        /// Checks the scheduled item if it has servers specified to run on. If it does, then it checks to see if the server is active.
        /// </summary>
        /// <param name="scheduleItem">The schedule to check</param>
        /// <returns>True if nothing happens to the scheduled item. Returns false if the scheduled item was updated.</returns>
        /// <remarks>
        /// If the server specified is not active then the scheduled item will be assigned to the most recent active server.
        /// This assumes that all schedules will run on a single server if none of the servers specified are active.
        /// </remarks>
        protected virtual bool ValidateServersAreActiveForScheduledItem(ScheduleItem scheduleItem)
        {
            // If there are servers specified, verify requested server has been active within the last 10 minutes.
            if (string.IsNullOrEmpty(scheduleItem.Servers))
            {
                return true;
            }

            // Get the individual server names from the scheduled item
            var servers = scheduleItem.Servers.ToLowerInvariant().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            // Get the enabled servers with recent activity.
            var enabledServers = ServerController.GetEnabledServersWithActivity();

            // Validate that any server in the scheduled item is in the enabled server list with activity in the last 10 minutes.
            bool serverHasRecentActivity = enabledServers.Any(i => servers.Contains(i.ServerName.ToLowerInvariant()));

            // If no servers in the scheduled item had recent activity, assign this scheduled item to the server with the most recent activity
            if (!serverHasRecentActivity)
            {
                // Assign a new server to this task then run it
                scheduleItem.Servers = $",{enabledServers.First().ServerName.ToLowerInvariant()},";

                // Save the scheduled item back to the database.
                this.UpdateSchedule(scheduleItem);
                return false;
            }

            return true;
        }
    }
}
