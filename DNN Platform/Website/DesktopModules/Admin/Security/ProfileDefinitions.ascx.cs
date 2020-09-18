// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Admin.Users
{
    using System;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions;
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
    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The ProfileDefinitions PortalModuleBase is used to manage the Profile Properties
    /// for a portal.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public partial class ProfileDefinitions : PortalModuleBase, IActionable
    {
        private const int COLUMN_REQUIRED = 11;
        private const int COLUMN_VISIBLE = 12;
        private const int COLUMN_MOVE_DOWN = 2;
        private const int COLUMN_MOVE_UP = 3;

        private readonly INavigationManager _navigationManager;
        private ProfilePropertyDefinitionCollection _profileProperties;
        private bool _requiredColumnHidden = false;

        public ProfileDefinitions()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Return Url for the page.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public string ReturnUrl
        {
            get
            {
                string returnURL;
                var filterParams = new string[string.IsNullOrEmpty(this.Request.QueryString["filterproperty"]) ? 1 : 2];

                if (string.IsNullOrEmpty(this.Request.QueryString["filterProperty"]))
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                }
                else
                {
                    filterParams.SetValue("filter=" + this.Request.QueryString["filter"], 0);
                    filterParams.SetValue("filterProperty=" + this.Request.QueryString["filterProperty"], 1);
                }

                if (string.IsNullOrEmpty(this.Request.QueryString["filter"]))
                {
                    returnURL = this._navigationManager.NavigateURL(this.TabId);
                }
                else
                {
                    returnURL = this._navigationManager.NavigateURL(this.TabId, string.Empty, filterParams);
                }

                return returnURL;
            }
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString(ModuleActionType.AddContent, this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "add.gif",
                    this.EditUrl("EditProfileProperty"),
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);
                actions.Add(
                    this.GetNextActionID(),
                    Localization.GetString("Cancel.Action", this.LocalResourceFile),
                    ModuleActionType.AddContent,
                    string.Empty,
                    "lt.gif",
                    this.ReturnUrl,
                    false,
                    SecurityAccessLevel.Admin,
                    true,
                    false);
                return actions;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets a value indicating whether gets whether we are dealing with SuperUsers.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected bool IsSuperUser
        {
            get
            {
                return Globals.IsHostTab(this.PortalSettings.ActiveTab.TabID);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the collection of Profile Proeprties.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected ProfilePropertyDefinitionCollection ProfileProperties
        {
            get
            {
                return this._profileProperties ?? (this._profileProperties = ProfileController.GetPropertyDefinitionsByPortal(this.UsersPortalId, false, false));
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the Portal Id whose Users we are managing.
        /// </summary>
        /// -----------------------------------------------------------------------------
        protected int UsersPortalId
        {
            get
            {
                int intPortalId = this.PortalId;
                if (this.IsSuperUser)
                {
                    intPortalId = Null.NullInteger;
                }

                return intPortalId;
            }
        }

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
            if (!string.IsNullOrEmpty(definition.DefaultVisibility.ToString()))
            {
                retValue = this.LocalizeString(definition.DefaultVisibility.ToString()) ?? definition.DefaultVisibility.ToString();
            }

            return retValue;
        }

        public void Update()
        {
            try
            {
                this.UpdateProperties();

                // Redirect to upadte page
                this.Response.Redirect(this.Request.RawUrl, true);
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected override void LoadViewState(object savedState)
        {
            if (savedState != null)
            {
                // Load State from the array of objects that was saved with SaveViewState.
                var myState = (object[])savedState;

                // Load Base Controls ViewState
                if (myState[0] != null)
                {
                    base.LoadViewState(myState[0]);
                }

                // Load ModuleID
                if (myState[1] != null)
                {
                    this._profileProperties = (ProfilePropertyDefinitionCollection)myState[1];
                }
            }
        }

        protected override object SaveViewState()
        {
            var allStates = new object[2];

            // Save the Base Controls ViewState
            allStates[0] = base.SaveViewState();
            allStates[1] = this.ProfileProperties;

            return allStates;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Init runs when the control is initialised.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            foreach (DataGridColumn column in this.grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    var checkBoxColumn = (CheckBoxColumn)column;
                    if (checkBoxColumn.DataField == "Required" && this.UsersPortalId == Null.NullInteger)
                    {
                        checkBoxColumn.Visible = false;
                        this._requiredColumnHidden = true;
                    }

                    if (this.SupportsRichClient() == false)
                    {
                        checkBoxColumn.CheckedChanged += this.grdProfileProperties_ItemCheckedChanged;
                    }
                }
                else if (ReferenceEquals(column.GetType(), typeof(ImageCommandColumn)))
                {
                    // Manage Delete Confirm JS
                    var imageColumn = (ImageCommandColumn)column;
                    switch (imageColumn.CommandName)
                    {
                        case "Delete":
                            imageColumn.OnClickJS = Localization.GetString("DeleteItem");
                            imageColumn.Text = Localization.GetString("Delete", this.LocalResourceFile);
                            break;
                        case "Edit":
                            // The Friendly URL parser does not like non-alphanumeric characters
                            // so first create the format string with a dummy value and then
                            // replace the dummy value with the FormatString place holder
                            string formatString = this.EditUrl("PropertyDefinitionID", "KEYFIELD", "EditProfileProperty");
                            formatString = formatString.Replace("KEYFIELD", "{0}");
                            imageColumn.NavigateURLFormatString = formatString;
                            imageColumn.Text = Localization.GetString("Edit", this.LocalResourceFile);
                            break;
                        case "MoveUp":
                            imageColumn.Text = Localization.GetString("MoveUp", this.LocalResourceFile);
                            break;
                        case "MoveDown":
                            imageColumn.Text = Localization.GetString("MoveDown", this.LocalResourceFile);
                            break;
                    }
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.cmdRefresh.Click += this.cmdRefresh_Click;
            this.grdProfileProperties.ItemCommand += this.grdProfileProperties_ItemCommand;
            this.grdProfileProperties.ItemCreated += this.grdProfileProperties_ItemCreated;
            this.grdProfileProperties.ItemDataBound += this.grdProfileProperties_ItemDataBound;

            this.cmdAdd.NavigateUrl = this.EditUrl("EditProfileProperty");

            try
            {
                if (!this.Page.IsPostBack)
                {
                    Localization.LocalizeDataGrid(ref this.grdProfileProperties, this.LocalResourceFile);
                    this.BindGrid();
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemDataBound runs when a row in the grid is bound to its data source
        /// Grid.
        /// </summary>
        /// <remarks>
        /// </remarks>
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

                    switch (profProperty.PropertyName.ToLowerInvariant())
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Helper function that determines whether the client-side functionality is possible.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private bool SupportsRichClient()
        {
            return ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.DHTML);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a property.
        /// </summary>
        /// <param name="index">The index of the Property to delete.</param>
        /// -----------------------------------------------------------------------------
        private void DeleteProperty(int index)
        {
            ProfileController.DeletePropertyDefinition(this.ProfileProperties[index]);

            this.RefreshGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property.
        /// </summary>
        /// <param name="index">The index of the Property to move.</param>
        /// <param name="destIndex">The new index of the Property.</param>
        /// -----------------------------------------------------------------------------
        private void MoveProperty(int index, int destIndex)
        {
            ProfilePropertyDefinition profileProperty = this.ProfileProperties[index];
            ProfilePropertyDefinition nextProfileProperty = this.ProfileProperties[destIndex];

            int currentOrder = profileProperty.ViewOrder;
            int nextOrder = nextProfileProperty.ViewOrder;

            // Swap ViewOrders
            profileProperty.ViewOrder = nextOrder;
            nextProfileProperty.ViewOrder = currentOrder;

            // Refresh Grid
            this.ProfileProperties.Sort();
            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property down in the ViewOrder.
        /// </summary>
        /// <param name="index">The index of the Property to move.</param>
        /// -----------------------------------------------------------------------------
        private void MovePropertyDown(int index)
        {
            this.MoveProperty(index, index + 1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Moves a property up in the ViewOrder.
        /// </summary>
        /// <param name="index">The index of the Property to move.</param>
        /// -----------------------------------------------------------------------------
        private void MovePropertyUp(int index)
        {
            this.MoveProperty(index, index - 1);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Binds the Property Collection to the Grid.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void BindGrid()
        {
            bool allRequired = true;
            bool allVisible = true;

            // Check whether the checkbox column headers are true or false
            foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
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

            foreach (DataGridColumn column in this.grdProfileProperties.Columns)
            {
                if (ReferenceEquals(column.GetType(), typeof(CheckBoxColumn)))
                {
                    // Manage CheckBox column events
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

            this.grdProfileProperties.DataSource = this.ProfileProperties;
            this.grdProfileProperties.DataBind();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Refresh the Property Collection to the Grid.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void RefreshGrid()
        {
            this._profileProperties = null;
            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates any "dirty" properties.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void UpdateProperties()
        {
            this.ProcessPostBack();
            foreach (ProfilePropertyDefinition property in this.ProfileProperties)
            {
                if (property.IsDirty)
                {
                    if (this.UsersPortalId == Null.NullInteger)
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
        /// persisting it to the property definition collection.
        /// </summary>
        /// -----------------------------------------------------------------------------
        private void ProcessPostBack()
        {
            string[] newOrder = ClientAPI.GetClientSideReorder(this.grdProfileProperties.ClientID, this.Page);
            for (int i = 0; i <= this.grdProfileProperties.Items.Count - 1; i++)
            {
                DataGridItem dataGridItem = this.grdProfileProperties.Items[i];
                ProfilePropertyDefinition profileProperty = this.ProfileProperties[i];
                CheckBox checkBox = (CheckBox)dataGridItem.Cells[COLUMN_REQUIRED].Controls[0];
                profileProperty.Required = checkBox.Checked;
                checkBox = (CheckBox)dataGridItem.Cells[COLUMN_VISIBLE].Controls[0];
                profileProperty.Visible = checkBox.Checked;
            }

            // assign vieworder
            for (int i = 0; i <= newOrder.Length - 1; i++)
            {
                this.ProfileProperties[Convert.ToInt32(newOrder[i])].ViewOrder = i;
            }

            this.ProfileProperties.Sort();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// cmdRefresh_Click runs when the refresh button is clciked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void cmdRefresh_Click(object sender, EventArgs e)
        {
            this.RefreshGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemCheckedChanged runs when a checkbox in the grid
        /// is clicked.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCheckedChanged(object sender, DNNDataGridCheckChangedEventArgs e)
        {
            string propertyName = e.Field;
            bool propertyValue = e.Checked;

            if (e.IsAll)
            {
                // Update All the properties
                foreach (ProfilePropertyDefinition profProperty in this.ProfileProperties)
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
                // Update the indexed property
                ProfilePropertyDefinition profileProperty = this.ProfileProperties[e.Item.ItemIndex];
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

            this.BindGrid();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// grdProfileProperties_ItemCommand runs when a Command event is raised in the
        /// Grid.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCommand(object source, DataGridCommandEventArgs e)
        {
            int index = e.Item.ItemIndex;

            switch (e.CommandName)
            {
                case "Delete":
                    this.DeleteProperty(index);
                    break;
                case "MoveUp":
                    this.MovePropertyUp(index);
                    break;
                case "MoveDown":
                    this.MovePropertyDown(index);
                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// When it is determined that the client supports a rich interactivity the grdProfileProperties_ItemCreated
        /// event is responsible for disabling all the unneeded AutoPostBacks, along with assiging the appropriate
        ///     client-side script for each event handler.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void grdProfileProperties_ItemCreated(object sender, DataGridItemEventArgs e)
        {
            if (this.SupportsRichClient())
            {
                switch (e.Item.ItemType)
                {
                    case ListItemType.Header:
                        // we combined the header label and checkbox in same place, so it is control 1 instead of 0
                        ((WebControl)e.Item.Cells[COLUMN_REQUIRED].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + COLUMN_REQUIRED + ");");
                        ((CheckBox)e.Item.Cells[COLUMN_REQUIRED].Controls[1]).AutoPostBack = false;

                        int column_visible = this._requiredColumnHidden ? COLUMN_VISIBLE - 1 : COLUMN_VISIBLE;
                        ((WebControl)e.Item.Cells[COLUMN_VISIBLE].Controls[1]).Attributes.Add("onclick", "dnn.util.checkallChecked(this," + column_visible + ");");
                        ((CheckBox)e.Item.Cells[COLUMN_VISIBLE].Controls[1]).AutoPostBack = false;
                        break;
                    case ListItemType.AlternatingItem:
                    case ListItemType.Item:
                        ((CheckBox)e.Item.Cells[COLUMN_REQUIRED].Controls[0]).AutoPostBack = false;
                        ((CheckBox)e.Item.Cells[COLUMN_VISIBLE].Controls[0]).AutoPostBack = false;
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_DOWN].Controls[0], this.Page, false, this.grdProfileProperties.ClientID);
                        ClientAPI.EnableClientSideReorder(e.Item.Cells[COLUMN_MOVE_UP].Controls[0], this.Page, true, this.grdProfileProperties.ClientID);
                        break;
                }
            }
        }
    }
}
