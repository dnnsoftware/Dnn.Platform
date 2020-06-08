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
