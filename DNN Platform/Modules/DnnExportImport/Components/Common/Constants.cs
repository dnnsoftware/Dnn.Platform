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
using System.Web.Caching;
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

        internal const string ExportFolder = @"\Install\ExportImport\";
        internal const string ExportDateFormat = "yyyyMMdd-HHmmss";
        internal const string ExportDbExt = ".dnndb"; // exportDB file extension
        internal const string ExportZipExt = ".resources"; // zipped file extension to prevent downloading

        internal const string LogTypeSiteExport = "SITE_EXPORT";
        internal const string LogTypeSiteImport = "SITE_IMPORT";

        internal const string JobRunDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        internal const string LastJobStartTimeKey = "EXPORT_LastJobStartTime";
        internal const string MaxTimeToRunJobKey = "EXPORT_MaxTimeToRunJob";
        
        internal const int LogColumnLength = 255;

        internal static DateTime MinDbTime = new DateTime(2000, 1, 1, 0, 0 , 0, DateTimeKind.Utc);
        internal static DateTime MaxDbTime = new DateTime(3000, 1, 1, 0, 0 , 0, DateTimeKind.Utc);

        internal const string SharedResources = "/DesktopModules/SiteExportImport/App_LocalResources/ExportImport.resx";

        // these are set by the API caller
        public const string Category_Users = "USERS";
        public const string Category_Vocabularies = "VOCABULARIES";
        public const string Category_Roles = "ROLES";
        public const string Category_ProfileProps = "PROFILE_PROPERTIES";
        public const string Category_Content = "CONTENT";
        public const string Category_tEMPLATES = "TEMPLATES";
        public const string Category_Assets = "ASSETS";

        // these are added internally by the engine
        internal const string Category_Portal = "PORTAL";
        internal const string Category_Pages = "PAGES";
        internal const string Category_UsersData = "USERS_DATA";
    }
}
