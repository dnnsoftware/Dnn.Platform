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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Vendors;
using DotNetNuke.UI.Utilities;
using Telerik.Web.UI;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Vendors
{
	/// -----------------------------------------------------------------------------
	/// <summary>
	/// The Vendors PortalModuleBase is used to manage the Vendors of a portal
	/// </summary>
    /// <remarks>
	/// </remarks>
	/// <history>
	/// 	[cnurse]	9/17/2004	Updated to reflect design changes for Help, 508 support
	///                       and localisation
	/// </history>
	/// -----------------------------------------------------------------------------
    public partial class Vendors : PortalModuleBase
    {
        protected int CurrentPage = -1;
        protected int TotalPages = -1;
        private string _searchFilter;
	    private string _searchField;

		#region Private Methods

	    protected bool CanEdit()
        {
            return ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, "EDIT", ModuleConfiguration);
        }

        private void SetDataSource()
        {
            CreateLetterSearch();

            bool isUnauthorized = false;
            if (_searchFilter.Equals(Localization.GetString("All"), StringComparison.InvariantCultureIgnoreCase))
            {
                _searchFilter = "";
            }
            else if (_searchFilter.Equals(Localization.GetString("Unauthorized"), StringComparison.InvariantCultureIgnoreCase))
            {
                _searchFilter = "";
                isUnauthorized = true;
            }
			
            //Get the list of vendors from the database
            var totalRecords = 0;
            var vendorController = new VendorController();
            int portal = Globals.IsHostTab(PortalSettings.ActiveTab.TabID) ? Null.NullInteger : PortalId;
            
			if (String.IsNullOrEmpty(_searchFilter))
			{
			    grdVendors.DataSource = vendorController.GetVendors(portal, isUnauthorized, grdVendors.CurrentPageIndex, grdVendors.PageSize, ref totalRecords);
			    grdVendors.VirtualItemCount = totalRecords;
			}
            else
            {
                if (_searchField == "email")
                {
                    grdVendors.DataSource = vendorController.GetVendorsByEmail(_searchFilter, portal, grdVendors.CurrentPageIndex, grdVendors.PageSize, ref totalRecords);
                    grdVendors.VirtualItemCount = totalRecords;
                }
                else
                {
                    grdVendors.DataSource = vendorController.GetVendorsByName(_searchFilter, portal, grdVendors.CurrentPageIndex, grdVendors.PageSize, ref totalRecords);
                    grdVendors.VirtualItemCount = totalRecords;
                }
            }
        }

        private void CreateLetterSearch()
        {
            string[] strAlphabet = {
                                       "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", Localization.GetString("All"),
                                       Localization.GetString("Unauthorized")
                                   };
            rptLetterSearch.DataSource = strAlphabet;
            rptLetterSearch.DataBind();
        }

		#endregion

		#region Public Methods

        /// <summary>
        /// DisplayAddress correctly formats an Address
        /// </summary>
        public string DisplayAddress(object Unit, object Street, object City, object Region, object Country, object PostalCode)
        {
            return Globals.FormatAddress(Unit, Street, City, Region, Country, PostalCode);
        }

        /// <summary>
        /// FormatURL correctly formats the Url for the Edit Vendor Link
        /// </summary>
        public string FormatURL(string strKeyName, string strKeyValue)
        {
            return !String.IsNullOrEmpty(_searchFilter) ? EditUrl(strKeyName, strKeyValue, "", "filter=" + _searchFilter) : EditUrl(strKeyName, strKeyValue);
        }

        /// <summary>
        /// FilterURL correctly formats the Url for filter by first letter and paging
        /// </summary>
        protected string FilterURL(string Filter, string CurrentPage)
        {
            if (!String.IsNullOrEmpty(Filter))
            {
                if (!String.IsNullOrEmpty(CurrentPage))
                {
                    return Globals.NavigateURL(TabId, "", "filter=" + Filter, "currentpage=" + CurrentPage, "PageRecords=" + grdVendors.PageSize);
                }
                else
                {
                    return Globals.NavigateURL(TabId, "", "filter=" + Filter, "PageRecords=" + grdVendors.PageSize);
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(CurrentPage))
                {
                    return Globals.NavigateURL(TabId, "", "currentpage=" + CurrentPage, "PageRecords=" + grdVendors.PageSize);
                }
                else
                {
                    return Globals.NavigateURL(TabId, "", "PageRecords=" + grdVendors.PageSize);
                }
            }
        }

		#endregion

		#region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnSearch.Click += OnSearchClick;
            cmdAddVendor.Click += cmdAddVendor_Click;
            cmdDeleteUnAuthorized.Click += cmdDeleteUnAuthorized_Click;
            cmdAddVendor.Visible = CanEdit();
            cmdDeleteUnAuthorized.Visible = CanEdit();

			ClientAPI.RegisterKeyCapture(txtSearch, btnSearch, 13);

            try
            {
                CurrentPage = Request.QueryString["CurrentPage"] != null ? Convert.ToInt32(Request.QueryString["CurrentPage"]) : 1;
                _searchFilter = Request.QueryString["filter"] ?? "";
                if (!Page.IsPostBack)
                {
                    if (Request.QueryString["PageRecords"] != null)
                    {
                        int pageSize = Convert.ToInt32(Request.QueryString["PageRecords"]);
                        if (pageSize >= 1 && pageSize <= 250)
                        {
                            grdVendors.PageSize = pageSize;
                        }
                    }
                    _searchField = "name";
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void cmdDeleteUnAuthorized_Click(object sender, EventArgs e)
        {
            try
            {
                var objVendors = new VendorController();
                if (Globals.IsHostTab(PortalSettings.ActiveTab.TabID))
                {
                    objVendors.DeleteVendors();
                }
                else
                {
                    objVendors.DeleteVendors(PortalId);
                }
                Response.Redirect(Globals.NavigateURL(), true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void cmdAddVendor_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl(), true);
        }

        protected void OnSearchClick(Object sender, EventArgs e)
        {
            grdVendors.CurrentPageIndex = 0;
            _searchField = ddlSearchType.SelectedValue;
            _searchFilter = txtSearch.Text;
            SetDataSource();
            grdVendors.DataBind();
        }
		
		#endregion

	    protected void NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
	    {
	        SetDataSource();
	    }
    }
}