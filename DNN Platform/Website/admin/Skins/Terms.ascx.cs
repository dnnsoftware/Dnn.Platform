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
    public partial class Terms : SkinObjectBase
    {
        private readonly INavigationManager _navigationManager;
        private const string MyFileName = "Terms.ascx";
        public string Text { get; set; }

        public string CssClass { get; set; }

        public Terms()
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
                    hypTerms.CssClass = CssClass;
                }
                if (!String.IsNullOrEmpty(Text))
                {
                    hypTerms.Text = Text;
                }
                else
                {
                    hypTerms.Text = Localization.GetString("Terms", Localization.GetResourceFile(this, MyFileName));
                }
                hypTerms.NavigateUrl = PortalSettings.TermsTabId == Null.NullInteger ? _navigationManager.NavigateURL(PortalSettings.ActiveTab.TabID, "Terms") : _navigationManager.NavigateURL(PortalSettings.TermsTabId);

                hypTerms.Attributes["rel"] = "nofollow";
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}
