// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Terms : SkinObjectBase
    {
        private const string MyFileName = "Terms.ascx";
        private readonly INavigationManager _navigationManager;

        public Terms()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string Text { get; set; }

        public string CssClass { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.hypTerms.CssClass = this.CssClass;
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.hypTerms.Text = this.Text;
                }
                else
                {
                    this.hypTerms.Text = Localization.GetString("Terms", Localization.GetResourceFile(this, MyFileName));
                }

                this.hypTerms.NavigateUrl = this.PortalSettings.TermsTabId == Null.NullInteger ? this._navigationManager.NavigateURL(this.PortalSettings.ActiveTab.TabID, "Terms") : this._navigationManager.NavigateURL(this.PortalSettings.TermsTabId);

                this.hypTerms.Attributes["rel"] = "nofollow";
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void InitializeComponent()
        {
        }
    }
}
