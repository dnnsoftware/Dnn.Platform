// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Threading;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Scheduling
{
	/// <summary>
	/// This class is inherited by any class that wants to run tasks in the scheduler.
	/// </summary>
    public abstract class SchedulerClient
    {
        public SchedulerClient()
        {
            SchedulerEventGUID = Null.NullString;
            aProcessMethod = Null.NullString;
            Status = Null.NullString;
            ScheduleHistoryItem = new ScheduleHistoryItem();
        }

        public ScheduleHistoryItem ScheduleHistoryItem { get; set; }

        public string SchedulerEventGUID { get; set; }

        public string aProcessMethod { get; set; }

        public string Status { get; set; }

        public int ThreadID
        {
            get
            {
                return Thread.CurrentThread.ManagedThreadId;
            }
        }

        public event WorkStarted ProcessStarted;
        public event WorkProgressing ProcessProgressing;
        public event WorkCompleted ProcessCompleted;
        public event WorkErrored ProcessErrored;

        public void Started()
        {
            if (ProcessStarted != null)
            {
                ProcessStarted(this);
            }
        }

        public void Progressing()
        {
            if (ProcessProgressing != null)
            {
                ProcessProgressing(this);
            }
        }

        public void Completed()
        {
            if (ProcessCompleted != null)
            {
                ProcessCompleted(this);
            }
        }

        public void Errored(ref Exception objException)
        {
            if (ProcessErrored != null)
            {
                ProcessErrored(this, objException);
            }
        }

        /// '''''''''''''''''''''''''''''''''''''''''''''''''''
		/// <summary>
        /// This is the sub that kicks off the actual
        /// work within the SchedulerClient's subclass
		/// </summary>
		/// '''''''''''''''''''''''''''''''''''''''''''''''''''
		public abstract void DoWork();
    }
}
