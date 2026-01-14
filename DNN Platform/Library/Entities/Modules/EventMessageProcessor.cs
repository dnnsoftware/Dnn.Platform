// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Globalization;
    using System.Web;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.EventQueue;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The primary event message processor.</summary>
    public class EventMessageProcessor : EventMessageProcessorBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventMessageProcessor));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="EventMessageProcessor"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IServiceProvider. Scheduled removal in v12.0.0.")]
        public EventMessageProcessor()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EventMessageProcessor"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        public EventMessageProcessor(IBusinessControllerProvider businessControllerProvider, IEventLogger eventLogger)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.DependencyProvider.GetRequiredService<IBusinessControllerProvider>();
            this.eventLogger = eventLogger ?? Globals.DependencyProvider.GetRequiredService<IEventLogger>();
        }

        /// <summary>Creates and send an <c>"ImportModule"</c> <see cref="EventMessage"/> for the module.</summary>
        /// <param name="objModule">The module info.</param>
        /// <param name="content">The content to import.</param>
        /// <param name="version">The module version.</param>
        /// <param name="userId">The user ID.</param>
        public static void CreateImportModuleMessage(ModuleInfo objModule, string content, string version, int userId)
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
            appStartMessage.Attributes.Add("ModuleId", objModule.ModuleID.ToString(CultureInfo.InvariantCulture));
            appStartMessage.Attributes.Add("Content", content);
            appStartMessage.Attributes.Add("Version", version);
            appStartMessage.Attributes.Add("UserID", userId.ToString(CultureInfo.InvariantCulture));

            // send it to occur on next App_Start Event
            EventQueueController.SendMessage(appStartMessage, "Application_Start_FirstRequest");
        }

        /// <inheritdoc/>
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
                        this.UpgradeModule(message);
                        break;
                    case "ImportModule":
                        this.ImportModule(message);
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

        private static void UpdateSupportedFeatures(EventMessage message)
        {
            var businessControllerClass = message.Attributes["BusinessControllerClass"];
            var controllerType = Reflection.CreateType(businessControllerClass);
            UpdateSupportedFeatures(controllerType, Convert.ToInt32(message.Attributes["DesktopModuleId"], CultureInfo.InvariantCulture));
        }

        private static void UpdateSupportedFeatures(Type controllerType, int desktopModuleId)
        {
            try
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                if (desktopModule == null)
                {
                    return;
                }

                // Initialise the SupportedFeatures
                desktopModule.SupportedFeatures = 0;

                // Test the interfaces
                desktopModule.IsPortable = typeof(IPortable).IsAssignableFrom(controllerType);
                desktopModule.IsSearchable = typeof(ModuleSearchBase).IsAssignableFrom(controllerType);
                desktopModule.IsUpgradeable = typeof(IUpgradeable).IsAssignableFrom(controllerType);
                DesktopModuleController.SaveDesktopModule(desktopModule, false, false, false);

                foreach (IPortalInfo portal in PortalController.Instance.GetPortals())
                {
                    DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.DesktopModuleCacheKey, portal.PortalId));
                    DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.PortalDesktopModuleCacheKey, portal.PortalId));
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private void ImportModule(EventMessage message)
        {
            try
            {
                var controller = this.businessControllerProvider.GetInstance<IPortable>(message.Attributes["BusinessControllerClass"]);
                if (controller is null)
                {
                    return;
                }

                var moduleId = Convert.ToInt32(message.Attributes["ModuleId"], CultureInfo.InvariantCulture);
                var content = HttpUtility.HtmlDecode(message.Attributes["Content"]);
                var version = message.Attributes["Version"];
                var userId = Convert.ToInt32(message.Attributes["UserId"], CultureInfo.InvariantCulture);

                // call the IPortable interface for the module/version
                controller.ImportModule(moduleId, content, version, userId);

                // Synchronize Module Cache
                ModuleController.SynchronizeModule(moduleId);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }

        private void UpgradeModule(EventMessage message)
        {
            try
            {
                var businessControllerClass = message.Attributes["BusinessControllerClass"];
                var businessControllerType = Reflection.CreateType(businessControllerClass);
                var controller = this.businessControllerProvider.GetInstance<IUpgradeable>(businessControllerClass);
                if (controller is not null)
                {
                    // get the list of applicable versions
                    var upgradeVersions = message.Attributes["UpgradeVersionsList"].Split(',');
                    foreach (var version in upgradeVersions)
                    {
                        // call the IUpgradeable interface for the module/version
                        var results = controller.UpgradeModule(version);

                        // log the upgrade results
                        var log = new LogInfo { LogTypeKey = nameof(EventLogType.MODULE_UPDATED), };
                        log.AddProperty("Module Upgraded", businessControllerClass);
                        log.AddProperty("Version", version);
                        if (!string.IsNullOrEmpty(results))
                        {
                            log.AddProperty("Results", results);
                        }

                        this.eventLogger.AddLog(log);
                    }
                }

                var desktopModuleId = Convert.ToInt32(message.Attributes["DesktopModuleId"], CultureInfo.InvariantCulture);
                UpdateSupportedFeatures(businessControllerType, desktopModuleId);
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
        }
    }
}
