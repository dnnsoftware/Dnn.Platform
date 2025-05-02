// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application;

using System;

using DotNetNuke.Abstractions.Security;

/// <summary>Provides access to well-known host settings.</summary>
public interface IHostSettings
{
    /// <summary>Gets the duration before an account is automatically unlocked.</summary>
    public TimeSpan AutoAccountUnlockDuration { get; }

    /// <summary>Gets the <c>HttpCacheability</c> value for authenticated requests.</summary>
    /// <remarks>The following mapping is used:
    /// <list type="bullet">
    /// <item><c>"0"</c> - <c>HttpCacheability.NoCache</c></item>
    /// <item><c>"1"</c> - <c>HttpCacheability.Private</c></item>
    /// <item><c>"2"</c> - <c>HttpCacheability.Public</c></item>
    /// <item><c>"3"</c> - <c>HttpCacheability.Server</c></item>
    /// <item><c>"4"</c> - <c>HttpCacheability.ServerAndNoCache</c></item>
    /// <item><c>"5"</c> - <c>HttpCacheability.ServerAndPrivate</c></item>
    /// </list>
    /// </remarks>
    public string AuthenticatedCacheability { get; }

    /// <summary>Gets the <c>HttpCacheability</c> value for authenticated requests.</summary>
    /// <remarks>The following mapping is used:
    /// <list type="bullet">
    /// <item><c>"0"</c> - <c>HttpCacheability.NoCache</c></item>
    /// <item><c>"1"</c> - <c>HttpCacheability.Private</c></item>
    /// <item><c>"2"</c> - <c>HttpCacheability.Public</c></item>
    /// <item><c>"3"</c> - <c>HttpCacheability.Server</c></item>
    /// <item><c>"4"</c> - <c>HttpCacheability.ServerAndNoCache</c></item>
    /// <item><c>"5"</c> - <c>HttpCacheability.ServerAndPrivate</c></item>
    /// </list>
    /// </remarks>
    public string UnauthenticatedCacheability { get; }

    /// <summary>Gets a value indicating whether CDN has been enabled for all registered javascript libraries.</summary>
    public bool CdnEnabled { get; }

    /// <summary>Gets a value indicating whether the Upgrade Indicator is enabled.</summary>
    public bool CheckUpgrade { get; }

    /// <summary>Gets the Control Panel.</summary>
    public string ControlPanel { get; }

    /// <summary>Gets a value indicating whether the default Edit Bar is disabled.</summary>
    public bool DisableEditBar { get; }

    /// <summary>
    /// Gets a value indicating whether setting to control where the control panel is loaded by the core and allowed to control its own visibility.
    /// this is useful when the control panel needs to be visible for all users regardless of edit page/module permissions.
    /// it's also for backwards compatibility, prior to 7.2 the control panel was always loaded.
    /// </summary>
    public bool AllowControlPanelToDetermineVisibility { get; }

    /// <summary>Gets a value indicating whether indicates whether Composite Files are enabled at the host level.</summary>
    public bool CrmEnableCompositeFiles { get; }

    /// <summary>Gets a value indicating whether indicates whether CSS Minification is enabled at the host level.</summary>
    public bool CrmMinifyCss { get; }

    /// <summary>Gets a value indicating whether indicates whether JS Minification is enabled at the host level.</summary>
    public bool CrmMinifyJs { get; }

    /// <summary>Gets the Client Resource Management version number.</summary>
    public int CrmVersion { get; }

    /// <summary>Gets the Default Admin Container.</summary>
    public string DefaultAdminContainer { get; }

    /// <summary>Gets the Default Admin Skin.</summary>
    public string DefaultAdminSkin { get; }

    /// <summary>Gets the Default Doc Type.</summary>
    public string DefaultDocType { get; }

    /// <summary>Gets the Default Portal Container.</summary>
    public string DefaultPortalContainer { get; }

    /// <summary>Gets the Default Portal Skin.</summary>
    public string DefaultPortalSkin { get; }

    /// <summary>Gets a value indicating whether to display the beta notice.</summary>
    public bool DisplayBetaNotice { get; }

    /// <summary>Gets a value indicating whether enable checking for banned words when setting password during registration.</summary>
    public bool EnableBannedList { get; }

    /// <summary>Gets a value indicating whether Browser Language Detection is Enabled.</summary>
    public bool EnableBrowserLanguage { get; }

    /// <summary>Gets a value indicating whether content localization is enabled.</summary>
    public bool EnableContentLocalization { get; }

