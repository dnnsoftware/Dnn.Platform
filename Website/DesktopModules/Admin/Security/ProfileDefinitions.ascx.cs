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
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Modules.Admin.Users
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileDefinitions PortalModuleBase is used to manage the Profile Properties
    /// for a portal
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[cnurse]	02/16/2006  Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public partial class ProfileDefinitions : PortalModuleBase, IActionable
    {
        #region Constants

        private const int COLUMN_REQUIRED = 11;
        private const int COLUMN_VISIBLE = 12;
        private const int COLUMN_MOVE_DOWN = 2;
        private const int COLUMN_MOVE_UP = 3;

        #endregion

        #region Private Members

        private ProfilePropertyDefinitionCollection _profileProperties;
        private bool _requiredColumnHidden = false;

        #endregion

        #region Protected Members

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets whether we are dealing with SuperUsers
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected bool IsSuperUser
        {
            get
            {
            	return Globals.IsHostTab(PortalSettings.ActiveTab.TabID);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of Profile Proeprties
        /// </summary>
        /// <history>
        /// 	[cnurse]	12/03/2008  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected ProfilePropertyDefinitionCollection ProfileProperties
        {
            get
            {
                return _profileProperties ?? (_profileProperties = ProfileController.GetPropertyDefinitionsByPortal(UsersPortalId, false, false));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page
        /// </summary>
        /// <history>
        /// 	[cnurse]	03/09/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public string ReturnUrl
        {
            get
            {
                string returnURL;
                var filterParams = new string[String.IsNullOrEmpty(Request.QueryString["filterproperty"]) ? 1 : 2];

                if (String.IsNullOrEmpty(Request.QueryString["filterProperty"]))
                {
                    filterParams.SetValue("filter=" + Request.QueryString["filter"], 0);
                }
                else
                {
                    filterParams.SetValue("filter=" + Request.QueryString["filter"], 0);
                    filterParams.SetValue("filterProperty=" + Request.QueryString["filterProperty"], 1);
                }
                if (string.IsNullOrEmpty(Request.QueryString["filter"]))
                {
                    returnURL = Globals.NavigateURL(TabId);
                }
                else
                {
                    returnURL = Globals.NavigateURL(TabId, "", filterParams);
                }
                return returnURL;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Portal Id whose Users we are managing
        /// </summary>
        /// <history>
        /// 	[cnurse]	05/11/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected int UsersPortalId
        {
            get
            {
                int intPortalId = PortalId;
                if (IsSuperUser)
                {
                    intPortalId = Null.NullInteger;
                }
                return intPortalId;
            }
        }

        #endregion

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                actions.Add(GetNextActionID(),
                            Localization.GetString(ModuleActionType.AddContent, LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "add.gif",
                            EditUrl("EditProfileProperty"),
                            false,
                            SecurityAccessLevel.Admin,
                            true,
                            false);
                actions.Add(GetNextActionID(),
                            Localization.GetString("Cancel.Action", LocalResourceFile),
                            ModuleActionType.AddContent,
                            "",
                            "lt.gif",
                            ReturnUrl,
                            false,
                            SecurityAccessLevel.Admin,
                            true,
                            false);
                return actions;
            }
        }

        #endregion

        #region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Helper function that determines whether the client-side functionality is possible
        /// </summary>
        /// <history>
        ///     [Jon Henning]	03/12/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private bool SupportsRichClient()
        {
            return ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a property
        /// </summary>
        /// <param name="index">The index of the Property to delete</param>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void DeleteProperty(int index)
        {
            ProfileController.DeletePropertyDefinition(ProfileProperties[index]);

            RefreshGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property
        /// </summary>
        /// <param name="index">The index of the Property to move</param>
        /// <param name="destIndex">The new index of the Property</param>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MoveProperty(int index, int destIndex)
        {
            ProfilePropertyDefinition profileProperty = ProfileProperties[index];
            ProfilePropertyDefinition nextProfileProperty = ProfileProperties[destIndex];

            int currentOrder = profileProperty.ViewOrder;
            int nextOrder = nextProfileProperty.ViewOrder;

            //Swap ViewOrders
            profileProperty.ViewOrder = nextOrder;
            nextProfileProperty.ViewOrder = currentOrder;

            //Refresh Grid
            ProfileProperties.Sort();
            BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property down in the ViewOrder
        /// </summary>
        /// <param name="index">The index of the Property to move</param>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MovePropertyDown(int index)
        {
            MoveProperty(index, index + 1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property up in the ViewOrder
        /// </summary>
        /// <param name="index">The index of the Property to move</param>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void MovePropertyUp(int index)
        {
            MoveProperty(index, index - 1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Binds the Property Collection to the Grid
        /// </summary>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void BindGrid()
        {
            bool allRequired = true;
            bool allVisible = true;

            //Check whether the checkbox column headers are true or false
            foreach (ProfilePropertyDefinition profProperty in ProfileProperties)
            {
                if (profProperty.Required == false)
                {
                    allRequired = false;
                }
                if (profProperty.Visible == false)
                {
                    allVisible = false;
                }
                if (!allRequired && !allVisible)
                {
                    break;
                }
            }
            foreach (DataGridColumn column in grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    //Manage CheckBox column events
                    var checkBoxColumn = (CheckBoxColumn)column;
                    if (checkBoxColumn.DataField == "Required")
                    {
                        checkBoxColumn.Checked = allRequired;
                    }
                    if (checkBoxColumn.DataField == "Visible")
                    {
                        checkBoxColumn.Checked = allVisible;
                    }
                }
            }
            grdProfileProperties.DataSource = ProfileProperties;
            grdProfileProperties.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Refresh the Property Collection to the Grid
        /// </summary>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void RefreshGrid()
        {
            _profileProperties = null;
            BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates any "dirty" properties
        /// </summary>
        /// <history>
        ///     [cnurse]	02/23/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void UpdateProperties()
        {
            ProcessPostBack();
            foreach (ProfilePropertyDefinition property in ProfileProperties)
            {
                if (property.IsDirty)
                {
                    if (UsersPortalId == Null.NullInteger)
                    {
                        property.Required = false;
                    }
                    ProfileController.UpdatePropertyDefinition(property);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// This method is responsible for taking in posted information from the grid and
        /// persisting it to the property definition collection
        /// </summary>
        /// <history>
        ///     [Jon Henning]	03/12/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void ProcessPostBack()
        {

            string[] newOrder = ClientAPI.GetClientSideReorder(grdProfileProperties.ClientID, Page);
            for (int i = 0; i <= grdProfileProperties.Items.Count - 1; i++)
            {
                DataGridItem dataGridItem = grdProfileProperties.Items[i];
                ProfilePropertyDefinition profileProperty = ProfileProperties[i];
                CheckBox checkBox = (CheckBox)dataGridItem.Cells[COLUMN_REQUIRED].Controls[0];
                profileProperty.Required = checkBox.Checked;
                checkBox = (CheckBox)dataGridItem.Cells[COLUMN_VISIBLE].Controls[0];
                profileProperty.Visible = checkBox.Checked;
            }

            //assign vieworder
            for (int i = 0; i <= newOrder.Length - 1; i++)
            {
                ProfileProperties[Convert.ToInt32(newOrder[i])].ViewOrder = i;
            }
            ProfileProperties.Sort();
        }

        #endregion

        #region Protected Methods

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                //Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                //Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                //Load ModuleID
                if (myState[1] != null)
                {
                    _profileProperties = (ProfilePropertyDefinitionCollection)myState[1];
                }
            }
        }

        protected override object SaveViewState()
        {
            var allStates = new object[2];

            //Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();
            allStates[1] = ProfileProperties;

            return allStates;
        }

        #endregion

        #region Public Methods

        public string DisplayDataType(ProfilePropertyDefinition definition)
        {
            string retValue = Null.NullString;
            var listController = new ListController();
            ListEntryInfo definitionEntry = listController.GetListEntryInfo("DataType", definition.DataType);
            if (definitionEntry != null)
            {
                retValue = definitionEntry.Value;
            }
            return retValue;
        }

        public string DisplayDefaultVisibility(ProfilePropertyDefinition definition)
        {
            string retValue = Null.NullString;
            if (!String.IsNullOrEmpty(definition.DefaultVisibility.ToString()))
            {
                retValue = LocalizeString(definition.DefaultVisibility.ToString()) ?? definition.DefaultVisibility.ToString();
            }
            return retValue;
        }

        public void Update()
        {
            try
            {
                UpdateProperties();

                //Redirect to upadte page
                Response.Redirect(Request.RawUrl, true);
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }            
        }

        #endregion

        #region Event Handlers

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/16/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            foreach (DataGridColumn column in grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    var checkBoxColumn = (CheckBoxColumn)column;
                    if (checkBoxColumn.DataField == "Required" && UsersPortalId == Null.NullInteger)
                    {
                        checkBoxColumn.Visible = false;
                        _requiredColumnHidden = true;
                    }
                    if (SupportsRichClient() == false)
                    {
                        checkBoxColumn.CheckedChanged += grdProfileProperties_ItemCheckedChanged;
                    }
                }
                else if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    //Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn)column;
                    switch (imageColumn.CommandName)
                    {
                        case "Delete":
                            imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                            imageColumn.Text = Localization.GetString("Delete", LocalResourceFile);
                            break;
                        case "Edit":
                            //The Friendly URL parser does not like non-alphanumeric characters
                            //so first create the format string with a dummy value and then
                            //replace the dummy value with the FormatString place holder
                            string formatString = EditUrl("PropertyDefinitionID", "KEYFIELD", "EditProfileProperty");
                            formatString = formatString.Replace("KEYFIELD", "{0}");
                            imageColumn.NavigateURLFormatString = formatString;
                            imageColumn.Text = Localization.GetString("Edit", LocalResourceFile);
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

            cmdRefresh.Click += cmdRefresh_Click;
            grdProfileProperties.ItemCommand += grdProfileProperties_ItemCommand;
            grdProfileProperties.ItemCreated += grdProfileProperties_ItemCreated;
            grdProfileProperties.ItemDataBound += grdProfileProperties_ItemDataBound;

            cmdAdd.NavigateUrl = EditUrl("EditProfileProperty");

            try
            {
                if (!Page.IsPostBack)
                {
                    Localization.LocalizeDataGrid(ref grdProfileProperties, LocalResourceFile);
                    BindGrid();
                }
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRefresh_Click runs when the refresh button is clciked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/23/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            RefreshGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemCheckedChanged runs when a checkbox in the grid
        /// is clicked
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/23/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
            string propertyName = e.Field;
            bool propertyValue = e.Checked;

            if (e.IsAll)
            {
                //Update All the properties
                foreach (ProfilePropertyDefinition profProperty in ProfileProperties)
                {
                    switch (propertyName)
                    {
                        case "Required":
                            profProperty.Required = propertyValue;
                            break;
                        case "Visible":
                            profProperty.Visible = propertyValue;
                            break;
                    }
                }
            }
            else
            {
                //Update the indexed property
                ProfilePropertyDefinition profileProperty = ProfileProperties[e.Item.ItemIndex];
                switch (propertyName)
                {
                    case "Required":
                        profileProperty.Required = propertyValue;
                        break;
                    case "Visible":
                        profileProperty.Visible = propertyValue;
                        break;
                }
            }
            BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemCommand runs when a Command event is raised in the
        /// Grid
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/23/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            int index = e.Item.ItemIndex;

            switch (e.CommandName)
            {
                case "Delete":
                    DeleteProperty(index);
                    break;
                case "MoveUp":
                    MovePropertyUp(index);
                    break;
                case "MoveDown":
                    MovePropertyDown(index);
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// When it is determined that the client supports a rich interactivity the grdProfileProperties_ItemCreated 
        /// event is responsible for disabling all the unneeded AutoPostBacks, along with assiging the appropriate
        ///	client-side script for each event handler
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [Jon Henning]	03/12/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCreated(object sender, DataGridItemEventArgs e)
        {
            if (SupportsRichClient())
            {
                switch (e.Item.ItemType)
                {
                    case ListItemType.Header:
                        //we combined the header label and checkbox in same place, so it is control 1 instead of 0                       
                        ((WebControl)e.Item.Cells[COLUMN_REQUIRED].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + COLUMN_REQUIRED + ");");
                        ((CheckBox)e.Item.Cells[COLUMN_REQUIRED].Controls[1]).AutoPostBack = false;

                        int column_visible = _requiredColumnHidden ? COLUMN_VISIBLE - 1 : COLUMN_VISIBLE;
                        ((WebControl)e.Item.Cells[COLUMN_VISIBLE].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + column_visible + ");");
                        ((CheckBox)e.Item.Cells[COLUMN_VISIBLE].Controls[1]).AutoPostBack = false;
                        break;
                    case ListItemType.AlternatingItem:
                    case ListItemType.Item:
                        ((CheckBox)e.Item.Cells[COLUMN_REQUIRED].Controls[0]).AutoPostBack = false;
                        ((CheckBox)e.Item.Cells[COLUMN_VISIBLE].Controls[0]).AutoPostBack = false;
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_DOWN].Controls[0], Page, false, grdProfileProperties.ClientID);
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_UP].Controls[0], Page, true, grdProfileProperties.ClientID);
                        break;
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemDataBound runs when a row in the grid is bound to its data source
        /// Grid
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	02/06/2007  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        protected void grdProfileProperties_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            DataGridItem item = e.Item;
            if (item.ItemType == ListItemType.Item || item.ItemType == ListItemType.AlternatingItem || item.ItemType == ListItemType.SelectedItem)
            {
                Control imgColumnControl = item.Controls[1].Controls[0];
                if (imgColumnControl is ImageButton)
                {
                    var delImage = (ImageButton)imgColumnControl;
                    var profProperty = (ProfilePropertyDefinition)item.DataItem;

                    switch (profProperty.PropertyName.ToLower())
                    {
                        case "lastname":
                        case "firstname":
                        case "preferredtimezone":
                        case "preferredlocale":
                            delImage.Visible = false;
                            break;
                        default:
                            delImage.Visible = true;
                            break;
                    }
                }
            }
        }

        #endregion
    }
}