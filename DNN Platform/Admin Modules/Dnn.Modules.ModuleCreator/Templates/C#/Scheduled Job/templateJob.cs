// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using DotNetNuke;

namespace _OWNER_._MODULE_
{

    public class _MODULE_Job : DotNetNuke.Services.Scheduling.SchedulerClient
    {

        public _MODULE_Job(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try {
                this.Progressing();
                string strMessage = Processing();
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("_MODULE_ Succeeded");
            } catch (Exception exc) {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("_MODULE_ Failed");
                this.Errored(ref exc);
            }
        }

        public string Processing()
        {
            string Message = "";
            return Message;
        }

    }

}
