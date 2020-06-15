// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System;
    using System.Web;

    using Microsoft.Win32;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>
    /// Performs verifications about the IIS environment.
    /// </summary>
    public class IISVerificationStep : BaseInstallationStep
    {
        /// <summary>
        /// Executes verifications on the IIS environment.
        /// </summary>
        public override void Execute()
        {
            this.Status = StepStatus.Running;

            this.Details = Localization.GetString("CheckingIIS", this.LocalInstallResourceFile);

            // Checks for integrated pipeline mode.
            if (!HttpRuntime.UsingIntegratedPipeline)
            {
                this.Errors.Add(Localization.GetString("IISVerificationFail", this.LocalInstallResourceFile));
                this.Status = StepStatus.Abort;
                return;
            }

            // Check for .Net Framework 4.7.2
            if (!IsDotNetVersionAtLeast(461808))
            {
                this.Errors.Add(Localization.GetString("DotNetVersion472Required", this.LocalInstallResourceFile));
                this.Status = StepStatus.Abort;
                return;
            }

            this.Status = StepStatus.Done;
        }

        private static bool IsDotNetVersionAtLeast(int version)
        {
            return GetVersion() >= version;
        }

        private static int GetVersion()
        {
            int release = 0;
            using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                release = Convert.ToInt32(key.GetValue("Release"));
            }

            return release;
        }
    }
}
