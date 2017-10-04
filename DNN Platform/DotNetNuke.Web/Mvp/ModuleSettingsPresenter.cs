#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
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