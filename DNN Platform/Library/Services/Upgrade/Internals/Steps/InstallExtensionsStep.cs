// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;
    using System.IO;

    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallExtensionsStep - Step that installs all the Extensions.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class InstallExtensionsStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallExtensionsStep));

        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public override void Execute()
        {
            var packages = Upgrade.GetInstallPackages();
            if (packages.Count == 0)
            {
                this.Percentage = 100;
                this.Status = StepStatus.Done;
                return;
            }

            this.Percentage = 0;
            this.Status = StepStatus.Running;

            var percentForEachStep = 100 / packages.Count;
            var counter = 0;
            foreach (var package in packages)
            {
                var file = package.Key;
                var packageType = package.Value.PackageType;
                var message = string.Format(Localization.GetString("InstallingExtension", this.LocalInstallResourceFile), packageType, Path.GetFileName(file));
                this.Details = message;
                Logger.Trace(this.Details);
                var success = Upgrade.InstallPackage(file, packageType, false);
                if (!success)
                {
                    this.Errors.Add(message);
                    break;
                }

                this.Percentage = percentForEachStep * counter++;
            }

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
