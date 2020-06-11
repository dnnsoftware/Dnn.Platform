﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System;

using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
    public class SendLogNotifications : SchedulerClient
    {
        public SendLogNotifications(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
				//notification that the event is progressing
                this.Progressing(); //OPTIONAL
                LoggingProvider.Instance().SendLogNotifications();
                this.ScheduleHistoryItem.Succeeded = true; //REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Sent log notifications successfully"); //OPTIONAL
            }
            catch (Exception exc) //REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; //REQUIRED
                this.ScheduleHistoryItem.AddLogNote("EXCEPTION: " + exc); //OPTIONAL
                this.Errored(ref exc); //REQUIRED
                //log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
