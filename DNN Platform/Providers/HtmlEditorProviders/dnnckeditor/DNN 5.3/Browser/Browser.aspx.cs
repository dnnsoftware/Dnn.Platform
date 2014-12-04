/*
 * CKEditor Html Editor Provider for DotNetNuke
 * ========
 * http://dnnckeditor.codeplex.com/
 * Copyright (C) Ingo Herbote
 *
 * The software, this file and its contents are subject to the CKEditor Provider
 * License. Please read the license.txt file before using, installing, copying,
 * modifying or distribute this file or part of its contents. The contents of
 * this file is part of the Source Code of the CKEditor Provider.
 */

namespace WatchersNET.CKEditor.Browser
{
    #region

    using System;
    using System.Collections;
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

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Telerik.Web.UI;

    using WatchersNET.CKEditor.Constants;
    using WatchersNET.CKEditor.Controls;
    using WatchersNET.CKEditor.Objects;
    using WatchersNET.CKEditor.Utilities;

    using Encoder = System.Drawing.Imaging.Encoder;
    using Globals = DotNetNuke.Common.Globals;
    using Image = System.Drawing.Image;

    #endregion

    /// <summary>
    /// The browser.
    /// </summary>
    [ScriptService]
    public partial class Browser : Page 
    {
        #region Constants and Fields

        /// <summary>
        /// The Image or Link that is selected inside the Editor.
        /// </summary>
        private static string ckFileUrl;

        /// <summary>
        ///   The allowed flash ext.
        /// </summary>
        private readonly string[] allowedFlashExt = { "swf", "flv", "mp3" };

        /// <summary>
        ///   The allowed image ext.
        /// </summary>
        private readonly string[] allowedImageExt = { "bmp", "gif", "jpeg", "jpg", "png" };

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
        private string extensionWhiteList;

        /// <summary>
        ///   The browser modus
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
                return this.ViewState["AcceptFileTypes"] != null ? this.ViewState["AcceptFileTypes"].ToString() : ".*";
            }

