// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework.MvcPipeline
{
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Settings;
    using DotNetNuke.Entities.Portals;

    internal class MvcPipelineSettingsRepository : SettingsRepository<MvcPipelineSettings>
    {
        private readonly IPortalSettings portalSettings;

        public MvcPipelineSettingsRepository(
          IModuleController moduleController,
          IHostSettings hostSettings,
          IHostSettingsService hostSettingsService,
          IPortalController portalController,
          IPortalSettings portalSettings)
         : base(moduleController, hostSettings, hostSettingsService, portalController)
        {
            this.portalSettings = portalSettings;
        }

        public MvcPipelineSettings GetSettings()
        {
            return this.GetSettings(this.portalSettings.PortalId);
        }

        public void SaveSettings(MvcPipelineSettings settings)
        {
            this.SaveSettings(this.portalSettings.PortalId, settings);
        }
    }
}
