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



#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Scheduling;

namespace Dnn.PersonaBar.TaskScheduler.Components
{
    public class TaskSchedulerController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TaskSchedulerController));

        private string LocalResourcesFile
        {
            get
            {
                return Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.TaskScheduler/App_LocalResources/TaskScheduler.resx");
            }
        }

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

        public void StopSchedule()
        {
            SchedulingProvider.Instance().Halt(Localization.GetString("ManuallyStopped", LocalResourcesFile));
        }

        public ScheduleItem CreateScheduleItem(string typeFullName, string friendlyName, int timeLapse, string timeLapseMeasurement,
            int retryTimeLapse, string retryTimeLapseMeasurement, int retainHistoryNum, string attachToEvent, bool catchUpEnabled,
            bool enabled, string objectDependencies, string scheduleStartDate, string servers)
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
        public IEnumerable<ScheduleItem> GetScheduleItems(bool? enabled, string serverName = "", string taskName = "")
        {
            try
            {
                IEnumerable<ScheduleItem> scheduleviews;
                if (string.IsNullOrEmpty(serverName) || serverName == Localization.GetString("All"))
                {
                    scheduleviews = SchedulingController.GetSchedule();
                }
                else
                {
                    scheduleviews = SchedulingController.GetSchedule(serverName);
                }
                if (!string.IsNullOrEmpty(taskName))
                    scheduleviews = scheduleviews.Where(item => item.FriendlyName.IndexOf(taskName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (enabled.HasValue)
                    scheduleviews = scheduleviews.Where(item => item.Enabled == enabled.Value);

                var scheduleItems = scheduleviews as IList<ScheduleItem> ?? scheduleviews.ToList();
                foreach (var item in scheduleItems.Where(x => x.NextStart == Null.NullDate)
                            .Where(item => item.ScheduleStartDate != Null.NullDate))
                    item.NextStart = item.ScheduleStartDate;

                return scheduleItems;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return null;
            }
        }
    }
}