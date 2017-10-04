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
#region Usings

using System;

using DotNetNuke.UI.Modules;

#endregion

namespace DotNetNuke.Web.Mvp
{
    [Obsolete("Deprecated in DNN 9.2.0. Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
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