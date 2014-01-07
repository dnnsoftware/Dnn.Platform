#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Collections;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client.ClientResourceManagement;

using Globals = DotNetNuke.Common.Globals;
using DotNetNuke.Entities.Icons;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    using Web.Client;

    public partial class Search : SkinObjectBase
    {
        #region Private Members

        private const string MyFileName = "Search.ascx";
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

        #endregion

        #region Public Members

        /// <summary>
        /// Gets or sets the CSS class for the option buttons and search button
        /// </summary>
        /// <remarks>If you are using the DropDownList option then you can style the search
        /// elements without requiring a custom CssClass.</remarks>
        public string CssClass { get; set; }

        /// <summary>
        /// Gets or sets the visibility setting for the radio button corresponding to site based searchs.
        /// </summary>
        /// <remarks>Set this value to false to hide the "Site" radio button.  This setting has no effect
        /// if UseDropDownList is true.</remarks>
        public bool ShowSite
        {
            get
            {
                return _showSite;
            }
            set
            {
                _showSite = value;
            }
        }

        /// <summary>
        /// Gets or sets the visibility setting for the radio button corresponding to web based searchs.
        /// </summary>
        /// <remarks>Set this value to false to hide the "Web" radio button.  This setting has no effect
        /// if UseDropDownList is true.</remarks>
        public bool ShowWeb
        {
            get
            {
                return _showWeb;
            }
            set
            {
                _showWeb = value;
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
                if (string.IsNullOrEmpty(_siteIconURL))
                {
                    return IconController.IconURL("DnnSearch");
                }
                return _siteIconURL;
            }
            set
            {
                _siteIconURL = value;
            }
        }
                
        public string SeeMoreText
        {
            get
            {
                return Localization.GetSafeJSString("SeeMoreResults", Localization.GetResourceFile(this, MyFileName));
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
        /// Gets or sets the text for the "site" radio button or option list item.
        /// </summary>
        /// <value>The site text.</value>
        /// <remarks>If the value is not set or is an empty string, then the localized value from
        /// /admin/skins/app_localresources/Search.ascx.resx localresource file is used.</remarks>
        public string SiteText
        {
            get
            {
                if (string.IsNullOrEmpty(_siteText))
                {
                    return Localization.GetString("Site", Localization.GetResourceFile(this, MyFileName));
                }
                return _siteText;
            }
            set
            {
                _siteText = value;
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
                if (string.IsNullOrEmpty(_siteToolTip))
                {
                    return Localization.GetString("Site.ToolTip", Localization.GetResourceFile(this, MyFileName));
                }
                return _siteToolTip;
            }
            set
            {
                _siteToolTip = value;
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
                if (string.IsNullOrEmpty(_siteURL))
                {
                    return Localization.GetString("URL", Localization.GetResourceFile(this, MyFileName));
                }
                return _siteURL;
            }
            set
            {
                _siteURL = value;
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
                if (string.IsNullOrEmpty(_webIconURL))
                {
                    return IconController.IconURL("GoogleSearch");
                }
                return _webIconURL;
            }
            set
            {
                _webIconURL = value;
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
                if (string.IsNullOrEmpty(_webText))
                {
                    return Localization.GetString("Web", Localization.GetResourceFile(this, MyFileName));
                }
                return _webText;
            }
            set
            {
                _webText = value;
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
                if (string.IsNullOrEmpty(_webToolTip))
                {
                    return Localization.GetString("Web.ToolTip", Localization.GetResourceFile(this, MyFileName));
                }
                return _webToolTip;
            }
            set
            {
                _webToolTip = value;
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
                if (string.IsNullOrEmpty(_webURL))
                {
                    return Localization.GetString("URL", Localization.GetResourceFile(this, MyFileName));
                }
                return _webURL;
            }
            set
            {
                _webURL = value;
            }
        }

        /// <summary>
        /// minium chars required to trigger auto search
        /// </summary>
        public int MinCharRequired { get; set; }

        /// <summary>
        /// The millisecond to delay trigger auto search
        /// </summary>
        public int AutoSearchDelayInMilliSecond { get; set; }

        private bool _enableWildSearch = true;
        /// <summary>
        /// Disable the wild search
        /// </summary>
        public bool EnableWildSearch { get { return _enableWildSearch; } set { _enableWildSearch = value; } }

        protected int PortalId { get; set; }

        protected string SearchType { get; set; }

        protected string CultureCode { get; set; }

        #endregion

        #region Private Methods

        private int GetSearchTabId()
        {
            int searchTabId = PortalSettings.SearchTabId;
            if (searchTabId == Null.NullInteger)
            {
                var objModules = new ModuleController();
                ArrayList arrModules = objModules.GetModulesByDefinition(PortalSettings.PortalId, "Search Results");
                if (arrModules.Count > 1)
                {
                    foreach (ModuleInfo SearchModule in arrModules)
                    {
                        if (SearchModule.CultureCode == PortalSettings.CultureCode)
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
            int searchTabId = GetSearchTabId();

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
                        if (UseWebForSite)
                        {
                            strURL = SiteURL;
                            if (!string.IsNullOrEmpty(strURL))
                            {
                                strURL = strURL.Replace("[TEXT]", Server.UrlEncode(searchText));
                                strURL = strURL.Replace("[DOMAIN]", Request.Url.Host);
                                UrlUtils.OpenNewWindow(Page, GetType(), strURL);
                            }
                        }
                        else
                        {
                            if (Host.UseFriendlyUrls)
                            {
                                Response.Redirect(Globals.NavigateURL(searchTabId) + "?Search=" + Server.UrlEncode(searchText));
                            }
                            else
                            {
                                Response.Redirect(Globals.NavigateURL(searchTabId) + "&Search=" + Server.UrlEncode(searchText));
                            }
                        }
                        break;
                    case "W":
                        // web
                        strURL = WebURL;
                        if (!string.IsNullOrEmpty(strURL))
                        {
                            strURL = strURL.Replace("[TEXT]", Server.UrlEncode(searchText));
                            strURL = strURL.Replace("[DOMAIN]", "");
                            UrlUtils.OpenNewWindow(Page, GetType(), strURL);
                        }
                        break;
                }
            }
            else
            {
                if (Host.UseFriendlyUrls)
                {
                    Response.Redirect(Globals.NavigateURL(searchTabId));
                }
                else
                {
                    Response.Redirect(Globals.NavigateURL(searchTabId));
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
                        
            Framework.jQuery.RegisterDnnJQueryPlugins(this.Page);
            Framework.ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            ClientResourceManager.RegisterStyleSheet(Page, "~/Resources/Search/SearchSkinObjectPreview.css");
            ClientResourceManager.RegisterScript(Page, "~/Resources/Search/SearchSkinObjectPreview.js");
            

            cmdSearch.Click += CmdSearchClick;
            cmdSearchNew.Click += CmdSearchNewClick;

            if (!Page.IsPostBack)
            {
                if (MinCharRequired == 0) MinCharRequired = 2;
                if (AutoSearchDelayInMilliSecond == 0) AutoSearchDelayInMilliSecond = 400;
                PortalId = PortalSettings.ActiveTab.IsSuperTab ? PortalSettings.PortalId : -1;

                if (!String.IsNullOrEmpty(Submit))
                {
                    if (Submit.IndexOf("src=", StringComparison.Ordinal) != -1)
                    {
                        Submit = Submit.Replace("src=\"", "src=\"" + PortalSettings.ActiveTab.SkinPath);
                        Submit = Submit.Replace("src='", "src='" + PortalSettings.ActiveTab.SkinPath);
                    }
                }
                else
                {
                    Submit = Localization.GetString("Search", Localization.GetResourceFile(this, MyFileName));
                }
                cmdSearch.Text = Submit;
                cmdSearchNew.Text = Submit;
                if (!String.IsNullOrEmpty(CssClass))
                {
                    WebRadioButton.CssClass = CssClass;
                    SiteRadioButton.CssClass = CssClass;
                    cmdSearch.CssClass = CssClass;
                    cmdSearchNew.CssClass = CssClass;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the cmdSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event is only used when <see cref="UseDropDownList">UseDropDownList</see> is false.</remarks>
        private void CmdSearchClick(object sender, EventArgs e)
        {
            SearchType = "S";
            if (WebRadioButton.Visible)
            {
                if (WebRadioButton.Checked)
                {
                    SearchType = "W";
                }
            }
            ExecuteSearch(txtSearch.Text.Trim(), SearchType);
        }

        /// <summary>
        /// Handles the Click event of the cmdSearchNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event is only used when <see cref="UseDropDownList">UseDropDownList</see> is true.</remarks>
        protected void CmdSearchNewClick(object sender, EventArgs e)
        {
            SearchType = ClientAPI.GetClientVariable(Page, "SearchIconSelected");
            ExecuteSearch(txtSearchNew.Text.Trim(), SearchType);
        }

        /// <summary>
        /// Handles the PreRender event of the Page control.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        /// <remarks>This event performs final initialization tasks for the search object UI.</remarks>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ClassicSearch.Visible = !UseDropDownList;
            DropDownSearch.Visible = UseDropDownList;
            CultureCode = System.Threading.Thread.CurrentThread.CurrentCulture.ToString();
            
            if (UseDropDownList)
            {
                //Client Variables will survive a postback so there is no reason to register them.
                if (!Page.IsPostBack)
                {

                    downArrow.AlternateText = Localization.GetString("DropDownGlyph.AltText", Localization.GetResourceFile(this, MyFileName));
                    downArrow.ToolTip = downArrow.AlternateText;

                    ClientAPI.RegisterClientVariable(Page, "SearchIconWebUrl", string.Format("url({0})", ResolveUrl(WebIconURL)), true);
                    ClientAPI.RegisterClientVariable(Page, "SearchIconSiteUrl", string.Format("url({0})", ResolveUrl(SiteIconURL)), true);

                    //We are going to use a dnn client variable to store which search option (web/site) is selected.
                    ClientAPI.RegisterClientVariable(Page, "SearchIconSelected", "S", true);
                    SearchType = "S";
                }

                JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
                ClientResourceManager.RegisterScript(Page, "~/Resources/Search/Search.js", FileOrder.Js.DefaultPriority, "DnnFormBottomProvider");

                txtSearchNew.Attributes.Add("autocomplete", "off");
                txtSearchNew.Attributes.Add("placeholder", PlaceHolderText);
            }
            else
            {
                if (!Page.IsPostBack)
                {
                    WebRadioButton.Visible = ShowWeb;
                    SiteRadioButton.Visible = ShowSite;

                    if (WebRadioButton.Visible)
                    {
                        WebRadioButton.Checked = true;
                        WebRadioButton.Text = WebText;
                        WebRadioButton.ToolTip = WebToolTip;
                    }
                    if (SiteRadioButton.Visible)
                    {
                        SiteRadioButton.Checked = true;
                        SiteRadioButton.Text = SiteText;
                        SiteRadioButton.ToolTip = SiteToolTip;
                    }

                    SearchType = "S";
                    txtSearch.Attributes.Add("autocomplete", "off");
                    txtSearch.Attributes.Add("placeholder", PlaceHolderText);
                }
            }
        }

        #endregion
    }
}