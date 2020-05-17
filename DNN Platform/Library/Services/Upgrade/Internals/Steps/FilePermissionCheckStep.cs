﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// FilePermissionCheck - Step that performs file permission checks prior to installation
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class FilePermissionCheckStep : BaseInstallationStep
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (FilePermissionCheckStep));
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var verifiers = new List<FileSystemPermissionVerifier>
                                {
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~")),
                                    new FileSystemPermissionVerifier(HttpContext.Current.Server.MapPath("~/App_Data"))
                                };

            Details = Localization.Localization.GetString("FolderCreateCheck", LocalInstallResourceFile)
                    + Localization.Localization.GetString("FileCreateCheck", LocalInstallResourceFile)
                    + Localization.Localization.GetString("FileDeleteCheck", LocalInstallResourceFile)
                    + Localization.Localization.GetString("FolderDeleteCheck", LocalInstallResourceFile);
            Logger.TraceFormat("FilePermissionCheck - {0}", Details);

            if (!verifiers.All(v => v.VerifyAll()))
                Errors.Add(string.Format(Localization.Localization.GetString("StepFailed", LocalInstallResourceFile), Details));
            Percentage = 100;

            Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
            Logger.TraceFormat("FilePermissionCheck Status - {0}", Status);
        }

        #endregion
    }
}
