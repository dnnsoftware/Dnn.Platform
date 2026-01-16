// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Internals
{
    using System.Globalization;
    using System.Net;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;

    /// <summary>Manages the FavIcon of a portal.</summary>
    public class FavIcon
    {
        private const string SettingName = "FavIconPath";

        private readonly int portalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavIcon"/> class.
        /// Initializes a FavIcon instance.
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        public FavIcon(int portalId)
        {
            this.portalId = portalId;
        }

        /// <summary>Get the HTML for a favicon link.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The HTML for the favicon link for the portal, or an empty string if there is no favicon.</returns>
        public static string GetHeaderLink(IHostSettings hostSettings, int portalId)
        {
            string headerLink;
            object fromCache = DataCache.GetCache(GetCacheKey(portalId));

            if (fromCache == null)
            {
                // only create an instance of FavIcon when there is a cache miss
                string favIconPath = new FavIcon(portalId).GetRelativeUrl();
                if (!string.IsNullOrEmpty(favIconPath))
                {
                    headerLink = $"<link rel='icon' href='{WebUtility.HtmlEncode(favIconPath)}' type='image/x-icon' />";
                }
                else
                {
                    headerLink = string.Empty;
                }

                // cache link or empty string to ensure we don't always have a
                // cache miss when no favicon is in use
                UpdateCachedHeaderLink(hostSettings, portalId, headerLink);
            }
            else
            {
                headerLink = fromCache.ToString();
            }

            return headerLink;
        }

        /// <summary>Get the path of the favicon file relative to the portal root.</summary>
        /// <remarks>This relative path is only relevant to use with Host/Portal Settings the path is not guaranteed any
        /// physical relevance in the local file system.</remarks>
        /// <returns>Path to the favicon file relative to portal root, or empty string when there is no favicon set.</returns>
        public string GetSettingPath()
        {
            return PortalController.GetPortalSetting(SettingName, this.portalId, string.Empty);
        }

        /// <summary>Update the file to use for a favIcon.</summary>
        /// <param name="fileId">The file id or Null.NullInteger for none.</param>
        public void Update(int fileId)
        {
            var settingValue = fileId != Null.NullInteger ? string.Format(CultureInfo.InvariantCulture, "FileID={0}", fileId) : string.Empty;
            PortalController.UpdatePortalSetting(this.portalId, SettingName, settingValue, clearCache: true);
            DataCache.ClearCache(GetCacheKey(this.portalId));
        }

        private static void UpdateCachedHeaderLink(IHostSettings hostSettings, int portalId, string headerLink)
        {
            if (hostSettings.PerformanceSetting != PerformanceSettings.NoCaching)
            {
                DataCache.SetCache(GetCacheKey(portalId), headerLink);
            }
        }

        private static string GetCacheKey(int portalId)
        {
            return "FAVICON" + portalId;
        }

        private string GetRelativeUrl()
        {
            var fileInfo = this.GetFileInfo();
            return fileInfo == null ? string.Empty : FileManager.Instance.GetUrl(fileInfo);
        }

        private IFileInfo GetFileInfo()
        {
            var path = this.GetSettingPath();
            if (!string.IsNullOrEmpty(path))
            {
                return FileManager.Instance.GetFile(this.portalId, path);
            }

            return null;
        }
    }
}
