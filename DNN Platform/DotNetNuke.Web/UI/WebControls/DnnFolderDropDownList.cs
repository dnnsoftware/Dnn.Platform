using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.UI.WebControls.Extensions;

namespace DotNetNuke.Web.UI.WebControls
{
    [ToolboxData("<{0}:DnnFolderDropDownList runat='server'></{0}:DnnFolderDropDownList>")]
    public class DnnFolderDropDownList : DnnDropDownList
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SelectItemDefaultText = Localization.GetString("DropDownList.SelectFolderDefaultText", Localization.SharedResourceFile);
            Services.GetTreeMethod = "ItemListService/GetFolders";
            Services.GetNodeDescendantsMethod = "ItemListService/GetFolderDescendants";
            Services.SearchTreeMethod = "ItemListService/SearchFolders";
            Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForFolder";
            Services.SortTreeMethod = "ItemListService/SortFolders";
            Services.ServiceRoot = "InternalServices";
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("folder");
            base.OnPreRender(e);
        }

        /// <summary>
        /// Gets the selected Folder in the control, or selects the Folder in the control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFolderInfo SelectedFolder
        {
            get
            {
                var folderId = SelectedItemValueAsInt;
                return (folderId == Null.NullInteger) ? null : CBOWrapper.Instance.FillObject<FolderInfo>(DataProvider.Instance().GetFolder(folderId));
            }
            set
            {
                SelectedItem = (value != null) ? new ListItem() { Text = value.FolderName, Value = value.FolderID.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

    }
}
