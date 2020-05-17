﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Log.EventLog;

using Microsoft.VisualBasic;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Services.Scheduling
{
    public class SchedulingController
    {
        public static int AddSchedule(string TypeFullName, int TimeLapse, string TimeLapseMeasurement, int RetryTimeLapse, string RetryTimeLapseMeasurement, int RetainHistoryNum, string AttachToEvent,
                                      bool CatchUpEnabled, bool Enabled, string ObjectDependencies, string Servers, string FriendlyName, DateTime ScheduleStartDate)
        {
            EventLogController.Instance.AddLog("TypeFullName", TypeFullName, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.SCHEDULE_CREATED);
            return DataProvider.Instance().AddSchedule(TypeFullName,
                                                       TimeLapse,
                                                       TimeLapseMeasurement,
                                                       RetryTimeLapse,
                                                       RetryTimeLapseMeasurement,
                                                       RetainHistoryNum,
                                                       AttachToEvent,
                                                       CatchUpEnabled,
                                                       Enabled,
                                                       ObjectDependencies,
                                                       Servers,
                                                       UserController.Instance.GetCurrentUserInfo().UserID,
                                                       FriendlyName,
                                                       ScheduleStartDate);
        }

        public static int AddScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            return DataProvider.Instance().AddScheduleHistory(objScheduleHistoryItem.ScheduleID, objScheduleHistoryItem.StartDate, ServerController.GetExecutingServerName());
        }

        public static void AddScheduleItemSetting(int ScheduleID, string Name, string Value)
        {
            DataProvider.Instance().AddScheduleItemSetting(ScheduleID, Name, Value);
        }

        public static void DeleteSchedule(int ScheduleID)
        {
            DataProvider.Instance().DeleteSchedule(ScheduleID);
            EventLogController.Instance.AddLog("ScheduleID",
                               ScheduleID.ToString(),
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

        public static ScheduleItem GetNextScheduledTask(string Server)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetNextScheduledTask(Server));
        }

        public static List<ScheduleItem> GetSchedule()
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetSchedule());
        }

        public static List<ScheduleItem> GetSchedule(string Server)
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetSchedule(Server));
        }

        public static ScheduleItem GetSchedule(string TypeFullName, string Server)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetSchedule(TypeFullName, Server));
        }

        public static ScheduleItem GetSchedule(int ScheduleID)
        {
            return CBO.FillObject<ScheduleItem>(DataProvider.Instance().GetSchedule(ScheduleID));
        }

        public static List<ScheduleItem> GetScheduleByEvent(string EventName, string Server)
        {
            return CBO.FillCollection<ScheduleItem>(DataProvider.Instance().GetScheduleByEvent(EventName, Server));
        }

        public static List<ScheduleHistoryItem> GetScheduleHistory(int ScheduleID)
        {
            return CBO.FillCollection<ScheduleHistoryItem>(DataProvider.Instance().GetScheduleHistory(ScheduleID));
        }

        public static Hashtable GetScheduleItemSettings(int ScheduleID)
        {
            var h = new Hashtable();
            using(var r = DataProvider.Instance().GetScheduleItemSettings(ScheduleID))
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
			UpdateSchedule(scheduleItem.ScheduleID,
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
							scheduleItem.FriendlyName);
#pragma warning restore 618
		}

        public static void UpdateSchedule(int ScheduleID, string TypeFullName, int TimeLapse, string TimeLapseMeasurement, int RetryTimeLapse, string RetryTimeLapseMeasurement, int RetainHistoryNum,
                                  string AttachToEvent, bool CatchUpEnabled, bool Enabled, string ObjectDependencies, string Servers, string FriendlyName)
        {
#pragma warning disable 618
			UpdateSchedule(ScheduleID,
                            TypeFullName,
                            TimeLapse,
                            TimeLapseMeasurement,
                            RetryTimeLapse,
                            RetryTimeLapseMeasurement,
                            RetainHistoryNum,
                            AttachToEvent,
                            CatchUpEnabled,
                            Enabled,
                            ObjectDependencies,
                            Servers,
                            FriendlyName);
#pragma warning restore 618
		}

        public static void UpdateScheduleHistory(ScheduleHistoryItem objScheduleHistoryItem)
        {
            DataProvider.Instance().UpdateScheduleHistory(objScheduleHistoryItem.ScheduleHistoryID,
                                                          objScheduleHistoryItem.EndDate,
                                                          objScheduleHistoryItem.Succeeded,
                                                          objScheduleHistoryItem.LogNotes,
                                                          objScheduleHistoryItem.NextStart);
        }

        public static bool CanRunOnThisServer(string servers)
        {
            string lwrServers = "";
            if (servers != null)
            {
                lwrServers = servers.ToLowerInvariant();
            }
            if (String.IsNullOrEmpty(lwrServers) || lwrServers.Contains(Globals.ServerName.ToLowerInvariant()))
            {
                return true;
            }

            return false;
        }

    }
}
