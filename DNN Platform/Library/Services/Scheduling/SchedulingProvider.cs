// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Abstractions.Application;
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

        /// <summary>The application starting.</summary>
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        APPLICATION_START = 0,
    }

    public enum ScheduleSource
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>The source is not known.</summary>
        NOT_SET = 0,

        /// <summary>The schedule changed.</summary>
        STARTED_FROM_SCHEDULE_CHANGE = 1,

        /// <summary>An event triggered the scheduled task.</summary>
        STARTED_FROM_EVENT = 2,

        /// <summary>The timer triggered the scheduled task.</summary>
        STARTED_FROM_TIMER = 3,

        /// <summary>The beginning of a request triggered the scheduled task.</summary>
        STARTED_FROM_BEGIN_REQUEST = 4,
#pragma warning restore CA1707
    }

    public enum ScheduleStatus
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>The status is not set.</summary>
        NOT_SET = 0,

        /// <summary>The task is waiting for an open thread.</summary>
        WAITING_FOR_OPEN_THREAD = 1,

        /// <summary>The task is running from an event trigger.</summary>
        RUNNING_EVENT_SCHEDULE = 2,

        /// <summary>The task is running from a timer trigger.</summary>
        RUNNING_TIMER_SCHEDULE = 3,

        /// <summary>The task is running from a request trigger.</summary>
        RUNNING_REQUEST_SCHEDULE = 4,

        /// <summary>The task is waiting for a request.</summary>
        WAITING_FOR_REQUEST = 5,

        /// <summary>The scheduler is shutting down.</summary>
        SHUTTING_DOWN = 6,

        /// <summary>The scheduler is stopped.</summary>
        STOPPED = 7,
#pragma warning restore CA1707
    }

    /// <inheritdoc cref="Abstractions.Application.SchedulerMode"/>
    public enum SchedulerMode
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <inheritdoc cref="Abstractions.Application.SchedulerMode.Disabled"/>
        DISABLED = 0,

        /// <inheritdoc cref="Abstractions.Application.SchedulerMode.TimerMethod"/>
        TIMER_METHOD = 1,

        /// <inheritdoc cref="Abstractions.Application.SchedulerMode.REQUEST_METHOD"/>
        REQUEST_METHOD = 2,
#pragma warning restore CA1707
    }

    public abstract class SchedulingProvider
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
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

        public static bool Enabled => SchedulerMode != SchedulerMode.DISABLED;

        public static bool ReadyForPoll => DataCache.GetCache("ScheduleLastPolled") == null;

        public static SchedulerMode SchedulerMode => Host.SchedulerMode;

        /// <summary>
        /// Gets the number of seconds since application start where no timer-initiated
        /// schedulers are allowed to run before. This safeguards against overlapped
        /// application re-starts. See "Disable Overlapped Recycling" under Recycling
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

        public virtual Dictionary<string, string> Settings => new();

        public string ProviderPath { get; private set; }

        public static SchedulingProvider Instance() => ComponentFactory.GetComponent<SchedulingProvider>();

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
    }
}
