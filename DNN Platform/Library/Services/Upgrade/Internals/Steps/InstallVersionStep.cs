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
    /// DatabaseVerificationStep - Step that performs database verification checks prior to installation
    /// </summary>
    /// ------------------------------------------------------------------------------------------------  
    public class InstallVersionStep : BaseInstallationStep
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(InstallVersionStep));

        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var databaseVersion = DataProvider.Instance().GetInstallVersion();

            string strError = Config.UpdateInstallVersion(databaseVersion);

            if (!string.IsNullOrEmpty(strError))
            {
                Errors.Add(Localization.Localization.GetString("InstallVersion", LocalInstallResourceFile) + ": " + strError);
                Logger.TraceFormat("Adding InstallVersion : {0}", strError);
            }

            Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }
    }
}
