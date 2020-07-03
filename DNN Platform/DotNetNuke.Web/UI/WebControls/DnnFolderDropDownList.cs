// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    [ToolboxData("<{0}:DnnFolderDropDownList runat='server'></{0}:DnnFolderDropDownList>")]
    public class DnnFolderDropDownList : DnnDropDownList
    {
        /// <summary>
        /// Gets or sets the selected Folder in the control, or selects the Folder in the control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFolderInfo SelectedFolder
        {
            get
            {
                var folderId = this.SelectedItemValueAsInt;
                return (folderId == Null.NullInteger) ? null : FolderManager.Instance.GetFolder(folderId);
            }

            set
            {
                var folderName = value != null ? value.FolderName : null;
                if (folderName == string.Empty)
                {
                    folderName = PortalSettings.Current.ActiveTab.IsSuperTab ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;
                }

                this.SelectedItem = (value != null) ? new ListItem() { Text = folderName, Value = value.FolderID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.SelectItemDefaultText = Localization.GetString("DropDownList.SelectFolderDefaultText", Localization.SharedResourceFile);
            this.Services.GetTreeMethod = "ItemListService/GetFolders";
            this.Services.GetNodeDescendantsMethod = "ItemListService/GetFolderDescendants";
            this.Services.SearchTreeMethod = "ItemListService/SearchFolders";
            this.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForFolder";
            this.Services.SortTreeMethod = "ItemListService/SortFolders";
            this.Services.ServiceRoot = "InternalServices";

            this.Options.ItemList.DisableUnspecifiedOrder = true;
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("folder");
            base.OnPreRender(e);

            // add the selected folder's level path so that it can expand to the selected node in client side.
            var selectedFolder = this.SelectedFolder;
            if (selectedFolder != null && selectedFolder.ParentID > Null.NullInteger)
            {
                var folderLevel = string.Empty;
                var parentFolder = FolderManager.Instance.GetFolder(selectedFolder.ParentID);
                while (parentFolder != null)
                {
                    folderLevel = string.Format("{0},{1}", parentFolder.FolderID, folderLevel);
                    parentFolder = (parentFolder.ParentID < 0) ? null : FolderManager.Instance.GetFolder(parentFolder.ParentID);
                }

                this.ExpandPath = folderLevel.TrimEnd(',');
            }
        }
    }
}
