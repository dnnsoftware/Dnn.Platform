using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;

namespace DotNetNuke.Web.UI.WebControls
{

    [ToolboxData("<{0}:DnnFileUpload runat='server'></{0}:DnnFileUpload>")]
    public class DnnFileUpload : Control, INamingContainer
    {

        private readonly Lazy<DnnFileUploadOptions> _options =
            new Lazy<DnnFileUploadOptions>(() => new DnnFileUploadOptions());

        private DnnFileUploadOptions Options
        {
            get
            {
                return _options.Value;
            }
        }

        public string Skin { get; set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (DesignMode)
            {
                return;
            }
            if (GetCurrent(Page) != null)
            {
                throw new InvalidOperationException("Only one instance of the DnnFileUpload can be contained by the page.");
            }
            Page.Items[typeof(DnnFileUpload)] = this;
        }

        public static DnnFileUpload GetCurrent(Page page)
        {
            return page.Items[typeof(DnnFileUpload)] as DnnFileUpload;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            RegisterClientScript(Page, Skin);
            RegisterStartupScript();
        }

        private static void RegisterClientScript(Page page, string skin)
        {
            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();
            jQuery.RegisterFileUpload(page);

            DnnDropDownList.RegisterClientScript(page, skin);

            ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload.css");
            if (!string.IsNullOrEmpty(skin))
            {
                ClientResourceManager.RegisterStyleSheet(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload." + skin + ".css");
            }

            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.WebResourceUrl.js", FileOrder.Js.DefaultPriority + 2);
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/scripts/dnn.jquery.extensions.js", FileOrder.Js.DefaultPriority + 3);
            ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Components/FileUpload/dnn.FileUpload.js", FileOrder.Js.DefaultPriority + 4);
        }

        private void RegisterStartupScript()
        {

            var folder = FolderManager.Instance.GetFolder(PortalSettings.Current.PortalId, string.Empty);
            var SelectedItem = (folder != null) ? new ListItem() { Text = folder.FolderName, Value = folder.FolderID.ToString(CultureInfo.InvariantCulture) } : null;



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
