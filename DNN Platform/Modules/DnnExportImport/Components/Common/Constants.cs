#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System;
// ReSharper disable InconsistentNaming

namespace Dnn.ExportImport.Components.Common
{
    public class Constants
    {
        /// <summary>
        /// This is the currently supported schema version support as of this release.
        /// In future releases thi must be updated to be compatible wiht th e
        /// </summary>
        public const string CurrentSchemaVersion = "1.0.0";

        internal const string ExportFolder = @"\App_Data\ExportImport\";
        internal const string ExportManifestName = "export.json"; // export manifest file name
        internal const string ExportDbName = "export.dnndb"; // export database file name
        internal const string ExportZipDbName = "export_db.zip"; // export compressed database file name
        internal const string ExportZipFiles = "export_files.zip"; //Compressed assets file name
        internal const string ExportZipTemplates = "export_templates.zip"; //Compressed templates file name
        internal const string ExportZipPackages = "export_packages.zip"; //Compressed extension packages

        internal const string LogTypeSiteExport = "SITE_EXPORT";
        internal const string LogTypeSiteImport = "SITE_IMPORT";

        internal const string JobRunDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        internal const string LastJobStartTimeKey = "EXPORT_LastJobStartTime";
        internal const string MaxTimeToRunJobKey = "EXPORT_MaxTimeToRunJob";
        internal const string TemplatesExtension = "template";
        internal const int DefaultPageSize = 1000;

        internal const int LogColumnLength = 255;

        internal static DateTime EpochTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static DateTime MinDbTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static DateTime MaxDbTime = new DateTime(3000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static int MaxZipFilesMemory = 104857600;//100 MB

        internal const string SharedResources = "/DesktopModules/SiteExportImport/App_LocalResources/ExportImport.resx";

        // these are set by the API caller
        public const string Category_Users = "USERS";
        public const string Category_Vocabularies = "VOCABULARIES";
        public const string Category_Roles = "ROLES";
        public const string Category_ProfileProps = "PROFILE_PROPERTIES";
        public const string Category_Content = "CONTENT";
        public const string Category_Templates = "TEMPLATES";
        public const string Category_Assets = "ASSETS";
        public const string Category_Packages = "PACKAGES";

        // these are added internally by the engine
        internal const string Category_Portal = "PORTAL";
        internal const string Category_Pages = "PAGES";
        internal const string Category_UsersData = "USERS_DATA";
    }
}
