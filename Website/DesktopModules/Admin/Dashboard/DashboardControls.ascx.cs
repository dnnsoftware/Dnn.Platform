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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Modules.Dashboard.Components;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Dashboard
{

    public partial class DashboardControls : PortalModuleBase
    {

        #region Private Members

        private const int COLUMN_ENABLED = 5;
        private const int COLUMN_MOVE_DOWN = 1;
        private const int COLUMN_MOVE_UP = 2;
        private List<DashboardControl> _DashboardControls;

        private static bool SupportsRichClient()
        {
            return ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML);
        }

        #endregion

        protected List<DashboardControl> DashboardControlList
        {
            get
            {
                return _DashboardControls ?? (_DashboardControls = DashboardController.GetDashboardControls(false));
            }
        }

        #region Private Methods

        private void DeleteControl(int index)
        {
            var dashboardControl = DashboardControlList[index];

            var returnUrl = Server.UrlEncode(UrlUtils.PopUpUrl(Globals.NavigateURL(TabId, "DashboardControls", "mid=" + ModuleId), this, PortalSettings, false, true));
            Response.Redirect(Util.UnInstallURL(TabId, dashboardControl.PackageID, returnUrl), true);
        }

        private void MoveControl(int index, int destIndex)
        {
            var dashboardControl = DashboardControlList[index];
            var nextControl = DashboardControlList[destIndex];

            var currentOrder = dashboardControl.ViewOrder;
            var nextOrder = nextControl.ViewOrder;

            //Swap ViewOrders
            dashboardControl.ViewOrder = nextOrder;
            nextControl.ViewOrder = currentOrder;

            //Refresh Grid
            DashboardControlList.Sort();
            BindGrid();
        }

        private void MoveControlDown(int index)
        {
            MoveControl(index, index + 1);
        }

        private void MoveControlUp(int index)
        {
            MoveControl(index, index - 1);
        }

        private void BindGrid()
        {
            var allEnabled = true;

            //Check whether the checkbox column headers are true or false
            foreach (var dashboardControl in DashboardControlList)
            {
                if (dashboardControl.IsEnabled == false)
                {
                    allEnabled = false;
                }
                if (!allEnabled)
                {
                    break;
                }
            }
            foreach (DataGridColumn column in grdDashboardControls.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (CheckBoxColumn)))
                {
					//Manage CheckBox column events
                    var cbColumn = (CheckBoxColumn) column;
                    if (cbColumn.DataField == "IsEnabled")
                    {
                        cbColumn.Checked = allEnabled;
                    }
                }
            }
            grdDashboardControls.DataSource = DashboardControlList;
            grdDashboardControls.DataBind();
        }

        private void RefreshGrid()
        {
            _DashboardControls = null;
            BindGrid();
        }

        private void UpdateControls()
        {
            foreach (var dashboardControl in DashboardControlList)
            {
                if (dashboardControl.IsDirty)
                {
                    DashboardController.UpdateDashboardControl(dashboardControl);
                }
            }
        }

        private void ProcessPostBack()
        {
            try
            {
                var aryNewOrder = ClientAPI.GetClientSideReorder(grdDashboardControls.ClientID, Page);
                DashboardControl dashboardControl;
                DataGridItem objItem;
                CheckBox chk;
                for (var i = 0; i <= grdDashboardControls.Items.Count - 1; i++)
                {
                    objItem = grdDashboardControls.Items[i];
                    dashboardControl = DashboardControlList[i];
                    chk = (CheckBox) objItem.Cells[COLUMN_ENABLED].Controls[0];
                    dashboardControl.IsEnabled = chk.Checked;
                }
				//assign vieworder
                for (var i = 0; i <= aryNewOrder.Length - 1; i++)
                {
                    DashboardControlList[Convert.ToInt32(aryNewOrder[i])].ViewOrder = i + 1;
                }
                DashboardControlList.Sort();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region Event Handlers

        protected override void LoadViewState(object savedState)
        {
            if ((savedState != null))
            {
				//Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[]) savedState;
				
				//Load Base Controls ViewState
                if ((myState[0] != null))
                {
                    base.LoadViewState(myState[0]);
                }
				
				//Load ModuleID
                if ((myState[1] != null))
                {
                    _DashboardControls = (List<DashboardControl>) myState[1];
                }
            }
        }

        protected override object SaveViewState()
        {
            var allStates = new object[2];
			
			//Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();

            //Save the Profile Properties
            allStates[1] = DashboardControlList;

            return allStates;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            foreach (DataGridColumn column in grdDashboardControls.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof (CheckBoxColumn)))
                {
                    if (SupportsRichClient() == false)
                    {
                        var cbColumn = (CheckBoxColumn) column;
                        cbColumn.CheckedChanged += OnDashboardControlsItemChecked;
                    }
                }
                else if (ReferenceEquals(column.GetType(), typeof (ImageCommandColumn)))
                {
					//Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn) column;
                    switch (imageColumn.CommandName)
                    {
                        case "Delete":
                            imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                            imageColumn.Text = Localization.GetString("Delete", LocalResourceFile);
                            break;
                        case "MoveUp":
                            imageColumn.Text = Localization.GetString("MoveUp", LocalResourceFile);
                            break;
                        case "MoveDown":
                            imageColumn.Text = Localization.GetString("MoveDown", LocalResourceFile);
                            break;
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdRefresh.Click += OnRefreshClick;
            cmdUpdate.Click += OnUpdateClick;
            grdDashboardControls.ItemCommand += OnDashboardControlsItemCommand;
            grdDashboardControls.ItemCreated += OnDashboardControlsItemCreated;
            grdDashboardControls.ItemDataBound += OnDashboardControlsItemDataBound;

            try
            {
                if (!Page.IsPostBack)
                {
                    cmdCancel.NavigateUrl = Globals.NavigateURL();
                    cmdInstall.NavigateUrl = Util.InstallURL(TabId, Server.UrlEncode(Globals.NavigateURL(TabId, "DashboardControls", "mid=" + ModuleId)), "DashboardControl");

                    Localization.LocalizeDataGrid(ref grdDashboardControls, LocalResourceFile);
                    BindGrid();
                }
                else
                {
                    ProcessPostBack();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnRefreshClick(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        protected void OnUpdateClick(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();

                RefreshGrid();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void OnDashboardControlsItemChecked(object sender, DNNDataGridCheckChangedEventArgs e)
        {
            var propertyName = e.Field;
            var propertyValue = e.Checked;
            var isAll = e.IsAll;
            var index = e.Item.ItemIndex;

            DashboardControl dashboardControl;

            if (isAll)
            {
				//Update All the properties
                foreach (DashboardControl dashboard in DashboardControlList)
                {
                    switch (propertyName)
                    {
                        case "IsEnabled":
                            dashboard.IsEnabled = propertyValue;
                            break;
                    }
                }
            }
            else
            {
				//Update the indexed property
                dashboardControl = DashboardControlList[index];
                switch (propertyName)
                {
                    case "IsEnabled":
                        dashboardControl.IsEnabled = propertyValue;
                        break;
                }
            }
            BindGrid();
        }

        protected void OnDashboardControlsItemCommand(object source, DataGridCommandEventArgs e)
        {
            var commandName = e.CommandName;
            var index = e.Item.ItemIndex;

            switch (commandName)
            {
                case "Delete":
                    DeleteControl(index);
                    break;
                case "MoveUp":
                    MoveControlUp(index);
                    break;
                case "MoveDown":
                    MoveControlDown(index);
                    break;
            }
        }

        protected void OnDashboardControlsItemCreated(object sender, DataGridItemEventArgs e)
        {
            if (SupportsRichClient())
            {
                switch (e.Item.ItemType)
                {
                    case ListItemType.Header:
                        //we combined the header label and checkbox in same place, so it is control 1 instead of 0
                        ((WebControl) e.Item.Cells[COLUMN_ENABLED].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + COLUMN_ENABLED + ");");
                        ((CheckBox) e.Item.Cells[COLUMN_ENABLED].Controls[1]).AutoPostBack = false;
                        break;
                    case ListItemType.AlternatingItem:
                    case ListItemType.Item:
                        ((CheckBox) e.Item.Cells[COLUMN_ENABLED].Controls[0]).AutoPostBack = false;

                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_DOWN].Controls[0], Page, false, grdDashboardControls.ClientID);
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_UP].Controls[0], Page, true, grdDashboardControls.ClientID);
                        break;
                }
            }
        }

        protected void OnDashboardControlsItemDataBound(object sender, DataGridItemEventArgs e)
        {
            var item = e.Item;
            switch (item.ItemType)
            {
                case ListItemType.SelectedItem:
                case ListItemType.AlternatingItem:
                case ListItemType.Item:
                    {
                        var imgColumnControl = item.Controls[0].Controls[0];
                        if (imgColumnControl is ImageButton)
                        {
                            var delImage = (ImageButton) imgColumnControl;
                            var dashboardControl = (DashboardControl) item.DataItem;

                            switch (dashboardControl.DashboardControlKey)
                            {
                                case "Server":
                                case "Database":
                                case "Host":
                                case "Portals":
                                case "Modules":
                                case "Skins":
                                    delImage.Visible = false;
                                    break;
                                default:
                                    delImage.Visible = true;
                                    break;
                            }
                        }
                    }
                    break;
            }
        }

        #endregion

    }
}