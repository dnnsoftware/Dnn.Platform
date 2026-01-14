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
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Extensions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.ClientDependency;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Blocker;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Pages;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.UI;
    using DotNetNuke.UI.Internals;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Client.ResourceManager;
    using Microsoft.Extensions.DependencyInjection;

    using DataCache = DotNetNuke.Common.Utilities.DataCache;
    using Globals = DotNetNuke.Common.Globals;
    using ReleaseMode = DotNetNuke.Abstractions.Application.ReleaseMode;

    /// <summary>The DNN default page.</summary>
    public partial class DefaultPage : CDefault, IClientAPICallbackEventHandler
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DefaultPage));
        private static readonly Regex HeaderTextRegex = new Regex(
            "<meta([^>])+name=('|\")robots('|\")",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);

        private readonly IApplicationInfo appInfo;
        private readonly IModuleControlPipeline moduleControlPipeline;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;
        private readonly IPortalSettingsController portalSettingsController;
        private readonly IClientResourceController clientResourceController;
        private readonly IPageService pageService;

        /// <summary>Initializes a new instance of the <see cref="DefaultPage"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public DefaultPage()
            : this(null, null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DefaultPage"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="appInfo">The application info.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="moduleControlPipeline">The module control pipeline.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalSettingsController">The portal settings controller.</param>
        /// <param name="clientResourceController">The client resources controller.</param>
        /// <param name="pageService">The page service.</param>
        public DefaultPage(
            INavigationManager navigationManager,
            IApplicationInfo appInfo,
            IApplicationStatusInfo appStatus,
            IModuleControlPipeline moduleControlPipeline,
            IHostSettings hostSettings,
            IEventLogger eventLogger,
            IPortalController portalController,
            IPortalSettingsController portalSettingsController,
            IClientResourceController clientResourceController,
            IPageService pageService)
            : base(portalController, appStatus, hostSettings)
        {
            this.NavigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
            this.appInfo = appInfo ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.moduleControlPipeline = moduleControlPipeline ?? Globals.GetCurrentServiceProvider().GetRequiredService<IModuleControlPipeline>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.portalSettingsController = portalSettingsController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalSettingsController>();
            this.clientResourceController = clientResourceController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>();
            this.pageService = pageService ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPageService>();
        }

        public string CurrentSkinPath => ((PortalSettings)HttpContext.Current.Items["PortalSettings"]).ActiveTab.SkinPath;

        /// <summary>Gets or sets property to allow the programmatic assigning of ScrollTop position.</summary>
        /// <value>Property to allow the programmatic assigning of ScrollTop position.</value>
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

            set
            {
                this.ScrollTop.Value = value.ToString();
            }
        }

        /// <summary>Gets a service that provides navigation features.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <summary>Gets a string representation of the list HTML attributes.</summary>
        protected string HtmlAttributeList
        {
            get
            {
                if (this.HtmlAttributes is not { Count: > 0 })
                {
                    return string.Empty;
                }

                var attr = new StringBuilder();
                foreach (string attributeName in this.HtmlAttributes.Keys)
                {
                    if (string.IsNullOrEmpty(attributeName) || this.HtmlAttributes[attributeName] == null)
                    {
                        continue;
                    }

                    var attributeValue = this.HtmlAttributes[attributeName];
                    if (attributeValue.IndexOf(',') > 0)
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

                return attr.ToString();
            }
        }

        private IPortalAliasInfo CurrentPortalAlias => this.PortalSettings.PortalAlias;

        private IPortalAliasInfo PrimaryPortalAlias => this.PortalSettings.PrimaryAlias;

        /// <inheritdoc/>
        public string RaiseClientAPICallbackEvent(string eventArgument)
        {
            var dict = this.ParsePageCallBackArgs(eventArgument);
            if (!dict.ContainsKey("type"))
            {
                return string.Empty;
            }

            if (!DNNClientAPI.IsPersonalizationKeyRegistered(dict["namingcontainer"] + ClientAPI.CUSTOM_COLUMN_DELIMITER + dict["key"]))
            {
                throw new Exception($"This personalization key has not been enabled ({dict["namingcontainer"]}:{dict["key"]}).  Make sure you enable it with DNNClientAPI.EnableClientPersonalization");
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

        /// <summary>Checks if the current version is not a production version.</summary>
        /// <returns>A value indicating whether the current version is not a production version.</returns>
        protected bool NonProductionVersion()
        {
            return this.appInfo.Status != ReleaseMode.Stable;
        }

        /// <summary>Contains the functionality to populate the Root aspx page with controls.</summary>
        /// <param name="e">The event arguments.</param>
        /// <remarks>
        /// - obtain PortalSettings from Current Context
        /// - set global page settings.
        /// - initialise reference paths to load the cascading style sheets
        /// - add skin control placeholder.  This holds all the modules and content of the page.
        /// </remarks>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // set global page settings
            this.InitializePage();

            var ctlSkin = this.GetSkin();

            // check for and read skin package level doctype
            this.SetSkinDoctype();

            // Manage disabled pages
            if (this.PortalSettings.ActiveTab.DisableLink)
            {
                if (TabPermissionController.CanAdminPage())
                {
                    var heading = Localization.GetString("PageDisabled.Header");
                    var message = Localization.GetString("PageDisabled.Text");
                    this.pageService.AddMessage(new PageMessage(
                        heading,
                        message,
                        PageMessageType.Warning,
                        string.Empty,
                        PagePriority.Page));
                }
                else
                {
                    if (this.PortalSettings.HomeTabId > 0)
                    {
                        this.Response.Redirect(this.NavigationManager.NavigateURL(this.PortalSettings.HomeTabId), true);
                    }
                    else
                    {
                        this.Response.Redirect(Globals.GetPortalDomainName(this.CurrentPortalAlias.HttpAlias, this.Request, true), true);
                    }
                }
            }

            // Manage canonical urls
            if (this.PortalSettings.PortalAliasMappingMode == PortalSettings.PortalAliasMapping.CanonicalUrl)
            {
                string primaryHttpAlias = null;
                if (Config.GetFriendlyUrlProvider() == "advanced")
                {
                    // advanced mode compares on the primary alias as set during alias identification
                    if (this.PrimaryPortalAlias != null && this.PortalSettings.PortalAlias != null)
                    {
                        if (string.Compare(this.PrimaryPortalAlias.HttpAlias, this.CurrentPortalAlias.HttpAlias, StringComparison.InvariantCulture) != 0)
                        {
                            primaryHttpAlias = this.PrimaryPortalAlias.HttpAlias;
                        }
                    }
                }
                else
                {
                    // other modes just depend on the default alias
                    if (string.Compare(this.CurrentPortalAlias.HttpAlias, this.PortalSettings.DefaultPortalAlias, StringComparison.InvariantCulture) != 0)
                    {
                        primaryHttpAlias = this.PortalSettings.DefaultPortalAlias;
                    }
                }

                if (primaryHttpAlias != null && string.IsNullOrEmpty(this.CanonicalLinkUrl))
                {
                    // a primary http alias was identified
                    var originalurl = this.Context.Items["UrlRewrite:OriginalUrl"].ToString();
                    this.CanonicalLinkUrl = originalurl.Replace(this.CurrentPortalAlias.HttpAlias, primaryHttpAlias);

                    if (UrlUtils.IsSecureConnectionOrSslOffload(this.Request))
                    {
                        this.CanonicalLinkUrl = this.CanonicalLinkUrl.Replace("http://", "https://");
                    }
                }
            }

            // add CSS links
            this.clientResourceController.CreateStylesheet("~/Resources/Shared/stylesheets/dnndefault/10.0.0/default.css")
                .SetNameAndVersion("dnndefault", "10.0.0", false)
                .SetPriority(FileOrder.Css.DefaultCss)
                .Register();

            this.clientResourceController.RegisterStylesheet(string.Concat(ctlSkin.SkinPath, "skin.css"), FileOrder.Css.SkinCss, true);
            this.clientResourceController.RegisterStylesheet(ctlSkin.SkinSrc.Replace(".ascx", ".css"), FileOrder.Css.SpecificSkinCss, true);

            // add skin to page
            this.SkinPlaceHolder.Controls.Add(ctlSkin);

            this.clientResourceController.RegisterStylesheet(string.Concat(this.PortalSettings.HomeDirectory, "portal.css"), FileOrder.Css.PortalCss, true);

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
                AJAX.GetScriptManager(this).AsyncPostBackTimeout = (int)this.hostSettings.AsyncTimeout.TotalSeconds;
            }

            this.DnnResources1.ApplicationPath = Globals.ApplicationPath;
            this.DnnResources2.ApplicationPath = Globals.ApplicationPath;
            this.DnnResources3.ApplicationPath = Globals.ApplicationPath;
        }

        /// <summary>Initialize the Scrolltop html control which controls the open / closed nature of each module.</summary>
        /// <param name="e">The event arguments.</param>
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

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs evt)
        {
            base.OnPreRender(evt);

            // Set the Head tags
            this.metaPanel.Visible = !UrlUtils.InPopUp();
            if (!UrlUtils.InPopUp())
            {
                this.pageService.SetTitle(this.Title, PagePriority.Page);
                this.Title = this.pageService.GetTitle();
                this.pageService.SetDescription(this.Description, PagePriority.Page);
                this.Description = this.pageService.GetDescription();
                this.pageService.SetKeyWords(this.KeyWords, PagePriority.Page);
                this.KeyWords = this.pageService.GetKeyWords();

                this.MetaGenerator.Content = this.Generator;
                this.MetaGenerator.Visible = !string.IsNullOrEmpty(this.Generator);
                this.MetaAuthor.Content = this.PortalSettings.PortalName;
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

            this.pageService.SetCanonicalLinkUrl(this.CanonicalLinkUrl, PagePriority.Page);
            this.CanonicalLinkUrl = this.pageService.GetCanonicalLinkUrl();
            if (!string.IsNullOrEmpty(this.CanonicalLinkUrl))
            {
                // Add Canonical <link> using the primary alias
                var canonicalLink = new HtmlLink();
                canonicalLink.Href = this.CanonicalLinkUrl;
                canonicalLink.Attributes.Add("rel", "canonical");

                // Add the HtmlLink to the Head section of the page.
                this.Page.Header.Controls.Add(canonicalLink);
            }

            foreach (var item in this.pageService.GetHeadTags())
            {
                this.Page.Header.Controls.Add(new LiteralControl(item.Value));
            }

            foreach (var item in this.pageService.GetMetaTags())
            {
                this.Page.Header.Controls.Add(new Meta() { Name = item.Name, Content = item.Content });
            }

            foreach (var item in this.pageService.GetMessages())
            {
                Skin.AddPageMessage(this, item.Heading, item.Message, item.MessageType.ToModuleMessageType(), item.IconSrc);
            }
        }

        /// <inheritdoc/>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Personalization.GetUserMode() == PortalSettings.Mode.Edit)
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

        /// <summary>
        /// Initializes the page.
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
        private void InitializePage()
        {
            // There could be a pending installation/upgrade process
            if (InstallBlocker.Instance.IsInstallInProgress())
            {
                Exceptions.ProcessHttpException(new HttpException(503, Localization.GetString("SiteAccessedWhileInstallationWasInProgress.Error", Localization.GlobalResourceFile)));
            }

            // Configure the ActiveTab with Skin/Container information
            this.portalSettingsController.ConfigureActiveTab(this.PortalSettings);

            this.clientResourceController.RegisterPathNameAlias("SkinPath", this.CurrentSkinPath);

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

            var cacheability = this.Request.IsAuthenticated ? this.hostSettings.AuthenticatedCacheability : this.hostSettings.UnauthenticatedCacheability;
            switch (cacheability)
            {
                case CacheControlHeader.NoCache:
                    this.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    break;
                case CacheControlHeader.Private:
                    this.Response.Cache.SetCacheability(HttpCacheability.Private);
                    break;
                case CacheControlHeader.Public:
                    this.Response.Cache.SetCacheability(HttpCacheability.Public);
                    break;
                case CacheControlHeader.ServerAndNoCache:
                    this.Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);
                    break;
                case CacheControlHeader.ServerAndPrivate:
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
                    var control = (IModuleControl)this.moduleControlPipeline.CreateModuleControl(slaveModule);
                    string extension = Path.GetExtension(slaveModule.ModuleControl.ControlSrc.ToLowerInvariant());
                    switch (extension)
                    {
                        case ".mvc":
                            var segments = slaveModule.ModuleControl.ControlSrc.Replace(".mvc", string.Empty).Split('/');
                            control.LocalResourceFile =
                                $"~/DesktopModules/MVC/{slaveModule.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{segments[0]}.resx";
                            break;
                        default:
                            var controlFileName = Path.GetFileName(slaveModule.ModuleControl.ControlSrc);
                            var controlSrcPath = slaveModule.ModuleControl.ControlSrc.Replace(controlFileName, string.Empty);
                            control.LocalResourceFile =
                                $"{controlSrcPath}{Localization.LocalResourceDirectory}/{controlFileName}";
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
            if (this.PortalSettings.ActiveTab.RefreshInterval > 0 && Personalization.GetUserMode() == PortalSettings.Mode.View && this.Request.QueryString["ctl"] == null)
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
            if (this.NonProductionVersion() && this.hostSettings.DisplayBetaNotice && !UrlUtils.InPopUp())
            {
                string versionString = $" ({this.appInfo.Status} Version: {this.appInfo.Version})";
                this.Title += versionString;
            }

            // register css variables
            var cssVariablesStyleSheet = this.GetCssVariablesStylesheet();
            this.clientResourceController.RegisterStylesheet(cssVariablesStyleSheet, FileOrder.Css.DefaultCss);

            // register the custom stylesheet of current page
            if (this.PortalSettings.ActiveTab.TabSettings.ContainsKey("CustomStylesheet") && !string.IsNullOrEmpty(this.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString()))
            {
                var styleSheet = this.PortalSettings.ActiveTab.TabSettings["CustomStylesheet"].ToString();

                // Try and go through the FolderProvider first
                var stylesheetFile = this.GetPageStylesheetFileInfo(styleSheet);
                if (stylesheetFile != null)
                {
                    this.clientResourceController.RegisterStylesheet(FileManager.Instance.GetUrl(stylesheetFile));
                }
                else
                {
                    this.clientResourceController.RegisterStylesheet(styleSheet);
                }
            }

            // Cookie Consent
            if (this.PortalSettings.ShowCookieConsent)
            {
                JavaScript.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn);
                ClientAPI.RegisterClientVariable(this, "cc_morelink", this.PortalSettings.CookieMoreLink, true);
                ClientAPI.RegisterClientVariable(this, "cc_message", Localization.GetString("cc_message", Localization.GlobalResourceFile), true);
                ClientAPI.RegisterClientVariable(this, "cc_dismiss", Localization.GetString("cc_dismiss", Localization.GlobalResourceFile), true);
                ClientAPI.RegisterClientVariable(this, "cc_link", Localization.GetString("cc_link", Localization.GlobalResourceFile), true);
                this.clientResourceController.RegisterScript("~/Resources/Shared/Components/CookieConsent/cookieconsent.min.js", FileOrder.Js.DnnControls);
                this.clientResourceController.RegisterStylesheet("~/Resources/Shared/Components/CookieConsent/cookieconsent.min.css", FileOrder.Css.ResourceCss);
                this.clientResourceController.RegisterStylesheet("~/js/dnn.cookieconsent.js");
            }
        }

        /// <summary>
        /// Look for skin level doctype configuration file, and inject the value into the top of default.aspx
        /// when no configuration if found, the doctype for versions prior to 4.4 is used to maintain backwards compatibility with existing skins.
        /// Adds xmlns and lang parameters when appropiate.
        /// </summary>
        private void SetSkinDoctype()
        {
            string strLang = CultureInfo.CurrentCulture.ToString();
            string strDocType = this.PortalSettings.ActiveTab.SkinDoctype;
            string strDir = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? "rtl" : "ltr";
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

            // Add "dir" attribute for text direction
            this.HtmlAttributes.Add("dir", strDir);

            // Find the placeholder control and render the doctype
            this.skinDocType.Text = this.PortalSettings.ActiveTab.SkinDoctype;
            this.attributeList.Text = this.HtmlAttributeList;

            // Add 'rtl' class to body for right-to-left language support
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {
                string existingClass = this.Body.Attributes["class"] ?? string.Empty;
                this.Body.Attributes["class"] = (existingClass + " rtl").Trim();
            }
        }

        private Skin GetSkin()
        {
            // We want the popup scripts to be loaded only if popups are enabled.
            this.LoadPopupScriptsIfNeeded();

            // But the popup skin should only be used if we are inside a popup.
            if (UrlUtils.InPopUp())
            {
                return Skin.GetPopUpSkin(this);
            }

            return Skin.GetSkin(this.hostSettings, this);
        }

        private void LoadPopupScriptsIfNeeded()
        {
            if (this.PortalSettings.EnablePopUps)
            {
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, this.PortalSettings, CommonJs.jQueryUI);
                var popupFilePath = HttpContext.Current.IsDebuggingEnabled
                                   ? "~/js/Debug/dnn.modalpopup.js"
                                   : "~/js/dnn.modalpopup.js";
                this.clientResourceController.RegisterScript(popupFilePath, FileOrder.Js.DnnModalPopup);
            }
        }

        private void ManageFavicon()
        {
            string headerLink = FavIcon.GetHeaderLink(this.hostSettings, this.PortalSettings.PortalId);

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

        private IFileInfo GetBackgroundFileInfo()
        {
            string cacheKey = string.Format(Common.Utilities.DataCache.PortalCacheKey, this.PortalSettings.PortalId, "BackgroundFile");
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, Common.Utilities.DataCache.PortalCacheTimeOut, Common.Utilities.DataCache.PortalCachePriority),
                this.GetBackgroundFileInfoCallBack);

            return file;
        }

        private IFileInfo GetBackgroundFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, this.PortalSettings.BackgroundFile);
        }

        private IFileInfo GetPageStylesheetFileInfo(string styleSheet)
        {
            string cacheKey = string.Format(Common.Utilities.DataCache.PortalCacheKey, this.PortalSettings.PortalId, "PageStylesheet" + styleSheet);
            var file = CBO.GetCachedObject<Services.FileSystem.FileInfo>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, Common.Utilities.DataCache.PortalCacheTimeOut, Common.Utilities.DataCache.PortalCachePriority, styleSheet),
                this.GetPageStylesheetInfoCallBack);

            return file;
        }

        private IFileInfo GetPageStylesheetInfoCallBack(CacheItemArgs itemArgs)
        {
            var styleSheet = itemArgs.Params[0].ToString();
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, styleSheet);
        }

        private string GetCssVariablesStylesheet()
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture, DataCache.PortalStylesCacheKey, this.PortalSettings.PortalId);
            var cacheArgs = new CacheItemArgs(
                cacheKey,
                DataCache.PortalCacheTimeOut,
                DataCache.PortalCachePriority,
                this.PortalSettings.GetStyles());
            string filePath = CBO.GetCachedObject<string>(this.hostSettings, cacheArgs, this.GetCssVariablesStylesheetCallback);
            return filePath;
        }

        private string GetCssVariablesStylesheetCallback(CacheItemArgs args)
        {
            var portalStyles = (PortalStyles)args.Params[0];

            var directory = this.PortalSettings.HomeSystemDirectoryMapPath;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var webPath = $"{this.PortalSettings.HomeSystemDirectory}{portalStyles.FileName}";

            var physicalPath = $"{directory}{portalStyles.FileName}";
            if (File.Exists(physicalPath))
            {
                return webPath;
            }

            var styles = portalStyles.ToString();
            File.WriteAllText(physicalPath, styles);

            return webPath;
        }
    }
}
