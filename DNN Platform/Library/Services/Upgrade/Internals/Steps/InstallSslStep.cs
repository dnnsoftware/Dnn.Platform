// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Upgrade.Internals;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>InstallSslStep - Step that checks and sets SSL where needed.</summary>
    public class InstallSslStep : BaseInstallationStep
    {
        /// <summary>Main method to execute the step.</summary>
        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            var installConfig = InstallController.Instance.GetInstallConfig();

            var counter = 0;
            var totalSteps = installConfig.Portals.Count;
            var percentForEachStep = 100 / totalSteps;

            var portals = PortalController.Instance.GetPortals().Cast<PortalInfo>();

            foreach (var portalConfig in installConfig.Portals)
            {
                IPortalInfo portal = portals.FirstOrDefault(p => p.PortalName == portalConfig.PortalName);
                var description = Localization.GetString("InstallingSslStep", this.LocalInstallResourceFile);
                this.Details = string.Format(CultureInfo.CurrentCulture, description, portalConfig.PortalName);
                if (portal != null)
                {
                    PortalController.UpdatePortalSetting(portal.PortalId, "SSLSetup", portalConfig.IsSsl ? "1" : "0", false);
                    DataProvider.Instance().SetAllPortalTabsSecure(portal.PortalId, portalConfig.IsSsl);
                }
                else
                {
                    this.Errors.Add(string.Format(CultureInfo.CurrentCulture, Localization.GetString("InstallingSslStepSiteNotFound", this.LocalInstallResourceFile), portalConfig.PortalName));
                }

                counter++;
                this.Percentage = counter * percentForEachStep;
            }

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
