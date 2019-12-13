#region Usings

using System;
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallExtensionsStep - Step that installs all the Extensions
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InstallExtensionsStep : BaseInstallationStep
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (InstallExtensionsStep));
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            var packages = Upgrade.GetInstallPackages();
            if (packages.Count == 0)
            {
                Percentage = 100;
                Status = StepStatus.Done;
                return;
            }

            Percentage = 0;
            Status = StepStatus.Running;

            var percentForEachStep = 100 / packages.Count;
            var counter = 0;
            foreach (var package in packages)
            {
                var file = package.Key;
                var packageType = package.Value.PackageType;
                var message = string.Format(Localization.Localization.GetString("InstallingExtension", LocalInstallResourceFile), packageType, Path.GetFileName(file));
                Details = message;
                Logger.Trace(Details);
                var success = Upgrade.InstallPackage(file, packageType, false);
                if (!success)
                {
                    Errors.Add(message);
                    break;
                }
                Percentage = percentForEachStep * counter++;
            }
			Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }

        #endregion
    }
}
