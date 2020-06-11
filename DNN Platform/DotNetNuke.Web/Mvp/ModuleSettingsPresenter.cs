﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            view.OnLoadSettings += this.OnLoadSettingsInternal;
            view.OnSaveSettings += this.OnSaveSettingsInternal;

            this.ModuleSettings = new Dictionary<string, string>();
            this.TabModuleSettings = new Dictionary<string, string>();
        }

        #endregion

        public Dictionary<string, string> ModuleSettings { get; set; }

        public Dictionary<string, string> TabModuleSettings { get; set; }

        #region Event Handlers

        private void OnLoadSettingsInternal(object sender, EventArgs e)
        {
            this.LoadSettings();
        }

        private void OnSaveSettingsInternal(object sender, EventArgs e)
        {
            this.SaveSettings();
        }

        #endregion

        #region Protected Methods

        protected override void LoadFromContext()
        {
            base.LoadFromContext();

            foreach (var key in this.ModuleContext.Configuration.ModuleSettings.Keys)
            {
                this.ModuleSettings.Add(Convert.ToString(key), Convert.ToString(this.ModuleContext.Configuration.ModuleSettings[key]));
            }

            foreach (var key in this.ModuleContext.Configuration.TabModuleSettings.Keys)
            {
                this.TabModuleSettings.Add(Convert.ToString(key), Convert.ToString(this.ModuleContext.Configuration.TabModuleSettings[key]));
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
