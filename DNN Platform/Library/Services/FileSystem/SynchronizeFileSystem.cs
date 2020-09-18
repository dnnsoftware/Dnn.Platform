// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.FileSystem
{
    using System;
    using System.Collections;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Scheduling;

    public class SynchronizeFileSystem : SchedulerClient
    {
        public SynchronizeFileSystem(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                // notification that the event is progressing
                this.Progressing(); // OPTIONAL

                this.Synchronize();

                this.ScheduleHistoryItem.Succeeded = true; // REQUIRED

                this.ScheduleHistoryItem.AddLogNote("File System Synchronized."); // OPTIONAL
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false;

                this.ScheduleHistoryItem.AddLogNote("File System Synchronization failed. " + exc);

                // notification that we have errored
                this.Errored(ref exc);

                // log the exception
                Exceptions.Exceptions.LogException(exc); // OPTIONAL
            }
        }

        private void Synchronize()
        {
            var folderManager = FolderManager.Instance;

            folderManager.Synchronize(Null.NullInteger);

            var portals = PortalController.Instance.GetPortals();

            // Sync Portals
            for (var intIndex = 0; intIndex <= portals.Count - 1; intIndex++)
            {
                var portal = (PortalInfo)portals[intIndex];
                folderManager.Synchronize(portal.PortalID);
            }
        }
    }
}
