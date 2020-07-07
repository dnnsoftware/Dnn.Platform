// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp
{
    using System;

    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead. Scheduled removal in v11.0.0.")]
    public abstract class SettingsView<TModel> : SettingsViewBase, ISettingsView<TModel>
        where TModel : SettingsModel, new()
    {
        private TModel _model;

        public TModel Model
        {
            get
            {
                if (this._model == null)
                {
                    throw new InvalidOperationException(
                        "The Model property is currently null, however it should have been automatically initialized by the presenter. This most likely indicates that no presenter was bound to the control. Check your presenter bindings.");
                }

                return this._model;
            }

            set { this._model = value; }
        }

        protected string GetModuleSetting(string key, string defaultValue)
        {
            var value = defaultValue;

            if (this.Model.ModuleSettings.ContainsKey(key))
            {
                value = this.Model.ModuleSettings[key];
            }

            return value;
        }

        protected string GetTabModuleSetting(string key, string defaultValue)
        {
            var value = defaultValue;

            if (this.Model.TabModuleSettings.ContainsKey(key))
            {
                value = this.Model.TabModuleSettings[key];
            }

            return value;
        }
    }
}
