#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Scheduling;

#endregion

namespace DotNetNuke.Services.OutputCache
{
    public class PurgeOutputCache : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PurgeOutputCache));

        public PurgeOutputCache(ScheduleHistoryItem objScheduleHistoryItem)
        {
            ScheduleHistoryItem = objScheduleHistoryItem; //REQUIRED
        }

        public override void DoWork()
        {
            try
            {
                var portals = PortalController.Instance.GetPortals();
                foreach (KeyValuePair<string, OutputCachingProvider> kvp in OutputCachingProvider.GetProviderList())
                {
                    try
                    {
                        foreach (PortalInfo portal in portals)
                        {
                            kvp.Value.PurgeExpiredItems(portal.PortalID);
                            ScheduleHistoryItem.AddLogNote(string.Format("Purged output cache for {0}.  ", kvp.Key));
                        }
                    }
                    catch (NotSupportedException exc)
                    {
						//some output caching providers don't use this feature
                        Logger.Debug(exc);
                    }
                }
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED

                ScheduleHistoryItem.AddLogNote(string.Format("Purging output cache task failed: {0}.", exc.ToString())); //OPTIONAL

                //notification that we have errored
                Errored(ref exc);
				
				//log the exception
                Exceptions.Exceptions.LogException(exc); //OPTIONAL
            }
        }
    }
}