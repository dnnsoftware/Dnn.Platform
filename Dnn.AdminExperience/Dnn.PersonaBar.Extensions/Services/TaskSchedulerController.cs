// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.TaskScheduler.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.TaskScheduler.Services.Dto;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Web.Api;
    using Microsoft.VisualBasic;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class TaskSchedulerController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TaskSchedulerController));
        private static string localResourcesFile = Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.TaskScheduler/App_LocalResources/TaskScheduler.resx");
        private Components.TaskSchedulerController controller = new Components.TaskSchedulerController();

        /// GET: api/TaskScheduler/GetServers
        /// <summary>Gets list of servers.</summary>
        /// <returns>List of servers.</returns>
        [HttpGet]
        public HttpResponseMessage GetServers()
        {
            try
            {
                var servers = ServerController.GetServers();
                var query = from ServerInfo server in servers
                            where server.Enabled == true
                            select server;
                var availableServers = query.Select(v => new
                {
                    ServerID = v.ServerID.ToString(CultureInfo.InvariantCulture),
                    v.ServerName,
                }).ToList();
                availableServers.Insert(0, new
                {
                    ServerID = "*",
                    ServerName = Localization.GetString("All"),
                });
                var response = new
                {
                    Success = true,
                    Results = availableServers,
                    TotalResults = servers.Count,
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/TaskScheduler/GetScheduleItems
        /// <summary>Gets list of schedule items.</summary>
        /// <param name="serverName">The server name (default <c>""</c>).</param>
        /// <returns>List of schedule items.</returns>
        [HttpGet]
        public HttpResponseMessage GetScheduleItems(string serverName = "")
        {
            try
            {
                var scheduleviews = this.controller.GetScheduleItems(null, serverName);
                var arrSchedule = scheduleviews.ToArray();
                var response = new
                {
                    Success = true,
                    Results = arrSchedule.Select(v => new
                    {
                        v.ScheduleID,
                        v.FriendlyName,
                        v.Enabled,
                        RetryTimeLapse = this.controller.GetTimeLapse(v.RetryTimeLapse, v.RetryTimeLapseMeasurement),
                        NextStart = (v.Enabled && !Null.IsNull(v.NextStart)) ? v.NextStart.ToString(CultureInfo.CurrentCulture) : string.Empty,
                        Frequency = this.controller.GetTimeLapse(v.TimeLapse, v.TimeLapseMeasurement),
                    }),
                    TotalResults = arrSchedule.Length,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/TaskScheduler/GetSchedulerSettings
        /// <summary>Gets scheduler settings.</summary>
        /// <returns>scheduler settings.</returns>
        [HttpGet]
        public HttpResponseMessage GetSchedulerSettings()
        {
            try
            {
                KeyValuePair<string, string>[] modes = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(Localization.GetString("Disabled", localResourcesFile), "0"),
                    new KeyValuePair<string, string>(Localization.GetString("TimerMethod", localResourcesFile), "1"),
                    new KeyValuePair<string, string>(Localization.GetString("RequestMethod", localResourcesFile), "2"),
                };

                var response = new
                {
                    Results = new
                    {
                        SchedulerMode = HostController.Instance.GetString("SchedulerMode"),
                        SchedulerModeOptions = modes,
                        SchedulerdelayAtAppStart = HostController.Instance.GetInteger("SchedulerdelayAtAppStart", 1),
                    },
                    TotalResults = 1,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/UpdateSchedulerSettings
        /// <summary>Updates scheduler settings.</summary>
        /// <param name="request">The update request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateSchedulerSettings(UpdateSettingsRequest request)
        {
            try
            {
                var originalSchedulerMode = (SchedulerMode)Convert.ToInt32(HostController.Instance.GetString("SchedulerMode"), CultureInfo.InvariantCulture);
                Enum.TryParse(request.SchedulerMode, true, out SchedulerMode newSchedulerMode);
                if (originalSchedulerMode != newSchedulerMode)
                {
                    switch (newSchedulerMode)
                    {
                        case SchedulerMode.DISABLED:
                            var newThread1 = new Thread(new ThreadStart(Halt)) { IsBackground = true };
                            newThread1.Start();
                            break;
                        case SchedulerMode.TIMER_METHOD:
                            var newThread2 = new Thread(SchedulingProvider.Instance().Start) { IsBackground = true };
                            newThread2.Start();
                            break;
                        default:
                            var newThread3 = new Thread(new ThreadStart(Halt)) { IsBackground = true };
                            newThread3.Start();
                            break;
                    }
                }

                HostController.Instance.Update("SchedulerMode", request.SchedulerMode, false);
                HostController.Instance.Update("SchedulerdelayAtAppStart", request.SchedulerdelayAtAppStart);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/TaskScheduler/GetScheduleItemHistory
        /// <summary>Gets schedule item history.</summary>
        /// <param name="scheduleId">The schedule ID (default -1).</param>
        /// <param name="pageIndex">The page index (default 0).</param>
        /// <param name="pageSize">The page size (default 20).</param>
        /// <returns>schedule item history.</returns>
        [HttpGet]
        public HttpResponseMessage GetScheduleItemHistory(int scheduleId = -1, int pageIndex = 0, int pageSize = 20)
        {
            try
            {
                var query =
                    from ScheduleHistoryItem history in SchedulingProvider.Instance().GetScheduleHistory(scheduleId)
                    select new
                    {
                        history.FriendlyName,
                        history.LogNotes,
                        history.Server,
                        ElapsedTime = Math.Round(history.ElapsedTime, 3),
                        history.Succeeded,
                        StartDate =
                            !Null.IsNull(history.StartDate)
                                ? history.StartDate.ToString(CultureInfo.CurrentCulture)
                                : string.Empty,
                        EndDate =
                            !Null.IsNull(history.EndDate)
                                ? history.EndDate.ToString(CultureInfo.CurrentCulture)
                                : string.Empty,
                        NextStart = !Null.IsNull(history.NextStart)
                            ? history.NextStart.ToString(CultureInfo.CurrentCulture)
                            : string.Empty,
                    };

                var response = new
                {
                    Success = true,
                    Results = query.Skip(pageIndex * pageSize).Take(pageSize),
                    TotalResults = query.Count(),
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/TaskScheduler/GetScheduleItem
        /// <summary>Gets an existing schedule item.</summary>
        /// <param name="scheduleId">The schedule ID.</param>
        /// <returns>schedule item.</returns>
        [HttpGet]
        public HttpResponseMessage GetScheduleItem(int scheduleId)
        {
            try
            {
                ScheduleItem scheduleItem = SchedulingProvider.Instance().GetSchedule(scheduleId);

                var recommendedServers = this.controller.GetRecommendedServers(scheduleItem.ScheduleID);

                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        scheduleItem.ScheduleID,
                        scheduleItem.FriendlyName,
                        scheduleItem.TypeFullName,
                        scheduleItem.Enabled,
                        ScheduleStartDate = !Null.IsNull(scheduleItem.ScheduleStartDate) ? scheduleItem.ScheduleStartDate.ToString(CultureInfo.CurrentCulture.DateTimeFormat.SortableDateTimePattern, CultureInfo.CurrentCulture) : string.Empty,
                        Locale = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                        scheduleItem.TimeLapse,
                        scheduleItem.TimeLapseMeasurement,
                        scheduleItem.RetryTimeLapse,
                        scheduleItem.RetryTimeLapseMeasurement,
                        scheduleItem.RetainHistoryNum,
                        scheduleItem.AttachToEvent,
                        scheduleItem.CatchUpEnabled,
                        scheduleItem.ObjectDependencies,
                        scheduleItem.Servers,
                        RecommendedServers = recommendedServers,
                    },
                    TotalResults = 1,
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/CreateScheduleItem
        /// <summary>Creates a new schedule item.</summary>
        /// <param name="scheduleDto">The schedule info.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage CreateScheduleItem(ScheduleDto scheduleDto)
        {
            try
            {
                if (scheduleDto.RetryTimeLapse == 0)
                {
                    scheduleDto.RetryTimeLapse = Null.NullInteger;
                }

                if (!VerifyValidTimeLapseRetry(scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement, scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("InvalidFrequencyAndRetry", localResourcesFile));
                }

                var scheduleItem = this.controller.CreateScheduleItem(
                    scheduleDto.TypeFullName,
                    scheduleDto.FriendlyName,
                    scheduleDto.TimeLapse,
                    scheduleDto.TimeLapseMeasurement,
                    scheduleDto.RetryTimeLapse,
                    scheduleDto.RetryTimeLapseMeasurement,
                    scheduleDto.RetainHistoryNum,
                    scheduleDto.AttachToEvent,
                    scheduleDto.CatchUpEnabled,
                    scheduleDto.Enabled,
                    scheduleDto.ObjectDependencies,
                    scheduleDto.ScheduleStartDate,
                    scheduleDto.Servers);
                SchedulingProvider.Instance().AddSchedule(scheduleItem);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/UpdateScheduleItem
        /// <summary>Updates an existing schedule item.</summary>
        /// <param name="scheduleDto">The schedule info.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateScheduleItem(ScheduleDto scheduleDto)
        {
            try
            {
                if (scheduleDto.RetryTimeLapse == 0)
                {
                    scheduleDto.RetryTimeLapse = Null.NullInteger;
                }

                if (!VerifyValidTimeLapseRetry(scheduleDto.TimeLapse, scheduleDto.TimeLapseMeasurement, scheduleDto.RetryTimeLapse, scheduleDto.RetryTimeLapseMeasurement))
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, Localization.GetString("InvalidFrequencyAndRetry", localResourcesFile));
                }

                var existingItem = SchedulingProvider.Instance().GetSchedule(scheduleDto.ScheduleID);

                var updatedItem = this.controller.CreateScheduleItem(
                    scheduleDto.TypeFullName,
                    scheduleDto.FriendlyName,
                    scheduleDto.TimeLapse,
                    scheduleDto.TimeLapseMeasurement,
                    scheduleDto.RetryTimeLapse,
                    scheduleDto.RetryTimeLapseMeasurement,
                    scheduleDto.RetainHistoryNum,
                    scheduleDto.AttachToEvent,
                    scheduleDto.CatchUpEnabled,
                    scheduleDto.Enabled,
                    scheduleDto.ObjectDependencies,
                    scheduleDto.ScheduleStartDate,
                    scheduleDto.Servers);
                updatedItem.ScheduleID = scheduleDto.ScheduleID;

                if (updatedItem.ScheduleStartDate != existingItem.ScheduleStartDate ||
                    updatedItem.Enabled ||
                    updatedItem.Enabled != existingItem.Enabled ||
                    updatedItem.TimeLapse != existingItem.TimeLapse ||
                    updatedItem.RetryTimeLapse != existingItem.RetryTimeLapse ||
                    updatedItem.RetryTimeLapseMeasurement != existingItem.RetryTimeLapseMeasurement ||
                    updatedItem.TimeLapseMeasurement != existingItem.TimeLapseMeasurement)
                {
                    SchedulingProvider.Instance().UpdateSchedule(updatedItem);
                }
                else
                {
                    SchedulingProvider.Instance().UpdateScheduleWithoutExecution(updatedItem);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// GET: api/TaskScheduler/GetScheduleStatus
        /// <summary>Gets schedule status.</summary>
        /// <returns>schedule status.</returns>
        [HttpGet]
        public HttpResponseMessage GetScheduleStatus()
        {
            try
            {
                if (SchedulingProvider.Enabled)
                {
                    var processing =
                        from ScheduleHistoryItem item in SchedulingProvider.Instance().GetScheduleProcessing()
                        select new
                        {
                            item.ScheduleID,
                            item.TypeFullName,
                            StartDate = !Null.IsNull(item.StartDate) ? item.StartDate.ToString(CultureInfo.CurrentCulture) : string.Empty,
                            ElapsedTime = Math.Round(item.ElapsedTime, 3),
                            item.ObjectDependencies,
                            ScheduleSource = item.ScheduleSource.ToString(),
                            item.ThreadID,
                            item.Servers,
                        };

                    var queue =
                        from ScheduleHistoryItem item in SchedulingProvider.Instance().GetScheduleQueue()
                        select new
                        {
                            item.ScheduleID,
                            item.FriendlyName,
                            NextStart = !Null.IsNull(item.NextStart) ? item.NextStart.ToString(CultureInfo.CurrentCulture) : string.Empty,
                            item.Overdue,
                            RemainingTime = GetTimeStringFromSeconds(item.RemainingTime),
                            RemainingSeconds = item.RemainingTime,
                            item.ObjectDependencies,
                            ScheduleSource = item.ScheduleSource.ToString(),
                            item.ThreadID,
                            item.Servers,
                        };

                    var response = new
                    {
                        Success = true,
                        Results = new
                        {
                            ServerTime = DateTime.Now.ToString(CultureInfo.CurrentCulture),
                            SchedulingEnabled = SchedulingProvider.Enabled.ToString(),
                            Status = SchedulingProvider.Instance().GetScheduleStatus().ToString(),
                            FreeThreadCount = SchedulingProvider.Instance().GetFreeThreadCount().ToString(CultureInfo.InvariantCulture),
                            ActiveThreadCount = SchedulingProvider.Instance().GetActiveThreadCount().ToString(CultureInfo.InvariantCulture),
                            MaxThreadCount = SchedulingProvider.Instance().GetMaxThreadCount().ToString(CultureInfo.InvariantCulture),
                            ScheduleProcessing = processing,
                            ScheduleQueue = queue.ToList().OrderBy(q => q.RemainingSeconds),
                        },
                        TotalResults = 1,
                    };
                    return this.Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    var response = new
                    {
                        Success = true,
                        Results = new
                        {
                            SchedulingEnabled = "False",
                            Status = Localization.GetString("Disabled", localResourcesFile),
                            FreeThreadCount = "0",
                            ActiveThreadCount = "0",
                            MaxThreadCount = "0",
                            ScheduleProcessing = new List<string>(),
                            ScheduleQueue = new List<string>(),
                        },
                        TotalResults = 1,
                    };
                    return this.Request.CreateResponse(HttpStatusCode.OK, response);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/StartSchedule
        /// <summary>Starts schedule.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage StartSchedule()
        {
            try
            {
                SchedulingProvider.Instance().StartAndWaitForResponse();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/StopSchedule
        /// <summary>Stops schedule.</summary>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage StopSchedule()
        {
            try
            {
                this.controller.StopSchedule();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/RunSchedule
        /// <summary>Runs schedule.</summary>
        /// <param name="scheduleDto">The schedule info.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RunSchedule(ScheduleDto scheduleDto)
        {
            try
            {
                var scheduleItem = this.controller.CreateScheduleItem(
                    scheduleDto.TypeFullName,
                    scheduleDto.FriendlyName,
                    scheduleDto.TimeLapse,
                    scheduleDto.TimeLapseMeasurement,
                    scheduleDto.RetryTimeLapse,
                    scheduleDto.RetryTimeLapseMeasurement,
                    scheduleDto.RetainHistoryNum,
                    scheduleDto.AttachToEvent,
                    scheduleDto.CatchUpEnabled,
                    scheduleDto.Enabled,
                    scheduleDto.ObjectDependencies,
                    scheduleDto.ScheduleStartDate,
                    scheduleDto.Servers);
                scheduleItem.ScheduleID = scheduleDto.ScheduleID;
                SchedulingProvider.Instance().RunScheduleItemNow(scheduleItem, true);

                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                {
                    SchedulingProvider.Instance().ReStart("Change made to schedule.");
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        /// POST: api/TaskScheduler/DeleteSchedule
        /// <summary>Runs schedule.</summary>
        /// <param name="scheduleDto">The schedule info.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteSchedule(ScheduleDto scheduleDto)
        {
            try
            {
                var objScheduleItem = new ScheduleItem { ScheduleID = scheduleDto.ScheduleID };
                SchedulingProvider.Instance().DeleteSchedule(objScheduleItem);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static DateTime CalculateTime(int lapse, string measurement)
        {
            var nextTime = default(DateTime);
            switch (measurement)
            {
                case "s":
                    nextTime = DateTime.Now.AddSeconds(lapse);
                    break;
                case "m":
                    nextTime = DateTime.Now.AddMinutes(lapse);
                    break;
                case "h":
                    nextTime = DateTime.Now.AddHours(lapse);
                    break;
                case "d":
                    nextTime = DateTime.Now.AddDays(lapse);
                    break;
                case "w":
                    nextTime = DateTime.Now.AddDays(lapse);
                    break;
                case "mo":
                    nextTime = DateTime.Now.AddMonths(lapse);
                    break;
                case "y":
                    nextTime = DateTime.Now.AddYears(lapse);
                    break;
            }

            return nextTime;
        }

        private static void Halt()
        {
            SchedulingProvider.Instance().Halt("Host Settings");
        }

        private static string GetTimeStringFromSeconds(double sec)
        {
            var time = TimeSpan.FromSeconds(sec);
            if (time.Days > 0)
            {
                return $"{time.Days} {Localization.GetString(time.Days == 1 ? "DaySingular" : "DayPlural", localResourcesFile)}";
            }

            if (time.Hours > 0)
            {
                return $"{time.Hours} {Localization.GetString(time.Hours == 1 ? "HourSingular" : "HourPlural", localResourcesFile)}";
            }

            if (time.Minutes > 0)
            {
                return $"{time.Minutes} {Localization.GetString(time.Minutes == 1 ? "MinuteSingular" : "MinutePlural", localResourcesFile)}";
            }

            return Localization.GetString("LessThanMinute", localResourcesFile);
        }

        private static bool VerifyValidTimeLapseRetry(int timeLapse, string timeLapseMeasurement, int retryTimeLapse, string retryTimeLapseMeasurement)
        {
            if (retryTimeLapse == 0)
            {
                return true;
            }

            var frequency = CalculateTime(Convert.ToInt32(timeLapse), timeLapseMeasurement);
            var retry = CalculateTime(Convert.ToInt32(retryTimeLapse), retryTimeLapseMeasurement);
            if (retry > frequency)
            {
                return false;
            }

            return true;
        }
    }
}
