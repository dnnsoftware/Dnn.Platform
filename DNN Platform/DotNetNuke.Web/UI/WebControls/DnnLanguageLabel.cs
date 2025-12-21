// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    public class DnnLanguageLabel : CompositeControl, ILocalizable
    {
        private Image flag;

        private Label label;

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

        /// <inheritdoc/>
        public bool Localize { get; set; }

        /// <inheritdoc/>
        public string LocalResourceFile { get; set; }

        /// <inheritdoc/>
        public virtual void LocalizeStrings()
        {
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        /// <summary>
        ///   CreateChildControls overrides the Base class's method to correctly build the
        ///   control based on the configuration.
        /// </summary>
        protected override void CreateChildControls()
        {
            // First clear the controls collection
            this.Controls.Clear();

            this.flag = new Image { ViewStateMode = ViewStateMode.Disabled };
            this.Controls.Add(this.flag);

            this.Controls.Add(new LiteralControl("&nbsp;"));

            this.label = new Label();
            this.label.ViewStateMode = ViewStateMode.Disabled;
            this.Controls.Add(this.label);

            // Call base class's method
            base.CreateChildControls();
        }

        /// <summary>OnPreRender runs just before the control is rendered.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (string.IsNullOrEmpty(this.Language))
            {
                this.flag.ImageUrl = "~/images/Flags/none.gif";
            }
            else
            {
                this.flag.ImageUrl = $"~/images/Flags/{this.Language}.gif";
            }

            if (this.DisplayType == 0)
            {
                PortalSettings portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                string viewTypePersonalizationKey = "ViewType" + portalSettings.PortalId;
                string viewType = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", viewTypePersonalizationKey), CultureInfo.InvariantCulture);
                switch (viewType)
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

            this.label.Text = localeName;
            this.flag.AlternateText = localeName;
        }
    }
}
