// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;

using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class SettingsViewBase : ModuleViewBase, ISettingsView, ISettingsControl
    {
        #region ISettingsControl Members

        public void LoadSettings()
        {
            if (OnLoadSettings != null)
            {
                OnLoadSettings(this, EventArgs.Empty);
            }

            OnSettingsLoaded();

        }

        public void UpdateSettings()
        {
            OnSavingSettings();

            if (OnSaveSettings != null)
            {
                OnSaveSettings(this, EventArgs.Empty);
            }
        }

        #endregion

        #region ISettingsView Members

        public event EventHandler OnLoadSettings;
        public event EventHandler OnSaveSettings;

        #endregion

        /// <summary>
        /// The OnSettingsLoaded method is called when the Settings have been Loaded 
        /// </summary>
        protected virtual void OnSettingsLoaded()
        {
        }

        /// <summary>
        /// OnSavingSettings method is called just before the Settings are saved
        /// </summary>
        protected virtual void OnSavingSettings()
        {
        }
    }
}
