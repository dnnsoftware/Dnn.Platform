#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Web;
using System.Web.Configuration;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Upgrade.Internals;
using DotNetNuke.Services.Upgrade.Internals.Steps;

#endregion

namespace DotNetNuke.Services.Upgrade.InternalController.Steps
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// UpdateLanguagePackStep - Step that downloads and installs language pack
    /// </summary>
    /// -----------------------------------------------------------------------------    
    public class UpdateLanguagePackStep : BaseInstallationStep
    {
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpdateLanguagePackStep));
        #region Implementation of IInstallationStep

        /// <summary>
        /// Main method to execute the step
        /// </summary>        
        public override void Execute()
        {
            Percentage = 0;
            Status = StepStatus.Running;

            var installConfig = InstallController.Instance.GetInstallConfig();
            string culture = installConfig.InstallCulture;
          
            if (culture.ToLower() != "en-us")
            {
	            try
	            {
					//need apply the Licensing module after packages installed, so that we can know whats the edition of install instance. CE/PE/EE
					var document = Config.Load();
					var licensingNode = document.SelectSingleNode("/configuration/system.webServer/modules/add[@name='Licensing']");
					if (licensingNode != null)
					{
						var type = licensingNode.Attributes["type"].Value;
						var module = Reflection.CreateObject(type, null, false) as IHttpModule;
						module.Init(HttpContext.Current.ApplicationInstance);
					}

					InstallController.Instance.IsAvailableLanguagePack(culture);    
	            }
	            catch (Exception ex)
	            {
					//we shouldn't break the install process when LP download failed, for admin user can install the LP after website created.
					//so we logged what's wrong here, and user can check it later.
		            Logger.Error(ex);
	            }

            }
            Status = StepStatus.Done;
        }

        #endregion
    }
}
