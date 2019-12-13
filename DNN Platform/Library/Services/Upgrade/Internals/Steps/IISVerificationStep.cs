using Microsoft.Win32;
using System;
using System.Web;

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
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
            Status = StepStatus.Running;

            Details = Localization.Localization.GetString("CheckingIIS", LocalInstallResourceFile);

            // Checks for integrated pipeline mode.
            if (!HttpRuntime.UsingIntegratedPipeline)
            {
                Errors.Add(Localization.Localization.GetString("IISVerificationFail", LocalInstallResourceFile));
                Status = StepStatus.Abort;
                return;
            }

            // Check for .Net Framework 4.7.2
            if (!IsDotNetVersionAtLeast(461808))
            {
                Errors.Add(Localization.Localization.GetString("DotNetVersion472Required", LocalInstallResourceFile));
                Status = StepStatus.Abort;
                return;
            }

            Status = StepStatus.Done;
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
