// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.TaskScheduler.Components
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using Microsoft.Extensions.DependencyInjection;

    public class TaskSchedulerController
    {
        private static readonly string SchedulersToRunOnSameWebServerKey = "SchedulersToRunOnSameWebServer";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TaskSchedulerController));

        private static string LocalResourcesFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.TaskScheduler/App_LocalResources/TaskScheduler.resx");

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string GetTimeLapse(int timeLapse, string timeLapseMeasurement)
        {
            if (timeLapse != Null.NullInteger)
            {
                var str = Null.NullString;
                var strPrefix = Localization.GetString("TimeLapsePrefix", LocalResourcesFile);
                var strSec = Localization.GetString("Second", LocalResourcesFile);
                var strMn = Localization.GetString("Minute", LocalResourcesFile);
                var strHour = Localization.GetString("Hour", LocalResourcesFile);
                var strDay = Localization.GetString("Day", LocalResourcesFile);
                var strWeek = Localization.GetString("Week", LocalResourcesFile);
                var strMonth = Localization.GetString("Month", LocalResourcesFile);
                var strYear = Localization.GetString("Year", LocalResourcesFile);
                var strSecs = Localization.GetString("Seconds");
                var strMns = Localization.GetString("Minutes");
                var strHours = Localization.GetString("Hours");
                var strDays = Localization.GetString("Days");
                var strWeeks = Localization.GetString("Weeks");
                var strMonths = Localization.GetString("Months");
                var strYears = Localization.GetString("Years");
                switch (timeLapseMeasurement)
                {
                    case "s":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strSecs : strSec);
                        break;
                    case "m":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMns : strMn);
                        break;
                    case "h":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strHours : strHour);
                        break;
                    case "d":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strDays : strDay);
                        break;
                    case "w":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strWeeks : strWeek);
                        break;
                    case "mo":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strMonths : strMonth);
                        break;
                    case "y":
                        str = strPrefix + " " + timeLapse + " " + (timeLapse > 1 ? strYears : strYear);
                        break;
                }

                return str;
            }

            return Localization.GetString("n/a", LocalResourcesFile);
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public void StopSchedule()
        {
            SchedulingProvider.Instance().Halt(Localization.GetString("ManuallyStopped", LocalResourcesFile));
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public ScheduleItem CreateScheduleItem(string typeFullName, string friendlyName, int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled, bool enabled, string objectDependencies, string scheduleStartDate, string servers)
        {
            var scheduleItem = new ScheduleItem();
            scheduleItem.TypeFullName = typeFullName;
            scheduleItem.FriendlyName = friendlyName;
            scheduleItem.TimeLapse = timeLapse;
            scheduleItem.TimeLapseMeasurement = string.IsNullOrEmpty(timeLapseMeasurement) ? "s" : timeLapseMeasurement;
            scheduleItem.RetryTimeLapse = retryTimeLapse;
            scheduleItem.RetryTimeLapseMeasurement = string.IsNullOrEmpty(retryTimeLapseMeasurement) ? "s" : retryTimeLapseMeasurement;
            scheduleItem.RetainHistoryNum = retainHistoryNum;
            scheduleItem.AttachToEvent = string.IsNullOrEmpty(attachToEvent) ? string.Empty : attachToEvent;
            scheduleItem.CatchUpEnabled = catchUpEnabled;
            scheduleItem.Enabled = enabled;
            scheduleItem.ObjectDependencies = string.IsNullOrEmpty(objectDependencies) ? string.Empty : objectDependencies;
            scheduleItem.ScheduleStartDate = !string.IsNullOrEmpty(scheduleStartDate) ? Convert.ToDateTime(scheduleStartDate, CultureInfo.InvariantCulture) : Null.NullDate;

            if (!string.IsNullOrEmpty(servers))
            {
                if (!servers.StartsWith(","))
                {
                    servers = "," + servers;
                }

                if (!servers.EndsWith(","))
                {
                    servers = servers + ",";
                }
            }

            scheduleItem.Servers = string.IsNullOrEmpty(servers) ? Null.NullString : servers;
            return scheduleItem;
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<ScheduleItem> GetScheduleItems(bool? enabled, string serverName = "", string taskName = "")
        {
            try
            {
                IEnumerable<ScheduleItem> scheduleViews;
                if (string.IsNullOrEmpty(serverName) || serverName == Localization.GetString("All"))
                {
                    scheduleViews = SchedulingController.GetSchedule();
                }
                else
                {
                    scheduleViews = SchedulingController.GetSchedule(serverName);
                }

                if (!string.IsNullOrEmpty(taskName))
                {
                    scheduleViews = scheduleViews.Where(item => item.FriendlyName.IndexOf(taskName, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (enabled.HasValue)
                {
                    scheduleViews = scheduleViews.Where(item => item.Enabled == enabled.Value);
                }

                var scheduleItems = scheduleViews as IList<ScheduleItem> ?? scheduleViews.ToList();
                foreach (var item in scheduleItems.Where(x => x.NextStart == Null.NullDate)
                            .Where(item => item.ScheduleStartDate != Null.NullDate))
                {
                    item.NextStart = item.ScheduleStartDate;
                }

                return scheduleItems;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
        }

        /// <summary>Gets a list of servers to be recommended for a particular scheduler.</summary>
        /// <param name="schedulerId">Scheduler Id.</param>
        /// <returns>List of recommended servers for specified <paramref name="schedulerId"/>.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public IEnumerable<string> GetRecommendedServers(int schedulerId)
        {
            var hostSettingsService = Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>();

            var schedulerIds = hostSettingsService.GetString(SchedulersToRunOnSameWebServerKey, string.Empty)
                .Split([',',], StringSplitOptions.RemoveEmptyEntries)
                .Where(x => int.TryParse(x, out var id))
                .Select(x => int.Parse(x, CultureInfo.InvariantCulture))
                .ToArray();

            if (!schedulerIds.Contains(schedulerId))
            {
                return [];
            }

            return SchedulingProvider.Instance().GetSchedule()
                .Cast<ScheduleItem>()
                .Where(x => x.ScheduleID != schedulerId
                            && x.Enabled
                            && schedulerIds.Contains(x.ScheduleID)
                            && !string.IsNullOrWhiteSpace(x.Servers))
                .SelectMany(x => x.Servers
                    .Split([',',], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x);
        }
    }
}
