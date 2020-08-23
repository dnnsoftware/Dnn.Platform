// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Portals
{
    using System;

    public interface IPortalSettings
    {
        //TabInfo ActiveTab { get; set; }
        bool AddCachebusterToResourceUris { get; }
        string AddCompatibleHttpHeader { get; }
        int AdministratorId { get; set; }
        int AdministratorRoleId { get; set; }
        string AdministratorRoleName { get; set; }
        int AdminTabId { get; set; }
        //FileExtensionWhitelist AllowedExtensionsWhitelist { get; }
        bool AllowUserUICulture { get; }
        string BackgroundFile { get; set; }
        int BannerAdvertising { get; set; }
        //CacheLevel Cacheability { get; }
        int CdfVersion { get; }
        //PortalSettings.ControlPanelPermission ControlPanelSecurity { get; }
        bool ControlPanelVisible { get; }
        string CookieMoreLink { get; }
        string CultureCode { get; set; }
        string Currency { get; set; }
        bool DataConsentActive { get; }
        int DataConsentConsentRedirect { get; }
        int DataConsentDelay { get; }
        string DataConsentDelayMeasurement { get; }
        DateTime DataConsentTermsLastChange { get; }
        //PortalSettings.UserDeleteAction DataConsentUserDeleteAction { get; }
        string DefaultAdminContainer { get; }
        string DefaultAdminSkin { get; }
        string DefaultAuthProvider { get; }
        //PortalSettings.Mode DefaultControlPanelMode { get; }
        bool DefaultControlPanelVisibility { get; }
        string DefaultIconLocation { get; }
        string DefaultLanguage { get; set; }
        string DefaultModuleActionMenu { get; }
        int DefaultModuleId { get; }
        string DefaultPortalAlias { get; }
        string DefaultPortalContainer { get; }
        string DefaultPortalSkin { get; }
        int DefaultTabId { get; }
        string Description { get; set; }
        bool DisablePrivateMessage { get; }
        string Email { get; set; }
        bool EnableBrowserLanguage { get; }
        bool EnableCompositeFiles { get; }
        bool EnableModuleEffect { get; }
        bool EnablePopUps { get; }
        bool EnableRegisterNotification { get; }
        bool EnableSkinWidgets { get; }

        bool ContentLocalizationEnabled { get; }
        int ErrorPage404 { get; }
        int ErrorPage500 { get; }
        DateTime ExpiryDate { get; set; }
        string FooterText { get; set; }
        Guid GUID { get; set; }
        bool HideFoldersEnabled { get; }
        bool HideLoginControl { get; }
        string HomeDirectory { get; set; }
        string HomeDirectoryMapPath { get; }
        string HomeSystemDirectory { get; set; }
        string HomeSystemDirectoryMapPath { get; }
        int HomeTabId { get; set; }
        float HostFee { get; set; }
        int HostSpace { get; set; }
        bool InjectModuleHyperLink { get; }
        bool InlineEditorEnabled { get; }
        bool IsLocked { get; }
        bool IsThisPortalLocked { get; }
        string KeyWords { get; set; }
        int LoginTabId { get; set; }
        string LogoFile { get; set; }
        string PageHeadText { get; }
        int PageQuota { get; set; }
        int Pages { get; set; }

        bool EnableUrlLanguage { get; }
        //IPortalAliasInfo PortalAlias { get; set; }
        //PortalSettings.PortalAliasMapping PortalAliasMappingMode { get; }
        int PortalId { get; set; }
        string PortalName { get; set; }
        //IPortalAliasInfo PrimaryAlias { get; set; }
        int PrivacyTabId { get; set; }
        int RegisteredRoleId { get; set; }
        string RegisteredRoleName { get; set; }
        int RegisterTabId { get; set; }
        //RegistrationSettings Registration { get; set; }
        bool SearchIncludeCommon { get; }
        string SearchIncludedTagInfoFilter { get; }
        bool SearchIncludeNumeric { get; }
        int SearchMaxWordlLength { get; }
        int SearchMinWordlLength { get; }
        int SearchTabId { get; set; }
        bool ShowCookieConsent { get; }
        int SiteLogHistory { get; set; }
        int SMTPConnectionLimit { get; }
        int SMTPMaxIdleTime { get; }
        int SplashTabId { get; set; }

        bool SSLEnabled { get; }
        bool SSLEnforced { get; }

        string SSLURL { get; }
        string STDURL { get; }
        int SuperTabId { get; set; }
        int TermsTabId { get; set; }
        TimeZoneInfo TimeZone { get; set; }
        int UserId { get; }
        //IUserInfo UserInfo { get; }
        //PortalSettings.Mode UserMode { get; }
        int UserQuota { get; set; }
        int UserRegistration { get; set; }
        int Users { get; set; }
        int UserTabId { get; set; }

        //IPortalSettings Clone();
        //string GetProperty(string propertyName, string format, CultureInfo formatProvider, UserInfo accessingUser, Scope accessLevel, ref bool propertyNotFound);
    }
}
