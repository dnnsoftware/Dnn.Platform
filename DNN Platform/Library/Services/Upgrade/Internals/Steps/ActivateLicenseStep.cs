// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// DatabaseVerificationStep - Step that performs database verification checks prior to installation
    /// </summary>
    /// ------------------------------------------------------------------------------------------------  
    public class ActivateLicenseStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ActivateLicenseStep));

        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            Details = Localization.Localization.GetString("LicenseActivation", LocalInstallResourceFile);
            var installConfig = InstallController.Instance.GetInstallConfig();
            var licenseConfig = (installConfig != null) ? installConfig.License : null;            

            if (licenseConfig != null && (!string.IsNullOrEmpty(licenseConfig.AccountEmail) && !string.IsNullOrEmpty(licenseConfig.InvoiceNumber)
                                && !string.IsNullOrEmpty(licenseConfig.LicenseType) && !string.IsNullOrEmpty(licenseConfig.WebServer)))
            {
                try
                {
                    var activationResult = "";
                    activationResult = Upgrade.ActivateLicense();

                    if (!activationResult.ToLowerInvariant().Contains("success"))
                    {
                        Errors.Add(Localization.Localization.GetString("LicenseActivation", LocalInstallResourceFile) + ": " + activationResult);
                        Logger.TraceFormat("ActivateLicense Status - {0}", activationResult);
                    }
                }
                catch (Exception ex)
                {
                    Errors.Add(Localization.Localization.GetString("LicenseActivation", LocalInstallResourceFile) + ": " + ex.Message);
                    Logger.TraceFormat("ActivateLicense Status - {0}", ex.Message);
                }
            }

            Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
