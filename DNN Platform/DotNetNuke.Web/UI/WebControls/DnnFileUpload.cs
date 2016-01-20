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

namespace DotNetNuke.Web.UI.WebControls
{

    [ToolboxData("<{0}:DnnFileUpload runat='server'></{0}:DnnFileUpload>")]
    public class DnnFileUpload : Control, INamingContainer
    {

        private readonly Lazy<DnnFileUploadOptions> _options = new Lazy<DnnFileUploadOptions>(() => new DnnFileUploadOptions());

        public DnnFileUploadOptions Options
        {
            get
            {
                return _options.Value;
            }
        }

        public int ModuleId
        {
            set
            {
                var moduleIdString = value.ToString(CultureInfo.InvariantCulture);
                Options.ModuleId = moduleIdString;
                Options.FolderPicker.Services.ModuleId = moduleIdString;
            }
        }

        public string ParentClientId
        {
            set { Options.ParentClientId = value; }
        }

        public bool ShowOnStartup
        {
            set { Options.ShowOnStartup = value; }
        }

        public string Skin { get; set; }

        public bool SupportHost { get; set; }

        public int Width
        {
            get { return Options.Width; }
            set { Options.Width = value; }
        }

        public int Height
        {
            get { return Options.Height; }
            set { Options.Height = value; }
        }

        public static DnnFileUpload GetCurrent(Page page)
        {
            return page.Items[typeof(DnnFileUpload)] as DnnFileUpload;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RegisterFileUpload(Page);            
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            RegisterClientScript(Page, Skin);
            RegisterStartupScript();
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
            Options.ClientId = ClientID;

            var portalSettings = PortalSettings.Current;

            if (Options.FolderPicker.InitialState == null)
            {
                var folder = FolderManager.Instance.GetFolder(portalSettings.PortalId, string.Empty);
                var rootFolder = (SupportHost && portalSettings.ActiveTab.IsSuperTab) ? DynamicSharedConstants.HostRootFolder : DynamicSharedConstants.RootFolder;

                Options.FolderPicker.InitialState = new DnnDropDownListState
                {
                    SelectedItem = (folder != null) ? new SerializableKeyValuePair<string, string>(folder.FolderID.ToString(CultureInfo.InvariantCulture), rootFolder) : null
                };
            }

            if (Options.Extensions.Count > 0)
            {
                var extensionsText = Options.Extensions.Aggregate(string.Empty, (current, extension) => current.Append(extension, ", "));
                Options.Resources.InvalidFileExtensions = string.Format(Options.Resources.InvalidFileExtensions, extensionsText);
            }

            if (Options.MaxFiles > 0)
            {
                Options.Resources.TooManyFiles = string.Format(Options.Resources.TooManyFiles, Options.MaxFiles.ToString(CultureInfo.InvariantCulture));
            }

            if (!SupportHost)
            {
                Options.FolderPicker.Services.Parameters["portalId"] = portalSettings.PortalId.ToString();
            }
            Options.FolderPicker.Services.GetTreeMethod = "ItemListService/GetFolders";
            Options.FolderPicker.Services.GetNodeDescendantsMethod = "ItemListService/GetFolderDescendants";
            Options.FolderPicker.Services.SearchTreeMethod = "ItemListService/SearchFolders";
            Options.FolderPicker.Services.GetTreeWithNodeMethod = "ItemListService/GetTreePathForFolder";
            Options.FolderPicker.Services.SortTreeMethod = "ItemListService/SortFolders";
            Options.FolderPicker.Services.ServiceRoot = "InternalServices";

            var optionsAsJsonString = Json.Serialize(Options);

            var script = string.Format("dnn.createFileUpload({0});{1}", optionsAsJsonString, Environment.NewLine);

            if (ScriptManager.GetCurrent(Page) != null)
            {
                // respect MS AJAX
                ScriptManager.RegisterStartupScript(Page, GetType(), ClientID + "DnnFileUpload", script, true);
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), ClientID + "DnnFileUpload", script, true);
            }

        }

    }
}
