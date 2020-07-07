// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Scheduling
{
    using System;
    using System.Threading;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;

    /// <summary>
    /// This class is inherited by any class that wants to run tasks in the scheduler.
    /// </summary>
    public abstract class SchedulerClient
    {
        public SchedulerClient()
        {
            this.SchedulerEventGUID = Null.NullString;
            this.aProcessMethod = Null.NullString;
            this.Status = Null.NullString;
            this.ScheduleHistoryItem = new ScheduleHistoryItem();
        }

        public event WorkStarted ProcessStarted;

        public event WorkProgressing ProcessProgressing;

        public event WorkCompleted ProcessCompleted;

        public event WorkErrored ProcessErrored;

        public int ThreadID
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }

        public ScheduleHistoryItem ScheduleHistoryItem { get; set; }

        public string SchedulerEventGUID { get; set; }

        public string aProcessMethod { get; set; }

        public string Status { get; set; }

        public void Started()
        {
            if (this.ProcessStarted != null)
            {
                this.ProcessStarted(this);
            }
        }

        public void Progressing()
        {
            if (this.ProcessProgressing != null)
            {
                this.ProcessProgressing(this);
            }
        }

        public void Completed()
        {
            if (this.ProcessCompleted != null)
            {
                this.ProcessCompleted(this);
            }
        }

        public void Errored(ref Exception objException)
        {
            if (this.ProcessErrored != null)
            {
                this.ProcessErrored(this, objException);
            }
        }

        /// '''''''''''''''''''''''''''''''''''''''''''''''''''
        /// <summary>
        /// This is the sub that kicks off the actual
        /// work within the SchedulerClient's subclass.
        /// </summary>
        /// '''''''''''''''''''''''''''''''''''''''''''''''''''
        public abstract void DoWork();
    }
}
