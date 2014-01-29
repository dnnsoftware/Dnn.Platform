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
using System.Drawing;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Vendors;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <remarks></remarks>
    /// <history>
    /// 	[cniknet]	10/15/2004	Replaced public members with properties and removed
    ///                             brackets from property names
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class Banner : SkinObjectBase
    {
	
		#region "Private Properties"
		
        private const string MyFileName = "Banner.ascx";
		
		#endregion
		
		#region "Public Members"
		
        public string GroupName { get; set; }

        public bool AllowNullBannerType { get; set; }

        public string BannerTypeId { get; set; }

        public string BannerCount { get; set; }

        public string Width { get; set; }

        public string Orientation { get; set; }

        public string BorderWidth { get; set; }

        public string BorderColor { get; set; }

        public string RowHeight { get; set; }

        public string ColWidth { get; set; }

		public RepeatLayout BannerLayout { get; set; }

		public int BannerColumns { get; set; }

		public int BannerCellPadding { get; set; }

		public int BannerCellSpacing { get; set; }

		#endregion

		#region "Event Handlers"

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            if (PortalSettings.BannerAdvertising != 0 && Visible)
            {
                int BannerType = 0;
                //read bannertype from definition, if not use portalsetting
                if (!string.IsNullOrEmpty(BannerTypeId))
                {
                    BannerType = Int32.Parse(Convert.ToString(BannerTypeId));
                }
                else
                {
                    if (AllowNullBannerType)
                    {
                        BannerType = PortalController.GetPortalSettingAsInteger("BannerTypeId", PortalSettings.PortalId, 1);
                    }
                }

                //public attributes
                if (String.IsNullOrEmpty(GroupName))
                {
                    GroupName = PortalController.GetPortalSetting("BannerGroupName", PortalSettings.PortalId, "");
                }

                if (String.IsNullOrEmpty(BannerCount))
                {
                    BannerCount = "1";
                }
                int intPortalId;
                if (PortalSettings.BannerAdvertising == 1)
                {
                    intPortalId = PortalSettings.PortalId; //portal
                }
                else
                {
                    intPortalId = Null.NullInteger; //host
                }
				
                //load banners
                var objBanners = new BannerController();

                ArrayList arrBanners = objBanners.LoadBanners(intPortalId, Null.NullInteger, BannerType, GroupName, int.Parse(BannerCount));

                //bind to datalist
                lstBanners.DataSource = arrBanners;
                lstBanners.DataBind();

                //set banner display characteristics
                if (lstBanners.Items.Count != 0)
                {
                	lstBanners.RepeatLayout = BannerLayout;

                    if (!String.IsNullOrEmpty(Width))
                    {
                        lstBanners.Width = Unit.Parse(Width + "px");
                    }

					if(BannerColumns > 0)
					{
						lstBanners.RepeatColumns = BannerColumns;
					}

					if(BannerCellPadding > 0)
					{
						lstBanners.CellPadding = BannerCellPadding;
					}
					else
					{
						lstBanners.CellPadding = lstBanners.Items.Count == 1 ? 0 : 4;
					}

                	lstBanners.CellSpacing = BannerCellSpacing;
					
                    if (!String.IsNullOrEmpty(Orientation))
                    {
                        switch (Orientation)
                        {
                            case "H":
                                lstBanners.RepeatDirection = RepeatDirection.Horizontal;
                                break;
                            case "V":
                                lstBanners.RepeatDirection = RepeatDirection.Vertical;
                                break;
                        }
                    }
                    else
                    {
                        lstBanners.RepeatDirection = RepeatDirection.Vertical;
                    }
					
                    if (!String.IsNullOrEmpty(BorderWidth))
                    {
                        lstBanners.ItemStyle.BorderWidth = Unit.Parse(BorderWidth + "px");
                    }
					
                    if (!String.IsNullOrEmpty(BorderColor))
                    {
                        var objColorConverter = new ColorConverter();
                        lstBanners.ItemStyle.BorderColor = (Color) objColorConverter.ConvertFrom(BorderColor);
                    }
					
					//item attributes
                    if (!String.IsNullOrEmpty(RowHeight))
                    {
                        lstBanners.ItemStyle.Height = Unit.Parse(RowHeight + "px");
                    }
					
                    if (!String.IsNullOrEmpty(ColWidth))
                    {
                        lstBanners.ItemStyle.Width = Unit.Parse(ColWidth + "px");
                    }
                }
                else
                {
                    lstBanners.Visible = false;
                }
            }
            else
            {
                lstBanners.Visible = false;
            }
        }

		#endregion

		#region "Public Methods"
		
        public string FormatItem(int VendorId, int BannerId, int BannerTypeId, string BannerName, string ImageFile, string Description, string URL, int Width, int Height)
        {
            var objBanners = new BannerController();
            return objBanners.FormatBanner(VendorId,
                                           BannerId,
                                           BannerTypeId,
                                           BannerName,
                                           ImageFile,
                                           Description,
                                           URL,
                                           Width,
                                           Height,
                                           PortalSettings.BannerAdvertising == 1 ? "L" : "G",
                                           PortalSettings.HomeDirectory);
        }
		
		#endregion
    }
}