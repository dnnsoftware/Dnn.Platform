// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Instrumentation;

namespace DotNetNuke.Services.Upgrade.Internals.Steps
{
    /// ------------------------------------------------------------------------------------------------
    /// <summary>
    /// AddFcnModeVerificationStep - Step that performs FcnMode verification checks prior to installation
    /// </summary>
    /// ------------------------------------------------------------------------------------------------  
    public class AddFcnModeStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(AddFcnModeStep));

        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            string strError = Config.AddFCNMode(Config.FcnMode.Single);
            if (!string.IsNullOrEmpty(strError))
            {
                Errors.Add(Localization.Localization.GetString("FcnMode", LocalInstallResourceFile) + ": " + strError);
                Logger.TraceFormat("Adding FcnMode : {0}", strError);
            }
            Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
