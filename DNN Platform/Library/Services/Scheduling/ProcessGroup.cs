// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Web.Compilation;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using Microsoft.Extensions.DependencyInjection;

    public class ProcessGroup
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ProcessGroup));

        private static int numberOfProcessesInQueue;
        private static int numberOfProcesses;
        private static int processesCompleted;
        private static int ticksElapsed;

        // ''''''''''''''''''''''''''''''''''''''''''''''''''
        // This class represents a process group for
        // our threads to run in.
        // ''''''''''''''''''''''''''''''''''''''''''''''''''
        public delegate void CompletedEventHandler();

        public event CompletedEventHandler Completed;

        private static int GetTicksElapsed
        {
            get
            {
                return ticksElapsed;
            }
        }

        private static int GetProcessesCompleted
        {
            get
            {
                return processesCompleted;
            }
        }

        private static int GetProcessesInQueue
        {
            get
            {
                return numberOfProcessesInQueue;
            }
        }

        public void Run(ScheduleHistoryItem objScheduleHistoryItem)
        {
            IServiceScope serviceScope = null;
            SchedulerClient Process = null;
            try
            {
                // This is called from RunPooledThread()
                ticksElapsed = Environment.TickCount - ticksElapsed;
                serviceScope = Globals.DependencyProvider.CreateScope();
                Process = this.GetSchedulerClient(serviceScope.ServiceProvider, objScheduleHistoryItem.TypeFullName, objScheduleHistoryItem);
                Process.ScheduleHistoryItem = objScheduleHistoryItem;

                // Set up the handlers for the CoreScheduler
                Process.ProcessStarted += Scheduler.CoreScheduler.WorkStarted;
                Process.ProcessProgressing += Scheduler.CoreScheduler.WorkProgressing;
                Process.ProcessCompleted += Scheduler.CoreScheduler.WorkCompleted;
                Process.ProcessErrored += Scheduler.CoreScheduler.WorkErrored;

                // This kicks off the DoWork method of the class
                // type specified in the configuration.
                Process.Started();
                try
                {
                    Process.ScheduleHistoryItem.Succeeded = false;
                    Process.DoWork();
                }
                catch (Exception exc)
                {
                    // in case the scheduler client
                    // didn't have proper exception handling
                    // make sure we fire the Errored event
                    Logger.Error(exc);

                    if (Process != null)
                    {
                        if (Process.ScheduleHistoryItem != null)
                        {
                            Process.ScheduleHistoryItem.Succeeded = false;
                        }

                        Process.Errored(ref exc);
                    }
                }

                if (Process.ScheduleHistoryItem.Succeeded)
                {
                    Process.Completed();
                }

                // If all processes in this ProcessGroup have
                // completed, set the ticksElapsed and raise
                // the Completed event.
                // I don't think this is necessary with the
                // other events.  I'll leave it for now and
                // will probably take it out later.
                if (processesCompleted == numberOfProcesses)
                {
                    if (processesCompleted == numberOfProcesses)
                    {
                        ticksElapsed = Environment.TickCount - ticksElapsed;
                        if (this.Completed != null)
                        {
                            this.Completed();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                // in case the scheduler client
                // didn't have proper exception handling
                // make sure we fire the Errored event
                if (Process != null)
                {
                    if (Process.ScheduleHistoryItem != null)
                    {
                        Process.ScheduleHistoryItem.Succeeded = false;
                    }

                    Process.Errored(ref exc);
                }
                else
                {
                    // when the schedule has invalid config and can't initialize the Process,
                    // we need also trigger work errored event so that the schedule can remove from inprogress and inqueue list to prevent endless loop.
                    Scheduler.CoreScheduler.WorkStarted(objScheduleHistoryItem);
                    objScheduleHistoryItem.Succeeded = false;
                    Scheduler.CoreScheduler.WorkErrored(objScheduleHistoryItem, exc);
                }
            }
            finally
            {
                serviceScope?.Dispose();

                // Track how many processes have completed for
                // this instanciation of the ProcessGroup
                numberOfProcessesInQueue -= 1;
                processesCompleted += 1;
            }
        }

        // Add a queue request to Threadpool with a
        // callback to RunPooledThread which calls Run()
        public void AddQueueUserWorkItem(ScheduleItem s)
        {
            numberOfProcessesInQueue += 1;
            numberOfProcesses += 1;
            var obj = new ScheduleHistoryItem(s);
            try
            {
                // Create a callback to subroutine RunPooledThread
                WaitCallback callback = this.RunPooledThread;

                // And put in a request to ThreadPool to run the process.
                ThreadPool.QueueUserWorkItem(callback, obj);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.ProcessSchedulerException(exc);
            }
        }

        private SchedulerClient GetSchedulerClient(IServiceProvider services, string strProcess, ScheduleHistoryItem objScheduleHistoryItem)
        {
            // This is a method to encapsulate returning
            // an object whose class inherits SchedulerClient.
            Type t = BuildManager.GetType(strProcess, true, true);
            return (SchedulerClient)ActivatorUtilities.CreateInstance(services, t, objScheduleHistoryItem);
        }

        // This subroutine is callback for Threadpool.QueueWorkItem.  This is the necessary
        // subroutine signature for QueueWorkItem, and Run() is proper for creating a Thread
        // so the two subroutines cannot be combined, so instead just call Run from here.
        private void RunPooledThread(object objScheduleHistoryItem)
        {
            this.Run((ScheduleHistoryItem)objScheduleHistoryItem);
        }
    }
}
