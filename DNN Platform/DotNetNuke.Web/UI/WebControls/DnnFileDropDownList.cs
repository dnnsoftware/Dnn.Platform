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
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    [ToolboxData("<{0}:DnnFileDropDownList runat='server'></{0}:DnnFileDropDownList>")]
    public class DnnFileDropDownList : DnnDropDownList
    {
        /// <summary>
        /// Gets or sets the selected Folder in the control, or selects the Folder in the control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IFileInfo SelectedFile
        {
            get
            {
                var fileId = this.SelectedItemValueAsInt;
                return (fileId == Null.NullInteger) ? null : FileManager.Instance.GetFile(fileId);
            }

            set
            {
                this.SelectedItem = (value != null) ? new ListItem() { Text = value.FileName, Value = value.FileId.ToString(CultureInfo.InvariantCulture) } : null;
            }
        }

        public int FolderId
        {
            get
            {
                return this.Services.Parameters.ContainsKey("parentId") ? Convert.ToInt32(this.Services.Parameters["parentId"]) : Null.NullInteger;
            }

            set
            {
                this.Services.Parameters["parentId"] = value.ToString();
            }
        }

        public string Filter
        {
            get { return this.Services.Parameters.ContainsKey("filter") ? this.Services.Parameters["filter"] : string.Empty; }
            set { this.Services.Parameters["filter"] = value; }
        }

        public bool IncludeNoneSpecificItem { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.SelectItemDefaultText = Localization.GetString("DropDownList.SelectFileDefaultText", Localization.SharedResourceFile);
            this.Services.GetTreeMethod = "ItemListService/GetFiles";
            this.Services.SearchTreeMethod = "ItemListService/SearchFiles";
            this.Services.SortTreeMethod = "ItemListService/SortFiles";
            this.Services.ServiceRoot = "InternalServices";
            this.Options.ItemList.DisableUnspecifiedOrder = true;

            this.FolderId = Null.NullInteger;
        }

        protected override void OnPreRender(EventArgs e)
        {
            this.AddCssClass("file");

            if (this.IncludeNoneSpecificItem)
            {
                this.UndefinedItem = new ListItem(DynamicSharedConstants.Unspecified, Null.NullInteger.ToString(CultureInfo.InvariantCulture));
            }

            base.OnPreRender(e);
        }
    }
}
