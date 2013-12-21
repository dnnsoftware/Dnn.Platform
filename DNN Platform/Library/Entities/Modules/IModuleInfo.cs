using System;
namespace DotNetNuke.Entities.Modules
{
    public interface IModuleInfo
    {
        string Alignment { get; set; }
        bool AllModules { get; set; }
        bool AllTabs { get; set; }
        string AuthorizedEditRoles { get; }
        string AuthorizedViewRoles { get; }
        string Border { get; set; }
        string BusinessControllerClass { get; set; }
        DotNetNuke.Services.Tokens.CacheLevel Cacheability { get; }
        string CacheMethod { get; set; }
        int CacheTime { get; set; }
        ModuleInfo Clone();
        string Color { get; set; }
        string CompatibleVersions { get; set; }
        string ContainerPath { get; set; }
        string ContainerSrc { get; set; }
        string ControlSrc { get; set; }
        string ControlTitle { get; set; }
        DotNetNuke.Security.SecurityAccessLevel ControlType { get; set; }
        string CultureCode { get; set; }
        int DefaultCacheTime { get; set; }
        Guid DefaultLanguageGuid { get; set; }
        ModuleInfo DefaultLanguageModule { get; }
        string Dependencies { get; set; }
        string Description { get; set; }
        DesktopModuleInfo DesktopModule { get; }
        int DesktopModuleID { get; set; }
        bool DisplayPrint { get; set; }
        bool DisplaySyndicate { get; set; }
        bool DisplayTitle { get; set; }
        DateTime EndDate { get; set; }
        void Fill(System.Data.IDataReader dr);
        string FolderName { get; set; }
        string Footer { get; set; }
        string FriendlyName { get; set; }
        string GetEffectiveCacheMethod();
        string GetProperty(string propertyName, string format, System.Globalization.CultureInfo formatProvider, DotNetNuke.Entities.Users.UserInfo accessingUser, DotNetNuke.Services.Tokens.Scope currentScope, ref bool propertyNotFound);
        string Header { get; set; }
        string HelpUrl { get; set; }
        bool HideAdminBorder { get; }
        string IconFile { get; set; }
        bool InheritViewPermissions { get; set; }
        void Initialize(int portalId);
        bool IsAdmin { get; set; }
        bool IsDefaultLanguage { get; }
        bool IsDefaultModule { get; set; }
        bool IsDeleted { get; set; }
        bool IsLocalized { get; }
        bool IsNeutralCulture { get; }
        bool IsPortable { get; }
        bool IsPremium { get; set; }
        bool IsSearchable { get; }
        bool IsShareable { get; set; }
        bool IsShareableViewOnly { get; set; }
        bool IsShared { get; }
        bool IsTranslated { get; }
        bool IsUpgradeable { get; }
        bool IsWebSlice { get; set; }
        int KeyID { get; set; }
        DateTime LastContentModifiedOnDate { get; set; }
        System.Collections.Generic.Dictionary<string, ModuleInfo> LocalizedModules { get; }
        Guid LocalizedVersionGuid { get; set; }
        ModuleControlInfo ModuleControl { get; }
        int ModuleControlId { get; set; }
        int ModuleDefID { get; set; }
        DotNetNuke.Entities.Modules.Definitions.ModuleDefinitionInfo ModuleDefinition { get; }
        string ModuleName { get; set; }
        int ModuleOrder { get; set; }
        DotNetNuke.Security.Permissions.ModulePermissionCollection ModulePermissions { get; set; }
        System.Collections.Hashtable ModuleSettings { get; }
        string ModuleTitle { get; set; }
        int OwnerPortalID { get; set; }
        int PaneModuleCount { get; set; }
        int PaneModuleIndex { get; set; }
        string PaneName { get; set; }
        DotNetNuke.Entities.Tabs.TabInfo ParentTab { get; }
        string Permissions { get; set; }
        int PortalID { get; set; }
        DateTime StartDate { get; set; }
        int SupportedFeatures { get; set; }
        bool SupportsPartialRendering { get; set; }
        int TabModuleID { get; set; }
        System.Collections.Hashtable TabModuleSettings { get; }
        Guid UniqueId { get; set; }
        string Version { get; set; }
        Guid VersionGuid { get; set; }
        VisibilityState Visibility { get; set; }
        DateTime WebSliceExpiryDate { get; set; }
        string WebSliceTitle { get; set; }
        int WebSliceTTL { get; set; }
    }
}
