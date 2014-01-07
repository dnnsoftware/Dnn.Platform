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
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.UI.WebControls;
using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Common.Lists
{
	/// <summary>
	/// Manages Entry List
	/// </summary>
	public partial class ListEntries : PortalModuleBase
	{

		#region "Protected Properties"

		/// <summary>
		///     Gets and sets the DefinitionID of the current List
		/// </summary>
		protected int DefinitionID
		{
			get
			{
				if (ViewState["DefinitionID"] == null)
				{
					ViewState["DefinitionID"] = Null.NullInteger;
				}
				return Convert.ToInt32(ViewState["DefinitionID"]);
			}
			set
			{
				ViewState["DefinitionID"] = value;
			}
		}

		/// <summary>
		///     Property to determine if this list has custom sort order
		/// </summary>
		/// <remarks>
		///     Up/Down button in datagrid will be visibled based on this property.
		///     If disable, list will be sorted anphabetically
		/// </remarks>
		protected bool EnableSortOrder
		{
			get
			{
				if (ViewState["EnableSortOrder"] == null)
				{
					ViewState["EnableSortOrder"] = false;
				}
				return Convert.ToBoolean(ViewState["EnableSortOrder"]);
			}
			set
			{
				ViewState["EnableSortOrder"] = value;
			}
		}

		/// <summary>
		/// Gets the selected ListInfo
		/// </summary>
		protected ListInfo SelectedList
		{
			get
			{
				return GetList(SelectedKey, true);
			}
		}

		/// <summary>
		/// Gets the selected collection of List Items
		/// </summary>
		protected IEnumerable<ListEntryInfo> SelectedListItems
		{
			get
			{
				var ctlLists = new ListController();
				if (SelectedList != null)
				{
					return ctlLists.GetListEntryInfoItems(SelectedList.Name, SelectedList.ParentKey, SelectedList.PortalID);
				}

				return ctlLists.GetListEntryInfoItems(ListName, ParentKey, ListPortalID);
			}
		}

		/// <summary>
		///     Property to determine if this list is system (DNN core)
		/// </summary>
		/// <remarks>
		///     Default entries in system list can not be deleted
		///     Entries in system list is sorted anphabetically
		/// </remarks>
		protected bool SystemList
		{
			get
			{
				if (ViewState["SystemList"] == null)
				{
					ViewState["SystemList"] = false;
				}
				return Convert.ToBoolean(ViewState["SystemList"]);
			}
			set
			{
				ViewState["SystemList"] = value;
			}
		}

		#endregion

		#region "Public Properties"

		/// <summary>
		/// Get or set the ListName for this set of List Entries
		/// </summary>
		public string ListName
		{
			get
			{
				return HttpUtility.HtmlEncode(ViewState["ListName"] != null ? ViewState["ListName"].ToString() : "");
			}
			set
			{
				ViewState["ListName"] = value;
			}
		}

		/// <summary>
		/// Gets the portalId for this set of List Entries
		/// </summary>
		public int ListPortalID
		{
			get
			{
				if (ViewState["ListPortalID"] == null)
				{
					ViewState["ListPortalID"] = Null.NullInteger;
				}
				return Convert.ToInt32(ViewState["ListPortalID"]);
			}
			set
			{
				ViewState["ListPortalID"] = value;
			}
		}

		public string Mode
		{
			get
			{
				return ViewState["Mode"] != null ? ViewState["Mode"].ToString() : "";
			}
			set
			{
				ViewState["Mode"] = value;
			}
		}

		/// <summary>
		/// Get or set the ParentKey for this set of List Entries
		/// </summary>
		public string ParentKey
		{
			get
			{
				return HttpUtility.HtmlEncode(ViewState["ParentKey"] != null ? ViewState["ParentKey"].ToString() : "");
			}
			set
			{
				ViewState["ParentKey"] = value;
			}
		}

		/// <summary>
		/// Gets or sets the Selected key
		/// </summary>
		public string SelectedKey
		{
			get
			{
				return ViewState["SelectedKey"] != null ? ViewState["SelectedKey"].ToString() : "";
			}
			set
			{
				ViewState["SelectedKey"] = value;
			}
		}

		public bool ShowDelete
		{
			get
			{
				return ViewState["ShowDelete"] != null && Convert.ToBoolean(ViewState["ShowDelete"]);
			}
			set
			{
				ViewState["ShowDelete"] = value;
			}
		}

		public event EventHandler ListCreated;
		public event EventHandler ListEntryCreated;

		#endregion

		#region "Private Methods"

		/// <summary>
		///     Loads top level entry list
		/// </summary>
		private void BindGrid()
		{
			foreach (GridColumn column in grdEntries.Columns)
			{
				if (ReferenceEquals(column.GetType(), typeof(DnnGridImageCommandColumn)))
				{
					//Manage Delete Confirm JS
					var imageColumn = (DnnGridImageCommandColumn)column;
					if (imageColumn.CommandName == "Delete")
					{
						imageColumn.OnClickJs = Localization.GetString("DeleteItem");
						if (SystemList)
						{
							column.Visible = false;
						}
						else
						{
							column.Visible = true;
						}
					}

					//Localize Image Column Text
					if (!String.IsNullOrEmpty(imageColumn.CommandName))
					{
						imageColumn.Text = Localization.GetString(imageColumn.CommandName, LocalResourceFile);
					}
				}

                else if(ReferenceEquals(column.GetType(), typeof(DnnGridTemplateColumn)))
                {
                    if (EnableSortOrder)
                    {
                        column.Visible = true;
                    }
                    else
                    {
                        column.Visible = false;
                    }
                    
                }
			}
			grdEntries.DataSource = SelectedListItems; //selList
		    grdEntries.DataBind();
            
		    if (SelectedListItems == null)
			{
				lblEntryCount.Text = "0 " + Localization.GetString("Entries", LocalResourceFile);
			}
			else
			{
				lblEntryCount.Text = SelectedListItems.Count() + " " + Localization.GetString("Entries", LocalResourceFile);
				foreach (var item in SelectedListItems)
				{
					//list cannot be deleted if any of the item belongs to host
					if (item.SystemList)
					{
						cmdDeleteList.Visible = false;
						break;
					}
				}
			}
		}

		/// <summary>
		///     Loads top level entry list
		/// </summary>
		private void BindListInfo()
		{
			lblListName.Text = ListName;
			lblListParent.Text = ParentKey;
			rowListParent.Visible = (!String.IsNullOrEmpty(ParentKey));
			chkEnableSortOrder.Checked = EnableSortOrder;
			if (!SystemList && ShowDelete)
			{
				cmdDeleteList.Visible = true;
				ClientAPI.AddButtonConfirm(cmdDeleteList, Localization.GetString("DeleteItem"));
			}
			else
			{
				cmdDeleteList.Visible = false;
			}
			switch (Mode)
			{
				case "ListEntries":
					EnableView(true);
					break;
				case "EditEntry":
					EnableView(false);
					EnableEdit(false);
					break;
				case "AddEntry":
					EnableView(false);
					EnableEdit(false);
					if (SelectedList != null)
					{
						txtParentKey.Text = SelectedList.ParentKey;
					}
					else
					{
						rowEnableSortOrder.Visible = true;
					}
					txtEntryName.Text = ListName;
					rowListName.Visible = false;
					txtEntryValue.Text = "";
					txtEntryText.Text = "";
					cmdSaveEntry.CommandName = "SaveEntry";
					break;
				case "AddList":
					EnableView(false);
					EnableEdit(true);

					rowListName.Visible = true;
					txtParentKey.Text = "";
					txtEntryName.Text = "";
					txtEntryValue.Text = "";
					txtEntryText.Text = "";
					txtEntryName.ReadOnly = false;
					cmdSaveEntry.CommandName = "SaveList";

					var ctlLists = new ListController();

					ddlSelectList.Enabled = true;
					ddlSelectList.DataSource = ctlLists.GetListInfoCollection(string.Empty, string.Empty, PortalSettings.ActiveTab.PortalID);
					ddlSelectList.DataTextField = "DisplayName";
					ddlSelectList.DataValueField = "Key";
					ddlSelectList.DataBind();
					//ddlSelectList.Items.Insert(0, new ListItem(Localization.GetString("None_Specified"), ""));
                    ddlSelectList.InsertItem(0, Localization.GetString("None_Specified"), "");

					//Reset dropdownlist
					ddlSelectParent.ClearSelection();
					ddlSelectParent.Enabled = false;

					break;
			}
		}

		private void DeleteItem(int entryId)
		{
			if (SelectedListItems.Any())
			{
				try
				{
					var ctlLists = new ListController();
					ctlLists.DeleteListEntryByID(entryId, true);
					DataBind();
				}
				catch (Exception exc) //Module failed to load
				{
					Exceptions.ProcessModuleLoadException(this, exc);
				}
			}
			else
			{
				DeleteList();
			}
		}

		private void DeleteList()
		{
			var ctlLists = new ListController();

			ctlLists.DeleteList(SelectedList, true);

			Response.Redirect(Globals.NavigateURL(TabId));
		}

		/// <summary>
		///     Switching to edit mode, change controls visibility for editing depends on AddList params
		/// </summary>
		private void EnableEdit(bool addList)
		{
			rowListdetails.Visible = (!addList);
			rowSelectList.Visible = addList;
			rowSelectParent.Visible = addList;
			rowEnableSortOrder.Visible = addList;
			rowParentKey.Visible = false;
			cmdDelete.Visible = false;
		}

		/// <summary>
		///     Switching to view mode, change controls visibility for viewing
		/// </summary>
		/// <param name="viewMode">Boolean value to determine View or Edit mode</param>
		private void EnableView(bool viewMode)
		{
			rowListdetails.Visible = true;
			rowEntryGrid.Visible = viewMode;
			rowEntryEdit.Visible = (!viewMode);
		}

		private ListInfo GetList(string key, bool update)
		{
			var ctlLists = new ListController();
			int index = key.IndexOf(":", StringComparison.Ordinal);
			string listName = key.Substring(index + 1);
			string parentKey = Null.NullString;
			if (index > 0)
			{
				parentKey = key.Substring(0, index);
			}
			if (update)
			{
				ListName = listName;
				ParentKey = parentKey;
			}
			return ctlLists.GetListInfo(listName, parentKey, ListPortalID);
		}

		/// <summary>
		///     Loads top level entry list
		/// </summary>
		private void InitList()
		{
			if (SelectedList != null)
			{
				DefinitionID = SelectedList.DefinitionID;
				EnableSortOrder = SelectedList.EnableSortOrder;
				SystemList = SelectedList.SystemList;
			}
		}

		#endregion

		#region "Public Methods"

		public override void DataBind()
		{
			InitList();
			BindListInfo();
			BindGrid();
		}

		#endregion

		#region Event Handlers

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);


			grdEntries.ItemCommand += EntriesGridItemCommand;
			grdEntries.ItemDataBound += EntriesGridItemDataBound;
			ddlSelectList.SelectedIndexChanged += SelectListIndexChanged;
			cmdAddEntry.Click += OnAddEntryClick;
			cmdCancel.Click += OnCancelClick;
			cmdDelete.Click += OnDeleteClick;
			cmdDeleteList.Click += OnDeleteListClick;
			cmdSaveEntry.Click += OnSaveEntryClick;

			try
			{
				if (!Page.IsPostBack)
				{
					Mode = "ListEntries";
				}
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		/// <summary>
		///     Handles events when clicking image button in the grid (Edit/Up/Down)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="e"></param>
		protected void EntriesGridItemCommand(object source, GridCommandEventArgs e)
		{
			try
			{
				var ctlLists = new ListController();
			    int entryID = Convert.ToInt32(((GridDataItem) e.Item).GetDataKeyValue("EntryID"));

				switch (e.CommandName.ToLower())
				{
					case "delete":
						Mode = "ListEntries";
						DeleteItem(entryID);
						break;
					case "edit":
						Mode = "EditEntry";

						ListEntryInfo entry = ctlLists.GetListEntryInfo(entryID);
						txtEntryID.Text = entryID.ToString(CultureInfo.InvariantCulture);
						txtParentKey.Text = entry.ParentKey;
						txtEntryValue.Text = entry.Value;
						txtEntryText.Text = entry.Text;
						rowListName.Visible = false;
						cmdSaveEntry.CommandName = "Update";

						if (!SystemList)
						{
							cmdDelete.Visible = true;
							ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("DeleteItem"));
						}
						else
						{
							cmdDelete.Visible = false;
						}
				        e.Canceled = true;  //stop the grid from providing inline editing
						DataBind();
						break;
					case "up":
						ctlLists.UpdateListSortOrder(entryID, true);
						DataBind();
						break;
					case "down":
						ctlLists.UpdateListSortOrder(entryID, false);
						DataBind();
						break;
				}
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void EntriesGridItemDataBound(object sender, GridItemEventArgs e)
		{
			if (e.Item.ItemType == GridItemType.Item || e.Item.ItemType == GridItemType.EditItem || e.Item.ItemType == GridItemType.AlternatingItem || e.Item.ItemType == GridItemType.SelectedItem)
			{
	            var entry = (ListEntryInfo)e.Item.DataItem;
				if (entry != null)
				{
					//Hide Edit option for system list
                    var length = e.Item.Controls.Count;
					var editCommand = e.Item.Controls[length - 2].Controls[0] as ImageButton;
					if (editCommand != null)
					{
						editCommand.Visible = PortalSettings.ActiveTab.IsSuperTab || !(entry.SystemList || entry.PortalID == Null.NullInteger);
					}

					//Hide Delete option for system list
					var delCommand = e.Item.Controls[length - 1].Controls[0] as ImageButton;
					if (delCommand != null)
					{
						delCommand.Visible = PortalSettings.ActiveTab.IsSuperTab || !(entry.SystemList || entry.PortalID == Null.NullInteger);
					}
				}
			}
		}

		/// <summary>
		///     Select a list in dropdownlist
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>        
		protected void SelectListIndexChanged(object sender, EventArgs e)
		{
			var ctlLists = new ListController();
			if (!String.IsNullOrEmpty(ddlSelectList.SelectedValue))
			{
				ListInfo selList = GetList(ddlSelectList.SelectedItem.Value, false);
				{
					ddlSelectParent.Enabled = true;
					ddlSelectParent.DataSource = ctlLists.GetListEntryInfoItems(selList.Name, selList.ParentKey);
					ddlSelectParent.DataTextField = "DisplayName";
					ddlSelectParent.DataValueField = "EntryID";
					ddlSelectParent.DataBind();
				}
			}
			else
			{
				ddlSelectParent.Enabled = false;
				ddlSelectParent.Items.Clear();
			}
		}

		/// <summary>
		///     Handles Add New Entry command
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		///     Using "CommandName" property of cmdSaveEntry to determine this is a new entry of an existing list
		/// </remarks>
		protected void OnAddEntryClick(object sender, EventArgs e)
		{
			Mode = "AddEntry";
			DataBind();
		}

		/// <summary>
		///     Handles cmdSaveEntry.Click
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		///     Using "CommandName" property of cmdSaveEntry to determine action to take (ListUpdate/AddEntry/AddList)
		/// </remarks>
		protected void OnSaveEntryClick(object sender, EventArgs e)
		{
		    String entryValue;
		    String entryText;
            if (UserInfo.IsSuperUser)
            {
                entryValue = txtEntryValue.Text;
                entryText = txtEntryText.Text;
            }
            else
            {
                var ps = new PortalSecurity();

                entryValue = ps.InputFilter(txtEntryValue.Text, PortalSecurity.FilterFlag.NoScripting);
                entryText = ps.InputFilter(txtEntryText.Text, PortalSecurity.FilterFlag.NoScripting);
            }
			var listController = new ListController();
			var entry = new ListEntryInfo();
			{
				entry.DefinitionID = Null.NullInteger;
				entry.PortalID = ListPortalID;
				entry.ListName = txtEntryName.Text;
                entry.Value = entryValue;
                entry.Text = entryText;
			}
			if (Page.IsValid)
			{
				Mode = "ListEntries";
				switch (cmdSaveEntry.CommandName.ToLower())
				{
					case "update":
						entry.ParentKey = SelectedList.ParentKey;
						entry.EntryID = Int16.Parse(txtEntryID.Text);
						bool canUpdate = true;
						foreach (var curEntry in listController.GetListEntryInfoItems(SelectedList.Name, entry.ParentKey, entry.PortalID))
						{
							if (entry.EntryID != curEntry.EntryID) //not the same item we are trying to update
							{
								if (entry.Value == curEntry.Value && entry.Text == curEntry.Text)
								{
									UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ItemAlreadyPresent", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
									canUpdate = false;
									break;
								}

							}
						}

						if (canUpdate)
						{
							listController.UpdateListEntry(entry);
							DataBind();
						}
						break;
					case "saveentry":
						if (SelectedList != null)
						{
							entry.ParentKey = SelectedList.ParentKey;
							entry.ParentID = SelectedList.ParentID;
							entry.Level = SelectedList.Level;
						}
						if (chkEnableSortOrder.Checked)
						{
							entry.SortOrder = 1;
						}
						else
						{
							entry.SortOrder = 0;
						}

						if (listController.AddListEntry(entry) == Null.NullInteger) //entry already found in database
						{
							UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ItemAlreadyPresent", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
						}


						DataBind();
						break;
					case "savelist":
						if (ddlSelectParent.SelectedIndex != -1)
						{
							int parentID = Int32.Parse(ddlSelectParent.SelectedItem.Value);
							ListEntryInfo parentEntry = listController.GetListEntryInfo(parentID);
							entry.ParentID = parentID;
							entry.DefinitionID = parentEntry.DefinitionID;
							entry.Level = parentEntry.Level + 1;
							entry.ParentKey = parentEntry.Key;
						}
						if (chkEnableSortOrder.Checked)
						{
							entry.SortOrder = 1;
						}
						else
						{
							entry.SortOrder = 0;
						}

						if (listController.AddListEntry(entry) == Null.NullInteger) //entry already found in database
						{
							UI.Skins.Skin.AddModuleMessage(this, Localization.GetString("ItemAlreadyPresent", LocalResourceFile), ModuleMessage.ModuleMessageType.RedError);
						}
						else
						{
							SelectedKey = entry.ParentKey.Replace(":", ".") + ":" + entry.ListName;
							Response.Redirect(Globals.NavigateURL(TabId, "", "Key=" + SelectedKey));
						}
						break;
				}
			}
		}

		/// <summary>
		///     Delete List
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void OnDeleteListClick(object sender, EventArgs e)
		{
			DeleteList();
		}

		/// <summary>
		///     Delete List
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		/// <remarks>
		///     If deleting entry is not the last one in the list, rebinding the grid, otherwise return back to main page (rebinding DNNTree)
		/// </remarks>
		protected void OnDeleteClick(object sender, EventArgs e)
		{
			DeleteItem(Convert.ToInt32(txtEntryID.Text));
		}

		/// <summary>
		///     Cancel
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>       
		protected void OnCancelClick(object sender, EventArgs e)
		{
			try
			{
				Mode = "ListEntries";
				if (!String.IsNullOrEmpty(SelectedKey))
				{
					DataBind();
				}
				else
				{
					Response.Redirect(Globals.NavigateURL(TabId));
				}
			}
			catch (Exception exc) //Module failed to load
			{
				Exceptions.ProcessModuleLoadException(this, exc);
			}
		}

		protected void OnListCreated(EventArgs e)
		{
			if (ListCreated != null)
			{
				ListCreated(this, e);
			}
		}

		protected void OnListEntryCreated(EventArgs e)
		{
			if (ListEntryCreated != null)
			{
				ListEntryCreated(this, e);
			}
		}

		#endregion

	}
}