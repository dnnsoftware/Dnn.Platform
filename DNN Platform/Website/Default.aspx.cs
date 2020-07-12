// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Internals;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// Project  : DotNetNuke
    /// Class    : CDefault
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class DefaultPage : CDefault, IClientAPICallbackEventHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DefaultPage));

        private static readonly Regex HeaderTextRegex = new Regex(
            "<meta([^>])+name=('|\")robots('|\")",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        public DefaultPage()
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public string CurrentSkinPath
        {
            get
            {
                return ((PortalSettings)HttpContext.Current.Items["PortalSettings"]).ActiveTab.SkinPath;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets property to allow the programmatic assigning of ScrollTop position.
        /// </summary>
        /// <value>
        /// <placeholder>Property to allow the programmatic assigning of ScrollTop position</placeholder>
        /// </value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public int PageScrollTop
        {
            get
            {
                int pageScrollTop;
                var scrollValue = this.ScrollTop != null ? this.ScrollTop.Value : string.Empty;
                if (!int.TryParse(scrollValue, out pageScrollTop) || pageScrollTop < 0)
                {
                    pageScrollTop = Null.NullInteger;
                }

                return pageScrollTop;
            }

            set { this.ScrollTop.Value = value.ToString(); }
        }

        protected INavigationManager NavigationManager { get; }

        protected string HtmlAttributeList
        {
            get
            {
                if ((this.HtmlAttributes != null) && (this.HtmlAttributes.Count > 0))
                {
                    var attr = new StringBuilder();
                    foreach (string attributeName in this.HtmlAttributes.Keys)
                    {
                        if ((!string.IsNullOrEmpty(attributeName)) && (this.HtmlAttributes[attributeName] != null))
                        {
                            string attributeValue = this.HtmlAttributes[attributeName];
                            if (attributeValue.IndexOf(",") > 0)
                            {
                                var attributeValues = attributeValue.Split(',');
                                for (var attributeCounter = 0;
                                     attributeCounter <= attributeValues.Length - 1;
                                     attributeCounter++)
                                {
                                    attr.Append(string.Concat(" ", attributeName, "=\"", attributeValues[attributeCounter], "\""));
                                }
                            }
                            else
                            {
                                attr.Append(string.Concat(" ", attributeName, "=\"", attributeValue, "\""));
                            }
                        }
                    }

                    return attr.ToString();
                }

                return string.Empty;
            }
        }

        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
            var dict = this.ParsePageCallBackArgs(eventArgument);
            if (dict.ContainsKey("type"))
            {
                if (DNNClientAPI.IsPersonalizationKeyRegistered(dict["namingcontainer"] + ClientAPI.CUSTOM_COLUMN_DELIMITER + dict["key"]) == false)
                {
                    throw new Exception(string.Format("This personalization key has not been enabled ({0}:{1}).  Make sure you enable it with DNNClientAPI.EnableClientPersonalization", dict["namingcontainer"], dict["key"]));
                }

                switch ((DNNClientAPI.PageCallBackType)Enum.Parse(typeof(DNNClientAPI.PageCallBackType), dict["type"]))
                {
                    case DNNClientAPI.PageCallBackType.GetPersonalization:
                        return Personalization.GetProfile(dict["namingcontainer"], dict["key"]).ToString();
                    case DNNClientAPI.PageCallBackType.SetPersonalization:
                        Personalization.SetProfile(dict["namingcontainer"], dict["key"], dict["value"]);
                        return dict["value"];
                    default:
                        throw new Exception("Unknown Callback Type");
                }
            }

            return string.Empty;
        }

        protected bool NonProductionVersion()
        {
            return DotNetNukeContext.Current.Application.Status != ReleaseMode.Stable;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Contains the functionality to populate the Root aspx page with controls.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// - obtain PortalSettings from Current Context
        /// - set global page settings.
        /// - initialise reference paths to load the cascading style sheets
        /// - add skin control placeholder.  This holds all the modules and content of the page.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // set global page settings
            this.InitializePage();

            // load skin control and register UI js
            UI.Skins.Skin ctlSkin;
            if (this.PortalSettings.EnablePopUps)
            {
                ctlSkin = UrlUtils.InPopUp() ? UI.Skins.Skin.GetPopUpSkin(this) : UI.Skins.Skin.GetSkin(this);

                // register popup js
                JavaScript.RequestRegistration(CommonJs.jQueryUI);

                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";

                ClientResourceManager.RegisterScript(this, popupFilePath, FileOrder.Js.DnnModalPopup);
            }
            else
            {
                ctlSkin = UI.Skins.Skin.GetSkin(this);
            }

            // DataBind common paths for the client resource loader
            this.ClientResourceLoader.DataBind();
            this.ClientResourceLoader.PreRender += (sender, args) => JavaScript.Register(this.Page);

            // check for and read skin package level doctype
            this.SetSkinDoctype();

            // Manage disabled pages
            if (this.PortalSettings.ActiveTab.DisableLink)
            {
                if (TabPermissionController.CanAdminPage())
                {
                    var heading = Localization.GetString("PageDisabled.Header");
                    var message = Localization.GetString("PageDisabled.Text");
                    UI.Skins.Skin.AddPageMessage(ctlSkin, heading, message,
                                                 ModuleMessage.ModuleMessageType.YellowWarning);
                }
                else
                {
                    if (this.PortalSettings.HomeTabId > 0)
                    {
                        this.Response.Redirect(this.NavigationManager.NavigateURL(this.PortalSettings.HomeTabId), true);
                    }
                    else
                    {
                        this.Response.Redirect(Globals.GetPortalDomainName(this.PortalSettings.PortalAlias.HTTPAlias, this.Request, true), true);
                    }
                }
            }

            // Manage canonical urls
            if (this.PortalSettings.PortalAliasMappingMode == PortalSettings.PortalAliasMapping.CanonicalUrl)
            {
                string primaryHttpAlias = null;
                if (Config.GetFriendlyUrlProvider() == "advanced") // advanced mode compares on the primary alias as set during alias identification
                {
                    if (this.PortalSettings.PrimaryAlias != null && this.PortalSettings.PortalAlias != null)
                    {
                        if (string.Compare(this.PortalSettings.PrimaryAlias.HTTPAlias, this.PortalSettings.PortalAlias.HTTPAlias, StringComparison.InvariantCulture) != 0)
                        {
                            primaryHttpAlias = this.PortalSettings.PrimaryAlias.HTTPAlias;
                        }
                    }
                }
                else // other modes just depend on the default alias
                {
                    if (string.Compare(this.PortalSettings.PortalAlias.HTTPAlias, this.PortalSettings.DefaultPortalAlias, StringComparison.InvariantCulture) != 0)
                    {
                        primaryHttpAlias = this.PortalSettings.DefaultPortalAlias;
                    }
                }

                if (primaryHttpAlias != null && string.IsNullOrEmpty(this.CanonicalLinkUrl)) // a primary http alias was identified
                {
                    var originalurl = this.Context.Items["UrlRewrite:OriginalUrl"].ToString();
                    this.CanonicalLinkUrl = originalurl.Replace(this.PortalSettings.PortalAlias.HTTPAlias, primaryHttpAlias);

                    if (UrlUtils.IsSecureConnectionOrSslOffload(this.Request))
                    {
                        this.CanonicalLinkUrl = this.CanonicalLinkUrl.Replace("http://", "https://");
                    }
                }
            }

            // check if running with known account defaults
            if (this.Request.IsAuthenticated && string.IsNullOrEmpty(this.Request.QueryString["runningDefault"]) == false)
            {
                var userInfo = HttpContext.Current.Items["UserInfo"] as UserInfo;
                var usernameLower = userInfo?.Username?.ToLowerInvariant();

                // only show message to default users
                if ("admin".Equals(usernameLower) || "host".Equals(usernameLower))
                {
                    var messageText = this.RenderDefaultsWarning();
                    var messageTitle = Localization.GetString("InsecureDefaults.Title", Localization.GlobalResourceFile);
                    UI.Skins.Skin.AddPageMessage(ctlSkin, messageTitle, messageText, ModuleMessage.ModuleMessageType.RedError);
                }
            }

            // add CSS links
            ClientResourceManager.RegisterDefaultStylesheet(this, string.Concat(Globals.ApplicationPath, "/Resources/Shared/stylesheets/dnndefault/7.0.0/default.css"));
            ClientResourceManager.RegisterIEStylesheet(this, string.Concat(Globals.HostPath, "ie.css"));

            ClientResourceManager.RegisterStyleSheet(this, string.Concat(ctlSkin.SkinPath, "skin.css"), FileOrder.Css.SkinCss);
            ClientResourceManager.RegisterStyleSheet(this, ctlSkin.SkinSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificSkinCss);

            // add skin to page
            this.SkinPlaceHolder.Controls.Add(ctlSkin);

            ClientResourceManager.RegisterStyleSheet(this, string.Concat(this.PortalSettings.HomeDirectory, "portal.css"), FileOrder.Css.PortalCss);

            // add Favicon
            this.ManageFavicon();

            // ClientCallback Logic
            ClientAPI.HandleClientAPICallbackEvent(this);

            // add viewstateuserkey to protect against CSRF attacks
            if (this.User.Identity.IsAuthenticated)
            {
                this.ViewStateUserKey = this.User.Identity.Name;
            }

            // set the async postback timeout.
            if (AJAX.IsEnabled())
            {
                AJAX.GetScriptManager(this).AsyncPostBackTimeout = Host.AsyncTimeout;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initialize the Scrolltop html control which controls the open / closed nature of each module.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.ManageInstallerFiles();

            if (!string.IsNullOrEmpty(this.ScrollTop.Value))
            {
                DNNClientAPI.SetScrollTop(this.Page);
                this.ScrollTop.Value = this.ScrollTop.Value;
            }
        }

        protected override void OnPreRender(EventArgs evt)
        {
            base.OnPreRender(evt);

            // Set the Head tags
            this.metaPanel.Visible = !UrlUtils.InPopUp();
            if (!UrlUtils.InPopUp())
            {
                this.MetaGenerator.Content = this.Generator;
                this.MetaGenerator.Visible = !string.IsNullOrEmpty(this.Generator);
                this.MetaAuthor.Content = this.PortalSettings.PortalName;
                /*
                 * Never show to be html5 compatible and stay backward compatible
                 *
                 * MetaCopyright.Content = Copyright;
                 * MetaCopyright.Visible = (!String.IsNullOrEmpty(Copyright));
                 */
                this.MetaKeywords.Content = this.KeyWords;
                this.MetaKeywords.Visible = !string.IsNullOrEmpty(this.KeyWords);
                this.MetaDescription.Content = this.Description;
                this.MetaDescription.Visible = !string.IsNullOrEmpty(this.Description);
            }

            this.Page.Header.Title = this.Title;
            if (!string.IsNullOrEmpty(this.PortalSettings.AddCompatibleHttpHeader) && !this.HeaderIsWritten)
            {
                this.Page.Response.AddHeader("X-UA-Compatible", this.PortalSettings.AddCompatibleHttpHeader);
            }

            if (!string.IsNullOrEmpty(this.CanonicalLinkUrl))
            {
                // Add Canonical <link> using the primary alias
                var canonicalLink = new HtmlLink();
                canonicalLink.Href = this.CanonicalLinkUrl;
                canonicalLink.Attributes.Add("rel", "canonical");

                // Add the HtmlLink to the Head section of the page.
                this.Page.Header.Controls.Add(canonicalLink);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            if (this.PortalSettings.UserMode == PortalSettings.Mode.Edit)
            {
                var editClass = "dnnEditState";

                var bodyClass = this.Body.Attributes["class"];
                if (!string.IsNullOrEmpty(bodyClass))
                {
                    this.Body.Attributes["class"] = string.Format("{0} {1}", bodyClass, editClass);
                }
                else
                {
                    this.Body.Attributes["class"] = editClass;
                }
            }

            base.Render(writer);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// - Obtain PortalSettings from Current Context
        /// - redirect to a specific tab based on name
        /// - if first time loading this page then reload to avoid caching
        /// - set page title and stylesheet
        /// - check to see if we should show the Assembly Version in Page Title
        /// - set the background image if there is one selected
        /// - set META tags, copyright, keywords and description.
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void InitializePage()
        {
            // There could be a pending installation/upgrade process
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                Exceptions.ProcessHttpException(new HttpException(503, Localization.GetString("SiteAccessedWhileInstallationWasInProgress.Error", Localization.GlobalResourceFile)));
            }

            // Configure the ActiveTab with Skin/Container information
            PortalSettingsController.Instance().ConfigureActiveTab(this.PortalSettings);

            // redirect to a specific tab based on name
            if (!string.IsNullOrEmpty(this.Request.QueryString["tabname"]))
            {
                TabInfo tab = TabController.Instance.GetTabByName(this.Request.QueryString["TabName"], this.PortalSettings.PortalId);
                if (tab != null)
                {
                    var parameters = new List<string>(); // maximum number of elements
                    for (int intParam = 0; intParam <= this.Request.QueryString.Count - 1; intParam++)
                    {
                        switch (this.Request.QueryString.Keys[intParam].ToLowerInvariant())
                        {
                            case "tabid":
                            case "tabname":
                                break;
                            default:
                                parameters.Add(
                                    this.Request.QueryString.Keys[intParam] + "=" + this.Request.QueryString[intParam]);
                                break;
                        }
                    }

                    this.Response.Redirect(this.NavigationManager.NavigateURL(tab.TabID, Null.NullString, parameters.ToArray()), true);
                }
                else
                {
                    // 404 Error - Redirect to ErrorPage
                    Exceptions.ProcessHttpException(this.Request);
                }
            }

            string cacheability = this.Request.IsAuthenticated ? Host.AuthenticatedCacheability : Host.UnauthenticatedCacheability;

            switch (cacheability)
            {
                case "0":
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    break;
                case "1":
                    this.Response.Cache.SetCacheability(HttpCacheability.Private);
                    break;
                case "2":
                    this.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case "3":
                    this.Response.Cache.SetCacheability(HttpCacheability.Server);
                    break;
                case "4":
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    break;
                case "5":
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndPrivate);
                    break;
            }

            // Only insert the header control if a comment is needed
            if (!string.IsNullOrWhiteSpace(this.Comment))
            {
                this.Page.Header.Controls.AddAt(0, new LiteralControl(this.Comment));
            }

            if (this.PortalSettings.ActiveTab.PageHeadText != Null.NullString && !Globals.IsAdminControl())
            {
                this.Page.Header.Controls.Add(new LiteralControl(this.PortalSettings.ActiveTab.PageHeadText));
            }

            if (!string.IsNullOrEmpty(this.PortalSettings.PageHeadText))
            {
                this.metaPanel.Controls.Add(new LiteralControl(this.PortalSettings.PageHeadText));
            }

            // set page title
            if (UrlUtils.InPopUp())
            {
                var strTitle = new StringBuilder(this.PortalSettings.PortalName);
                var slaveModule = UIUtilities.GetSlaveModule(this.PortalSettings.ActiveTab.TabID);

                // Skip is popup is just a tab (no slave module)
                if (slaveModule.DesktopModuleID != Null.NullInteger)
                {
                    var control = ModuleControlFactory.CreateModuleControl(slaveModule) as IModuleControl;
                    string extension = Path.GetExtension(slaveModule.ModuleControl.ControlSrc.ToLowerInvariant());
                    switch (extension)
                    {
                        case ".mvc":
                            var segments = slaveModule.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');

                            control.LocalResourceFile = string.Format(
                                "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                                slaveModule.DesktopModule.FolderName,
                                Localization.LocalResourceDirectory,
                                segments[0]);
                            break;
                        default:
                            control.LocalResourceFile = string.Concat(
                                slaveModule.ModuleControl.ControlSrc.Replace(
                                    Path.GetFileName(slaveModule.ModuleControl.ControlSrc), string.Empty),
                                Localization.LocalResourceDirectory, "/",
                                Path.GetFileName(slaveModule.ModuleControl.ControlSrc));
                            break;
                    }

                    var title = Localization.LocalizeControlTitle(control);

                    strTitle.Append(string.Concat(" > ", this.PortalSettings.ActiveTab.LocalizedTabName));
                    strTitle.Append(string.Concat(" > ", title));
                }
                else
                {
                    strTitle.Append(string.Concat(" > ", this.PortalSettings.ActiveTab.LocalizedTabName));
                }

                // Set to page
                this.Title = strTitle.ToString();
            }
            else
            {
                // If tab is named, use that title, otherwise build it out via breadcrumbs
                if (!string.IsNullOrEmpty(this.PortalSettings.ActiveTab.Title))
                {
                    this.Title = this.PortalSettings.ActiveTab.Title;
                }
                else
                {
                    // Elected for SB over true concatenation here due to potential for long nesting depth
                    var strTitle = new StringBuilder(this.PortalSettings.PortalName);
                    foreach (TabInfo tab in this.PortalSettings.ActiveTab.BreadCrumbs)
                    {
                        strTitle.Append(string.Concat(" > ", tab.TabName));
                    }

                    this.Title = strTitle.ToString();
                }
            }

            // set the background image if there is one selected
            if (!UrlUtils.InPopUp() && this.FindControl("Body") != null)
            {
                if (!string.IsNullOrEmpty(this.PortalSettings.BackgroundFile))
                {
                    var fileInfo = this.GetBackgroundFileInfo();
                    var url = FileManager.Instance.GetUrl(fileInfo);

                    ((HtmlGenericControl)this.FindControl("Body")).Attributes["style"] = string.Concat("background-image: url('", url, "')");
                }
            }

            // META Refresh
            // Only autorefresh the page if we are in VIEW-mode and if we aren't displaying some module's subcontrol.
            if (this.PortalSettings.ActiveTab.RefreshInterval > 0 && this.PortalSettings.UserMode == PortalSettings.Mode.View && this.Request.QueryString["ctl"] == null)
            {
                this.MetaRefresh.Content = this.PortalSettings.ActiveTab.RefreshInterval.ToString();
                this.MetaRefresh.Visible = true;
            }
            else
            {
                this.MetaRefresh.Visible = false;
            }

            // META description
            if (!string.IsNullOrEmpty(this.PortalSettings.ActiveTab.Description))
            {
                this.Description = this.PortalSettings.ActiveTab.Description;
            }
            else
            {
                this.Description = this.PortalSettings.Description;
            }

            // META keywords
            if (!string.IsNullOrEmpty(this.PortalSettings.ActiveTab.KeyWords))
            {
                this.KeyWords = this.PortalSettings.ActiveTab.KeyWords;
            }
            else
            {
                this.KeyWords = this.PortalSettings.KeyWords;
            }

            // META copyright
            if (!string.IsNullOrEmpty(this.PortalSettings.FooterText))
            {
                this.Copyright = this.PortalSettings.FooterText.Replace("[year]", DateTime.Now.Year.ToString());
            }
            else
            {
                this.Copyright = string.Concat("Copyright (c) ", DateTime.Now.Year, " by ", this.PortalSettings.PortalName);
            }

            // META generator
            this.Generator = string.Empty;

            // META Robots - hide it inside popups and if PageHeadText of current tab already contains a robots meta tag
            if (!UrlUtils.InPopUp() &&
                !(HeaderTextRegex.IsMatch(this.PortalSettings.ActiveTab.PageHeadText) ||
                  HeaderTextRegex.IsMatch(this.PortalSettings.PageHeadText)))
            {
                this.MetaRobots.Visible = true;
                var allowIndex = true;
                if ((this.PortalSettings.ActiveTab.TabSettings.ContainsKey("AllowIndex") &&
                     bool.TryParse(this.PortalSettings.ActiveTab.TabSettings["AllowIndex"].ToString(), out allowIndex) &&
                     !allowIndex)
                    ||
                    (this.Request.QueryString["ctl"] != null &&
                     (this.Request.QueryString["ctl"] == "Login" || this.Request.QueryString["ctl"] == "Register")))
                {
                    this.MetaRobots.Content = "NOINDEX, NOFOLLOW";
                }
                else
                {
                    this.MetaRobots.Content = "INDEX, FOLLOW";
                }
            }

            // NonProduction Label Injection
            if (this.NonProductionVersion() && Host.DisplayBetaNotice && !UrlUtils.InPopUp())
            {
                string versionString = string.Format(" ({0} Version: {1})", DotNetNukeContext.Current.Application.Status,
                                                     DotNetNukeContext.Current.Application.Version);
                this.Title += versionString;
            }

            // register the custom stylesheet of current page
            if (this.PortalSettings.ActiveTab.TabSettings.ContainsKey("CustomStylesheet") && !string.IsNullOrEmpty(this.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString()))
            {
                var customStylesheet = Path.Combine(this.PortalSettings.HomeDirectory, this.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString());
                ClientResourceManager.RegisterStyleSheet(this, customStylesheet);
            }

            // Cookie Consent
            if (this.PortalSettings.ShowCookieConsent)
            {
                ClientAPI.RegisterClientVariable(this, "cc_morelink", this.PortalSettings.CookieMoreLink, true);
                ClientAPI.RegisterClientVariable(this, "cc_message", Localization.GetString("cc_message", Localization.GlobalResourceFile), true);
                ClientAPI.RegisterClientVariable(this, "cc_dismiss", Localization.GetString("cc_dismiss", Localization.GlobalResourceFile), true);
                ClientAPI.RegisterClientVariable(this, "cc_link", Localization.GetString("cc_link", Localization.GlobalResourceFile), true);
                ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Components/CookieConsent/cookieconsent.min.js", FileOrder.Js.DnnControls);
                ClientResourceManager.RegisterStyleSheet(this.Page, "~/Resources/Shared/Components/CookieConsent/cookieconsent.min.css", FileOrder.Css.ResourceCss);
                ClientResourceManager.RegisterScript(this.Page, "~/js/dnn.cookieconsent.js", FileOrder.Js.DefaultPriority);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Look for skin level doctype configuration file, and inject the value into the top of default.aspx
        /// when no configuration if found, the doctype for versions prior to 4.4 is used to maintain backwards compatibility with existing skins.
        /// Adds xmlns and lang parameters when appropiate.
        /// </summary>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        private void SetSkinDoctype()
        {
            string strLang = CultureInfo.CurrentCulture.ToString();
            string strDocType = this.PortalSettings.ActiveTab.SkinDoctype;
            if (strDocType.Contains("XHTML 1.0"))
            {
                // XHTML 1.0
                this.HtmlAttributes.Add("xml:lang", strLang);
                this.HtmlAttributes.Add("lang", strLang);
                this.HtmlAttributes.Add("xmlns", "http://www.w3.org/1999/xhtml");
            }
            else if (strDocType.Contains("XHTML 1.1"))
            {
                // XHTML 1.1
                this.HtmlAttributes.Add("xml:lang", strLang);
                this.HtmlAttributes.Add("xmlns", "http://www.w3.org/1999/xhtml");
            }
            else
            {
                // other
                this.HtmlAttributes.Add("lang", strLang);
            }

            // Find the placeholder control and render the doctype
            this.skinDocType.Text = this.PortalSettings.ActiveTab.SkinDoctype;
            this.attributeList.Text = this.HtmlAttributeList;
        }

        private void ManageFavicon()
        {
            string headerLink = FavIcon.GetHeaderLink(this.PortalSettings.PortalId);

            if (!string.IsNullOrEmpty(headerLink))
            {
                this.Page.Header.Controls.Add(new Literal { Text = headerLink });
            }
        }

        // I realize the parsing of this is rather primitive.  A better solution would be to use json serialization
        // unfortunately, I don't have the time to write it.  When we officially adopt MS AJAX, we will get this type of
        // functionality and this should be changed to utilize it for its plumbing.
        private Dictionary<string, string> ParsePageCallBackArgs(string strArg)
        {
            string[] aryVals = strArg.Split(new[] { ClientAPI.COLUMN_DELIMITER }, StringSplitOptions.None);
            var objDict = new Dictionary<string, string>();
            if (aryVals.Length > 0)
            {
                objDict.Add("type", aryVals[0]);
                switch (
                    (DNNClientAPI.PageCallBackType)Enum.Parse(typeof(DNNClientAPI.PageCallBackType), objDict["type"]))
                {
                    case DNNClientAPI.PageCallBackType.GetPersonalization:
                        objDict.Add("namingcontainer", aryVals[1]);
                        objDict.Add("key", aryVals[2]);
                        break;
                    case DNNClientAPI.PageCallBackType.SetPersonalization:
                        objDict.Add("namingcontainer", aryVals[1]);
                        objDict.Add("key", aryVals[2]);
                        objDict.Add("value", aryVals[3]);
                        break;
                }
            }

            return objDict;
        }

        /// <summary>
        /// check if a warning about account defaults needs to be rendered.
        /// </summary>
        /// <returns>localised error message.</returns>
        /// <remarks></remarks>
        private string RenderDefaultsWarning()
        {
            var warningLevel = this.Request.QueryString["runningDefault"];
            var warningMessage = string.Empty;
            switch (warningLevel)
            {
                case "1":
                    warningMessage = Localization.GetString("InsecureAdmin.Text", Localization.SharedResourceFile);
                    break;
                case "2":
                    warningMessage = Localization.GetString("InsecureHost.Text", Localization.SharedResourceFile);
                    break;
                case "3":
                    warningMessage = Localization.GetString("InsecureDefaults.Text", Localization.SharedResourceFile);
                    break;
            }

            return warningMessage;
        }

        private IFileInfo GetBackgroundFileInfo()
        {
            string cacheKey = string.Format(Common.Utilities.DataCache.PortalCacheKey, this.PortalSettings.PortalId, "BackgroundFile");
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                new CacheItemArgs(cacheKey, Common.Utilities.DataCache.PortalCacheTimeOut, Common.Utilities.DataCache.PortalCachePriority),
                this.GetBackgroundFileInfoCallBack);

            return file;
        }

        private IFileInfo GetBackgroundFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, this.PortalSettings.BackgroundFile);
        }
    }
}
