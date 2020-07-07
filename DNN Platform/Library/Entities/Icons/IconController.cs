// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Icons
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.Hosting;
    using System.Xml;

    using DotNetNuke.Collections.Internal;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// IconController provides all operation to icons.
    /// </summary>
    /// <remarks>
    /// Tab is equal to page in DotNetNuke.
    /// Tabs will be a sitemap for a poatal, and every request at first need to check whether there is valid tab information
    /// include in the url, if not it will use default tab to display information.
    /// </remarks>
    public class IconController
    {
        public const string DefaultIconSize = "16X16";
        public const string DefaultLargeIconSize = "32X32";
        public const string DefaultIconStyle = "Standard";
        public const string IconKeyName = "IconKey";
        public const string IconSizeName = "IconSize";
        public const string IconStyleName = "IconStyle";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(IconController));

        private static readonly SharedDictionary<string, bool> _iconsStatusOnDisk = new SharedDictionary<string, bool>();

        /// <summary>
        /// Gets the Icon URL.
        /// </summary>
        /// <param name="key">Key to icon, e.g. edit.</param>
        /// <returns>Link to the image, e.g. /Icons/Sigma/edit_16x16_standard.png.</returns>
        public static string IconURL(string key)
        {
            return IconURL(key, DefaultIconSize, DefaultIconStyle);
        }

        /// <summary>
        /// Gets the Icon URL.
        /// </summary>
        /// <param name="key">Key to icon, e.g. edit.</param>
        /// <param name="size">Size of icon, e.g.16x16 (default) or 32x32.</param>
        /// <returns>Link to the image, e.g. /Icons/Sigma/edit_16x16_standard.png.</returns>
        public static string IconURL(string key, string size)
        {
            return IconURL(key, size, DefaultIconStyle);
        }

        /// <summary>
        /// Gets the Icon URL.
        /// </summary>
        /// <param name="key">Key to icon, e.g. edit.</param>
        /// <param name="size">Size of icon, e.g.16x16 (default) or 32x32.</param>
        /// <param name="style">Style of icon, e.g. Standard (default).</param>
        /// <returns>Link to the image, e.g. /Icons/Sigma/edit_16x16_standard.png.</returns>
        public static string IconURL(string key, string size, string style)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(size))
            {
                size = DefaultIconSize;
            }

            if (string.IsNullOrEmpty(style))
            {
                style = DefaultIconStyle;
            }

            string fileName = string.Format("{0}/{1}_{2}_{3}.png", PortalSettings.Current.DefaultIconLocation, key, size, style);

            // In debug mode, we want to warn (onluy once) if icon is not present on disk
#if DEBUG
            CheckIconOnDisk(fileName);
#endif
            return Globals.ApplicationPath + "/" + fileName;
        }

        public static string GetFileIconUrl(string extension)
        {
            if (!string.IsNullOrEmpty(extension) && File.Exists(HostingEnvironment.MapPath(IconURL("Ext" + extension, "32x32", "Standard"))))
            {
                return IconURL("Ext" + extension, "32x32", "Standard");
            }

            return IconURL("ExtFile", "32x32", "Standard");
        }

        public static string[] GetIconSets()
        {
            string iconPhysicalPath = Path.Combine(Globals.ApplicationMapPath, "icons");
            var iconRootDir = new DirectoryInfo(iconPhysicalPath);
            string result = string.Empty;
            foreach (var iconDir in iconRootDir.EnumerateDirectories())
            {
                string testFile = Path.Combine(iconDir.FullName, "About_16x16_Standard.png");
                if (File.Exists(testFile))
                {
                    result += iconDir.Name + ",";
                }
            }

            return result.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static void CheckIconOnDisk(string path)
        {
            using (_iconsStatusOnDisk.GetReadLock())
            {
                if (_iconsStatusOnDisk.ContainsKey(path))
                {
                    return;
                }
            }

            using (_iconsStatusOnDisk.GetWriteLock())
            {
                if (!_iconsStatusOnDisk.ContainsKey(path))
                {
                    _iconsStatusOnDisk.Add(path, true);
                    string iconPhysicalPath = Path.Combine(Globals.ApplicationMapPath, path.Replace('/', '\\'));
                    if (!File.Exists(iconPhysicalPath))
                    {
                        Logger.WarnFormat(string.Format("Icon Not Present on Disk {0}", iconPhysicalPath));
                    }
                }
            }
        }
    }
}
