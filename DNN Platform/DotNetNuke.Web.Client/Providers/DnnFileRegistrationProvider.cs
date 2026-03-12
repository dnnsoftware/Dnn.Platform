// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Client.Providers
{
    using System;

    using ClientDependency.Core.FileRegistration.Providers;

    /// <summary>DNN's file registration provider.</summary>
    public abstract class DnnFileRegistrationProvider : WebFormsFileRegistrationProvider
    {
        private readonly ClientResourceSettings dnnSettingsHelper = new ClientResourceSettings();

        /// <summary>
        /// Gets a value indicating whether checks if the composite files option is set for the current portal (DNN site settings).
        /// If not enabled at the portal level it defers to the core CDF setting (web.config).
        /// </summary>
        [Obsolete("Composite Files (bundling) have been deprecated in DNN 10.2.0. Scheduled for removal in DNN 12.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override bool EnableCompositeFiles
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            get
            {
                var settingsVersion = this.dnnSettingsHelper.AreCompositeFilesEnabled();
                return settingsVersion.HasValue ? settingsVersion.Value : base.EnableCompositeFiles;
            }
        }
    }
}