    /// <summary>
    ///   Gets a value indicating whether the installation runs in debug mode. This property can be used
    ///   by the framework and extensions alike to write more verbose logs/onscreen
    ///   information, etc. It is set in the host settings page.
    /// </summary>
    public bool DebugMode { get; }

    /// <summary>Gets a value indicating whether a CSS class based on the Module Name is automatically rendered.</summary>
    public bool EnableCustomModuleCssClass { get; }

    /// <summary>Gets a value indicating whether force upgrade wizard open in ssl channel.</summary>
    public bool UpgradeForceSsl { get; }

    /// <summary>Gets the domain used when upgrade wizard forced to shown in ssl channel.</summary>
    public string SslDomain { get; }

    /// <summary>Gets a value indicating whether File AutoSync is Enabled.</summary>
    public bool EnableFileAutoSync { get; }

    /// <summary>Gets a value indicating whether enable whether the IP address of the user is checked against a list during login.</summary>
    public bool EnableIPChecking { get; }

    /// <summary>Gets a value indicating whether Module Online Help is Enabled.</summary>
    public bool EnableModuleOnLineHelp { get; }

    /// <summary>Gets a value indicating whether the Request Filters are Enabled.</summary>
    public bool EnableRequestFilters { get; }

    /// <summary>Gets a value indicating whether enable whether a client-side password strength meter is shown on registration screen.</summary>
    public bool EnableStrengthMeter { get; }

    /// <summary>Gets a value indicating whether enable whether a previous passwords are stored to check if user is reusing them.</summary>
    public bool EnablePasswordHistory { get; }

    /// <summary>Gets a value indicating whether to use the Language in the Url.</summary>
    public bool EnableUrlLanguage { get; }

    /// <summary>Gets a value indicating whether the Event Log Buffer is Enabled.</summary>
    public bool EventLogBuffer { get; }

    /// <summary>Gets the allowed file extensions.</summary>
    public IFileExtensionAllowList AllowedExtensionAllowList { get; }

    /// <summary>Gets default list of extensions an end user is allowed to upload.</summary>
    public IFileExtensionAllowList DefaultEndUserExtensionAllowList { get; }

    /// <summary>Gets the host GUID.</summary>
    public string Guid { get; }

    /// <summary>Gets the Help URL.</summary>
    public string HelpUrl { get; }

    /// <summary>Gets the Host Email.</summary>
    public string HostEmail { get; }

    /// <summary>Gets the Host Portal's PortalId.</summary>
    public int HostPortalId { get; }

    /// <summary>Gets the Host Space.</summary>
    public double HostSpace { get; }

    /// <summary>Gets the Host Title.</summary>
    public string HostTitle { get; }

    /// <summary>Gets the Host URL.</summary>
    public string HostUrl { get; }

    /// <summary>Gets the HttpCompression Algorithm.</summary>
    public int HttpCompressionAlgorithm { get; }

    /// <summary>Gets size of the batch used to determine how many emails are sent per CoreMessaging Scheduler run.</summary>
    public int MessageSchedulerBatchSize { get; }

    /// <summary>Gets set length of time that reset links are valid for - default is 60 minutes.</summary>
    public TimeSpan MembershipResetLinkValidity { get; }

    /// <summary>Gets set length of time that reset links are valid for - default is 24 hours (1440 min).</summary>
    public TimeSpan AdminMembershipResetLinkValidity { get; }

    /// <summary>Gets set number of passwords stored for password change comparison operations - default is 5.</summary>
    public int MembershipNumberPasswords { get; }

    /// <summary>Gets the number of days that must pass before a password can be reused - default is 0 (i.e. password reuse is only governed by <see cref="EnablePasswordHistory"/> and <see cref="MembershipNumberPasswords"/>).</summary>
    public int MembershipDaysBeforePasswordReuse { get; }

    /// <summary>
    /// Gets the HTTP Status code returned if IP address filtering is enabled on login
    /// and the users IP does not meet criteria -default is 403.
    /// </summary>
    public string MembershipFailedIPException { get; }

    /// <summary>Gets the Module Caching method.</summary>
    public string ModuleCachingMethod { get; }

    /// <summary>Gets the Page Caching method.</summary>
    public string PageCachingMethod { get; }

    /// <summary>Gets the Page Quota.</summary>
    public int PageQuota { get; }

    /// <summary>Gets the PageState Persister.</summary>
    public string PageStatePersister { get; }

    /// <summary>Gets the Password Expiry.</summary>
    public TimeSpan PasswordExpiry { get; }

