// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
