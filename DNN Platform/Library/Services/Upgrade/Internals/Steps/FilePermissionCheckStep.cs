// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Upgrade.Internals.Steps;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// FilePermissionCheck - Step that performs file permission checks prior to installation.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class FilePermissionCheckStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(FilePermissionCheckStep));

        /// <summary>
        /// Main method to execute the step.
        /// </summary>
        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            var verifiers = new List<FileSystemPermissionVerifier>
                                {
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~")),
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~/App_Data")),
                                };

            this.Details = Localization.GetString("FolderCreateCheck", this.LocalInstallResourceFile)
                    + Localization.GetString("FileCreateCheck", this.LocalInstallResourceFile)
                    + Localization.GetString("FileDeleteCheck", this.LocalInstallResourceFile)
                    + Localization.GetString("FolderDeleteCheck", this.LocalInstallResourceFile);
            Logger.TraceFormat("FilePermissionCheck - {0}", this.Details);

            if (!verifiers.All(v => v.VerifyAll()))
            {
                this.Errors.Add(string.Format(Localization.GetString("StepFailed", this.LocalInstallResourceFile), this.Details));
            }

            this.Percentage = 100;

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
            Logger.TraceFormat("FilePermissionCheck Status - {0}", this.Status);
        }
    }
}
