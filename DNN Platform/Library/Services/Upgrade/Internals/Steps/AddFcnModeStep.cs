// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Instrumentation;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// AddFcnModeVerificationStep - Step that performs FcnMode verification checks prior to installation.
    /// </summary>
    /// ------------------------------------------------------------------------------------------------
    public class AddFcnModeStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddFcnModeStep));

        public override void Execute()
        {
            this.Percentage = 0;
            this.Status = StepStatus.Running;

            string strError = Config.AddFCNMode(Config.FcnMode.Single);
            if (!string.IsNullOrEmpty(strError))
            {
                this.Errors.Add(Localization.GetString("FcnMode", this.LocalInstallResourceFile) + ": " + strError);
                Logger.TraceFormat("Adding FcnMode : {0}", strError);
            }

            this.Status = this.Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
