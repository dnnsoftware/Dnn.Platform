// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using System;
using Microsoft.Extensions.DependencyInjection;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// -----------------------------------------------------------------------------
    public partial class Privacy : SkinObjectBase
    {
        private readonly INavigationManager _navigationManager;
        private const string MyFileName = "Privacy.ascx";
        public string Text { get; set; }

        public string CssClass { get; set; }

        public Privacy()
        {
            _navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        private void InitializeComponent()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!String.IsNullOrEmpty(CssClass))
                {
                    hypPrivacy.CssClass = CssClass;
                }
                if (!String.IsNullOrEmpty(Text))
                {
                    hypPrivacy.Text = Text;
                }
                else
                {
                    hypPrivacy.Text = Localization.GetString("Privacy", Localization.GetResourceFile(this, MyFileName));
                }
                hypPrivacy.NavigateUrl = PortalSettings.PrivacyTabId == Null.NullInteger ? _navigationManager.NavigateURL(PortalSettings.ActiveTab.TabID, "Privacy") : _navigationManager.NavigateURL(PortalSettings.PrivacyTabId);
                hypPrivacy.Attributes["rel"] = "nofollow";
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
