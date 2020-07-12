// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;
    using Microsoft.VisualBasic;

    internal static class Scheduler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(Scheduler));

        internal static class CoreScheduler
        {
            // If KeepRunning gets switched to false,
            // the scheduler stops running.
            public static bool KeepThreadAlive = true;
            public static bool KeepRunning = true;

            private static readonly SharedList<ScheduleItem> ScheduleQueue;
            private static readonly SharedList<ScheduleHistoryItem> ScheduleInProgress;
            private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(45);
            private static readonly ReaderWriterLockSlim StatusLock = new ReaderWriterLockSlim();

            // This is the heart of the scheduler mechanism.
            // This class manages running new events according
            // to the schedule.
            //
            // This class can also react to the three
            // scheduler events (Started, Progressing and Completed)
            private static bool _threadPoolInitialized;

            // The MaxThreadCount establishes the maximum
            // threads you want running simultaneously
            // for spawning SchedulerClient processes
            private static int _maxThreadCount;
            private static int _activeThreadCount;

            // If KeepRunning gets switched to false,
            // the scheduler stops running.
            private static bool _forceReloadSchedule;
            private static bool _debug;

            private static int _numberOfProcessGroups;

            // This is our array that holds the process group
            // where our threads will be kicked off.
            private static ProcessGroup[] _processGroup;

            // A ReaderWriterLockSlim will protect our objects
            // in memory from being corrupted by simultaneous
            // thread operations.  This block of code below
            // establishes variables to help keep track
            // of the ReaderWriter locks.
            private static int _readerTimeouts;
            private static int _writerTimeouts;
            private static ScheduleStatus _status = ScheduleStatus.STOPPED;

            static CoreScheduler()
            {
                var lockStrategy = new ReaderWriterLockStrategy(LockRecursionPolicy.SupportsRecursion);

                ScheduleQueue = new SharedList<ScheduleItem>(lockStrategy);
                ScheduleInProgress = new SharedList<ScheduleHistoryItem>(lockStrategy);
            }

            private delegate void AddToScheduleInProgressDelegate(ScheduleHistoryItem item);

            /// <summary>
            /// Gets tracks how many threads we have free to work with at any given time.
            /// </summary>
            public static int FreeThreads
            {
                get
                {
                    return _maxThreadCount - _activeThreadCount;
                }
            }

            public static ScheduleHistoryItem AddScheduleHistory(ScheduleHistoryItem scheduleHistoryItem)
            {
                try
                {
                    int scheduleHistoryID = SchedulingController.AddScheduleHistory(scheduleHistoryItem);
                    scheduleHistoryItem.ScheduleHistoryID = scheduleHistoryID;
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }

                return scheduleHistoryItem;
            }

            /// <summary>
            /// Adds an item to the collection of schedule items in queue.
            /// </summary>
            /// <param name="scheduleHistoryItem"></param>
            /// <remarks>Thread Safe.</remarks>
            public static void AddToScheduleQueue(ScheduleHistoryItem scheduleHistoryItem)
            {
                if (!ScheduleQueueContains(scheduleHistoryItem))
                {
                    try
                    {
                        // objQueueReadWriteLock.EnterWriteLock(WriteTimeout)
                        using (ScheduleQueue.GetWriteLock(LockTimeout))
                        {
                            // Do a second check just in case
                            if (!ScheduleQueueContains(scheduleHistoryItem) &&
                                !IsInProgress(scheduleHistoryItem))
                            {
                                // It is safe for this thread to read or write
                                // from the shared resource.
                                ScheduleQueue.Add(scheduleHistoryItem);
                            }
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        // The writer lock request timed out.
                        Interlocked.Increment(ref _writerTimeouts);
                        Exceptions.Exceptions.LogException(ex);
                    }
                }
            }

            public static void FireEvents()
            {
                // This method uses a thread pool to
                // call the SchedulerClient methods that need
                // to be called.

                // For each item in the queue that there
                // is an open thread for, set the object
                // in the array to a new ProcessGroup object.
                // Pass in the ScheduleItem to the ProcessGroup
                // so the ProcessGroup can pass it around for
                // logging and notifications.
                lock (ScheduleQueue)
                {
                    var scheduleList = new List<ScheduleItem>();
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleItem scheduleItem in ScheduleQueue)
                        {
                            scheduleList.Add(scheduleItem);
                        }
                    }

                    int numToRun = scheduleList.Count;
                    int numRun = 0;

                    foreach (ScheduleItem scheduleItem in scheduleList)
                    {
                        if (!KeepRunning)
                        {
                            return;
                        }

                        int processGroup = GetProcessGroup();

                        if (scheduleItem.NextStart <= DateTime.Now &&
                            scheduleItem.Enabled &&
                            !IsInProgress(scheduleItem) &&
                            !HasDependenciesConflict(scheduleItem) &&
                            numRun < numToRun)
                        {
                            scheduleItem.ProcessGroup = processGroup;
                            if (scheduleItem.ScheduleSource == ScheduleSource.NOT_SET)
                            {
                                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                                {
                                    scheduleItem.ScheduleSource = ScheduleSource.STARTED_FROM_TIMER;
                                }
                                else if (SchedulingProvider.SchedulerMode == SchedulerMode.REQUEST_METHOD)
                                {
                                    scheduleItem.ScheduleSource = ScheduleSource.STARTED_FROM_BEGIN_REQUEST;
                                }
                            }

                            var delegateFunc = new AddToScheduleInProgressDelegate(AddToScheduleInProgress);
                            var scheduleHistoryItem = new ScheduleHistoryItem(scheduleItem);
                            scheduleHistoryItem.StartDate = DateTime.Now;
                            delegateFunc.BeginInvoke(scheduleHistoryItem, null, null);
                            Thread.Sleep(1000);

                            _processGroup[processGroup].AddQueueUserWorkItem(scheduleItem);

                            LogEventAddedToProcessGroup(scheduleItem);
                            numRun += 1;
                        }
                        else
                        {
                            LogWhyTaskNotRun(scheduleItem);
                        }
                    }
                }
            }

            public static int GetActiveThreadCount()
            {
                return _activeThreadCount;
            }

            public static int GetFreeThreadCount()
            {
                return FreeThreads;
            }

            public static int GetMaxThreadCount()
            {
                return _maxThreadCount;
            }

            /// <summary>
            /// Gets a copy of the collection of schedule items in progress.
            /// </summary>
            /// <returns>Copy of the schedule items currently in progress.</returns>
            /// <remarks>This is a snapshot of the collection scheduled items could start or complete at any time.</remarks>
            public static Collection GetScheduleInProgress()
            {
                var c = new Collection();
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleHistoryItem item in ScheduleInProgress)
                        {
                            c.Add(item, item.ScheduleID.ToString(), null, null);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

                return c;
            }

            /// <summary>
            /// Gets the number of items in the collection of schedule items in progress.
            /// </summary>
            /// <returns>Number of items in progress.</returns>
            /// <remarks>Thread Safe
            /// This count is a snapshot and may change at any time.
            /// </remarks>
            public static int GetScheduleInProgressCount()
            {
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Count;
                    }
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

                    return 0;
                }
            }

            /// <summary>
            /// Gets a copy of collection of all schedule items in queue.
            /// </summary>
            /// <returns>A copy of the ScheduleQueue.</returns>
            /// <remarks>Thread Safe
            /// The returned collection is a snapshot in time the real ScheduleQueue may change at any time.
            /// </remarks>
            public static Collection GetScheduleQueue()
            {
                var c = new Collection();
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        foreach (ScheduleItem item in ScheduleQueue)
                        {
                            c.Add(item, item.ScheduleID.ToString(), null, null);
                        }
                    }

                    return c;
                }
                catch (ApplicationException ex)
                {
                    Interlocked.Increment(ref _readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

                return c;
            }

            /// <summary>
            /// Gets the number of items in the collection of schedule items in progress.
            /// </summary>
            /// <returns>Number of items in progress.</returns>
            /// <remarks>Thread Safe
            /// This count is a snapshot and may change at any time.
            /// </remarks>
            public static int GetScheduleQueueCount()
            {
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Count;
                    }
                }
                catch (ApplicationException)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    return 0;
                }
            }

            public static ScheduleStatus GetScheduleStatus()
            {
                try
                {
                    if (StatusLock.TryEnterReadLock(LockTimeout))
                    {
                        try
                        {
                            // ScheduleStatus is a value type a copy is returned (enumeration)
                            return _status;
                        }
                        finally
                        {
                            StatusLock.ExitReadLock();
                        }
                    }
                }
                catch (ApplicationException)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                }

                return ScheduleStatus.NOT_SET;
            }

            /// <summary>
            /// Halt the Scheduler.
            /// </summary>
            /// <param name="sourceOfHalt">Initiator of Halt.</param>
            public static void Halt(string sourceOfHalt)
            {
                // should do nothing if the scheduler havn't start yet.
                var currentStatus = GetScheduleStatus();
                if (currentStatus == ScheduleStatus.NOT_SET || currentStatus == ScheduleStatus.STOPPED)
                {
                    return;
                }

                SetScheduleStatus(ScheduleStatus.SHUTTING_DOWN);
                var log = new LogInfo { LogTypeKey = "SCHEDULER_SHUTTING_DOWN" };
                log.AddProperty("Initiator", sourceOfHalt);
                LogController.Instance.AddLog(log);

                KeepRunning = false;

                // wait for up to 120 seconds for thread
                // to shut down
                for (int i = 0; i <= 120; i++)
                {
                    if (GetScheduleStatus() == ScheduleStatus.STOPPED)
                    {
                        return;
                    }

                    Thread.Sleep(1000);
                }

                _activeThreadCount = 0;
            }

            public static bool HasDependenciesConflict(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        if (scheduleItem.ObjectDependencies.Any())
                        {
                            foreach (ScheduleHistoryItem item in ScheduleInProgress.Where(si => si.ObjectDependencies.Any()))
                            {
                                if (item.HasObjectDependencies(scheduleItem.ObjectDependencies))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

                    return false;
                }
            }

            public static void LoadQueueFromEvent(EventName eventName)
            {
                var executingServer = ServerController.GetExecutingServerName();
                List<ScheduleItem> schedule = SchedulingController.GetScheduleByEvent(eventName.ToString(), executingServer);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("loadqueue executingServer:" + executingServer);
                }

                var thisServer = GetServer(executingServer);
                if (thisServer == null)
                {
                    return;
                }

                bool runningInAGroup = !string.IsNullOrEmpty(thisServer.ServerGroup);

                var serverGroupServers = ServerGroupServers(thisServer);

                foreach (ScheduleItem scheduleItem in schedule)
                {
                    if (runningInAGroup && string.IsNullOrEmpty(scheduleItem.Servers))
                    {
                        scheduleItem.Servers = serverGroupServers;
                    }

                    var historyItem = new ScheduleHistoryItem(scheduleItem);

                    if (!IsInQueue(historyItem) &&
                        !IsInProgress(historyItem) &&
                        !HasDependenciesConflict(historyItem) &&
                        historyItem.Enabled)
                    {
                        historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_EVENT;
                        AddToScheduleQueue(historyItem);
                    }
                }
            }

            public static void LoadQueueFromTimer()
            {
                _forceReloadSchedule = false;
                var executingServer = ServerController.GetExecutingServerName();
                List<ScheduleItem> schedule = SchedulingController.GetSchedule(executingServer);
                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("LoadQueueFromTimer executingServer:" + executingServer);
                }

                var thisServer = GetServer(executingServer);
                if (thisServer == null)
                {
                    return;
                }

                bool runningInAGroup = !string.IsNullOrEmpty(thisServer.ServerGroup);

                var serverGroupServers = ServerGroupServers(thisServer);

                foreach (ScheduleItem scheduleItem in schedule)
                {
                    if (runningInAGroup && string.IsNullOrEmpty(scheduleItem.Servers))
                    {
                        scheduleItem.Servers = serverGroupServers;
                    }

                    var historyItem = new ScheduleHistoryItem(scheduleItem);

                    if (!IsInQueue(historyItem) &&
                        historyItem.TimeLapse != Null.NullInteger &&
                        historyItem.TimeLapseMeasurement != Null.NullString &&
                        historyItem.Enabled)
                    {
                        if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                        {
                            historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_TIMER;
                        }
                        else if (SchedulingProvider.SchedulerMode == SchedulerMode.REQUEST_METHOD)
                        {
                            historyItem.ScheduleSource = ScheduleSource.STARTED_FROM_BEGIN_REQUEST;
                        }

                        AddToScheduleQueue(historyItem);
                    }
                }
            }

            public static void PurgeScheduleHistory()
            {
                SchedulingController.PurgeScheduleHistory();
            }

            public static void ReloadSchedule()
            {
                _forceReloadSchedule = true;
            }

            /// <summary>
            /// Removes an item from the collection of schedule items in queue.
            /// </summary>
            /// <param name="scheduleItem">Item to remove.</param>
            public static void RemoveFromScheduleQueue(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleQueue.GetWriteLock(LockTimeout))
                    {
                        // the scheduleitem instances may not be equal even though the scheduleids are equal
                        var item = ScheduleQueue.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
                        if (item != null)
                        {
                            ScheduleQueue.Remove(item);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref _writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            public static void RunEventSchedule(EventName eventName)
            {
                try
                {
                    var log = new LogInfo { LogTypeKey = "SCHEDULE_FIRED_FROM_EVENT" };
                    log.AddProperty("EVENT", eventName.ToString());
                    LogController.Instance.AddLog(log);

                    // We allow for three threads to run simultaneously.
                    // As long as we have an open thread, continue.

                    // Load the queue to determine which schedule
                    // items need to be run.
                    LoadQueueFromEvent(eventName);

                    while (GetScheduleQueueCount() > 0)
                    {
                        SetScheduleStatus(ScheduleStatus.RUNNING_EVENT_SCHEDULE);

                        // Fire off the events that need running.
                        if (GetScheduleQueueCount() > 0)
                        {
                            FireEvents();
                        }

                        if (_writerTimeouts > 20 || _readerTimeouts > 20)
                        {
                            // Wait for 10 minutes so we don't fill up the logs
                            Thread.Sleep(TimeSpan.FromMinutes(10));
                        }
                        else
                        {
                            // Wait for 10 seconds to avoid cpu overutilization
                            Thread.Sleep(TimeSpan.FromSeconds(10));
                        }

                        if (GetScheduleQueueCount() == 0)
                        {
                            return;
                        }
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void SetScheduleStatus(ScheduleStatus newStatus)
            {
                try
                {
                    // note:locking inside this method is highly misleading
                    // as there is no lock in place between when the caller
                    // decides to call this method and when the lock is acquired
                    // the value could easily change in that time
                    if (StatusLock.TryEnterWriteLock(LockTimeout))
                    {
                        try
                        {
                            // It is safe for this thread to read or write
                            // from the shared resource.
                            _status = newStatus;
                        }
                        finally
                        {
                            // Ensure that the lock is released.
                            StatusLock.ExitWriteLock();
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref _writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            public static void Start()
            {
                try
                {
                    AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

                    _activeThreadCount = 0;

                    // This is where the action begins.
                    // Loop until KeepRunning = false
                    if (SchedulingProvider.SchedulerMode != SchedulerMode.REQUEST_METHOD || _debug)
                    {
                        var log = new LogInfo();
                        log.LogTypeKey = "SCHEDULER_STARTED";
                        LogController.Instance.AddLog(log);
                    }

                    while (KeepRunning)
                    {
                        try
                        {
                            if (Common.Globals.ElapsedSinceAppStart.TotalSeconds < SchedulingProvider.DelayAtAppStart)
                            {
                                if (!KeepThreadAlive)
                                {
                                    return;
                                }

                                Thread.Sleep(1000);
                                continue;
                            }

                            if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                            {
                                SetScheduleStatus(ScheduleStatus.RUNNING_TIMER_SCHEDULE);
                            }
                            else
                            {
                                SetScheduleStatus(ScheduleStatus.RUNNING_REQUEST_SCHEDULE);
                            }

                            // Load the queue to determine which schedule
                            // items need to be run.
                            LoadQueueFromTimer();

                            // Keep track of when the queue was last refreshed
                            // so we can perform a refresh periodically
                            DateTime lastQueueRefresh = DateTime.Now;
                            bool refreshQueueSchedule = false;

                            // We allow for [MaxThreadCount] threads to run
                            // simultaneously.  As long as we have an open thread
                            // and we don't have to refresh the queue, continue
                            // to loop.
                            // refreshQueueSchedule can get set to true near bottom of loop
                            // not sure why R# thinks it is always false
                            // ReSharper disable ConditionIsAlwaysTrueOrFalse
                            while (FreeThreads > 0 && !refreshQueueSchedule && KeepRunning && !_forceReloadSchedule)

                            // ReSharper restore ConditionIsAlwaysTrueOrFalse
                            {
                                // Fire off the events that need running.
                                if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD)
                                {
                                    SetScheduleStatus(ScheduleStatus.RUNNING_TIMER_SCHEDULE);
                                }
                                else
                                {
                                    SetScheduleStatus(ScheduleStatus.RUNNING_REQUEST_SCHEDULE);
                                }

                                // It is safe for this thread to read from
                                // the shared resource.
                                if (GetScheduleQueueCount() > 0)
                                {
                                    FireEvents();
                                }

                                if (KeepThreadAlive == false)
                                {
                                    return;
                                }

                                if (_writerTimeouts > 20 || _readerTimeouts > 20)
                                {
                                    // Some kind of deadlock on a resource.
                                    // Wait for 10 minutes so we don't fill up the logs
                                    if (KeepRunning)
                                    {
                                        Thread.Sleep(TimeSpan.FromMinutes(10));
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    // Wait for 10 seconds to avoid cpu overutilization
                                    if (KeepRunning)
                                    {
                                        Thread.Sleep(TimeSpan.FromSeconds(10));
                                    }
                                    else
                                    {
                                        return;
                                    }

                                    // Refresh queue from database every 10 minutes
                                    // if there are no items currently in progress
                                    if ((lastQueueRefresh.AddMinutes(10) <= DateTime.Now || _forceReloadSchedule) && FreeThreads == _maxThreadCount)
                                    {
                                        refreshQueueSchedule = true;
                                        break;
                                    }
                                }
                            }

                            // There are no available threads, all threads are being
                            // used.  Wait 10 seconds until one is available
                            if (KeepRunning)
                            {
                                if (refreshQueueSchedule == false)
                                {
                                    SetScheduleStatus(ScheduleStatus.WAITING_FOR_OPEN_THREAD);
                                    Thread.Sleep(10000); // sleep for 10 seconds
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                        catch (Exception exc)
                        {
                            Exceptions.Exceptions.ProcessSchedulerException(exc);

                            // sleep for 10 minutes
                            Thread.Sleep(600000);
                        }
                    }
                }
                finally
                {
                    if (SchedulingProvider.SchedulerMode == SchedulerMode.TIMER_METHOD || SchedulingProvider.SchedulerMode == SchedulerMode.DISABLED)
                    {
                        SetScheduleStatus(ScheduleStatus.STOPPED);
                    }
                    else
                    {
                        SetScheduleStatus(ScheduleStatus.WAITING_FOR_REQUEST);
                    }

                    if (SchedulingProvider.SchedulerMode != SchedulerMode.REQUEST_METHOD || _debug)
                    {
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_STOPPED" };
                        LogController.Instance.AddLog(log);
                    }
                }
            }

            public static void UpdateScheduleHistory(ScheduleHistoryItem scheduleHistoryItem)
            {
                try
                {
                    SchedulingController.UpdateScheduleHistory(scheduleHistoryItem);
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void WorkCompleted(SchedulerClient schedulerClient)
            {
                try
                {
                    ScheduleHistoryItem scheduleHistoryItem = schedulerClient.ScheduleHistoryItem;

                    // Remove the object in the ScheduleInProgress collection
                    RemoveFromScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has completed.  Decrease our ActiveThreadCount
                    Interlocked.Decrement(ref _activeThreadCount);

                    // Update the schedule item object property
                    // to note the end time and next start
                    scheduleHistoryItem.EndDate = DateTime.Now;

                    if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                    {
                        scheduleHistoryItem.NextStart = Null.NullDate;
                    }
                    else
                    {
                        if (scheduleHistoryItem.CatchUpEnabled)
                        {
                            switch (scheduleHistoryItem.TimeLapseMeasurement)
                            {
                                case "s":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "m":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "h":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "d":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "w":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                    break;
                                case "mo":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "y":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                    break;
                            }
                        }
                        else
                        {
                            switch (scheduleHistoryItem.TimeLapseMeasurement)
                            {
                                case "s":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddSeconds(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "m":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMinutes(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "h":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddHours(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "d":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "w":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                    break;
                                case "mo":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                    break;
                                case "y":
                                    scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                    break;
                            }
                        }
                    }

                    // Update the ScheduleHistory in the database
                    UpdateScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.NextStart != Null.NullDate)
                    {
                        // Put the object back into the ScheduleQueue
                        // collection with the new NextStart date.
                        scheduleHistoryItem.StartDate = Null.NullDate;
                        scheduleHistoryItem.EndDate = Null.NullDate;
                        scheduleHistoryItem.LogNotes = string.Empty;
                        scheduleHistoryItem.ProcessGroup = -1;
                        AddToScheduleQueue(scheduleHistoryItem);
                    }

                    if (schedulerClient.ScheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_COMPLETED" };
                        log.AddProperty("TYPE", schedulerClient.GetType().FullName);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString());
                        log.AddProperty("NEXT START", Convert.ToString(scheduleHistoryItem.NextStart));
                        log.AddProperty("SOURCE", schedulerClient.ScheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", _activeThreadCount.ToString());
                        log.AddProperty("FREE THREADS", FreeThreads.ToString());
                        log.AddProperty("READER TIMEOUTS", _readerTimeouts.ToString());
                        log.AddProperty("WRITER TIMEOUTS", _writerTimeouts.ToString());
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString());
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString());
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void WorkErrored(SchedulerClient schedulerClient, Exception exception)
            {
                WorkErrored(schedulerClient.ScheduleHistoryItem, exception);
            }

            public static void WorkErrored(ScheduleHistoryItem scheduleHistoryItem, Exception exception)
            {
                try
                {
                    // Remove the object in the ScheduleInProgress collection
                    RemoveFromScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has errored.  Decrease our ActiveThreadCount
                    Interlocked.Decrement(ref _activeThreadCount);

                    Exceptions.Exceptions.ProcessSchedulerException(exception);

                    // Update the schedule item object property
                    // to note the end time and next start
                    scheduleHistoryItem.EndDate = DateTime.Now;
                    if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                    {
                        scheduleHistoryItem.NextStart = Null.NullDate;
                    }
                    else if (scheduleHistoryItem.RetryTimeLapse != Null.NullInteger)
                    {
                        switch (scheduleHistoryItem.RetryTimeLapseMeasurement)
                        {
                            case "s":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddSeconds(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "m":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMinutes(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "h":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddHours(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "d":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "w":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.RetryTimeLapse * 7);
                                break;
                            case "mo":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.RetryTimeLapse);
                                break;
                            case "y":
                                scheduleHistoryItem.NextStart = scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.RetryTimeLapse);
                                break;
                        }
                    }

                    // Update the ScheduleHistory in the database
                    UpdateScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.NextStart != Null.NullDate && scheduleHistoryItem.RetryTimeLapse != Null.NullInteger)
                    {
                        // Put the object back into the ScheduleQueue
                        // collection with the new NextStart date.
                        scheduleHistoryItem.StartDate = Null.NullDate;
                        scheduleHistoryItem.EndDate = Null.NullDate;
                        scheduleHistoryItem.LogNotes = string.Empty;
                        scheduleHistoryItem.ProcessGroup = -1;
                        AddToScheduleQueue(scheduleHistoryItem);
                    }

                    if (scheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_FAILURE" };
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString());
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        if (exception != null)
                        {
                            log.AddProperty("EXCEPTION", exception.Message);
                        }

                        log.AddProperty("RESCHEDULED FOR", Convert.ToString(scheduleHistoryItem.NextStart));
                        log.AddProperty("SOURCE", scheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", _activeThreadCount.ToString());
                        log.AddProperty("FREE THREADS", FreeThreads.ToString());
                        log.AddProperty("READER TIMEOUTS", _readerTimeouts.ToString());
                        log.AddProperty("WRITER TIMEOUTS", _writerTimeouts.ToString());
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString());
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString());
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void WorkProgressing(SchedulerClient schedulerClient)
            {
                try
                {
                    // A SchedulerClient is notifying us that their
                    // process is in progress.  Informational only.
                    if (schedulerClient.ScheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_PROGRESSING" };
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString());
                        log.AddProperty("TYPE", schedulerClient.GetType().FullName);
                        log.AddProperty("SOURCE", schedulerClient.ScheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", _activeThreadCount.ToString());
                        log.AddProperty("FREE THREADS", FreeThreads.ToString());
                        log.AddProperty("READER TIMEOUTS", _readerTimeouts.ToString());
                        log.AddProperty("WRITER TIMEOUTS", _writerTimeouts.ToString());
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString());
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString());
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            public static void WorkStarted(SchedulerClient schedulerClient)
            {
                WorkStarted(schedulerClient.ScheduleHistoryItem);
            }

            public static void WorkStarted(ScheduleHistoryItem scheduleHistoryItem)
            {
                bool activeThreadCountIncremented = false;
                try
                {
                    scheduleHistoryItem.ThreadID = Thread.CurrentThread.GetHashCode();

                    // Put the object in the ScheduleInProgress collection
                    // and remove it from the ScheduleQueue
                    RemoveFromScheduleQueue(scheduleHistoryItem);
                    AddToScheduleInProgress(scheduleHistoryItem);

                    // A SchedulerClient is notifying us that their
                    // process has started.  Increase our ActiveThreadCount
                    Interlocked.Increment(ref _activeThreadCount);
                    activeThreadCountIncremented = true;

                    // Update the schedule item
                    // object property to note the start time.
                    scheduleHistoryItem.StartDate = DateTime.Now;
                    AddScheduleHistory(scheduleHistoryItem);

                    if (scheduleHistoryItem.RetainHistoryNum > 0)
                    {
                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_STARTED" };
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString());
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        log.AddProperty("SOURCE", scheduleHistoryItem.ScheduleSource.ToString());
                        log.AddProperty("ACTIVE THREADS", _activeThreadCount.ToString());
                        log.AddProperty("FREE THREADS", FreeThreads.ToString());
                        log.AddProperty("READER TIMEOUTS", _readerTimeouts.ToString());
                        log.AddProperty("WRITER TIMEOUTS", _writerTimeouts.ToString());
                        log.AddProperty("IN PROGRESS", GetScheduleInProgressCount().ToString());
                        log.AddProperty("IN QUEUE", GetScheduleQueueCount().ToString());
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
                    // Decrement the ActiveThreadCount because
                    // otherwise the number of active threads
                    // will appear to be climbing when in fact
                    // no tasks are being executed.
                    if (activeThreadCountIncremented)
                    {
                        Interlocked.Decrement(ref _activeThreadCount);
                    }

                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            // DNN-5001
            public static void StopScheduleInProgress(ScheduleItem scheduleItem, ScheduleHistoryItem runningscheduleHistoryItem)
            {
                try
                {
                    // attempt to stop task only if it is still in progress
                    if (GetScheduleItemFromScheduleInProgress(scheduleItem) != null)
                    {
                        var scheduleHistoryItem = GetScheduleItemFromScheduleInProgress(scheduleItem);
                        scheduleHistoryItem.ScheduleHistoryID = runningscheduleHistoryItem.ScheduleHistoryID;
                        scheduleHistoryItem.StartDate = runningscheduleHistoryItem.StartDate;

                        // Remove the object in the ScheduleInProgress collection
                        RemoveFromScheduleInProgress(scheduleHistoryItem);

                        // A SchedulerClient is notifying us that their
                        // process has completed.  Decrease our ActiveThreadCount
                        Interlocked.Decrement(ref _activeThreadCount);

                        // Update the schedule item object property
                        // to note the end time and next start
                        scheduleHistoryItem.EndDate = DateTime.Now;

                        if (scheduleHistoryItem.ScheduleSource == ScheduleSource.STARTED_FROM_EVENT)
                        {
                            scheduleHistoryItem.NextStart = Null.NullDate;
                        }
                        else
                        {
                            if (scheduleHistoryItem.CatchUpEnabled)
                            {
                                switch (scheduleHistoryItem.TimeLapseMeasurement)
                                {
                                    case "s":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddSeconds(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "m":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddMinutes(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "h":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddHours(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "d":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.NextStart.AddDays(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "w":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                        break;
                                    case "mo":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "y":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                        break;
                                }
                            }
                            else
                            {
                                switch (scheduleHistoryItem.TimeLapseMeasurement)
                                {
                                    case "s":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddSeconds(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "m":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddMinutes(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "h":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddHours(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "d":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "w":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddDays(scheduleHistoryItem.TimeLapse * 7);
                                        break;
                                    case "mo":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddMonths(scheduleHistoryItem.TimeLapse);
                                        break;
                                    case "y":
                                        scheduleHistoryItem.NextStart =
                                            scheduleHistoryItem.StartDate.AddYears(scheduleHistoryItem.TimeLapse);
                                        break;
                                }
                            }
                        }

                        // Update the ScheduleHistory in the database
                        UpdateScheduleHistory(scheduleHistoryItem);

                        if (scheduleHistoryItem.NextStart != Null.NullDate)
                        {
                            // Put the object back into the ScheduleQueue
                            // collection with the new NextStart date.
                            scheduleHistoryItem.StartDate = Null.NullDate;
                            scheduleHistoryItem.EndDate = Null.NullDate;
                            scheduleHistoryItem.LogNotes = string.Empty;
                            scheduleHistoryItem.ProcessGroup = -1;
                            AddToScheduleQueue(scheduleHistoryItem);
                        }

                        // Write out the log entry for this event
                        var log = new LogInfo { LogTypeKey = "SCHEDULER_EVENT_COMPLETED" };
                        log.AddProperty("REASON", "Scheduler task has been stopped manually");
                        log.AddProperty("TYPE", scheduleHistoryItem.TypeFullName);
                        log.AddProperty("THREAD ID", Thread.CurrentThread.GetHashCode().ToString());
                        log.AddProperty("NEXT START", Convert.ToString(scheduleHistoryItem.NextStart));
                        LogController.Instance.AddLog(log);
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.ProcessSchedulerException(exc);
                }
            }

            internal static bool IsInQueue(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Any(si => si.ScheduleID == scheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    return false;
                }
            }

            internal static ServerInfo GetServer(string executingServer)
            {
                try
                {
                    return ServerController.GetServers().FirstOrDefault(
                        s => ServerController.GetServerName(s).Equals(executingServer, StringComparison.OrdinalIgnoreCase) && s.Enabled);
                }
                catch (Exception)
                {
                    // catches edge-case where schedule runs before webserver registration
                    return null;
                }
            }

            internal static void InitializeThreadPool(bool boolDebug, int maxThreads)
            {
                _debug = boolDebug;
                lock (typeof(CoreScheduler))
                {
                    if (!_threadPoolInitialized)
                    {
                        _threadPoolInitialized = true;
                        if (maxThreads == -1)
                        {
                            maxThreads = 1;
                        }

                        _numberOfProcessGroups = maxThreads;
                        _maxThreadCount = maxThreads;
                        for (int i = 0; i < _numberOfProcessGroups; i++)
                        {
                            Array.Resize(ref _processGroup, i + 1);
                            _processGroup[i] = new ProcessGroup();
                        }
                    }
                }
            }

            /// <summary>
            /// adds an item to the collection of schedule items in progress.
            /// </summary>
            /// <param name="scheduleHistoryItem">Item to add.</param>
            /// <remarks>Thread Safe.</remarks>
            private static void AddToScheduleInProgress(ScheduleHistoryItem scheduleHistoryItem)
            {
                if (!ScheduleInProgressContains(scheduleHistoryItem))
                {
                    try
                    {
                        using (ScheduleInProgress.GetWriteLock(LockTimeout))
                        {
                            if (!ScheduleInProgressContains(scheduleHistoryItem))
                            {
                                ScheduleInProgress.Add(scheduleHistoryItem);
                            }
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        // The writer lock request timed out.
                        Interlocked.Increment(ref _writerTimeouts);
                        Exceptions.Exceptions.LogException(ex);
                    }
                }
            }

            private static int GetProcessGroup()
            {
                // return a random process group
                var r = new Random();
                return r.Next(0, _numberOfProcessGroups - 1);
            }

            private static bool IsInProgress(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Any(si => si.ScheduleID == scheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
                    // The reader lock request timed out.
                    Interlocked.Increment(ref _readerTimeouts);
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.Debug(ex);
                    }

                    return false;
                }
            }

            /// <summary>
            /// Removes an item from the collection of schedule items in progress.
            /// </summary>
            /// <param name="scheduleItem"></param>
            /// <remarks>Thread Safe.</remarks>
            private static void RemoveFromScheduleInProgress(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleInProgress.GetWriteLock(LockTimeout))
                    {
                        var item = ScheduleInProgress.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
                        if (item != null)
                        {
                            ScheduleInProgress.Remove(item);
                        }
                    }
                }
                catch (ApplicationException ex)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref _writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }
            }

            /// <summary>
            /// Gets a schedulehistory item from the collection of schedule items in progress.
            /// </summary>
            /// <param name="scheduleItem"></param>
            /// <remarks>Thread Safe.</remarks>
            private static ScheduleHistoryItem GetScheduleItemFromScheduleInProgress(ScheduleItem scheduleItem)
            {
                try
                {
                    using (ScheduleInProgress.GetWriteLock(LockTimeout))
                    {
                        var item = ScheduleInProgress.FirstOrDefault(si => si.ScheduleID == scheduleItem.ScheduleID);
                        return item;
                    }
                }
                catch (ApplicationException ex)
                {
                    // The writer lock request timed out.
                    Interlocked.Increment(ref _writerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                }

                return null;
            }

            private static bool ScheduleInProgressContains(ScheduleHistoryItem scheduleHistoryItem)
            {
                try
                {
                    using (ScheduleInProgress.GetReadLock(LockTimeout))
                    {
                        return ScheduleInProgress.Any(si => si.ScheduleID == scheduleHistoryItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
                    Interlocked.Increment(ref _readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                    return false;
                }
            }

            private static bool ScheduleQueueContains(ScheduleItem objScheduleItem)
            {
                try
                {
                    using (ScheduleQueue.GetReadLock(LockTimeout))
                    {
                        return ScheduleQueue.Any(si => si.ScheduleID == objScheduleItem.ScheduleID);
                    }
                }
                catch (ApplicationException ex)
                {
                    Interlocked.Increment(ref _readerTimeouts);
                    Exceptions.Exceptions.LogException(ex);
                    return false;
                }
            }

            private static void LogWhyTaskNotRun(ScheduleItem scheduleItem)
            {
                if (_debug)
                {
                    bool appended = false;
                    var strDebug = new StringBuilder("Task not run because ");
                    if (!(scheduleItem.NextStart <= DateTime.Now))
                    {
                        strDebug.Append(" task is scheduled for " + scheduleItem.NextStart);
                        appended = true;
                    }

                    if (!scheduleItem.Enabled)
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task is not enabled");
                        appended = true;
                    }

                    if (IsInProgress(scheduleItem))
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task is already in progress");
                        appended = true;
                    }

                    if (HasDependenciesConflict(scheduleItem))
                    {
                        if (appended)
                        {
                            strDebug.Append(" and");
                        }

                        strDebug.Append(" task has conflicting dependency");
                    }

                    var log = new LogInfo();
                    log.AddProperty("EVENT NOT RUN REASON", strDebug.ToString());
                    log.AddProperty("SCHEDULE ID", scheduleItem.ScheduleID.ToString());
                    log.AddProperty("TYPE FULL NAME", scheduleItem.TypeFullName);
                    log.LogTypeKey = "DEBUG";
                    LogController.Instance.AddLog(log);
                }
            }

            private static void LogEventAddedToProcessGroup(ScheduleItem scheduleItem)
            {
                if (_debug)
                {
                    var log = new LogInfo();
                    log.AddProperty("EVENT ADDED TO PROCESS GROUP " + scheduleItem.ProcessGroup, scheduleItem.TypeFullName);
                    log.AddProperty("SCHEDULE ID", scheduleItem.ScheduleID.ToString());
                    log.LogTypeKey = "DEBUG";
                    LogController.Instance.AddLog(log);
                }
            }

            private static string ServerGroupServers(ServerInfo thisServer)
            {
                // Get the servers
                var servers = ServerController.GetEnabledServers().Where(s => s.ServerGroup == thisServer.ServerGroup);
                return servers.Aggregate(string.Empty, (current, serverInfo) => current + ServerController.GetServerName(serverInfo) + ",");
            }
        }
    }
}
