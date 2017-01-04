#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;



#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnLanguageComboBox : WebControl
    {
        private readonly string _viewTypePersonalizationKey;
        private DnnComboBox _englishCombo;
        private LanguagesListType _languagesListType = LanguagesListType.Enabled;
        private RadioButtonList _modeRadioButtonList;
        private DnnComboBox _nativeCombo;

        private string _originalValue;

        protected override HtmlTextWriterTag TagKey
        {
            get
            {
                return HtmlTextWriterTag.Div;
            }
        }

        #region "Public Events"

        public event EventHandler ItemChanged;
        public event EventHandler ModeChanged;

        #endregion

        #region "Constructor"

        public DnnLanguageComboBox()
        {
            AutoPostBack = Null.NullBoolean;
            CausesValidation = Null.NullBoolean;
            ShowFlag = true;
        	ShowModeButtons = true;
            HideLanguagesList = new Dictionary<string, Locale>();
            FlagImageUrlFormatString = "~/images/Flags/{0}.gif";
            _viewTypePersonalizationKey = "ViewType" + PortalId;
        }

        #endregion

        #region "Public Properties"

        private string DisplayMode
        {
            get
            {
                string displayMode = Convert.ToString(Personalization.GetProfile("LanguageDisplayMode", _viewTypePersonalizationKey));
                if (string.IsNullOrEmpty(displayMode))
                {
                    displayMode = "NATIVE";
                }
                return displayMode;
            }
        }

        public string FlagImageUrlFormatString { get; set; }

        public Dictionary<string, Locale> HideLanguagesList { get; set; }

        public bool IncludeNoneSpecified { get; set; }

        public LanguagesListType LanguagesListType
        {
            get
            {
                return _languagesListType;
            }
            set
            {
                _languagesListType = value;
            }
        }

        public int PortalId { get; set; }

        public string SelectedValue
        {
            get
            {
                string selectedValue = DisplayMode.ToUpperInvariant() == "NATIVE" ? _nativeCombo.SelectedValue : _englishCombo.SelectedValue;
                if (selectedValue == "None")
                {
                    selectedValue = Null.NullString;
                }
                return selectedValue;
            }
        }

        public bool ShowFlag { get; set; }

        public bool ShowModeButtons { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Determines whether the List Auto Posts Back
        /// </summary>
        /// -----------------------------------------------------------------------------
        public bool AutoPostBack { get; set; }

        public bool CausesValidation { get; set; }

        #endregion

        #region "Private Methods"

        public void BindData(bool refresh)
        {
            if (refresh)
            {
                List<CultureInfo> cultures;
                switch (LanguagesListType)
                {
                    case LanguagesListType.Supported:
                        cultures = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(Null.NullInteger));
                        break;
                    case LanguagesListType.Enabled:
                        cultures = LocaleController.Instance.GetCultures(LocaleController.Instance.GetLocales(PortalId));
                        break;
                    default:
                        cultures = new List<CultureInfo>(CultureInfo.GetCultures(CultureTypes.SpecificCultures));
                        break;
                }

                foreach (KeyValuePair<string, Locale> lang in HideLanguagesList)
                {
                    string cultureCode = lang.Value.Code;
                    CultureInfo culture = cultures.Where(c => c.Name == cultureCode).SingleOrDefault();
                    if (culture != null)
                    {
                        cultures.Remove(culture);
                    }
                }

                _nativeCombo.DataSource = cultures.OrderBy(c => c.NativeName);
                _englishCombo.DataSource = cultures.OrderBy(c => c.EnglishName);
            }


            _nativeCombo.DataBind();
            _englishCombo.DataBind();

            if (IncludeNoneSpecified && refresh)
            {
                _englishCombo.Items.Insert(0, new ListItem(Localization.GetString("System_Default", Localization.SharedResourceFile), "None"));
                _nativeCombo.Items.Insert(0, new ListItem(Localization.GetString("System_Default", Localization.SharedResourceFile), "None"));
            }
        }

        #endregion

        #region "Protected Methods"

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            _nativeCombo = new DnnComboBox();
            _nativeCombo.DataValueField = "Name";
            _nativeCombo.DataTextField = "NativeName";
            _nativeCombo.SelectedIndexChanged += ItemChangedInternal;
            Controls.Add(_nativeCombo);

            _englishCombo = new DnnComboBox();
            _englishCombo.DataValueField = "Name";
            _englishCombo.DataTextField = "EnglishName";
            _englishCombo.SelectedIndexChanged += ItemChangedInternal;
            Controls.Add(_englishCombo);

            _modeRadioButtonList = new RadioButtonList();
            _modeRadioButtonList.AutoPostBack = true;
            _modeRadioButtonList.RepeatDirection = RepeatDirection.Horizontal;
            _modeRadioButtonList.Items.Add(new ListItem(Localization.GetString("NativeName", Localization.GlobalResourceFile), "NATIVE"));
            _modeRadioButtonList.Items.Add(new ListItem(Localization.GetString("EnglishName", Localization.GlobalResourceFile), "ENGLISH"));
            _modeRadioButtonList.SelectedIndexChanged += ModeChangedInternal;
            Controls.Add(_modeRadioButtonList);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _originalValue = SelectedValue;
        }

        protected virtual void OnItemChanged()
        {
            if (ItemChanged != null)
            {
                ItemChanged(this, new EventArgs());
            }
        }

        protected void OnModeChanged(EventArgs e)
        {
            if (ModeChanged != null)
            {
                ModeChanged(this, e);
            }
        }


        protected override void OnPreRender(EventArgs e)
        {
            if (DisplayMode.ToUpperInvariant() == "ENGLISH")
            {
                if (_englishCombo.FindItemByValue(_originalValue) != null)
                {
                    _englishCombo.FindItemByValue(_originalValue).Selected = true;
                }
            }
            else
            {
                if (_nativeCombo.FindItemByValue(_originalValue) != null)
                {
                    _nativeCombo.FindItemByValue(_originalValue).Selected = true;
                }
            }

            _modeRadioButtonList.Items.FindByValue(DisplayMode).Selected = true;

            //foreach (var item in _englishCombo.Items)
            //{
            //    item.ImageUrl = string.Format(FlagImageUrlFormatString, item.Value);
            //}
            //foreach (RadComboBoxItem item in _nativeCombo.Items)
            //{
            //    item.ImageUrl = string.Format(FlagImageUrlFormatString, item.Value);
            //}

            _englishCombo.AutoPostBack = AutoPostBack;
            _englishCombo.CausesValidation = CausesValidation;
            _englishCombo.Visible = (DisplayMode.ToUpperInvariant() == "ENGLISH");

            _nativeCombo.AutoPostBack = AutoPostBack;
            _nativeCombo.CausesValidation = CausesValidation;
            _nativeCombo.Visible = (DisplayMode.ToUpperInvariant() == "NATIVE");

            _modeRadioButtonList.Visible = ShowModeButtons;

            _englishCombo.Width = Width;
            _nativeCombo.Width = Width;

            base.OnPreRender(e);
        }

        #endregion

        #region "Public Methods"

        public void SetLanguage(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                _nativeCombo.SelectedIndex = _nativeCombo.FindItemIndexByValue("None");
                _englishCombo.SelectedIndex = _englishCombo.FindItemIndexByValue("None");
            }
            else
            {
                _nativeCombo.SelectedIndex = _nativeCombo.FindItemIndexByValue(code);
                _englishCombo.SelectedIndex = _englishCombo.FindItemIndexByValue(code);
            }
        }

        public override void DataBind()
        {
            BindData(!Page.IsPostBack);
        }

        #endregion

        #region "Event Handlers"

        private void ModeChangedInternal(object sender, EventArgs e)
        {
            Personalization.SetProfile("LanguageDisplayMode", _viewTypePersonalizationKey, _modeRadioButtonList.SelectedValue);

            //Resort
            BindData(true);

            OnModeChanged(EventArgs.Empty);
        }

        private void ItemChangedInternal(object sender, EventArgs e)
        {
            OnItemChanged();
        }

        #endregion
    }
}