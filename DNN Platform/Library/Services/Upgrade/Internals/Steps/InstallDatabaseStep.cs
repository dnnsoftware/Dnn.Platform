#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallDatabaseStep - Step that installs Database
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InstallDatabaseStep : BaseInstallationStep
    {
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var counter = 0;
            const int totalSteps = 6;
            const int percentForEachStep = 100 / totalSteps;
            var percentForMiniStep = 0;

            var installConfig = InstallController.Instance.GetInstallConfig();
            var providerPath = DataProvider.Instance().GetProviderPath();

            //Step 1 - Install Base Database. Only when it's not already installed. Globals.DataBaseVersion is null when SPs are not present
            if (Globals.DataBaseVersion == null)
            {
                var defaultProvider = Config.GetDefaultProvider("data").Name;

                percentForMiniStep = percentForEachStep / (installConfig.Scripts.Count + 1);
                foreach (var script in installConfig.Scripts)
                {
                    var scriptFile = providerPath + script + "." + defaultProvider;
                    var description = Localization.Localization.GetString("InstallingDataBaseScriptStep", LocalInstallResourceFile);
                    Details = description + Upgrade.GetFileNameWithoutExtension(scriptFile);
                    var exception = Upgrade.ExecuteScript(scriptFile, false);
                    if (!string.IsNullOrEmpty(exception))
                    {
	                    Errors.Add(exception);
						Status = StepStatus.Retry;
						return;
                    }
                    Percentage += percentForMiniStep;
                }

                // update the version
                Globals.UpdateDataBaseVersion(new Version(installConfig.Version));

                Details = Localization.Localization.GetString("InstallingMembershipDatabaseScriptStep", LocalInstallResourceFile);
                //Optionally Install the memberRoleProvider
                var exceptions = Upgrade.InstallMemberRoleProvider(providerPath, false);
				if (!string.IsNullOrEmpty(exceptions))
				{
					Errors.Add(exceptions);
					Status = StepStatus.Retry;
					return;
				}
            }
            Percentage = percentForEachStep * counter++;

            //Step 2 - Process the Upgrade Script files
            var versions = new List<Version>();
            var scripts = Upgrade.GetUpgradeScripts(providerPath, DataProvider.Instance().GetVersion());
            if (scripts.Count > 0)
            {
                percentForMiniStep = percentForEachStep/(scripts.Count);
                foreach (string scriptFile in scripts)
                {
                    var fileName = Upgrade.GetFileNameWithoutExtension(scriptFile);
                    var version = new Version(fileName);
                    string description = Localization.Localization.GetString("ProcessingUpgradeScript", LocalInstallResourceFile);
                    Details = description + fileName;

                    bool scriptExecuted;
					var exceptions = Upgrade.UpgradeVersion(scriptFile, false, out scriptExecuted);
					if (!string.IsNullOrEmpty(exceptions))
					{
						Errors.Add(exceptions);
						Status = StepStatus.Retry;
						return;
					}

                    if (scriptExecuted)
                    {
                        versions.Add(version);
                    }

                    Percentage += percentForMiniStep;
                }
            }
            Percentage = percentForEachStep * counter++;

            //Step 3 - Perform version specific application upgrades
            foreach (Version ver in versions)
            {
                string description = Localization.Localization.GetString("UpgradingVersionApplication", LocalInstallResourceFile);
                Details = description + ver;
                var exceptions = Upgrade.UpgradeApplication(providerPath, ver, false);
				if (!string.IsNullOrEmpty(exceptions))
				{
					Errors.Add(exceptions);
					Status = StepStatus.Retry;
					return;
				}
                Percentage += percentForMiniStep;
            }
            Percentage = percentForEachStep * counter++;


			//Step 4 - Execute config file updates
			foreach (Version ver in versions)
			{
				string description = Localization.Localization.GetString("UpdatingConfigFile", LocalInstallResourceFile);
				Details = description + ver;
				var exceptions = Upgrade.UpdateConfig(providerPath, ver, false);
				if (!string.IsNullOrEmpty(exceptions))
				{
					Errors.Add(exceptions);
					Status = StepStatus.Retry;
					return;
				}
				Percentage += percentForMiniStep;
			}
			Percentage = percentForEachStep * counter++;

            //Step 5 - Delete files which are no longer used
            foreach (Version ver in versions)
            {
                string description = Localization.Localization.GetString("DeletingOldFiles", LocalInstallResourceFile);
                Details = description + ver;
                var exceptions = Upgrade.DeleteFiles(providerPath, ver, false);
				if (!string.IsNullOrEmpty(exceptions))
				{
					Errors.Add(exceptions);
					Status = StepStatus.Retry;
					return;
				}
                Percentage += percentForMiniStep;
            }
            Percentage = percentForEachStep * counter++;

            //Step 6 - Perform general application upgrades
            Details = Localization.Localization.GetString("UpgradingNormalApplication", LocalInstallResourceFile);
            Upgrade.UpgradeApplication();

            DataCache.ClearHostCache(true);
            Percentage = percentForEachStep * counter++;
            
            Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }

        #endregion

    }
}
