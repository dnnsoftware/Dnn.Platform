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
        /// <summary>Initializes a new instance of the <see cref="PurgeClientDependencyFiles"/> class.</summary>
        /// <param name="objScheduleHistoryItem">The schedule history item.</param>
        public PurgeClientDependencyFiles(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem; // REQUIRED
        }

        /// <inheritdoc/>
        public override void DoWork()
        {
            try
            {
                string[] filePaths = Directory.GetFiles($"{Common.Globals.ApplicationMapPath}/App_Data/ClientDependency");
                foreach (string filePath in filePaths)
                {
                    File.Delete(filePath);
                }

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED
                this.ScheduleHistoryItem.AddLogNote("Purging client dependency files task succeeded");
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote($"Purging client dependency files task failed: {exc.ToString()}.");

                // notification that we have errored
                this.Errored(ref exc); // REQUIRED

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }
    }
}
