#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Tabs;

#endregion

namespace DotNetNuke.Services.Vendors
{
    public class BannerController
    {
		#region "Private Members"

        private string BannerClickThroughPage = "/DesktopModules/Admin/Banners/BannerClickThrough.aspx";

		#endregion

		#region "Private Methods"

        private string FormatImage(string File, int Width, int Height, string BannerName, string Description)
        {
            var Image = new StringBuilder();
            Image.Append("<img src=\"" + File + "\" border=\"0\"");
            if (!String.IsNullOrEmpty(Description))
            {
                Image.Append(" alt=\"" + Description + "\"");
            }
            else
            {
                Image.Append(" alt=\"" + BannerName + "\"");
            }
            if (Width > 0)
            {
                Image.Append(" width=\"" + Width + "\"");
            }
            if (Height > 0)
            {
                Image.Append(" height=\"" + Height + "\"");
            }
            Image.Append("/>");
            return Image.ToString();
        }

        private string FormatFlash(string File, int Width, int Height)
        {
            string Flash = "";

            Flash += "<object classid=\"clsid:D27CDB6E-AE6D-11cf-96B8-444553540000\" codebase=\"http://download.macromedia.com/pub/shockwave/cabs/flash/swflash.cab#version=4,0,2,0\" width=\"" + Width +
                     "\" height=\"" + Height + "\">";
            Flash += "<param name=movie value=\"" + File + "\">";
            Flash += "<param name=quality value=high>";
            Flash += "<embed src=\"" + File +
                     "\" quality=high pluginspage=\"http://www.macromedia.com/shockwave/download/index.cgi?P1_Prod_Version=ShockwaveFlash\" type=\"application/x-shockwave-flash\" width=\"" + Width +
                     "\" height=\"" + Height + "\">";
            Flash += "</embed>";
            Flash += "</object>";

            return Flash;
        }

		#endregion

		#region "Public Methods"

		public bool IsBannerActive(BannerInfo objBanner)
        {
            bool blnValid = true;

            if (Null.IsNull(objBanner.StartDate) == false && DateTime.Now < objBanner.StartDate)
            {
                blnValid = false;
            }
            if (blnValid)
            {
                switch (objBanner.Criteria)
                {
                    case 0: //AND = cancel the banner when the Impressions expire
                        if (objBanner.Impressions < objBanner.Views && objBanner.Impressions != 0)
                        {
                            blnValid = false;
                        }
                        break;
                    case 1: //OR = cancel the banner if either the EndDate OR Impressions expire
                        if ((objBanner.Impressions < objBanner.Views && objBanner.Impressions != 0) || (DateTime.Now > objBanner.EndDate && Null.IsNull(objBanner.EndDate) == false))
                        {
                            blnValid = false;
                        }
                        break;
                }
            }
            return blnValid;
        }

        private object LoadBannersCallback(CacheItemArgs cacheItemArgs)
        {
            var PortalId = (int) cacheItemArgs.ParamList[0];
            var BannerTypeId = (int) cacheItemArgs.ParamList[1];
            var GroupName = (string) cacheItemArgs.ParamList[2];

            //get list of all banners
            List<BannerInfo> FullBannerList = CBO.FillCollection<BannerInfo>(DataProvider.Instance().FindBanners(PortalId, BannerTypeId, GroupName));

            //create list of active banners
            var ActiveBannerList = new List<BannerInfo>();
            foreach (BannerInfo objBanner in FullBannerList)
            {
                if (IsBannerActive(objBanner))
                {
                    ActiveBannerList.Add(objBanner);
                }
            }
            return ActiveBannerList;
        }

        public void AddBanner(BannerInfo objBannerInfo)
        {
            DataProvider.Instance().AddBanner(objBannerInfo.BannerName,
                                              objBannerInfo.VendorId,
                                              objBannerInfo.ImageFile,
                                              objBannerInfo.URL,
                                              objBannerInfo.Impressions,
                                              objBannerInfo.CPM,
                                              objBannerInfo.StartDate,
                                              objBannerInfo.EndDate,
                                              objBannerInfo.CreatedByUser,
                                              objBannerInfo.BannerTypeId,
                                              objBannerInfo.Description,
                                              objBannerInfo.GroupName,
                                              objBannerInfo.Criteria,
                                              objBannerInfo.Width,
                                              objBannerInfo.Height);
            ClearBannerCache();
        }

