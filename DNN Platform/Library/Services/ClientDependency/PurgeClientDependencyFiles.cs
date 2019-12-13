// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.IO;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.ClientDependency
{
    class PurgeClientDependencyFiles : SchedulerClient
    {
        public PurgeClientDependencyFiles(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem; //REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                string[] filePaths = Directory.GetFiles(string.Format("{0}/App_Data/ClientDependency", Common.Globals.ApplicationMapPath));
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
                ScheduleHistoryItem.AddLogNote("Purging client dependency files task succeeded");
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging client dependency files task failed: {0}.", exc.ToString()));

                //notification that we have errored
                Errored(ref exc); //REQUIRED

                //log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
