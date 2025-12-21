// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.VisualBasic;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>Provides the ability to manage scheduled tasks.</summary>
    public partial class SchedulingController
    {
        public static int AddSchedule(string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
        {
            EventLogController.Instance.AddLog("TypeFullName", typeFullName, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.SCHEDULE_CREATED);
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

        public static void DeleteSchedule(int scheduleID)
        {
            DataProvider.Instance().DeleteSchedule(scheduleID);
            EventLogController.Instance.AddLog(
                "ScheduleID",
                scheduleID.ToString(CultureInfo.InvariantCulture),
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.SCHEDULE_DELETED);
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

        public static void UpdateSchedule(int scheduleID, string typeFullName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string servers, string friendlyName, DateTime scheduleStartDate)
        {
            DataProvider.Instance().UpdateSchedule(
                scheduleID,
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
            EventLogController.Instance.AddLog("TypeFullName", typeFullName, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.SCHEDULE_UPDATED);
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
