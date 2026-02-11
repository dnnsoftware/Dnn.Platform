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

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Common;
    using DotNetNuke.Web.UI.WebControls.Extensions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A file dropdown control.</summary>
    [ToolboxData("<{0}:DnnFileDropDownList runat='server'></{0}:DnnFileDropDownList>")]
    public class DnnFileDropDownList : DnnDropDownList
    {
        /// <summary>Initializes a new instance of the <see cref="DnnFileDropDownList"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IClientResourceController. Scheduled removal in v12.0.0.")]
        public DnnFileDropDownList()
            : this(Globals.GetCurrentServiceProvider().GetRequiredService<IClientResourceController>(), Globals.GetCurrentServiceProvider().GetRequiredService<IServicesFramework>())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFileDropDownList"/> class.</summary>
        /// <param name="clientResourceController">The client resource controller.</param>
        /// <param name="servicesFramework">The web API service framework.</param>
        public DnnFileDropDownList(IClientResourceController clientResourceController, IServicesFramework servicesFramework)
            : base(clientResourceController, servicesFramework)
        {
        }

        /// <summary>Gets or sets the selected Folder in the control, or selects the Folder in the control.</summary>
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

        /// <summary>Gets or sets the folder ID.</summary>
        public int FolderId
        {
            get
            {
                return this.Services.Parameters.TryGetValue("parentId", out var parentId) ? Convert.ToInt32(parentId, CultureInfo.InvariantCulture) : Null.NullInteger;
            }

            set
            {
                this.Services.Parameters["parentId"] = value.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary>Gets or sets the filter.</summary>
        public string Filter
        {
            get { return this.Services.Parameters.TryGetValue("filter", out var filter) ? filter : string.Empty; }
            set { this.Services.Parameters["filter"] = value; }
        }

        /// <summary>Gets or sets a value indicating whether to include a "none specified" item.</summary>
        public bool IncludeNoneSpecificItem { get; set; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
