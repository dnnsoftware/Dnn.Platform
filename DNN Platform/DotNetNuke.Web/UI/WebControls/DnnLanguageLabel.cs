// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    public class DnnLanguageLabel : CompositeControl, ILocalizable
    {
        private Image _Flag;

        private Label _Label;

        public DnnLanguageLabel()
        {
            this.Localize = true;
        }

        public CultureDropDownTypes DisplayType { get; set; }

        public string Language
        {
            get
            {
                return (string)this.ViewState["Language"];
            }

            set
            {
                this.ViewState["Language"] = value;
            }
        }

        public bool Localize { get; set; }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void CreateChildControls()
        {
            // First clear the controls collection
            this.Controls.Clear();

            this._Flag = new Image { ViewStateMode = ViewStateMode.Disabled };
            this.Controls.Add(this._Flag);

            this.Controls.Add(new LiteralControl("&nbsp;"));

            this._Label = new Label();
            this._Label.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add(this._Label);

            // Call base class's method
            base.CreateChildControls();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   OnPreRender runs just before the control is rendered.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (string.IsNullOrEmpty(this.Language))
            {
                this._Flag.ImageUrl = "~/images/Flags/none.gif";
            }
            else
            {
                this._Flag.ImageUrl = string.Format("~/images/Flags/{0}.gif", this.Language);
            }

            if (this.DisplayType == 0)
            {
                PortalSettings _PortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                string _ViewTypePersonalizationKey = "ViewType" + _PortalSettings.PortalId;
                string _ViewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", _ViewTypePersonalizationKey));
                switch (_ViewType)
                {
                    case "NATIVE":
                        this.DisplayType = CultureDropDownTypes.NativeName;
                        break;
                    case "ENGLISH":
                        this.DisplayType = CultureDropDownTypes.EnglishName;
                        break;
                    default:
                        this.DisplayType = CultureDropDownTypes.DisplayName;
                        break;
                }
            }

            string localeName = null;
            if (string.IsNullOrEmpty(this.Language))
            {
                localeName = Localization.GetString("NeutralCulture", Localization.GlobalResourceFile);
            }
            else
            {
                localeName = Localization.GetLocaleName(this.Language, this.DisplayType);
            }

            this._Label.Text = localeName;
            this._Flag.AlternateText = localeName;
        }
    }
}
