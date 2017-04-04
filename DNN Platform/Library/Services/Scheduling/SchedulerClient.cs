#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (SchedulerClient));

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
            Logger.Error(objException);
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