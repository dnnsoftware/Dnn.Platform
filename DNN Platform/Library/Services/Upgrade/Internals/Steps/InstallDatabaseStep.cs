// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Services.Upgrade.Internals;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallDatabaseStep - Step that installs Database.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class InstallDatabaseStep : BaseInstallationStep
    {
        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            var counter = 0;
            const int totalSteps = 6;
            const int percentForEachStep = 100 / totalSteps;
            var percentForMiniStep = 0;

            var installConfig = InstallController.Instance.GetInstallConfig();
            var providerPath = DataProvider.Instance().GetProviderPath();

            // Step 1 - Install Base Database. Only when it's not already installed. Globals.DataBaseVersion is null when SPs are not present
            if (Globals.DataBaseVersion == null)
            {
                var defaultProvider = Config.GetDefaultProvider("data").Name;

                percentForMiniStep = percentForEachStep / (installConfig.Scripts.Count + 1);
                foreach (var script in installConfig.Scripts)
                {
                    var scriptFile = providerPath + script + "." + defaultProvider;
                    var description = Localization.GetString("InstallingDataBaseScriptStep", this.LocalInstallResourceFile);
                    this.Details = description + Upgrade.GetFileNameWithoutExtension(scriptFile);
                    var exception = Upgrade.ExecuteScript(scriptFile, false);
                    if (!string.IsNullOrEmpty(exception))
                    {
                        this.Errors.Add(exception);
                        this.Status = StepStatus.Retry;
                        return;
                    }

                    this.Percentage += percentForMiniStep;
                }

                // update the version
                Globals.UpdateDataBaseVersion(new Version(installConfig.Version));

                this.Details = Localization.GetString("InstallingMembershipDatabaseScriptStep", this.LocalInstallResourceFile);

                // Optionally Install the memberRoleProvider
                var exceptions = Upgrade.InstallMemberRoleProvider(providerPath, false);
                if (!string.IsNullOrEmpty(exceptions))
                {
                    this.Errors.Add(exceptions);
                    this.Status = StepStatus.Retry;
                    return;
                }
            }

            this.Percentage = percentForEachStep * counter++;

            // Step 2 - Process the Upgrade Script files
            var versions = new List<Version>();
            var scripts = Upgrade.GetUpgradeScripts(providerPath, DataProvider.Instance().GetVersion());
            if (scripts.Count > 0)
            {
                percentForMiniStep = percentForEachStep / scripts.Count;
                foreach (string scriptFile in scripts)
                {
                    var fileName = Upgrade.GetFileNameWithoutExtension(scriptFile);
                    var version = new Version(fileName);
                    string description = Localization.GetString("ProcessingUpgradeScript", this.LocalInstallResourceFile);
                    this.Details = description + fileName;

                    bool scriptExecuted;
                    var exceptions = Upgrade.UpgradeVersion(scriptFile, false, out scriptExecuted);
                    if (!string.IsNullOrEmpty(exceptions))
                    {
                        this.Errors.Add(exceptions);
                        this.Status = StepStatus.Retry;
                        return;
                    }

                    if (scriptExecuted)
                    {
                        versions.Add(version);
                    }

                    this.Percentage += percentForMiniStep;
                }
            }

            this.Percentage = percentForEachStep * counter++;

            // Step 3 - Perform version specific application upgrades
            foreach (Version ver in versions)
            {
                string description = Localization.GetString("UpgradingVersionApplication", this.LocalInstallResourceFile);
                this.Details = description + ver;
                var exceptions = Upgrade.UpgradeApplication(providerPath, ver, false);
                if (!string.IsNullOrEmpty(exceptions))
                {
                    this.Errors.Add(exceptions);
                    this.Status = StepStatus.Retry;
                    return;
                }

                this.Percentage += percentForMiniStep;
            }

            this.Percentage = percentForEachStep * counter++;

            // Step 4 - Execute config file updates
            foreach (Version ver in versions)
            {
                string description = Localization.GetString("UpdatingConfigFile", this.LocalInstallResourceFile);
                this.Details = description + ver;
                var exceptions = Upgrade.UpdateConfig(providerPath, ver, false);
                if (!string.IsNullOrEmpty(exceptions))
                {
                    this.Errors.Add(exceptions);
                    this.Status = StepStatus.Retry;
                    return;
                }

                this.Percentage += percentForMiniStep;
            }

            this.Percentage = percentForEachStep * counter++;

            // Step 5 - Delete files which are no longer used
            foreach (Version ver in versions)
            {
                string description = Localization.GetString("DeletingOldFiles", this.LocalInstallResourceFile);
                this.Details = description + ver;
                var exceptions = Upgrade.DeleteFiles(providerPath, ver, false);
                if (!string.IsNullOrEmpty(exceptions))
                {
                    this.Errors.Add(exceptions);
                    this.Status = StepStatus.Retry;
                    return;
                }

                this.Percentage += percentForMiniStep;
            }

            this.Percentage = percentForEachStep * counter++;

            // Step 6 - Perform general application upgrades
            this.Details = Localization.GetString("UpgradingNormalApplication", this.LocalInstallResourceFile);
            Upgrade.UpgradeApplication();

            // Step 7 - Save Accept DNN Terms flag
            HostController.Instance.Update("AcceptDnnTerms", "Y");

            DataCache.ClearHostCache(true);
            this.Percentage = percentForEachStep * counter++;

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
