// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvp
{
    using System;

    using DotNetNuke.UI.Modules;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class SettingsViewBase : ModuleViewBase, ISettingsView, ISettingsControl
    {
        public event EventHandler OnLoadSettings;

        public event EventHandler OnSaveSettings;

        public void LoadSettings()
        {
            if (this.OnLoadSettings != null)
            {
                this.OnLoadSettings(this, EventArgs.Empty);
            }

            this.OnSettingsLoaded();
        }

        public void UpdateSettings()
        {
            this.OnSavingSettings();

            if (this.OnSaveSettings != null)
            {
                this.OnSaveSettings(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// The OnSettingsLoaded method is called when the Settings have been Loaded.
        /// </summary>
        protected virtual void OnSettingsLoaded()
        {
        }

        /// <summary>
        /// OnSavingSettings method is called just before the Settings are saved.
        /// </summary>
        protected virtual void OnSavingSettings()
        {
        }
    }
}
