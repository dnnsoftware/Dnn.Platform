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
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI.WebControls;
using Telerik.Web.UI;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Portals
{

	/// <summary>
	/// The Portals PortalModuleBase is used to manage the portlas.
	/// </summary>
    public partial class Portals : PortalModuleBase
    {
		#region Private Members

        public Portals()
	    {
	        Filter = "";
	    }

	    #endregion

		#region Protected Members

	    protected string Filter { get; set; }

		#endregion

		#region Private Methods

		/// <summary>
		/// BindData fetches the data from the database and updates the controls
		/// </summary>
        private void BindData()
        {
            CreateLetterSearch();

		    int totalRecords = 0;
		    ArrayList portals;
            if (Filter == Localization.GetString("Expired", LocalResourceFile))
            {
                portals = PortalController.GetExpiredPortals();
                totalRecords = portals.Count;
            }
            else
            {
                portals = PortalController.GetPortalsByName(Filter + "%", grdPortals.CurrentPageIndex, grdPortals.PageSize, ref totalRecords);
            }
		    grdPortals.VirtualItemCount = totalRecords;
            grdPortals.DataSource = portals;
        }

        private void CheckSecurity()
        {
            if (!UserInfo.IsSuperUser)
            {
                Response.Redirect(Globals.NavigateURL("Access Denied"), true);
            }
        }

        /// <summary>
        /// Builds the letter filter
        /// </summary>
        private void CreateLetterSearch()
        {
            var filters = Localization.GetString("Filter.Text", LocalResourceFile);

            filters += "," + Localization.GetString("All");
            filters += "," + Localization.GetString("Expired", LocalResourceFile);

            var strAlphabet = filters.Split(',');
            rptLetterSearch.DataSource = strAlphabet;
            rptLetterSearch.DataBind();
        }

        /// <summary>
        /// Deletes all expired portals
        /// </summary>
        private void DeleteExpiredPortals()
        {
            try
            {
                CheckSecurity();
                PortalController.DeleteExpiredPortals(Globals.GetAbsoluteServerPath(Request));

                BindData();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

		#endregion

		#region Protected Methods

        /// <summary>
        /// FilterURL correctly formats the Url for filter by first letter and paging
        /// </summary>
        protected string FilterURL(string filter, string currentPage)
        {
            string url;
            if (!String.IsNullOrEmpty(filter))
            {
                url = !String.IsNullOrEmpty(currentPage) ? Globals.NavigateURL(TabId, "", "filter=" + filter, "currentpage=" + grdPortals.CurrentPageIndex) : Globals.NavigateURL(TabId, "", "filter=" + filter);
            }
            else
            {
                url = !String.IsNullOrEmpty(currentPage) ? Globals.NavigateURL(TabId, "", "currentpage=" + grdPortals.CurrentPageIndex) : Globals.NavigateURL(TabId, "");
            }
            return url;
        }

		#endregion

		#region Public Methods

        /// <summary>
        /// FormatExpiryDate formats the expiry date and filter out null-dates
        /// </summary>
        public string FormatExpiryDate(DateTime dateTime)
        {
            var strDate = string.Empty;
            try
            {
                if (!Null.IsNull(dateTime))
                {
                    strDate = dateTime.ToShortDateString();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return strDate;
        }

        /// <summary>
        /// FormatExpiryDate formats the format name as an a tag
        /// </summary>
        public string FormatPortalAliases(int portalID)
        {
            var str = new StringBuilder();
            try
            {
                var arr = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portalID).ToList();
                foreach ( PortalAliasInfo portalAliasInfo in arr)
                {
                    var httpAlias = Globals.AddHTTP(portalAliasInfo.HTTPAlias);
                    var originalUrl = HttpContext.Current.Items["UrlRewrite:OriginalUrl"].ToString().ToLowerInvariant();

                    httpAlias = Globals.AddPort(httpAlias, originalUrl);

                    str.Append("<a href=\"" + httpAlias + "\">" + portalAliasInfo.HTTPAlias + "</a>" + "<BR>");
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return str.ToString();
        }
		
		#endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdDeleteExpired.Visible = PortalController.GetExpiredPortals().Count > 0;
            cmdDeleteExpired.Click += cmdDeleteExpired_Click;

            foreach (GridColumn column in grdPortals.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (DnnGridImageCommandColumn)))
                {
					//Manage Delete Confirm JS
                    var imageColumn = (DnnGridImageCommandColumn)column;
                    if (imageColumn.CommandName == "Delete")
                    {
                        imageColumn.OnClickJs = Localization.GetString("DeleteItem");
                    }
					
                    //Manage Edit Column NavigateURLFormatString
                    if (imageColumn.CommandName == "Edit")
                    {
                        //so first create the format string with a dummy value and then
                        //replace the dummy value with the FormatString place holder
                        var formatString = EditUrl("pid", "KEYFIELD", "Edit");
                        formatString = formatString.Replace("KEYFIELD", "{0}");
                        imageColumn.NavigateURLFormatString = formatString;
                    }
					
                    //Localize Image Column Text
                    if (!String.IsNullOrEmpty(imageColumn.CommandName))
                    {
                        imageColumn.Text = Localization.GetString(imageColumn.CommandName, LocalResourceFile);
                    }
                }
            }
        }

        void cmdDeleteExpired_Click(object sender, EventArgs e)
        {
            DeleteExpiredPortals();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            grdPortals.DeleteCommand += OnGridDeleteCommand;
            grdPortals.ItemDataBound += OnGridItemDataBound;

            try
            {

                if (!UserInfo.IsSuperUser)
                {
                    Response.Redirect(Globals.NavigateURL("Access Denied"), true);
                }
                if (Request.QueryString["CurrentPage"] != null)
                {
                    grdPortals.CurrentPageIndex = Convert.ToInt32(Request.QueryString["CurrentPage"]);
                }
                if (Request.QueryString["filter"] != null)
                {
                    Filter = Request.QueryString["filter"];
                }
                if (Filter == Localization.GetString("All"))
                {
                    Filter = "";
                }
                if (!Page.IsPostBack)
                {
					BindData();
                }
            }
            catch (Exception exc)
            {
				//Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnGridDeleteCommand(object source, GridCommandEventArgs e)
        {
            try
            {
                var objPortalController = new PortalController();
                var portal = objPortalController.GetPortal(Int32.Parse(e.CommandArgument.ToString()));
                if (portal != null)
                {
                    var strMessage = PortalController.DeletePortal(portal, Globals.GetAbsoluteServerPath(Request));
                    if (string.IsNullOrEmpty(strMessage))
                    {
                        var objEventLog = new EventLogController();
                        objEventLog.AddLog("PortalName", portal.PortalName, PortalSettings, UserId, EventLogController.EventLogType.PORTAL_DELETED);
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("PortalDeleted", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, strMessage, ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                BindData();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnGridItemDataBound(object sender, GridItemEventArgs e)
        {
            var item = e.Item;
            switch (item.ItemType)
            {
                case GridItemType.SelectedItem:
                case GridItemType.AlternatingItem:
                case GridItemType.Item:
                    {
                        var imgColumnControl = ((GridDataItem)item)["DeleteColumn"].Controls[0];
                        if (imgColumnControl is ImageButton)
                        {
                            var delImage = (ImageButton) imgColumnControl;
                            var portal = (PortalInfo) item.DataItem;
                            delImage.Visible = (portal.PortalID != PortalSettings.PortalId && !PortalController.IsMemberOfPortalGroup(portal.PortalID));
                        }
                    }
                    break;
            }
        }

        #endregion

	    protected void GridNeedsDataSource(object sender, GridNeedDataSourceEventArgs e)
	    {
	        BindData();
	    }
    }
}