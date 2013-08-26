#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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

using System;
using System.Collections.Generic;

using DotNetNuke.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using DotNetNuke.UI.WebControls;
using Telerik.Web.UI;
using Globals = DotNetNuke.Common.Globals;

namespace DotNetNuke.Modules.DigitalAssets
{
    public partial class FolderMappings : PortalModuleBase
    {
        #region Private Variables

        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;

        #endregion

        #region Properties

        public int FolderPortalID
        {
            get
            {
                return IsHostMenu ? Null.NullInteger : PortalId;
            }
        }

        protected List<FolderMappingInfo> FolderMappingsList
        {
            get
            {
                try
                {
                    var obj = Session["FolderMappingsList"];
                    if (obj == null)
                    {
                        obj = _folderMappingController.GetFolderMappings(FolderPortalID);
                        if (obj != null)
                        {
                            Session["FolderMappingsList"] = obj;
                        }
                        else
                        {
                            obj = new List<FolderMappingInfo>();
                        }
                    }
                    return (List<FolderMappingInfo>)obj;
                }
                catch
                {
                    Session["FolderMappingsList"] = null;
                }
                return new List<FolderMappingInfo>();
            }
            set { Session["FolderMappingsList"] = value; }
        }

        #endregion

        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ClientAPI.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);
            CancelButton.NavigateUrl = Globals.NavigateURL();
            NewMappingButton.Click += OnNewMappingClick;

            if (!IsPostBack)
            {
                Session["FolderMappingsList"] = null;

                if (ModuleConfiguration.ModuleControl.SupportsPopUps)
                {
                    MappingsGrid.Rebind();
                }
            }
            if (DotNetNukeContext.Current.Application.Name == "DNNCORP.CE")
            {
                NewMappingButton.Visible = false;
            }
        }

        protected void MappingsGrid_OnItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                Response.Redirect(Globals.NavigateURL(TabId, "EditFolderMapping", "mid=" + ModuleId, "popUp=true", "ItemID=" + e.CommandArgument.ToString()));
            }
            else
            {
                var folderMappingsList = FolderMappingsList;
                var folderMapping = folderMappingsList.Find(f => f.FolderMappingID == int.Parse(e.CommandArgument.ToString()));

                switch (e.CommandName)
                {
                    case "Delete":
                        _folderMappingController.DeleteFolderMapping(folderMapping.PortalID, folderMapping.FolderMappingID);
                        folderMappingsList.Remove(folderMapping);
                        break;
                    default:
                        break;
                }

                FolderMappingsList = folderMappingsList;
                MappingsGrid.Rebind();
            }
        }

        protected void MappingsGrid_OnItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType != GridItemType.Item && e.Item.ItemType != GridItemType.AlternatingItem) return;

            var folderMapping = (e.Item.DataItem as FolderMappingInfo);
            if (folderMapping == null || !folderMapping.IsEditable)
            {
                return;
            }

            var cmdEditMapping = (e.Item.FindControl("EditMappingButton") as CommandButton);
            if (cmdEditMapping != null) cmdEditMapping.ToolTip = Localization.GetString("cmdEdit");

            var cmdDeleteMapping = (e.Item.FindControl("DeleteMappingButton") as CommandButton);
            if (cmdDeleteMapping == null) return;

            cmdDeleteMapping.ToolTip = Localization.GetString("cmdDelete");

            var deleteMessage = string.Format(Localization.GetString("DeleteConfirm", LocalResourceFile), folderMapping.MappingName);
            cmdDeleteMapping.OnClientClick = "return confirm(\"" + ClientAPI.GetSafeJSString(deleteMessage) + "\");";
        }

        protected void MappingsGrid_OnNeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            MappingsGrid.DataSource = FolderMappingsList;
        }

        protected void OnNewMappingClick(object sender, EventArgs e)
        {
            try
            {
                Response.Redirect(Globals.NavigateURL(TabId, "EditFolderMapping", "mid=" + ModuleId, "popUp=true"));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Private Methods

        private void UpdateFolderMappings(IList<FolderMappingInfo> folderMappingsList)
        {
            for (var i = 3; i < folderMappingsList.Count; i++)
            {
                folderMappingsList[i].Priority = i + 1;
                _folderMappingController.UpdateFolderMapping(folderMappingsList[i]);
            }
        }

        #endregion

    }
}