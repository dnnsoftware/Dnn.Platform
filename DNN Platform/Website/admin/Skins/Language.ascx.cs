// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// The Language skinobject allows the visitor to select the language of the page.
    /// </summary>
    /// <remarks></remarks>
    public partial class Language : SkinObjectBase
    {
        private const string MyFileName = "Language.ascx";
        private string _SelectedItemTemplate;
        private string _alternateTemplate;
        private string _commonFooterTemplate;
        private string _commonHeaderTemplate;
        private string _footerTemplate;
        private string _headerTemplate;
        private string _itemTemplate;
        private string _localResourceFile;
        private LanguageTokenReplace _localTokenReplace;
        private string _separatorTemplate;
        private bool _showMenu = true;

        public string AlternateTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._alternateTemplate))
                {
                    this._alternateTemplate = Localization.GetString("AlternateTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._alternateTemplate;
            }

            set
            {
                this._alternateTemplate = value;
            }
        }

        public string CommonFooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._commonFooterTemplate))
                {
                    this._commonFooterTemplate = Localization.GetString("CommonFooterTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._commonFooterTemplate;
            }

            set
            {
                this._commonFooterTemplate = value;
            }
        }

        public string CommonHeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._commonHeaderTemplate))
                {
                    this._commonHeaderTemplate = Localization.GetString("CommonHeaderTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._commonHeaderTemplate;
            }

            set
            {
                this._commonHeaderTemplate = value;
            }
        }

        public string CssClass { get; set; }

        public string FooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._footerTemplate))
                {
                    this._footerTemplate = Localization.GetString("FooterTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._footerTemplate;
            }

            set
            {
                this._footerTemplate = value;
            }
        }

        public string HeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._headerTemplate))
                {
                    this._headerTemplate = Localization.GetString("HeaderTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._headerTemplate;
            }

            set
            {
                this._headerTemplate = value;
            }
        }

        public string ItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._itemTemplate))
                {
                    this._itemTemplate = Localization.GetString("ItemTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._itemTemplate;
            }

            set
            {
                this._itemTemplate = value;
            }
        }

        public string SelectedItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._SelectedItemTemplate))
                {
                    this._SelectedItemTemplate = Localization.GetString("SelectedItemTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._SelectedItemTemplate;
            }

            set
            {
                this._SelectedItemTemplate = value;
            }
        }

        public string SeparatorTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this._separatorTemplate))
                {
                    this._separatorTemplate = Localization.GetString("SeparatorTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this._separatorTemplate;
            }

            set
            {
                this._separatorTemplate = value;
            }
        }

        public bool ShowLinks { get; set; }

        public bool ShowMenu
        {
            get
            {
                if ((this._showMenu == false) && (this.ShowLinks == false))
                {
                    // this is to make sure that at least one type of selector will be visible if multiple languages are enabled
                    this._showMenu = true;
                }

                return this._showMenu;
            }

            set
            {
                this._showMenu = value;
            }
        }

        public bool UseCurrentCultureForTemplate { get; set; }

        protected string CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture.ToString();
            }
        }

        protected string TemplateCulture
        {
            get
            {
                return this.UseCurrentCultureForTemplate ? this.CurrentCulture : "en-US";
            }
        }

        protected string LocalResourceFile
        {
            get
            {
                if (string.IsNullOrEmpty(this._localResourceFile))
                {
                    this._localResourceFile = Localization.GetResourceFile(this, MyFileName);
                }

                return this._localResourceFile;
            }
        }

        protected LanguageTokenReplace LocalTokenReplace
        {
            get
            {
                if (this._localTokenReplace == null)
                {
                    this._localTokenReplace = new LanguageTokenReplace { resourceFile = this.LocalResourceFile };
                }

                return this._localTokenReplace;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.selectCulture.SelectedIndexChanged += this.selectCulture_SelectedIndexChanged;
            this.rptLanguages.ItemDataBound += this.rptLanguages_ItemDataBound;

            try
            {
                var locales = new Dictionary<string, Locale>();
                IEnumerable<ListItem> cultureListItems = DotNetNuke.Services.Localization.Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, this.CurrentCulture, string.Empty, false);
                foreach (Locale loc in LocaleController.Instance.GetLocales(this.PortalSettings.PortalId).Values)
                {
                    string defaultRoles = PortalController.GetPortalSetting(string.Format("DefaultTranslatorRoles-{0}", loc.Code), this.PortalSettings.PortalId, "Administrators");
                    if (!this.PortalSettings.ContentLocalizationEnabled ||
                        (this.LocaleIsAvailable(loc) &&
                            (PortalSecurity.IsInRoles(this.PortalSettings.AdministratorRoleName) || loc.IsPublished || PortalSecurity.IsInRoles(defaultRoles))))
                    {
                        locales.Add(loc.Code, loc);
                        foreach (var cultureItem in cultureListItems)
                        {
                            if (cultureItem.Value == loc.Code)
                            {
                                this.selectCulture.Items.Add(cultureItem);
                            }
                        }
                    }
                }

                if (this.ShowLinks)
                {
                    if (locales.Count > 1)
                    {
                        this.rptLanguages.DataSource = locales.Values;
                        this.rptLanguages.DataBind();
                    }
                    else
                    {
                        this.rptLanguages.Visible = false;
                    }
                }

                if (this.ShowMenu)
                {
                    if (!string.IsNullOrEmpty(this.CssClass))
                    {
                        this.selectCulture.CssClass = this.CssClass;
                    }

                    if (!this.IsPostBack)
                    {
                        // select the default item
                        if (this.CurrentCulture != null)
                        {
                            ListItem item = this.selectCulture.Items.FindByValue(this.CurrentCulture);
                            if (item != null)
                            {
                                this.selectCulture.SelectedIndex = -1;
                                item.Selected = true;
                            }
                        }
                    }

                    // only show language selector if more than one language
                    if (this.selectCulture.Items.Count <= 1)
                    {
                        this.selectCulture.Visible = false;
                    }
                }
                else
                {
                    this.selectCulture.Visible = false;
                }

                this.handleCommonTemplates();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, this.Request.RawUrl);
            }
        }

        /// <summary>
        /// Binds data to repeater. a template is used to render the items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void rptLanguages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                var litTemplate = e.Item.FindControl("litItemTemplate") as Literal;
                if (litTemplate != null)
                {
                    // load proper template for this Item
                    string strTemplate = string.Empty;
                    switch (e.Item.ItemType)
                    {
                        case ListItemType.Item:
                            strTemplate = this.ItemTemplate;
                            break;
                        case ListItemType.AlternatingItem:
                            if (!string.IsNullOrEmpty(this.AlternateTemplate))
                            {
                                strTemplate = this.AlternateTemplate;
                            }
                            else
                            {
                                strTemplate = this.ItemTemplate;
                            }

                            break;
                        case ListItemType.Header:
                            strTemplate = this.HeaderTemplate;
                            break;
                        case ListItemType.Footer:
                            strTemplate = this.FooterTemplate;
                            break;
                        case ListItemType.Separator:
                            strTemplate = this.SeparatorTemplate;
                            break;
                    }

                    if (string.IsNullOrEmpty(strTemplate))
                    {
                        litTemplate.Visible = false;
                    }
                    else
                    {
                        if (e.Item.DataItem != null)
                        {
                            var locale = e.Item.DataItem as Locale;
                            if (locale != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
                            {
                                if (locale.Code == this.CurrentCulture && !string.IsNullOrEmpty(this.SelectedItemTemplate))
                                {
                                    strTemplate = this.SelectedItemTemplate;
                                }

                                litTemplate.Text = this.parseTemplate(strTemplate, locale.Code);
                            }
                        }
                        else
                        {
                            // for non data items use page culture
                            litTemplate.Text = this.parseTemplate(strTemplate, this.CurrentCulture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, this.Request.RawUrl);
            }
        }

        private string parseTemplate(string template, string locale)
        {
            string strReturnValue = template;
            try
            {
                if (!string.IsNullOrEmpty(locale))
                {
                    // for non data items use locale
                    this.LocalTokenReplace.Language = locale;
                }
                else
                {
                    // for non data items use page culture
                    this.LocalTokenReplace.Language = this.CurrentCulture;
                }

                // perform token replacements
                strReturnValue = this.LocalTokenReplace.ReplaceEnvironmentTokens(strReturnValue);
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, this.Request.RawUrl);
            }

            return strReturnValue;
        }

        private void handleCommonTemplates()
        {
            if (string.IsNullOrEmpty(this.CommonHeaderTemplate))
            {
                this.litCommonHeaderTemplate.Visible = false;
            }
            else
            {
                this.litCommonHeaderTemplate.Text = this.parseTemplate(this.CommonHeaderTemplate, this.CurrentCulture);
            }

            if (string.IsNullOrEmpty(this.CommonFooterTemplate))
            {
                this.litCommonFooterTemplate.Visible = false;
            }
            else
            {
                this.litCommonFooterTemplate.Text = this.parseTemplate(this.CommonFooterTemplate, this.CurrentCulture);
            }
        }

        private bool LocaleIsAvailable(Locale locale)
        {
            var tab = this.PortalSettings.ActiveTab;
            if (tab.DefaultLanguageTab != null)
            {
                tab = tab.DefaultLanguageTab;
            }

            var localizedTab = TabController.Instance.GetTabByCulture(tab.TabID, tab.PortalID, locale);

            return localizedTab != null && !localizedTab.IsDeleted && TabPermissionController.CanViewPage(localizedTab);
        }

        private void selectCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Redirect to same page to update all controls for newly selected culture
            this.LocalTokenReplace.Language = this.selectCulture.SelectedItem.Value;

            // DNN-6170 ensure skin value is culture specific in case of  static localization
            DataCache.RemoveCache(string.Format(DataCache.PortalSettingsCacheKey, this.PortalSettings.PortalId, Null.NullString));
            this.Response.Redirect(this.LocalTokenReplace.ReplaceEnvironmentTokens("[URL]"));
        }
    }
}
