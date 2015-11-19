using System;
using System.ComponentModel;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Common;
using DotNetNuke.Web.UI.WebControls.Extensions;

namespace DotNetNuke.Web.UI.WebControls
{
    [ToolboxData("<{0}:DnnFileDropDownList runat='server'></{0}:DnnFileDropDownList>")]
    public class DnnFileDropDownList : DnnDropDownList
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            SelectItemDefaultText = Localization.GetString("DropDownList.SelectFileDefaultText", Localization.SharedResourceFile);
            Services.GetTreeMethod = "ItemListService/GetFiles";
            Services.SearchTreeMethod = "ItemListService/SearchFiles";
            Services.SortTreeMethod = "ItemListService/SortFiles";
            Services.ServiceRoot = "InternalServices";
            Options.ItemList.DisableUnspecifiedOrder = true;

            FolderId = Null.NullInteger;
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("file");

            if (IncludeNoneSpecificItem)
            {
                UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, Null.NullInteger.ToString(CultureInfo.InvariantCulture));
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Gets the selected Folder in the control, or selects the Folder in the control.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFileInfo SelectedFile
        {
            get
            {
                var fileId = SelectedItemValueAsInt;
                return (fileId == Null.NullInteger) ? null : FileManager.Instance.GetFile(fileId);
            }
            set
            {
                SelectedItem = (value != null) ? new ListItem() { Text = value.FileName, Value = value.FileId.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        public int FolderId
        {
            get
            {
                return Services.Parameters.ContainsKey("parentId") ? Convert.ToInt32(Services.Parameters["parentId"]) : Null.NullInteger;
            }
            set
            {
                Services.Parameters["parentId"] = value.ToString();
            }
        }

        public string Filter
        {
            get { return Services.Parameters.ContainsKey("filter") ? Services.Parameters["filter"] : string.Empty; }
            set { Services.Parameters["filter"] = value; }
        }

        public bool IncludeNoneSpecificItem { get; set; }
    }
}
