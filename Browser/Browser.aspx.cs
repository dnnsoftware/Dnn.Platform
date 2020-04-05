using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Controls;
using DNNConnect.CKEditorProvider.Objects;
using DNNConnect.CKEditorProvider.Utilities;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Utilities;
using Microsoft.JScript;
using Convert = System.Convert;
using Encoder = System.Drawing.Imaging.Encoder;
using Globals = DotNetNuke.Common.Globals;
using Image = System.Drawing.Image;
using DNNConnect.CKEditorProvider.Helper;

namespace DNNConnect.CKEditorProvider.Browser
{
    /// <summary>
    /// The browser.
    /// </summary>
    [ScriptService]
    public partial class Browser : Page
    {
        #region Constants and Fields

        private const string fileItemDisplayFormat =
            "<span class=\"FileName\">{2}</span><br /><span class=\"FileInfo\">{0}: {3}</span><br /><span class=\"FileInfo\">{1}: {4}</span>";
        /// <summary>
        /// The Image or Link that is selected inside the Editor.
        /// </summary>
        private static string ckFileUrl;

        /// <summary>
        ///   The allowed flash extensions.
        /// </summary>
        private static readonly ISet<string> AllowedFlashExtensions = new HashSet<string>(new[] { "swf", "flv", "mp3" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///   The allowed image extensions.
        /// </summary>
        private static readonly ISet<string> AllowedImageExtensions = new HashSet<string>(new[] { "bmp", "gif", "jpeg", "jpg", "png", "svg" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>
        ///   The request.
        /// </summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>
        /// Current Settings Base
        /// </summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>
        ///   The _portal settings.
        /// </summary>
        private PortalSettings _portalSettings;

        /// <summary>
        ///   The extension white list.
        /// </summary>
        private FileExtensionWhitelist extensionWhiteList;

        /// <summary>
        /// The browser modus
        /// </summary>
        private string browserModus;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the accept file types.
        /// </summary>
        /// <value>
        /// The accept file types.
        /// </value>
        public string AcceptFileTypes
        {
            get
            {
                return ViewState["AcceptFileTypes"] != null ? ViewState["AcceptFileTypes"].ToString() : ".*";
            }

            set
            {
                ViewState["AcceptFileTypes"] = value;
            }
        }

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LanguageCode
        {
            get
            {
                return !string.IsNullOrEmpty(request.QueryString["lang"])
                           ? request.QueryString["lang"]
                           : "en-US";
            }
        }

        /// <summary>
        /// Gets the Name for the Current Resource file name
        /// </summary>
        /// <value>
        /// The resource executable file.
        /// </value>
        protected string ResXFile
        {
            get
            {
                string[] page = Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    TemplateSourceDirectory.Replace("/DNNConnect.CKE/Browser", "/DNNConnect.CKE"),
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        /// <summary>
        /// Gets the maximum size of the upload.
        /// </summary>
        /// <value>
        /// The maximum size of the upload.
        /// </value>
        protected long MaxUploadSize
        {
            get
            {
                return currentSettings.UploadFileSizeLimit > 0
                       && currentSettings.UploadFileSizeLimit <= Utility.GetMaxUploadSize()
                           ? currentSettings.UploadFileSizeLimit
                           : Utility.GetMaxUploadSize();
            }
        }

        /// <summary>
        /// Gets the get folder information identifier.
        /// </summary>
        /// <value>
        /// The get folder information identifier.
        /// </value>
        protected int CurrentFolderId
        {
            get
            {
                if (ViewState["CurrentFolderId"] != null)
                {
                    return Convert.ToInt32(ViewState["CurrentFolderId"]);
                }

                return StartingDir().FolderID;
            }
            set
            {
                ViewState["CurrentFolderId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the files table.
        /// </summary>
        /// <value>
        /// The files table.
        /// </value>
        private DataTable FilesTable
        {
            get
            {
                return ViewState["FilesTable"] as DataTable;
            }

            set
            {
                ViewState["FilesTable"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort files Ascending].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort files Ascending].
        /// </value>
        private bool SortFilesAscending
        {
            get
            {
                return ViewState["SortFilesAscending"] != null && (bool)ViewState["SortFilesAscending"];
            }

            set
            {
                ViewState["SortFilesAscending"] = value;
                FilesTable = null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort files descending].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort files descending].
        /// </value>
        private bool SortFilesDescending
        {
            get
            {
                return ViewState["SortFilesDescending"] != null && (bool)ViewState["SortFilesDescending"];
            }

            set
            {
                ViewState["SortFilesDescending"] = value;
                FilesTable = null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort files by Ascending Date].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort files by ascending date].
        /// </value>
        private bool SortFilesDateAscending
        {
            get
            {
                return ViewState["SortFilesDateAscending"] != null && (bool)ViewState["SortFilesDateAscending"];
            }

            set
            {
                ViewState["SortFilesDateAscending"] = value;
                FilesTable = null;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort files by descending Date].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort files by descending date]; otherwise sort by ascending Date.
        /// </value>
        private bool SortFilesDateDescending
        {
            get
            {
                return ViewState["SortFilesDateDescending"] != null && (bool)ViewState["SortFilesDateDescending"];
            }

            set
            {
                ViewState["SortFilesDateDescending"] = value;
                FilesTable = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the file url from JavaScript to code
        /// </summary>
        /// <param name="fileUrl">
        /// The file url.
        /// </param>
        [WebMethod]
        public static void SetFile(string fileUrl)
        {
            ckFileUrl = fileUrl;
        }

        /// <summary>
        /// Get all Files and Put them in a DataTable for the GridView
        /// </summary>
        /// <param name="currentFolderInfo">The current folder info.</param>
        /// <returns>
        /// The File Table
        /// </returns>
        public DataTable GetFiles(IFolderInfo currentFolderInfo)
        {
            var sizeResx = Localization.GetString("Size.Text", ResXFile, LanguageCode);
            var createdResx = Localization.GetString("Created.Text", ResXFile, LanguageCode);

            var filesTable = new DataTable();

            filesTable.Columns.Add(new DataColumn("FileName", typeof(string)));
            filesTable.Columns.Add(new DataColumn("PictureURL", typeof(string)));
            filesTable.Columns.Add(new DataColumn("Info", typeof(string)));
            filesTable.Columns.Add(new DataColumn("FileId", typeof(int)));

            HttpRequest httpRequest = HttpContext.Current.Request;
            
            var type = "Link";

            if (!string.IsNullOrEmpty(httpRequest.QueryString["Type"]))
            {
                type = httpRequest.QueryString["Type"];
            }
            
            //Get the files
            var files = FolderManager.Instance.GetFiles(currentFolderInfo).ToList();

            if(SortFilesAscending)
            {
                Utility.SortAscending(files, item => item.FileName);
            }

            if (SortFilesDescending)
            {
                Utility.SortDescending(files, item => item.FileName);
            }

            if (SortFilesDateAscending)
            {
                Utility.SortAscending(files, item => item.CreatedOnDate);
            }

            if (SortFilesDateDescending)
            {
                Utility.SortDescending(files, item => item.CreatedOnDate);
            }

            foreach (var fileItem in files)
            {
                var item = fileItem;

                var name = fileItem.FileName;
                var extension = fileItem.Extension;

                if (currentFolderInfo.IsProtected)
                {
                    name = GetFileNameCleaned(name);
                    extension = Path.GetExtension(name);
                }

                switch (type)
                {
                    case "Image":
                        {
                            if (AllowedImageExtensions.Contains(extension))
                            {
                                var dr = filesTable.NewRow();

                                dr["PictureURL"] = FileManager.Instance.GetUrl(fileItem);
                                dr["FileName"] = name;
                                dr["FileId"] = item.FileId;

                                dr["Info"] =
                                    string.Format(fileItemDisplayFormat,
                                            sizeResx,
                                            createdResx,
                                            name,
                                            fileItem.Size,
                                            fileItem.LastModificationTime);

                                filesTable.Rows.Add(dr);
                            }
                        }

                        break;
                    case "Flash":
                        {
                            if (AllowedFlashExtensions.Contains(extension))
                            {
                                var dr = filesTable.NewRow();

                                dr["PictureURL"] = "images/types/swf.png";

                                dr["Info"] =
                                    string.Format(fileItemDisplayFormat,
                                            sizeResx,
                                            createdResx,
                                            name,
                                            fileItem.Size,
                                            fileItem.LastModificationTime);

                                dr["FileName"] = name;
                                dr["FileId"] = item.FileId;

                                filesTable.Rows.Add(dr);
                            }
                        }

                        break;

                    default:
                        {
                            if (extension.StartsWith("."))
                            {
                                extension = extension.Replace(".", string.Empty);
                            }

                            if (extension.Count() <= 1 || !extensionWhiteList.IsAllowedExtension(extension))
                            {
                                continue;
                            }

                            DataRow dr = filesTable.NewRow();
                            
                            var imageExtension = string.Format("images/types/{0}.png", extension);

                            if (File.Exists(MapPath(imageExtension)))
                            {
                                dr["PictureURL"] = imageExtension;
                            }
                            else
                            {
                                dr["PictureURL"] = "images/types/unknown.png";
                            }

                            if (AllowedImageExtensions.Any(sAllowImgExt => name.EndsWith(sAllowImgExt, StringComparison.OrdinalIgnoreCase)))
                            {
                                dr["PictureUrl"] = FileManager.Instance.GetUrl(fileItem);
                            }

                            dr["FileName"] = name;
                            dr["FileId"] = fileItem.FileId;

                            dr["Info"] =
                                string.Format(fileItemDisplayFormat,
                                        sizeResx,
                                        createdResx,
                                        name,
                                        fileItem.Size,
                                        fileItem.LastModificationTime);

                            filesTable.Rows.Add(dr);
                        }

                        break;
                }
            }

            return filesTable;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Register JavaScripts and CSS
        /// </summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            LoadFavIcon();

            var jqueryScriptLink = new HtmlGenericControl("script");

            jqueryScriptLink.Attributes["type"] = "text/javascript";
            jqueryScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js";

            favicon.Controls.Add(jqueryScriptLink);

            var jqueryUiScriptLink = new HtmlGenericControl("script");

            jqueryUiScriptLink.Attributes["type"] = "text/javascript";
            jqueryUiScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js";

            favicon.Controls.Add(jqueryUiScriptLink);

            var jqueryImageSliderScriptLink = new HtmlGenericControl("script");

            jqueryImageSliderScriptLink.Attributes["type"] = "text/javascript";
            jqueryImageSliderScriptLink.Attributes["src"] = ResolveUrl("js/jquery.ImageSlider.js");

            favicon.Controls.Add(jqueryImageSliderScriptLink);

            var jqueryImageResizerScriptLink = new HtmlGenericControl("script");

            jqueryImageResizerScriptLink.Attributes["type"] = "text/javascript";
            jqueryImageResizerScriptLink.Attributes["src"] = ResolveUrl("js/jquery.cropzoom.js");

            favicon.Controls.Add(jqueryImageResizerScriptLink);

            var jqueryCropZoomScriptLink = new HtmlGenericControl("script");

            jqueryCropZoomScriptLink.Attributes["type"] = "text/javascript";
            jqueryCropZoomScriptLink.Attributes["src"] = ResolveUrl("js/jquery.ImageResizer.js");

            favicon.Controls.Add(jqueryCropZoomScriptLink);

            var jqueryPageMetodScriptLink = new HtmlGenericControl("script");

            jqueryPageMetodScriptLink.Attributes["type"] = "text/javascript";
            jqueryPageMetodScriptLink.Attributes["src"] = ResolveUrl("js/jquery.pagemethod.js");

            favicon.Controls.Add(jqueryPageMetodScriptLink);

            var jqueryFileUploadScriptLink = new HtmlGenericControl("script");

            jqueryFileUploadScriptLink.Attributes["type"] = "text/javascript";
            jqueryFileUploadScriptLink.Attributes["src"] = ResolveUrl("js/jquery.fileupload.comb.min.js");

            favicon.Controls.Add(jqueryFileUploadScriptLink);

            var objCssLink = new HtmlGenericSelfClosing("link");

            objCssLink.Attributes["rel"] = "stylesheet";
            objCssLink.Attributes["type"] = "text/css";
            objCssLink.Attributes["href"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css";

            favicon.Controls.Add(objCssLink);

            GetSelectedImageOrLink();

            base.OnPreRender(e);
        }

        /// <summary>
        /// Close Browser Window
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void CmdCloseClick(object sender, EventArgs e)
        {
            if (!panLinkMode.Visible && panPageMode.Visible)
            {
                if (dnntreeTabs.SelectedNode == null)
                {
                    return;
                }

                var tabController = new TabController();

                var selectTab = tabController.GetTab(
                    int.Parse(dnntreeTabs.SelectedValue), _portalSettings.PortalId, true);

                string fileName = null;
                var domainName = string.Format("http://{0}", Globals.GetDomainName(Request, true));

                // Add Language Parameter ?!
                var localeSelected = LanguageRow.Visible && LanguageList.SelectedIndex > 0;

                var friendlyUrl = localeSelected
                                      ? Globals.FriendlyUrl(
                                          selectTab,
                                          string.Format(
                                              "{0}&language={1}",
                                              Globals.ApplicationURL(selectTab.TabID),
                                              LanguageList.SelectedValue),
                                          _portalSettings)
                                      : Globals.FriendlyUrl(
                                          selectTab, Globals.ApplicationURL(selectTab.TabID), _portalSettings);

                var locale = localeSelected
                                 ? string.Format("language/{0}/", LanguageList.SelectedValue)
                                 : string.Empty;

                // Relative or Absolute Url  
                switch (rblLinkType.SelectedValue)
                {
                    case "relLnk":
                        {
                            if (chkHumanFriendy.Checked)
                            {
                                fileName = friendlyUrl;

                                fileName =
                                    Globals.ResolveUrl(
                                        Regex.Replace(fileName, domainName, "~", RegexOptions.IgnoreCase));
                            }
                            else
                            {
                                fileName =
                                    Globals.ResolveUrl(
                                        string.Format("~/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale));
                            }

                            break;
                        }

                    case "absLnk":
                        {
                            if (chkHumanFriendy.Checked)
                            {
                                fileName = friendlyUrl;

                                fileName = Regex.Replace(
                                    fileName, domainName, string.Format("{0}", domainName), RegexOptions.IgnoreCase);
                            }
                            else
                            {
                                fileName = string.Format(
                                    "{2}/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale, domainName);
                            }
                        }

                        break;
                }

                // Add Page Anchor if one is selected
                if (AnchorList.SelectedIndex > 0 && AnchorList.Items.Count > 1)
                {
                    fileName = string.Format("{0}#{1}", fileName, AnchorList.SelectedItem.Text);
                }

                Response.Write("<script type=\"text/javascript\">");
                Response.Write(GetJavaScriptCode(fileName, null, true));
                Response.Write("</script>");

                Response.End();
            }
            else if (panLinkMode.Visible && !panPageMode.Visible)
            {
                if (!string.IsNullOrEmpty(lblFileName.Text) && !string.IsNullOrEmpty(FileId.Text))
                {
                    var fileInfo = FileManager.Instance.GetFile(int.Parse(FileId.Text));
                
                    var filePath = FileManager.Instance.GetUrl(fileInfo);

                    if (rblLinkType.SelectedValue.Equals("absLnk", StringComparison.InvariantCultureIgnoreCase))
                        filePath = BuildAbsoluteUrl(filePath);


                    Response.Write("<script type=\"text/javascript\">");
                    Response.Write(GetJavaScriptCode(string.Empty, filePath, false));
                    Response.Write("</script>");

                    Response.End();
                }
                else
                {
                    Response.Write("<script type=\"text/javascript\">");
                    Response.Write(
                        string.Format(
                            "javascript:alert('{0}');",
                            Localization.GetString("Error5.Text", ResXFile, LanguageCode)));
                    Response.Write("</script>");

                    Response.End();
                }
            }
        }

        /// <summary>
        /// Gets the java script code.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="isPageLink">if set to <c>true</c> [is page link].</param>
        /// <returns>
        /// Returns the java script code
        /// </returns>
        protected virtual string GetJavaScriptCode(string fileName, string fileUrl, bool isPageLink)
        {
            
            if (!string.IsNullOrEmpty(fileUrl) && !string.IsNullOrEmpty(fileName))
            {
                //If we have both, combine them
                fileUrl = !fileUrl.EndsWith("/")
                               ? string.Format("{0}/{1}", fileUrl, fileName)
                               : string.Format("{0}{1}", fileUrl, fileName);
            }
            else if(string.IsNullOrEmpty(fileUrl))
            {
                //If no URL, default to the file name
                fileUrl = fileName;
            }

            if (!fileUrl.Contains("?") && !isPageLink)
            {
                fileUrl = GlobalObject.escape(fileUrl);

                if (fileUrl.Contains("%3A"))
                {
                    fileUrl = fileUrl.Replace("%3A", ":");
                }

                if (fileUrl.Contains(".aspx%23"))
                {
                    fileUrl = fileUrl.Replace("aspx%23", "aspx#");
                }
            }

            HttpRequest httpRequest = HttpContext.Current.Request;

            // string _CKEditorName = httpRequest.QueryString["CKEditor"];
            string funcNum = httpRequest.QueryString["CKEditorFuncNum"];

            string errorMsg = string.Empty;

            funcNum = Regex.Replace(funcNum, @"[^0-9]", string.Empty, RegexOptions.None);

            return
                string.Format(
                    "var E = window.top.opener;E.CKEDITOR.tools.callFunction({0},'{1}','{2}') ;self.close();",
                    funcNum,
                    fileUrl.Replace("'", "\\'"),
                    errorMsg.Replace("'", "\\'"));
        }

        /// <summary>
        /// Gets the java script upload code.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileUrl">The file url.</param>
        /// <returns>
        /// Returns the formatted java script block
        /// </returns>
        protected virtual string GetJsUploadCode(string fileName, string fileUrl)
        {
            fileUrl = string.Format(!fileUrl.EndsWith("/") ? "{0}/{1}" : "{0}{1}", fileUrl, fileName);

            var httpRequest = HttpContext.Current.Request;

            // var _CKEditorName = request.QueryString["CKEditor"];
            var funcNum = httpRequest.QueryString["CKEditorFuncNum"];

            var errorMsg = string.Empty;

            funcNum = Regex.Replace(funcNum, @"[^0-9]", string.Empty, RegexOptions.None);

            return string.Format(
                "var E = window.parent;E['CKEDITOR'].tools.callFunction({0},'{1}','{2}') ;",
                funcNum,
                GlobalObject.escape(fileUrl),
                errorMsg.Replace("'", "\\'"));
        }

        /// <summary>
        /// Handles the Page Changed event of the Pager FileLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerFileLinks_PageChanged(object sender, EventArgs e)
        {
            ShowFilesIn(GetCurrentFolder(), true);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files in ascending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortAscendingClick(object sender, EventArgs e)
        {
            SortFilesAscending = true;
            SortFilesDescending = false;
            SortFilesDateAscending = false;
            SortFilesDateDescending = false;

            SetSortButtonClasses();

            ShowFilesIn(GetCurrentFolder(), true);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files in descending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortDescendingClick(object sender, EventArgs e)
        {
            SortFilesAscending = false;
            SortFilesDescending = true;
            SortFilesDateAscending = false;
            SortFilesDateDescending = false;

            SetSortButtonClasses();

            ShowFilesIn(GetCurrentFolder(), true);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files by Date in ascending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortByDateAscendingClick(object sender, EventArgs e)
        {
            SortFilesAscending = false;
            SortFilesDescending = false;
            SortFilesDateAscending = true;
            SortFilesDateDescending = false;

            SetSortButtonClasses();

            ShowFilesIn(GetCurrentFolder(), true);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files by Date in descending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortByDateDescendingClick(object sender, EventArgs e)
        {
            SortFilesAscending = false;
            SortFilesDescending = false;
            SortFilesDateAscending = false;
            SortFilesDateDescending = true;

            SetSortButtonClasses();

            ShowFilesIn(GetCurrentFolder(), true);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;
        }

        /// <summary>
        /// Sets the sort button classes.
        /// </summary>
        private void SetSortButtonClasses()
        {
            SortAscending.CssClass = !SortFilesAscending ? "ButtonNormal" : "ButtonSelected";
            SortDescending.CssClass = !SortFilesDescending ? "ButtonNormal" : "ButtonSelected";
            SortByDateAscending.CssClass = !SortFilesDateAscending ? "ButtonNormal" : "ButtonSelected";
            SortByDateDescending.CssClass = !SortFilesDateDescending ? "ButtonNormal" : "ButtonSelected";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event to initialize the page.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);

            SetSortButtonClasses();

            extensionWhiteList = Host.AllowedExtensionWhitelist;

            if (!string.IsNullOrEmpty(request.QueryString["mode"]))
            {
                currentSettings.SettingMode =
                    (SettingsMode)Enum.Parse(typeof(SettingsMode), request.QueryString["mode"]);
            }

            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var settingsDictionary = EditorController.GetEditorHostSettings();
            
            var portalRoles = RoleController.Instance.GetRoles(_portalSettings.PortalId);

            switch (currentSettings.SettingMode)
            {
                case SettingsMode.Default:
                    // Load Default Settings
                    currentSettings = SettingsUtil.GetDefaultSettings(
                        _portalSettings,
                        _portalSettings.HomeDirectoryMapPath,
                        objProvider.Attributes["ck_configFolder"],
                        portalRoles);
                    break;
                case SettingsMode.Host:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        _portalSettings,
                        currentSettings,
                        settingsDictionary,
                        "DNNCKH#",
                        portalRoles);
                    break;
                case SettingsMode.Portal:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        _portalSettings,
                        currentSettings,
                        settingsDictionary,
                        $"DNNCKP#{request.QueryString["PortalID"]}#",
                        portalRoles);
                    break;
                case SettingsMode.Page:
                    currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        _portalSettings,
                        currentSettings,
                        settingsDictionary,
                        $"DNNCKT#{request.QueryString["tabid"]}#",
                        portalRoles);
                    break;
                case SettingsMode.ModuleInstance:
                    currentSettings = SettingsUtil.LoadModuleSettings(
                        _portalSettings,
                        currentSettings,
                        $"DNNCKMI#{request.QueryString["mid"]}#INS#{request.QueryString["ckId"]}#",
                        int.Parse(request.QueryString["mid"]),
                        portalRoles);
                    break;
            }

            // set current Upload file size limit
            currentSettings.UploadFileSizeLimit = SettingsUtil.GetCurrentUserUploadSize(
                currentSettings,
                _portalSettings,
                HttpContext.Current.Request);

            if (currentSettings.BrowserMode.Equals(BrowserType.StandardBrowser)
                && HttpContext.Current.Request.IsAuthenticated)
            {
                string command = null;

                try
                {
                    if (request.QueryString["Command"] != null)
                    {
                        command = request.QueryString["Command"];
                    }
                }
                catch (Exception)
                {
                    command = null;
                }

                try
                {
                    if (request.QueryString["Type"] != null)
                    {
                        browserModus = request.QueryString["Type"];
                        var browserModusText = Localization.GetString("lblBrowserModus.Text", ResXFile, LanguageCode);
                        var browserModusTypeKey = string.Format("BrowserModus.{0}.Text", browserModus);
                        var browserModusTypeText = Localization.GetString(browserModusTypeKey, ResXFile, LanguageCode);
                        lblModus.Text = string.Format(browserModusText, browserModusTypeText);

                        if (!IsPostBack)
                        {
                            AcceptFileTypes = GetAcceptedFileTypes();

                            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblModus.Text);

                            AnchorList.Visible = currentSettings.UseAnchorSelector;
                            LabelAnchor.Visible = currentSettings.UseAnchorSelector;

                            ListViewState.Value = currentSettings.FileListViewMode.ToString();

                            // Set default link mode
                            switch (currentSettings.DefaultLinkMode)
                            {
                                case LinkMode.RelativeURL:
                                    rblLinkType.SelectedValue = "relLink";
                                    break;
                                case LinkMode.AbsoluteURL:
                                    rblLinkType.SelectedValue = "absLnk";
                                    break;
                            }

                            switch (browserModus)
                            {
                                case "Link":
                                    BrowserMode.Visible = true;

                                    if (currentSettings.ShowPageLinksTabFirst)
                                    {
                                        BrowserMode.SelectedValue = "page";
                                        panLinkMode.Visible = false;
                                        panPageMode.Visible = true;
                                        
                                        lblModus.Text = string.Format(
                                            "Browser-Modus: {0}",
                                            string.Format("Page {0}", browserModus));
                                        title.InnerText = string.Format(
                                            "{0} - DNNConnect.CKEditorProvider.FileBrowser",
                                            lblModus.Text);

                                        RenderTabs();
                                    }
                                    else
                                    {
                                        BrowserMode.SelectedValue = "file";
                                        panPageMode.Visible = false;
                                    }

                                    break;
                                case "Image":
                                    BrowserMode.Visible = false;
                                    panPageMode.Visible = false;
                                    break;
                                case "Flash":
                                    BrowserMode.Visible = false;
                                    panPageMode.Visible = false;
                                    break;
                                default:
                                    BrowserMode.Visible = false;
                                    panPageMode.Visible = false;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    browserModus = null;
                }

                if (command != null)
                {
                    if (!command.Equals("FileUpload") && !command.Equals("FlashUpload")
                        && !command.Equals("ImageUpload"))
                    {
                        return;
                    }

                    var uploadedFile =
                        HttpContext.Current.Request.Files[HttpContext.Current.Request.Files.AllKeys[0]];

                    if (uploadedFile != null)
                    {
                        UploadFile(uploadedFile, command);
                    }
                }
                else
                {
                    if (!IsPostBack)
                    {
                        OverrideFile.Checked = currentSettings.OverrideFileOnUpload;

                        SetLanguage();

                        GetLanguageList();

                        var startFolder = StartingDir();

                        FillFolderTree(startFolder);

                        bool folderSelected = false;

                        if (!string.IsNullOrEmpty(ckFileUrl))
                        {
                            try
                            {
                                folderSelected = SelectFolderFile(ckFileUrl);
                                ckFileUrl = null;
                            }
                            catch (Exception)
                            {
                                folderSelected = false;
                                ckFileUrl = null;
                            }
                        }

                        if (!folderSelected)
                        {
                            var folderName = !string.IsNullOrEmpty(startFolder.FolderPath) ?
                                startFolder.FolderPath : Localization.GetString("RootFolder.Text", ResXFile, LanguageCode);
                            lblCurrentDir.Text = folderName;

                            ShowFilesIn(startFolder);
                        }
                    }

                    FillQualityPrecentages();
                }
            }
            else
            {
                var errorScript = string.Format(
                    "javascript:alert('{0}');self.close();",
                    Localization.GetString("Error1.Text", ResXFile, LanguageCode));

                Response.Write("<script type=\"text/javascript\">");
                Response.Write(errorScript);
                Response.Write("</script>");

                Response.End();
            }
        }

        /// <summary>
        /// Show Create New Folder Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Create_Click(object sender, EventArgs e)
        {
            panCreate.Visible = true;

            if (panUploadDiv.Visible)
            {
                panUploadDiv.Visible = false;
            }

            if (panThumb.Visible)
            {
                panThumb.Visible = false;
            }
        }

        /// <summary>
        /// Synchronize Current Folder With the Database
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Syncronize_Click(object sender, EventArgs e)
        {
            var currentFolderInfo = GetCurrentFolder();

            FolderManager.Instance.Synchronize(_portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            // Reload Folder
            ShowFilesIn(GetCurrentFolder());
        }

        /// <summary>
        /// Delete Selected File
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Delete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FileId.Text))
            {
                return;
            }

            var deleteFile = FileManager.Instance.GetFile(int.Parse(FileId.Text));

            var thumbFolder = Path.Combine(GetCurrentFolder().PhysicalPath, "_thumbs");

            var thumbPath =
                Path.Combine(thumbFolder, lblFileName.Text).Replace(
                    lblFileName.Text.Substring(lblFileName.Text.LastIndexOf(".", StringComparison.Ordinal)), ".png");

            try
            {
                FileManager.Instance.DeleteFile(deleteFile);

                // Also Delete Thumbnail?);
                if (File.Exists(thumbPath))
                {
                    File.Delete(thumbPath);
                }
            }
            catch (Exception exception)
            {
                Response.Write("<script type=\"text/javascript\">");

                var message =
                    exception.Message.Replace("'", string.Empty).Replace("\r\n", string.Empty).Replace(
                        "\n", string.Empty).Replace("\r", string.Empty);

                Response.Write(string.Format("javascript:alert('{0}');", Context.Server.HtmlEncode(message)));

                Response.Write("</script>");
            }
            finally
            {
                ShowFilesIn(GetCurrentFolder());

                SetDefaultLinkTypeText();

                FileId.Text = null;
                lblFileName.Text = null;
            }
        }

        /// <summary>
        /// Download selected File
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs e.
        /// </param>
        protected void Download_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FileId.Text))
            {
                return;
            }

            var downloadFile = FileManager.Instance.GetFile(int.Parse(FileId.Text));

            FileManager.Instance.WriteFileToResponse(downloadFile, ContentDisposition.Attachment);
        }

        /// <summary>
        /// Opens the Re-sizing Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Resizer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lblFileName.Text))
            {
                return;
            }

            // Hide Link Panel and show Image Editor
            panThumb.Visible = true;
            panImagePreview.Visible = true;
            panImageEdHead.Visible = true;

            imgOriginal.Visible = true;

            cmdRotate.Visible = true;
            cmdCrop.Visible = true;
            cmdZoom.Visible = true;
            cmdResize2.Visible = false;

            panLinkMode.Visible = false;
            BrowserMode.Visible = false;

            lblResizeHeader.Text = Localization.GetString("lblResizeHeader.Text", ResXFile, LanguageCode);
            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblResizeHeader.Text);

            // Hide all Unwanted Elements from the Image Editor
            cmdClose.Visible = false;
            panInfo.Visible = false;

            panImageEditor.Visible = false;
            lblCropInfo.Visible = false;

            var fileInfo = FileManager.Instance.GetFile(GetCurrentFolder(), lblFileName.Text);
            string sFilePath = FileManager.Instance.GetUrl(fileInfo);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(fileInfo.FileName);

            txtThumbName.Text = string.Format("{0}_resized", sFileNameNoExt);

            if (!AllowedImageExtensions.Contains(fileInfo.Extension))
            {
                return;
            }

            var fs = FileManager.Instance.GetFileContent(fileInfo);

            Image image = Image.FromStream(fs);

            StringBuilder sbScript1 = new StringBuilder();

            // Show Preview Images
            imgOriginal.ImageUrl = sFilePath;
            imgResized.ImageUrl = sFilePath;

            int w = image.Width;
            int h = image.Height;

            int longestDimension = (w > h) ? w : h;
            int shortestDimension = (w < h) ? w : h;

            float factor = ((float)longestDimension) / shortestDimension;

            double newWidth = 400;
            double newHeight = 300 / factor;

            if (w < h)
            {
                newWidth = 400 / factor;
                newHeight = 300;
            }

            if (newWidth > image.Width)
            {
                newWidth = image.Width;
            }

            if (newHeight > image.Height)
            {
                newHeight = image.Height;
            }

            int iDefaultWidth, iDefaultHeight;

            if (currentSettings.ResizeWidth > 0)
            {
                iDefaultWidth = currentSettings.ResizeWidth;

                // Check if Default Value is greater the Image Value
                if (iDefaultWidth > image.Width)
                {
                    iDefaultWidth = image.Width;
                }
            }
            else
            {
                iDefaultWidth = (int)newWidth;
            }

            if (currentSettings.ResizeHeight > 0)
            {
                iDefaultHeight = currentSettings.ResizeHeight;

                // Check if Default Value is greater the Image Value
                if (iDefaultHeight > image.Height)
                {
                    iDefaultHeight = image.Height;
                }
            }
            else
            {
                iDefaultHeight = (int)newHeight;
            }

            txtHeight.Text = iDefaultHeight.ToString();
            txtWidth.Text = iDefaultWidth.ToString();

            imgOriginal.Height = (int)newHeight;
            imgOriginal.Width = (int)newWidth;

            imgResized.Height = iDefaultHeight;
            imgResized.Width = iDefaultWidth;

            imgOriginal.ToolTip = Localization.GetString("imgOriginal.Text", ResXFile, LanguageCode);
            imgOriginal.AlternateText = imgOriginal.ToolTip;

            imgResized.ToolTip = Localization.GetString("imgResized.Text", ResXFile, LanguageCode);
            imgResized.AlternateText = imgResized.ToolTip;

            sbScript1.Append("ResizeMe('#imgResized', 360, 300);");

            //////////////
            sbScript1.AppendFormat(
                "SetupSlider('#SliderWidth', 1, {0}, 1, 'horizontal', {1}, '#txtWidth');", image.Width, iDefaultWidth);
            sbScript1.AppendFormat(
                "SetupSlider('#SliderHeight', 1, {0}, 1, 'vertical', {1}, '#txtHeight');", image.Height, iDefaultHeight);

            Page.ClientScript.RegisterStartupScript(GetType(), "SliderScript", sbScript1.ToString(), true);

            image.Dispose();
        }

        /// <summary>
        /// Show Upload Controls
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Upload_Click(object sender, EventArgs e)
        {
            panUploadDiv.Visible = true;

            if (panCreate.Visible)
            {
                panCreate.Visible = false;
            }

            if (panThumb.Visible)
            {
                panThumb.Visible = false;
            }
        }

        /// <summary>
        /// Formats a MapPath into relative MapUrl
        /// </summary>
        /// <param name="sPath">
        /// MapPath Input string
        /// </param>
        /// <returns>
        /// The output URL string
        /// </returns>
        private static string MapUrl(string sPath)
        {
            string sAppPath = HttpContext.Current.Server.MapPath("~");

            string sUrl = string.Format(
                "{0}",
                HttpContext.Current.Request.ApplicationPath + sPath.Replace(sAppPath, string.Empty).Replace("\\", "/"));

            return sUrl;
        }

        /// <summary>
        /// Get File Name without .resources extension
        /// </summary>
        /// <param name="fileName">File Name</param>
        /// <returns>Cleaned File Name</returns>
        private static string GetFileNameCleaned(string fileName)
        {
            return fileName.Replace(".resources", string.Empty);
        }

        /// <summary>
        /// The get encoder.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <returns>
        /// The Encoder
        /// </returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <summary>
        /// Get Folder Icon.
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        /// <returns>
        /// Returns if folder is Secure
        /// </returns>
        private string GetFolderIcon(int folderId)
        {
            var folderInfo = FolderManager.Instance.GetFolder(folderId);
            return GetFolderIcon(folderInfo);
        }

        /// <summary>
        /// Get Folder Icon.
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        private string GetFolderIcon(IFolderInfo folderInfo)
        {
            if (folderInfo == null || string.IsNullOrEmpty(folderInfo.FolderPath))
            {
                return "Images/folder.gif";
            }

            switch (folderInfo.StorageLocation)
            {
                case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                    return "Images/folder.gif";
                case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                    return "Images/folderLocked.gif";
                case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                    return "Images/folderdb.gif";
                default:
                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(folderInfo.PortalID, folderInfo.FolderMappingID);
                    if (folderMapping != null)
                    {
                        return folderMapping.ImageUrl;
                    }

                    return "Images/folder.gif";
            }
        }

        /// <summary>
        /// Hide Create Items if User has no write access to the Current Folder
        /// </summary>
        /// <param name="folderId">The folder id to check</param>
        /// <param name="isFileSelected">if set to <c>true</c> [is file selected].</param>
        private void CheckFolderAccess(int folderId, bool isFileSelected)
        {
            var hasWriteAccess = Utility.CheckIfUserHasFolderWriteAccess(folderId, _portalSettings);

            cmdUpload.Enabled = hasWriteAccess;
            cmdCreate.Enabled = hasWriteAccess;
            Syncronize.Enabled = hasWriteAccess;
            cmdDelete.Enabled = hasWriteAccess && isFileSelected;
            cmdResizer.Enabled = hasWriteAccess && isFileSelected;
            cmdDownload.Enabled = isFileSelected;

            cmdUpload.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            cmdCreate.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            Syncronize.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            cmdDelete.CssClass = hasWriteAccess && isFileSelected ? "LinkNormal" : "LinkDisabled";
            cmdResizer.CssClass = hasWriteAccess && isFileSelected ? "LinkNormal" : "LinkDisabled";
            cmdDownload.CssClass = isFileSelected ? "LinkNormal" : "LinkDisabled";
        }

        /// <summary>
        /// Set Folder Permission
        /// </summary>
        /// <param name="folderId">The Folder Id.</param>
        private void SetFolderPermission(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);

            SetFolderPermission(folder);
        }

        /// <summary>
        /// Set Folder Permission
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        private void SetFolderPermission(IFolderInfo folderInfo)
        {
            FolderManager.Instance.CopyParentFolderPermissions(folderInfo);
        }

        /// <summary>
        /// Set Folder Permission for the Current User
        /// </summary>
        /// <param name="folderInfo">The folder info.</param>
        /// <param name="currentUserInfo">The current user info.</param>
        private void SetUserFolderPermission(IFolderInfo folderInfo, UserInfo currentUserInfo)
        {
            if (FolderPermissionController.CanManageFolder((FolderInfo)folderInfo))
            {
                return;
            }

            foreach (
                var folderPermission in from PermissionInfo permission in PermissionController.GetPermissionsByFolder()
                                        where
                                            permission.PermissionKey.ToUpper() == "READ"
                                            || permission.PermissionKey.ToUpper() == "WRITE"
                                            || permission.PermissionKey.ToUpper() == "BROWSE"
                                        select
                                            new FolderPermissionInfo(permission)
                                            {
                                                FolderID = folderInfo.FolderID,
                                                UserID = currentUserInfo.UserID,
                                                RoleID = Convert.ToInt32(Globals.glbRoleNothing),
                                                AllowAccess = true
                                            })
            {
                folderInfo.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folderInfo);
        }

        /// <summary>
        /// Sets the default link type text.
        /// </summary>
        private void SetDefaultLinkTypeText()
        {
            rblLinkType.Items[0].Text = Localization.GetString("relLnk.Text", ResXFile, LanguageCode);
            rblLinkType.Items[1].Text = Localization.GetString("absLnk.Text", ResXFile, LanguageCode);
        }

        /// <summary>
        /// Fill the Folder TreeView with all (Sub)Directories
        /// </summary>
        /// <param name="currentFolderInfo">The current folder information.</param>
        private void FillFolderTree(IFolderInfo currentFolderInfo)
        {
            FoldersTree.Nodes.Clear();

            var folderName = !string.IsNullOrEmpty(currentFolderInfo.FolderPath) ? 
                currentFolderInfo.FolderPath : Localization.GetString("RootFolder.Text", ResXFile, LanguageCode);

            TreeNode folderNode = new TreeNode
            {
                Text = folderName,
                Value = currentFolderInfo.FolderID.ToString(),
                ImageUrl = GetFolderIcon(currentFolderInfo)
                //ExpandedImageUrl = "Images/folderOpen.gif"
            };

            FoldersTree.Nodes.Add(folderNode);

            // gets the list of folders the specified user has read permissions and transfrom into dictionary:
            //      Key = parrent folder id 
            //      Value = list of child folders
            // this will let us possible to create folders tree much faster on a recursion below
            var readableFolders = FolderManager.Instance.GetFolders(UserController.Instance.GetCurrentUserInfo())
                .GroupBy(folder => folder.ParentID)
                .ToDictionary(key => key.Key, value => value?.Select(folder => folder) ?? Enumerable.Empty<IFolderInfo>());

            // get all folders where parrent folder is current one
            if (!readableFolders.TryGetValue(currentFolderInfo.FolderID, out IEnumerable<IFolderInfo> folders))
            {
                return;
            }

            foreach (TreeNode node in
                folders.Cast<FolderInfo>().Select(folder => RenderFolder(folder, readableFolders)).Where(node => node != null))
            {
                folderNode.ChildNodes.Add(node);
            }
        }

        /// <summary>
        /// Fill Quality Values 1-100 %
        /// </summary>
        private void FillQualityPrecentages()
        {
            for (int i = 00; i < 101; i++)
            {
                dDlQuality.Items.Add(new ListItem { Text = i.ToString(), Value = i.ToString() });
            }

            dDlQuality.Items[100].Selected = true;
        }

        /// <summary>
        /// The get portal settings.
        /// </summary>
        /// <returns>
        /// Current Portal Settings
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            int iTabId = 0, iPortalId = 0;

            PortalSettings portalSettings;

            try
            {
                if (request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(request.QueryString["tabid"]);
                }

                if (request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(request.QueryString["PortalID"]);
                }

                string sDomainName = Globals.GetDomainName(Request, true);

                string sPortalAlias = PortalAliasController.GetPortalAliasByPortal(iPortalId, sDomainName);

                PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(sPortalAlias);

                portalSettings = new PortalSettings(iTabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>
        /// Get the Current Starting Directory
        /// </summary>
        /// <returns>
        /// Returns the Starting Directory.
        /// </returns>
        private IFolderInfo StartingDir()
        {
            IFolderInfo startingFolderInfo = null;

            if (!currentSettings.BrowserRootDirId.Equals(-1))
            {
                // var rootFolder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, this.currentSettings.BrowserRootDirId);
                var rootFolder = FolderManager.Instance.GetFolder(currentSettings.BrowserRootDirId);

                if (rootFolder != null)
                {
                    startingFolderInfo = rootFolder;
                }
            }
            else
            {
                startingFolderInfo = FolderManager.Instance.GetFolder(_portalSettings.PortalId, string.Empty);
            }

            if (Utility.IsInRoles(_portalSettings.AdministratorRoleName, _portalSettings))
            {
                return startingFolderInfo;
            }

            if (currentSettings.SubDirs)
            {
                startingFolderInfo = GetUserFolderInfo(startingFolderInfo.PhysicalPath);
            }
            else
            {
                return startingFolderInfo;
            }

            if (Directory.Exists(startingFolderInfo.PhysicalPath))
            {
                return startingFolderInfo;
            }

            var folderStart = startingFolderInfo.PhysicalPath;

            folderStart =
                folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace(
                    "\\", "/");

            startingFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

            Directory.CreateDirectory(startingFolderInfo.PhysicalPath);

            SetFolderPermission(startingFolderInfo);

            return startingFolderInfo;
        }

        /// <summary>
        /// Gets the user folder Info.
        /// </summary>
        /// <param name="startingDir">The Starting Directory.</param>
        /// <returns>Returns the user folder path</returns>
        private IFolderInfo GetUserFolderInfo(string startingDir)
        {
            IFolderInfo userFolderInfo;

            var userFolderPath = Path.Combine(startingDir, "userfiles");

            // Create "userfiles" folder if not exists
            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                SetFolderPermission(userFolderInfo);
            }

            // Create user folder based on the user id
            userFolderPath = Path.Combine(
                userFolderPath,
                string.Format("{0}\\", UserController.Instance.GetCurrentUserInfo().UserID));

            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(_portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(_portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                SetFolderPermission(userFolderInfo);

                SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }
            else
            {
                userFolderInfo = Utility.ConvertFilePathToFolderInfo(userFolderPath, _portalSettings);

                // make sure the user has the correct permissions set
                SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }

            return userFolderInfo;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _portalSettings = GetPortalSettings();

            cmdCancel.Click += Cancel_Click;
            cmdUploadNow.Click += UploadNow_Click;
            cmdUploadCancel.Click += UploadCancel_Click;
            cmdCreateFolder.Click += CreateFolder_Click;
            cmdCreateCancel.Click += CreateCancel_Click;
            cmdResizeCancel.Click += ResizeCancel_Click;
            cmdResizeNow.Click += ResizeNow_Click;
            cmdRotate.Click += Rotate_Click;
            cmdCrop.Click += Rotate_Click;
            cmdZoom.Click += Rotate_Click;
            cmdResize2.Click += Resizer_Click;
            cmdCropCancel.Click += ResizeCancel_Click;
            cmdCropNow.Click += CropNow_Click;

            BrowserMode.SelectedIndexChanged += BrowserMode_SelectedIndexChanged;
            dnntreeTabs.SelectedNodeChanged += TreeTabs_NodeClick;

            // this.FoldersTree.SelectedNodeChanged += new EventHandler(FoldersTree_SelectedNodeChanged);
            FoldersTree.SelectedNodeChanged += FoldersTree_NodeClick;

            FilesList.ItemCommand += FilesList_ItemCommand;
        }

        /// <summary>
        /// Load Favicon from Current Portal Home Directory
        /// </summary>
        private void LoadFavIcon()
        {
            if (!File.Exists(Path.Combine(_portalSettings.HomeDirectoryMapPath, "favicon.ico")))
            {
                return;
            }

            var faviconUrl = Path.Combine(_portalSettings.HomeDirectory, "favicon.ico");

            var objLink = new HtmlGenericSelfClosing("link");

            objLink.Attributes["rel"] = "shortcut icon";
            objLink.Attributes["href"] = faviconUrl;

            favicon.Controls.Add(objLink);
        }

        /// <summary>
        /// Render all Directories and sub directories recursive
        /// </summary>
        /// <param name="folderInfo">The folder Info.</param>
        /// <param name="readableFolders">The list of folders that the current user has READ access</param>
        /// <returns>
        /// TreeNode List
        /// </returns>
        private TreeNode RenderFolder(FolderInfo folderInfo, IDictionary<int, IEnumerable<IFolderInfo>> readableFolders)
        {
            TreeNode tnFolder = new TreeNode
            {
                Text = folderInfo.FolderName,
                Value = folderInfo.FolderID.ToString(),
                ImageUrl = GetFolderIcon(folderInfo)
            };

            if (!readableFolders.TryGetValue(folderInfo.FolderID, out IEnumerable<IFolderInfo> folders))
            {
                return tnFolder;
            }

            foreach (TreeNode node in
                folders.Cast<FolderInfo>().Select(folder => RenderFolder(folder, readableFolders)).Where(node => node != null))
            {
                tnFolder.ChildNodes.Add(node);
            }

            return tnFolder;
        }
        
        /// <summary>
        /// Gets the language list, and sets the default locale if Content Localization is Enabled
        /// </summary>
        private void GetLanguageList()
        {
            foreach (
                var languageListItem in
                    new LocaleController().GetLocales(_portalSettings.PortalId)
                                          .Values.Select(
                                              language => new ListItem { Text = language.Text, Value = language.Code }))
            {
                LanguageList.Items.Add(languageListItem);
            }

            if (LanguageList.Items.Count.Equals(1))
            {
                LanguageRow.Visible = false;
            }
            else
            {
                // Set default locale and remove no locale if Content Localization is Enabled
                if (!_portalSettings.ContentLocalizationEnabled)
                {
                    return;
                }

                var currentTab = new TabController().GetTab(
                    int.Parse(request.QueryString["tabid"]), _portalSettings.PortalId, false);

                if (currentTab == null || string.IsNullOrEmpty(currentTab.CultureCode))
                {
                    return;
                }

                LanguageList.Items.RemoveAt(0);

                var currentTabCultureItem = LanguageList.Items.FindByValue(currentTab.CultureCode);

                if (currentTabCultureItem != null)
                {
                    currentTabCultureItem.Selected = true;
                }
            }
        }

        /// <summary>
        /// Load the Portal Tabs for the Page Links TreeView Selector
        /// </summary>
        private void RenderTabs()
        {
            if (dnntreeTabs.Nodes.Count > 0)
            {
                return;
            }

            var allPortalTabsList = TabController.GetPortalTabs(this._portalSettings.PortalId, -1, false, null, true, false, true, true, false);
            var allPortalTabs = new HashSet<TabInfo>(allPortalTabsList);

            Func<TabInfo, int> getNodeId = x => x.TabID;
            Func<TabInfo, int> getParentId = x => x.ParentId;
            Func<TabInfo, string> getNodeText = x => x.TabName;
            Func<TabInfo, string> getNodeValue = x => x.TabID.ToString();
            Func<int, bool> getParentIdCheck = x => x != -1;
            Func<TabInfo, string> getNodeImageURL = x =>
                string.IsNullOrWhiteSpace(x.IconFile)
                ? "Images/Page.gif"
                : this.ResolveUrl(x.IconFile);

            TreeViewHelper<int> tvh = new TreeViewHelper<int>();
            tvh.LoadNodes(allPortalTabs, dnntreeTabs.Nodes, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, getParentIdCheck);
        }

        /// <summary>
        /// Scroll to a Selected File or Uploaded File
        /// </summary>
        /// <param name="elementId">
        /// The element Id.
        /// </param>
        private void ScrollToSelectedFile(string elementId)
        {
            StringBuilder sbScript1 = new StringBuilder();

            sbScript1.AppendFormat("document.getElementById('{0}').scrollIntoView();", elementId);

            Page.ClientScript.RegisterStartupScript(
                GetType(), string.Format("ScrollToSelected{0}", Guid.NewGuid()), sbScript1.ToString(), true);
        }

        /// <summary>
        /// Select a folder and the file inside the Browser
        /// </summary>
        /// <param name="fileUrl">
        /// The file url.
        /// </param>
        /// <returns>
        /// if folder was selected
        /// </returns>
        private bool SelectFolderFile(string fileUrl)
        {
            var fileName = fileUrl.Substring(fileUrl.LastIndexOf("/", StringComparison.Ordinal) + 1);

            if (fileName.StartsWith("LinkClick") ||
                fileUrl.StartsWith("http:") ||
                fileUrl.StartsWith("https:") ||
                fileUrl.StartsWith("mailto:"))
            {
                ckFileUrl = null;
                return false;
            }

            var selectedDir = MapPath(fileUrl).Replace(fileName, string.Empty);

            if (!Directory.Exists(selectedDir))
            {
                ckFileUrl = null;
                return false;
            }

            lblCurrentDir.Text = selectedDir;

            var newDir = lblCurrentDir.Text;

            TreeNode tnNewFolder = FoldersTree.FindNode(newDir);

            if (tnNewFolder != null)
            {
                tnNewFolder.Selected = true;
                tnNewFolder.Expand();
                tnNewFolder.Expanded = true;
            }

            ShowFilesIn(newDir);

            GoToSelectedFile(fileName);

            return true;
        }

        /// <summary>
        /// JS Code that gets the selected File Url
        /// </summary>
        private void GetSelectedImageOrLink()
        {
            var scriptSelected = new StringBuilder();

            scriptSelected.Append("var editor = window.top.opener;");
            scriptSelected.Append("if (typeof(CKEDITOR) !== 'undefined') {");
            scriptSelected.AppendFormat(
                "var selection = CKEDITOR.instances.{0}.getSelection(),", request.QueryString["CKEditor"]);
            scriptSelected.Append("element = selection.getStartElement();");

            scriptSelected.Append("if( element.getName()  == 'img')");
            scriptSelected.Append("{");

            scriptSelected.Append("var imageUrl = element.getAttribute('src');");

            scriptSelected.Append("if (element.getAttribute('src') && imageUrl.indexOf('LinkClick') == -1 && imageUrl.indexOf('http:') == -1 && imageUrl.indexOf('https:') == -1) {");
            scriptSelected.Append(
                "jQuery.PageMethod('Browser.aspx', 'SetFile', function(message){if (location.href.indexOf('reload')==-1) location.replace(location.href+'&reload=true');}, null, 'fileUrl', imageUrl);");

            scriptSelected.Append("} else {");
            scriptSelected.Append("if (location.href.indexOf('reload')==-1) location.replace(location.href+'&reload=true');");

            scriptSelected.Append("} }");
            scriptSelected.Append("else if (element.getName() == 'a')");
            scriptSelected.Append("{");

            scriptSelected.Append("var fileUrl = element.getAttribute('href');");

            scriptSelected.Append("if (element.getAttribute('href') && fileUrl.indexOf('LinkClick') == -1 && fileUrl.indexOf('http:') == -1 && fileUrl.indexOf('https:') == -1) {");

            scriptSelected.Append(
                "jQuery.PageMethod('Browser.aspx', 'SetFile', function(message){if (location.href.indexOf('reload')==-1) location.replace(location.href+'&reload=true');}, null, 'fileUrl', fileUrl);");
            scriptSelected.Append("} else {");
            scriptSelected.Append("if (location.href.indexOf('reload')==-1) location.replace(location.href+'&reload=true');");

            scriptSelected.Append("} }");

            scriptSelected.Append("}");

            Page.ClientScript.RegisterStartupScript(
                GetType(), "GetSelectedImageLink", scriptSelected.ToString(), true);
        }

        /// <summary>
        /// Set Language for all Controls on this Page
        /// </summary>
        private void SetLanguage()
        {
            // Buttons
            cmdResizeCancel.Text = Localization.GetString("cmdResizeCancel.Text", ResXFile, LanguageCode);
            cmdResizeNow.Text = Localization.GetString("cmdResizeNow.Text", ResXFile, LanguageCode);
            cmdUploadCancel.Text = Localization.GetString("cmdUploadCancel.Text", ResXFile, LanguageCode);
            cmdCancel.Text = Localization.GetString("cmdCancel.Text", ResXFile, LanguageCode);
            cmdClose.Text = Localization.GetString("cmdClose.Text", ResXFile, LanguageCode);
            cmdCreateFolder.Text = Localization.GetString("cmdCreateFolder.Text", ResXFile, LanguageCode);
            cmdCreateCancel.Text = Localization.GetString("cmdCreateCancel.Text", ResXFile, LanguageCode);
            cmdCrop.Text = Localization.GetString("cmdCrop.Text", ResXFile, LanguageCode);
            cmdZoom.Text = Localization.GetString("cmdZoom.Text", ResXFile, LanguageCode);
            cmdRotate.Text = Localization.GetString("cmdRotate.Text", ResXFile, LanguageCode);
            cmdResize2.Text = Localization.GetString("cmdResize2.Text", ResXFile, LanguageCode);
            cmdCropNow.Text = Localization.GetString("cmdCropNow.Text", ResXFile, LanguageCode);
            cmdCropCancel.Text = Localization.GetString("cmdCropCancel.Text", ResXFile, LanguageCode);

            // Labels
            lblConFiles.Text = Localization.GetString("lblConFiles.Text", ResXFile, LanguageCode);
            lblCurrent.Text = Localization.GetString("lblCurrent.Text", ResXFile, LanguageCode);
            lblSubDirs.Text = Localization.GetString("lblSubDirs.Text", ResXFile, LanguageCode);
            lblUrlType.Text = Localization.GetString("lblUrlType.Text", ResXFile, LanguageCode);
            rblLinkType.ToolTip = Localization.GetString("lblUrlType.Text", ResXFile, LanguageCode);
            lblChoosetab.Text = Localization.GetString("lblChoosetab.Text", ResXFile, LanguageCode);
            lblHeight.Text = Localization.GetString("lblHeight.Text", ResXFile, LanguageCode);
            lblWidth.Text = Localization.GetString("lblWidth.Text", ResXFile, LanguageCode);
            lblThumbName.Text = Localization.GetString("lblThumbName.Text", ResXFile, LanguageCode);
            lblImgQuality.Text = Localization.GetString("lblImgQuality.Text", ResXFile, LanguageCode);
            lblResizeHeader.Text = Localization.GetString("lblResizeHeader.Text", ResXFile, LanguageCode);
            lblOtherTools.Text = Localization.GetString("lblOtherTools.Text", ResXFile, LanguageCode);
            lblCropImageName.Text = Localization.GetString("lblThumbName.Text", ResXFile, LanguageCode);
            lblCropInfo.Text = Localization.GetString("lblCropInfo.Text", ResXFile, LanguageCode);
            lblShowPreview.Text = Localization.GetString("lblShowPreview.Text", ResXFile, LanguageCode);
            lblClearPreview.Text = Localization.GetString("lblClearPreview.Text", ResXFile, LanguageCode);
            lblOriginal.Text = Localization.GetString("lblOriginal.Text", ResXFile, LanguageCode);
            lblPreview.Text = Localization.GetString("lblPreview.Text", ResXFile, LanguageCode);
            lblNewFoldName.Text = Localization.GetString("lblNewFoldName.Text", ResXFile, LanguageCode);
            LabelAnchor.Text = Localization.GetString("LabelAnchor.Text", ResXFile, LanguageCode);
            NewFolderTitle.Text = Localization.GetString("cmdCreate.Text", ResXFile, LanguageCode);
            UploadTitle.Text = Localization.GetString("cmdUpload.Text", ResXFile, LanguageCode);
            AddFiles.Text = Localization.GetString("AddFiles.Text", ResXFile, LanguageCode);
            Wait.Text = Localization.GetString("Wait.Text", ResXFile, LanguageCode);
            WaitMessage.Text = Localization.GetString("WaitMessage.Text", ResXFile, LanguageCode);
            ExtraTabOptions.Text = Localization.GetString("ExtraTabOptions.Text", ResXFile, LanguageCode);
            LabelTabLanguage.Text = Localization.GetString("LabelTabLanguage.Text", ResXFile, LanguageCode);

            MaximumUploadSizeInfo.Text =
                string.Format(
                    Localization.GetString("FileSizeRestriction", ResXFile, LanguageCode),
                    MaxUploadSize / (1024 * 1024),
                    AcceptFileTypes.Replace("|", ","));

            // RadioButtonList
            BrowserMode.Items[0].Text = Localization.GetString("FileLink.Text", ResXFile, LanguageCode);
            BrowserMode.Items[1].Text = Localization.GetString("PageLink.Text", ResXFile, LanguageCode);

            // DropDowns
            LanguageList.Items[0].Text = Localization.GetString("None.Text", ResXFile, LanguageCode);
            AnchorList.Items[0].Text = Localization.GetString("None.Text", ResXFile, LanguageCode);

            // CheckBoxes
            chkAspect.Text = Localization.GetString("chkAspect.Text", ResXFile, LanguageCode);
            chkHumanFriendy.Text = Localization.GetString("chkHumanFriendy.Text", ResXFile, LanguageCode);
            OverrideFile.Text = Localization.GetString("OverrideFile.Text", ResXFile, LanguageCode);

            // LinkButtons (with Image)
            Syncronize.Text = string.Format(
               "<img src=\"Images/SyncFolder.png\" alt=\"{0}\" title=\"{1}\" />",
               Localization.GetString("Syncronize.Text", ResXFile, LanguageCode),
               Localization.GetString("Syncronize.Help", ResXFile, LanguageCode));
            Syncronize.ToolTip = Localization.GetString("Syncronize.Help", ResXFile, LanguageCode);

            cmdCreate.Text = string.Format(
                "<img src=\"Images/CreateFolder.png\" alt=\"{0}\" title=\"{1}\" />",
                Localization.GetString("cmdCreate.Text", ResXFile, LanguageCode),
                Localization.GetString("cmdCreate.Help", ResXFile, LanguageCode));
            cmdCreate.ToolTip = Localization.GetString("cmdCreate.Help", ResXFile, LanguageCode);

            cmdDownload.Text =
                string.Format(
                    "<img src=\"Images/DownloadButton.png\" alt=\"{0}\" title=\"{1}\" />",
                    Localization.GetString("cmdDownload.Text", ResXFile, LanguageCode),
                    Localization.GetString("cmdDownload.Help", ResXFile, LanguageCode));
            cmdDownload.ToolTip = Localization.GetString("cmdDownload.Help", ResXFile, LanguageCode);

            cmdUpload.Text = string.Format(
                "<img src=\"Images/UploadButton.png\" alt=\"{0}\" title=\"{1}\" />",
                Localization.GetString("cmdUpload.Text", ResXFile, LanguageCode),
                Localization.GetString("cmdUpload.Help", ResXFile, LanguageCode));
            cmdUpload.ToolTip = Localization.GetString("cmdUpload.Help", ResXFile, LanguageCode);

            cmdDelete.Text = string.Format(
                "<img src=\"Images/DeleteFile.png\" alt=\"{0}\" title=\"{1}\" />",
                Localization.GetString("cmdDelete.Text", ResXFile, LanguageCode),
                Localization.GetString("cmdDelete.Help", ResXFile, LanguageCode));
            cmdDelete.ToolTip = Localization.GetString("cmdDelete.Help", ResXFile, LanguageCode);

            cmdResizer.Text = string.Format(
                "<img src=\"Images/ResizeImage.png\" alt=\"{0}\" title=\"{1}\" />",
                Localization.GetString("cmdResizer.Text", ResXFile, LanguageCode),
                Localization.GetString("cmdResizer.Help", ResXFile, LanguageCode));
            cmdResizer.ToolTip = Localization.GetString("cmdResizer.Help", ResXFile, LanguageCode);

            const string SwitchContent =
                "<a class=\"Switch{0}\" onclick=\"javascript: SwitchView('{0}');\" href=\"javascript:void(0)\"><img src=\"Images/{0}.png\" alt=\"{1}\" title=\"{2}\" />{1}</a>";

            SwitchDetailView.Text = string.Format(
                SwitchContent,
                "DetailView",
                Localization.GetString("DetailView.Text", ResXFile, LanguageCode),
                Localization.GetString("DetailViewTitle.Text", ResXFile, LanguageCode));
            SwitchDetailView.ToolTip = Localization.GetString("DetailViewTitle.Text", ResXFile, LanguageCode);

            SwitchListView.Text = string.Format(
                SwitchContent,
                "ListView",
                Localization.GetString("ListView.Text", ResXFile, LanguageCode),
                Localization.GetString("ListViewTitle.Text", ResXFile, LanguageCode));
            SwitchListView.ToolTip = Localization.GetString("ListViewTitle.Text", ResXFile, LanguageCode);

            SwitchIconsView.Text = string.Format(
                SwitchContent,
                "IconsView",
                Localization.GetString("IconsView.Text", ResXFile, LanguageCode),
                Localization.GetString("IconsViewTitle.Text", ResXFile, LanguageCode));
            SwitchIconsView.ToolTip = Localization.GetString("IconsViewTitle.Text", ResXFile, LanguageCode);

            SortAscending.Text = string.Format(
                 "<img src=\"Images/SortAscending.png\" alt=\"{0}\" title=\"{1}\" />",
                 Localization.GetString("SortAscending.Text", ResXFile, LanguageCode),
                 Localization.GetString("SortAscending.Help", ResXFile, LanguageCode));
            SortAscending.ToolTip = Localization.GetString("SortAscending.Help", ResXFile, LanguageCode);

            SortDescending.Text = string.Format(
                 "<img src=\"Images/SortDescending.png\" alt=\"{0}\" title=\"{1}\" />",
                 Localization.GetString("SortDescending.Text", ResXFile, LanguageCode),
                 Localization.GetString("SortDescending.Help", ResXFile, LanguageCode));
            SortDescending.ToolTip = Localization.GetString("SortDescending.Help", ResXFile, LanguageCode);

            SortByDateAscending.Text = string.Format(
                 "<img src=\"Images/AscendingArrow.png\" alt=\"{0}\" title=\"{1}\" />{2}",
                 Localization.GetString("SortByDateAscending.Text", ResXFile, LanguageCode),
                 Localization.GetString("SortByDateAscending.Help", ResXFile, LanguageCode),
                 Localization.GetString("SortByDate.Text", ResXFile, LanguageCode));
            SortByDateAscending.ToolTip = Localization.GetString("SortByDateAscending.Help", ResXFile, LanguageCode);

            SortByDateDescending.Text = string.Format(
                 "<img src=\"Images/DescendingArrow.png\" alt=\"{0}\" title=\"{1}\" />{2}",
                 Localization.GetString("SortByDateDescending.Text", ResXFile, LanguageCode),
                 Localization.GetString("SortByDateDescending.Help", ResXFile, LanguageCode),
                 Localization.GetString("SortByDate.Text", ResXFile, LanguageCode));
            SortByDateDescending.ToolTip = Localization.GetString("SortByDateDescending.Help", ResXFile, LanguageCode);

            ClientAPI.AddButtonConfirm(cmdDelete, Localization.GetString("AreYouSure.Text", ResXFile, LanguageCode));

            SetDefaultLinkTypeText();
        }

        /// <summary>
        /// Goes to selected file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void GoToSelectedFile(string fileName)
        {
            // Find the File inside the Repeater
            foreach (RepeaterItem item in FilesList.Items)
            {
                HtmlGenericControl listRow = (HtmlGenericControl)item.FindControl("ListRow");

                switch (item.ItemType)
                {
                    case ListItemType.Item:
                        listRow.Attributes["class"] = "FilesListRow";
                        break;
                    case ListItemType.AlternatingItem:
                        listRow.Attributes["class"] = "FilesListRowAlt";
                        break;
                }

                if (listRow.Attributes["title"] != fileName)
                {
                    continue;
                }

                listRow.Attributes["class"] += " Selected";

                LinkButton fileListItem = (LinkButton)item.FindControl("FileListItem");

                if (fileListItem == null)
                {
                    return;
                }

                int fileId = Convert.ToInt32(fileListItem.CommandArgument);

                var fileInfo = FileManager.Instance.GetFile(fileId);

                ShowFileHelpUrl(fileInfo.FileName, fileInfo);

                ScrollToSelectedFile(fileListItem.ClientID);
            }
        }

        /// <summary>
        /// Show Preview for the URLs
        /// </summary>
        /// <param name="fileName">
        /// Selected FileName
        /// </param>
        /// <param name="fileInfo">
        /// The file Info.
        /// </param>
        private void ShowFileHelpUrl(string fileName, IFileInfo fileInfo)
        {
            try
            {
                SetDefaultLinkTypeText();

                // Enable Buttons
                CheckFolderAccess(fileInfo.FolderId, true);

                var folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);

                rblLinkType.Items[0].Selected = true;

                var isAllowedExtension = AllowedImageExtensions.Contains(Path.GetExtension(fileName).TrimStart('.'));


                cmdResizer.Enabled = cmdResizer.Enabled && isAllowedExtension;
                cmdResizer.CssClass = cmdResizer.Enabled ? "LinkNormal" : "LinkDisabled";

                FileId.Text = fileInfo.FileId.ToString();
                lblFileName.Text = fileName;
                var providerFileUrl = FileManager.Instance.GetUrl(fileInfo);

                // Relative Url  (Or provider default)
                rblLinkType.Items[0].Text = Regex.Replace(rblLinkType.Items[0].Text, "/Images/MyImage.jpg", providerFileUrl, RegexOptions.IgnoreCase);

                // Absolute Url
                rblLinkType.Items[1].Text = Regex.Replace(rblLinkType.Items[1].Text, "http://www.MyWebsite.com/Images/MyImage.jpg", BuildAbsoluteUrl(providerFileUrl), RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                SetDefaultLinkTypeText();
            }
        }

        private string BuildAbsoluteUrl(string fileUrl)
        {
            if (fileUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                return fileUrl;

            return string.Format(
                    "{0}://{1}{2}",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Authority,
                    fileUrl);
        }

        /// <summary>
        /// Shows the files in directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pagerChanged">if set to <c>true</c> [pager changed].</param>
        protected void ShowFilesIn(string directory, bool pagerChanged = false)
        {
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(directory, _portalSettings);

            ShowFilesIn(currentFolderInfo, pagerChanged);
        }

        /// <summary>
        /// Shows the files in directory.
        /// </summary>
        /// <param name="currentFolderInfo">The current folder information.</param>
        /// <param name="pagerChanged">if set to <c>true</c> [pager changed].</param>
        private void ShowFilesIn(IFolderInfo currentFolderInfo, bool pagerChanged = false)
        {
            CheckFolderAccess(currentFolderInfo.FolderID, false);

            if (!pagerChanged)
            {
                FilesTable = GetFiles(currentFolderInfo);

                GetDiskSpaceUsed();
            }
            else
            {
                if (FilesTable == null)
                {
                    FilesTable = GetFiles(currentFolderInfo);
                }
            }

            var filesPagedDataSource = new PagedDataSource { DataSource = FilesTable.DefaultView };

            if (currentSettings.FileListPageSize > 0)
            {
                filesPagedDataSource.AllowPaging = true;
                filesPagedDataSource.PageSize = currentSettings.FileListPageSize;
                filesPagedDataSource.CurrentPageIndex = pagerChanged ? PagerFileLinks.CurrentPageIndex : 0;
            }

            PagerFileLinks.PageCount = filesPagedDataSource.PageCount;
            PagerFileLinks.RessourceFile = ResXFile;
            PagerFileLinks.LanguageCode = LanguageCode;

            PagerFileLinks.Visible = filesPagedDataSource.PageCount > 1;

            // this.FilesList.DataSource = this.GetFiles(directory);
            FilesList.DataSource = filesPagedDataSource;
            FilesList.DataBind();
        }

        /// <summary>
        /// Uploads a File
        /// </summary>
        /// <param name="file">
        /// The Uploaded File
        /// </param>
        /// <param name="command">
        /// The Upload Command Type
        /// </param>
        private void UploadFile(HttpPostedFile file, string command)
        {
            var fileName = Path.GetFileName(file.FileName).Trim();

            if (!string.IsNullOrEmpty(fileName))
            {
                // Replace dots in the name with underscores (only one dot can be there... security issue).
                fileName = Regex.Replace(fileName, @"\.(?![^.]*$)", "_", RegexOptions.None);

                // Check for Illegal Chars
                if (Utility.ValidateFileName(fileName))
                {
                    fileName = Utility.CleanFileName(fileName);
                }

                // Convert Unicode Chars
                fileName = Utility.ConvertUnicodeChars(fileName);
            }
            else
            {
                return;
            }

            // Check if file is to big for that user
            if (currentSettings.UploadFileSizeLimit > 0
                && file.ContentLength > currentSettings.UploadFileSizeLimit)
            {
                Page.ClientScript.RegisterStartupScript(
                    GetType(),
                    "errorcloseScript",
                    string.Format(
                        "javascript:alert('{0}')",
                        Localization.GetString("FileToBigMessage.Text", ResXFile, LanguageCode)),
                    true);

                Response.End();

                return;
            }

            if (fileName.Length > 220)
            {
                fileName = fileName.Substring(fileName.Length - 220);
            }

            string sExtension = Path.GetExtension(file.FileName);
            sExtension = sExtension.TrimStart('.');

            bool bAllowUpl = false;

            switch (command)
            {
                case "FlashUpload":
                    if (AllowedFlashExtensions.Contains(sExtension))
                    {
                        bAllowUpl = true;
                    }

                    break;
                case "ImageUpload":
                    if (AllowedImageExtensions.Contains(sExtension))
                    {
                        bAllowUpl = true;
                    }

                    break;
                case "FileUpload":
                    if (extensionWhiteList.IsAllowedExtension(sExtension))
                    {
                        bAllowUpl = true;
                    }

                    break;
            }

            if (bAllowUpl)
            {
                string sFileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

                int iCounter = 0;

                var uploadPhysicalPath = StartingDir().PhysicalPath;

                var currentFolderInfo = GetCurrentFolder();

                if (!currentSettings.UploadDirId.Equals(-1) && !currentSettings.SubDirs)
                {
                    var uploadFolder = FolderManager.Instance.GetFolder(currentSettings.UploadDirId);

                    if (uploadFolder != null)
                    {
                        uploadPhysicalPath = uploadFolder.PhysicalPath;

                        currentFolderInfo = uploadFolder;
                    }
                }

                string sFilePath = Path.Combine(uploadPhysicalPath, fileName);

                if (File.Exists(sFilePath))
                {
                    iCounter++;
                    fileName = string.Format("{0}_{1}{2}", sFileNameNoExt, iCounter, Path.GetExtension(file.FileName));

                    FileManager.Instance.AddFile(currentFolderInfo, fileName, file.InputStream);
                }
                else
                {
                    FileManager.Instance.AddFile(currentFolderInfo, fileName, file.InputStream);
                }

                Response.Write("<script type=\"text/javascript\">");
                Response.Write(GetJsUploadCode(fileName, MapUrl(uploadPhysicalPath)));
                Response.Write("</script>");
                Response.End();
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(
                    GetType(),
                    "errorcloseScript",
                    string.Format(
                        "javascript:alert('{0}')",
                        Localization.GetString("Error2.Text", ResXFile, LanguageCode)),
                    true);

                Response.End();
            }
        }

        /// <summary>
        /// Exit Dialog
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
                GetType(), "closeScript", "javascript:self.close();", true);
        }

        /// <summary>
        /// Hide Create New Folder Panel
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void CreateCancel_Click(object sender, EventArgs e)
        {
            panCreate.Visible = false;
        }

        /// <summary>
        /// Create a New Sub Folder
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CreateFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbFolderName.Text))
            {
                if (Utility.ValidatePath(tbFolderName.Text))
                {
                    tbFolderName.Text = Utility.CleanPath(tbFolderName.Text);
                }

                tbFolderName.Text = Utility.CleanPath(tbFolderName.Text);
                var newFolderId = Null.NullInteger;

                try
                {
                    var portalId = _portalSettings.PortalId;
                    var currentFolder = GetCurrentFolder();
                    var newFolderPath = string.Format("{0}{1}/", currentFolder.FolderPath, tbFolderName.Text);

                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(currentFolder.FolderMappingID);

                    if (!FolderManager.Instance.FolderExists(portalId, newFolderPath))
                    {
                        var newFolder = FolderManager.Instance.AddFolder(folderMapping, newFolderPath);
                        newFolderId = newFolder.FolderID;
                        SetFolderPermission(newFolder.FolderID);
                        lblCurrentDir.Text = newFolder.FolderPath;
                    }
                }
                catch (Exception exception)
                {
                    Response.Write("<script type=\"text/javascript\">");

                    var message =
                    exception.Message.Replace("'", string.Empty).Replace("\r\n", string.Empty).Replace(
                        "\n", string.Empty).Replace("\r", string.Empty);

                    Response.Write(string.Format("javascript:alert('{0}');", Context.Server.HtmlEncode(message)));

                    Response.Write("</script>");
                }
                finally
                {
                    FillFolderTree(StartingDir());

                    if (newFolderId > Null.NullInteger)
                    {
                        TreeNode tnNewFolder = FindNodeByValue(FoldersTree, newFolderId.ToString());

                        if (tnNewFolder != null)
                        {
                            tnNewFolder.Selected = true;
                            CurrentFolderId = newFolderId;

                            var expandNode = tnNewFolder.Parent;
                            while (expandNode != null)
                            {
                                expandNode.Expand();
                                expandNode = expandNode.Parent;
                            }
                        }
                    }

                    ShowFilesIn(GetCurrentFolder());
                }
            }

            panCreate.Visible = false;
        }

        /// <summary>
        /// Save the New Cropped Image
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void CropNow_Click(object sender, EventArgs e)
        {
            // Hide Image Editor Panels
            panImagePreview.Visible = false;
            panImageEdHead.Visible = false;
            panImageEditor.Visible = false;
            panThumb.Visible = false;

            // Show Link Panel
            panLinkMode.Visible = true;
            cmdClose.Visible = true;
            panInfo.Visible = true;

            if (browserModus.Equals("Link"))
            {
                BrowserMode.Visible = true;
            }

            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblModus.Text);

            // Add new file to database
            var currentFolderInfo = GetCurrentFolder();

            FolderManager.Instance.Synchronize(_portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            ShowFilesIn(GetCurrentFolder());

            string sExtension = Path.GetExtension(lblFileName.Text);

            GoToSelectedFile(string.Format("{0}{1}", txtCropImageName.Text, sExtension));
        }

        /// <summary>
        /// Hide Image Re-sizing Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ResizeCancel_Click(object sender, EventArgs e)
        {
            // Hide Image Editor Panels
            panImagePreview.Visible = false;
            panImageEdHead.Visible = false;
            panImageEditor.Visible = false;
            panThumb.Visible = false;

            // Show Link Panel
            panLinkMode.Visible = true;
            cmdClose.Visible = true;
            panInfo.Visible = true;
            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblModus.Text);

            if (browserModus.Equals("Link"))
            {
                BrowserMode.Visible = true;
            }
        }

        /// <summary>
        /// Resize Image based on User Input
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ResizeNow_Click(object sender, EventArgs e)
        {
            var file = FileManager.Instance.GetFile(GetCurrentFolder(), lblFileName.Text);
            string resizedFileName;

            using (var fileStream = FileManager.Instance.GetFileContent(file))
            {
                var oldImage = Image.FromStream(fileStream);
                int newWidth, newHeight;

                if (!int.TryParse(txtWidth.Text, out newWidth))
                {
                    newWidth = oldImage.Width;
                }

                if (!int.TryParse(txtHeight.Text, out newHeight))
                {
                    newHeight = oldImage.Height;
                }

                if (!string.IsNullOrEmpty(txtThumbName.Text))
                {
                    resizedFileName = $"{txtThumbName.Text}.{file.Extension}";
                }
                else
                {
                    resizedFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_resized.{file.Extension}";
                }

                // Create an Resized Thumbnail
                if (chkAspect.Checked)
                {
                    var finalHeight = Math.Abs(oldImage.Height*newWidth/oldImage.Width);

                    if (finalHeight > newHeight)
                    {
                        // Height resize if necessary
                        newWidth = oldImage.Width*newHeight/oldImage.Height;
                        finalHeight = newHeight;
                    }

                    newHeight = finalHeight;
                }

                var counter = 0;
                while (FileManager.Instance.FileExists(GetCurrentFolder(), resizedFileName))
                {
                    counter++;
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(resizedFileName);
                    resizedFileName = $"{fileNameWithoutExtension}_{counter}.{file.Extension}";
                }

                // Add Compression to Jpeg Images
                if (oldImage.RawFormat.Equals(ImageFormat.Jpeg))
                {
                    ImageCodecInfo jpgEncoder = GetEncoder(oldImage.RawFormat);

                    Encoder myEncoder = Encoder.Quality;
                    EncoderParameters encodeParams = new EncoderParameters(1);
                    EncoderParameter encodeParam = new EncoderParameter(myEncoder, long.Parse(dDlQuality.SelectedValue));
                    encodeParams.Param[0] = encodeParam;

                    using (Bitmap dst = new Bitmap(newWidth, newHeight))
                    {
                        using (Graphics g = Graphics.FromImage(dst))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.DrawImage(oldImage, 0, 0, dst.Width, dst.Height);
                        }

                        using (var stream = new MemoryStream())
                        {
                            dst.Save(stream, jpgEncoder, encodeParams);
                            FileManager.Instance.AddFile(GetCurrentFolder(), resizedFileName, stream);
                        }
                    }
                }
                else
                {
                    // Finally Create a new Resized Image
                    using (Image newImage = oldImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                    {
                        var imageFormat = oldImage.RawFormat;
                        oldImage.Dispose();
                        using (var stream = new MemoryStream())
                        {
                            newImage.Save(stream, imageFormat);
                            FileManager.Instance.AddFile(GetCurrentFolder(), resizedFileName, stream);
                        }
                    }
                }
            }

            // Add new file to database
            var currentFolderInfo = GetCurrentFolder();

            FolderManager.Instance.Synchronize(_portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            // Hide Image Editor Panels
            panImagePreview.Visible = false;
            panImageEdHead.Visible = false;
            panImageEditor.Visible = false;
            panThumb.Visible = false;

            // Show Link Panel
            panLinkMode.Visible = true;
            cmdClose.Visible = true;
            panInfo.Visible = true;
            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblModus.Text);

            if (browserModus.Equals("Link"))
            {
                BrowserMode.Visible = true;
            }

            ShowFilesIn(GetCurrentFolder());

            GoToSelectedFile(resizedFileName);
        }

        /// <summary>
        /// Hide Resize Panel and Show CropZoom Panel
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Args e.
        /// </param>
        private void Rotate_Click(object sender, EventArgs e)
        {
            panThumb.Visible = false;
            panImageEditor.Visible = true;

            imgOriginal.Visible = false;

            lblCropInfo.Visible = true;

            cmdRotate.Visible = false;
            cmdCrop.Visible = false;
            cmdZoom.Visible = false;
            cmdResize2.Visible = true;

            lblResizeHeader.Text = Localization.GetString("lblResizeHeader2.Text", ResXFile, LanguageCode);
            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblResizeHeader.Text);

            var file = FileManager.Instance.GetFile(GetCurrentFolder(), lblFileName.Text);
            string sFilePath = FileManager.Instance.GetUrl(file);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(file.FileName);
            txtCropImageName.Text = string.Format("{0}_Crop", sFileNameNoExt);

            StringBuilder sbCropZoom = new StringBuilder();

            sbCropZoom.Append("jQuery(document).ready(function () {");

            sbCropZoom.Append("jQuery('#imgResized').hide();");

            sbCropZoom.Append("var cropzoom = jQuery('#ImageOriginal').cropzoom({");
            sbCropZoom.Append("width: 400,");
            sbCropZoom.Append("height: 300,");
            sbCropZoom.Append("bgColor: '#CCC',");
            sbCropZoom.Append("enableRotation: true,");
            sbCropZoom.Append("enableZoom: true,");

            sbCropZoom.Append("selector: {");

            sbCropZoom.Append("w:100,");
            sbCropZoom.Append("h:80,");
            sbCropZoom.Append("showPositionsOnDrag: true,");
            sbCropZoom.Append("showDimetionsOnDrag: true,");
            sbCropZoom.Append("bgInfoLayer: '#FFF',");
            sbCropZoom.Append("infoFontSize: 10,");
            sbCropZoom.Append("infoFontColor: 'blue',");
            sbCropZoom.Append("showPositionsOnDrag: true,");
            sbCropZoom.Append("showDimetionsOnDrag: true,");
            sbCropZoom.Append("maxHeight: null,");
            sbCropZoom.Append("maxWidth: null,");
            sbCropZoom.Append("centered: true,");
            sbCropZoom.Append("borderColor: 'blue',");
            sbCropZoom.Append("borderColorHover: '#9eda29'");

            sbCropZoom.Append("},");

            sbCropZoom.Append("image: {");
            sbCropZoom.AppendFormat("source: '{0}',", sFilePath);
            sbCropZoom.AppendFormat("width: {0},", file.Width);
            sbCropZoom.AppendFormat("height: {0},", file.Height);
            sbCropZoom.Append("minZoom: 10,");
            sbCropZoom.Append("maxZoom: 150");
            sbCropZoom.Append("}");
            sbCropZoom.Append("});");

            // Preview Button
            sbCropZoom.Append("jQuery('#PreviewCrop').click(function () {");

            sbCropZoom.Append("jQuery('#lblCropInfo').hide();");
            sbCropZoom.Append(
                "jQuery('#imgResized').attr('src', 'ProcessImage.ashx?fileId=" + file.FileId + "&' + cropzoom.PreviewParams()).show();");

            sbCropZoom.Append("ResizeMe('#imgResized', 360, 300);");

            sbCropZoom.Append("});");

            // Reset Button
            sbCropZoom.Append("jQuery('#ClearCrop').click(function(){");
            sbCropZoom.Append("jQuery('#imgResized').hide();");
            sbCropZoom.Append("jQuery('#lblCropInfo').show();");
            sbCropZoom.Append("cropzoom.restore();");
            sbCropZoom.Append("});");

            // Save Button
            sbCropZoom.Append("jQuery('#CropNow').click(function(e) {");
            sbCropZoom.Append("e.preventDefault();");
            sbCropZoom.Append(
                "cropzoom.send('ProcessImage.ashx', 'POST', { newFileName:  jQuery('#txtCropImageName').val(), saveFile: true, fileId: " + file.FileId + " }, function(){ javascript: __doPostBack('cmdCropNow', ''); });");
            sbCropZoom.Append("});");

            sbCropZoom.Append("});");

            Page.ClientScript.RegisterStartupScript(
                GetType(), string.Format("CropZoomScript{0}", Guid.NewGuid()), sbCropZoom.ToString(), true);
        }

        /// <summary>
        /// Cancel Upload - Hide Upload Controls
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The Event Args e.
        /// </param>
        private void UploadCancel_Click(object sender, EventArgs e)
        {
            panUploadDiv.Visible = false;
        }

        /// <summary>
        /// Upload Selected File
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void UploadNow_Click(object sender, EventArgs e)
        {
            FilesTable = null;
            ShowFilesIn(GetCurrentFolder());

            panUploadDiv.Visible = false;
        }

        /// <summary>
        /// Show Preview of the Page links
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeTabs_NodeClick(object sender, EventArgs eventArgs)
        {
            if (dnntreeTabs.SelectedNode == null)
            {
                return;
            }

            SetDefaultLinkTypeText();

            var tabController = new TabController();

            var selectTab = tabController.GetTab(
                int.Parse(dnntreeTabs.SelectedValue), _portalSettings.PortalId, true);

            string sDomainName = string.Format("http://{0}", Globals.GetDomainName(Request, true));

            // Add Language Parameter ?!
            var localeSelected = LanguageRow.Visible && LanguageList.SelectedIndex > 0;

            if (chkHumanFriendy.Checked)
            {
                var fileName = localeSelected
                                       ? Globals.FriendlyUrl(
                                           selectTab,
                                           string.Format(
                                               "{0}&language={1}",
                                               Globals.ApplicationURL(selectTab.TabID),
                                               LanguageList.SelectedValue),
                                           _portalSettings)
                                       : Globals.FriendlyUrl(
                                           selectTab, Globals.ApplicationURL(selectTab.TabID), _portalSettings);

                // Relative Url
                fileName = Globals.ResolveUrl(Regex.Replace(fileName, sDomainName, "~", RegexOptions.IgnoreCase));

                rblLinkType.Items[0].Text = Regex.Replace(
                    rblLinkType.Items[0].Text,
                    "/Images/MyImage.jpg",
                    Globals.ResolveUrl(Regex.Replace(fileName, sDomainName, "~", RegexOptions.IgnoreCase)),
                    RegexOptions.IgnoreCase);

                // Absolute Url  
                rblLinkType.Items[1].Text = Regex.Replace(
                    rblLinkType.Items[1].Text,
                    "http://www.MyWebsite.com/Images/MyImage.jpg",
                    Regex.Replace(fileName, sDomainName, string.Format("{0}", sDomainName), RegexOptions.IgnoreCase),
                    RegexOptions.IgnoreCase);
            }
            else
            {
                string locale = localeSelected ? string.Format("language/{0}/", LanguageList.SelectedValue) : string.Empty;

                // Relative Url
                rblLinkType.Items[0].Text = Regex.Replace(
                    rblLinkType.Items[0].Text,
                    "/Images/MyImage.jpg",
                    Globals.ResolveUrl(string.Format("~/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale)),
                    RegexOptions.IgnoreCase);

                // Absolute Url  
                rblLinkType.Items[1].Text = Regex.Replace(
                    rblLinkType.Items[1].Text,
                    "http://www.MyWebsite.com/Images/MyImage.jpg",
                    string.Format("{2}/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale, sDomainName),
                    RegexOptions.IgnoreCase);
            }

            if (currentSettings.UseAnchorSelector)
            {
                FindAnchorsOnTab(selectTab);
            }

            Page.ClientScript.RegisterStartupScript(
                GetType(),
                string.Format("hideLoadingScript{0}", Guid.NewGuid()),
                "jQuery('#panelLoading').hide();",
                true);
        }

        /// <summary>
        /// Find and List all Anchors from the Selected Page.
        /// </summary>
        /// <param name="selectedTab">
        /// The selected tab.
        /// </param>
        private void FindAnchorsOnTab(TabInfo selectedTab)
        {
            // Clear Item list first...
            AnchorList.Items.Clear();

            var noneText = Localization.GetString("None.Text", ResXFile, LanguageCode);

            try
            {
                var wc = new WebClient();

                var tabUrl = selectedTab.FullUrl;

                if (tabUrl.StartsWith("/"))
                {
                    tabUrl = string.Format(
                        "{0}://{1}{2}",
                        HttpContext.Current.Request.Url.Scheme,
                        HttpContext.Current.Request.Url.Authority,
                        tabUrl);
                }

                var page = wc.DownloadString(tabUrl);

                foreach (LinkItem i in AnchorFinder.ListAll(page).Where(i => !string.IsNullOrEmpty(i.Anchor)))
                {
                    AnchorList.Items.Add(i.Anchor);
                }

                // Add No Anchor item
                AnchorList.Items.Insert(0, noneText);
            }
            catch (Exception)
            {
                // Add No Anchor item
                AnchorList.Items.Add(noneText);
            }
        }

        /// <summary>
        /// Show Info for Selected File
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        private void FilesList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            foreach (RepeaterItem item in FilesList.Items)
            {
                var listRowItem = (HtmlGenericControl)item.FindControl("ListRow");

                switch (item.ItemType)
                {
                    case ListItemType.Item:
                        listRowItem.Attributes["class"] = "FilesListRow";
                        break;
                    case ListItemType.AlternatingItem:
                        listRowItem.Attributes["class"] = "FilesListRowAlt";
                        break;
                }
            }

            var listRow = (HtmlGenericControl)e.Item.FindControl("ListRow");
            listRow.Attributes["class"] += " Selected";

            var fileListItem = (LinkButton)e.Item.FindControl("FileListItem");

            if (fileListItem == null)
            {
                return;
            }

            int fileId = Convert.ToInt32(fileListItem.CommandArgument);

            var currentFile = FileManager.Instance.GetFile(fileId);

            ShowFileHelpUrl(currentFile.FileName, currentFile);

            ScrollToSelectedFile(fileListItem.ClientID);
        }

        /// <summary>
        /// Switch Browser in Link Modus between Link and Page Mode
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void BrowserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (BrowserMode.SelectedValue)
            {
                case "file":
                    panLinkMode.Visible = true;
                    panPageMode.Visible = false;
                    lblModus.Text = string.Format("Browser-Modus: {0}", browserModus);
                    break;
                case "page":
                    panLinkMode.Visible = false;
                    panPageMode.Visible = true;
                    lblModus.Text = string.Format("Browser-Modus: {0}", string.Format("Page {0}", browserModus));

                    RenderTabs();
                    break;
            }

            title.InnerText = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", lblModus.Text);

            SetDefaultLinkTypeText();
        }


        /// <summary>
        /// Load Files of Selected Folder
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void FoldersTree_NodeClick(object sender, EventArgs eventArgs)
        {
            var folderId = Convert.ToInt32(FoldersTree.SelectedNode.Value);
            var folder = FolderManager.Instance.GetFolder(folderId);

            var folderName = !string.IsNullOrEmpty(folder.FolderPath) ?
                folder.FolderPath : Localization.GetString("RootFolder.Text", ResXFile, LanguageCode);
            lblCurrentDir.Text = folderName;

            CurrentFolderId = folderId;

            ShowFilesIn(folder);

            // Reset selected file
            SetDefaultLinkTypeText();

            FileId.Text = null;
            lblFileName.Text = null;

            // Expand Sub folders (if) exists
            FoldersTree.SelectedNode.Expanded = true;
        }

        /// <summary>
        /// Gets the disk space used.
        /// </summary>
        private void GetDiskSpaceUsed()
        {
            var spaceAvailable = _portalSettings.HostSpace.Equals(0)
                                     ? Localization.GetString("UnlimitedSpace.Text", ResXFile, LanguageCode)
                                     : string.Format("{0}MB", _portalSettings.HostSpace);

            var spaceUsed = new PortalController().GetPortalSpaceUsedBytes(_portalSettings.PortalId);

            string usedSpace;

            string[] suffix = { "B", "KB", "MB", "GB", "TB" };

            var index = 0;

            double spaceUsedDouble = spaceUsed;

            if (spaceUsed > 1024)
            {
                for (index = 0; (spaceUsed / 1024) > 0; index++, spaceUsed /= 1024)
                {
                    spaceUsedDouble = spaceUsed / 1024.0;
                }

                usedSpace = string.Format("{0:0.##}{1}", spaceUsedDouble, suffix[index]);
            }
            else
            {
                usedSpace = string.Format("{0:0.##}{1}", spaceUsedDouble, suffix[index]);
            }

            FileSpaceUsedLabel.Text =
                string.Format(
                    Localization.GetString("SpaceUsed.Text", ResXFile, LanguageCode),
                    usedSpace,
                    spaceAvailable);
        }

        /// <summary>Gets the accepted file types.</summary>
        private string GetAcceptedFileTypes()
        {
            switch (browserModus)
            {
                case "Flash":
                    return string.Join("|", AllowedFlashExtensions);
                case "Image":
                    return string.Join("|", AllowedImageExtensions);
                default:
                    return extensionWhiteList.ToStorageString().Replace(",", "|");
            }
        }

        private IFolderInfo GetCurrentFolder()
        {
            return FolderManager.Instance.GetFolder(CurrentFolderId);
        }

        private TreeNode FindNodeByValue(TreeView tree, string value)
        {
            return FindNodeByValue(tree.Nodes, value);
        }

        private TreeNode FindNodeByValue(TreeNodeCollection nodes, string value)
        {
            TreeNode foundNode = null;
            foreach (TreeNode node in nodes)
            {
                if (node.Value == value)
                {
                    foundNode = node;
                }
                else if (node.ChildNodes.Count > 0)
                {
                    foundNode = FindNodeByValue(node.ChildNodes, value);
                }

                if (foundNode != null)
                {
                    break;
                }
            }

            return foundNode;
        }

        #endregion
    }
}
