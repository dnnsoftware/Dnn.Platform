#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
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