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