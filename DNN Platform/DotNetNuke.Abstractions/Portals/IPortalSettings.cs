// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// The PortalSettings class encapsulates all of the settings for the Portal,
    /// as well as the configuration settings required to execute the current tab
    /// view within the portal.
    /// </summary>
    public interface IPortalSettings
    {
        /// <summary>Gets a value indicating whether if true then add a cachebuster parameter to generated file URI's.</summary>
        bool AddCachebusterToResourceUris { get; }

        /// <summary>Gets a compatible page header.</summary>
        /// <remarks>
        /// generates a : Page.Response.AddHeader("X-UA-Compatible", "").
        /// </remarks>
        string AddCompatibleHttpHeader { get; }

        /// <summary>Gets or sets the administrator id.</summary>
        int AdministratorId { get; set; }

        /// <summary>Gets or sets the administrator role id.</summary>
        int AdministratorRoleId { get; set; }

        /// <summary>Gets or sets the administrator role name.</summary>
        string AdministratorRoleName { get; set; }

        /// <summary>Gets or sets the admin tab id.</summary>
        int AdminTabId { get; set; }

        /// <summary>
        /// Gets a value indicating whether allows users to select their own UI culture.
        /// When set to false (default) framework will allways same culture for both
        /// CurrentCulture (content) and CurrentUICulture (interface).
        /// </summary>
        /// <remarks>Defaults to False.</remarks>
        bool AllowUserUICulture { get; }

        /// <summary>Gets or sets the background file.</summary>
        string BackgroundFile { get; set; }

        /// <summary>Gets or sets the banner advertising.</summary>
        int BannerAdvertising { get; set; }

        /// <summary>Gets the CDF version.</summary>
        int CdfVersion { get; }

        /// <summary>Gets a value indicating whether the control panel is visible.</summary>
        bool ControlPanelVisible { get; }

        /// <summary>
        /// Gets link for the user to find out more about cookies. If not specified the link
        /// shown will point to cookiesandyou.com.
        /// </summary>
        string CookieMoreLink { get; }

        /// <summary>Gets or sets the culture code.</summary>
        string CultureCode { get; set; }

        /// <summary>Gets or sets the currency.</summary>
        string Currency { get; set; }

        /// <summary>Gets a value indicating whether if true then all users will be pushed through the data consent workflow.</summary>
        bool DataConsentActive { get; }

        /// <summary>
        /// Gets if set this is a tab id of a page where the user will be redirected to for consent. If not set then
        /// the platform's default logic is used.
        /// </summary>
        int DataConsentConsentRedirect { get; }

        /// <summary>Gets the delay time (in conjunction with DataConsentDelayMeasurement) for the DataConsentUserDeleteAction.</summary>
        int DataConsentDelay { get; }

        /// <summary>Gets units for DataConsentDelay.</summary>
        string DataConsentDelayMeasurement { get; }

        /// <summary>
        /// Gets last time the terms and conditions have been changed. This will determine if the user needs to
        /// reconsent or not. Legally once the terms have changed, users need to sign again. This value is set
        /// by the "reset consent" button on the UI.
        /// </summary>
        DateTime DataConsentTermsLastChange { get; }

        /// <summary>Gets the default admin container.</summary>
        string DefaultAdminContainer { get; }

        /// <summary>Gets the default admin skin.</summary>
        string DefaultAdminSkin { get; }

        /// <summary>Gets the default authentication provider.</summary>
        string DefaultAuthProvider { get; }

        /// <summary>Gets a value indicating whether the default control panel is visible.</summary>
        bool DefaultControlPanelVisibility { get; }

        /// <summary>Gets the default icon location.</summary>
        string DefaultIconLocation { get; }

        /// <summary>Gets or sets the default location.</summary>
        string DefaultLanguage { get; set; }

        /// <summary>Gets the default module action menu.</summary>
        string DefaultModuleActionMenu { get; }

        /// <summary>Gets the Default Module Id.</summary>
        /// <remarks>Defaults to Null.NullInteger.</remarks>
        int DefaultModuleId { get; }

        /// <summary>Gets the default portal alias.</summary>
        string DefaultPortalAlias { get; }

        /// <summary>Gets the default portal container.</summary>
        string DefaultPortalContainer { get; }

        /// <summary>Gets the default portal skin.</summary>
        string DefaultPortalSkin { get; }

        /// <summary>Gets the Default Tab Id.</summary>
        /// <remarks>Defaults to Null.NullInteger.</remarks>
        int DefaultTabId { get; }

        /// <summary>Gets or sets the portal description.</summary>
        string Description { get; set; }

        /// <summary>Gets a value indicating whether if this is true, then regular users can't send message to specific user/group.</summary>
        bool DisablePrivateMessage { get; }

        /// <summary>Gets or sets the portal email.</summary>
        string Email { get; set; }

        /// <summary>Gets a value indicating whether Browser Language Detection is Enabled.</summary>
        /// <remarks>Defaults to True.</remarks>
        bool EnableBrowserLanguage { get; }

        /// <summary>Gets a value indicating whether the composite files for client scripts is enabled.</summary>
        bool EnableCompositeFiles { get; }

        /// <summary>Gets a value indicating whether to use the popup.</summary>
        /// <remarks>Defaults to True.</remarks>
        bool EnablePopUps { get; }

        /// <summary>Gets a value indicating whether website Administrator will receive the notification email when new user register.</summary>
        bool EnableRegisterNotification { get; }

        /// <summary>Gets a value indicating whether the Skin Widgets are enabled/supported.</summary>
        /// <remarks>Defaults to True.</remarks>
        [Obsolete("Deprecated in DotNetNuke 8.0.0. Skin widgets are no longer supported. Scheduled removal in v11.0.0.")]
        bool EnableSkinWidgets { get; }

        /// <summary>Gets a value indicating whether the content is localized.</summary>
        bool ContentLocalizationEnabled { get; }

        /// <summary>Gets the error page 404.</summary>
        int ErrorPage404 { get; }

        /// <summary>Gets the error page 500.</summary>
        int ErrorPage500 { get; }

        /// <summary>Gets or sets the expiry date.</summary>
        DateTime ExpiryDate { get; set; }

        /// <summary>Gets or sets the portal footer text.</summary>
        string FooterText { get; set; }

        /// <summary>Gets or sets the portal GUID.</summary>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", Justification = "Breaking change")]
        Guid GUID { get; set; }

        /// <summary>Gets a value indicating whether folders which are hidden or whose name begins with underscore are included in folder synchronization.</summary>
        /// <remarks>Defaults to True.</remarks>
        bool HideFoldersEnabled { get; }

        /// <summary>Gets a value indicating whether hide the login link.</summary>
        /// <remarks>Defaults to False.</remarks>
        bool HideLoginControl { get; }

        /// <summary>Gets or sets the portal home directory.</summary>
        string HomeDirectory { get; set; }

        /// <summary>Gets the portals home directory mapped path.</summary>
        string HomeDirectoryMapPath { get; }

        /// <summary>Gets or sets the portals home system directory.</summary>
        string HomeSystemDirectory { get; set; }

        /// <summary>Gets the portals home directory mapped path.</summary>
        string HomeSystemDirectoryMapPath { get; }

        /// <summary>Gets or sets the portals home tab id.</summary>
        int HomeTabId { get; set; }

        /// <summary>Gets or sets the host fee.</summary>
        float HostFee { get; set; }

        /// <summary>Gets or sets the host space.</summary>
        int HostSpace { get; set; }

        /*
         * add <a name="[moduleid]"></a> on the top of the module
         *
         * Desactivate this remove the html5 compatibility warnings
         * (and make the output smaller)
         *
         */

        /// <summary>Gets a value indicating whether a module can inject a hyperlink.</summary>
        /// <remarks>
        /// add <![CDATA[<a name="[moduleid]"></a>]]> on the top of the module.
        /// Desactivate this remove the html5 compatibility warnings
        /// (and make the output smaller).
        /// </remarks>
        bool InjectModuleHyperLink { get; }

        /// <summary>Gets a value indicating whether the Inline Editor is enabled.</summary>
        /// <remarks>Defaults to True.</remarks>
        bool InlineEditorEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether
        /// the current portal is in maintenance mode (if either this specific
        /// portal or the entire instance is locked). If locked, any actions
        /// which update the database should be disabled.
        /// </summary>
        bool IsLocked { get; }

        /// <summary>
        /// Gets a value indicating whether
        /// the current portal is in maintenance mode (note, the entire
        /// instance may still be locked, this only indicates whether this
        /// portal is specifically locked). If locked, any actions which
        /// update the database should be disabled.
        /// </summary>
        bool IsThisPortalLocked { get; }

        /// <summary>Gets or sets the portal keywords.</summary>
        string KeyWords { get; set; }

        /// <summary>Gets or sets the portals login tab id.</summary>
        int LoginTabId { get; set; }

        /// <summary>Gets or sets the portals logo file.</summary>
        string LogoFile { get; set; }

        /// <summary>Gets the portals page head text.</summary>
        string PageHeadText { get; }

        /// <summary>Gets or sets the portals page quota.</summary>
        int PageQuota { get; set; }

        /// <summary>Gets or sets the portals pages.</summary>
        int Pages { get; set; }

        /// <summary>Gets a value indicating whether enable url language.</summary>
        /// <remarks>Defaults to True.</remarks>
        bool EnableUrlLanguage { get; }

        /// <summary>Gets or sets the portal id.</summary>
        int PortalId { get; set; }

        /// <summary>Gets or sets the portal name.</summary>
        string PortalName { get; set; }

        /// <summary>Gets or sets the portals privacy tab id.</summary>
        int PrivacyTabId { get; set; }

        /// <summary>Gets or sets the portals registered role id.</summary>
        int RegisteredRoleId { get; set; }

        /// <summary>Gets or sets the portals registered role name.</summary>
        string RegisteredRoleName { get; set; }

        /// <summary>Gets or sets the portals register tab id.</summary>
        int RegisterTabId { get; set; }

        /// <summary>Gets a value indicating whether to include Common Words in the Search Index.</summary>
        /// <remarks>Defaults to False.</remarks>
        bool SearchIncludeCommon { get; }

        /// <summary>Gets the filter used for inclusion of tag info.</summary>
        /// <remarks>
        /// Defaults to "".
        /// </remarks>
        string SearchIncludedTagInfoFilter { get; }

        /// <summary>Gets a value indicating whether to include Numbers in the Search Index.</summary>
        /// <remarks>
        /// Defaults to False.
        /// </remarks>
        bool SearchIncludeNumeric { get; }

        /// <summary>Gets the maximum Search Word length to index.</summary>
        /// <remarks>
        /// Defaults to 3.
        /// </remarks>
        int SearchMaxWordlLength { get; }

        /// <summary>Gets the minimum Search Word length to index.</summary>
        /// <remarks>
        /// Defaults to 3.
        /// </remarks>
        int SearchMinWordlLength { get; }

        /// <summary>Gets or sets the portals search tab id.</summary>
        int SearchTabId { get; set; }

        /// <summary>Gets a value indicating whether a cookie consent popup should be shown.</summary>
        /// <remarks>Defaults to False.</remarks>
        bool ShowCookieConsent { get; }

        /// <summary>Gets the portals SMTP connection limit.</summary>
        int SMTPConnectionLimit { get; }

        /// <summary>Gets the portals SMPT max idle time.</summary>
        int SMTPMaxIdleTime { get; }

        /// <summary>Gets or sets the portals splash tab id.</summary>
        int SplashTabId { get; set; }

        /// <summary>Gets a value indicating what the SSL setup is for this portal.</summary>
        Security.SiteSslSetup SSLSetup { get; }

        /// <summary>Gets a value indicating whether SSL is enabled for the portal.</summary>
        bool SSLEnabled { get; }

        /// <summary>Gets a value indicating whether SLL is enforced for the portal.</summary>
        bool SSLEnforced { get; }

        /// <summary>Gets the SSL url for the portal.</summary>
        string SSLURL { get; }

        /// <summary>Gets the standard url for the portal.</summary>
        string STDURL { get; }

        /// <summary>Gets or sets the super tab id.</summary>
        int SuperTabId { get; set; }

        /// <summary>Gets or sets the terms and conditions tab id.</summary>
        int TermsTabId { get; set; }

        /// <summary>Gets or sets the timezone for the portal.</summary>
        TimeZoneInfo TimeZone { get; set; }

        /// <summary>Gets the currently logged in user identifier.</summary>
        /// <value>
        /// The user identifier.
        /// </value>
        int UserId { get; }

        /// <summary>Gets or sets the user quota for the portal.</summary>
        int UserQuota { get; set; }

        /// <summary>Gets or sets the user registration for the portal.</summary>
        int UserRegistration { get; set; }

        /// <summary>Gets or sets the users for the portal.</summary>
        int Users { get; set; }

        /// <summary>Gets or sets the user tab id for the portal.</summary>
        int UserTabId { get; set; }

        /// <summary>Gets a value indicating whether to display the dropdowns to quickly add a moduel to the page in the edit bar.</summary>
        bool ShowQuickModuleAddMenu { get; }
    }
}
