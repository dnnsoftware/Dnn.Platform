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
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.WebControls;
using DotNetNuke.Web.UI.WebControls;
using Telerik.Web.UI;

#endregion

namespace DesktopModules.Admin.Security
{

	/// <summary>
	/// The Roles PortalModuleBase is used to manage the Security Roles for the
	/// portal.
	/// </summary>
    public partial class Roles : PortalModuleBase
    {

        #region Private Members

        private int _roleGroupId = -1;

        #endregion

        #region Private Methods

        private void BindData()
        {
            var roles = _roleGroupId < -1
                                ? RoleController.Instance.GetRoles(PortalId)
                                : RoleController.Instance.GetRoles(PortalId, r => r.RoleGroupID == _roleGroupId);
            grdRoles.DataSource = roles;

            if (_roleGroupId < 0)
            {
                lnkEditGroup.Visible = false;
                cmdDelete.Visible = false;
            }
            else
            {
                lnkEditGroup.Visible = true;
                lnkEditGroup.NavigateUrl = EditUrl("RoleGroupId", _roleGroupId.ToString(CultureInfo.InvariantCulture), "EditGroup");
                cmdDelete.Visible = roles.Count == 0;
            }
            
            grdRoles.DataBind();
        }

        private void BindGroups()
        {
            ArrayList arrGroups = RoleController.GetRoleGroups(PortalId);

            if (arrGroups.Count > 0)
            {
                cboRoleGroups.Items.Clear();
                //cboRoleGroups.Items.Add(new ListItem(Localization.GetString("AllRoles"), "-2"));
                cboRoleGroups.AddItem(Localization.GetString("AllRoles"), "-2");

				var item = new DnnComboBoxItem(Localization.GetString("GlobalRoles"), "-1");
                if (_roleGroupId == -1)
                {
                    item.Selected = true;
                }
                cboRoleGroups.Items.Add(item);

                foreach (RoleGroupInfo roleGroup in arrGroups)
                {
					item = new DnnComboBoxItem(roleGroup.RoleGroupName, roleGroup.RoleGroupID.ToString(CultureInfo.InvariantCulture));
                    if (_roleGroupId == roleGroup.RoleGroupID)
                    {
                        item.Selected = true;
                    }
                    cboRoleGroups.Items.Add(item);
                }
                divGroups.Visible = true;
            }
            else
            {
                _roleGroupId = -2;
                divGroups.Visible = false;
            }
            BindData();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Get text description of Frequency Value
        /// </summary>
        protected string FormatFrequency(string frequency)
        {
            if (frequency == "N") return string.Empty;

            var ctlEntry = new ListController();
            ListEntryInfo entry = ctlEntry.GetListEntryInfo("Frequency", frequency);
            return entry != null ? entry.Text : frequency;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// FormatPeriod filters out Null values from the Period column of the Grid
        /// </summary>
        public string FormatPeriod(int period)
        {
            var formatPeriod = Null.NullString;
            try
            {
                if (period != Null.NullInteger)
                {
                    formatPeriod = period.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatPeriod;
        }

        /// <summary>
        /// FormatPrice correctly formats the fee
        /// </summary>
        public string FormatPrice(float price)
        {
            var formatPrice = Null.NullString;
            try
            {
// ReSharper disable CompareOfFloatsByEqualityOperator
                if (price != Null.NullSingle)
// ReSharper restore CompareOfFloatsByEqualityOperator
                {
                    formatPrice = price.ToString("##0.00");
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return formatPrice;
        }

		#endregion

		#region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdAddRole.Click += cmdAddRole_Click;
            cmdAddRoleGroup.Click += cmdAddRoleGroup_Click;

            jQuery.RequestDnnPluginsRegistration();

            foreach (var column in grdRoles.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (DnnGridImageCommandColumn)))
                {
					//Manage Delete Confirm JS
                    var imageColumn = (DnnGridImageCommandColumn)column;
                    imageColumn.Visible = ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "EDIT");
                    if (imageColumn.CommandName == "Delete")
                    {
                        imageColumn.OnClickJs = Localization.GetString("DeleteItem");
                    }
					
                    //Manage Edit Column NavigateURLFormatString
                    if (imageColumn.CommandName == "Edit")
                    {
                        //so first create the format string with a dummy value and then
                        //replace the dummy value with the FormatString place holder
                        string formatString = EditUrl("RoleID", "KEYFIELD", "Edit");
                        formatString = formatString.Replace("KEYFIELD", "{0}");
                        imageColumn.NavigateURLFormatString = formatString;
                    }
					
                    //Manage Roles Column NavigateURLFormatString
                    if (imageColumn.CommandName == "UserRoles")
                    {
                        //so first create the format string with a dummy value and then
                        //replace the dummy value with the FormatString place holder
                        string formatString = EditUrl("RoleId", "KEYFIELD", "User Roles");
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

        void cmdAddRoleGroup_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl("EditGroup"));
        }

        void cmdAddRole_Click(object sender, EventArgs e)
        {
            Response.Redirect(EditUrl("RoleGroupID", Request.QueryString["RoleGroupID"]), true);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cboRoleGroups.SelectedIndexChanged += OnRoleGroupIndexChanged;
            cmdDelete.Click += OnDeleteClick;
            grdRoles.ItemDataBound += OnRolesGridItemDataBound;

            try
            {
                if (!Page.IsPostBack)
                {
                    if ((Request.QueryString["RoleGroupID"] != null))
                    {
                        _roleGroupId = Int32.Parse(Request.QueryString["RoleGroupID"]);
                    }
                    BindGroups();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnRoleGroupIndexChanged(object sender, EventArgs e)
        {
			Response.Redirect(Globals.NavigateURL("", string.Format("RoleGroupID={0}", cboRoleGroups.SelectedValue)));
        }

        protected void OnDeleteClick(object sender, ImageClickEventArgs e)
        {
            _roleGroupId = Int32.Parse(cboRoleGroups.SelectedValue);
            if (_roleGroupId > -1)
            {
                RoleController.DeleteRoleGroup(PortalId, _roleGroupId);
                _roleGroupId = -1;
            }
            BindGroups();
        }

        protected void OnRolesGridItemDataBound(object sender, GridItemEventArgs e)
        {
            var item = e.Item;
            switch (item.ItemType)
            {
                case GridItemType.SelectedItem:
                case GridItemType.AlternatingItem:
                case GridItemType.Item:
                    {
                        var gridDataItem = (GridDataItem) item;

                        var editLink = gridDataItem["EditButton"].Controls[0] as HyperLink;
                        if (editLink != null)
                        {
                            var role = (RoleInfo) item.DataItem;
                            editLink.Visible = role.RoleName != PortalSettings.AdministratorRoleName || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName));
                        }

                        var rolesLink = gridDataItem["RolesButton"].Controls[0] as HyperLink;
                        if (rolesLink != null)
                        {
                            var role = (RoleInfo) item.DataItem;
                            rolesLink.Visible = (role.Status == RoleStatus.Approved) && (role.RoleName != PortalSettings.AdministratorRoleName || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)));
                        }
                    }
                    break;
            }
        }

        #endregion

    }
}