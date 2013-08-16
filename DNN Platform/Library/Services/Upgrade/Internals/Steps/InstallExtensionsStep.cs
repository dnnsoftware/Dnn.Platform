#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.IO;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// InstallExtensionsStep - Step that installs all the Extensions
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class InstallExtensionsStep : BaseInstallationStep
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (InstallExtensionsStep));
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var extensions = new string[] { "Module", "Skin", "Container", "Language", "Provider", "AuthSystem", "Package" };
            var percentForEachStep = 100 / extensions.Length;
            var counter = 0;

            foreach (var extension in extensions)
            {
                try
                {
                    InstallPackages(extension);
                    Percentage = percentForEachStep * counter++;
                }

                catch (Exception ex)
                {
                    Errors.Add(Localization.Localization.GetString("InstallingExtensionType", LocalInstallResourceFile) + ": " + ex.Message);
                }
            }

			Status = Errors.Count > 0 ? StepStatus.Retry : StepStatus.Done;
        }

        #endregion

        #region private methods

        private void InstallPackages(string packageType)
        {

            Details = string.Format(Localization.Localization.GetString("InstallingExtensionType", LocalInstallResourceFile), packageType);
            Logger.Trace(Details);
            var installPackagePath = Globals.ApplicationMapPath + "\\Install\\" + packageType;
            if (Directory.Exists(installPackagePath))
            {
                var files = Directory.GetFiles(installPackagePath);
                if (files.Length > 0)
                {
                    var percentForEachStep = 100/files.Length;
                    var counter = 0;
                    foreach (string file in files)
                    {
                        if (Path.GetExtension(file.ToLower()) == ".zip")
                        {
	                        var message = string.Format(Localization.Localization.GetString("InstallingExtension", LocalInstallResourceFile), packageType, Path.GetFileName(file));
                            Details = message;
                            Logger.Trace(Details);
                            var success = Upgrade.InstallPackage(file, packageType, false);
							if (!success)
							{
								Errors.Add(message);
								break;
							}
                        }
                        Percentage = percentForEachStep * counter++;
                    }
                }
            }
        }

        #endregion
    }
}