﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
          
            if (culture.ToLowerInvariant() != "en-us")
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
