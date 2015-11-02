// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Dnn.Modules.DynamicContentManager.Components.Entities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Personalization;

namespace Dnn.Modules.DynamicContentManager.Components
{
    public class SettingsManager : ServiceLocator<ISettingsManager, SettingsManager>, ISettingsManager
    {

        public DCCSettings Get(int portalId, int moduleId)
        {
            var settings = (DCCSettings)Personalization.GetProfile("DCC", "UserSettings" + portalId + moduleId) ?? GetDefaultSettings();
            
            return settings;
        }

        public void Save(DCCSettings setting, PortalSettings portalSettings, int moduleId)
        {
            try
            {
                var personalizationController = new PersonalizationController();
                var personalization = personalizationController.LoadProfile(portalSettings.UserId, portalSettings.PortalId);

                Personalization.SetProfile(personalization, "DCC", "UserSettings" + portalSettings.PortalId + moduleId, setting);
                personalizationController.SaveProfile(personalization);
            }
            catch (Exception e)
            {
                DotNetNuke.Services.Log.EventLog.EventLogController.Instance.AddLog("Personalization Save Failed",
                    "Failed to load/save personalization data.", portalSettings, -1, DotNetNuke.Services.Log.EventLog.EventLogController.EventLogType.ADMIN_ALERT);
            }
        }


        private DCCSettings GetDefaultSettings()
        {
            return new DCCSettings
            {
                ContentTypePageSize = 10, //TODO Set Default AnalyticPeriod
                DataTypePageSize = 10,
                TemplatePageSize = 10
            };
        }

        protected override Func<ISettingsManager> GetFactory()
        {
            return () => new SettingsManager();
        }
    }
}
