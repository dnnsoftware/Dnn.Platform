// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    public class EventMessageProcessor : EventMessageProcessorBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventMessageProcessor));

        public static void CreateImportModuleMessage(ModuleInfo objModule, string content, string version, int userID)
        {
            var appStartMessage = new EventMessage
            {
                Priority = MessagePriority.High,
                ExpirationDate = DateTime.Now.AddYears(-1),
                SentDate = DateTime.Now,
                Body = string.Empty,
                ProcessorType = "DotNetNuke.Entities.Modules.EventMessageProcessor, DotNetNuke",
                ProcessorCommand = "ImportModule",
            };

            // Add custom Attributes for this message
            appStartMessage.Attributes.Add("BusinessControllerClass", objModule.DesktopModule.BusinessControllerClass);
            appStartMessage.Attributes.Add("ModuleId", objModule.ModuleID.ToString());
            appStartMessage.Attributes.Add("Content", content);
            appStartMessage.Attributes.Add("Version", version);
            appStartMessage.Attributes.Add("UserID", userID.ToString());

            // send it to occur on next App_Start Event
            EventQueueController.SendMessage(appStartMessage, "Application_Start_FirstRequest");
        }

        public override bool ProcessMessage(EventMessage message)
        {
            try
            {
                switch (message.ProcessorCommand)
                {
                    case "UpdateSupportedFeatures":
                        UpdateSupportedFeatures(message);
                        break;
                    case "UpgradeModule":
                        UpgradeModule(message);
                        break;
                    case "ImportModule":
                        ImportModule(message);
                        break;
                    default:
                        // other events can be added here
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

        private static void ImportModule(EventMessage message)
        {
            try
            {
                string BusinessControllerClass = message.Attributes["BusinessControllerClass"];
                object controller = Reflection.CreateObject(BusinessControllerClass, string.Empty);
                if (controller is IPortable)
                {
                    int ModuleId = Convert.ToInt32(message.Attributes["ModuleId"]);
                    string Content = HttpUtility.HtmlDecode(message.Attributes["Content"]);
                    string Version = message.Attributes["Version"];
                    int UserID = Convert.ToInt32(message.Attributes["UserId"]);

                    // call the IPortable interface for the module/version
                    ((IPortable)controller).ImportModule(ModuleId, Content, Version, UserID);

                    // Synchronize Module Cache
                    ModuleController.SynchronizeModule(ModuleId);
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private static void UpgradeModule(EventMessage message)
        {
            try
            {
                int desktopModuleId = Convert.ToInt32(message.Attributes["DesktopModuleId"]);
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);

                string BusinessControllerClass = message.Attributes["BusinessControllerClass"];
                object controller = Reflection.CreateObject(BusinessControllerClass, string.Empty);
                if (controller is IUpgradeable)
                {
                    // get the list of applicable versions
                    string[] UpgradeVersions = message.Attributes["UpgradeVersionsList"].Split(',');
                    foreach (string Version in UpgradeVersions)
                    {
                        // call the IUpgradeable interface for the module/version
                        string Results = ((IUpgradeable)controller).UpgradeModule(Version);

                        // log the upgrade results
                        var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.MODULE_UPDATED.ToString() };
                        log.AddProperty("Module Upgraded", BusinessControllerClass);
                        log.AddProperty("Version", Version);
                        if (!string.IsNullOrEmpty(Results))
                        {
                            log.AddProperty("Results", Results);
                        }

                        LogController.Instance.AddLog(log);
                    }
                }

                UpdateSupportedFeatures(controller, Convert.ToInt32(message.Attributes["DesktopModuleId"]));
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private static void UpdateSupportedFeatures(EventMessage message)
        {
            string BusinessControllerClass = message.Attributes["BusinessControllerClass"];
            object controller = Reflection.CreateObject(BusinessControllerClass, string.Empty);
            UpdateSupportedFeatures(controller, Convert.ToInt32(message.Attributes["DesktopModuleId"]));
        }

        private static void UpdateSupportedFeatures(object objController, int desktopModuleId)
        {
            try
            {
                DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                if (desktopModule != null)
                {
                    // Initialise the SupportedFeatures
                    desktopModule.SupportedFeatures = 0;

                    // Test the interfaces
                    desktopModule.IsPortable = objController is IPortable;
#pragma warning disable 0618
                    desktopModule.IsSearchable = (objController is ModuleSearchBase) || (objController is ISearchable);
#pragma warning restore 0618
                    desktopModule.IsUpgradeable = objController is IUpgradeable;
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, false, false);

                    foreach (PortalInfo portal in PortalController.Instance.GetPortals())
                    {
                        DataCache.RemoveCache(string.Format(DataCache.DesktopModuleCacheKey, portal.PortalID));
                        DataCache.RemoveCache(string.Format(DataCache.PortalDesktopModuleCacheKey, portal.PortalID));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }
    }
}
