// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.ClientDependency
{
    using System;
    using System.IO;

    using DotNetNuke.Services.Scheduling;

    internal class PurgeClientDependencyFiles : SchedulerClient
    {
        public PurgeClientDependencyFiles(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
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

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging client dependency files task succeeded");
            }
            catch (Exception exc) // REQUIRED
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote(string.Format("Purging client dependency files task failed: {0}.", exc.ToString()));

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
