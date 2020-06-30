// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Collections;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Icons;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    public partial class Search : SkinObjectBase
    {
        private const string MyFileName = "Search.ascx";

        private readonly INavigationManager _navigationManager;
        private bool _showSite = true;
        private bool _showWeb = true;
        private string _siteIconURL;
        private string _siteText;
        private string _siteToolTip;
        private string _siteURL;
        private string _webIconURL;
        private string _webText;
        private string _webToolTip;
        private string _webURL;

        private bool _enableWildSearch = true;

        public Search()
        {
            this._navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string SeeMoreText
        {
            get
            {
                return Localization.GetSafeJSString("SeeMoreResults", Localization.GetResourceFile(this, MyFileName));
            }
        }

        public string ClearQueryText
        {
            get
            {
                return Localization.GetSafeJSString("SearchClearQuery", Localization.GetResourceFile(this, MyFileName));
            }
        }

        public string NoResultText
        {
            get
            {
                return Localization.GetSafeJSString("NoResult", Localization.GetResourceFile(this, MyFileName));
            }
        }

        public string PlaceHolderText
        {
            get
            {
                return Localization.GetSafeJSString("Placeholder", Localization.GetResourceFile(this, MyFileName));
            }
        }

        /// <summary>
        /// Gets or sets the CSS class for the option buttons and search button.
        /// </summary>
        /// <remarks>If you are using the DropDownList option then you can style the search
        /// elements without requiring a custom CssClass.</remarks>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the visibility setting for the radio button corresponding to site based searchs.
        /// </summary>
        /// <remarks>Set this value to false to hide the "Site" radio button.  This setting has no effect
        /// if UseDropDownList is true.</remarks>
        public bool ShowSite
        {
            get
            {
                return this._showSite;
            }

            set
            {
                this._showSite = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets the visibility setting for the radio button corresponding to web based searchs.
        /// </summary>
        /// <remarks>Set this value to false to hide the "Web" radio button.  This setting has no effect
        /// if UseDropDownList is true.</remarks>
        public bool ShowWeb
        {
            get
            {
                return this._showWeb;
            }

            set
            {
                this._showWeb = value;
            }
        }

        /// <summary>
        /// Gets or sets the site icon URL.
        /// </summary>
        /// <value>The site icon URL.</value>
        /// <remarks>If the SiteIconURL is not set or is an empty string then this will return a site relative URL for the
        /// DnnSearch_16X16_Standard.png image in the images/search subfolder.  SiteIconURL supports using
        /// app relative virtual paths designated by the use of the tilde (~).</remarks>
        public string SiteIconURL
        {
            get
            {
                if (string.IsNullOrEmpty(this._siteIconURL))
                {
                    return IconController.IconURL("DnnSearch");
                }

                return this._siteIconURL;
            }

            set
            {
                this._siteIconURL = value;
            }
        }

        /// <summary>
        /// Gets or sets the text for the "site" radio button or option list item.
        /// </summary>
        /// <value>The site text.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.</remarks>
        public string SiteText
        {
            get
            {
                if (string.IsNullOrEmpty(this._siteText))
                {
                    return Localization.GetString("Site", Localization.GetResourceFile(this, MyFileName));
                }

                return this._siteText;
            }

            set
            {
                this._siteText = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text for the "site" radio button.
        /// </summary>
        /// <value>The site tool tip.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.</remarks>
        public string SiteToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(this._siteToolTip))
                {
                    return Localization.GetString("Site.ToolTip", Localization.GetResourceFile(this, MyFileName));
                }

                return this._siteToolTip;
            }

            set
            {
                this._siteToolTip = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL for doing web based site searches.
        /// </summary>
        /// <value>The site URL.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.
        /// <para>The site URL is a template for an external search engine, which by default, uses Google.com.  The siteURL should
        /// include the tokens [TEXT] and [DOMAIN] to be replaced automatically by the search text and the current site domain.</para></remarks>
        public string SiteURL
        {
            get
            {
                if (string.IsNullOrEmpty(this._siteURL))
                {
                    return Localization.GetString("URL", Localization.GetResourceFile(this, MyFileName));
                }

                return this._siteURL;
            }

            set
            {
                this._siteURL = value;
            }
        }

        /// <summary>
        /// Gets or sets the html for the submit button.
        /// </summary>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.
        /// <para>If you set the value to an hmtl img tag, then the src attribute will be made relative
        /// to the current skinpath.</para></remarks>
        public string Submit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to use the web search engine for site searches.
        /// </summary>
        /// <remarks>Set this value to true to perform a domain limited search using the search engine defined by <see cref="SiteURL">SiteURL</see>.</remarks>
        public bool UseWebForSite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the site/web options using a drop down list.
        /// </summary>
        /// <remarks>If true, then the site and web options are displayed in a drop down list.  If the
        /// drop down list is used, then the <see cref="ShowWeb">ShowWeb</see> and <see cref="ShowSite">ShowSite</see>
        /// properties are not used.</remarks>
        public bool UseDropDownList { get; set; }

        /// <summary>
        /// Gets or sets the web icon URL.
        /// </summary>
        /// <value>The web icon URL.</value>
        /// <remarks>If the WebIconURL is not set or is an empty string then this will return a site relative URL for the
        /// google-icon.gif image in the images/search subfolder.  WebIconURL supports using
        /// app relative virtual paths designated by the use of the tilde (~).</remarks>
        public string WebIconURL
        {
            get
            {
                if (string.IsNullOrEmpty(this._webIconURL))
                {
                    return IconController.IconURL("GoogleSearch");
                }

                return this._webIconURL;
            }

            set
            {
                this._webIconURL = value;
            }
        }

        /// <summary>
        /// Gets or sets the text for the "web" radio button or option list item.
        /// </summary>
        /// <value>The web text.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.</remarks>
        public string WebText
        {
            get
            {
                if (string.IsNullOrEmpty(this._webText))
                {
                    return Localization.GetString("Web", Localization.GetResourceFile(this, MyFileName));
                }

                return this._webText;
            }

            set
            {
                this._webText = value;
            }
        }

        /// <summary>
        /// Gets or sets the tooltip text for the "web" radio button.
        /// </summary>
        /// <value>The web tool tip.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.</remarks>
        public string WebToolTip
        {
            get
            {
                if (string.IsNullOrEmpty(this._webToolTip))
                {
                    return Localization.GetString("Web.ToolTip", Localization.GetResourceFile(this, MyFileName));
                }

                return this._webToolTip;
            }

            set
            {
                this._webToolTip = value;
            }
        }

        /// <summary>
        /// Gets or sets the URL for doing web based searches.
        /// </summary>
        /// <value>The web URL.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.
        /// <para>The web URL is a template for an external search engine, which by default, uses Google.com.  The WebURL should
        /// include the token [TEXT] to be replaced automatically by the search text.  The [DOMAIN] token, if present, will be
        /// replaced by an empty string.</para></remarks>
        public string WebURL
        {
            get
            {
                if (string.IsNullOrEmpty(this._webURL))
                {
                    return Localization.GetString("URL", Localization.GetResourceFile(this, MyFileName));
                }

                return this._webURL;
            }

            set
            {
                this._webURL = value;
            }
        }

        /// <summary>
        /// Gets or sets minium chars required to trigger auto search.
        /// </summary>
        public int MinCharRequired { get; set; }

        /// <summary>
        /// Gets or sets the millisecond to delay trigger auto search.
        /// </summary>
        public int AutoSearchDelayInMilliSecond { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether disable the wild search.
        /// </summary>
        public bool EnableWildSearch
        {
            get { return this._enableWildSearch; }
            set { this._enableWildSearch = value; }
        }

        protected int PortalId { get; set; }

        protected string SearchType { get; set; }

        protected string CultureCode { get; set; }

        /// <summary>
        ///   Executes the search.
        /// </summary>
        /// <param name = "searchText">The text which will be used to perform the search.</param>
        /// <param name = "searchType">The type of the search. Use "S" for a site search, and "W" for a web search.</param>
        /// <remarks>
        ///   All web based searches will open in a new window, while site searches will open in the current window.  A site search uses the built
        ///   in search engine to perform the search, while both web based search variants will use an external search engine to perform a search.
        /// </remarks>
        protected void ExecuteSearch(string searchText, string searchType)
        {
            int searchTabId = this.GetSearchTabId();

            if (searchTabId == Null.NullInteger)
            {
                return;
            }

            string strURL;
            if (!string.IsNullOrEmpty(searchText))
            {
                switch (searchType)
                {
                    case "S":
                        // site
                        if (this.UseWebForSite)
                        {
                            strURL = this.SiteURL;
                            if (!string.IsNullOrEmpty(strURL))
                            {
                                strURL = strURL.Replace("[TEXT]", this.Server.UrlEncode(searchText));
                                strURL = strURL.Replace("[DOMAIN]", this.Request.Url.Host);
                                UrlUtils.OpenNewWindow(this.Page, this.GetType(), strURL);
                            }
                        }
                        else
                        {
                            if (Host.UseFriendlyUrls)
                            {
                                this.Response.Redirect(this._navigationManager.NavigateURL(searchTabId) + "?Search=" + this.Server.UrlEncode(searchText));
                            }
                            else
                            {
                                this.Response.Redirect(this._navigationManager.NavigateURL(searchTabId) + "&Search=" + this.Server.UrlEncode(searchText));
                            }
                        }

                        break;
                    case "W":
                        // web
                        strURL = this.WebURL;
                        if (!string.IsNullOrEmpty(strURL))
                        {
                            strURL = strURL.Replace("[TEXT]", this.Server.UrlEncode(searchText));
                            strURL = strURL.Replace("[DOMAIN]", string.Empty);
                            UrlUtils.OpenNewWindow(this.Page, this.GetType(), strURL);
                        }

                        break;
                }
            }
            else
            {
                if (Host.UseFriendlyUrls)
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL(searchTabId));
                }
                else
                {
                    this.Response.Redirect(this._navigationManager.NavigateURL(searchTabId));
                }
            }
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Search/SearchSkinObjectPreview.css", FileOrder.Css.ModuleCss);
            ClientResourceManager.RegisterScript(this.Page, "~/Resources/Search/SearchSkinObjectPreview.js");

            this.cmdSearch.Click += this.CmdSearchClick;
            this.cmdSearchNew.Click += this.CmdSearchNewClick;

            if (this.MinCharRequired == 0)
            {
                this.MinCharRequired = 2;
            }

            if (this.AutoSearchDelayInMilliSecond == 0)
            {
                this.AutoSearchDelayInMilliSecond = 400;
            }

            this.PortalId = this.PortalSettings.ActiveTab.IsSuperTab ? this.PortalSettings.PortalId : -1;

            if (!string.IsNullOrEmpty(this.Submit))
            {
                if (this.Submit.IndexOf("src=", StringComparison.Ordinal) != -1)
                {
                    this.Submit = this.Submit.Replace("src=\"", "src=\"" + this.PortalSettings.ActiveTab.SkinPath);
                    this.Submit = this.Submit.Replace("src='", "src='" + this.PortalSettings.ActiveTab.SkinPath);
                }
            }
            else
            {
                this.Submit = Localization.GetString("Search", Localization.GetResourceFile(this, MyFileName));
            }

            this.cmdSearch.Text = this.Submit;
            this.cmdSearchNew.Text = this.Submit;
            if (!string.IsNullOrEmpty(this.CssClass))
            {
                this.WebRadioButton.CssClass = this.CssClass;
                this.SiteRadioButton.CssClass = this.CssClass;
                this.cmdSearch.CssClass = this.CssClass;
                this.cmdSearchNew.CssClass = this.CssClass;
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdSearchNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event is only used when <see cref="UseDropDownList">UseDropDownList</see> is true.</remarks>
        protected void CmdSearchNewClick(object sender, EventArgs e)
        {
            this.SearchType = ClientAPI.GetClientVariable(this.Page, "SearchIconSelected");
            this.ExecuteSearch(this.txtSearchNew.Text.Trim(), this.SearchType);
        }

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event performs final initialization tasks for the search object UI.</remarks>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.ClassicSearch.Visible = !this.UseDropDownList;
            this.DropDownSearch.Visible = this.UseDropDownList;
            this.CultureCode = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();

            if (this.UseDropDownList)
            {
                // Client Variables will survive a postback so there is no reason to register them.
                if (!this.Page.IsPostBack)
                {
                    this.downArrow.AlternateText = Localization.GetString("DropDownGlyph.AltText", Localization.GetResourceFile(this, MyFileName));
                    this.downArrow.ToolTip = this.downArrow.AlternateText;

                    ClientAPI.RegisterClientVariable(this.Page, "SearchIconWebUrl", string.Format("url({0})", this.ResolveUrl(this.WebIconURL)), true);
                    ClientAPI.RegisterClientVariable(this.Page, "SearchIconSiteUrl", string.Format("url({0})", this.ResolveUrl(this.SiteIconURL)), true);

                    // We are going to use a dnn client variable to store which search option (web/site) is selected.
                    ClientAPI.RegisterClientVariable(this.Page, "SearchIconSelected", "S", true);
                    this.SearchType = "S";
                }

                JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
                ClientResourceManager.RegisterScript(this.Page, "~/Resources/Search/Search.js", FileOrder.Js.DefaultPriority, "DnnFormBottomProvider");

                this.txtSearchNew.Attributes.Add("autocomplete", "off");
                this.txtSearchNew.Attributes.Add("placeholder", this.PlaceHolderText);
            }
            else
            {
                this.WebRadioButton.Visible = this.ShowWeb;
                this.SiteRadioButton.Visible = this.ShowSite;

                if (this.WebRadioButton.Visible)
                {
                    this.WebRadioButton.Checked = true;
                    this.WebRadioButton.Text = this.WebText;
                    this.WebRadioButton.ToolTip = this.WebToolTip;
                }

                if (this.SiteRadioButton.Visible)
                {
                    this.SiteRadioButton.Checked = true;
                    this.SiteRadioButton.Text = this.SiteText;
                    this.SiteRadioButton.ToolTip = this.SiteToolTip;
                }

                this.SearchType = "S";
                this.txtSearch.Attributes.Add("autocomplete", "off");
                this.txtSearch.Attributes.Add("placeholder", this.PlaceHolderText);
            }
        }

        private int GetSearchTabId()
        {
            int searchTabId = this.PortalSettings.SearchTabId;
            if (searchTabId == Null.NullInteger)
            {
                ArrayList arrModules = ModuleController.Instance.GetModulesByDefinition(this.PortalSettings.PortalId, "Search Results");
                if (arrModules.Count > 1)
                {
                    foreach (ModuleInfo SearchModule in arrModules)
                    {
                        if (SearchModule.CultureCode == this.PortalSettings.CultureCode)
                        {
                            searchTabId = SearchModule.TabID;
                        }
                    }
                }
                else if (arrModules.Count == 1)
                {
                    searchTabId = ((ModuleInfo)arrModules[0]).TabID;
                }
            }

            return searchTabId;
        }

        /// <summary>
        /// Handles the Click event of the cmdSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event is only used when <see cref="UseDropDownList">UseDropDownList</see> is false.</remarks>
        private void CmdSearchClick(object sender, EventArgs e)
        {
            this.SearchType = "S";
            if (this.WebRadioButton.Visible)
            {
                if (this.WebRadioButton.Checked)
                {
                    this.SearchType = "W";
                }
            }

            this.ExecuteSearch(this.txtSearch.Text.Trim(), this.SearchType);
        }
    }
}
