// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualBasic;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>Provides the ability to manage scheduled tasks.</summary>
    public partial class SchedulingController
    {
        /// <summary>Add a new scheduled task.</summary>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        /// <returns>The schedule ID.</returns>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial int AddSchedule(string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
            => AddSchedule(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, friendlyName, scheduleStartDate);

        /// <summary>Add a new scheduled task.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        /// <returns>The schedule ID.</returns>
        public static int AddSchedule(IEventLogger eventLogger, string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
        {
            eventLogger.AddLog("TypeFullName", typeFullName, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.SCHEDULE_CREATED);
            return DataProvider.Instance().AddSchedule(
                typeFullName,
                timeLapse,
                timeLapseMeasurement,
                retryTimeLapse,
                retryTimeLapseMeasurement,
                retainHistoryNum,
                attachToEvent,
                catchUpEnabled,
                enabled,
                objectDependencies,
                servers,
                UserController.Instance.GetCurrentUserInfo().UserID,
                friendlyName,
                scheduleStartDate);
        }

        public static int AddScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            return DataProvider.Instance().AddScheduleHistory(objScheduleHistoryItem.ScheduleID, objScheduleHistoryItem.StartDate, ServerController.GetExecutingServerName());
        }

        public static void AddScheduleItemSetting(int scheduleID, string name, string value)
        {
            DataProvider.Instance().AddScheduleItemSetting(scheduleID, name, value);
        }

        /// <summary>Delete a scheduled task.</summary>
        /// <param name="scheduleID">The schedule ID.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void DeleteSchedule(int scheduleID)
            => DeleteSchedule(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), scheduleID);

        /// <summary>Delete a scheduled task.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="scheduleId">The schedule ID.</param>
        public static void DeleteSchedule(IEventLogger eventLogger, int scheduleId)
        {
            DataProvider.Instance().DeleteSchedule(scheduleId);
            eventLogger.AddLog(
                "ScheduleID",
                scheduleId.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogType.SCHEDULE_DELETED);
        }

        public static int GetActiveThreadCount()
        {
            return Scheduler.CoreScheduler.GetActiveThreadCount();
        }

        public static int GetFreeThreadCount()
        {
            return Scheduler.CoreScheduler.GetFreeThreadCount();
        }

        public static int GetMaxThreadCount()
        {
            return Scheduler.CoreScheduler.GetMaxThreadCount();
        }

        public static ScheduleItem GetNextScheduledTask(string server)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetNextScheduledTask(server));
        }

        public static List<ScheduleItem> GetSchedule()
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetSchedule());
        }

        public static List<ScheduleItem> GetSchedule(string server)
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetSchedule(server));
        }

        public static ScheduleItem GetSchedule(string typeFullName, string server)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetSchedule(typeFullName, server));
        }

        public static ScheduleItem GetSchedule(int scheduleID)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetSchedule(scheduleID));
        }

        public static List<ScheduleItem> GetScheduleByEvent(string eventName, string server)
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetScheduleByEvent(eventName, server));
        }

        public static List<ScheduleHistoryItem> GetScheduleHistory(int scheduleID)
        {
            return CBO.FillCollection<ScheduleHistoryItem>(DataProvider.Instance().GetScheduleHistory(scheduleID));
        }

        public static Hashtable GetScheduleItemSettings(int scheduleID)
        {
            var h = new Hashtable();
            using (var r = DataProvider.Instance().GetScheduleItemSettings(scheduleID))
            {
                while (r.Read())
                {
                    h.Add(r["SettingName"], r["SettingValue"]);
                }
            }

            return h;
        }

        public static Collection GetScheduleProcessing()
        {
            return Scheduler.CoreScheduler.GetScheduleInProgress();
        }

        public static Collection GetScheduleQueue()
        {
            return Scheduler.CoreScheduler.GetScheduleQueue();
        }

        public static ScheduleStatus GetScheduleStatus()
        {
            return Scheduler.CoreScheduler.GetScheduleStatus();
        }

        public static void PurgeScheduleHistory()
        {
            DataProvider.Instance().PurgeScheduleHistory();
        }

        public static void ReloadSchedule()
        {
            Scheduler.CoreScheduler.ReloadSchedule();
        }

        public static void UpdateSchedule(ScheduleItem scheduleItem)
        {
#pragma warning disable 618
            UpdateSchedule(
                scheduleItem.ScheduleID,
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
#pragma warning restore 618
        }

        /// <summary>Updates the scheduled task.</summary>
        /// <param name="scheduleID">The ID of the scheduled task.</param>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        [DnnDeprecated(10, 2, 2, "Use overload taking IEventLogger")]
        public static partial void UpdateSchedule(int scheduleID, string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
            => UpdateSchedule(Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(), scheduleID, typeFullName, timeLapse, timeLapseMeasurement, retryTimeLapse, retryTimeLapseMeasurement, retainHistoryNum, attachToEvent, catchUpEnabled, enabled, objectDependencies, servers, friendlyName, scheduleStartDate);

        /// <summary>Updates the scheduled task.</summary>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="scheduleId">The ID of the scheduled task.</param>
        /// <param name="typeFullName">The full name of the <see cref="SchedulerClient"/> implementation.</param>
        /// <param name="timeLapse">The amount of time between runs of the task (in the unit specified by <paramref name="timeLapseMeasurement"/>).</param>
        /// <param name="timeLapseMeasurement">
        /// The time unit of <paramref name="timeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retryTimeLapse">The amount of time between retries after a task failure (in the unit specified by <paramref name="retryTimeLapseMeasurement"/>).</param>
        /// <param name="retryTimeLapseMeasurement">
        /// The time unit of <paramref name="retryTimeLapse"/>. Options include the following:
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <description>Meaning</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>"s"</c></term>
        ///         <description>Seconds</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"m"</c></term>
        ///         <description>Minutes</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"h"</c></term>
        ///         <description>Hours</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"d"</c></term>
        ///         <description>Days</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"mo"</c></term>
        ///         <description>Months</description>
        ///     </item>
        ///     <item>
        ///         <term><c>"y"</c></term>
        ///         <description>Years</description>
        ///     </item>
        /// </list>
        /// </param>
        /// <param name="retainHistoryNum">The number of log entries to retain.</param>
        /// <param name="attachToEvent">The name of an event which should trigger the task to run.</param>
        /// <param name="catchUpEnabled">Whether to catch up on task runs that were missed.</param>
        /// <param name="enabled">Whether the task is enabled.</param>
        /// <param name="objectDependencies">The name of objects (e.g. database tables) that the task depends on.</param>
        /// <param name="servers">A comma-delimited list of server names on which the task should run, or <see cref="string.Empty"/> to run on all servers.</param>
        /// <param name="friendlyName">The friendly name of the task.</param>
        /// <param name="scheduleStartDate">The date/time on which the task should start running.</param>
        public static void UpdateSchedule(IEventLogger eventLogger, int scheduleId, string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
        {
            DataProvider.Instance().UpdateSchedule(
                scheduleId,
                typeFullName,
                timeLapse,
                timeLapseMeasurement,
                retryTimeLapse,
                retryTimeLapseMeasurement,
                retainHistoryNum,
                attachToEvent,
                catchUpEnabled,
                enabled,
                objectDependencies,
                servers,
                UserController.Instance.GetCurrentUserInfo().UserID,
                friendlyName,
                scheduleStartDate);
            eventLogger.AddLog("TypeFullName", typeFullName, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.SCHEDULE_UPDATED);
        }

        public static void UpdateScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            DataProvider.Instance().UpdateScheduleHistory(
                objScheduleHistoryItem.ScheduleHistoryID,
                objScheduleHistoryItem.EndDate,
                objScheduleHistoryItem.Succeeded,
                objScheduleHistoryItem.LogNotes,
                objScheduleHistoryItem.NextStart);
        }

        public static bool CanRunOnThisServer(string servers)
        {
            string lwrServers = string.Empty;
            if (servers != null)
            {
                lwrServers = servers.ToLowerInvariant();
            }

            if (string.IsNullOrEmpty(lwrServers) || lwrServers.Contains(Globals.ServerName.ToLowerInvariant()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Replaces the old server name, with the new server name on all schedules where the old server name was found.
        /// </summary>
        /// <param name="oldServer">The old server to replace.</param>
        /// <param name="newServer">The new server to use.</param>
        internal static void ReplaceServer(ServerInfo oldServer, ServerInfo newServer)
        {
            DataProvider.Instance().ReplaceServerOnSchedules(oldServer.ServerName, newServer.ServerName);
        }
    }
}
