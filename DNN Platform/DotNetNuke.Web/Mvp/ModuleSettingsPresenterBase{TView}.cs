// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>Represents a class that is a presenter for a settings control in a Web Forms Model-View-Presenter application.</summary>
    /// <typeparam name="TView">The type of the view.</typeparam>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class ModuleSettingsPresenterBase<TView> : ModulePresenterBase<TView>
        where TView : class, ISettingsView
    {
        /// <summary>Initializes a new instance of the <see cref="ModuleSettingsPresenterBase{TView}"/> class.</summary>
        /// <param name="view">The view.</param>
        public ModuleSettingsPresenterBase(TView view)
            : base(view)
        {
            view.OnLoadSettings += this.OnLoadSettingsInternal;
            view.OnSaveSettings += this.OnSaveSettingsInternal;

            this.ModuleSettings = new Dictionary<string, string>();
            this.TabModuleSettings = new Dictionary<string, string>();
        }

        public Dictionary<string, string> ModuleSettings { get; set; }

        public Dictionary<string, string> TabModuleSettings { get; set; }

        /// <inheritdoc/>
        protected override void LoadFromContext()
        {
            base.LoadFromContext();

            foreach (var key in this.ModuleContext.Configuration.ModuleSettings.Keys)
            {
                this.ModuleSettings.Add(Convert.ToString(key, CultureInfo.InvariantCulture), Convert.ToString(this.ModuleContext.Configuration.ModuleSettings[key], CultureInfo.InvariantCulture));
            }

            foreach (var key in this.ModuleContext.Configuration.TabModuleSettings.Keys)
            {
                this.TabModuleSettings.Add(Convert.ToString(key, CultureInfo.InvariantCulture), Convert.ToString(this.ModuleContext.Configuration.TabModuleSettings[key], CultureInfo.InvariantCulture));
            }
        }

        protected virtual void LoadSettings()
        {
        }

        protected virtual void SaveSettings()
        {
        }

        private void OnLoadSettingsInternal(object sender, EventArgs e)
        {
            this.LoadSettings();
        }

        private void OnSaveSettingsInternal(object sender, EventArgs e)
        {
            this.SaveSettings();
        }
    }
}
