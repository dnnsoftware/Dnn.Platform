#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// **********************************
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using System;

#endregion


namespace DotNetNuke.Services.Users
{
    public class PurgeDeletedUsers : SchedulerClient
    {

        public PurgeDeletedUsers(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem;
        }

        public override void DoWork()
        {
            try
            {
                foreach (PortalInfo portal in new PortalController().GetPortals())
                {
                    var settings = new PortalSettings(portal.PortalID);
                    if (settings.DataConsentActive)
                    {
                        if (settings.DataConsentUserDeleteAction == PortalSettings.UserDeleteAction.DelayedHardDelete)
                        {
                            var thresholdDate = DateTime.Now;
                            switch (settings.DataConsentDelayMeasurement)
                            {
                                case "h":
                                    thresholdDate = DateTime.Now.AddHours(-1 * settings.DataConsentDelay);
                                    break;
                                case "d":
                                    thresholdDate = DateTime.Now.AddDays(-1 * settings.DataConsentDelay);
                                    break;
                                case "w":
                                    thresholdDate = DateTime.Now.AddDays(-7 * settings.DataConsentDelay);
                                    break;
                            }
                            var deletedUsers = UserController.GetDeletedUsers(portal.PortalID);
                            foreach (UserInfo user in deletedUsers)
                            {
                                if (user.LastModifiedOnDate < thresholdDate && user.RequestsRemoval)
                                {
                                    UserController.RemoveUser(user);
                                    ScheduleHistoryItem.AddLogNote(string.Format("Removed user {0}{1}", user.Username, Environment.NewLine));
                                }
                            }
                        }
                    }
                }
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
                ScheduleHistoryItem.AddLogNote("Purging deleted users task completed");
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging deleted users task failed: {0}.", exc.ToString()));

                //notification that we have errored
                Errored(ref exc); //REQUIRED

                //log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}
