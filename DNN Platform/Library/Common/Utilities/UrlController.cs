// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities;

using System;
using System.Collections;
using System.IO;

using DotNetNuke.Data;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.FileSystem;

public class UrlController
{
    public ArrayList GetUrls(int portalID)
    {
        return CBO.FillCollection(DataProvider.Instance().GetUrls(portalID), typeof(UrlInfo));
    }

    public UrlInfo GetUrl(int portalID, string url)
    {
        return CBO.FillObject<UrlInfo>(DataProvider.Instance().GetUrl(portalID, url));
    }

    public UrlTrackingInfo GetUrlTracking(int portalID, string url, int moduleId)
    {
        return CBO.FillObject<UrlTrackingInfo>(DataProvider.Instance().GetUrlTracking(portalID, url, moduleId));
    }

    public void UpdateUrl(int portalID, string url, string urlType, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
    {
        this.UpdateUrl(portalID, url, urlType, 0, Null.NullDate, Null.NullDate, logActivity, trackClicks, moduleID, newWindow);
    }

    public void UpdateUrl(int portalID, string url, string urlType, int clicks, DateTime lastClick, DateTime createdDate, bool logActivity, bool trackClicks, int moduleID, bool newWindow)
    {
        if (!string.IsNullOrEmpty(url))
        {
            if (urlType == "U")
            {
                if (this.GetUrl(portalID, url) == null)
                {
                    DataProvider.Instance().AddUrl(portalID, url.Replace(@"\", @"/"));
                }
            }

            UrlTrackingInfo objURLTracking = this.GetUrlTracking(portalID, url, moduleID);
            if (objURLTracking == null)
            {
                DataProvider.Instance().AddUrlTracking(portalID, url, urlType, logActivity, trackClicks, moduleID, newWindow);
            }
            else
            {
                DataProvider.Instance().UpdateUrlTracking(portalID, url, logActivity, trackClicks, moduleID, newWindow);
            }
        }
    }

    public void DeleteUrl(int portalID, string url)
    {
        DataProvider.Instance().DeleteUrl(portalID, url);
    }

    public void UpdateUrlTracking(int portalID, string url, int moduleId, int userID)
    {
        TabType urlType = Globals.GetURLType(url);
        if (urlType == TabType.File && url.StartsWith("fileid=", StringComparison.InvariantCultureIgnoreCase) == false)
        {
            // to handle legacy scenarios before the introduction of the FileServerHandler
            var fileName = Path.GetFileName(url);

            var folderPath = url.Substring(0, url.LastIndexOf(fileName));
            var folder = FolderManager.Instance.GetFolder(portalID, folderPath);

            var file = FileManager.Instance.GetFile(folder, fileName);

            url = "FileID=" + file.FileId;
        }

        UrlTrackingInfo objUrlTracking = this.GetUrlTracking(portalID, url, moduleId);
        if (objUrlTracking != null)
        {
            if (objUrlTracking.TrackClicks)
            {
                DataProvider.Instance().UpdateUrlTrackingStats(portalID, url, moduleId);
                if (objUrlTracking.LogActivity)
                {
                    if (userID == -1)
                    {
                        userID = UserController.Instance.GetCurrentUserInfo().UserID;
                    }

                    DataProvider.Instance().AddUrlLog(objUrlTracking.UrlTrackingID, userID);
                }
            }
        }
    }

    public ArrayList GetUrlLog(int portalID, string url, int moduleId, DateTime startDate, DateTime endDate)
    {
        ArrayList arrUrlLog = null;
        UrlTrackingInfo objUrlTracking = this.GetUrlTracking(portalID, url, moduleId);
        if (objUrlTracking != null)
        {
            arrUrlLog = CBO.FillCollection(DataProvider.Instance().GetUrlLog(objUrlTracking.UrlTrackingID, startDate, endDate), typeof(UrlLogInfo));
        }

        return arrUrlLog;
    }
}