        public void ClearBannerCache()
        {
			//Clear all cached Banners collections
            DataCache.ClearCache("Banners:");
        }

        public void DeleteBanner(int BannerId)
        {
            DataProvider.Instance().DeleteBanner(BannerId);
            ClearBannerCache();
        }

        public string FormatBanner(int VendorId, int BannerId, int BannerTypeId, string BannerName, string ImageFile, string Description, string URL, int Width, int Height, string BannerSource,
                                   string HomeDirectory, string BannerClickthroughUrl)
        {
            string strBanner = "";
            string strWindow = "_new";
            if (Globals.GetURLType(URL) == TabType.Tab)
            {
                strWindow = "_self";
            }
            string strURL = "";
            if (BannerId != -1)
            {
                if (string.IsNullOrEmpty(BannerClickthroughUrl))
                {
                    strURL = Globals.ApplicationPath + BannerClickThroughPage + "?BannerId=" + BannerId + "&VendorId=" + VendorId + "&PortalId=" + Globals.GetPortalSettings().PortalId;
                }
                else
                {
                    strURL = BannerClickthroughUrl + "?BannerId=" + BannerId + "&VendorId=" + VendorId + "&PortalId=" + Globals.GetPortalSettings().PortalId;
                }
            }
            else
            {
                strURL = URL;
            }
            strURL = HttpUtility.HtmlEncode(strURL);

            switch (BannerTypeId)
            {
                case (int) BannerType.Text:
                    strBanner += "<a href=\"" + strURL + "\" class=\"NormalBold\" target=\"" + strWindow + "\" rel=\"nofollow\"><u>" + BannerName + "</u></a><br />";
                    strBanner += "<span class=\"Normal\">" + Description + "</span><br />";
                    if (!String.IsNullOrEmpty(ImageFile))
                    {
                        URL = ImageFile;
                    }
                    if (URL.IndexOf("://") != -1)
                    {
                        URL = URL.Substring(URL.IndexOf("://") + 3);
                    }
                    strBanner += "<a href=\"" + strURL + "\" class=\"NormalRed\" target=\"" + strWindow + "\" rel=\"nofollow\">" + URL + "</a>";
                    break;
                case (int) BannerType.Script:
                    strBanner += Description;
                    break;
                default:
                    if (ImageFile.IndexOf("://") == -1 && ImageFile.StartsWith("/") == false)
                    {
                        if (ImageFile.ToLowerInvariant().IndexOf(".swf") == -1)
                        {
                            strBanner += "<a href=\"" + strURL + "\" target=\"" + strWindow + "\" rel=\"nofollow\">";
                            switch (BannerSource)
                            {
                                case "L": //local
                                    strBanner += FormatImage(HomeDirectory + ImageFile, Width, Height, BannerName, Description);
                                    break;
                                case "G": //global
                                    strBanner += FormatImage(Globals.HostPath + ImageFile, Width, Height, BannerName, Description);
                                    break;
                            }
                            strBanner += "</a>";
                        }
                        else //flash
                        {
                            switch (BannerSource)
                            {
                                case "L": //local
                                    strBanner += FormatFlash(HomeDirectory + ImageFile, Width, Height);
                                    break;
                                case "G": //global
                                    strBanner += FormatFlash(Globals.HostPath + ImageFile, Width, Height);
                                    break;
                            }
                        }
                    }
                    else
                    {
                        if (ImageFile.ToLowerInvariant().IndexOf(".swf") == -1)
                        {
                            strBanner += "<a href=\"" + strURL + "\" target=\"" + strWindow + "\" rel=\"nofollow\">";
                            strBanner += FormatImage(ImageFile, Width, Height, BannerName, Description);
                            strBanner += "</a>";
                        }
                        else //flash
                        {
                            strBanner += FormatFlash(ImageFile, Width, Height);
                        }
                    }
                    break;
            }
            return strBanner;
        }

        public string FormatBanner(int VendorId, int BannerId, int BannerTypeId, string BannerName, string ImageFile, string Description, string URL, int Width, int Height, string BannerSource,
                                   string HomeDirectory)
        {
            return FormatBanner(VendorId, BannerId, BannerTypeId, BannerName, ImageFile, Description, URL, Width, Height, BannerSource, HomeDirectory, string.Empty);
        }

