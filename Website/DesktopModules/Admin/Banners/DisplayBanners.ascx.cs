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
using System.Drawing;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Vendors;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
    public partial class DisplayBanners : PortalModuleBase, IActionable
    {
        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var Actions = new ModuleActionCollection();
                Actions.Add(GetNextActionID(),
                            Localization.GetString(ModuleActionType.AddContent, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "",
                            EditUrl(),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);
                return Actions;
            }
        }

        #endregion

        /// <summary>
        /// The Page_Load event handler on this User Control is used to
        /// obtain a DataReader of banner information from the Banners
        /// table, and then databind the results to a templated DataList
        /// server control.  It uses the DotNetNuke.BannerDB()
        /// data component to encapsulate all data functionality.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks></remarks>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            //exit without displaying banners to crawlers
			if (Request.Browser.Crawler)
            {
                return;
            }
            try
            {
                int intPortalId = 0;
                int intBannerTypeId = 0;
                string strBannerGroup;
                int intBanners = 0;

                //banner parameters
                switch (Convert.ToString(Settings["bannersource"]))
                {
                    case "L": //local
                    case "":
                        intPortalId = PortalId;
                        break;
                    case "G": //global
                        intPortalId = Null.NullInteger;
                        break;
                }
                if (!String.IsNullOrEmpty(Convert.ToString(Settings["bannertype"])))
                {
                    intBannerTypeId = Int32.Parse(Convert.ToString(Settings["bannertype"]));
                }
                strBannerGroup = Convert.ToString(Settings["bannergroup"]);
                if (!String.IsNullOrEmpty(Convert.ToString(Settings["bannercount"])))
                {
                    intBanners = Int32.Parse(Convert.ToString(Settings["bannercount"]));
                }
                if (!String.IsNullOrEmpty(Convert.ToString(Settings["padding"])))
                {
                    lstBanners.CellPadding = Int32.Parse(Convert.ToString(Settings["padding"]));
                }
				
                //load banners
                if (intBanners != 0)
                {
                    var objBanners = new BannerController();
                    lstBanners.DataSource = objBanners.LoadBanners(intPortalId, ModuleId, intBannerTypeId, strBannerGroup, intBanners);
                    lstBanners.DataBind();
                }
				
                //set banner display characteristics
                if (lstBanners.Items.Count != 0)
                {
					//container attributes
                    lstBanners.RepeatLayout = RepeatLayout.Table;
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["orientation"])))
                    {
                        switch (Convert.ToString(Settings["orientation"]))
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
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["border"])))
                    {
                        lstBanners.ItemStyle.BorderWidth = Unit.Parse(Convert.ToString(Settings["border"]) + "px");
                    }
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["bordercolor"])))
                    {
                        var objColorConverter = new ColorConverter();
                        lstBanners.ItemStyle.BorderColor = (Color) objColorConverter.ConvertFrom(Convert.ToString(Settings["bordercolor"]));
                    }
					
                    //item attributes
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["rowheight"])))
                    {
                        lstBanners.ItemStyle.Height = Unit.Parse(Convert.ToString(Settings["rowheight"]) + "px");
                    }
                    if (!String.IsNullOrEmpty(Convert.ToString(Settings["colwidth"])))
                    {
                        lstBanners.ItemStyle.Width = Unit.Parse(Convert.ToString(Settings["colwidth"]) + "px");
                    }
                }
                else
                {
                    lstBanners.Visible = false;
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

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
                                           Convert.ToString(Settings["bannersource"]),
                                           PortalSettings.HomeDirectory,
                                           Convert.ToString(Settings["bannerclickthroughurl"]));
        }
    }
}