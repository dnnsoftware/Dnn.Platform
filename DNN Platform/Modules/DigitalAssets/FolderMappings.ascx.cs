// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Application;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.UI.WebControls;
    using Microsoft.Extensions.DependencyInjection;
    using Telerik.Web.UI;

    using Globals = DotNetNuke.Common.Globals;

    public partial class FolderMappings : PortalModuleBase
    {
        private readonly INavigationManager _navigationManager;

        private readonly IFolderMappingController _folderMappingController = FolderMappingController.Instance;

        public FolderMappings()
        {
            this._navigationManager = this.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        public int FolderPortalID
        {
            get
            {
                return this.IsHostMenu ? Null.NullInteger : this.PortalId;
            }
        }

        protected List<FolderMappingInfo> FolderMappingsList
        {
            get
            {
                try
                {
                    var obj = this.Session["FolderMappingsList"];
                    if (obj == null)
                    {
                        obj = this._folderMappingController.GetFolderMappings(this.FolderPortalID);
                        if (obj != null)
                        {
                            this.Session["FolderMappingsList"] = obj;
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
                    this.Session["FolderMappingsList"] = null;
                }

                return new List<FolderMappingInfo>();
            }

            set { this.Session["FolderMappingsList"] = value; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!this.UserInfo.IsSuperUser && !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                this.Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
            this.CancelButton.NavigateUrl = this._navigationManager.NavigateURL();
            this.NewMappingButton.Click += this.OnNewMappingClick;

            if (!this.IsPostBack)
            {
                this.Session["FolderMappingsList"] = null;

                if (this.ModuleConfiguration.ModuleControl.SupportsPopUps)
                {
                    this.MappingsGrid.Rebind();
                }
            }
        }

        protected void MappingsGrid_OnItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(this.TabId, "EditFolderMapping", "mid=" + this.ModuleId, "popUp=true", "ItemID=" + e.CommandArgument.ToString()));
            }
            else
            {
                var folderMappingsList = this.FolderMappingsList;
                var folderMapping = folderMappingsList.Find(f => f.FolderMappingID == int.Parse(e.CommandArgument.ToString()));

                switch (e.CommandName)
                {
                    case "Delete":
                        this._folderMappingController.DeleteFolderMapping(folderMapping.PortalID, folderMapping.FolderMappingID);
                        folderMappingsList.Remove(folderMapping);
                        break;
                    default:
                        break;
                }

                this.FolderMappingsList = folderMappingsList;
                this.MappingsGrid.Rebind();
            }
        }

        protected void MappingsGrid_OnItemDataBound(object sender, GridItemEventArgs e)
        {
            if (e.Item.ItemType != GridItemType.Item && e.Item.ItemType != GridItemType.AlternatingItem)
            {
                return;
            }

            var folderMapping = e.Item.DataItem as FolderMappingInfo;
            if (folderMapping == null || !folderMapping.IsEditable)
            {
                return;
            }

            var cmdEditMapping = e.Item.FindControl("EditMappingButton") as CommandButton;
            if (cmdEditMapping != null)
            {
                cmdEditMapping.ToolTip = Localization.GetString("cmdEdit");
            }

            var cmdDeleteMapping = e.Item.FindControl("DeleteMappingButton") as CommandButton;
            if (cmdDeleteMapping == null)
            {
                return;
            }

            cmdDeleteMapping.ToolTip = Localization.GetString("cmdDelete");

            var deleteMessage = string.Format(Localization.GetString("DeleteConfirm", this.LocalResourceFile), folderMapping.MappingName);
            cmdDeleteMapping.OnClientClick = "return confirm(\"" + ClientAPI.GetSafeJSString(deleteMessage) + "\");";
        }

        protected void MappingsGrid_OnNeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            this.MappingsGrid.DataSource = this.FolderMappingsList;
        }

        protected void OnNewMappingClick(object sender, EventArgs e)
        {
            try
            {
                this.Response.Redirect(this._navigationManager.NavigateURL(this.TabId, "EditFolderMapping", "mid=" + this.ModuleId, "popUp=true"));
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void UpdateFolderMappings(IList<FolderMappingInfo> folderMappingsList)
        {
            for (var i = 3; i < folderMappingsList.Count; i++)
            {
                folderMappingsList[i].Priority = i + 1;
                this._folderMappingController.UpdateFolderMapping(folderMappingsList[i]);
            }
        }
    }
}