        public BannerInfo GetBanner(int BannerId)
        {
            return CBO.FillObject<BannerInfo>(DataProvider.Instance().GetBanner(BannerId));
        }

        public DataTable GetBannerGroups(int PortalId)
        {
            return DataProvider.Instance().GetBannerGroups(PortalId);
        }

        public ArrayList GetBanners(int VendorId)
        {
            return CBO.FillCollection(DataProvider.Instance().GetBanners(VendorId), typeof (BannerInfo));
        }

        public ArrayList LoadBanners(int PortalId, int ModuleId, int BannerTypeId, string GroupName, int Banners)
        {
            if (GroupName == null)
            {
                GroupName = Null.NullString;
            }
			
            //set cache key
            string cacheKey = string.Format(DataCache.BannersCacheKey, PortalId, BannerTypeId, GroupName);

            //get list of active banners
            var bannersList = CBO.GetCachedObject<List<BannerInfo>>(new CacheItemArgs(cacheKey, DataCache.BannersCacheTimeOut, DataCache.BannersCachePriority, PortalId, BannerTypeId, GroupName),
                                                                    LoadBannersCallback);
																	
            //create return collection
            var arReturnBanners = new ArrayList(Banners);

            if (bannersList.Count > 0)
            {
                if (Banners > bannersList.Count)
                {
                    Banners = bannersList.Count;
                }
				
                //set Random start index based on the list of banners
                int intIndex = new Random().Next(0, bannersList.Count);
                //set counter
                int intCounter = 1;

                while (intCounter <= bannersList.Count && arReturnBanners.Count != Banners)
                {
					//manage the rotation for the circular collection
                    intIndex += 1;
                    if (intIndex > (bannersList.Count - 1))
                    {
                        intIndex = 0;
                    }
					
                    //get the banner object
                    BannerInfo objBanner = bannersList[intIndex];

                    //add to return collection
                    arReturnBanners.Add(objBanner);

                    //update banner attributes
                    objBanner.Views += 1;
                    if (Null.IsNull(objBanner.StartDate))
                    {
                        objBanner.StartDate = DateTime.Now;
                    }
                    if (Null.IsNull(objBanner.EndDate) && objBanner.Views >= objBanner.Impressions && objBanner.Impressions != 0)
                    {
                        objBanner.EndDate = DateTime.Now;
                    }
                    DataProvider.Instance().UpdateBannerViews(objBanner.BannerId, objBanner.StartDate, objBanner.EndDate);

                    //expire cached collection of banners if a banner is no longer active
                    if (!IsBannerActive(objBanner))
                    {
                        DataCache.RemoveCache(cacheKey);
                    }
                    intCounter += 1;
                }
            }
            return arReturnBanners;
        }

        public void UpdateBanner(BannerInfo objBannerInfo)
        {
            DataProvider.Instance().UpdateBanner(objBannerInfo.BannerId,
                                                 objBannerInfo.BannerName,
                                                 objBannerInfo.ImageFile,
                                                 objBannerInfo.URL,
                                                 objBannerInfo.Impressions,
                                                 objBannerInfo.CPM,
                                                 objBannerInfo.StartDate,
                                                 objBannerInfo.EndDate,
                                                 objBannerInfo.CreatedByUser,
                                                 objBannerInfo.BannerTypeId,
                                                 objBannerInfo.Description,
                                                 objBannerInfo.GroupName,
                                                 objBannerInfo.Criteria,
                                                 objBannerInfo.Width,
                                                 objBannerInfo.Height);
            ClearBannerCache();
        }

        public void UpdateBannerClickThrough(int BannerId, int VendorId)
        {
            DataProvider.Instance().UpdateBannerClickThrough(BannerId, VendorId);
        }

		#endregion

		#region "Obsolete methods"

		[Obsolete("Deprecated in DNN 5.6.2. Use BannerController.GetBanner(Int32)")]
        public BannerInfo GetBanner(int BannerId, int VendorId, int PortalId)
        {
            return GetBanner(BannerId);
        }

        #endregion
    }
}