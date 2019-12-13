// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.FileSystem
{
    public class SynchronizeFileSystem : SchedulerClient
    {
        public SynchronizeFileSystem(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
				//notification that the event is progressing
                Progressing(); //OPTIONAL

                Synchronize();

                ScheduleHistoryItem.Succeeded = true; //REQUIRED

                ScheduleHistoryItem.AddLogNote("File System Synchronized."); //OPTIONAL
            }
            catch (Exception exc)
            {
                ScheduleHistoryItem.Succeeded = false;

                ScheduleHistoryItem.AddLogNote("File System Synchronization failed. " + exc);

                //notification that we have errored
                Errored(ref exc);
				
				//log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }

        private void Synchronize()
        {
            var folderManager = FolderManager.Instance;
            
            folderManager.Synchronize(Null.NullInteger);

            var portals = PortalController.Instance.GetPortals();
            //Sync Portals
			for (var intIndex = 0; intIndex <= portals.Count - 1; intIndex++)
            {
                var portal = (PortalInfo) portals[intIndex];
                folderManager.Synchronize(portal.PortalID);
            }
        }
    }
}
