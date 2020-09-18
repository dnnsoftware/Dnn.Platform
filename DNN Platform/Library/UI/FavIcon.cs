// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Internals
{
    using System;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// Manages the FavIcon of a portal.
    /// </summary>
    public class FavIcon
    {
        private const string SettingName = "FavIconPath";

        private readonly int _portalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavIcon"/> class.
        /// Initializes a FavIcon instance.
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        public FavIcon(int portalId)
        {
            this._portalId = portalId;
        }

        /// <summary>
        /// Get the HTML for a favicon link.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>The HTML for the favicon link for the portal, or an empty string if there is no favicon.</returns>
        public static string GetHeaderLink(int portalId)
        {
            string headerLink;
            object fromCache = DataCache.GetCache(GetCacheKey(portalId));

            if (fromCache == null)
            {
                // only create an instance of FavIcon when there is a cache miss
                string favIconPath = new FavIcon(portalId).GetRelativeUrl();
                if (!string.IsNullOrEmpty(favIconPath))
                {
                    headerLink = string.Format("<link rel='SHORTCUT ICON' href='{0}' type='image/x-icon' />", favIconPath);
                }
                else
                {
                    headerLink = string.Empty;
                }

                // cache link or empty string to ensure we don't always have a
                // cache miss when no favicon is in use
                UpdateCachedHeaderLink(portalId, headerLink);
            }
            else
            {
                headerLink = fromCache.ToString();
            }

            return headerLink;
        }

        /// <summary>
        /// Get the path of the favicon file relative to the portal root.
        /// </summary>
        /// <remarks>This relative path is only relevant to use with Host/Portal Settings the path is not guaranteed any
        /// physical relevance in the local file system.</remarks>
        /// <returns>Path to the favicon file relative to portal root, or empty string when there is no favicon set.</returns>
        public string GetSettingPath()
        {
            return PortalController.GetPortalSetting(SettingName, this._portalId, string.Empty);
        }

        /// <summary>
        /// Update the file to use for a favIcon.
        /// </summary>
        /// <param name="fileId">The file id or Null.NullInteger for none.</param>
        public void Update(int fileId)
        {
            PortalController.UpdatePortalSetting(this._portalId, SettingName, fileId != Null.NullInteger ? string.Format("FileID={0}", fileId) : string.Empty, /*clearCache*/ true);
            DataCache.ClearCache(GetCacheKey(this._portalId));
        }

        private static void UpdateCachedHeaderLink(int portalId, string headerLink)
        {
            if (Host.PerformanceSetting != Globals.PerformanceSettings.NoCaching)
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
                return FileManager.Instance.GetFile(this._portalId, path);
            }

            return null;
        }
    }
}
