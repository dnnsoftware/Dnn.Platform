// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using Telerik.Web.UI;

    public class DnnLanguageComboBox : WebControl
    {
        private readonly string _viewTypePersonalizationKey;
        private DnnComboBox _englishCombo;
        private LanguagesListType _languagesListType = LanguagesListType.Enabled;
        private RadioButtonList _modeRadioButtonList;
        private DnnComboBox _nativeCombo;

        private string _originalValue;

        public DnnLanguageComboBox()
        {
            this.AutoPostBack = Null.NullBoolean;
            this.CausesValidation = Null.NullBoolean;
            this.ShowFlag = true;
            this.ShowModeButtons = true;
            this.HideLanguagesList = new Dictionary<string, Locale>();
            this.FlagImageUrlFormatString = "~/images/Flags/{0}.gif";
            this._viewTypePersonalizationKey = "ViewType" + this.PortalId;
        }

        public event EventHandler ItemChanged;

        public event EventHandler ModeChanged;

        public string SelectedValue
        {
            get
            {
                string selectedValue = this.DisplayMode.Equals("NATIVE", StringComparison.InvariantCultureIgnoreCase) ? this._nativeCombo.SelectedValue : this._englishCombo.SelectedValue;
                if (selectedValue == "None")
                {
                    selectedValue = Null.NullString;
                }

                return selectedValue;
            }
        }

        public string FlagImageUrlFormatString { get; set; }

        public Dictionary<string, Locale> HideLanguagesList { get; set; }

        public bool IncludeNoneSpecified { get; set; }

        public LanguagesListType LanguagesListType
        {
            get
            {
                return this._languagesListType;
            }

            set
            {
                this._languagesListType = value;
            }
        }

        public int PortalId { get; set; }

        public bool ShowFlag { get; set; }

        public bool ShowModeButtons { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets or sets a value indicating whether determines whether the List Auto Posts Back.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool AutoPostBack { get; set; }

        public bool CausesValidation { get; set; }

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        private string DisplayMode
        {
            get
            {
                string displayMode = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", this._viewTypePersonalizationKey));
                if (string.IsNullOrEmpty(displayMode))
                {
                    displayMode = "NATIVE";
                }

                return displayMode;
            }
        }

        public void BindData(bool refresh)
        {
            if (refresh)
            {
                List<CultureInfo> cultures;
                switch (this.LanguagesListType)
                {
                    case LanguagesListType.Supported:
                        cultures = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
                        break;
                    case LanguagesListType.Enabled:
                        cultures = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(this.PortalId));
                        break;
                    default:
                        cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
                        break;
                }

                foreach (KeyValuePair<string, Locale> lang in this.HideLanguagesList)
                {
                    string cultureCode = lang.Value.Code;
                    CultureInfo culture = cultures.Where(c => c.Name == cultureCode).SingleOrDefault();
                    if (culture != null)
                    {
                        cultures.Remove(culture);
                    }
                }

                this._nativeCombo.DataSource = cultures.OrderBy(c => c.NativeName);
                this._englishCombo.DataSource = cultures.OrderBy(c => c.EnglishName);
            }

            this._nativeCombo.DataBind();
            this._englishCombo.DataBind();

            if (this.IncludeNoneSpecified && refresh)
            {
                this._englishCombo.Items.Insert(0, new RadComboBoxItem(Localization.GetString("System_Default", Localization.SharedResourceFile), "None"));
                this._nativeCombo.Items.Insert(0, new RadComboBoxItem(Localization.GetString("System_Default", Localization.SharedResourceFile), "None"));
            }
        }

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                this._nativeCombo.SelectedIndex = this._nativeCombo.FindItemIndexByValue("None");
                this._englishCombo.SelectedIndex = this._englishCombo.FindItemIndexByValue("None");
            }
            else
            {
                this._nativeCombo.SelectedIndex = this._nativeCombo.FindItemIndexByValue(code);
                this._englishCombo.SelectedIndex = this._englishCombo.FindItemIndexByValue(code);
            }
        }

        public override void DataBind()
        {
            this.BindData(!this.Page.IsPostBack);
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this._nativeCombo = new DnnComboBox();
            this._nativeCombo.DataValueField = "Name";
            this._nativeCombo.DataTextField = "NativeName";
            this._nativeCombo.SelectedIndexChanged += this.ItemChangedInternal;
            this.Controls.Add(this._nativeCombo);

            this._englishCombo = new DnnComboBox();
            this._englishCombo.DataValueField = "Name";
            this._englishCombo.DataTextField = "EnglishName";
            this._englishCombo.SelectedIndexChanged += this.ItemChangedInternal;
            this.Controls.Add(this._englishCombo);

            this._modeRadioButtonList = new RadioButtonList();
            this._modeRadioButtonList.AutoPostBack = true;
            this._modeRadioButtonList.RepeatDirection = RepeatDirection.Horizontal;
            this._modeRadioButtonList.Items.Add(new ListItem(Localization.GetString("NativeName", Localization.GlobalResourceFile), "NATIVE"));
            this._modeRadioButtonList.Items.Add(new ListItem(Localization.GetString("EnglishName", Localization.GlobalResourceFile), "ENGLISH"));
            this._modeRadioButtonList.SelectedIndexChanged += this.ModeChangedInternal;
            this.Controls.Add(this._modeRadioButtonList);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this._originalValue = this.SelectedValue;
        }

        protected virtual void OnItemChanged()
        {
            if (this.ItemChanged != null)
            {
                this.ItemChanged(this, new EventArgs());
            }
        }

        protected void OnModeChanged(EventArgs e)
        {
            if (this.ModeChanged != null)
            {
                this.ModeChanged(this, e);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            if (this.DisplayMode.Equals("ENGLISH", StringComparison.InvariantCultureIgnoreCase))
            {
                if (this._englishCombo.Items.FindItemByValue(this._originalValue) != null)
                {
                    this._englishCombo.Items.FindItemByValue(this._originalValue).Selected = true;
                }
            }
            else
            {
                if (this._nativeCombo.Items.FindItemByValue(this._originalValue) != null)
                {
                    this._nativeCombo.Items.FindItemByValue(this._originalValue).Selected = true;
                }
            }

            this._modeRadioButtonList.Items.FindByValue(this.DisplayMode).Selected = true;

            foreach (RadComboBoxItem item in this._englishCombo.Items)
            {
                item.ImageUrl = string.Format(this.FlagImageUrlFormatString, item.Value);
            }

            foreach (RadComboBoxItem item in this._nativeCombo.Items)
            {
                item.ImageUrl = string.Format(this.FlagImageUrlFormatString, item.Value);
            }

            this._englishCombo.AutoPostBack = this.AutoPostBack;
            this._englishCombo.CausesValidation = this.CausesValidation;
            this._englishCombo.Visible = this.DisplayMode.Equals("ENGLISH", StringComparison.InvariantCultureIgnoreCase);

            this._nativeCombo.AutoPostBack = this.AutoPostBack;
            this._nativeCombo.CausesValidation = this.CausesValidation;
            this._nativeCombo.Visible = this.DisplayMode.Equals("NATIVE", StringComparison.InvariantCultureIgnoreCase);

            this._modeRadioButtonList.Visible = this.ShowModeButtons;

            this._englishCombo.Width = this.Width;
            this._nativeCombo.Width = this.Width;

            base.OnPreRender(e);
        }

        private void ModeChangedInternal(object sender, EventArgs e)
        {
            Personalization.SetProfile("LanguageDisplayMode", this._viewTypePersonalizationKey, this._modeRadioButtonList.SelectedValue);

            // Resort
            this.BindData(true);

            this.OnModeChanged(EventArgs.Empty);
        }

        private void ItemChangedInternal(object sender, EventArgs e)
        {
            this.OnItemChanged();
        }
    }
}
