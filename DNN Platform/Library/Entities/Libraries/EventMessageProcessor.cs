#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.EventQueue;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Libraries
{
    public class EventMessageProcessor : EventMessageProcessorBase
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (EventMessageProcessor));

        private static void UpgradeLibrary(EventMessage message)
        {
            try
            {
                string businessControllerClass = message.Attributes["BusinessControllerClass"];
                object controller = Reflection.CreateObject(businessControllerClass, "");
                var eventLogController = new EventLogController();
                var upgradeableController = controller as IUpgradeable;
                if (upgradeableController != null)
                {
					//get the list of applicable versions
                    string[] upgradeVersions = message.Attributes["UpgradeVersionsList"].Split(',');
                    foreach (string version in upgradeVersions)
                    {
						//call the IUpgradeable interface for the library/version
                        string results = upgradeableController.UpgradeModule(version);
                        //log the upgrade results
                        var eventLogInfo = new LogInfo();
                        eventLogInfo.AddProperty("Library Upgraded", businessControllerClass);
                        eventLogInfo.AddProperty("Version", version);
                        if (!string.IsNullOrEmpty(results))
                        {
                            eventLogInfo.AddProperty("Results", results);
                        }
                        eventLogInfo.LogTypeKey = EventLogController.EventLogType.LIBRARY_UPDATED.ToString();
                        eventLogController.AddLog(eventLogInfo);
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        public override bool ProcessMessage(EventMessage message)
        {
            try
            {
                switch (message.ProcessorCommand)
                {
                    case "UpgradeLibrary":
                        UpgradeLibrary(message);
                        break;
                    default:
						//other events can be added here
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                message.ExceptionMessage = ex.Message;
                return false;
            }
            return true;
        }
    }
}