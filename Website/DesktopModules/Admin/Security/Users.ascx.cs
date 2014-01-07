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
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI.WebControls;
using Telerik.Web.UI;
using Globals = DotNetNuke.Common.Globals;

#endregion

// ReSharper disable CheckNamespace
namespace DotNetNuke.Modules.Admin.Users
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// The Users PortalModuleBase is used to manage the Registered Users of a portal
    /// </summary>
    public partial class UserAccounts : PortalModuleBase
    {
		#region Private Members

        public UserAccounts()
        {
            Users = new ArrayList();
            FilterProperty = "";
            Filter = "";
        }

        #endregion

		#region Protected Members

        protected string Filter { get; set; }

        protected string FilterProperty { get; set; }

        protected bool IsSuperUser
        {
            get
            {
            	return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        protected int UsersPortalId
        {
            get
            {
                var intPortalId = PortalId;
                if (IsSuperUser)
                {
                    intPortalId = Null.NullInteger;
                }
                return intPortalId;
            }
        }

        protected ArrayList Users { get; set; }

        #endregion

		#region Private Methods

        protected string UserFilter(bool newFilter)
        {
            var page = "currentpage=" + grdUsers.CurrentPageIndex;
            string filterString;
            string filterPropertyString;
            if (!newFilter)
            {
                filterString = !string.IsNullOrEmpty(Filter) ? "filter=" + Filter : "";
                filterPropertyString = !string.IsNullOrEmpty(FilterProperty) ? "filterproperty=" + FilterProperty : "";
            }
            else
            {
                filterString = !string.IsNullOrEmpty(txtSearch.Text) ? "filter=" + Server.UrlEncode(txtSearch.Text) : "";
                filterPropertyString = !string.IsNullOrEmpty(ddlSearchType.SelectedValue) ? "filterproperty=" + ddlSearchType.SelectedValue : "";
            }
            if (!string.IsNullOrEmpty(filterString))
            {
                filterString += "&";
            }
            if (!string.IsNullOrEmpty(filterPropertyString))
            {
                filterString += filterPropertyString + "&";
            }
            if (!string.IsNullOrEmpty(page))
            {
                filterString += page;
            }
            return filterString;
        }

        private DnnComboBoxItem AddSearchItem(string name)
        {
            var propertyName = Null.NullString;
            if (Request.QueryString["filterProperty"] != null)
            {
                propertyName = Request.QueryString["filterProperty"];
            }
            var text = Localization.GetString(name, LocalResourceFile);
            if (String.IsNullOrEmpty(text))
            {
                text = name;
            }
            var item = new DnnComboBoxItem(text, name);
            if (name == propertyName)
            {
                item.Selected = true;
            }
            return item;
        }

        private void SetGridDataSource()
        {
            var searchText = Filter;
            var searchField = ddlSearchType.SelectedValue;

            CreateLetterSearch();

            int totalRecords = 0;

            if (searchText == Localization.GetString("Unauthorized"))
            {
                Users = UserController.GetUnAuthorizedUsers(UsersPortalId, true, IsSuperUser);
				totalRecords = Users.Count;
            }
            else if (searchText == Localization.GetString("Deleted"))
            {
                Users = UserController.GetDeletedUsers(UsersPortalId);
				totalRecords = Users.Count;
            }
            else if (searchText == Localization.GetString("OnLine"))
            {
                Users = UserController.GetOnlineUsers(UsersPortalId);
	            totalRecords = Users.Count;
            }
            else if (searchText == Localization.GetString("All"))
            {
                Users = UserController.GetUsers(UsersPortalId, grdUsers.CurrentPageIndex, grdUsers.PageSize, ref totalRecords, true, IsSuperUser);                
            }
            else if (searchText != "None")
            {
                if (searchText.Length > 1)
                {
                    searchText = "%" + searchText;
                }
                switch (searchField)
                {
                    case "Email":
                        Users = UserController.GetUsersByEmail(UsersPortalId, searchText + "%", grdUsers.CurrentPageIndex, grdUsers.PageSize, ref totalRecords, true, IsSuperUser);
                        break;
                    case "Username":
                        Users = UserController.GetUsersByUserName(UsersPortalId, searchText + "%", grdUsers.CurrentPageIndex, grdUsers.PageSize, ref totalRecords, true, IsSuperUser);
                        break;
					case "DisplayName":
						Users = UserController.GetUsersByDisplayName(UsersPortalId, searchText + "%", grdUsers.CurrentPageIndex, grdUsers.PageSize, ref totalRecords, true, IsSuperUser);
                        break;
                    default:
                        Users = UserController.GetUsersByProfileProperty(UsersPortalId, searchField, searchText + "%", grdUsers.CurrentPageIndex, grdUsers.PageSize, ref totalRecords, true, IsSuperUser);
                        break;
                }
            }

            grdUsers.DataSource = Users;
            grdUsers.VirtualItemCount = totalRecords;
        }

        private void CreateLetterSearch()
        {
            var filters = Localization.GetString("Filter.Text", LocalResourceFile);

            filters += "," + Localization.GetString("All");
            filters += "," + Localization.GetString("OnLine");
            filters += "," + Localization.GetString("Unauthorized");
            filters += "," + Localization.GetString("Deleted");
            var strAlphabet = filters.Split(',');
            rptLetterSearch.DataSource = strAlphabet;
            rptLetterSearch.DataBind();
        }

        private void DeleteUnAuthorizedUsers()
        {
            try
            {
                UserController.DeleteUnauthorizedUsers(PortalId);
                RebindGrid();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void RebindGrid()
        {
            SetGridDataSource();
            grdUsers.Rebind();
        }

        private void RemoveDeletedUsers()
        {
            try
            {
                UserController.RemoveDeletedUsers(UsersPortalId);
                RebindGrid();
            }
            catch (Exception exc)   //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private bool IsCommandAllowed(UserInfo user, string command)
        {
            var imageVisibility = !(user.IsSuperUser) || UserInfo.IsSuperUser;

            if (imageVisibility)
            {
				imageVisibility = !IsPortalAdministrator(user.UserID)
                                        && (!user.IsInRole(PortalSettings.AdministratorRoleName)
                                            || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)))
                                        && user.UserID != UserId;
            }

            if ((imageVisibility))
            {
                switch (command)
                {
                    case "Delete":
                        if ((user.IsDeleted))
                        {
                            imageVisibility = false;
                        }
                        break;
                    case "Restore":
                    case "Remove":
                        imageVisibility = (user.IsDeleted);
                        break;
                }
            }
            return imageVisibility;
        }

		private bool IsPortalAdministrator(int userId)
		{
			var portalController = new PortalController();
			var groupId = portalController.GetPortal(PortalSettings.PortalId).PortalGroupID;
			if (groupId != Null.NullInteger)
			{
				return PortalGroupController.Instance.GetPortalsByGroup(groupId).Any(p => p.AdministratorId == userId);
			}

			return userId == PortalSettings.AdministratorId;
		}
		
		#endregion

		#region Public Methods

        public string DisplayAddress(object unit, object street, object city, object region, object country, object postalCode)
        {
            var address = Null.NullString;
            try
            {
                address = Globals.FormatAddress(unit, street, city, region, country, postalCode);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return address;
        }

        public string DisplayEmail(string email)
        {
            var displayEmail = Null.NullString;
            try
            {
                if (email != null)
                {
                    displayEmail = HtmlUtils.FormatEmail(email, false);
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return displayEmail;
        }

        public string DisplayDate(DateTime userDate)
        {
            var date = Null.NullString;
            try
            {
                date = !Null.IsNull(userDate) ? userDate.ToString(CultureInfo.InvariantCulture) : "";
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return date;
        }

        protected string FormatURL(string strKeyName, string strKeyValue)
        {
            var url = Null.NullString;
            try
            {
                url = !String.IsNullOrEmpty(Filter) ? EditUrl(strKeyName, strKeyValue, "", "filter=" + Filter) : EditUrl(strKeyName, strKeyValue);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
            return url;
        }

        protected string FilterURL(string filter)
        {
            var parameters = new List<string> {string.Format("pagesize=" + grdUsers.PageSize)};

            if (!String.IsNullOrEmpty(Filter))
            {
                parameters.Add("filter=" + filter);
            }
                
            return Globals.NavigateURL(TabId, "", parameters.ToArray());
        }

		#endregion

		#region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            cmdSearch.Click += OnSearchClick;
            cmdDeleteUnAuthorized.Click += cmdDeleteUnAuthorized_Click;
            cmdRemoveDeleted.Click += cmdRemoveDeleted_Click;
            grdUsers.ItemDataBound += GrdUsersOnItemDataBound;
            grdUsers.ItemCommand += GrdUsersOnItemCommand;
            grdUsers.PreRender += GrdUsersOnPreRender;

            if (!IsPostBack)
            {
                SetInitialPageSize();
            }

            if (Request.QueryString["filter"] != null)
            {
                Filter = Request.QueryString["filter"];
            }
            if (Request.QueryString["filterproperty"] != null)
            {
                FilterProperty = Request.QueryString["filterproperty"];
            }
            if (String.IsNullOrEmpty(Filter))
            {
				//Get Default View
                var setting = UserModuleBase.GetSetting(UsersPortalId, "Display_Mode");
                var mode = (DisplayMode) setting;
                switch (mode)
                {
                    case DisplayMode.All:
                        Filter = Localization.GetString("All");
                        break;
                    case DisplayMode.FirstLetter:
                        Filter = Localization.GetString("Filter.Text", LocalResourceFile).Substring(0, 1);
                        break;
                    case DisplayMode.None:
                        Filter = "None";
                        break;
                }
            }
            foreach (GridColumn column in grdUsers.Columns)
            {
                bool isVisible;
                var header = column.HeaderText;
                if (String.IsNullOrEmpty(header) || header.ToLower() == "username")
                {
                    isVisible = true;
                }
                else
                {
                    var settingKey = "Column_" + header;
                    var setting = UserModuleBase.GetSetting(UsersPortalId, settingKey);
                    isVisible = Convert.ToBoolean(setting);
                }
                if (ReferenceEquals(column.GetType(), typeof (DnnGridImageCommandColumn)))
                {
                    isVisible = ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "EDIT");

                    var imageColumn = (DnnGridImageCommandColumn)column;
					
                	//Manage Edit Column NavigateURLFormatString
                	if (imageColumn.CommandName == "Edit")
                    {
                        //so first create the format string with a dummy value and then
                        //replace the dummy value with the FormatString place holder
                        var formatString = EditUrl("UserId", "KEYFIELD", "Edit", UserFilter(false));
                        formatString = formatString.Replace("KEYFIELD", "{0}");
                        imageColumn.NavigateURLFormatString = formatString;
                    }
					
                    //Manage Roles Column NavigateURLFormatString
                    if (imageColumn.CommandName == "UserRoles")
                    {
                        if (IsHostMenu)
                        {
                            isVisible = false;
                        }
                        else
                        {
							//The Friendly URL parser does not like non-alphanumeric characters
                            //so first create the format string with a dummy value and then
                            //replace the dummy value with the FormatString place holder
                            var formatString = EditUrl("UserId", "KEYFIELD", "User Roles", UserFilter(false));
                            formatString = formatString.Replace("KEYFIELD", "{0}");
                            imageColumn.NavigateURLFormatString = formatString;
                        }
                    }
					
					//Localize Image Column Text
                    if (!String.IsNullOrEmpty(imageColumn.CommandName))
                    {
                        imageColumn.Text = Localization.GetString(imageColumn.CommandName, LocalResourceFile);
                    }
                }
                column.Visible = isVisible;
            }
        }

        private void SetInitialPageSize()
        {
            if (Request.QueryString["pagesize"] != null)
            {
                grdUsers.PageSize = Convert.ToInt32(Request.QueryString["pagesize"]);
            }
            else
            {
                var setting = UserModuleBase.GetSetting(UsersPortalId, "Records_PerPage");
                grdUsers.PageSize = Convert.ToInt32(setting);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (!Page.IsPostBack)
                {
					ClientAPI.RegisterKeyCapture(txtSearch, cmdSearch, 13);
					//Load the Search Combo
                    ddlSearchType.Items.Add(AddSearchItem("Username"));
                    ddlSearchType.Items.Add(AddSearchItem("Email"));
					ddlSearchType.Items.Add(AddSearchItem("DisplayName"));
					var controller = new ListController();
					ListEntryInfo imageDataType = controller.GetListEntryInfo("DataType", "Image");
                    ProfilePropertyDefinitionCollection profileProperties = ProfileController.GetPropertyDefinitionsByPortal(PortalId, false, false);
                    foreach (ProfilePropertyDefinition definition in profileProperties)
                    {
                        if (imageDataType != null && definition.DataType != imageDataType.EntryID)
                        {
							ddlSearchType.Items.Add(AddSearchItem(definition.PropertyName));
                        }
                    }
                    
					//Sent controls to current Filter
					if ((!String.IsNullOrEmpty(Filter) && Filter.ToUpper() != "NONE") && !String.IsNullOrEmpty(FilterProperty))
                    {
                        txtSearch.Text = Filter;
                        ddlSearchType.SelectedValue = FilterProperty;
                    }
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void cmdRemoveDeleted_Click(object sender, EventArgs e)
        {
            RemoveDeletedUsers();
        }

        void cmdDeleteUnAuthorized_Click(object sender, EventArgs e)
        {
            DeleteUnAuthorizedUsers();
        }

        private void OnSearchClick(Object sender, EventArgs e)
        {
            txtSearch.Text = txtSearch.Text.Trim();
            Response.Redirect(Globals.NavigateURL(TabId, "", UserFilter(true)));
        }

        private void GrdUsersOnItemCommand(object source, GridCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "Delete":
                    DeleteUser(GetUserId(e));
                    break;
                case "Remove":
                    RemoveUser(GetUserId(e));
                    break;
                case "Restore":
                    RestoreUser(GetUserId(e));
                    break;
            }
        }

        private int GetUserId(GridCommandEventArgs e)
        {
            return Convert.ToInt32(e.CommandArgument);
        }

        private void DeleteUser(int userId)
        {            
            try
            {
                var user = UserController.GetUserById(UsersPortalId, userId);
                if (user != null)
                {
                    if (UserController.DeleteUser(ref user, true, false))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDeleted", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserDeleteError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }
                if (!String.IsNullOrEmpty(txtSearch.Text))
                {
                    Filter = txtSearch.Text;
                }
                RebindGrid();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void RemoveUser(int userId)
        {            
            try
            {
                UserInfo user = UserController.GetUserById(UsersPortalId, userId);

                if ((user != null))
                {
                    if (UserController.RemoveUser(user))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserRemoved", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserRemoveError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    Filter = txtSearch.Text;
                }
                RebindGrid();

            }
            catch (Exception exc)   //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void RestoreUser(int userId)
        {            
            try
            {
                var user = UserController.GetUserById(UsersPortalId, userId);

                if ((user != null))
                {
                    if (UserController.RestoreUser(ref user))
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserRestored", LocalResourceFile), ModuleMessage.ModuleMessageType.GreenSuccess);
                    }
                    else
                    {
                        UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("UserRestoreError", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
                    }
                }

                if (!string.IsNullOrEmpty(txtSearch.Text))
                {
                    Filter = txtSearch.Text;
                }
                RebindGrid();

                //Module failed to load
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        void GrdUsersOnPreRender(object sender, EventArgs e)
        {
            grdUsers.Columns.FindByUniqueName("UsersOnline").Visible = Entities.Host.Host.EnableUsersOnline;
        }

        private void GrdUsersOnItemDataBound(object sender, GridItemEventArgs e)
        {
            var item = e.Item;
            if (item.ItemType == GridItemType.Item || item.ItemType == GridItemType.AlternatingItem || item.ItemType == GridItemType.SelectedItem)
            {
                var imgApprovedDeleted = item.FindControl("imgApprovedDeleted");
                var imgNotApprovedDeleted = item.FindControl("imgNotApprovedDeleted");
                var imgApproved = item.FindControl("imgApproved");
                var imgNotApproved = item.FindControl("imgNotApproved");

                var user = (UserInfo)item.DataItem;

                if (user == null)
                {
                    return;
                }

                if (user.IsDeleted)
                {
                    foreach (WebControl control in item.Controls)
                    {
                        control.Attributes.Remove("class");
                        control.Attributes.Add("class", "NormalDeleted");
                    }
                    if (imgApprovedDeleted != null && user.Membership.Approved)
                    {
                        imgApprovedDeleted.Visible = true;
                    }
                    else if (imgNotApprovedDeleted != null && !user.Membership.Approved)
                    {
                        imgNotApprovedDeleted.Visible = true;
                    }
                }
                else
                {
                    if (imgApproved != null && user.Membership.Approved)
                    {
                        imgApproved.Visible = true;
                    }
                    else if (imgNotApproved != null && !user.Membership.Approved)
                    {
                        imgNotApproved.Visible = true;
                    }
                }

                var gridDataItem = (GridDataItem)item;

                var editLink = gridDataItem["EditButton"].Controls[0] as HyperLink;
                if (editLink != null)
                {
                    editLink.Visible = (!user.IsInRole(PortalSettings.AdministratorRoleName) || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)));
                    if (editLink.Visible)
                    {
                        if (user.IsSuperUser)
                        {
                            editLink.Visible = PortalSettings.UserInfo.IsSuperUser;
                        }
                    }
                }

                var delete = (DnnImageButton)item.FindControl("Delete");
                delete.Visible = IsCommandAllowed(user, "Delete");
                delete.CommandArgument = user.UserID.ToString(CultureInfo.InvariantCulture);
                delete.ToolTip = Localization.GetString("Delete.Text", LocalResourceFile);

                var restore = (DnnImageButton)item.FindControl("Restore");
                restore.Visible = IsCommandAllowed(user, "Restore");
                restore.CommandArgument = user.UserID.ToString(CultureInfo.InvariantCulture);
                restore.ToolTip = Localization.GetString("Restore.Text", LocalResourceFile);

                var remove = (DnnImageButton)item.FindControl("Remove");
                remove.Visible = IsCommandAllowed(user, "Remove");
                remove.CommandArgument = user.UserID.ToString(CultureInfo.InvariantCulture);
                remove.ToolTip = Localization.GetString("Remove.Text", LocalResourceFile);

                var rolesColumn = gridDataItem["RolesButton"].Controls[0];
                rolesColumn.Visible = !user.IsSuperUser && (!user.IsInRole(PortalSettings.AdministratorRoleName) 
                                                              || (PortalSecurity.IsInRole(PortalSettings.AdministratorRoleName)));

                var onlineControl = (DnnImage)item.FindControl("imgOnline");
                if (onlineControl != null)
                {
                    onlineControl.Visible = user.Membership.IsOnLine;
                    onlineControl.ToolTip = Localization.GetString("Online.Text", LocalResourceFile);
                }
            }
        }

        protected void SetupAddUserLink(object sender, EventArgs e)
        {
            if(IsEditable)
            {
                AddUserLink.Text = Localization.GetString(ModuleActionType.AddContent, LocalResourceFile);
                AddUserLink.NavigateUrl = EditUrl();
            }
            else
            {
                AddUserLink.Visible = false;
            }
        }

        protected void NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            SetGridDataSource();
        }

        #endregion
    }
}