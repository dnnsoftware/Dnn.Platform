// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    /// <summary>
    ///   LocaleSelectorControl is a user control that provides all the server code to manage
    ///   localisation selection.
    /// </summary>
    public abstract class LocaleSelectorControl : UserControlBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected DropDownList ddlPortalDefaultLanguage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected Literal litStatus;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RadioButtonList rbViewType;
        private string myFileName = "LocaleSelectorControl.ascx";
        private string viewType = string.Empty;

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
                if (string.IsNullOrEmpty(this.viewType))
                {
                    this.viewType = Convert.ToString(Personalization.GetProfile("LanguageEnabler", string.Format(CultureInfo.InvariantCulture, "ViewType{0}", this.PortalSettings.PortalId)), CultureInfo.InvariantCulture);
                }

                if (string.IsNullOrEmpty(this.viewType))
                {
                    this.viewType = "NATIVE";
                }

                return this.viewType;
            }
        }

        public void BindDefaultLanguageSelector()
        {
            if (this.Page.IsPostBack == false)
            {
                Localization.LoadCultureDropDownList(this.ddlPortalDefaultLanguage, this.DisplayType, this.PortalSettings.DefaultLanguage, true);
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.rbViewType.SelectedIndexChanged += this.RbViewType_SelectedIndexChanged;

            if (!this.Page.IsPostBack)
            {
                ListItem item = default(ListItem);

                item = new ListItem(Localization.GetString("NativeName.Text", Localization.GetResourceFile(this, this.myFileName)), "NATIVE");
                this.rbViewType.Items.Add(item);
                if (this.ViewType == "NATIVE")
                {
                    item.Selected = true;
                }

                item = new ListItem(Localization.GetString("EnglishName.Text", Localization.GetResourceFile(this, this.myFileName)), "ENGLISH");
                this.rbViewType.Items.Add(item);
                if (this.ViewType == "ENGLISH")
                {
                    item.Selected = true;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (this.Visible)
            {
                this.BindDefaultLanguageSelector();
            }
        }

        private void RbViewType_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.viewType = this.rbViewType.SelectedValue;
        }
    }
}
