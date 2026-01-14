// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace Dnn.ExportImport.Components.Common
{
    using System;

    /// <summary>Lists constants used across the library.</summary>
    public class Constants
    {
        /// <summary>
        /// This is the currently supported schema version support as of this release.
        /// In future releases this must be updated to be compatible with the schema version.
        /// </summary>
        public const string CurrentSchemaVersion = "1.0.2";

        // these are set by the API caller
#pragma warning disable SA1310 // Field names should not contain underscore, bypassing this warning in case something unexpected consumes these.
#pragma warning disable CA1707 // Identifiers should not contain underscores

        /// <summary>The name of the users category.</summary>
        public const string Category_Users = "USERS";

        /// <summary>The name of the vocabularies category.</summary>
        public const string Category_Vocabularies = "VOCABULARIES";

        /// <summary>The name of the roles category.</summary>
        public const string Category_Roles = "ROLES";

        /// <summary>The name of the profile properties category.</summary>
        public const string Category_ProfileProps = "PROFILE_PROPERTIES";

        /// <summary>The name of the content category.</summary>
        public const string Category_Content = "CONTENT";

        /// <summary>The name of the templates category.</summary>
        public const string Category_Templates = "TEMPLATES";

        /// <summary>The name of the assets category.</summary>
        public const string Category_Assets = "ASSETS";

        /// <summary>The name of the packages category.</summary>
        public const string Category_Packages = "PACKAGES";

        /// <summary>The name of the themes category.</summary>
        public const string Category_Themes = "THEMES";

        /// <summary>The name of the workflows category.</summary>
        public const string Category_Workflows = "WORKFLOW";

        /// <summary>The name of the relative path used for export/import.</summary>
        internal const string ExportFolder = @"\App_Data\ExportImport\";

        /// <summary>The filename of the export manifest.</summary>
        internal const string ExportManifestName = "export.json";

        /// <summary>The filename of the export database.</summary>
        internal const string ExportDbName = "export.dnndb";

        /// <summary>The filename of the compressed database.</summary>
        internal const string ExportZipDbName = "export_db.zip";

        /// <summary>The filename of the compressed assets.</summary>
        internal const string ExportZipFiles = "export_files.zip";

        /// <summary>The filename of the compressed templates.</summary>
        internal const string ExportZipTemplates = "export_templates.zip";

        /// <summary>The filename of the compressed extension packages.</summary>
        internal const string ExportZipPackages = "export_packages.zip";

        /// <summary>The name of the users category.</summary>
        internal const string ExportZipThemes = "export_themes.zip";

        /// <summary>The export log type.</summary>
        internal const string LogTypeSiteExport = "SITE_EXPORT";

        /// <summary>The import log type.</summary>
        internal const string LogTypeSiteImport = "SITE_IMPORT";

        /// <summary>The date/time format for the last job start time setting.</summary>
        internal const string JobRunDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>The key of the schedule setting for the last job start time.</summary>
        internal const string LastJobStartTimeKey = "EXPORT_LastJobStartTime";

        /// <summary>The key of the import/export setting for the maximum number of seconds to run the job.</summary>
        internal const string MaxSecondsToRunJobKey = "MaxSecondsToRunJob";

        /// <summary>The key of the import/export setting for the portal settings to include in the export.</summary>
        internal const string PortalSettingExportKey = "PortalSettingExportList";

        /// <summary>The file extension for template files.</summary>
        internal const string TemplatesExtension = "template";

        /// <summary>The default page size when retrieving data.</summary>
        internal const int DefaultPageSize = 1000;

        /// <summary>The log column length.</summary>
        internal const int LogColumnLength = 255;

        /// <summary>The path to the shared resources file.</summary>
        internal const string SharedResources = "/DesktopModules/SiteExportImport/App_LocalResources/ExportImport.resx";

        // these are added internally by the engine

        /// <summary>The name of the portal category.</summary>
        internal const string Category_Portal = "PORTAL";

        /// <summary>The name of the pages category.</summary>
        internal const string Category_Pages = "PAGES";

        /// <summary>The name of the users data category.</summary>
        internal const string Category_UsersData = "USERS_DATA";

        /// <summary>The minimum date/time supported in the database.</summary>
        internal static readonly DateTime MinDbTime = new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>The maximum date/time supported in the database.</summary>
        internal static readonly DateTime MaxDbTime = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>The maximum zip file size in bytes (100MB).</summary>
        internal static readonly int MaxZipFilesMemory = 104857600;
#pragma warning restore SA1310 // Field names should not contain underscore
#pragma warning restore CA1707
    }
}
