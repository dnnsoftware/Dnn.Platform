// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Language SkinObject allows the visitor to select the language of the page.</summary>
    public partial class Language : SkinObjectBase
    {
        private const string MyFileName = "Language.ascx";
        private readonly IPortalController portalController;
        private string selectedItemTemplate;
        private string alternateTemplate;
        private string commonFooterTemplate;
        private string commonHeaderTemplate;
        private string footerTemplate;
        private string headerTemplate;
        private string itemTemplate;
        private string localResourceFile;
        private LanguageTokenReplace localTokenReplace;
        private string separatorTemplate;
        private bool showMenu = true;

        /// <summary>Initializes a new instance of the <see cref="Language"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public Language()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="Language"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        public Language(IPortalController portalController)
        {
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        }

        public string AlternateTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.alternateTemplate))
                {
                    this.alternateTemplate = Localization.GetString("AlternateTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.alternateTemplate;
            }

            set
            {
                this.alternateTemplate = value;
            }
        }

        public string CommonFooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.commonFooterTemplate))
                {
                    this.commonFooterTemplate = Localization.GetString("CommonFooterTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.commonFooterTemplate;
            }

            set
            {
                this.commonFooterTemplate = value;
            }
        }

        public string CommonHeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.commonHeaderTemplate))
                {
                    this.commonHeaderTemplate = Localization.GetString("CommonHeaderTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.commonHeaderTemplate;
            }

            set
            {
                this.commonHeaderTemplate = value;
            }
        }

        public string CssClass { get; set; }

        public string FooterTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.footerTemplate))
                {
                    this.footerTemplate = Localization.GetString("FooterTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.footerTemplate;
            }

            set
            {
                this.footerTemplate = value;
            }
        }

        public string HeaderTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.headerTemplate))
                {
                    this.headerTemplate = Localization.GetString("HeaderTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.headerTemplate;
            }

            set
            {
                this.headerTemplate = value;
            }
        }

        public string ItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.itemTemplate))
                {
                    this.itemTemplate = Localization.GetString("ItemTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.itemTemplate;
            }

            set
            {
                this.itemTemplate = value;
            }
        }

        public string SelectedItemTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.selectedItemTemplate))
                {
                    this.selectedItemTemplate = Localization.GetString("SelectedItemTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.selectedItemTemplate;
            }

            set
            {
                this.selectedItemTemplate = value;
            }
        }

        public string SeparatorTemplate
        {
            get
            {
                if (string.IsNullOrEmpty(this.separatorTemplate))
                {
                    this.separatorTemplate = Localization.GetString("SeparatorTemplate.Default", this.LocalResourceFile, this.TemplateCulture);
                }

                return this.separatorTemplate;
            }

            set
            {
                this.separatorTemplate = value;
            }
        }

        public bool ShowLinks { get; set; }

        public bool ShowMenu
        {
            get
            {
                if ((this.showMenu == false) && (this.ShowLinks == false))
                {
                    // this is to make sure that at least one type of selector will be visible if multiple languages are enabled
                    this.showMenu = true;
                }

                return this.showMenu;
            }

            set
            {
                this.showMenu = value;
            }
        }

        public bool UseCurrentCultureForTemplate { get; set; }

        protected string CurrentCulture => CultureInfo.CurrentCulture.ToString();

        protected string TemplateCulture => this.UseCurrentCultureForTemplate ? this.CurrentCulture : "en-US";

        protected string LocalResourceFile
        {
            get
            {
                if (string.IsNullOrEmpty(this.localResourceFile))
                {
                    this.localResourceFile = Localization.GetResourceFile(this, MyFileName);
                }

                return this.localResourceFile;
            }
        }

        protected LanguageTokenReplace LocalTokenReplace
        {
            get
            {
                if (this.localTokenReplace == null)
                {
                    this.localTokenReplace = new LanguageTokenReplace { resourceFile = this.LocalResourceFile };
                }

                return this.localTokenReplace;
            }
        }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.selectCulture.SelectedIndexChanged += this.SelectCulture_SelectedIndexChanged;
            this.rptLanguages.ItemDataBound += this.rptLanguages_ItemDataBound;

            try
            {
                var locales = new Dictionary<string, Locale>();
                IEnumerable<ListItem> cultureListItems = Localization.LoadCultureInListItems(CultureDropDownTypes.NativeName, this.CurrentCulture, string.Empty, false);
                foreach (Locale loc in LocaleController.Instance.GetLocales(this.PortalSettings.PortalId).Values)
                {
                    string defaultRoles = PortalController.GetPortalSetting(this.portalController, $"DefaultTranslatorRoles-{loc.Code}", this.PortalSettings.PortalId, "Administrators");
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

                this.HandleCommonTemplates();
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, this.Request.RawUrl);
            }
        }

        /// <summary>Binds data to repeater. a template is used to render the items.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]

        // ReSharper disable once InconsistentNaming
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

                                litTemplate.Text = this.ParseTemplate(strTemplate, locale.Code);
                            }
                        }
                        else
                        {
                            // for non data items use page culture
                            litTemplate.Text = this.ParseTemplate(strTemplate, this.CurrentCulture);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.ProcessPageLoadException(ex, this.Request.RawUrl);
            }
        }

        private string ParseTemplate(string template, string locale)
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

        private void HandleCommonTemplates()
        {
            if (string.IsNullOrEmpty(this.CommonHeaderTemplate))
            {
                this.litCommonHeaderTemplate.Visible = false;
            }
            else
            {
                this.litCommonHeaderTemplate.Text = this.ParseTemplate(this.CommonHeaderTemplate, this.CurrentCulture);
            }

            if (string.IsNullOrEmpty(this.CommonFooterTemplate))
            {
                this.litCommonFooterTemplate.Visible = false;
            }
            else
            {
                this.litCommonFooterTemplate.Text = this.ParseTemplate(this.CommonFooterTemplate, this.CurrentCulture);
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

        private void SelectCulture_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Redirect to same page to update all controls for newly selected culture
            this.LocalTokenReplace.Language = this.selectCulture.SelectedItem.Value;

            // DNN-6170 ensure skin value is culture specific in case of  static localization
            DataCache.RemoveCache(string.Format(DataCache.PortalSettingsCacheKey, this.PortalSettings.PortalId, Null.NullString));
            this.Response.Redirect(this.LocalTokenReplace.ReplaceEnvironmentTokens("[URL]"));
        }
    }
}
