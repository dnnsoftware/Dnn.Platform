// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Html
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    ///   The Settings ModuleSettingsBase is used to manage the
    ///   settings for the HTML Module.
    /// </summary>
    public partial class Settings : ModuleSettingsBase
    {
        private readonly INavigationManager navigationManager;
        private HtmlModuleSettings moduleSettings;

        /// <summary>Initializes a new instance of the <see cref="Settings"/> class.</summary>
        public Settings()
        {
            this.navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private new HtmlModuleSettings ModuleSettings
        {
            get
            {
                return this.moduleSettings ?? (this.moduleSettings = new HtmlModuleSettingsRepository().GetSettings(this.ModuleConfiguration));
            }
        }

        /// <summary>  LoadSettings loads the settings from the Database and displays them.</summary>
        public override void LoadSettings()
        {
            try
            {
                if (!this.Page.IsPostBack)
                {
                    var htmlTextController = new HtmlTextController(this.navigationManager);

                    this.chkReplaceTokens.Checked = this.ModuleSettings.ReplaceTokens;
                    this.cbDecorate.Checked = this.ModuleSettings.UseDecorate;
                    this.txtSearchDescLength.Text = this.ModuleSettings.SearchDescLength.ToString();
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>  UpdateSettings saves the modified settings to the Database.</summary>
        public override void UpdateSettings()
        {
            try
            {
                var htmlTextController = new HtmlTextController(this.navigationManager);

                // update replace token setting
                this.ModuleSettings.ReplaceTokens = this.chkReplaceTokens.Checked;
                this.ModuleSettings.UseDecorate = this.cbDecorate.Checked;
                this.ModuleSettings.SearchDescLength = int.Parse(this.txtSearchDescLength.Text);
                var repo = new HtmlModuleSettingsRepository();
                repo.SaveSettings(this.ModuleConfiguration, this.ModuleSettings);

                // disable module caching if token replace is enabled
                if (this.chkReplaceTokens.Checked)
                {
                    ModuleInfo module = ModuleController.Instance.GetModule(this.ModuleId, this.TabId, false);
                    if (module.CacheTime > 0)
                    {
                        module.CacheTime = 0;
                        ModuleController.Instance.UpdateModule(module);
                    }
                }

                // Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