    /// <summary>Gets the Password Expiry Reminder window.</summary>
    public TimeSpan PasswordExpiryReminder { get; }

    /// <summary>Gets the Proxy Server Password.</summary>
    public string ProxyPassword { get; }

    /// <summary>Gets the Proxy Server Port.</summary>
    public int ProxyPort { get; }

    /// <summary>Gets the Proxy Server.</summary>
    public string ProxyServer { get; }

    /// <summary>Gets the Proxy Server UserName.</summary>
    public string ProxyUsername { get; }

    /// <summary>Gets a value indicating whether to use the remember me checkbox.</summary>
    public bool RememberCheckbox { get; }

    /// <summary>Gets the Scheduler Mode.</summary>
    public SchedulerMode SchedulerMode { get; }

    /// <summary>Gets the delayAtAppStart value.</summary>
    /// <remarks>Defaults is 1 min(60 sec).</remarks>
    public TimeSpan SchedulerDelayAtAppStart { get; }

    /// <summary>Gets a value indicating whether to include Common Words in the Search Index.</summary>
    public bool SearchIncludeCommon { get; }

    /// <summary>Gets a value indicating whether to include Numbers in the Search Index.</summary>
    public bool SearchIncludeNumeric { get; }

    /// <summary>Gets the maximum Search Word length to index.</summary>
    public int SearchMaxWordLength { get; }

    /// <summary>Gets the maximum Search Word length to index.</summary>
    public int SearchMinWordLength { get; }

    /// <summary>Gets the filter used for inclusion of tag info.</summary>
    public string SearchIncludedTagInfoFilter { get; }

    /// <summary>Gets a value indicating whether display the text of errors injected via the error querystring parameter.</summary>
    public bool ShowCriticalErrors { get; }

    /// <summary>Gets a value indicating whether Exceptions are rethrown.</summary>
    public bool ThrowCboExceptions { get; }

    /// <summary>Gets a value indicating whether Friendly Urls is Enabled.</summary>
    public bool UseFriendlyUrls { get; }

    /// <summary>Gets a value indicating whether Custom Error Messages is Enabled.</summary>
    public bool UseCustomErrorMessages { get; }

    /// <summary>Gets the User Quota.</summary>
    public int UserQuota { get; }

    /// <summary>Gets the WebRequest Timeout value.</summary>
    public TimeSpan WebRequestTimeout { get; }

    /// <summary>Gets a value indicating whether to use a hosted version of the MS Ajax Library.</summary>
    public bool EnableMsAjaxCdn { get; }

    /// <summary>Gets the time, in seconds, before asynchronous postbacks time out if no response is received.</summary>
    public TimeSpan AsyncTimeout { get; }

    /// <summary>Gets a value indicating whether to put the entire instance into maintenance mode.</summary>
    public bool IsLocked { get; }

    /// <summary>Gets or sets the PerformanceSettings.</summary>
    public PerformanceSettings PerformanceSetting { get; set; }

    /// <summary>Gets a value indicating whether SSL is Enabled for SMTP.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>Whether SSL is enabled for SMTP in the given portal.</returns>
    public bool EnableSmtpSsl(int portalId);

    /// <summary>Gets the currently configured SMTP OAuth provider if existing, for the current portal if portal SMTP enabled, otherwise for the installation.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The name of the SMTP OAuth provider.</returns>
    public string SmtpAuthProvider(int portalId);

    /// <summary>Gets the SMTP Authentication type.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authentication type indicator, <c>"1"</c> for basic auth, <c>"2"</c> for NTLM auth, <c>"3"</c> for OAuth, any other value for anonymous auth.</returns>
    public string SmtpAuthentication(int portalId);

    /// <summary>Gets the SMTP Password.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The SMTP password.</returns>
    public string SmtpPassword(int portalId);

    /// <summary>Gets the SMTP Server.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The SMTP server.</returns>
    public string SmtpServer(int portalId);

    /// <summary>Gets the SMTP Username.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The SMTP username.</returns>
    public string SmtpUsername(int portalId);

    /// <summary>Gets the SMTP Connection Limit.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The SMTP connection limit.</returns>
    public int SmtpConnectionLimit(int portalId);

    /// <summary>Gets the SMTP MaxIdleTime.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The SMTP maximum idle time.</returns>
    public TimeSpan SmtpMaxIdleTime(int portalId);

    /// <summary>Gets a value indicating whether SMTP information is stored at the portal level.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>Whether SMTP information is stored per portal.</returns>
    public bool SmtpPortalEnabled(int portalId);
}
