// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Collections;
    using System.IO;

    using DotNetNuke.Data;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

    public class UrlController
    {
        public ArrayList GetUrls(int PortalID)
        {
            return CBO.FillCollection(DataProvider.Instance().GetUrls(PortalID), typeof(UrlInfo));
        }

        public UrlInfo GetUrl(int PortalID, string Url)
        {
            return CBO.FillObject<UrlInfo>(DataProvider.Instance().GetUrl(PortalID, Url));
        }

        public UrlTrackingInfo GetUrlTracking(int PortalID, string Url, int ModuleId)
        {
            return CBO.FillObject<UrlTrackingInfo>(DataProvider.Instance().GetUrlTracking(PortalID, Url, ModuleId));
        }

        public void UpdateUrl(int PortalID, string Url, string UrlType, bool LogActivity, bool TrackClicks, int ModuleID, bool NewWindow)
        {
            this.UpdateUrl(PortalID, Url, UrlType, 0, Null.NullDate, Null.NullDate, LogActivity, TrackClicks, ModuleID, NewWindow);
        }

        public void UpdateUrl(int PortalID, string Url, string UrlType, int Clicks, DateTime LastClick, DateTime CreatedDate, bool LogActivity, bool TrackClicks, int ModuleID, bool NewWindow)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                if (UrlType == "U")
                {
                    if (this.GetUrl(PortalID, Url) == null)
                    {
                        DataProvider.Instance().AddUrl(PortalID, Url.Replace(@"\", @"/"));
                    }
                }

                UrlTrackingInfo objURLTracking = this.GetUrlTracking(PortalID, Url, ModuleID);
                if (objURLTracking == null)
                {
                    DataProvider.Instance().AddUrlTracking(PortalID, Url, UrlType, LogActivity, TrackClicks, ModuleID, NewWindow);
                }
                else
                {
                    DataProvider.Instance().UpdateUrlTracking(PortalID, Url, LogActivity, TrackClicks, ModuleID, NewWindow);
                }
            }
        }

        public void DeleteUrl(int PortalID, string Url)
        {
            DataProvider.Instance().DeleteUrl(PortalID, Url);
        }

        public void UpdateUrlTracking(int PortalID, string Url, int ModuleId, int UserID)
        {
            TabType UrlType = Globals.GetURLType(Url);
            if (UrlType == TabType.File && Url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase) == false)
            {
                // to handle legacy scenarios before the introduction of the FileServerHandler
                var fileName = Path.GetFileName(Url);

                var folderPath = Url.Substring(0, Url.LastIndexOf(fileName));
                var folder = FolderManager.Instance.GetFolder(PortalID, folderPath);

                var file = FileManager.Instance.GetFile(folder, fileName);

                Url = "FileID=" + file.FileId;
            }

            UrlTrackingInfo objUrlTracking = this.GetUrlTracking(PortalID, Url, ModuleId);
            if (objUrlTracking != null)
            {
                if (objUrlTracking.TrackClicks)
                {
                    DataProvider.Instance().UpdateUrlTrackingStats(PortalID, Url, ModuleId);
                    if (objUrlTracking.LogActivity)
                    {
                        if (UserID == -1)
                        {
                            UserID = UserController.Instance.GetCurrentUserInfo().UserID;
                        }

                        DataProvider.Instance().AddUrlLog(objUrlTracking.UrlTrackingID, UserID);
                    }
                }
            }
        }

        public ArrayList GetUrlLog(int PortalID, string Url, int ModuleId, DateTime StartDate, DateTime EndDate)
        {
            ArrayList arrUrlLog = null;
            UrlTrackingInfo objUrlTracking = this.GetUrlTracking(PortalID, Url, ModuleId);
            if (objUrlTracking != null)
            {
                arrUrlLog = CBO.FillCollection(DataProvider.Instance().GetUrlLog(objUrlTracking.UrlTrackingID, StartDate, EndDate), typeof(UrlLogInfo));
            }

            return arrUrlLog;
        }
    }
}
