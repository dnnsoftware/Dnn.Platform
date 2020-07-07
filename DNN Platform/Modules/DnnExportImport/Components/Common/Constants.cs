// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Components.Common
{
    using System;

    public class Constants
    {
        /// <summary>
        /// This is the currently supported schema version support as of this release.
        /// In future releases thi must be updated to be compatible wiht th e.
        /// </summary>
        public const string CurrentSchemaVersion = "1.0.2";

        // these are set by the API caller
        public const string Category_Users = "USERS";
        public const string Category_Vocabularies = "VOCABULARIES";
        public const string Category_Roles = "ROLES";
        public const string Category_ProfileProps = "PROFILE_PROPERTIES";
        public const string Category_Content = "CONTENT";
        public const string Category_Templates = "TEMPLATES";
        public const string Category_Assets = "ASSETS";
        public const string Category_Packages = "PACKAGES";
        public const string Category_Themes = "THEMES";
        public const string Category_Workflows = "WORKFLOW";

        internal const string ExportFolder = @"\App_Data\ExportImport\";
        internal const string ExportManifestName = "export.json"; // export manifest file name
        internal const string ExportDbName = "export.dnndb"; // export database file name
        internal const string ExportZipDbName = "export_db.zip"; // export compressed database file name
        internal const string ExportZipFiles = "export_files.zip"; // Compressed assets file name
        internal const string ExportZipTemplates = "export_templates.zip"; // Compressed templates file name
        internal const string ExportZipPackages = "export_packages.zip"; // Compressed extension packages
        internal const string ExportZipThemes = "export_themes.zip"; // Compressed site used themes

        internal const string LogTypeSiteExport = "SITE_EXPORT";
        internal const string LogTypeSiteImport = "SITE_IMPORT";

        internal const string JobRunDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        internal const string LastJobStartTimeKey = "EXPORT_LastJobStartTime";
        internal const string MaxSecondsToRunJobKey = "MaxSecondsToRunJob";
        internal const string PortalSettingExportKey = "PortalSettingExportList";
        internal const string TemplatesExtension = "template";
        internal const int DefaultPageSize = 1000;

        internal const int LogColumnLength = 255;

        internal const string SharedResources = "/DesktopModules/SiteExportImport/App_LocalResources/ExportImport.resx";

        // these are added internally by the engine
        internal const string Category_Portal = "PORTAL";
        internal const string Category_Pages = "PAGES";
        internal const string Category_UsersData = "USERS_DATA";

        internal static DateTime MinDbTime = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static DateTime MaxDbTime = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static int MaxZipFilesMemory = 104857600; // 100 MB
    }
}
