// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
