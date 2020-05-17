// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public class ModuleSettingsPresenterBase<TView> : ModulePresenterBase<TView> where TView : class, ISettingsView
    {
        #region Constructors

        public ModuleSettingsPresenterBase(TView view) : base(view)
        {
            view.OnLoadSettings += OnLoadSettingsInternal;
            view.OnSaveSettings += OnSaveSettingsInternal;

            ModuleSettings = new Dictionary<string, string>();
            TabModuleSettings = new Dictionary<string, string>();
        }

        #endregion

        public Dictionary<string, string> ModuleSettings { get; set; }

        public Dictionary<string, string> TabModuleSettings { get; set; }

        #region Event Handlers

        private void OnLoadSettingsInternal(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void OnSaveSettingsInternal(object sender, EventArgs e)
        {
            SaveSettings();
        }

        #endregion

        #region Protected Methods

        protected override void LoadFromContext()
        {
            base.LoadFromContext();

            foreach (var key in ModuleContext.Configuration.ModuleSettings.Keys)
            {
                ModuleSettings.Add(Convert.ToString(key), Convert.ToString(ModuleContext.Configuration.ModuleSettings[key]));
            }

            foreach (var key in ModuleContext.Configuration.TabModuleSettings.Keys)
            {
                TabModuleSettings.Add(Convert.ToString(key), Convert.ToString(ModuleContext.Configuration.TabModuleSettings[key]));
            }
        }

        protected virtual void LoadSettings()
        {
        }

        protected virtual void SaveSettings()
        {
        }

        #endregion

    }
}
