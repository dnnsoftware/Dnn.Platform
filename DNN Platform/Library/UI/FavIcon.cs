#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.UI.Internals
{
    /// <summary>
    /// Manages the FavIcon of a portal
    /// </summary>
    public class FavIcon
    {
        private const string SettingName = "FavIconPath";

        private readonly int _portalId;

        /// <summary>
        /// Initializes a FavIcon instance
        /// </summary>
        /// <param name="portalId">The id of the portal</param>
        public FavIcon(int portalId)
        {
            _portalId = portalId;
        }

        /// <summary>
        /// Get the path of the favicon file relative to the portal root
        /// </summary>
        /// <remarks>This relative path is only relevant to use with Host/Portal Settings the path is not guaranteed any 
        /// physical relevance in the local file system</remarks>
        /// <returns>Path to the favicon file relative to portal root, or empty string when there is no favicon set</returns>
        public string GetSettingPath()
        {
            return PortalController.GetPortalSetting(SettingName, _portalId, "");
        }

        /// <summary>
        /// Update the file to use for a favIcon
        /// </summary>
        /// <param name="fileId">The file id or Null.NullInteger for none</param>
        public void Update(int fileId)
        {
            PortalController.UpdatePortalSetting(_portalId, SettingName, fileId != Null.NullInteger ? string.Format("FileID={0}", fileId) : "", /*clearCache*/ true);
            DataCache.ClearCache(GetCacheKey(_portalId));
        }

        /// <summary>
        /// Get the HTML for a favicon link
        /// </summary>
        /// <param name="portalId">The portal id</param>
        /// <returns>The HTML for the favicon link for the portal, or an empty string if there is no favicon</returns>
        public static string GetHeaderLink(int portalId)
        {
            string headerLink;
            object fromCache = DataCache.GetCache(GetCacheKey(portalId));

            if (fromCache == null)
            {
                //only create an instance of FavIcon when there is a cache miss
                string favIconPath = new FavIcon(portalId).GetRelativeUrl();
                if (!string.IsNullOrEmpty(favIconPath))
                {
                    headerLink = string.Format("<link rel='SHORTCUT ICON' href='{0}' type='image/x-icon' />", favIconPath);
                }
                else
                {
                    headerLink = "";
                }

                //cache link or empty string to ensure we don't always have a
                //cache miss when no favicon is in use
                UpdateCachedHeaderLink(portalId, headerLink);
            }
            else
            {
                headerLink = fromCache.ToString();
            }

            return headerLink;
        }

        private string GetRelativeUrl()
        {
            var fileInfo = GetFileInfo();
            return fileInfo == null ? string.Empty : FileManager.Instance.GetUrl(fileInfo);
        }

        private IFileInfo GetFileInfo()
        {
            var path = GetSettingPath();
            if( ! String.IsNullOrEmpty(path) )
            { 
                return FileManager.Instance.GetFile(_portalId, path);
            }

            return null;
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
    }
}