// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using ClientDependency.Core.Config;

namespace DotNetNuke.Web.Client.Providers
{
    using ClientDependency.Core.FileRegistration.Providers;

    public abstract class DnnFileRegistrationProvider : WebFormsFileRegistrationProvider
    {
        private readonly ClientResourceSettings dnnSettingsHelper = new ClientResourceSettings();

        /// <summary>
        /// Checks if the composite files option is set for the current portal (DNN site settings).
        /// If not enabled at the portal level it defers to the core CDF setting (web.config).
        /// </summary>
        public override bool EnableCompositeFiles
        {
            get
            {
                var settingsVersion = dnnSettingsHelper.AreCompositeFilesEnabled();
                return settingsVersion.HasValue ? settingsVersion.Value : base.EnableCompositeFiles;
            }
        }
    }
}
