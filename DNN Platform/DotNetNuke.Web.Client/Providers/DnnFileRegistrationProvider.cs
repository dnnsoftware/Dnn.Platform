// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;

    using ClientDependency.Core.Config;
    using ClientDependency.Core.FileRegistration.Providers;

    public abstract class DnnFileRegistrationProvider : WebFormsFileRegistrationProvider
    {
        private readonly ClientResourceSettings dnnSettingsHelper = new ClientResourceSettings();

        /// <summary>
        /// Gets a value indicating whether checks if the composite files option is set for the current portal (DNN site settings).
        /// If not enabled at the portal level it defers to the core CDF setting (web.config).
        /// </summary>
        public override bool EnableCompositeFiles
        {
            get
            {
                var settingsVersion = this.dnnSettingsHelper.AreCompositeFilesEnabled();
                return settingsVersion.HasValue ? settingsVersion.Value : base.EnableCompositeFiles;
            }
        }
    }
}