            set
            {
                this.ViewState["AcceptFileTypes"] = value;
            }
        }

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LanguageCode
        {
            get
            {
                return !string.IsNullOrEmpty(this.request.QueryString["lang"])
                           ? this.request.QueryString["lang"]
                           : "en-US";
            }
        }

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        protected string ResXFile
        {
            get
            {
                string[] page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    this.TemplateSourceDirectory,
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
                return this.currentSettings.UploadFileSizeLimit > 0
                        && this.currentSettings.UploadFileSizeLimit <= Utility.GetMaxUploadSize()
                            ? this.currentSettings.UploadFileSizeLimit
                            : Utility.GetMaxUploadSize();
            }
        }

        /// <summary>
        /// Gets the get folder information identifier.
        /// </summary>
        /// <value>
        /// The get folder information identifier.
        /// </value>
        protected int GetFolderInfoID
        {
            get
            {
                return Utility.ConvertFilePathToFolderInfo(this.lblCurrentDir.Text, this._portalSettings).FolderID;
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
                return this.ViewState["FilesTable"] as DataTable;
            }

            set
            {
                this.ViewState["FilesTable"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [sort files descending].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [sort files descending]; otherwise sort ascending.
        /// </value>
        private bool SortFilesDescending
        {
            get
            {
                return this.ViewState["SortFilesDescending"] != null && (bool)this.ViewState["SortFilesDescending"];
            }

            set
            {
                this.ViewState["SortFilesDescending"] = value;
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
        public DataTable GetFiles(FolderInfo currentFolderInfo)
        {
            DataTable filesTable = new DataTable();

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

            // Get Folder Info Secure?
            var isSecure =
                this.GetStorageLocationType(currentFolderInfo.FolderID).Equals(
                    FolderController.StorageLocationTypes.SecureFileSystem);

            var isDatabaseSecure =
                this.GetStorageLocationType(currentFolderInfo.FolderID).Equals(
                    FolderController.StorageLocationTypes.DatabaseSecure);

            var filesArrayList = FileSystemUtils.GetFilesByFolder(
                this._portalSettings.PortalId, currentFolderInfo.FolderID);

            var files = filesArrayList.OfType<DotNetNuke.Services.FileSystem.FileInfo>().ToList();

            if (this.SortFilesDescending)
            {
                Utility.SortDescending(files, item => item.FileName);
            }

            foreach (DotNetNuke.Services.FileSystem.FileInfo fileItem in files)
            {
                // Check if File Exists
                /*if (!File.Exists(string.Format("{0}{1}", fileItem.PhysicalPath, isSecure ? ".resources" : string.Empty)))
                {
                    continue;
                }*/

                var item = fileItem;

                var name = fileItem.FileName;
                var extension = fileItem.Extension;

                if (isSecure)
                {
                    name = GetFileNameCleaned(name);
                    extension = Path.GetExtension(name);
                }

                switch (type)
                {
                    case "Image":
                        {
                            // Show Images only
                            foreach (DataRow dr in
                                from sAllowExt in this.allowedImageExt
                                where name.ToLower().EndsWith(sAllowExt)
                                select filesTable.NewRow())
                            {
                                if (isSecure || isDatabaseSecure)
                                {
                                    var link = string.Format("fileID={0}", fileItem.FileId);

                                    dr["PictureURL"] = Globals.LinkClick(link, int.Parse(this.request.QueryString["tabid"]), Null.NullInteger);
                                }
                                else
                                {
                                    dr["PictureURL"] = MapUrl(fileItem.PhysicalPath);
                                }

                                dr["FileName"] = name;
                                dr["FileId"] = item.FileId;

                                dr["Info"] =
                                    string.Format(
                                        "<span class=\"FileName\">{0}</span><br /><span class=\"FileInfo\">Size: {1}</span>",
                                        name,
                                        fileItem.Size);

                                filesTable.Rows.Add(dr);
                            }
                        }

                        break;
                    case "Flash":
                        {
                            // Show Flash Files only
                            foreach (DataRow dr in
                                from sAllowExt in this.allowedFlashExt
                                where name.ToLower().EndsWith(sAllowExt)
                                select filesTable.NewRow())
                            {
                                dr["PictureURL"] = "images/types/swf.png";

                                dr["FileName"] = name;
                                dr["FileId"] = item.FileId;

                                dr["Info"] =
                                    string.Format(
                                        "<span class=\"FileName\">{0}</span><br /><span class=\"FileInfo\">Size: {1}</span>",
                                        name,
                                        fileItem.Size);

                                filesTable.Rows.Add(dr);
                            }
                        }

                        break;
                    default:
                        {
                            // Show all allowed File types
                            if (extension.StartsWith("."))
                            {
                                extension = extension.Replace(".", string.Empty);
                            }

                            if (extension.Count() <= 1 || !this.extensionWhiteList.Contains(extension.ToLower()))
                            {
                                continue;
                            }

                            DataRow dr = filesTable.NewRow();

                            var imageExtension = string.Format("images/types/{0}.png", extension);

                            if (File.Exists(this.MapPath(imageExtension)))
                            {
                                dr["PictureURL"] = imageExtension;
                            }
                            else
                            {
                                dr["PictureURL"] = "images/types/unknown.png";
                            }

                            if (this.allowedImageExt.Any(sAllowImgExt => name.ToLower().EndsWith(sAllowImgExt)))
                            {
                                if (isSecure || isDatabaseSecure)
                                {
                                    var link = string.Format("fileID={0}", fileItem.FileId);

                                    dr["PictureURL"] = Globals.LinkClick(link, int.Parse(this.request.QueryString["tabid"]), Null.NullInteger);
                                }
                                else
                                {
                                    dr["PictureURL"] = MapUrl(fileItem.PhysicalPath);
                                }
                            }

                            dr["FileName"] = name;
                            dr["FileId"] = fileItem.FileId;

                            dr["Info"] =
                                string.Format(
                                    "<span class=\"FileName\">{0}</span><br /><span class=\"FileInfo\">Size: {1}</span>",
                                    name,
                                    fileItem.Size);

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
            this.LoadFavIcon();

            var jqueryScriptLink = new HtmlGenericControl("script");

            jqueryScriptLink.Attributes["type"] = "text/javascript";
            jqueryScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js";

            this.favicon.Controls.Add(jqueryScriptLink);

            var jqueryUiScriptLink = new HtmlGenericControl("script");

            jqueryUiScriptLink.Attributes["type"] = "text/javascript";
            jqueryUiScriptLink.Attributes["src"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/jquery-ui.min.js";

            this.favicon.Controls.Add(jqueryUiScriptLink);

            var jqueryImageSliderScriptLink = new HtmlGenericControl("script");

            jqueryImageSliderScriptLink.Attributes["type"] = "text/javascript";
            jqueryImageSliderScriptLink.Attributes["src"] = this.ResolveUrl("js/jquery.ImageSlider.js");

            this.favicon.Controls.Add(jqueryImageSliderScriptLink);

            var jqueryImageResizerScriptLink = new HtmlGenericControl("script");

            jqueryImageResizerScriptLink.Attributes["type"] = "text/javascript";
            jqueryImageResizerScriptLink.Attributes["src"] = this.ResolveUrl("js/jquery.cropzoom.js");

            this.favicon.Controls.Add(jqueryImageResizerScriptLink);

            var jqueryCropZoomScriptLink = new HtmlGenericControl("script");

            jqueryCropZoomScriptLink.Attributes["type"] = "text/javascript";
            jqueryCropZoomScriptLink.Attributes["src"] = this.ResolveUrl("js/jquery.ImageResizer.js");

            this.favicon.Controls.Add(jqueryCropZoomScriptLink);

            var jqueryPageMetodScriptLink = new HtmlGenericControl("script");

            jqueryPageMetodScriptLink.Attributes["type"] = "text/javascript";
            jqueryPageMetodScriptLink.Attributes["src"] = this.ResolveUrl("js/jquery.pagemethod.js");

            this.favicon.Controls.Add(jqueryPageMetodScriptLink);

            var objCssLink = new HtmlGenericSelfClosing("link");

            objCssLink.Attributes["rel"] = "stylesheet";
            objCssLink.Attributes["type"] = "text/css";
            objCssLink.Attributes["href"] = "//ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css";

            this.favicon.Controls.Add(objCssLink);

            this.GetSelectedImageOrLink();

            base.OnPreRender(e);
        }

        /// <summary>
        /// Close Browser Window
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void CmdCloseClick(object sender, EventArgs e)
        {
            if (!this.panLinkMode.Visible && this.panPageMode.Visible)
            {
                if (this.dnntreeTabs.SelectedNode == null)
                {
                    return;
                }

                var tabController = new TabController();

                var selectTab = tabController.GetTab(
                    int.Parse(this.dnntreeTabs.SelectedValue), this._portalSettings.PortalId, true);

                string fileName = null;
                var domainName = string.Format("http://{0}", Globals.GetDomainName(this.Request, true));

                // Add Language Parameter ?!
                var localeSelected = this.LanguageRow.Visible && this.LanguageList.SelectedIndex > 0;

                var friendlyUrl = localeSelected
                                      ? Globals.FriendlyUrl(
                                          selectTab,
                                          string.Format(
                                              "{0}&language={1}",
                                              Globals.ApplicationURL(selectTab.TabID),
                                              this.LanguageList.SelectedValue),
                                          this._portalSettings)
                                      : Globals.FriendlyUrl(
                                          selectTab, Globals.ApplicationURL(selectTab.TabID), this._portalSettings);

                var locale = localeSelected
                                 ? string.Format("language/{0}/", this.LanguageList.SelectedValue)
                                 : string.Empty;

                // Relative or Absolute Url  
                switch (this.rblLinkType.SelectedValue)
                {
                    case "relLnk":
                        {
                            if (this.chkHumanFriendy.Checked)
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
                            if (this.chkHumanFriendy.Checked)
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
                    case "lnkClick":
                        {
                            fileName = Globals.LinkClick(
                                selectTab.TabID.ToString(),
                                this.TrackClicks.Checked
                                    ? int.Parse(this.request.QueryString["tabid"])
                                    : Null.NullInteger,
                                Null.NullInteger);

                            if (fileName.Contains("&language"))
                            {
                                fileName = fileName.Remove(fileName.IndexOf("&language"));
                            }

                            break;
                        }

                    case "lnkAbsClick":
                        {
                            fileName = string.Format(
                                "{0}://{1}{2}",
                                HttpContext.Current.Request.Url.Scheme,
                                HttpContext.Current.Request.Url.Authority,
                                Globals.LinkClick(
                                    selectTab.TabID.ToString(),
                                    this.TrackClicks.Checked
                                        ? int.Parse(this.request.QueryString["tabid"])
                                        : Null.NullInteger,
                                    Null.NullInteger));

                            if (fileName.Contains("&language"))
                            {
                                fileName = fileName.Remove(fileName.IndexOf("&language"));
                            }

                            break;
                        }
                }

                // Add Page Anchor if one is selected
                if (this.AnchorList.SelectedIndex > 0 && this.AnchorList.Items.Count > 1)
                {
                    fileName = string.Format("{0}#{1}", fileName, this.AnchorList.SelectedItem.Text);
                }

                this.Response.Write("<script type=\"text/javascript\">");
                this.Response.Write(this.GetJavaScriptCode(fileName, null, true));
                this.Response.Write("</script>");

                this.Response.End();
            }
            else if (this.panLinkMode.Visible && !this.panPageMode.Visible)
            {
                if (!string.IsNullOrEmpty(this.lblFileName.Text) && !string.IsNullOrEmpty(this.FileId.Text))
                {
                    var fileInfo = new FileController().GetFileById(int.Parse(this.FileId.Text), this._portalSettings.PortalId);

                    var fileName = fileInfo.FileName;

                    var filePath = string.Empty;

                    // Relative or Absolute Url  
                    switch (this.rblLinkType.SelectedValue)
                    {
                        case "relLnk":
                            {
                                filePath = MapUrl(this.lblCurrentDir.Text);
                                break;
                            }

                        case "absLnk":
                            {
                                filePath = string.Format(
                                    "{0}://{1}{2}",
                                    HttpContext.Current.Request.Url.Scheme,
                                    HttpContext.Current.Request.Url.Authority,
                                    MapUrl(this.lblCurrentDir.Text));
                                break;
                            }

                        case "lnkClick":
                            {
                                var link = string.Format("fileID={0}", fileInfo.FileId);

                                fileName = Globals.LinkClick(link, int.Parse(this.request.QueryString["tabid"]), Null.NullInteger, this.TrackClicks.Checked);

                                filePath = string.Empty;

                                break;
                            }

                        case "lnkAbsClick":
                            {
                                var link = string.Format("fileID={0}", fileInfo.FileId);

                                fileName = string.Format(
                                    "{0}://{1}{2}",
                                    HttpContext.Current.Request.Url.Scheme,
                                    HttpContext.Current.Request.Url.Authority,
                                    Globals.LinkClick(link, int.Parse(this.request.QueryString["tabid"]), Null.NullInteger, this.TrackClicks.Checked));

                                filePath = string.Empty;

                                break;
                            }
                    }

                    this.Response.Write("<script type=\"text/javascript\">");
                    this.Response.Write(this.GetJavaScriptCode(fileName, filePath, false));
                    this.Response.Write("</script>");

                    this.Response.End();
                }
                else
                {
                    this.Response.Write("<script type=\"text/javascript\">");
                    this.Response.Write(
                        string.Format(
                            "javascript:alert('{0}');", 
                            Localization.GetString("Error5.Text", this.ResXFile, this.LanguageCode)));
                    this.Response.Write("</script>");

                    this.Response.End();
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
            if (!string.IsNullOrEmpty(fileUrl))
            {
                fileUrl = !fileUrl.EndsWith("/")
                               ? string.Format("{0}/{1}", fileUrl, fileName)
                               : string.Format("{0}{1}", fileUrl, fileName);
            }
            else
            {
                fileUrl = string.Format("{0}{1}", fileUrl, fileName);
            }

            if (!fileUrl.Contains("?") && !isPageLink)
            {
                fileUrl = Microsoft.JScript.GlobalObject.escape(fileUrl);

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
                    fileUrl,
                    errorMsg.Replace("'", "\\'"));
        }

        /// <summary>
        /// Gets the java script upload code.
        /// </summary>
        /// <param name="fileName">
        /// The file name.
        /// </param>
        /// <param name="fileUrl">
        /// The file url.
        /// </param>
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
                Microsoft.JScript.GlobalObject.escape(fileUrl),
                errorMsg.Replace("'", "\\'"));
        }

        /// <summary>
        /// Handles the Page Changed event of the Pager FileLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerFileLinks_PageChanged(object sender, EventArgs e)
        {
            this.ShowFilesIn(this.lblCurrentDir.Text, true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files in ascending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortAscendingClick(object sender, EventArgs e)
        {
            this.SortFilesDescending = false;

            this.SortAscending.CssClass = this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";
            this.SortDescending.CssClass = !this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";

            this.ShowFilesIn(this.lblCurrentDir.Text, true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>
        /// Sorts the Files in descending order
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortDescendingClick(object sender, EventArgs e)
        {
            this.SortFilesDescending = true;

            this.SortAscending.CssClass = this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";
            this.SortDescending.CssClass = !this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";

            this.ShowFilesIn(this.lblCurrentDir.Text, true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event to initialize the page.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.SortAscending.CssClass = this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";
            this.SortDescending.CssClass = !this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";

            this.extensionWhiteList = Host.FileExtensions.ToLower();

            if (!string.IsNullOrEmpty(this.request.QueryString["mode"]))
            {
                this.currentSettings.SettingMode =
                    (SettingsMode)Enum.Parse(typeof(SettingsMode), this.request.QueryString["mode"]);
            }

            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var settingsDictionary = Utility.GetEditorHostSettings();
            var portalRoles = new RoleController().GetPortalRoles(this._portalSettings.PortalId);

            switch (this.currentSettings.SettingMode)
            {
                case SettingsMode.Default:
                    // Load Default Settings
                    this.currentSettings = SettingsUtil.GetDefaultSettings(
                        this._portalSettings,
                        this._portalSettings.HomeDirectoryMapPath,
                        objProvider.Attributes["ck_configFolder"],
                        portalRoles);
                    break;
                case SettingsMode.Portal:
                    this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                        this._portalSettings,
                        this.currentSettings,
                        settingsDictionary,
                        string.Format("DNNCKP#{0}#", this.request.QueryString["PortalID"]),
                        portalRoles);
                    break;
                case SettingsMode.Page:
                    this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                        this._portalSettings,
                        this.currentSettings,
                        settingsDictionary,
                        string.Format("DNNCKT#{0}#", this.request.QueryString["tabid"]),
                        portalRoles);
                    break;
                case SettingsMode.ModuleInstance:
                    this.currentSettings = SettingsUtil.LoadModuleSettings(
                        this._portalSettings,
                        this.currentSettings,
                        string.Format(
                            "DNNCKMI#{0}#INS#{1}#", this.request.QueryString["mid"], this.request.QueryString["ckId"]),
                        int.Parse(this.request.QueryString["mid"]),
                        portalRoles);
                    break;
            }

            // set current Upload file size limit
            this.currentSettings.UploadFileSizeLimit = SettingsUtil.GetCurrentUserUploadSize(
                this.currentSettings,
                this._portalSettings,
                HttpContext.Current.Request);

            if (this.currentSettings.BrowserMode.Equals(Constants.Browser.StandardBrowser) && HttpContext.Current.Request.IsAuthenticated)
            {
                string command = null;

                try
                {
                    if (this.request.QueryString["Command"] != null)
                    {
                        command = this.request.QueryString["Command"];
                    }
                }
                catch (Exception)
                {
                    command = null;
                }

                try
                {
                    if (this.request.QueryString["Type"] != null)
                    {
                        this.browserModus = this.request.QueryString["Type"];
                        this.lblModus.Text = string.Format("Browser-Modus: {0}", this.browserModus);

                        if (!this.IsPostBack)
                        {
                            this.GetAcceptedFileTypes();

                            this.OverrideFile.Checked = this.currentSettings.OverrideFileOnUpload;

                            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblModus.Text);

                            this.AnchorList.Visible = this.currentSettings.UseAnchorSelector;
                            this.LabelAnchor.Visible = this.currentSettings.UseAnchorSelector;

                            this.ListViewState.Value = this.currentSettings.FileListViewMode.ToString();

                            // Set default link mode
                            switch (this.currentSettings.DefaultLinkMode)
                            {
                                case LinkMode.RelativeURL:
                                    this.rblLinkType.SelectedValue = "relLink";
                                    break;
                                case LinkMode.AbsoluteURL:
                                    this.rblLinkType.SelectedValue = "absLnk";
                                    break;
                                case LinkMode.RelativeSecuredURL:
                                    this.rblLinkType.SelectedValue = "lnkClick";
                                    break;
                                case LinkMode.AbsoluteSecuredURL:
                                    this.rblLinkType.SelectedValue = "lnkAbsClick";
                                    break;
                            }

                            switch (this.browserModus)
                            {
                                case "Link":
                                    this.BrowserMode.Visible = true;

                                    if (this.currentSettings.ShowPageLinksTabFirst)
                                    {
                                        this.BrowserMode.SelectedValue = "page";
                                        this.panLinkMode.Visible = false;
                                        this.panPageMode.Visible = true;

                                        this.TrackClicks.Visible = false;
                                        this.lblModus.Text = string.Format(
                                            "Browser-Modus: {0}", string.Format("Page {0}", this.browserModus));
                                        this.title.InnerText = string.Format(
                                            "{0} - WatchersNET.FileBrowser", this.lblModus.Text);

                                        this.RenderTabs();
                                    }
                                    else
                                    {
                                        this.BrowserMode.SelectedValue = "file";
                                        this.panPageMode.Visible = false;
                                    }

                                    break;
                                case "Image":
                                    this.BrowserMode.Visible = false;
                                    this.panPageMode.Visible = false;
                                    break;
                                case "Flash":
                                    this.BrowserMode.Visible = false;
                                    this.panPageMode.Visible = false;
                                    break;
                                default:
                                    this.BrowserMode.Visible = false;
                                    this.panPageMode.Visible = false;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    this.browserModus = null;
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
                        this.UploadFile(uploadedFile, command);
                    }
                }
                else
                {
                    if (this.IsPostBack)
                    {
                        return;
                    }

                    this.SetLanguage();

                    this.GetLanguageList();

                    string sStartingDir = this.StartingDir();

                    if (!Utility.IsInRoles(this._portalSettings.AdministratorRoleName, this._portalSettings))
                    {
                        // Hide physical file Path
                        this.lblCurrentDir.Visible = false;
                        this.lblCurrent.Visible = false;
                    }

                    this.ShowDirectoriesIn(sStartingDir);

                    bool folderSelected = false;

                    if (!string.IsNullOrEmpty(ckFileUrl))
                    {
                        try
                        {
                            folderSelected = this.SelectFolderFile(ckFileUrl);
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
                        this.lblCurrentDir.Text = sStartingDir;

                        this.ShowFilesIn(sStartingDir);
                    }

                    this.FillQualityPrecentages();
                }
            }
            else
            {
                var errorScript = string.Format(
                    "javascript:alert('{0}');self.close();",
                    Localization.GetString("Error1.Text", this.ResXFile, this.LanguageCode));

                this.Response.Write("<script type=\"text/javascript\">");
                this.Response.Write(errorScript);
                this.Response.Write("</script>");

                this.Response.End();
            }
        }

        /// <summary>
        /// Show Create New Folder Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Create_Click(object sender, EventArgs e)
        {
            this.panCreate.Visible = true;

            if (this.panUploadDiv.Visible)
            {
                this.panUploadDiv.Visible = false;
            }

            if (this.panThumb.Visible)
            {
                this.panThumb.Visible = false;
            }
        }

        /// <summary>
        /// Synchronize Current Folder With the Database
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Syncronize_Click(object sender, EventArgs e)
        {
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(this.lblCurrentDir.Text, this._portalSettings);

            FileSystemUtils.SynchronizeFolder(
                this._portalSettings.PortalId,
                currentFolderInfo.PhysicalPath,
                currentFolderInfo.FolderPath,
                false,
                this._portalSettings.HideFoldersEnabled);

            // Reload Folder
            this.ShowFilesIn(this.lblCurrentDir.Text);
        }

        /// <summary>
        /// Delete Selected File
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Delete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.lblFileName.Text))
            {
                return;
            }

            var filePath = Path.Combine(this.lblCurrentDir.Text, this.lblFileName.Text);

            var thumbFolder = Path.Combine(this.lblCurrentDir.Text, "_thumbs");

            var thumbPath =
                Path.Combine(thumbFolder, this.lblFileName.Text).Replace(
                    this.lblFileName.Text.Substring(this.lblFileName.Text.LastIndexOf(".", StringComparison.Ordinal)), ".png");

            var message = string.Empty;

            try
            {
                message = FileSystemUtils.DeleteFile(filePath, this._portalSettings, true);

                // Also Delete Thumbnail?
                if (File.Exists(thumbPath))
                {
                    File.Delete(thumbPath);
                }
            }
            catch (Exception exception)
            {
                this.Response.Write("<script type=\"text/javascript\">");

                var exMessage =
                    exception.Message.Replace("'", string.Empty).Replace("\r\n", string.Empty).Replace(
                        "\n", string.Empty).Replace("\r", string.Empty);

                this.Response.Write(
                     string.Format(
                         "javascript:alert('{0} {1}');",
                         this.Context.Server.HtmlEncode(exMessage),
                         this.Context.Server.HtmlEncode(message)));

                this.Response.Write("</script>");
            }
            finally
            {
                this.ShowFilesIn(this.lblCurrentDir.Text);

                this.SetDefaultLinkTypeText();

                this.FileId.Text = null;
                this.lblFileName.Text = null;
            }
        }

        /// <summary>
        /// Download selected File
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Download_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileId.Text))
            {
                return;
            }

            FileSystemUtils.DownloadFile(this._portalSettings.PortalId, int.Parse(this.FileId.Text), false, true);
        }

        /// <summary>
        /// Opens the Re-sizing Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Resizer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.lblFileName.Text))
            {
                return;
            }

            // Hide Link Panel and show Image Editor
            this.panThumb.Visible = true;
            this.panImagePreview.Visible = true;
            this.panImageEdHead.Visible = true;

            this.imgOriginal.Visible = true;

            this.cmdRotate.Visible = true;
            this.cmdCrop.Visible = true;
            this.cmdZoom.Visible = true;
            this.cmdResize2.Visible = false;

            this.panLinkMode.Visible = false;
            this.BrowserMode.Visible = false;

            this.lblResizeHeader.Text = Localization.GetString("lblResizeHeader.Text", this.ResXFile, this.LanguageCode);
            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblResizeHeader.Text);

            // Hide all Unwanted Elements from the Image Editor
            this.cmdClose.Visible = false;
            this.panInfo.Visible = false;

            this.panImageEditor.Visible = false;
            this.lblCropInfo.Visible = false;

            ////
            string sFilePath = Path.Combine(this.lblCurrentDir.Text, this.lblFileName.Text);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(sFilePath);

            this.txtThumbName.Text = string.Format("{0}_resized", sFileNameNoExt);

            string sExtension = Path.GetExtension(sFilePath);
            sExtension = sExtension.TrimStart('.');

            bool bEnable = this.allowedImageExt.Any(sAllowExt => sAllowExt.Equals(sExtension.ToLower()));

            if (!bEnable)
            {
                return;
            }

            FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            Image image = Image.FromStream(fs);

            StringBuilder sbScript1 = new StringBuilder();

            // Show Preview Images
            this.imgOriginal.ImageUrl = MapUrl(sFilePath);
            this.imgResized.ImageUrl = MapUrl(sFilePath);

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

            if (this.currentSettings.ResizeWidth > 0)
            {
                iDefaultWidth = this.currentSettings.ResizeWidth;

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

            if (this.currentSettings.ResizeHeight > 0)
            {
                iDefaultHeight = this.currentSettings.ResizeHeight;

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

            this.txtHeight.Text = iDefaultHeight.ToString();
            this.txtWidth.Text = iDefaultWidth.ToString();

            this.imgOriginal.Height = (int)newHeight;
            this.imgOriginal.Width = (int)newWidth;

            this.imgResized.Height = iDefaultHeight;
            this.imgResized.Width = iDefaultWidth;

            this.imgOriginal.ToolTip = Localization.GetString("imgOriginal.Text", this.ResXFile, this.LanguageCode);
            this.imgOriginal.AlternateText = this.imgOriginal.ToolTip;

            this.imgResized.ToolTip = Localization.GetString("imgResized.Text", this.ResXFile, this.LanguageCode);
            this.imgResized.AlternateText = this.imgResized.ToolTip;

            sbScript1.Append("ResizeMe('#imgResized', 360, 300);");

            //////////////
            sbScript1.AppendFormat(
                "SetupSlider('#SliderWidth', 1, {0}, 1, 'horizontal', {1}, '#txtWidth');", image.Width, iDefaultWidth);
            sbScript1.AppendFormat(
                "SetupSlider('#SliderHeight', 1, {0}, 1, 'vertical', {1}, '#txtHeight');", image.Height, iDefaultHeight);
            
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "SliderScript", sbScript1.ToString(), true);

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
            this.panUploadDiv.Visible = true;

            if (this.panCreate.Visible)
            {
                this.panCreate.Visible = false;
            }

            if (this.panThumb.Visible)
            {
                this.panThumb.Visible = false;
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
        /// <returns>Returned the Cleaned File Name</returns>
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

        /*
        /// <summary>
        ///  Get an Resized Image
        /// </summary>
        /// <param name="imgPhoto">
        /// Original Image
        /// </param>
        /// <param name="ts">
        /// New Size
        /// </param>
        /// <returns>
        /// The Resized Image
        /// </returns>
        private static Image GetResizedImage(Image imgPhoto, Size ts)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            const int sourceX = 0;
            const int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent;

            bool sourceVertical = sourceWidth < sourceHeight;
            bool targetVeritcal = ts.Width < ts.Height;

            if (sourceVertical != targetVeritcal)
            {
                int t = ts.Width;
                ts.Width = ts.Height;
                ts.Height = t;
            }

            float nPercentW = ts.Width / (float)sourceWidth;
            float nPercentH = ts.Height / (float)sourceHeight;

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = Convert.ToInt16((ts.Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt16((ts.Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(ts.Width, ts.Height, PixelFormat.Format24bppRgb);

            bmPhoto.MakeTransparent(Color.Transparent);

            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);

            // grPhoto.Clear(Color.White);
            grPhoto.Clear(Color.Transparent);

            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(
                imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }*/

        /// <summary>
        /// Check if Folder is a Secure Folder
        /// </summary>
        /// <param name="folderId">The folder id.</param>
        /// <returns>
        /// Returns if folder is Secure
        /// </returns>
        private FolderController.StorageLocationTypes GetStorageLocationType(int folderId)
        {
            FolderController.StorageLocationTypes storagelocationType;

            try
            {
                var folderInfo = new FolderController().GetFolderInfo(this._portalSettings.PortalId, folderId);

                storagelocationType = (FolderController.StorageLocationTypes)folderInfo.StorageLocation;
            }
            catch (Exception)
            {
                storagelocationType = FolderController.StorageLocationTypes.InsecureFileSystem;
            }

            return storagelocationType;
        }

        /// <summary>
        /// Check if Folder is a Secure Folder
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        /// <returns>
        /// Returns if folder is Secure
        /// </returns>
        private FolderController.StorageLocationTypes GetStorageLocationType(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return FolderController.StorageLocationTypes.InsecureFileSystem;
            }

            try
            {
                folderPath = folderPath.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");
            }
            catch (Exception)
            {
                folderPath = folderPath.Replace("\\", "/");
            }

            FolderController.StorageLocationTypes storagelocationType;

            try
            {
                var folderInfo = new FolderController().GetFolder(this._portalSettings.PortalId, folderPath, false);

                storagelocationType = (FolderController.StorageLocationTypes)folderInfo.StorageLocation;
            }
            catch (Exception)
            {
                storagelocationType = FolderController.StorageLocationTypes.InsecureFileSystem;
            }

            return storagelocationType;
        }

        /// <summary>
        /// Hide Create Items if User has no write access to the Current Folder
        /// </summary>
        /// <param name="folderId">The folder id to check</param>
        /// <param name="isFileSelected">if set to <c>true</c> [is file selected].</param>
        private void CheckFolderAccess(int folderId, bool isFileSelected)
        {
            var hasWriteAccess = Utility.CheckIfUserHasFolderWriteAccess(folderId, this._portalSettings);

            this.cmdUpload.Enabled = hasWriteAccess;
            this.cmdCreate.Enabled = hasWriteAccess;
            this.Syncronize.Enabled = hasWriteAccess;
            this.cmdDelete.Enabled = hasWriteAccess && isFileSelected;
            this.cmdResizer.Enabled = hasWriteAccess && isFileSelected;
            this.cmdDownload.Enabled = isFileSelected;

            this.cmdUpload.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            this.cmdCreate.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            this.Syncronize.CssClass = hasWriteAccess ? "LinkNormal" : "LinkDisabled";
            this.cmdDelete.CssClass = hasWriteAccess && isFileSelected ? "LinkNormal" : "LinkDisabled";
            this.cmdResizer.CssClass = hasWriteAccess && isFileSelected ? "LinkNormal" : "LinkDisabled";
            this.cmdDownload.CssClass = isFileSelected ? "LinkNormal" : "LinkDisabled";
        }

        /// <summary>
        /// Set Folder Permission
        /// </summary>
        /// <param name="folderId">The Folder Id.</param>
        private void SetFolderPermission(int folderId)
        {
            var folder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, folderId);

            FileSystemUtils.SetFolderPermissions(this._portalSettings.PortalId, folderId, folder.FolderPath);
        }

        /// <summary>
        /// Set Folder Permission for the Current User
        /// </summary>
        /// <param name="folderId">The Folder Id.</param>
        /// <param name="currentUserInfo">The current user info.</param>
        private void SetUserFolderPermission(int folderId, UserInfo currentUserInfo)
        {
            var folder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, folderId);

            if (FolderPermissionController.CanManageFolder(folder))
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
                                                    FolderID = folder.FolderID,
                                                    UserID = currentUserInfo.UserID,
                                                    RoleID = Null.NullInteger,
                                                    AllowAccess = true
                                                })
            {
                folder.FolderPermissions.Add(folderPermission);
            }

            FolderPermissionController.SaveFolderPermissions(folder);
        }

        /// <summary>
        /// Sets the default link type text.
        /// </summary>
        private void SetDefaultLinkTypeText()
        {
            this.rblLinkType.Items[0].Text = Localization.GetString("relLnk.Text", this.ResXFile, this.LanguageCode);
            this.rblLinkType.Items[1].Text = Localization.GetString("absLnk.Text", this.ResXFile, this.LanguageCode);

            if (this.rblLinkType.Items.Count <= 2)
            {
                return;
            }

            this.rblLinkType.Items[2].Text = Localization.GetString("lnkClick.Text", this.ResXFile, this.LanguageCode);
            this.rblLinkType.Items[3].Text = Localization.GetString(
                "lnkAbsClick.Text", this.ResXFile, this.LanguageCode);
        }

        /// <summary>
        /// Fill the Folder TreeView with all (Sub)Directories
        /// </summary>
        /// <param name="path">
        /// Root Path of the TreeView
        /// </param>
        private void FillFolderTree(string path)
        {
            this.FoldersTree.Nodes.Clear();

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            RadTreeNode folderNode = new RadTreeNode
                {
                    Text = dirInfo.Name, 
                    Value = dirInfo.FullName, 
                    ImageUrl = "Images/folder.gif", 
                    ExpandedImageUrl = "Images/folderOpen.gif"
                };

            switch (this.GetStorageLocationType(path))
            {
                case FolderController.StorageLocationTypes.SecureFileSystem:
                    {
                        folderNode.ImageUrl = "Images/folderLocked.gif";
                        folderNode.ExpandedImageUrl = "Images/folderOpenLocked.gif";
                    }

                    break;
                case FolderController.StorageLocationTypes.DatabaseSecure:
                    {
                        folderNode.ImageUrl = "Images/folderdb.gif";
                        folderNode.ExpandedImageUrl = "Images/folderdb.gif";
                    }

                    break;
            }

            this.FoldersTree.Nodes.Add(folderNode);

            string sFolder = path;

            sFolder = sFolder.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

            var folders = FileSystemUtils.GetFoldersByParentFolder(this._portalSettings.PortalId, sFolder);

            foreach (RadTreeNode node in folders.Cast<FolderInfo>().Select(this.RenderFolder).Where(node => node != null))
            {
                switch (this.GetStorageLocationType(Convert.ToInt32(node.ToolTip)))
                {
                    case FolderController.StorageLocationTypes.SecureFileSystem:
                        {
                            node.ImageUrl = "Images/folderLocked.gif";
                            node.ExpandedImageUrl = "Images/folderOpenLocked.gif";
                        }

                        break;
                    case FolderController.StorageLocationTypes.DatabaseSecure:
                        {
                            node.ImageUrl = "Images/folderdb.gif";
                            node.ExpandedImageUrl = "Images/folderdb.gif";
                        }

                        break;
                }

                folderNode.Nodes.Add(node);
            }
        }

        /// <summary>
        /// Fill Quality Values 1-100 %
        /// </summary>
        private void FillQualityPrecentages()
        {
            for (int i = 00; i < 101; i++)
            {
                this.dDlQuality.Items.Add(new ListItem { Text = i.ToString(), Value = i.ToString() });
            }

            this.dDlQuality.Items[100].Selected = true;
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
                if (this.request.QueryString["tabid"] != null)
                {
                    iTabId = int.Parse(this.request.QueryString["tabid"]);
                }

                if (this.request.QueryString["PortalID"] != null)
                {
                    iPortalId = int.Parse(this.request.QueryString["PortalID"]);
                }

                string sDomainName = Globals.GetDomainName(this.Request, true);

                string sPortalAlias = PortalAliasController.GetPortalAliasByPortal(iPortalId, sDomainName);

                PortalAliasInfo objPortalAliasInfo = PortalAliasController.GetPortalAliasInfo(sPortalAlias);

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
        private string StartingDir()
        {
            var startingDir = this._portalSettings.HomeDirectoryMapPath;

            if (!this.currentSettings.BrowserRootDirId.Equals(-1))
            {
                var rootFolder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, this.currentSettings.BrowserRootDirId);

                if (rootFolder != null)
                {
                    startingDir = rootFolder.PhysicalPath;
                }
            }

            if (Utility.IsInRoles(this._portalSettings.AdministratorRoleName, this._portalSettings))
            {
                return startingDir;
            }

            if (this.currentSettings.SubDirs)
            {
                startingDir = this.GetUserFolder(startingDir);
            }
            else
            {
                return startingDir;
            }

            if (Directory.Exists(startingDir))
            {
                return startingDir;
            }

            string sFolderStart = startingDir;

            sFolderStart =
                sFolderStart.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace(
                    "\\", "/");

            var folderId = new FolderController().AddFolder(this._portalSettings.PortalId, sFolderStart);

            Directory.CreateDirectory(startingDir);

            this.SetFolderPermission(folderId);

            return startingDir;
        }

        /// <summary>
        /// Gets the user folder ("userfiles\[USERID]").
        /// </summary>
        /// <param name="startingDir">The Starting Directory.</param>
        /// <returns>Returns the user folder path</returns>
        private string GetUserFolder(string startingDir)
        {
            var userFolderPath = Path.Combine(startingDir, "userfiles");

            // Create "userfiles" folder if not exists
            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart =
                    folderStart.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace(
                        "\\", "/");

                var folderId = new FolderController().AddFolder(this._portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                this.SetFolderPermission(folderId);
            }

            // Create user folder based on the user id
            userFolderPath = Path.Combine(
                userFolderPath,
                string.Format("{0}\\", UserController.GetCurrentUserInfo().UserID));

            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart =
                    folderStart.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace(
                        "\\", "/");

                var folderId = new FolderController().AddFolder(this._portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                this.SetFolderPermission(folderId);

                this.SetUserFolderPermission(folderId, UserController.GetCurrentUserInfo());
            }
            else
            {
                var userFolderInfo = Utility.ConvertFilePathToFolderInfo(userFolderPath, this._portalSettings);

                // make sure the user has the correct permissions set
                this.SetUserFolderPermission(userFolderInfo.FolderID, UserController.GetCurrentUserInfo());
            }

            return userFolderPath;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._portalSettings = this.GetPortalSettings();

            this.cmdCancel.Click += this.Cancel_Click;
            this.cmdUploadNow.Click += this.UploadNow_Click;
            this.cmdUploadCancel.Click += this.UploadCancel_Click;
            this.cmdCreateFolder.Click += this.CreateFolder_Click;
            this.cmdCreateCancel.Click += this.CreateCancel_Click;
            this.cmdResizeCancel.Click += this.ResizeCancel_Click;
            this.cmdResizeNow.Click += this.ResizeNow_Click;
            this.cmdRotate.Click += this.Rotate_Click;
            this.cmdCrop.Click += this.Rotate_Click;
            this.cmdZoom.Click += this.Rotate_Click;
            this.cmdResize2.Click += this.Resizer_Click;
            this.cmdCropCancel.Click += this.ResizeCancel_Click;
            this.cmdCropNow.Click += this.CropNow_Click;

            this.BrowserMode.SelectedIndexChanged += this.BrowserMode_SelectedIndexChanged;
            this.dnntreeTabs.NodeClick += this.TreeTabs_NodeClick;
            this.rblLinkType.SelectedIndexChanged += this.LinkType_SelectedIndexChanged;

            // this.FoldersTree.SelectedNodeChanged += new EventHandler(FoldersTree_SelectedNodeChanged);
            this.FoldersTree.NodeClick += this.FoldersTree_NodeClick;
            
            this.FilesList.ItemCommand += this.FilesList_ItemCommand;
        }

        /// <summary>
        /// Load Favicon from Current Portal Home Directory
        /// </summary>
        private void LoadFavIcon()
        {
            if (!File.Exists(Path.Combine(this._portalSettings.HomeDirectoryMapPath, "favicon.ico")))
            {
                return;
            }

            var faviconUrl = Path.Combine(this._portalSettings.HomeDirectory, "favicon.ico");

            var objLink = new HtmlGenericSelfClosing("link");

            objLink.Attributes["rel"] = "shortcut icon";
            objLink.Attributes["href"] = faviconUrl;

            this.favicon.Controls.Add(objLink);
        }

        /// <summary>
        /// Render all Directories and sub directories recursive
        /// </summary>
        /// <param name="folderInfo">The folder Info.</param>
        /// <returns>
        /// TreeNode List
        /// </returns>
        private RadTreeNode RenderFolder(FolderInfo folderInfo)
        {
            if (!FolderPermissionController.CanViewFolder(folderInfo))
            {
                return null;
            }

            RadTreeNode tnFolder = new RadTreeNode
                {
                    Text = folderInfo.FolderName, 
                    Value = folderInfo.PhysicalPath, 
                    ImageUrl = "Images/folder.gif",
                    ExpandedImageUrl = "Images/folderOpen.gif",
                    ToolTip = folderInfo.FolderID.ToString()
                };

            if (folderInfo.StorageLocation.Equals((int)FolderController.StorageLocationTypes.SecureFileSystem))
            {
                tnFolder.ImageUrl = "Images/folderLocked.gif";
                tnFolder.ExpandedImageUrl = "Images/folderOpenLocked.gif";
            }
            else if (folderInfo.StorageLocation.Equals((int)FolderController.StorageLocationTypes.DatabaseSecure))
            {
                tnFolder.ImageUrl = "Images/folderdb.gif";
                tnFolder.ExpandedImageUrl = "Images/folderdb.gif";
            }

            ArrayList folders = FileSystemUtils.GetFoldersByParentFolder(
                this._portalSettings.PortalId, folderInfo.FolderPath);

            if (folders.Count <= 0)
            {
                return tnFolder;
            }

            foreach (RadTreeNode node in
                folders.Cast<FolderInfo>().Select(this.RenderFolder).Where(node => node != null))
            {
                switch (this.GetStorageLocationType(Convert.ToInt32(node.ToolTip)))
                {
                    case FolderController.StorageLocationTypes.SecureFileSystem:
                        {
                            node.ImageUrl = "Images/folderLocked.gif";
                            node.ExpandedImageUrl = "Images/folderOpenLocked.gif";
                        }

                        break;
                    case FolderController.StorageLocationTypes.DatabaseSecure:
                        {
                            node.ImageUrl = "Images/folderdb.gif";
                            node.ExpandedImageUrl = "Images/folderdb.gif";
                        }

                        break;
                }

                tnFolder.Nodes.Add(node);
            }

            return tnFolder;
        }

        /// <summary>
        /// Render all Tabs including Child Tabs
        /// </summary>
        /// <param name="nodeParent">
        /// Parent Node(Tab)
        /// </param>
        /// <param name="iParentTabId">
        /// Parent Tab ID
        /// </param>
        private void RenderTabLevels(IRadTreeNodeContainer nodeParent, int iParentTabId)
        {
            foreach (TabInfo objTab in
                TabController.GetPortalTabs(
                    this._portalSettings.PortalId, -1, false, null, true, false, true, true, false))
            {
                if (!objTab.ParentId.Equals(iParentTabId))
                {
                    continue;
                }

                var nodeTab = new RadTreeNode();

                if (nodeParent != null)
                {
                    nodeParent.Nodes.Add(nodeTab);
                }
                else
                {
                    this.dnntreeTabs.Nodes.Add(nodeTab);
                }

                nodeTab.Text = objTab.TabName;
                nodeTab.Value = objTab.TabID.ToString();
                nodeTab.ImageUrl = "Images/Page.gif";

                // nodeTab.ExpandedImageUrl = "Images/folderOpen.gif";
                if (!string.IsNullOrEmpty(objTab.IconFile))
                {
                    nodeTab.ImageUrl = this.ResolveUrl(objTab.IconFile);
                }

                this.RenderTabLevels(nodeTab, objTab.TabID);
            }
        }

        /// <summary>
        /// Gets the language list.
        /// </summary>
        private void GetLanguageList()
        {
            foreach (var languageListItem in Localization.GetLocales(this._portalSettings.PortalId).Values.Select(language => new ListItem { Text = language.Text, Value = language.Code }))
            {
                this.LanguageList.Items.Add(languageListItem);
            }

            if (this.LanguageList.Items.Count.Equals(1))
            {
                this.LanguageRow.Visible = false;
            }
        }

        /// <summary>
        /// Load the Portal Tabs for the Page Links TreeView Selector
        /// </summary>
        private void RenderTabs()
        {
            if (this.dnntreeTabs.Nodes.Count > 0)
            {
                return;
            }

            this.RenderTabLevels(null, -1);
        }

        /// <summary>
        /// Goes to selected file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        private void GoToSelectedFile(string fileName)
        {
            // Find the File inside the Repeater
            foreach (RepeaterItem item in this.FilesList.Items)
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

                var fileInfo = new FileController().GetFileById(fileId, this._portalSettings.PortalId);

                this.ShowFileHelpUrl(fileInfo.FileName, fileInfo);

                this.ScrollToSelectedFile(fileListItem.ClientID);
            }
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

            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), string.Format("ScrollToSelected{0}", Guid.NewGuid()), sbScript1.ToString(), true);
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

            this.lblCurrentDir.Text = selectedDir;

            var newDir = this.lblCurrentDir.Text;

            RadTreeNode tnNewFolder = this.FoldersTree.FindNodeByValue(newDir);

            if (tnNewFolder != null)
            {
                tnNewFolder.Selected = true;
                tnNewFolder.ExpandParentNodes();
                tnNewFolder.Expanded = true;
            }

            this.ShowFilesIn(newDir);

            this.GoToSelectedFile(fileName);

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
                "var selection = CKEDITOR.instances.{0}.getSelection(),", this.request.QueryString["CKEditor"]);
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

            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "GetSelectedImageLink", scriptSelected.ToString(), true);
        }

        /// <summary>
        /// Set Language for all Controls on this Page
        /// </summary>
        private void SetLanguage()
        {
            // Buttons
            this.cmdResizeCancel.Text = Localization.GetString("cmdResizeCancel.Text", this.ResXFile, this.LanguageCode);
            this.cmdResizeNow.Text = Localization.GetString("cmdResizeNow.Text", this.ResXFile, this.LanguageCode);
            this.cmdUploadCancel.Text = Localization.GetString("cmdUploadCancel.Text", this.ResXFile, this.LanguageCode);
            this.cmdCancel.Text = Localization.GetString("cmdCancel.Text", this.ResXFile, this.LanguageCode);
            this.cmdClose.Text = Localization.GetString("cmdClose.Text", this.ResXFile, this.LanguageCode);
            this.cmdCreateFolder.Text = Localization.GetString("cmdCreateFolder.Text", this.ResXFile, this.LanguageCode);
            this.cmdCreateCancel.Text = Localization.GetString("cmdCreateCancel.Text", this.ResXFile, this.LanguageCode);
            this.cmdCrop.Text = Localization.GetString("cmdCrop.Text", this.ResXFile, this.LanguageCode);
            this.cmdZoom.Text = Localization.GetString("cmdZoom.Text", this.ResXFile, this.LanguageCode);
            this.cmdRotate.Text = Localization.GetString("cmdRotate.Text", this.ResXFile, this.LanguageCode);
            this.cmdResize2.Text = Localization.GetString("cmdResize2.Text", this.ResXFile, this.LanguageCode);
            this.cmdCropNow.Text = Localization.GetString("cmdCropNow.Text", this.ResXFile, this.LanguageCode);
            this.cmdCropCancel.Text = Localization.GetString("cmdCropCancel.Text", this.ResXFile, this.LanguageCode);

            // Labels
            this.lblConFiles.Text = Localization.GetString("lblConFiles.Text", this.ResXFile, this.LanguageCode);
            this.lblCurrent.Text = Localization.GetString("lblCurrent.Text", this.ResXFile, this.LanguageCode);
            this.lblSubDirs.Text = Localization.GetString("lblSubDirs.Text", this.ResXFile, this.LanguageCode);
            this.lblUrlType.Text = Localization.GetString("lblUrlType.Text", this.ResXFile, this.LanguageCode);
            this.rblLinkType.ToolTip = Localization.GetString("lblUrlType.Text", this.ResXFile, this.LanguageCode);
            this.lblChoosetab.Text = Localization.GetString("lblChoosetab.Text", this.ResXFile, this.LanguageCode);
            this.lblHeight.Text = Localization.GetString("lblHeight.Text", this.ResXFile, this.LanguageCode);
            this.lblWidth.Text = Localization.GetString("lblWidth.Text", this.ResXFile, this.LanguageCode);
            this.lblThumbName.Text = Localization.GetString("lblThumbName.Text", this.ResXFile, this.LanguageCode);
            this.lblImgQuality.Text = Localization.GetString("lblImgQuality.Text", this.ResXFile, this.LanguageCode);
            this.lblResizeHeader.Text = Localization.GetString("lblResizeHeader.Text", this.ResXFile, this.LanguageCode);
            this.lblOtherTools.Text = Localization.GetString("lblOtherTools.Text", this.ResXFile, this.LanguageCode);
            this.lblCropImageName.Text = Localization.GetString("lblThumbName.Text", this.ResXFile, this.LanguageCode);
            this.lblCropInfo.Text = Localization.GetString("lblCropInfo.Text", this.ResXFile, this.LanguageCode);
            this.lblShowPreview.Text = Localization.GetString("lblShowPreview.Text", this.ResXFile, this.LanguageCode);
            this.lblClearPreview.Text = Localization.GetString("lblClearPreview.Text", this.ResXFile, this.LanguageCode);
            this.lblOriginal.Text = Localization.GetString("lblOriginal.Text", this.ResXFile, this.LanguageCode);
            this.lblPreview.Text = Localization.GetString("lblPreview.Text", this.ResXFile, this.LanguageCode);
            this.lblNewFoldName.Text = Localization.GetString("lblNewFoldName.Text", this.ResXFile, this.LanguageCode);
            this.LabelAnchor.Text = Localization.GetString("LabelAnchor.Text", this.ResXFile, this.LanguageCode);
            this.NewFolderTitle.Text = Localization.GetString("cmdCreate.Text", this.ResXFile, this.LanguageCode);
            this.UploadTitle.Text = Localization.GetString("cmdUpload.Text", this.ResXFile, this.LanguageCode);
            this.AddFiles.Text = Localization.GetString("AddFiles.Text", this.ResXFile, this.LanguageCode);
            this.Wait.Text = Localization.GetString("Wait.Text", this.ResXFile, this.LanguageCode);
            this.WaitMessage.Text = Localization.GetString("WaitMessage.Text", this.ResXFile, this.LanguageCode);
            this.ExtraTabOptions.Text = Localization.GetString("ExtraTabOptions.Text", this.ResXFile, this.LanguageCode);
            this.LabelTabLanguage.Text = Localization.GetString("LabelTabLanguage.Text", this.ResXFile, this.LanguageCode);

            this.MaximumUploadSizeInfo.Text =
                string.Format(
                    Localization.GetString("FileSizeRestriction", this.ResXFile, this.LanguageCode),
                    this.MaxUploadSize / (1024 * 1024),
                    this.AcceptFileTypes.Replace("|", ","));

            // RadioButtonList
            this.BrowserMode.Items[0].Text = Localization.GetString("FileLink.Text", this.ResXFile, this.LanguageCode);
            this.BrowserMode.Items[1].Text = Localization.GetString("PageLink.Text", this.ResXFile, this.LanguageCode);

            // DropDowns
            this.LanguageList.Items[0].Text = Localization.GetString("None.Text", this.ResXFile, this.LanguageCode);
            this.AnchorList.Items[0].Text = Localization.GetString("None.Text", this.ResXFile, this.LanguageCode);

            // CheckBoxes
            this.chkAspect.Text = Localization.GetString("chkAspect.Text", this.ResXFile, this.LanguageCode);
            this.chkHumanFriendy.Text = Localization.GetString("chkHumanFriendy.Text", this.ResXFile, this.LanguageCode);
            this.TrackClicks.Text = Localization.GetString("TrackClicks.Text", this.ResXFile, this.LanguageCode);
            this.OverrideFile.Text = Localization.GetString("OverrideFile.Text", this.ResXFile, this.LanguageCode);

            // LinkButtons (with Image)
            this.Syncronize.Text = string.Format(
                "<img src=\"Images/SyncFolder.png\" alt=\"{0}\" title=\"{1}\" />",
                Localization.GetString("Syncronize.Text", this.ResXFile, this.LanguageCode),
                Localization.GetString("Syncronize.Help", this.ResXFile, this.LanguageCode));
            this.Syncronize.ToolTip = Localization.GetString("Syncronize.Help", this.ResXFile, this.LanguageCode);

            this.cmdCreate.Text = string.Format(
                "<img src=\"Images/CreateFolder.png\" alt=\"{0}\" title=\"{1}\" />", 
                Localization.GetString("cmdCreate.Text", this.ResXFile, this.LanguageCode), 
                Localization.GetString("cmdCreate.Help", this.ResXFile, this.LanguageCode));
            this.cmdCreate.ToolTip = Localization.GetString("cmdCreate.Help", this.ResXFile, this.LanguageCode);

            this.cmdDownload.Text =
                string.Format(
                    "<img src=\"Images/DownloadButton.png\" alt=\"{0}\" title=\"{1}\" />", 
                    Localization.GetString("cmdDownload.Text", this.ResXFile, this.LanguageCode), 
                    Localization.GetString("cmdDownload.Help", this.ResXFile, this.LanguageCode));
            this.cmdDownload.ToolTip = Localization.GetString("cmdDownload.Help", this.ResXFile, this.LanguageCode);

            this.cmdUpload.Text = string.Format(
                "<img src=\"Images/UploadButton.png\" alt=\"{0}\" title=\"{1}\" />", 
                Localization.GetString("cmdUpload.Text", this.ResXFile, this.LanguageCode), 
                Localization.GetString("cmdUpload.Help", this.ResXFile, this.LanguageCode));
            this.cmdUpload.ToolTip = Localization.GetString("cmdUpload.Help", this.ResXFile, this.LanguageCode);

            this.cmdDelete.Text = string.Format(
                "<img src=\"Images/DeleteFile.png\" alt=\"{0}\" title=\"{1}\" />", 
                Localization.GetString("cmdDelete.Text", this.ResXFile, this.LanguageCode), 
                Localization.GetString("cmdDelete.Help", this.ResXFile, this.LanguageCode));
            this.cmdDelete.ToolTip = Localization.GetString("cmdDelete.Help", this.ResXFile, this.LanguageCode);

            this.cmdResizer.Text = string.Format(
                "<img src=\"Images/ResizeImage.png\" alt=\"{0}\" title=\"{1}\" />", 
                Localization.GetString("cmdResizer.Text", this.ResXFile, this.LanguageCode), 
                Localization.GetString("cmdResizer.Help", this.ResXFile, this.LanguageCode));
            this.cmdResizer.ToolTip = Localization.GetString("cmdResizer.Help", this.ResXFile, this.LanguageCode);

            const string SwitchContent =
                "<a class=\"Switch{0}\" onclick=\"javascript: SwitchView('{0}');\" href=\"javascript:void(0)\"><img src=\"Images/{0}.png\" alt=\"{1}\" title=\"{2}\" />{1}</a>";

            this.SwitchDetailView.Text = string.Format(
                SwitchContent,
                "DetailView",
                Localization.GetString("DetailView.Text", this.ResXFile, this.LanguageCode),
                Localization.GetString("DetailViewTitle.Text", this.ResXFile, this.LanguageCode));
            this.SwitchDetailView.ToolTip = Localization.GetString("DetailViewTitle.Text", this.ResXFile, this.LanguageCode);

            this.SwitchListView.Text = string.Format(
                SwitchContent,
                "ListView",
                Localization.GetString("ListView.Text", this.ResXFile, this.LanguageCode),
                Localization.GetString("ListViewTitle.Text", this.ResXFile, this.LanguageCode));
            this.SwitchListView.ToolTip = Localization.GetString("ListViewTitle.Text", this.ResXFile, this.LanguageCode);

            this.SwitchIconsView.Text = string.Format(
                SwitchContent,
                "IconsView",
                Localization.GetString("IconsView.Text", this.ResXFile, this.LanguageCode),
                Localization.GetString("IconsViewTitle.Text", this.ResXFile, this.LanguageCode));
            this.SwitchIconsView.ToolTip = Localization.GetString("IconsViewTitle.Text", this.ResXFile, this.LanguageCode);

            this.SortAscending.Text = string.Format(
                 "<img src=\"Images/SortAscending.png\" alt=\"{0}\" title=\"{1}\" />",
                 Localization.GetString("SortAscending.Text", this.ResXFile, this.LanguageCode),
                 Localization.GetString("SortAscending.Help", this.ResXFile, this.LanguageCode));
            this.SortAscending.ToolTip = Localization.GetString("SortAscending.Help", this.ResXFile, this.LanguageCode);

            this.SortDescending.Text = string.Format(
                 "<img src=\"Images/SortDescending.png\" alt=\"{0}\" title=\"{1}\" />",
                 Localization.GetString("SortDescending.Text", this.ResXFile, this.LanguageCode),
                 Localization.GetString("SortDescending.Help", this.ResXFile, this.LanguageCode));
            this.SortDescending.ToolTip = Localization.GetString("SortDescending.Help", this.ResXFile, this.LanguageCode);

            ClientAPI.AddButtonConfirm(this.cmdDelete, Localization.GetString("AreYouSure.Text", this.ResXFile, this.LanguageCode));

            this.SetDefaultLinkTypeText();
        }

        /// <summary>
        /// The show directories in.
        /// </summary>
        /// <param name="dir">
        /// The directory.
        /// </param>
        private void ShowDirectoriesIn(string dir)
        {
            this.FillFolderTree(dir);
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
        private void ShowFileHelpUrl(string fileName, DotNetNuke.Services.FileSystem.FileInfo fileInfo)
        {
            try
            {
                this.SetDefaultLinkTypeText();

                // Enable Buttons
                this.CheckFolderAccess(fileInfo.FolderId, true);

                // Hide other Items if Secure Folder
                var folderPath = this.lblCurrentDir.Text;

                var isSecureFolder = false;

                var storageLocationType = this.GetStorageLocationType(folderPath);

                switch (storageLocationType)
                {
                    case FolderController.StorageLocationTypes.SecureFileSystem:
                        {
                            isSecureFolder = true;

                            fileName += ".resources";

                            this.cmdResizer.Enabled = false;
                            this.cmdResizer.CssClass = "LinkDisabled";

                            this.rblLinkType.Items[2].Selected = true;
                        }

                        break;
                    case FolderController.StorageLocationTypes.DatabaseSecure:
                        {
                            isSecureFolder = true;

                            this.cmdResizer.Enabled = false;
                            this.cmdResizer.CssClass = "LinkDisabled";

                            this.rblLinkType.Items[2].Selected = true;
                        }

                        break;
                    default:
                        {
                            this.rblLinkType.Items[0].Selected = true;

                            var extension = Path.GetExtension(fileName);
                            extension = extension.TrimStart('.');

                            var isAllowedExtension =
                                this.allowedImageExt.Any(sAllowExt => sAllowExt.Equals(extension.ToLower()));

                            this.cmdResizer.Enabled = isAllowedExtension;
                            this.cmdResizer.CssClass = isAllowedExtension ? "LinkNormal" : "LinkDisabled";
                        }

                        break;
                }

                this.rblLinkType.Items[0].Enabled = !isSecureFolder;
                this.rblLinkType.Items[1].Enabled = !isSecureFolder;
                //////

                this.FileId.Text = fileInfo.FileId.ToString();
                this.lblFileName.Text = fileName;

                // Relative Url  
                this.rblLinkType.Items[0].Text = Regex.Replace(
                    this.rblLinkType.Items[0].Text,
                    "/Images/MyImage.jpg",
                    MapUrl(Path.Combine(this.lblCurrentDir.Text, fileName)),
                    RegexOptions.IgnoreCase);

                var absoluteUrl = string.Format(
                    "{0}://{1}{2}{3}",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Authority,
                    MapUrl(this.lblCurrentDir.Text),
                    fileName);

                // Absolute Url
                this.rblLinkType.Items[1].Text = Regex.Replace(
                    this.rblLinkType.Items[1].Text,
                    "http://www.MyWebsite.com/Images/MyImage.jpg",
                    absoluteUrl,
                    RegexOptions.IgnoreCase);

                if (this.rblLinkType.Items.Count <= 2)
                {
                    return;
                }

                // LinkClick Url
                var link = string.Format("fileID={0}", fileInfo.FileId);

                var secureLink = Globals.LinkClick(link, int.Parse(this.request.QueryString["tabid"]), Null.NullInteger);

                this.rblLinkType.Items[2].Text =
                    this.rblLinkType.Items[2].Text.Replace(
                        @"/LinkClick.aspx?fileticket=xyz",
                        secureLink);

                absoluteUrl = string.Format(
                    "{0}://{1}{2}",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Authority,
                    secureLink);

                this.rblLinkType.Items[3].Text =
                    this.rblLinkType.Items[3].Text.Replace(
                        @"http://www.MyWebsite.com/LinkClick.aspx?fileticket=xyz",
                        absoluteUrl);
                ////////
            }
            catch (Exception)
            {
                this.SetDefaultLinkTypeText();
            }
        }

        /// <summary>
        /// Shows the files in directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pagerChanged">if set to <c>true</c> [pager changed].</param>
        private void ShowFilesIn(string directory, bool pagerChanged = false)
        {
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(directory, this._portalSettings);

            this.CheckFolderAccess(currentFolderInfo.FolderID, false);

            if (!pagerChanged)
            {
                this.FilesTable = this.GetFiles(currentFolderInfo);

                this.GetDiskSpaceUsed();
            }
            else
            {
                if (this.FilesTable == null)
                {
                    this.FilesTable = this.GetFiles(currentFolderInfo);
                }
            }

            var filesPagedDataSource = new PagedDataSource { DataSource = this.FilesTable.DefaultView };

            if (this.currentSettings.FileListPageSize > 0)
            {
                filesPagedDataSource.AllowPaging = true;
                filesPagedDataSource.PageSize = this.currentSettings.FileListPageSize;
                filesPagedDataSource.CurrentPageIndex = pagerChanged ? this.PagerFileLinks.CurrentPageIndex : 0;
            }

            this.PagerFileLinks.PageCount = filesPagedDataSource.PageCount;
            this.PagerFileLinks.RessourceFile = this.ResXFile;
            this.PagerFileLinks.LanguageCode = this.LanguageCode;

            this.PagerFileLinks.Visible = filesPagedDataSource.PageCount > 1;

            // this.FilesList.DataSource = this.GetFiles(directory);
            this.FilesList.DataSource = filesPagedDataSource;
            this.FilesList.DataBind();
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
            if (this.currentSettings.UploadFileSizeLimit > 0
                && file.ContentLength > this.currentSettings.UploadFileSizeLimit)
            {
                this.Page.ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "errorcloseScript",
                    string.Format(
                        "javascript:alert('{0}')",
                        Localization.GetString("FileToBigMessage.Text", this.ResXFile, this.LanguageCode)),
                    true);

                this.Response.End();

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
                    if (this.allowedFlashExt.Any(sAllowExt => sAllowExt.Equals(sExtension.ToLower())))
                    {
                        bAllowUpl = true;
                    }

                    break;
                case "ImageUpload":
                    if (this.allowedImageExt.Any(sAllowExt => sAllowExt.Equals(sExtension.ToLower())))
                    {
                        bAllowUpl = true;
                    }

                    break;
                case "FileUpload":
                    if (this.extensionWhiteList.Contains(sExtension.ToLower()))
                    {
                        bAllowUpl = true;
                    }

                    break;
            }

            if (bAllowUpl)
            {
                string sFileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

                int iCounter = 0;

                string sUploadDir = this.StartingDir();

                if (!this.currentSettings.UploadDirId.Equals(-1) && !this.currentSettings.SubDirs)
                {
                    var uploadFolder = new FolderController().GetFolderInfo(this._portalSettings.PortalId, this.currentSettings.UploadDirId);

                    if (uploadFolder != null)
                    {
                        sUploadDir = uploadFolder.PhysicalPath;
                    }
                }

                string sFilePath = Path.Combine(sUploadDir, fileName);

                if (File.Exists(sFilePath))
                {
                    iCounter++;
                    fileName = string.Format("{0}_{1}{2}", sFileNameNoExt, iCounter, Path.GetExtension(file.FileName));

                    // oFile.SaveAs(Path.Combine(sUploadDir, sFileName));
                    FileSystemUtils.UploadFile(sUploadDir, file, fileName);
                }
                else
                {
                    FileSystemUtils.UploadFile(sUploadDir, file, fileName);
                }

                this.Response.Write("<script type=\"text/javascript\">");
                this.Response.Write(this.GetJsUploadCode(fileName, MapUrl(sUploadDir)));
                this.Response.Write("</script>");
            }
            else
            {
                var error2Script = string.Format(
                     "javascript:alert('{0}')", Localization.GetString("Error2.Text", this.ResXFile, this.LanguageCode));

                this.Page.ClientScript.RegisterStartupScript(this.GetType(), "errorScript", error2Script, true);

                this.Response.End();
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
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "closeScript", "javascript:self.close();", true);
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
            this.panCreate.Visible = false;
        }

        /// <summary>
        /// Create a New Sub Folder
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CreateFolder_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(this.tbFolderName.Text))
            {
                if (Utility.ValidatePath(this.tbFolderName.Text))
                {
                    this.tbFolderName.Text = Utility.CleanPath(this.tbFolderName.Text);
                }

                this.tbFolderName.Text = Utility.CleanPath(this.tbFolderName.Text);

                var newDirPath = Path.Combine(this.lblCurrentDir.Text, this.tbFolderName.Text);

                try
                {
                    string sFolder = newDirPath;

                    sFolder = sFolder.Substring(this._portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                    FolderController folderController = new FolderController();

                    var storageLocation = (int)FolderController.StorageLocationTypes.InsecureFileSystem;

                    var currentStorageLocationType = this.GetStorageLocationType(this.lblCurrentDir.Text);

                    switch (currentStorageLocationType)
                    {
                        case FolderController.StorageLocationTypes.SecureFileSystem:
                            storageLocation = (int)FolderController.StorageLocationTypes.SecureFileSystem;
                            break;
                        case FolderController.StorageLocationTypes.DatabaseSecure:
                            storageLocation = (int)FolderController.StorageLocationTypes.DatabaseSecure;
                            break;
                    }

                    if (!Directory.Exists(newDirPath))
                    {
                        Directory.CreateDirectory(newDirPath);
                        var folderId = folderController.AddFolder(this._portalSettings.PortalId, sFolder, storageLocation, false, false);

                        this.SetFolderPermission(folderId);
                    }

                    this.lblCurrentDir.Text = string.Format("{0}\\", newDirPath);
                }
                catch (Exception exception)
                {
                    this.Response.Write("<script type=\"text/javascript\">");

                    var message =
                    exception.Message.Replace("'", string.Empty).Replace("\r\n", string.Empty).Replace(
                        "\n", string.Empty).Replace("\r", string.Empty);

                    this.Response.Write(string.Format("javascript:alert('{0}');", this.Context.Server.HtmlEncode(message)));

                    this.Response.Write("</script>");
                }
                finally
                {
                    this.ShowDirectoriesIn(this.StartingDir());

                    this.ShowFilesIn(newDirPath);

                    RadTreeNode tnNewFolder = this.FoldersTree.FindNodeByText(this.tbFolderName.Text);

                    if (tnNewFolder != null)
                    {
                        tnNewFolder.Selected = true;
                        tnNewFolder.ExpandParentNodes();
                    }
                }
            }

            this.panCreate.Visible = false;
        }

        /// <summary>
        /// Save the New Cropped Image
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void CropNow_Click(object sender, EventArgs e)
        {
            // Hide Image Editor Panels
            this.panImagePreview.Visible = false;
            this.panImageEdHead.Visible = false;
            this.panImageEditor.Visible = false;
            this.panThumb.Visible = false;

            // Show Link Panel
            this.panLinkMode.Visible = true;
            this.cmdClose.Visible = true;
            this.panInfo.Visible = true;

            if (this.browserModus.Equals("Link"))
            {
                this.BrowserMode.Visible = true;
            }

            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblModus.Text);

            // Add new file to database
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(this.lblCurrentDir.Text, this._portalSettings);

            FileSystemUtils.SynchronizeFolder(
                this._portalSettings.PortalId,
                currentFolderInfo.PhysicalPath,
                currentFolderInfo.FolderPath,
                false,
                this._portalSettings.HideFoldersEnabled);

            this.ShowFilesIn(this.lblCurrentDir.Text);

            string sExtension = Path.GetExtension(this.lblFileName.Text);

            this.GoToSelectedFile(this.txtCropImageName.Text + sExtension);
        }

        /// <summary>
        /// Hide Image Re-sizing Panel
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void ResizeCancel_Click(object sender, EventArgs e)
        {
            // Hide Image Editor Panels
            this.panImagePreview.Visible = false;
            this.panImageEdHead.Visible = false;
            this.panImageEditor.Visible = false;
            this.panThumb.Visible = false;

            // Show Link Panel
            this.panLinkMode.Visible = true;
            this.cmdClose.Visible = true;
            this.panInfo.Visible = true;
            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblModus.Text);

            if (this.browserModus.Equals("Link"))
            {
                this.BrowserMode.Visible = true;
            }
        }

        /// <summary>
        /// Resize Image based on User Input
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ResizeNow_Click(object sender, EventArgs e)
        {
            var filePath = Path.Combine(this.lblCurrentDir.Text, this.lblFileName.Text);

            var extension = Path.GetExtension(filePath);

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var oldImage = Image.FromStream(fileStream);

            string imageFullPath;

            int newWidth, newHeight;

            try
            {
                newWidth = int.Parse(this.txtWidth.Text);
            }
            catch (Exception)
            {
                newWidth = oldImage.Width;
            }

            try
            {
                newHeight = int.Parse(this.txtHeight.Text);
            }
            catch (Exception)
            {
                newHeight = oldImage.Height;
            }

            if (!string.IsNullOrEmpty(this.txtThumbName.Text))
            {
                imageFullPath = Path.Combine(this.lblCurrentDir.Text, this.txtThumbName.Text + extension);
            }
            else
            {
                imageFullPath = Path.Combine(
                    this.lblCurrentDir.Text,
                    string.Format("{0}_resized{1}", Path.GetFileNameWithoutExtension(filePath), extension));
            }

            // Create an Resized Thumbnail
            if (this.chkAspect.Checked)
            {
                var finalHeight = Math.Abs(oldImage.Height * newWidth / oldImage.Width);

                if (finalHeight > newHeight)
                {
                    // Height resize if necessary
                    newWidth = oldImage.Width * newHeight / oldImage.Height;
                    finalHeight = newHeight;
                }

                newHeight = finalHeight;
            }

            var counter = 0;

            while (File.Exists(imageFullPath))
            {
                counter++;

                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(imageFullPath);

                imageFullPath = Path.Combine(
                    this.lblCurrentDir.Text,
                    string.Format("{0}_{1}{2}", fileNameWithoutExtension, counter, Path.GetExtension(imageFullPath)));
            }

            // Add Compression to Jpeg Images
            if (oldImage.RawFormat.Equals(ImageFormat.Jpeg))
            {
                ImageCodecInfo jgpEncoder = GetEncoder(oldImage.RawFormat);

                Encoder myEncoder = Encoder.Quality;
                EncoderParameters encodParams = new EncoderParameters(1);
                EncoderParameter encodParam = new EncoderParameter(myEncoder, long.Parse(this.dDlQuality.SelectedValue));
                encodParams.Param[0] = encodParam;

                using (Bitmap dst = new Bitmap(newWidth, newHeight))
                {
                    using (Graphics g = Graphics.FromImage(dst))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.DrawImage(oldImage, 0, 0, dst.Width, dst.Height);
                    }

                    dst.Save(imageFullPath, jgpEncoder, encodParams);
                }
            }
            else
            {
                // Finally Create a new Resized Image
                Image newImage = oldImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero);
                oldImage.Dispose();

                newImage.Save(imageFullPath);
                newImage.Dispose();
            }

            // Add new file to database
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(this.lblCurrentDir.Text, this._portalSettings);

            FileSystemUtils.SynchronizeFolder(
                this._portalSettings.PortalId,
                currentFolderInfo.PhysicalPath,
                currentFolderInfo.FolderPath,
                false,
                this._portalSettings.HideFoldersEnabled);

            /*else if (OldImage.RawFormat.Equals(ImageFormat.Gif))
            {
                // Finally Create a new Resized Gif Image
                GifHelper gifHelper = new GifHelper();

                gifHelper.GetThumbnail(sFilePath,new Size(iNewWidth, iNewHeight), sImageFullPath);
            }*/

            // Hide Image Editor Panels
            this.panImagePreview.Visible = false;
            this.panImageEdHead.Visible = false;
            this.panImageEditor.Visible = false;
            this.panThumb.Visible = false;

            // Show Link Panel
            this.panLinkMode.Visible = true;
            this.cmdClose.Visible = true;
            this.panInfo.Visible = true;
            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblModus.Text);

            if (this.browserModus.Equals("Link"))
            {
                this.BrowserMode.Visible = true;
            }

            this.ShowFilesIn(this.lblCurrentDir.Text);

            this.GoToSelectedFile(Path.GetFileName(imageFullPath));
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
            this.panThumb.Visible = false;
            this.panImageEditor.Visible = true;

            this.imgOriginal.Visible = false;

            this.lblCropInfo.Visible = true;

            this.cmdRotate.Visible = false;
            this.cmdCrop.Visible = false;
            this.cmdZoom.Visible = false;
            this.cmdResize2.Visible = true;

            this.lblResizeHeader.Text = Localization.GetString("lblResizeHeader2.Text", this.ResXFile, this.LanguageCode);
            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblResizeHeader.Text);

            string sFilePath = Path.Combine(this.lblCurrentDir.Text, this.lblFileName.Text);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(sFilePath);

            this.txtCropImageName.Text = string.Format("{0}_Crop", sFileNameNoExt);

            FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            Image image = Image.FromStream(fs);

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
            sbCropZoom.AppendFormat("source: '{0}',", MapUrl(sFilePath));
            sbCropZoom.AppendFormat("width: {0},", image.Width);
            sbCropZoom.AppendFormat("height: {0},", image.Height);
            sbCropZoom.Append("minZoom: 10,");
            sbCropZoom.Append("maxZoom: 150");
            sbCropZoom.Append("}");
            sbCropZoom.Append("});");

            // Preview Button
            sbCropZoom.Append("jQuery('#PreviewCrop').click(function () {");

            sbCropZoom.Append("jQuery('#lblCropInfo').hide();");
            sbCropZoom.Append(
                "jQuery('#imgResized').attr('src', 'ProcessImage.ashx?' + cropzoom.PreviewParams()).show();");

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
                "cropzoom.send('ProcessImage.ashx', 'POST', { newFileName:  jQuery('#txtCropImageName').val(), saveFile: true }, function(){ javascript: __doPostBack('cmdCropNow', ''); });");
            sbCropZoom.Append("});");

            sbCropZoom.Append("});");

            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), string.Format("CropZoomScript{0}", Guid.NewGuid()), sbCropZoom.ToString(), true);
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
            this.panUploadDiv.Visible = false;
        }

        /// <summary>
        /// Upload Selected File
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void UploadNow_Click(object sender, EventArgs e)
        {
            this.ShowFilesIn(this.lblCurrentDir.Text);

            //this.GoToSelectedFile(fileName);

            this.panUploadDiv.Visible = false;
        }

        /// <summary>
        /// Show Preview of the Page links
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadTreeNodeEventArgs"/> instance containing the event data.</param>
        private void TreeTabs_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            if (this.dnntreeTabs.SelectedNode == null)
            {
                return;
            }

            this.SetDefaultLinkTypeText();

            var tabController = new TabController();

            var selectTab = tabController.GetTab(
                int.Parse(this.dnntreeTabs.SelectedValue), this._portalSettings.PortalId, true);

            string sDomainName = string.Format("http://{0}", Globals.GetDomainName(this.Request, true));

            // Add Language Parameter ?!
            var localeSelected = this.LanguageRow.Visible && this.LanguageList.SelectedIndex > 0;

            if (this.chkHumanFriendy.Checked)
            {
                var fileName = localeSelected
                                       ? Globals.FriendlyUrl(
                                           selectTab,
                                           string.Format(
                                               "{0}&language={1}",
                                               Globals.ApplicationURL(selectTab.TabID),
                                               this.LanguageList.SelectedValue),
                                           this._portalSettings)
                                       : Globals.FriendlyUrl(
                                           selectTab, Globals.ApplicationURL(selectTab.TabID), this._portalSettings);

                // Relative Url
                fileName = Globals.ResolveUrl(Regex.Replace(fileName, sDomainName, "~", RegexOptions.IgnoreCase));

                this.rblLinkType.Items[0].Text = Regex.Replace(
                    this.rblLinkType.Items[0].Text,
                    "/Images/MyImage.jpg",
                    Globals.ResolveUrl(Regex.Replace(fileName, sDomainName, "~", RegexOptions.IgnoreCase)),
                    RegexOptions.IgnoreCase);

                // Absolute Url  
                this.rblLinkType.Items[1].Text = Regex.Replace(
                    this.rblLinkType.Items[1].Text,
                    "http://www.MyWebsite.com/Images/MyImage.jpg",
                    Regex.Replace(fileName, sDomainName, string.Format("{0}", sDomainName), RegexOptions.IgnoreCase),
                    RegexOptions.IgnoreCase);
            }
            else
            {
                string locale = localeSelected ? string.Format("language/{0}/", this.LanguageList.SelectedValue) : string.Empty;

                // Relative Url
                this.rblLinkType.Items[0].Text = Regex.Replace(
                    this.rblLinkType.Items[0].Text,
                    "/Images/MyImage.jpg",
                    Globals.ResolveUrl(string.Format("~/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale)),
                    RegexOptions.IgnoreCase);

                // Absolute Url  
                this.rblLinkType.Items[1].Text = Regex.Replace(
                    this.rblLinkType.Items[1].Text,
                    "http://www.MyWebsite.com/Images/MyImage.jpg",
                    string.Format("{2}/tabid/{0}/{1}Default.aspx", selectTab.TabID, locale, sDomainName),
                    RegexOptions.IgnoreCase);
            }

            /////

            var secureLink = Globals.LinkClick(
               selectTab.TabID.ToString(), int.Parse(this.request.QueryString["tabid"]), Null.NullInteger);

            if (secureLink.Contains("&language"))
            {
                secureLink = secureLink.Remove(secureLink.IndexOf("&language"));
            }

            this.rblLinkType.Items[2].Text =
                this.rblLinkType.Items[2].Text.Replace(@"/LinkClick.aspx?fileticket=xyz", secureLink);

            var absoluteUrl = string.Format(
                "{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.Url.Authority,
                secureLink);

            this.rblLinkType.Items[3].Text =
                this.rblLinkType.Items[3].Text.Replace(
                    @"http://www.MyWebsite.com/LinkClick.aspx?fileticket=xyz", absoluteUrl);

            if (this.currentSettings.UseAnchorSelector)
            {
                this.FindAnchorsOnTab(selectTab);
            }

            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(),
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
            this.AnchorList.Items.Clear();

            var noneText = Localization.GetString("None.Text", this.ResXFile, this.LanguageCode);

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
                    this.AnchorList.Items.Add(i.Anchor);
                }

                // Add No Anchor item
                this.AnchorList.Items.Insert(0, noneText);
            }
            catch (Exception)
            {
                // Add No Anchor item
                this.AnchorList.Items.Add(noneText);
            }
        }

        /// <summary>
        /// Show Info for Selected File
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        private void FilesList_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            foreach (RepeaterItem item in this.FilesList.Items)
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

            var currentFile = new FileController().GetFileById(fileId, this._portalSettings.PortalId);

            this.ShowFileHelpUrl(currentFile.FileName, currentFile);

            this.ScrollToSelectedFile(fileListItem.ClientID);
        }

        /// <summary>
        /// Switch Browser in Link Modus between Link and Page Mode
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void BrowserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.BrowserMode.SelectedValue)
            {
                case "file":
                    this.panLinkMode.Visible = true;
                    this.panPageMode.Visible = false;
                    this.lblModus.Text = string.Format("Browser-Modus: {0}", this.browserModus);
                    break;
                case "page":
                    this.panLinkMode.Visible = false;
                    this.panPageMode.Visible = true;
                    this.TrackClicks.Visible = false;
                    this.lblModus.Text = string.Format("Browser-Modus: {0}", string.Format("Page {0}", this.browserModus));

                    this.RenderTabs();
                    break;
            }
            
            this.title.InnerText = string.Format("{0} - WatchersNET.FileBrowser", this.lblModus.Text);

            this.SetDefaultLinkTypeText();
        }

        /// <summary>
        /// Show / Hide "Track Clicks" Setting
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void LinkType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.rblLinkType.SelectedValue)
            {
                case "lnkClick":
                    this.TrackClicks.Visible = true;
                    break;
                case "lnkAbsClick":
                    this.TrackClicks.Visible = true;
                    break;
                default:
                    this.TrackClicks.Visible = false;
                    break;
            }
        }

        /// <summary>
        /// Load Files of Selected Folder
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RadTreeNodeEventArgs" /> instance containing the event data.</param>
        private void FoldersTree_NodeClick(object sender, RadTreeNodeEventArgs e)
        {
            var newDir = this.FoldersTree.SelectedNode.Value;

            this.lblCurrentDir.Text = !newDir.EndsWith("\\") ? string.Format("{0}\\", newDir) : newDir;

            this.ShowFilesIn(newDir);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;

            // Expand Sub folders (if) exists
            this.FoldersTree.SelectedNode.Expanded = true;
        }

        /// <summary>
        /// Gets the disk space used.
        /// </summary>
        private void GetDiskSpaceUsed()
        {
            var spaceAvailable = this._portalSettings.HostSpace.Equals(0)
                                     ? Localization.GetString("UnlimitedSpace.Text", this.ResXFile, this.LanguageCode)
                                     : string.Format("{0}MB", this._portalSettings.HostSpace);

            var spaceUsed = new PortalController().GetPortalSpaceUsedBytes(this._portalSettings.PortalId);

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

            this.FileSpaceUsedLabel.Text =
                string.Format(
                    Localization.GetString("SpaceUsed.Text", this.ResXFile, this.LanguageCode),
                    usedSpace,
                    spaceAvailable);
        }

        /// <summary>
        /// Gets the accepted file types.
        /// </summary>
        private void GetAcceptedFileTypes()
        {
            switch (this.browserModus)
            {
                case "Flash":
                    this.AcceptFileTypes = string.Join("|", this.allowedFlashExt);

                    break;
                case "Image":
                    this.AcceptFileTypes = string.Join("|", this.allowedImageExt);

                    break;
                default:
                    this.AcceptFileTypes = this.extensionWhiteList.Replace(",", "|");
                    break;
            }
        }

        #endregion
    }
}