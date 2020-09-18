// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Web.Services.Description;
    using System.Web.UI;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.Client;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using DotNetNuke.Web.Common;

    [ToolboxData("<{0}:DnnFileUpload runat='server'></{0}:DnnFileUpload>")]
    public class DnnFileUpload : Control, INamingContainer
    {
        private readonly Lazy<DnnFileUploadOptions> _options = new Lazy<DnnFileUploadOptions>(() => new DnnFileUploadOptions());

        public DnnFileUploadOptions Options
        {
            get
            {
                return this._options.Value;
            }
        }

        public int ModuleId
        {
            set
            {
                var moduleIdString = value.ToString(CultureInfo.InvariantCulture);
                this.Options.ModuleId = moduleIdString;
                this.Options.FolderPicker.Services.ModuleId = moduleIdString;
            }
        }

        public string ParentClientId
        {
            set { this.Options.ParentClientId = value; }
        }

        public bool ShowOnStartup
        {
            set { this.Options.ShowOnStartup = value; }
        }

        public string Skin { get; set; }

        public bool SupportHost { get; set; }

        public int Width
        {
            get { return this.Options.Width; }
            set { this.Options.Width = value; }
        }

        public int Height
        {
            get { return this.Options.Height; }
            set { this.Options.Height = value; }
        }

        public static DnnFileUpload GetCurrent(Page page)
        {
            return page.Items[typeof(DnnFileUpload)] as DnnFileUpload;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RegisterFileUpload(this.Page);
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterClientScript(this.Page, this.Skin);
            this.RegisterStartupScript();
        }

        private static void RegisterClientScript(Page page, string skin)
        {
            DnnDropDownList.RegisterClientScript(page, skin);

            ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload.css", FileOrder.Css.ResourceCss);
            if (!string.IsNullOrEmpty(skin))
            {
                ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload." + skin + ".css", FileOrder.Css.ResourceCss);
            }

            JavaScript.RequestRegistration(CommonJs.jQueryUI);

            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.WebResourceUrl.js", FileOrder.Js.DefaultPriority + 2);
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js", FileOrder.Js.DefaultPriority + 3);
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload.js", FileOrder.Js.DefaultPriority + 4);
        }

        private void RegisterStartupScript()
        {
            this.Options.ClientId = this.ClientID;

            var portalSettings = PortalSettings.Current;

            if (this.Options.FolderPicker.InitialState == null)
            {
                var folder = FolderManager.Instance.GetFolder(portalSettings.PortalId, string.Empty);
                var rootFolder = (this.SupportHost && portalSettings.ActiveTab.IsSuperTab) ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;

                this.Options.FolderPicker.InitialState = new DnnDropDownListState
                {
                    SelectedItem = (folder != null) ? new SerializableKeyValuePair<string, string>(folder.FolderID.ToString(CultureInfo.InvariantCulture), rootFolder) : null,
                };
            }

            if (this.Options.Extensions.Count > 0)
            {
                var extensionsText = this.Options.Extensions.Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                this.Options.Resources.InvalidFileExtensions = string.Format(this.Options.Resources.InvalidFileExtensions, extensionsText);
            }

            if (this.Options.MaxFiles > 0)
            {
                this.Options.Resources.TooManyFiles = string.Format(this.Options.Resources.TooManyFiles, this.Options.MaxFiles.ToString(CultureInfo.InvariantCulture));
            }

            if (!this.SupportHost)
            {
                this.Options.FolderPicker.Services.Parameters["portalId"] = portalSettings.PortalId.ToString();
            }

            this.Options.FolderPicker.Services.GetTreeMethod = "ItemListService/GetFolders";
            this.Options.FolderPicker.Services.GetNodeDescendantsMethod = "ItemListService/GetFolderDescendants";
            this.Options.FolderPicker.Services.SearchTreeMethod = "ItemListService/SearchFolders";
            this.Options.FolderPicker.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForFolder";
            this.Options.FolderPicker.Services.SortTreeMethod = "ItemListService/SortFolders";
            this.Options.FolderPicker.Services.ServiceRoot = "InternalServices";

            var optionsAsJsonString = Json.Serialize(this.Options);

            var script = string.Format("dnn.createFileUpload({0});{1}", optionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(this.Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(this.Page, this.GetType(), this.ClientID + "DnnFileUpload", script, true);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), this.ClientID + "DnnFileUpload", script, true);
            }
        }
    }
}
