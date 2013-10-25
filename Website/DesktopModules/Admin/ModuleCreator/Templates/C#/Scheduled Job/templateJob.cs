#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

using System;
using DotNetNuke;

namespace [OWNER].[MODULE]
{

    public class [MODULE]Job : DotNetNuke.Services.Scheduling.SchedulerClient
    {

        public [MODULE]Job(DotNetNuke.Services.Scheduling.ScheduleHistoryItem objScheduleHistoryItem) : base()
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try {
                this.Progressing();
                string strMessage = Processing();
                this.ScheduleHistoryItem.Succeeded = true;
                this.ScheduleHistoryItem.AddLogNote("[MODULE] Succeeded");
            } catch (Exception exc) {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("[MODULE] Failed");
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
