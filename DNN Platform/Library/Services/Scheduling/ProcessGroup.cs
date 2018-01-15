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

using System;
using System.Reflection;
using System.Threading;
using System.Web.Compilation;

using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Scheduling
{
    public class ProcessGroup
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ProcessGroup));
        //''''''''''''''''''''''''''''''''''''''''''''''''''
        //This class represents a process group for
        //our threads to run in.
        //''''''''''''''''''''''''''''''''''''''''''''''''''
        #region Delegates

        public delegate void CompletedEventHandler();

        #endregion

        private static int numberOfProcessesInQueue;
        private static int numberOfProcesses;
        private static int processesCompleted;
        private static int ticksElapsed;

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

        public event CompletedEventHandler Completed;

        public void Run(ScheduleHistoryItem objScheduleHistoryItem)
        {
            SchedulerClient Process = null;
            try
            {
                //This is called from RunPooledThread()
                ticksElapsed = Environment.TickCount - ticksElapsed;
                Process = GetSchedulerClient(objScheduleHistoryItem.TypeFullName, objScheduleHistoryItem);
                Process.ScheduleHistoryItem = objScheduleHistoryItem;
                
				//Set up the handlers for the CoreScheduler
                Process.ProcessStarted += Scheduler.CoreScheduler.WorkStarted;
                Process.ProcessProgressing += Scheduler.CoreScheduler.WorkProgressing;
                Process.ProcessCompleted += Scheduler.CoreScheduler.WorkCompleted;
                Process.ProcessErrored += Scheduler.CoreScheduler.WorkErrored;
                //This kicks off the DoWork method of the class
                //type specified in the configuration.

				Process.Started();
                try
                {
                    Process.ScheduleHistoryItem.Succeeded = false;
                    Process.DoWork();
                }
                catch (Exception exc)
                {
                    //in case the scheduler client
                    //didn't have proper exception handling
                    //make sure we fire the Errored event
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
                
				//If all processes in this ProcessGroup have
                //completed, set the ticksElapsed and raise
                //the Completed event.
                //I don't think this is necessary with the
                //other events.  I'll leave it for now and
                //will probably take it out later.

				if (processesCompleted == numberOfProcesses)
                {
                    if (processesCompleted == numberOfProcesses)
                    {
                        ticksElapsed = Environment.TickCount - ticksElapsed;
                        if (Completed != null)
                        {
                            Completed();
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                //in case the scheduler client
                //didn't have proper exception handling
                //make sure we fire the Errored event
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
                    //when the schedule has invalid config and can't initialize the Process, 
                    //we need also trigger work errored event so that the schedule can remove from inprogress and inqueue list to prevent endless loop.
                    Scheduler.CoreScheduler.WorkStarted(objScheduleHistoryItem);
                    objScheduleHistoryItem.Succeeded = false;
                    Scheduler.CoreScheduler.WorkErrored(objScheduleHistoryItem, exc);
                }
            }
            finally
            {
                //Track how many processes have completed for
                //this instanciation of the ProcessGroup
                numberOfProcessesInQueue -= 1;
                processesCompleted += 1;
            }
        }

        private SchedulerClient GetSchedulerClient(string strProcess, ScheduleHistoryItem objScheduleHistoryItem)
        {
            //This is a method to encapsulate returning
            //an object whose class inherits SchedulerClient.
            Type t = BuildManager.GetType(strProcess, true, true);
            var param = new ScheduleHistoryItem[1];
            param[0] = objScheduleHistoryItem;
            var types = new Type[1];
            
			//Get the constructor for the Class
            types[0] = typeof (ScheduleHistoryItem);
            ConstructorInfo objConstructor;
            objConstructor = t.GetConstructor(types);
            
			//Return an instance of the class as an object
            return (SchedulerClient) objConstructor.Invoke(param);
        }

        //This subroutine is callback for Threadpool.QueueWorkItem.  This is the necessary
        //subroutine signature for QueueWorkItem, and Run() is proper for creating a Thread
        //so the two subroutines cannot be combined, so instead just call Run from here.
        private void RunPooledThread(object objScheduleHistoryItem)
        {
            Run((ScheduleHistoryItem) objScheduleHistoryItem);
        }

        //Add a queue request to Threadpool with a 
        //callback to RunPooledThread which calls Run()
        public void AddQueueUserWorkItem(ScheduleItem s)
        {
            numberOfProcessesInQueue += 1;
            numberOfProcesses += 1;
            var obj = new ScheduleHistoryItem(s);
            try
            {
                //Create a callback to subroutine RunPooledThread
                WaitCallback callback = RunPooledThread;
                //And put in a request to ThreadPool to run the process.
                ThreadPool.QueueUserWorkItem(callback, obj);
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.ProcessSchedulerException(exc);
            }
        }
    }
}