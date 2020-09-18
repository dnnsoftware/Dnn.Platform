// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   LocaleSelectorControl is a user control that provides all the server code to manage
    ///   localisation selection.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public abstract class LocaleSelectorControl : UserControlBase
    {
        protected DropDownList ddlPortalDefaultLanguage;
        protected Literal litStatus;
        protected RadioButtonList rbViewType;
        private string MyFileName = "LocaleSelectorControl.ascx";
        private string _ViewType = string.Empty;

        public string CultureCode
        {
            get
            {
                return this.ddlPortalDefaultLanguage.SelectedValue;
            }
        }

        private CultureDropDownTypes DisplayType
        {
            get
            {
                switch (this.ViewType)
                {
                    case "NATIVE":
                        return CultureDropDownTypes.NativeName;
                    case "ENGLISH":
                        return CultureDropDownTypes.EnglishName;
                    default:
                        return CultureDropDownTypes.NativeName;
                }
            }
        }

        private string ViewType
        {
            get
            {
                if (string.IsNullOrEmpty(this._ViewType))
                {
                    this._ViewType = Convert.ToString(Personalization.GetProfile("LanguageEnabler", string.Format("ViewType{0}", this.PortalSettings.PortalId)));
                }

                if (string.IsNullOrEmpty(this._ViewType))
                {
                    this._ViewType = "NATIVE";
                }

                return this._ViewType;
            }
        }

        public void BindDefaultLanguageSelector()
        {
            if (this.Page.IsPostBack == false)
            {
                Localization.LoadCultureDropDownList(this.ddlPortalDefaultLanguage, this.DisplayType, this.PortalSettings.DefaultLanguage, true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.rbViewType.SelectedIndexChanged += this.rbViewType_SelectedIndexChanged;

            if (!this.Page.IsPostBack)
            {
                ListItem item = default(ListItem);

                item = new ListItem(Localization.GetString("NativeName.Text", Localization.GetResourceFile(this, this.MyFileName)), "NATIVE");
                this.rbViewType.Items.Add(item);
                if (this.ViewType == "NATIVE")
                {
                    item.Selected = true;
                }

                item = new ListItem(Localization.GetString("EnglishName.Text", Localization.GetResourceFile(this, this.MyFileName)), "ENGLISH");
                this.rbViewType.Items.Add(item);
                if (this.ViewType == "ENGLISH")
                {
                    item.Selected = true;
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Visible)
            {
                this.BindDefaultLanguageSelector();
            }
        }

        private void rbViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this._ViewType = this.rbViewType.SelectedValue;
        }
    }
}
