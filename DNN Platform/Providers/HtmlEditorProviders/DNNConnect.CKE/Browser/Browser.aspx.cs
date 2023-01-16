﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Browser
{
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
    using DNNConnect.CKEditorProvider.Helper;
    using DNNConnect.CKEditorProvider.Objects;
    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;
    using Microsoft.JScript;

    using Convert = System.Convert;
    using Encoder = System.Drawing.Imaging.Encoder;
    using Globals = DotNetNuke.Common.Globals;
    using Image = System.Drawing.Image;

    /// <summary>The browser.</summary>
    [ScriptService]
    public partial class Browser : PageBase
    {
        private const string FileItemDisplayFormat =
            "<span class=\"FileName\">{2}</span><br /><span class=\"FileInfo\">{0}: {3}</span><br /><span class=\"FileInfo\">{1}: {4}</span>";

        /// <summary>The allowed flash extensions.</summary>
        private static readonly ISet<string> AllowedFlashExtensions = new HashSet<string>(new[] { "swf", "flv", "mp3" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>The allowed image extensions.</summary>
        private static readonly ISet<string> AllowedImageExtensions = new HashSet<string>(new[] { "bmp", "gif", "jpeg", "jpg", "png", "svg" }, StringComparer.OrdinalIgnoreCase);

        /// <summary>The Image or Link that is selected inside the Editor.</summary>
        private static string ckFileUrl;

        /// <summary>  The request.</summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>Current Settings Base.</summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>Settings Base for All Portals.</summary>
        private EditorProviderSettings allPortalsSettings = new EditorProviderSettings();

        /// <summary>  The portal settings.</summary>
        private IPortalSettings portalSettings;

        /// <summary>  The extension white list.</summary>
        private FileExtensionWhitelist extensionWhiteList;

        /// <summary>The browser modus.</summary>
        private string browserModus;

        /// <summary>Gets or sets the accept file types.</summary>
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

        /// <summary>  Gets Current Language from Url.</summary>
        protected string LanguageCode
        {
            get
            {
                return !string.IsNullOrEmpty(this.request.QueryString["lang"])
                           ? this.request.QueryString["lang"]
                           : "en-US";
            }
        }

        /// <summary>Gets the Name for the Current Resource file name.</summary>
        /// <value>
        /// The resource executable file.
        /// </value>
        protected string ResXFile
        {
            get
            {
                string[] page = this.Request.ServerVariables["SCRIPT_NAME"].Split('/');

                string fileRoot = string.Format(
                    "{0}/{1}/{2}.resx",
                    this.TemplateSourceDirectory.Replace("/DNNConnect.CKE/Browser", "/DNNConnect.CKE"),
                    Localization.LocalResourceDirectory,
                    page[page.GetUpperBound(0)]);

                return fileRoot;
            }
        }

        /// <summary>Gets the maximum size of the upload.</summary>
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

        /// <summary>Gets or sets the current folder ID.</summary>
        protected int CurrentFolderId
        {
            get
            {
                if (this.ViewState["CurrentFolderId"] != null)
                {
                    return Convert.ToInt32(this.ViewState["CurrentFolderId"]);
                }

                return this.StartingDir().FolderID;
            }

            set
            {
                this.ViewState["CurrentFolderId"] = value;
            }
        }

        /// <summary>Gets or sets the files table.</summary>
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

        /// <summary>Gets or sets a value indicating whether [sort files Ascending].</summary>
        /// <value>
        ///   <c>true</c> if [sort files Ascending].
        /// </value>
        private bool SortFilesAscending
        {
            get
            {
                return this.ViewState["SortFilesAscending"] != null && (bool)this.ViewState["SortFilesAscending"];
            }

            set
            {
                this.ViewState["SortFilesAscending"] = value;
                this.FilesTable = null;
            }
        }

        /// <summary>Gets or sets a value indicating whether [sort files descending].</summary>
        /// <value>
        ///   <c>true</c> if [sort files descending].
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
                this.FilesTable = null;
            }
        }

        /// <summary>Gets or sets a value indicating whether [sort files by Ascending Date].</summary>
        /// <value>
        ///   <c>true</c> if [sort files by ascending date].
        /// </value>
        private bool SortFilesDateAscending
        {
            get
            {
                return this.ViewState["SortFilesDateAscending"] != null && (bool)this.ViewState["SortFilesDateAscending"];
            }

            set
            {
                this.ViewState["SortFilesDateAscending"] = value;
                this.FilesTable = null;
            }
        }

        /// <summary>Gets or sets a value indicating whether [sort files by descending Date].</summary>
        /// <value>
        ///   <c>true</c> if [sort files by descending date]; otherwise sort by ascending Date.
        /// </value>
        private bool SortFilesDateDescending
        {
            get
            {
                return this.ViewState["SortFilesDateDescending"] != null && (bool)this.ViewState["SortFilesDateDescending"];
            }

            set
            {
                this.ViewState["SortFilesDateDescending"] = value;
                this.FilesTable = null;
            }
        }

        /// <summary>Set the file url from JavaScript to code.</summary>
        /// <param name="fileUrl">
        /// The file url.
        /// </param>
        [WebMethod]
        public static void SetFile(string fileUrl)
        {
            ckFileUrl = fileUrl;
        }

        /// <summary>Get all Files and Put them in a DataTable for the GridView.</summary>
        /// <param name="currentFolderInfo">The current folder info.</param>
        /// <returns>
        /// The File Table.
        /// </returns>
        public DataTable GetFiles(IFolderInfo currentFolderInfo)
        {
            var sizeResx = this.LocalizeString("Size.Text");
            var createdResx = this.LocalizeString("Created.Text");

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

            // Get the files
            var files = FolderManager.Instance.GetFiles(currentFolderInfo).ToList();

            if (this.SortFilesAscending)
            {
                Utility.SortAscending(files, item => item.FileName);
            }

            if (this.SortFilesDescending)
            {
                Utility.SortDescending(files, item => item.FileName);
            }

            if (this.SortFilesDateAscending)
            {
                Utility.SortAscending(files, item => item.CreatedOnDate);
            }

            if (this.SortFilesDateDescending)
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

                                dr["Info"] = string.Format(
                                    FileItemDisplayFormat,
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

                                dr["Info"] = string.Format(
                                    FileItemDisplayFormat,
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

                            if (extension.Count() <= 1 || !this.extensionWhiteList.IsAllowedExtension(extension))
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

                            if (AllowedImageExtensions.Any(sAllowImgExt => name.EndsWith(sAllowImgExt, StringComparison.OrdinalIgnoreCase)))
                            {
                                dr["PictureUrl"] = FileManager.Instance.GetUrl(fileItem);
                            }

                            dr["FileName"] = name;
                            dr["FileId"] = fileItem.FileId;

                            dr["Info"] = string.Format(
                                FileItemDisplayFormat,
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

        /// <summary>Register JavaScripts and CSS.</summary>
        /// <param name="e">
        /// The Event Args.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            this.LoadFavIcon();

            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/Browser.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.ImageSlider.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.cropzoom.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.ImageResizer.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.pagemethod.js"));
            ClientResourceManager.RegisterScript(this.Page, this.ResolveUrl("js/jquery.fileupload.comb.min.js"));
            ClientResourceManager.RegisterStyleSheet(this.Page, "https://ajax.googleapis.com/ajax/libs/jqueryui/1/themes/blitzer/jquery-ui.css");
            ClientResourceManager.RegisterStyleSheet(this.Page, this.ResolveUrl("Browser.comb.min.css"));

            this.GetSelectedImageOrLink();

            base.OnPreRender(e);
        }

        /// <summary>Close Browser Window.</summary>
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
                    int.Parse(this.dnntreeTabs.SelectedValue), this.portalSettings.PortalId, true);

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
                                          this.portalSettings)
                                      : Globals.FriendlyUrl(
                                          selectTab, Globals.ApplicationURL(selectTab.TabID), this.portalSettings);

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
                    var fileInfo = FileManager.Instance.GetFile(int.Parse(this.FileId.Text));

                    var filePath = FileManager.Instance.GetUrl(fileInfo);

                    if (this.rblLinkType.SelectedValue.Equals("absLnk", StringComparison.InvariantCultureIgnoreCase))
                    {
                        filePath = this.BuildAbsoluteUrl(filePath);
                    }

                    this.Response.Write("<script type=\"text/javascript\">");
                    this.Response.Write(this.GetJavaScriptCode(string.Empty, filePath, false));
                    this.Response.Write("</script>");

                    this.Response.End();
                }
                else
                {
                    this.Response.Write("<script type=\"text/javascript\">");
                    this.Response.Write(
                        string.Format(
                            "javascript:alert('{0}');", this.LocalizeString("Error5.Text")));
                    this.Response.Write("</script>");

                    this.Response.End();
                }
            }
        }

        /// <summary>Gets the java script code.</summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileUrl">The file URL.</param>
        /// <param name="isPageLink">if set to <c>true</c> [is page link].</param>
        /// <returns>
        /// Returns the java script code.
        /// </returns>
        protected virtual string GetJavaScriptCode(string fileName, string fileUrl, bool isPageLink)
        {
            if (!string.IsNullOrEmpty(fileUrl) && !string.IsNullOrEmpty(fileName))
            {
                // If we have both, combine them
                fileUrl = !fileUrl.EndsWith("/")
                               ? string.Format("{0}/{1}", fileUrl, fileName)
                               : string.Format("{0}{1}", fileUrl, fileName);
            }
            else if (string.IsNullOrEmpty(fileUrl))
            {
                // If no URL, default to the file name
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

        /// <summary>Gets the java script upload code.</summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="fileUrl">The file url.</param>
        /// <returns>
        /// Returns the formatted java script block.
        /// </returns>
        protected virtual string GetJsUploadCode(string fileName, string fileUrl)
        {
            fileUrl = string.Format(!fileUrl.EndsWith("/") ? "{0}/{1}" : "{0}{1}", fileUrl, fileName);

            var httpRequest = HttpContext.Current.Request;

            // var _CKEditorName = request.QueryString["CKEditor"];
            // funcNum is null when EasyImageUpload is being used
            var funcNum = httpRequest.QueryString["CKEditorFuncNum"];

            var errorMsg = string.Empty;

            funcNum = Regex.Replace(funcNum, @"[^0-9]", string.Empty, RegexOptions.None);

            return string.Format(
                "var E = window.parent;E['CKEDITOR'].tools.callFunction({0},'{1}','{2}') ;",
                funcNum,
                GlobalObject.escape(fileUrl),
                errorMsg.Replace("'", "\\'"));
        }

        /// <summary>Handles the Page Changed event of the Pager FileLinks control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerFileLinks_PageChanged(object sender, EventArgs e)
        {
            this.ShowFilesIn(this.GetCurrentFolder(), true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>Sorts the Files in ascending order.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortAscendingClick(object sender, EventArgs e)
        {
            this.SortFilesAscending = true;
            this.SortFilesDescending = false;
            this.SortFilesDateAscending = false;
            this.SortFilesDateDescending = false;

            this.SetSortButtonClasses();

            this.ShowFilesIn(this.GetCurrentFolder(), true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>Sorts the Files in descending order.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortDescendingClick(object sender, EventArgs e)
        {
            this.SortFilesAscending = false;
            this.SortFilesDescending = true;
            this.SortFilesDateAscending = false;
            this.SortFilesDateDescending = false;

            this.SetSortButtonClasses();

            this.ShowFilesIn(this.GetCurrentFolder(), true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>Sorts the Files by Date in ascending order.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortByDateAscendingClick(object sender, EventArgs e)
        {
            this.SortFilesAscending = false;
            this.SortFilesDescending = false;
            this.SortFilesDateAscending = true;
            this.SortFilesDateDescending = false;

            this.SetSortButtonClasses();

            this.ShowFilesIn(this.GetCurrentFolder(), true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>Sorts the Files by Date in descending order.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SortByDateDescendingClick(object sender, EventArgs e)
        {
            this.SortFilesAscending = false;
            this.SortFilesDescending = false;
            this.SortFilesDateAscending = false;
            this.SortFilesDateDescending = true;

            this.SetSortButtonClasses();

            this.ShowFilesIn(this.GetCurrentFolder(), true);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;
        }

        /// <summary>Raises the <see cref="E:System.Web.UI.Control.Init"/> event to initialize the page.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>Handles the Load event of the Page control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            JavaScript.RequestRegistration(CommonJs.jQuery);

            this.SetSortButtonClasses();

            this.extensionWhiteList = Host.AllowedExtensionWhitelist;

            if (!string.IsNullOrEmpty(this.request.QueryString["mode"]))
            {
                this.currentSettings.SettingMode =
                    (SettingsMode)Enum.Parse(typeof(SettingsMode), this.request.QueryString["mode"]);
            }

            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(this.portalSettings.PortalId);

            this.allPortalsSettings = SettingsUtil.LoadEditorSettingsByKey(
                this.portalSettings,
                this.currentSettings,
                settingsDictionary,
                "DNNCKP#-1#",
                portalRoles);

            switch (this.currentSettings.SettingMode)
            {
                case SettingsMode.Default:
                    // Load Default Settings
                    this.currentSettings = SettingsUtil.GetDefaultSettings(
                        this.portalSettings,
                        this.portalSettings.HomeDirectoryMapPath,
                        objProvider.Attributes["ck_configFolder"],
                        portalRoles);
                    break;
                case SettingsMode.Host:
                    this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        this.currentSettings,
                        settingsDictionary,
                        "DNNCKH#",
                        portalRoles);
                    break;
                case SettingsMode.Portal:
                    this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        this.currentSettings,
                        settingsDictionary,
                        $"DNNCKP#{this.request.QueryString["PortalID"]}#",
                        portalRoles);
                    break;
                case SettingsMode.Page:
                    this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                        this.portalSettings,
                        this.currentSettings,
                        settingsDictionary,
                        $"DNNCKT#{this.request.QueryString["tabid"]}#",
                        portalRoles);
                    break;
                case SettingsMode.ModuleInstance:
                    this.currentSettings = SettingsUtil.LoadModuleSettings(
                        this.portalSettings,
                        this.currentSettings,
                        $"DNNCKMI#{this.request.QueryString["mid"]}#INS#{this.request.QueryString["ckId"]}#",
                        int.Parse(this.request.QueryString["mid"]),
                        portalRoles);
                    break;
            }

            // set current Upload file size limit
            this.currentSettings.UploadFileSizeLimit = SettingsUtil.GetCurrentUserUploadSize(
                this.currentSettings,
                this.portalSettings,
                HttpContext.Current.Request);

            if ((this.currentSettings.BrowserMode.Equals(BrowserType.StandardBrowser) || this.currentSettings.ImageButtonMode.Equals(ImageButtonType.EasyImageButton))
                && HttpContext.Current.Request.IsAuthenticated)
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
                        var browserModusText = this.LocalizeString("lblBrowserModus.Text");
                        var browserModusTypeKey = string.Format("BrowserModus.{0}.Text", this.browserModus);
                        var browserModusTypeText = this.LocalizeString(browserModusTypeKey);
                        this.lblModus.Text = string.Format(browserModusText, browserModusTypeText);

                        if (!this.IsPostBack)
                        {
                            this.AcceptFileTypes = this.GetAcceptedFileTypes();

                            this.title.Text = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", this.lblModus.Text);

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

                                        this.lblModus.Text = string.Format(
                                            this.LocalizeString("BrowserModus.Text"),
                                            string.Format("Page {0}", this.browserModus));
                                        this.title.Text = string.Format(
                                            "{0} - DNNConnect.CKEditorProvider.FileBrowser",
                                            this.lblModus.Text);

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
                        && !command.Equals("ImageUpload") && !command.Equals("EasyImageUpload"))
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
                    if (!this.IsPostBack)
                    {
                        this.OverrideFile.Checked = this.currentSettings.OverrideFileOnUpload;

                        this.SetLanguage();

                        this.GetLanguageList();

                        var startFolder = this.StartingDir();

                        this.FillFolderTree(startFolder);

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
                            var folderName = !string.IsNullOrEmpty(startFolder.FolderPath) ?
                                startFolder.FolderPath : this.LocalizeString("RootFolder.Text");
                            this.lblCurrentDir.Text = folderName;

                            this.ShowFilesIn(startFolder);
                        }
                    }

                    this.FillQualityPrecentages();
                }
            }
            else
            {
                var errorScript = string.Format(
                    "javascript:alert('{0}');self.close();", this.LocalizeString("Error1.Text"));

                this.Response.Write("<script type=\"text/javascript\">");
                this.Response.Write(errorScript);
                this.Response.Write("</script>");

                this.Response.End();
            }
        }

        /// <summary>Show Create New Folder Panel.</summary>
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

        /// <summary>Synchronize Current Folder With the Database.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Syncronize_Click(object sender, EventArgs e)
        {
            var currentFolderInfo = this.GetCurrentFolder();

            FolderManager.Instance.Synchronize(this.portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            // Reload Folder
            this.ShowFilesIn(this.GetCurrentFolder());
        }

        /// <summary>Delete Selected File.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Delete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileId.Text))
            {
                return;
            }

            var deleteFile = FileManager.Instance.GetFile(int.Parse(this.FileId.Text));

            var thumbFolder = Path.Combine(this.GetCurrentFolder().PhysicalPath, "_thumbs");

            var thumbPath =
                Path.Combine(thumbFolder, this.lblFileName.Text).Replace(
                    this.lblFileName.Text.Substring(this.lblFileName.Text.LastIndexOf(".", StringComparison.Ordinal)), ".png");

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
                this.Response.Write("<script type=\"text/javascript\">");

                var message =
                    exception.Message.Replace("'", string.Empty).Replace("\r\n", string.Empty).Replace(
                        "\n", string.Empty).Replace("\r", string.Empty);

                this.Response.Write(string.Format("javascript:alert('{0}');", this.Context.Server.HtmlEncode(message)));

                this.Response.Write("</script>");
            }
            finally
            {
                this.ShowFilesIn(this.GetCurrentFolder());

                this.SetDefaultLinkTypeText();

                this.FileId.Text = null;
                this.lblFileName.Text = null;
            }
        }

        /// <summary>Download selected File.</summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The EventArgs e.
        /// </param>
        protected void Download_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.FileId.Text))
            {
                return;
            }

            var downloadFile = FileManager.Instance.GetFile(int.Parse(this.FileId.Text));

            FileManager.Instance.WriteFileToResponse(downloadFile, ContentDisposition.Attachment);
        }

        /// <summary>Opens the Re-sizing Panel.</summary>
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

            this.lblResizeHeader.Text = this.LocalizeString("lblResizeHeader.Text");
            this.title.Text = string.Format("{0} - DNNConnect.CKEditorProvider.FileBrowser", this.lblResizeHeader.Text);

            // Hide all Unwanted Elements from the Image Editor
            this.cmdClose.Visible = false;
            this.panInfo.Visible = false;

            this.panImageEditor.Visible = false;
            this.lblCropInfo.Visible = false;

            var fileInfo = FileManager.Instance.GetFile(this.GetCurrentFolder(), this.lblFileName.Text);
            string sFilePath = FileManager.Instance.GetUrl(fileInfo);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(fileInfo.FileName);

            this.txtThumbName.Text = string.Format("{0}_resized", sFileNameNoExt);

            if (!AllowedImageExtensions.Contains(fileInfo.Extension))
            {
                return;
            }

            var fs = FileManager.Instance.GetFileContent(fileInfo);

            Image image = Image.FromStream(fs);

            StringBuilder sbScript1 = new StringBuilder();

            // Show Preview Images
            this.imgOriginal.ImageUrl = sFilePath;
            this.imgResized.ImageUrl = sFilePath;

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

            this.imgOriginal.ToolTip = this.LocalizeString("imgOriginal.Text");
            this.imgOriginal.AlternateText = this.imgOriginal.ToolTip;

            this.imgResized.ToolTip = this.LocalizeString("imgResized.Text");
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

        /// <summary>Show Upload Controls.</summary>
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

        /// <summary>Shows the files in directory.</summary>
        /// <param name="directory">The directory.</param>
        /// <param name="pagerChanged">if set to <c>true</c> [pager changed].</param>
        protected void ShowFilesIn(string directory, bool pagerChanged = false)
        {
            var currentFolderInfo = Utility.ConvertFilePathToFolderInfo(directory, this.portalSettings);

            this.ShowFilesIn(currentFolderInfo, pagerChanged);
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.ResXFile, this.LanguageCode);
        }

        /// <summary>Formats a MapPath into relative MapUrl.</summary>
        /// <param name="sPath">
        /// MapPath Input string.
        /// </param>
        /// <returns>
        /// The output URL string.
        /// </returns>
        private static string MapUrl(string sPath)
        {
            string sAppPath = HttpContext.Current.Server.MapPath("~");

            string sUrl = string.Format(
                "{0}",
                HttpContext.Current.Request.ApplicationPath + sPath.Replace(sAppPath, string.Empty).Replace("\\", "/"));

            return sUrl;
        }

        /// <summary>Get File Name without .resources extension.</summary>
        /// <param name="fileName">File Name.</param>
        /// <returns>Cleaned File Name.</returns>
        private static string GetFileNameCleaned(string fileName)
        {
            return fileName.Replace(".resources", string.Empty);
        }

        /// <summary>The get encoder.</summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <returns>
        /// The Encoder.
        /// </returns>
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
        }

        /// <summary>Sets the sort button classes.</summary>
        private void SetSortButtonClasses()
        {
            this.SortAscending.CssClass = !this.SortFilesAscending ? "ButtonNormal" : "ButtonSelected";
            this.SortDescending.CssClass = !this.SortFilesDescending ? "ButtonNormal" : "ButtonSelected";
            this.SortByDateAscending.CssClass = !this.SortFilesDateAscending ? "ButtonNormal" : "ButtonSelected";
            this.SortByDateDescending.CssClass = !this.SortFilesDateDescending ? "ButtonNormal" : "ButtonSelected";
        }

        /// <summary>Get Folder Icon.</summary>
        /// <param name="folderId">The folder id.</param>
        /// <returns>
        /// Returns if folder is Secure.
        /// </returns>
        private string GetFolderIcon(int folderId)
        {
            var folderInfo = FolderManager.Instance.GetFolder(folderId);
            return this.GetFolderIcon(folderInfo);
        }

        /// <summary>Get Folder Icon.</summary>
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

        /// <summary>Hide Create Items if User has no write access to the Current Folder.</summary>
        /// <param name="folderId">The folder id to check.</param>
        /// <param name="isFileSelected">if set to <c>true</c> [is file selected].</param>
        private void CheckFolderAccess(int folderId, bool isFileSelected)
        {
            var hasWriteAccess = Utility.CheckIfUserHasFolderWriteAccess(folderId, this.portalSettings);

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

        /// <summary>Set Folder Permission.</summary>
        /// <param name="folderId">The Folder Id.</param>
        private void SetFolderPermission(int folderId)
        {
            var folder = FolderManager.Instance.GetFolder(folderId);

            this.SetFolderPermission(folder);
        }

        /// <summary>Set Folder Permission.</summary>
        /// <param name="folderInfo">The folder info.</param>
        private void SetFolderPermission(IFolderInfo folderInfo)
        {
            FolderManager.Instance.CopyParentFolderPermissions(folderInfo);
        }

        /// <summary>Set Folder Permission for the Current User.</summary>
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
                                                AllowAccess = true,
                                            })
            {
                folderInfo.FolderPermissions.Add(folderPermission, true);
            }

            FolderPermissionController.SaveFolderPermissions((FolderInfo)folderInfo);
        }

        /// <summary>Sets the default link type text.</summary>
        private void SetDefaultLinkTypeText()
        {
            this.rblLinkType.Items[0].Text = this.LocalizeString("relLnk.Text");
            this.rblLinkType.Items[1].Text = this.LocalizeString("absLnk.Text");
        }

        /// <summary>Fill the Folder TreeView with all (Sub)Directories.</summary>
        /// <param name="currentFolderInfo">The current folder information.</param>
        private void FillFolderTree(IFolderInfo currentFolderInfo)
        {
            this.FoldersTree.Nodes.Clear();

            var folderName = !string.IsNullOrEmpty(currentFolderInfo.FolderPath) ?
                currentFolderInfo.FolderPath : this.LocalizeString("RootFolder.Text");

            TreeNode folderNode = new TreeNode
            {
                Text = folderName,
                Value = currentFolderInfo.FolderID.ToString(),
                ImageUrl = this.GetFolderIcon(currentFolderInfo),
                ////ExpandedImageUrl = "Images/folderOpen.gif"
            };

            this.FoldersTree.Nodes.Add(folderNode);

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
                folders.Cast<FolderInfo>().Select(folder => this.RenderFolder(folder, readableFolders)).Where(node => node != null))
            {
                folderNode.ChildNodes.Add(node);
            }
        }

        /// <summary>Fill Quality Values 1-100 %.</summary>
        private void FillQualityPrecentages()
        {
            for (int i = 00; i < 101; i++)
            {
                this.dDlQuality.Items.Add(new ListItem { Text = i.ToString(), Value = i.ToString() });
            }

            this.dDlQuality.Items[100].Selected = true;
        }

        /// <summary>The get portal settings.</summary>
        /// <returns>
        /// Current Portal Settings.
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

                PortalAliasInfo objPortalAliasInfo = PortalAliasController.Instance.GetPortalAlias(sPortalAlias);

                portalSettings = new PortalSettings(iTabId, objPortalAliasInfo);
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>Get the Current Starting Directory.</summary>
        /// <returns>
        /// Returns the Starting Directory.
        /// </returns>
        private IFolderInfo StartingDir()
        {
            IFolderInfo startingFolderInfo = null;

            if (this.browserModus == "Image" && !string.IsNullOrEmpty(this.allPortalsSettings.HostBrowserRootDirForImg))
            {
                startingFolderInfo = Utility.EnsureGetFolder(
                    this.portalSettings.PortalId,
                    this.allPortalsSettings.HostBrowserRootDirForImg);
            }
            else if (this.browserModus == "Image" && !this.currentSettings.BrowserRootDirForImgId.Equals(-1))
            {
                var rootFolder = FolderManager.Instance.GetFolder(this.currentSettings.BrowserRootDirForImgId);

                if (rootFolder != null)
                {
                    startingFolderInfo = rootFolder;
                }
            }
            else if (!string.IsNullOrEmpty(this.allPortalsSettings.HostBrowserRootDir))
            {
                startingFolderInfo = Utility.EnsureGetFolder(
                    this.portalSettings.PortalId,
                    this.allPortalsSettings.HostBrowserRootDir);
            }
            else if (!this.currentSettings.BrowserRootDirId.Equals(-1))
            {
                var rootFolder = FolderManager.Instance.GetFolder(this.currentSettings.BrowserRootDirId);

                if (rootFolder != null)
                {
                    startingFolderInfo = rootFolder;
                }
            }
            else
            {
                startingFolderInfo = FolderManager.Instance.GetFolder(this.portalSettings.PortalId, string.Empty);
            }

            if (Utility.IsInRoles(this.portalSettings.AdministratorRoleName, this.portalSettings))
            {
                return startingFolderInfo;
            }

            if (this.currentSettings.SubDirs)
            {
                startingFolderInfo = this.GetUserFolderInfo(startingFolderInfo.PhysicalPath);
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
                folderStart.Substring(this.portalSettings.HomeDirectoryMapPath.Length).Replace(
                    "\\", "/");

            startingFolderInfo = FolderManager.Instance.AddFolder(this.portalSettings.PortalId, folderStart);

            Directory.CreateDirectory(startingFolderInfo.PhysicalPath);

            this.SetFolderPermission(startingFolderInfo);

            return startingFolderInfo;
        }

        /// <summary>Gets the user folder Info.</summary>
        /// <param name="startingDir">The Starting Directory.</param>
        /// <returns>Returns the user folder path.</returns>
        private IFolderInfo GetUserFolderInfo(string startingDir)
        {
            IFolderInfo userFolderInfo;

            var userFolderPath = Path.Combine(startingDir, "userfiles");

            // Create "userfiles" folder if not exists
            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(this.portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(this.portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                this.SetFolderPermission(userFolderInfo);
            }

            // Create user folder based on the user id
            userFolderPath = Path.Combine(
                userFolderPath,
                string.Format("{0}\\", UserController.Instance.GetCurrentUserInfo().UserID));

            if (!Directory.Exists(userFolderPath))
            {
                var folderStart = userFolderPath;

                folderStart = folderStart.Substring(this.portalSettings.HomeDirectoryMapPath.Length).Replace("\\", "/");

                userFolderInfo = FolderManager.Instance.AddFolder(this.portalSettings.PortalId, folderStart);

                Directory.CreateDirectory(userFolderPath);

                this.SetFolderPermission(userFolderInfo);

                this.SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }
            else
            {
                userFolderInfo = Utility.ConvertFilePathToFolderInfo(userFolderPath, this.portalSettings);

                // make sure the user has the correct permissions set
                this.SetUserFolderPermission(userFolderInfo, UserController.Instance.GetCurrentUserInfo());
            }

            return userFolderInfo;
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.portalSettings = this.GetPortalSettings();

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
            this.dnntreeTabs.SelectedNodeChanged += this.TreeTabs_NodeClick;

            // this.FoldersTree.SelectedNodeChanged += new EventHandler(FoldersTree_SelectedNodeChanged);
            this.FoldersTree.SelectedNodeChanged += this.FoldersTree_NodeClick;

            this.FilesList.ItemCommand += this.FilesList_ItemCommand;

            this.ClientResourceLoader.DataBind();
            this.ClientResourceLoader.PreRender += (sender, args) => JavaScript.Register(this.Page);
        }

        /// <summary>Load Favicon from Current Portal Home Directory.</summary>
        private void LoadFavIcon()
        {
            this.favicon.Controls.Add(new LiteralControl(DotNetNuke.UI.Internals.FavIcon.GetHeaderLink(this.portalSettings.PortalId)));
        }

        /// <summary>Render all Directories and sub directories recursive.</summary>
        /// <param name="folderInfo">The folder Info.</param>
        /// <param name="readableFolders">The list of folders that the current user has READ access.</param>
        /// <returns>
        /// TreeNode List.
        /// </returns>
        private TreeNode RenderFolder(FolderInfo folderInfo, IDictionary<int, IEnumerable<IFolderInfo>> readableFolders)
        {
            TreeNode tnFolder = new TreeNode
            {
                Text = folderInfo.FolderName,
                Value = folderInfo.FolderID.ToString(),
                ImageUrl = this.GetFolderIcon(folderInfo),
            };

            if (!readableFolders.TryGetValue(folderInfo.FolderID, out IEnumerable<IFolderInfo> folders))
            {
                return tnFolder;
            }

            foreach (TreeNode node in
                folders.Cast<FolderInfo>().Select(folder => this.RenderFolder(folder, readableFolders)).Where(node => node != null))
            {
                tnFolder.ChildNodes.Add(node);
            }

            return tnFolder;
        }

        /// <summary>Gets the language list, and sets the default locale if Content Localization is Enabled.</summary>
        private void GetLanguageList()
        {
            foreach (
                var languageListItem in
                    new LocaleController().GetLocales(this.portalSettings.PortalId)
                                          .Values.Select(
                                              language => new ListItem { Text = language.Text, Value = language.Code }))
            {
                this.LanguageList.Items.Add(languageListItem);
            }

            if (this.LanguageList.Items.Count.Equals(1))
            {
                this.LanguageRow.Visible = false;
            }
            else
            {
                // Set default locale and remove no locale if Content Localization is Enabled
                if (!this.portalSettings.ContentLocalizationEnabled)
                {
                    return;
                }

                var currentTab = new TabController().GetTab(
                    int.Parse(this.request.QueryString["tabid"]), this.portalSettings.PortalId, false);

                if (currentTab == null || string.IsNullOrEmpty(currentTab.CultureCode))
                {
                    return;
                }

                this.LanguageList.Items.RemoveAt(0);

                var currentTabCultureItem = this.LanguageList.Items.FindByValue(currentTab.CultureCode);

                if (currentTabCultureItem != null)
                {
                    currentTabCultureItem.Selected = true;
                }
            }
        }

        /// <summary>Load the Portal Tabs for the Page Links TreeView Selector.</summary>
        private void RenderTabs()
        {
            if (this.dnntreeTabs.Nodes.Count > 0)
            {
                return;
            }

            var allPortalTabsList = TabController.GetPortalTabs(this.portalSettings.PortalId, -1, false, null, true, false, true, true, false);
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
            tvh.LoadNodes(allPortalTabs, this.dnntreeTabs.Nodes, getNodeId, getParentId, getNodeText, getNodeValue, getNodeImageURL, getParentIdCheck);
        }

        /// <summary>Scroll to a Selected File or Uploaded File.</summary>
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

        /// <summary>Select a folder and the file inside the Browser.</summary>
        /// <param name="fileUrl">
        /// The file url.
        /// </param>
        /// <returns>
        /// if folder was selected.
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

            var selectedDir = this.MapPath(fileUrl).Replace(fileName, string.Empty);

            if (!Directory.Exists(selectedDir))
            {
                ckFileUrl = null;
                return false;
            }

            this.lblCurrentDir.Text = selectedDir;

            var newDir = this.lblCurrentDir.Text;

            TreeNode tnNewFolder = this.FoldersTree.FindNode(newDir);

            if (tnNewFolder != null)
            {
                tnNewFolder.Selected = true;
                tnNewFolder.Expand();
                tnNewFolder.Expanded = true;
            }

            this.ShowFilesIn(newDir);

            this.GoToSelectedFile(fileName);

            return true;
        }

        /// <summary>JS Code that gets the selected File Url.</summary>
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

        /// <summary>Set Language for all Controls on this Page.</summary>
        private void SetLanguage()
        {
            // Buttons
            this.cmdResizeCancel.Text = this.LocalizeString("cmdResizeCancel.Text");
            this.cmdResizeNow.Text = this.LocalizeString("cmdResizeNow.Text");
            this.cmdUploadCancel.Text = this.LocalizeString("cmdUploadCancel.Text");
            this.cmdCancel.Text = this.LocalizeString("cmdCancel.Text");
            this.cmdClose.Text = this.LocalizeString("cmdClose.Text");
            this.cmdCreateFolder.Text = this.LocalizeString("cmdCreateFolder.Text");
            this.cmdCreateCancel.Text = this.LocalizeString("cmdCreateCancel.Text");
            this.cmdCrop.Text = this.LocalizeString("cmdCrop.Text");
            this.cmdZoom.Text = this.LocalizeString("cmdZoom.Text");
            this.cmdRotate.Text = this.LocalizeString("cmdRotate.Text");
            this.cmdResize2.Text = this.LocalizeString("cmdResize2.Text");
            this.cmdCropNow.Text = this.LocalizeString("cmdCropNow.Text");
            this.cmdCropCancel.Text = this.LocalizeString("cmdCropCancel.Text");

            // Labels
            this.lblConFiles.Text = this.LocalizeString("lblConFiles.Text");
            this.lblCurrent.Text = this.LocalizeString("lblCurrent.Text");
            this.lblSubDirs.Text = this.LocalizeString("lblSubDirs.Text");
            this.lblUrlType.Text = this.LocalizeString("lblUrlType.Text");
            this.rblLinkType.ToolTip = this.LocalizeString("lblUrlType.Text");
            this.lblChoosetab.Text = this.LocalizeString("lblChoosetab.Text");
            this.lblHeight.Text = this.LocalizeString("lblHeight.Text");
            this.lblWidth.Text = this.LocalizeString("lblWidth.Text");
            this.lblThumbName.Text = this.LocalizeString("lblThumbName.Text");
            this.lblImgQuality.Text = this.LocalizeString("lblImgQuality.Text");
            this.lblResizeHeader.Text = this.LocalizeString("lblResizeHeader.Text");
            this.lblOtherTools.Text = this.LocalizeString("lblOtherTools.Text");
            this.lblCropImageName.Text = this.LocalizeString("lblThumbName.Text");
            this.lblCropInfo.Text = this.LocalizeString("lblCropInfo.Text");
            this.lblShowPreview.Text = this.LocalizeString("lblShowPreview.Text");
            this.lblClearPreview.Text = this.LocalizeString("lblClearPreview.Text");
            this.lblOriginal.Text = this.LocalizeString("lblOriginal.Text");
            this.lblPreview.Text = this.LocalizeString("lblPreview.Text");
            this.lblNewFoldName.Text = this.LocalizeString("lblNewFoldName.Text");
            this.LabelAnchor.Text = this.LocalizeString("LabelAnchor.Text");
            this.NewFolderTitle.Text = this.LocalizeString("cmdCreate.Text");
            this.UploadTitle.Text = this.LocalizeString("cmdUpload.Text");
            this.AddFiles.Text = this.LocalizeString("AddFiles.Text");
            this.Wait.Text = this.LocalizeString("Wait.Text");
            this.WaitMessage.Text = this.LocalizeString("WaitMessage.Text");
            this.ExtraTabOptions.Text = this.LocalizeString("ExtraTabOptions.Text");
            this.LabelTabLanguage.Text = this.LocalizeString("LabelTabLanguage.Text");

            this.MaximumUploadSizeInfo.Text =
                string.Format(
                    this.LocalizeString("FileSizeRestriction"),
                    this.MaxUploadSize / (1024 * 1024),
                    this.AcceptFileTypes.Replace("|", ","));

            // RadioButtonList
            this.BrowserMode.Items[0].Text = this.LocalizeString("FileLink.Text");
            this.BrowserMode.Items[1].Text = this.LocalizeString("PageLink.Text");

            // DropDowns
            this.LanguageList.Items[0].Text = this.LocalizeString("None.Text");
            this.AnchorList.Items[0].Text = this.LocalizeString("None.Text");

            // CheckBoxes
            this.chkAspect.Text = this.LocalizeString("chkAspect.Text");
            this.chkHumanFriendy.Text = this.LocalizeString("chkHumanFriendy.Text");
            this.OverrideFile.Text = this.LocalizeString("OverrideFile.Text");

            // LinkButtons (with Image)
            this.Syncronize.Text = string.Format(
               "<img src=\"Images/SyncFolder.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("Syncronize.Text"), this.LocalizeString("Syncronize.Help"));
            this.Syncronize.ToolTip = this.LocalizeString("Syncronize.Help");

            this.cmdCreate.Text = string.Format(
                "<img src=\"Images/CreateFolder.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("cmdCreate.Text"), this.LocalizeString("cmdCreate.Help"));
            this.cmdCreate.ToolTip = this.LocalizeString("cmdCreate.Help");

            this.cmdDownload.Text =
                string.Format(
                    "<img src=\"Images/DownloadButton.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("cmdDownload.Text"), this.LocalizeString("cmdDownload.Help"));
            this.cmdDownload.ToolTip = this.LocalizeString("cmdDownload.Help");

            this.cmdUpload.Text = string.Format(
                "<img src=\"Images/UploadButton.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("cmdUpload.Text"), this.LocalizeString("cmdUpload.Help"));
            this.cmdUpload.ToolTip = this.LocalizeString("cmdUpload.Help");

            this.cmdDelete.Text = string.Format(
                "<img src=\"Images/DeleteFile.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("cmdDelete.Text"), this.LocalizeString("cmdDelete.Help"));
            this.cmdDelete.ToolTip = this.LocalizeString("cmdDelete.Help");

            this.cmdResizer.Text = string.Format(
                "<img src=\"Images/ResizeImage.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("cmdResizer.Text"), this.LocalizeString("cmdResizer.Help"));
            this.cmdResizer.ToolTip = this.LocalizeString("cmdResizer.Help");

            const string SwitchContent =
                "<a class=\"Switch{0}\" onclick=\"javascript: SwitchView('{0}');\" href=\"javascript:void(0)\"><img src=\"Images/{0}.png\" alt=\"{1}\" title=\"{2}\" />{1}</a>";

            this.SwitchDetailView.Text = string.Format(
                SwitchContent,
                "DetailView",
                this.LocalizeString("DetailView.Text"),
                this.LocalizeString("DetailViewTitle.Text"));
            this.SwitchDetailView.ToolTip = this.LocalizeString("DetailViewTitle.Text");

            this.SwitchListView.Text = string.Format(
                SwitchContent,
                "ListView",
                this.LocalizeString("ListView.Text"),
                this.LocalizeString("ListViewTitle.Text"));
            this.SwitchListView.ToolTip = this.LocalizeString("ListViewTitle.Text");

            this.SwitchIconsView.Text = string.Format(
                SwitchContent,
                "IconsView",
                this.LocalizeString("IconsView.Text"),
                this.LocalizeString("IconsViewTitle.Text"));
            this.SwitchIconsView.ToolTip = this.LocalizeString("IconsViewTitle.Text");

            this.SortAscending.Text = string.Format(
                 "<img src=\"Images/SortAscending.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("SortAscending.Text"), this.LocalizeString("SortAscending.Help"));
            this.SortAscending.ToolTip = this.LocalizeString("SortAscending.Help");

            this.SortDescending.Text = string.Format(
                 "<img src=\"Images/SortDescending.png\" alt=\"{0}\" title=\"{1}\" />", this.LocalizeString("SortDescending.Text"), this.LocalizeString("SortDescending.Help"));
            this.SortDescending.ToolTip = this.LocalizeString("SortDescending.Help");

            this.SortByDateAscending.Text = string.Format(
                 "<img src=\"Images/AscendingArrow.png\" alt=\"{0}\" title=\"{1}\" />{2}", this.LocalizeString("SortByDateAscending.Text"), this.LocalizeString("SortByDateAscending.Help"), this.LocalizeString("SortByDate.Text"));
            this.SortByDateAscending.ToolTip = this.LocalizeString("SortByDateAscending.Help");

            this.SortByDateDescending.Text = string.Format(
                 "<img src=\"Images/DescendingArrow.png\" alt=\"{0}\" title=\"{1}\" />{2}", this.LocalizeString("SortByDateDescending.Text"), this.LocalizeString("SortByDateDescending.Help"), this.LocalizeString("SortByDate.Text"));
            this.SortByDateDescending.ToolTip = this.LocalizeString("SortByDateDescending.Help");

            ClientAPI.AddButtonConfirm(this.cmdDelete, this.LocalizeString("AreYouSure.Text"));

            this.SetDefaultLinkTypeText();
        }

        /// <summary>Goes to selected file.</summary>
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

                var fileInfo = FileManager.Instance.GetFile(fileId);

                this.ShowFileHelpUrl(fileInfo.FileName, fileInfo);

                this.ScrollToSelectedFile(fileListItem.ClientID);
            }
        }

        /// <summary>Show Preview for the URLs.</summary>
        /// <param name="fileName">
        /// Selected FileName.
        /// </param>
        /// <param name="fileInfo">
        /// The file Info.
        /// </param>
        private void ShowFileHelpUrl(string fileName, IFileInfo fileInfo)
        {
            try
            {
                this.SetDefaultLinkTypeText();

                // Enable Buttons
                this.CheckFolderAccess(fileInfo.FolderId, true);

                var folder = FolderManager.Instance.GetFolder(fileInfo.FolderId);

                this.rblLinkType.Items[0].Selected = true;

                var isAllowedExtension = AllowedImageExtensions.Contains(Path.GetExtension(fileName).TrimStart('.'));

                this.cmdResizer.Enabled = this.cmdResizer.Enabled && isAllowedExtension;
                this.cmdResizer.CssClass = this.cmdResizer.Enabled ? "LinkNormal" : "LinkDisabled";

                this.FileId.Text = fileInfo.FileId.ToString();
                this.lblFileName.Text = fileName;
                var providerFileUrl = FileManager.Instance.GetUrl(fileInfo);

                // Relative Url  (Or provider default)
                this.rblLinkType.Items[0].Text = Regex.Replace(this.rblLinkType.Items[0].Text, "/Images/MyImage.jpg", providerFileUrl, RegexOptions.IgnoreCase);

                // Absolute Url
                this.rblLinkType.Items[1].Text = Regex.Replace(this.rblLinkType.Items[1].Text, "http://www.MyWebsite.com/Images/MyImage.jpg", this.BuildAbsoluteUrl(providerFileUrl), RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                this.SetDefaultLinkTypeText();
            }
        }

        private string BuildAbsoluteUrl(string fileUrl)
        {
            if (fileUrl.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                return fileUrl;
            }

            return string.Format(
                    "{0}://{1}{2}",
                    HttpContext.Current.Request.Url.Scheme,
                    HttpContext.Current.Request.Url.Authority,
                    fileUrl);
        }

        /// <summary>Shows the files in directory.</summary>
        /// <param name="currentFolderInfo">The current folder information.</param>
        /// <param name="pagerChanged">if set to <c>true</c> [pager changed].</param>
        private void ShowFilesIn(IFolderInfo currentFolderInfo, bool pagerChanged = false)
        {
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

        /// <summary>Uploads a File.</summary>
        /// <param name="file">
        /// The Uploaded File.
        /// </param>
        /// <param name="command">
        /// The Upload Command Type.
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
                        "javascript:alert('{0}')", this.LocalizeString("FileToBigMessage.Text")),
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
            bool bIsImage = false;

            switch (command)
            {
                case "FlashUpload":
                    if (AllowedFlashExtensions.Contains(sExtension))
                    {
                        bAllowUpl = true;
                    }

                    break;
                case "EasyImageUpload":
                case "ImageUpload":
                    if (AllowedImageExtensions.Contains(sExtension))
                    {
                        bAllowUpl = true;
                        bIsImage = true;
                    }

                    break;
                case "FileUpload":
                    if (this.extensionWhiteList.IsAllowedExtension(sExtension))
                    {
                        bAllowUpl = true;
                    }

                    break;
            }

            if (bAllowUpl)
            {
                string sFileNameNoExt = Path.GetFileNameWithoutExtension(fileName);

                int iCounter = 0;

                var uploadPhysicalPath = this.StartingDir().PhysicalPath;

                var currentFolderInfo = this.GetCurrentFolder();

                IFolderInfo uploadFolder = null;
                if (command == "ImageUpload" && !string.IsNullOrEmpty(this.allPortalsSettings.HostUploadDirForImg) && !this.currentSettings.SubDirs)
                {
                    uploadFolder = Utility.EnsureGetFolder(this.portalSettings.PortalId, this.allPortalsSettings.HostUploadDirForImg);
                }
                else if (command == "ImageUpload" && !this.currentSettings.UploadDirForImgId.Equals(-1) && !this.currentSettings.SubDirs)
                {
                    uploadFolder = FolderManager.Instance.GetFolder(this.currentSettings.UploadDirForImgId);
                }
                else if (!string.IsNullOrEmpty(this.allPortalsSettings.HostUploadDir) && !this.currentSettings.SubDirs)
                {
                    uploadFolder = Utility.EnsureGetFolder(this.portalSettings.PortalId, this.allPortalsSettings.HostUploadDir);
                }
                else if (!this.currentSettings.UploadDirId.Equals(-1) && !this.currentSettings.SubDirs)
                {
                    uploadFolder = FolderManager.Instance.GetFolder(this.currentSettings.UploadDirId);
                }

                if (uploadFolder != null)
                {
                    uploadPhysicalPath = uploadFolder.PhysicalPath;

                    currentFolderInfo = uploadFolder;
                }

                string sFilePath = Path.Combine(uploadPhysicalPath, fileName);
                if (File.Exists(sFilePath))
                {
                    iCounter++;
                    fileName = string.Format("{0}_{1}{2}", sFileNameNoExt, iCounter, Path.GetExtension(file.FileName));
                }

                if (!bIsImage)
                {
                    FileManager.Instance.AddFile(currentFolderInfo, fileName, file.InputStream);
                }
                else
                {
                    int maxWidth = this.currentSettings.ResizeWidthUpload;
                    int maxHeight = this.currentSettings.ResizeHeightUpload;
                    if (maxWidth <= 0 && maxHeight <= 0)
                    {
                        FileManager.Instance.AddFile(currentFolderInfo, fileName, file.InputStream);
                    }
                    else
                    {
                        // check if the size of the image is within boundaries
                        using (var uplImage = Image.FromStream(file.InputStream))
                        {
                            if (uplImage.Width > maxWidth || uplImage.Height > maxHeight)
                            {
                                // it's too big: we need to resize
                                int newWidth, newHeight;

                                // which determines the max: height or width?
                                double ratioWidth = (double)maxWidth / (double)uplImage.Width;
                                double ratioHeight = (double)maxHeight / (double)uplImage.Height;
                                if (ratioWidth < ratioHeight)
                                {
                                    // max width needs to be used
                                    newWidth = maxWidth;
                                    newHeight = (int)Math.Round(uplImage.Height * ratioWidth);
                                }
                                else
                                {
                                    // max height needs to be used
                                    newHeight = maxHeight;
                                    newWidth = (int)Math.Round(uplImage.Width * ratioHeight);
                                }

                                // Add Compression to Jpeg Images
                                if (uplImage.RawFormat.Equals(ImageFormat.Jpeg))
                                {
                                    ImageCodecInfo jpgEncoder = GetEncoder(uplImage.RawFormat);

                                    Encoder myEncoder = Encoder.Quality;
                                    EncoderParameters encodeParams = new EncoderParameters(1);
                                    EncoderParameter encodeParam = new EncoderParameter(myEncoder, 80L);
                                    encodeParams.Param[0] = encodeParam;

                                    using (Bitmap dst = new Bitmap(newWidth, newHeight))
                                    {
                                        using (Graphics g = Graphics.FromImage(dst))
                                        {
                                            g.SmoothingMode = SmoothingMode.AntiAlias;
                                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                                            g.DrawImage(uplImage, 0, 0, dst.Width, dst.Height);
                                        }

                                        using (var stream = new MemoryStream())
                                        {
                                            dst.Save(stream, jpgEncoder, encodeParams);
                                            FileManager.Instance.AddFile(currentFolderInfo, fileName, stream);
                                        }
                                    }
                                }
                                else
                                {
                                    // Finally Create a new Resized Image
                                    using (Image newImage =
                                           uplImage.GetThumbnailImage(newWidth, newHeight, null, IntPtr.Zero))
                                    {
                                        var imageFormat = uplImage.RawFormat;
                                        using (var stream = new MemoryStream())
                                        {
                                            newImage.Save(stream, imageFormat);
                                            FileManager.Instance.AddFile(currentFolderInfo, fileName, stream);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // fits within configured maximum dimensions
                                FileManager.Instance.AddFile(currentFolderInfo, fileName, file.InputStream);
                            }
                        }
                    }
                }

                if (command == "EasyImageUpload")
                {
                    var fileUrl = string.Format(!MapUrl(uploadPhysicalPath).EndsWith("/") ? "{0}/{1}" : "{0}{1}", MapUrl(uploadPhysicalPath), fileName);
                    this.Response.ClearContent();
                    this.Response.ContentType = "application/json";
                    this.Response.Write($"{{\"default\": \"{fileUrl}\"}}");
                }
                else
                {
                    var fileUrl = string.Format(!MapUrl(uploadPhysicalPath).EndsWith("/") ? "{0}/{1}" : "{0}{1}", MapUrl(uploadPhysicalPath), fileName);
                    this.Response.ClearContent();
                    this.Response.ContentType = "application/json";
                    this.Response.Write($"{{\"uploaded\": 1, \"fileName\": \"{fileName}\", \"url\": \"{fileUrl}\"}}");
                }

                this.Response.End();
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "errorcloseScript",
                    string.Format(
                        "javascript:alert('{0}')", this.LocalizeString("Error2.Text")),
                    true);

                this.Response.End();
            }
        }

        /// <summary>Exit Dialog.</summary>
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

        /// <summary>Hide Create New Folder Panel.</summary>
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

        /// <summary>Create a New Sub Folder.</summary>
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
                var newFolderId = Null.NullInteger;

                try
                {
                    var portalId = this.portalSettings.PortalId;
                    var currentFolder = this.GetCurrentFolder();
                    var newFolderPath = string.Format("{0}{1}/", currentFolder.FolderPath, this.tbFolderName.Text);

                    var folderMapping = FolderMappingController.Instance.GetFolderMapping(currentFolder.FolderMappingID);

                    if (!FolderManager.Instance.FolderExists(portalId, newFolderPath))
                    {
                        var newFolder = FolderManager.Instance.AddFolder(folderMapping, newFolderPath);
                        newFolderId = newFolder.FolderID;
                        this.SetFolderPermission(newFolder.FolderID);
                        this.lblCurrentDir.Text = newFolder.FolderPath;
                    }
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
                    this.FillFolderTree(this.StartingDir());

                    if (newFolderId > Null.NullInteger)
                    {
                        TreeNode tnNewFolder = this.FindNodeByValue(this.FoldersTree, newFolderId.ToString());

                        if (tnNewFolder != null)
                        {
                            tnNewFolder.Selected = true;
                            this.CurrentFolderId = newFolderId;

                            var expandNode = tnNewFolder.Parent;
                            while (expandNode != null)
                            {
                                expandNode.Expand();
                                expandNode = expandNode.Parent;
                            }
                        }
                    }

                    this.ShowFilesIn(this.GetCurrentFolder());
                }
            }

            this.panCreate.Visible = false;
        }

        /// <summary>Save the New Cropped Image.</summary>
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

            this.title.Text = $"{this.lblModus.Text} - DNNConnect.CKEditorProvider.FileBrowser";

            // Add new file to database
            var currentFolderInfo = this.GetCurrentFolder();

            FolderManager.Instance.Synchronize(this.portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            this.ShowFilesIn(this.GetCurrentFolder());

            string sExtension = Path.GetExtension(this.lblFileName.Text);

            this.GoToSelectedFile(string.Format("{0}{1}", this.txtCropImageName.Text, sExtension));
        }

        /// <summary>Hide Image Re-sizing Panel.</summary>
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
            this.title.Text = $"{this.lblModus.Text} - DNNConnect.CKEditorProvider.FileBrowser";

            if (this.browserModus.Equals("Link"))
            {
                this.BrowserMode.Visible = true;
            }
        }

        /// <summary>Resize Image based on User Input.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ResizeNow_Click(object sender, EventArgs e)
        {
            var file = FileManager.Instance.GetFile(this.GetCurrentFolder(), this.lblFileName.Text);
            string resizedFileName;

            using (var fileStream = FileManager.Instance.GetFileContent(file))
            {
                var oldImage = Image.FromStream(fileStream);
                int newWidth, newHeight;

                if (!int.TryParse(this.txtWidth.Text, out newWidth))
                {
                    newWidth = oldImage.Width;
                }

                if (!int.TryParse(this.txtHeight.Text, out newHeight))
                {
                    newHeight = oldImage.Height;
                }

                if (!string.IsNullOrEmpty(this.txtThumbName.Text))
                {
                    resizedFileName = $"{this.txtThumbName.Text}.{file.Extension}";
                }
                else
                {
                    resizedFileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_resized.{file.Extension}";
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
                while (FileManager.Instance.FileExists(this.GetCurrentFolder(), resizedFileName))
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
                    EncoderParameter encodeParam = new EncoderParameter(myEncoder, long.Parse(this.dDlQuality.SelectedValue));
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
                            FileManager.Instance.AddFile(this.GetCurrentFolder(), resizedFileName, stream);
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
                            FileManager.Instance.AddFile(this.GetCurrentFolder(), resizedFileName, stream);
                        }
                    }
                }
            }

            // Add new file to database
            var currentFolderInfo = this.GetCurrentFolder();

            FolderManager.Instance.Synchronize(this.portalSettings.PortalId, currentFolderInfo.FolderPath, false, true);

            // Hide Image Editor Panels
            this.panImagePreview.Visible = false;
            this.panImageEdHead.Visible = false;
            this.panImageEditor.Visible = false;
            this.panThumb.Visible = false;

            // Show Link Panel
            this.panLinkMode.Visible = true;
            this.cmdClose.Visible = true;
            this.panInfo.Visible = true;
            this.title.Text = $"{this.lblModus.Text} - DNNConnect.CKEditorProvider.FileBrowser";

            if (this.browserModus.Equals("Link"))
            {
                this.BrowserMode.Visible = true;
            }

            this.ShowFilesIn(this.GetCurrentFolder());

            this.GoToSelectedFile(resizedFileName);
        }

        /// <summary>Hide Resize Panel and Show CropZoom Panel.</summary>
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

            this.lblResizeHeader.Text = this.LocalizeString("lblResizeHeader2.Text");
            this.title.Text = $"{this.lblResizeHeader.Text} - DNNConnect.CKEditorProvider.FileBrowser";

            var file = FileManager.Instance.GetFile(this.GetCurrentFolder(), this.lblFileName.Text);
            string sFilePath = FileManager.Instance.GetUrl(file);

            string sFileNameNoExt = Path.GetFileNameWithoutExtension(file.FileName);
            this.txtCropImageName.Text = string.Format("{0}_Crop", sFileNameNoExt);

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

            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), string.Format("CropZoomScript{0}", Guid.NewGuid()), sbCropZoom.ToString(), true);
        }

        /// <summary>Cancel Upload - Hide Upload Controls.</summary>
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

        /// <summary>Upload Selected File.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void UploadNow_Click(object sender, EventArgs e)
        {
            this.FilesTable = null;
            this.ShowFilesIn(this.GetCurrentFolder());

            this.panUploadDiv.Visible = false;
        }

        /// <summary>Show Preview of the Page links.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void TreeTabs_NodeClick(object sender, EventArgs eventArgs)
        {
            if (this.dnntreeTabs.SelectedNode == null)
            {
                return;
            }

            this.SetDefaultLinkTypeText();

            var tabController = new TabController();

            var selectTab = tabController.GetTab(
                int.Parse(this.dnntreeTabs.SelectedValue), this.portalSettings.PortalId, true);

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
                                           this.portalSettings)
                                       : Globals.FriendlyUrl(
                                           selectTab, Globals.ApplicationURL(selectTab.TabID), this.portalSettings);

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

        /// <summary>Find and List all Anchors from the Selected Page.</summary>
        /// <param name="selectedTab">
        /// The selected tab.
        /// </param>
        private void FindAnchorsOnTab(TabInfo selectedTab)
        {
            // Clear Item list first...
            this.AnchorList.Items.Clear();

            var noneText = this.LocalizeString("None.Text");

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

        /// <summary>Show Info for Selected File.</summary>
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

            var currentFile = FileManager.Instance.GetFile(fileId);

            this.ShowFileHelpUrl(currentFile.FileName, currentFile);

            this.ScrollToSelectedFile(fileListItem.ClientID);
        }

        /// <summary>Switch Browser in Link Modus between Link and Page Mode.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void BrowserMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.BrowserMode.SelectedValue)
            {
                case "file":
                    this.panLinkMode.Visible = true;
                    this.panPageMode.Visible = false;
                    this.lblModus.Text = string.Format(this.LocalizeString("BrowserModus.Text"), this.browserModus);
                    break;
                case "page":
                    this.panLinkMode.Visible = false;
                    this.panPageMode.Visible = true;
                    this.lblModus.Text = string.Format(this.LocalizeString("BrowserModus.Text"), string.Format("Page {0}", this.browserModus));

                    this.RenderTabs();
                    break;
            }

            this.title.Text = $"{this.lblModus.Text} - DNNConnect.CKEditorProvider.FileBrowser";

            this.SetDefaultLinkTypeText();
        }

        /// <summary>Load Files of Selected Folder.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void FoldersTree_NodeClick(object sender, EventArgs eventArgs)
        {
            var folderId = Convert.ToInt32(this.FoldersTree.SelectedNode.Value);
            var folder = FolderManager.Instance.GetFolder(folderId);

            var folderName = !string.IsNullOrEmpty(folder.FolderPath) ?
                folder.FolderPath : this.LocalizeString("RootFolder.Text");
            this.lblCurrentDir.Text = folderName;

            this.CurrentFolderId = folderId;

            this.ShowFilesIn(folder);

            // Reset selected file
            this.SetDefaultLinkTypeText();

            this.FileId.Text = null;
            this.lblFileName.Text = null;

            // Expand Sub folders (if) exists
            this.FoldersTree.SelectedNode.Expanded = true;
        }

        /// <summary>Gets the disk space used.</summary>
        private void GetDiskSpaceUsed()
        {
            var spaceAvailable = this.portalSettings.HostSpace.Equals(0)
                                     ? this.LocalizeString("UnlimitedSpace.Text")
                                     : string.Format("{0}MB", this.portalSettings.HostSpace);

            var spaceUsed = new PortalController().GetPortalSpaceUsedBytes(this.portalSettings.PortalId);

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
                    this.LocalizeString("SpaceUsed.Text"),
                    usedSpace,
                    spaceAvailable);
        }

        /// <summary>Gets the accepted file types.</summary>
        private string GetAcceptedFileTypes()
        {
            switch (this.browserModus)
            {
                case "Flash":
                    return string.Join("|", AllowedFlashExtensions);
                case "Image":
                    return string.Join("|", AllowedImageExtensions);
                default:
                    return this.extensionWhiteList.ToStorageString().Replace(",", "|");
            }
        }

        private IFolderInfo GetCurrentFolder()
        {
            return FolderManager.Instance.GetFolder(this.CurrentFolderId);
        }

        private TreeNode FindNodeByValue(TreeView tree, string value)
        {
            return this.FindNodeByValue(tree.Nodes, value);
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
                    foundNode = this.FindNodeByValue(node.ChildNodes, value);
                }

                if (foundNode != null)
                {
                    break;
                }
            }

            return foundNode;
        }
    }
}
