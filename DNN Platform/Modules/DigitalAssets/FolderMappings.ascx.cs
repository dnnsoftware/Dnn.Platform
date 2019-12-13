using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

using DotNetNuke.Application;
using DotNetNuke.Abstractions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework.JavaScriptLibraries;
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
        private readonly INavigationManager _navigationManager;
        public FolderMappings()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!UserInfo.IsSuperUser && !UserInfo.IsInRole(PortalSettings.AdministratorRoleName))
            {
                Response.Redirect(Globals.AccessDeniedURL(), true);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn);
            CancelButton.NavigateUrl = _navigationManager.NavigateURL();
            NewMappingButton.Click += OnNewMappingClick;

            if (!IsPostBack)
            {
                Session["FolderMappingsList"] = null;

                if (ModuleConfiguration.ModuleControl.SupportsPopUps)
                {
                    MappingsGrid.Rebind();
                }
            }
        }

        protected void MappingsGrid_OnItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName == "Edit")
            {
                Response.Redirect(_navigationManager.NavigateURL(TabId, "EditFolderMapping", "mid=" + ModuleId, "popUp=true", "ItemID=" + e.CommandArgument.ToString()));
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
                Response.Redirect(_navigationManager.NavigateURL(TabId, "EditFolderMapping", "mid=" + ModuleId, "popUp=true"));
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
