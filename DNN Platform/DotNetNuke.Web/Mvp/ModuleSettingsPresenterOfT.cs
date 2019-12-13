// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class ModuleSettingsPresenter<TView, TModel> : ModulePresenterBase<TView>
        where TView : class, ISettingsView<TModel>
        where TModel : SettingsModel, new()
    {
        protected ModuleSettingsPresenter(TView view) : base(view)
        {
            view.OnLoadSettings += OnLoadSettingsInternal;
            view.OnSaveSettings += OnSaveSettingsInternal;
        }

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

        protected override void OnLoad()
        {
            base.OnLoad();

            if (IsPostBack)
            {
                //Initialize dictionaries as LoadSettings is not called on Postback
                View.Model.ModuleSettings = new Dictionary<string, string>();
                View.Model.TabModuleSettings = new Dictionary<string, string>();
            }
        }

        protected virtual void LoadSettings()
        {
            View.Model.ModuleSettings = new Dictionary<string, string>(
                                            ModuleContext.Configuration.ModuleSettings
                                            .Cast<DictionaryEntry>()
                                            .ToDictionary(kvp => (string)kvp.Key, kvp => (string)kvp.Value)
                                        );

            View.Model.TabModuleSettings = new Dictionary<string, string>(
                                            ModuleContext.Configuration.TabModuleSettings
                                            .Cast<DictionaryEntry>()
                                            .ToDictionary(kvp => (string)kvp.Key, kvp => (string)kvp.Value)
                                        );
        }

        protected virtual void SaveSettings()
        {
            var controller = ModuleController.Instance;

            foreach (var setting in View.Model.ModuleSettings)
            {
                ModuleController.Instance.UpdateModuleSetting(ModuleId, setting.Key, setting.Value);
            }

            foreach (var setting in View.Model.TabModuleSettings)
            {
                ModuleController.Instance.UpdateTabModuleSetting(ModuleContext.TabModuleId, setting.Key, setting.Value);
            }
        }

        #endregion
    }
}
