// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DNNConnect.CKEditorProvider
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;
    using System.Xml.Serialization;

    using DNNConnect.CKEditorProvider.Constants;
    using DNNConnect.CKEditorProvider.Controls;
    using DNNConnect.CKEditorProvider.Extensions;
    using DNNConnect.CKEditorProvider.Objects;
    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>The CKEditor options page.</summary>
    public partial class CKEditorOptions : PortalModuleBase
    {
        /// <summary>The provider type.</summary>
        private const string ProviderType = "htmlEditor";
        private const string UnauthenticatedUsersRoleName = "Unauthenticated Users";
        private const int NoPortal = -1;
        private const string KeyCurrentTabId = "CurrentTabId";
        private const string KeyCurrentPortalId = "CurrentPortalId";
        private const string KeyDefaultHostLoadMode = "DefaultHostLoadMode";
        private const string KeyIsHostMode = "IsHostMode";
        private const string KeyCurrentPortalOnly = "CurrentPortalOnly";
        private const string KeyCurrentSettingsMode = "CurrentSettingsMode";

        /// <summary>  The provider config.</summary>
        private readonly ProviderConfiguration provConfig = ProviderConfiguration.GetProviderConfiguration(ProviderType);

        /// <summary>  The request.</summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>  The _portal settings.</summary>
        private IPortalSettings portalSettings;

        /// <summary>Override Default Config Folder from Web.config.</summary>
        private string configFolder = string.Empty;

        /// <summary>  The list toolbars.</summary>
        private List<ToolbarSet> listToolbars;

        /// <summary>  The list of all available toolbar buttons.</summary>
        private List<ToolbarButton> listButtons;

        /// <summary>  The list of all available toolbar buttons.</summary>
        private List<ToolbarButton> allListButtons;

        private EditorProviderSettings currentSettings;

        /// <summary>  The module instance name.</summary>
        private string moduleInstanceName;

        /// <summary>  Gets or sets The Full Toolbar Set.</summary>
        private ToolbarSet toolbarSets;

        /// <summary>The _current module.</summary>
        private ModuleInfo currentModule;

        /// <summary>Gets or sets a value indicating whether this instance is host mode.</summary>
        /// <value><see langword="true"/> if this instance is host mode; otherwise, <see langword="false"/>.</value>
        public bool IsHostMode
        {
            get
            {
                return this.ViewState[KeyIsHostMode] != null && (bool)this.ViewState[KeyIsHostMode];
            }

            set
            {
                this.ViewState[KeyIsHostMode] = value;
            }
        }

        /// <summary>Gets or sets a value indicating whether [current portal only].</summary>
        /// <value><see langword="true"/> if [current portal only]; otherwise, <see langword="false"/>.</value>
        public bool CurrentPortalOnly { get; set; }

        /// <summary>Gets or sets the Current or selected Tab ID.</summary>
        public int CurrentOrSelectedTabId
        {
            get
            {
                var currentTabId = this.ViewState[KeyCurrentTabId];
                if (currentTabId != null)
                {
                    return (int)currentTabId;
                }

                return NoPortal;
            }

            set
            {
                this.ViewState[KeyCurrentTabId] = value;
            }
        }

        /// <summary>Gets or sets the Current or selected Portal ID.</summary>
        public int CurrentOrSelectedPortalId
        {
            get
            {
                return (int?)this.ViewState[KeyCurrentPortalId] ?? 0;
            }

            set
            {
                this.ViewState[KeyCurrentPortalId] = value;
            }
        }

        /// <summary>Gets or sets the default host load mode.</summary>
        /// <value> The default host load mode.</value>
        public int DefaultHostLoadMode
        {
            get
            {
                return (int?)this.ViewState[KeyDefaultHostLoadMode] ?? 0;
            }

            set
            {
                this.ViewState[KeyDefaultHostLoadMode] = value;
            }
        }

        /// <summary>Gets Current Language from Url.</summary>
        protected string LangCode => !string.IsNullOrEmpty(this.request.QueryString["langCode"])
                                         ? this.request.QueryString["langCode"]
                                         : CultureInfo.CurrentCulture.Name;

        /// <summary>Gets the Name for the Current Resource file name.</summary>
        protected string ResXFile => this.ResolveUrl($"~/Providers/HtmlEditorProviders/DNNConnect.CKE/{Localization.LocalResourceDirectory}/Options.aspx.resx");

        /// <summary>Gets or sets the current settings mode.</summary>
        private SettingsMode CurrentSettingsMode
        {
            get
            {
                return (SettingsMode)this.ViewState[KeyCurrentSettingsMode];
            }

            set
            {
                this.ViewState[KeyCurrentSettingsMode] = value;
            }
        }

        /// <summary>Gets the Config Url Control.</summary>
        private UrlControl ConfigUrl => this.ctlConfigUrl;

        /// <summary>Gets the CSS Url Control.</summary>
        private UrlControl CssUrl => this.ctlCssurl;

        /// <summary>Gets the Import File Url Control.</summary>
        private UrlControl ImportFile => this.ctlImportFile;

        /// <summary>Gets the Template Url Control.</summary>
        private UrlControl TemplUrl => this.ctlTemplUrl;

        /// <summary>Gets the Custom JS File Url Control.</summary>
        private UrlControl CustomJsFile => this.ctlCustomJsFile;

        /// <summary>Gets the current module.</summary>
        /// <value>The current module.</value>
        private ModuleInfo CurrentModule
        {
            get
            {
                if (this.currentModule != null)
                {
                    return this.currentModule;
                }

                if (this.ModuleConfiguration != null && !Null.IsNull(this.ModuleConfiguration.ModuleID))
                {
                    this.currentModule = this.ModuleConfiguration;
                    return this.currentModule;
                }

                this.currentModule = new ModuleController().GetModule(
                    this.Request.QueryString.GetValueOrDefault("ModuleId", -1),
                    this.TabId,
                    false);

                return this.currentModule;
            }
        }

        /// <summary>Gets a value indicating whether this instance is all instances.</summary>
        /// <value><see langword="true"/> if this instance is all instances; otherwise, <see langword="false"/>.</value>
        private bool IsAllInstances => this.IsHostMode && !this.CurrentPortalOnly;

        /// <summary>Gets the home directory.</summary>
        /// <value>The home directory.</value>
        private string HomeDirectory => this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;

        /// <summary>Binds the options data.</summary>
        /// <param name="reloadOptionsfromModule">if set to <see langword="true"/> [reload options from module].</param>
        internal void BindOptionsData(bool reloadOptionsfromModule = false)
        {
            // Check if Options Window is in Host Page
            if (this.IsHostMode)
            {
                if (!this.Page.IsPostBack)
                {
                    this.LastTabId.Value = "0";
                }

                this.portalSettings = this.GetPortalSettings();

                this.listToolbars = ToolbarUtil.GetToolbars(this.HomeDirectory, this.configFolder);

                this.RenderUrlControls(true);

                this.FillRoles();

                this.BindUserGroupsGridView();

                this.lblSetFor.Visible = false;
                this.rBlSetMode.Visible = false;
                this.lnkRemoveAll.Visible = false;
                this.InfoTabLi.Visible = false;
                this.InfoTabHolder.Visible = false;

                if (this.DefaultHostLoadMode.Equals(0))
                {
                    var currentPortalLabel = string.Empty;
                    if (this.CurrentOrSelectedPortalId > NoPortal)
                    {
                        currentPortalLabel = string.Format(
                            "- <em>{0} {1} : - Portal ID: {2}</em>",
                            Localization.GetString("lblPortal.Text", this.ResXFile, this.LangCode),
                            this.portalSettings?.PortalName ?? "Host",
                            this.CurrentOrSelectedPortalId);
                    }

                    this.lblSettings.Text = $"{Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode)} {currentPortalLabel}";
                }
                else if (this.DefaultHostLoadMode.Equals(1))
                {
                    var currentTabName = new TabController().GetTab(this.CurrentOrSelectedTabId, this.portalSettings?.PortalId ?? Host.HostPortalID, false).TabName;
                    var pageLabel = Localization.GetString("lblPage.Text", this.ResXFile, this.LangCode);
                    var settingsLabel = Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode);

                    this.lblSettings.Text = $"{settingsLabel} - <em>{pageLabel} {currentTabName} - TabID: {this.CurrentOrSelectedTabId}</em>";
                }
                else
                {
                    this.lblSettings.Text = Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode);
                }

                this.LoadSettings(this.DefaultHostLoadMode);
            }
            else
            {
                var pageKey = $"DNNCKT#{this.CurrentOrSelectedTabId}#";
                var moduleKey = $"DNNCKMI#{this.ModuleId}#INS#{this.moduleInstanceName}#";

                if (SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.ModuleId))
                {
                    this.LoadSettings(2);
                }
                else
                {
                    var settingsDictionary = EditorController.GetEditorHostSettings();

                    this.LoadSettings(SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey) ? 1 : 0);
                }
            }
        }

        /// <summary>Raises the <see cref="E:System.Web.UI.Control.Init" /> event.</summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            this.InitializeComponent();
            base.OnInit(e);

            var moduleId = this.Request.QueryString.GetValueOrDefault("ModuleId", Null.NullInteger);
            if (moduleId != Null.NullInteger && this.ModuleId == Null.NullInteger)
            {
                var module = ModuleController.Instance.GetModule(moduleId, this.TabId, false);
                if (module != null)
                {
                    this.ModuleConfiguration = module;
                }
            }
        }

        /// <summary>Handles the Load event of the Page control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.AddJavaScripts();
            this.LocalResourceFile = this.ResXFile;
            this.portalSettings = this.GetPortalSettings();

            if (this.IsAllInstances ? UserController.Instance.GetCurrentUserInfo().IsSuperUser : Utility.IsInRoles(this.portalSettings.AdministratorRoleName, this.portalSettings))
            {
                this.listToolbars = ToolbarUtil.GetToolbars(this.HomeDirectory, this.configFolder);

                if (this.Page.IsPostBack)
                {
                    return;
                }

                this.BindUserGroupsGridView();

                this.FillFolders();

                this.SetLanguage();

                this.BindOptionsData();

                this.FillInformations();

                // Load Skin List
                this.FillSkinList();

                this.RenderUrlControls();

                this.FillRoles();

                // Remove CKFinder from the Browser list if not installed
                if (
                    !File.Exists(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js"))
                {
                    this.ddlBrowser.Items.RemoveAt(2);
                }
            }
            else
            {
                this.Visible = false;

                this.Page.ClientScript.RegisterStartupScript(
                    this.GetType(),
                    "errorcloseScript",
                    $"javascript:alert({HttpUtility.JavaScriptStringEncode(Localization.GetString("Error1.Text", this.ResXFile, this.LangCode), addDoubleQuotes: true)});self.close();",
                    true);
            }
        }

        /// <summary>Import Current Settings.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Import_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.ImportFile.Url))
            {
                return;
            }

            var sXmlImport = this.ImportFile.Url;

            this.upOptions.Update();

            // RESET Dialog
            this.ImportFile.Url = null;

            int imageFileId = int.Parse(sXmlImport.Substring(7));

            // FileInfo objFileInfo = objFileController.GetFileById(imageFileId, this.portalSettings.PortalId);
            var objFileInfo = FileManager.Instance.GetFile(imageFileId);

            var homeDirectory = this.HomeDirectory;
            sXmlImport = homeDirectory + objFileInfo.Folder + objFileInfo.FileName;

            try
            {
                this.ImportXmlFile(sXmlImport);

                this.ShowNotification(Localization.GetString("Imported.Text", this.ResXFile, this.LangCode), "success");
            }
            catch (Exception)
            {
                this.ShowNotification(
                    Localization.GetString("BadImportXml.Text", this.ResXFile, this.LangCode), "error");
            }
        }

        /// <summary>Export Current Settings.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Export_Click(object sender, EventArgs e)
        {
            var exportSettings = this.ExportSettings();

            // Save XML file
            try
            {
                var serializer = new XmlSerializer(typeof(EditorProviderSettings));

                var xmlFileName = !string.IsNullOrEmpty(this.ExportFileName.Text.Trim())
                                      ? this.ExportFileName.Text
                                      : $"CKEditorSettings-{exportSettings.SettingMode}.xml";

                if (!xmlFileName.EndsWith(".xml"))
                {
                    xmlFileName += ".xml";
                }

                var exportFolderInfo = FolderManager.Instance.GetFolder(Convert.ToInt32(this.ExportDir.SelectedValue));

                var homeDirectory = this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;
                var textWriter = this.ExportDir.SelectedValue.Equals("-1")
                                     ? new StreamWriter(
                                           Path.Combine(homeDirectory, xmlFileName))
                                     : new StreamWriter(Path.Combine(exportFolderInfo.PhysicalPath, xmlFileName));

                serializer.Serialize(textWriter, exportSettings);

                textWriter.Close();

                this.ShowNotification(Localization.GetString("Export.Text", this.ResXFile, this.LangCode), "success");
            }
            catch (Exception exception)
            {
                this.ShowNotification(exception.Message, "error");
            }

            this.upOptions.Update();
        }

        /// <summary>Adds the Java scripts.</summary>
        private void AddJavaScripts()
        {
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                this.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/jquery.notification.css"));

            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                this.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/Options.css"));

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "jquery.notification",
                this.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/jquery.notification.js"));

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "OptionsJs",
                this.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/Options.js"));
        }

        /// <summary>Imports the XML file.</summary>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <param name="changeMode">if set to <see langword="true"/> [change mode].</param>
        private void ImportXmlFile(string xmlFilePath, bool changeMode = true)
        {
            var serializer = new XmlSerializer(typeof(EditorProviderSettings));

            var textReader = new StreamReader(xmlFilePath);

            var importedSettings = (EditorProviderSettings)serializer.Deserialize(textReader);

            if (!string.IsNullOrEmpty(importedSettings.EditorWidth))
            {
                importedSettings.Config.Width = importedSettings.EditorWidth;
            }

            if (!string.IsNullOrEmpty(importedSettings.EditorHeight))
            {
                importedSettings.Config.Height = importedSettings.EditorHeight;
            }

            textReader.Close();

            this.FillSettings(importedSettings, changeMode);
        }

        /// <summary>Fill file upload size limit controls.</summary>
        /// <param name="imporUploadSizeRoles">A list of <see cref="UploadSizeRoles"/>.</param>
        private void FillFileUploadSettings(List<UploadSizeRoles> imporUploadSizeRoles)
        {
            this.UploadFileLimits.DataSource = imporUploadSizeRoles;
            this.UploadFileLimits.DataBind();

            // Load Upload Size Setting for Each Portal Role
            foreach (var uploadSizeRole in imporUploadSizeRoles)
            {
                if (uploadSizeRole.RoleId.Equals(-1))
                {
                    for (var i = 0; i < this.UploadFileLimits.Rows.Count; i++)
                    {
                        Label label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals(UnauthenticatedUsersRoleName))
                        {
                            continue;
                        }

                        var sizeLimit =
                            (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                        sizeLimit.Text = uploadSizeRole.UploadFileLimit.ToString();
                    }
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleById(this.portalSettings?.PortalId ?? Host.HostPortalID, uploadSizeRole.RoleId);

                    if (objRole == null)
                    {
                        continue;
                    }

                    for (var i = 0; i < this.UploadFileLimits.Rows.Count; i++)
                    {
                        Label label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals(objRole.RoleName))
                        {
                            continue;
                        }

                        var sizeLimit =
                            (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                        sizeLimit.Text = uploadSizeRole.UploadFileLimit.ToString();
                    }
                }
            }
        }

        /// <summary>Fills the setting controls with the loaded Setting Values.</summary>
        /// <param name="importedSettings">The imported settings.</param>
        /// <param name="changeMode">if set to <see langword="true"/> [change mode].</param>
        private void FillSettings(EditorProviderSettings importedSettings, bool changeMode = true)
        {
            // Editor config settings
            foreach (
                PropertyInfo info in
                    SettingsUtil.GetEditorConfigProperties())
            {
                object value = null;

                if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount"))
                {
                    value = info.GetValue(importedSettings.Config, null);

                    if (value == null)
                    {
                        continue;
                    }
                }

                switch (info.PropertyType.Name)
                {
                    case "Decimal":
                    case "Int32":
                    case "String":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                textBox.Text = value?.ToString();
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                checkBox.Checked = (bool)value;
                            }
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ContentsLangDirection":
                    case "EnterMode":
                    case "ShiftEnterMode":
                    case "ToolbarLocation":
                    case "DefaultLinkType":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList != null)
                            {
                                if (dropDownList.Items.FindByValue(value.ToString()) != null)
                                {
                                    dropDownList.ClearSelection();
                                    dropDownList.Items.FindByValue(value.ToString()).Selected = true;
                                }
                            }
                        }

                        break;
                    case "CodeMirror":
                        {
                            foreach (
                           var codeMirrorInfo in
                               typeof(CodeMirror).GetProperties()
                                                 .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                            {
                                value = codeMirrorInfo.GetValue(importedSettings.Config.CodeMirror, null);

                                if (value == null)
                                {
                                    continue;
                                }

                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                textBox.Text = value.ToString();
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                checkBox.Checked = (bool)value;
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                    case "WordCount":
                        {
                            foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                            {
                                value = wordCountInfo.GetValue(importedSettings.Config.WordCount, null);

                                if (value == null)
                                {
                                    continue;
                                }

                                switch (wordCountInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                textBox.Text = value.ToString();
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                checkBox.Checked = (bool)value;
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                }
            }
            ///////////////////

            if (!string.IsNullOrEmpty(importedSettings.Config.Skin)
                && this.ddlSkin.Items.FindByValue(importedSettings.Config.Skin) != null)
            {
                this.ddlSkin.ClearSelection();
                this.ddlSkin.SelectedValue = importedSettings.Config.Skin;
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.CodeMirror.Theme)
                && this.CodeMirrorTheme.Items.FindByValue(importedSettings.Config.CodeMirror.Theme) != null)
            {
                this.CodeMirrorTheme.ClearSelection();
                this.CodeMirrorTheme.SelectedValue = importedSettings.Config.CodeMirror.Theme;
            }

            if (!string.IsNullOrEmpty(importedSettings.Browser)
                && this.ddlBrowser.Items.FindByValue(importedSettings.Browser) != null)
            {
                this.ddlBrowser.ClearSelection();
                this.ddlBrowser.SelectedValue = importedSettings.Browser;
            }

            if (!string.IsNullOrEmpty(importedSettings.ImageButton)
                && this.ddlImageButton.Items.FindByValue(importedSettings.ImageButton) != null)
            {
                this.ddlImageButton.ClearSelection();
                this.ddlImageButton.SelectedValue = importedSettings.ImageButton;
            }

            this.FileListPageSize.Text = importedSettings.FileListPageSize.ToString();

            this.FileListViewMode.SelectedValue = importedSettings.FileListViewMode.ToString();
            this.DefaultLinkMode.SelectedValue = importedSettings.DefaultLinkMode.ToString();
            this.UseAnchorSelector.Checked = importedSettings.UseAnchorSelector;
            this.ShowPageLinksTabFirst.Checked = importedSettings.ShowPageLinksTabFirst;

            this.cbBrowserDirs.Checked = importedSettings.SubDirs;

            this.OverrideFileOnUpload.Checked = importedSettings.OverrideFileOnUpload;

            // get the all-sites settings to be able to to show overridden values
            // when we're not in all-sites mode
            var allPortalsSettings = SettingsUtil.LoadEditorSettingsByKey(
                this.portalSettings, this.currentSettings, EditorController.GetEditorHostSettings(), SettingConstants.HostKey, new List<RoleInfo>());

            this.HostBrowserRootDir.ReadOnly = !this.IsHostMode || this.CurrentPortalOnly;
            this.HostBrowserRootDir.Text = this.HostBrowserRootDir.ReadOnly ? allPortalsSettings.HostBrowserRootDir : importedSettings.HostBrowserRootDir;

            this.BrowserRootDir.SelectedValue =
                 this.BrowserRootDir.Items.FindByValue(importedSettings.BrowserRootDirId.ToString()) != null
                     ? importedSettings.BrowserRootDirId.ToString()
                     : "-1";

            this.HostBrowserRootDirForImg.ReadOnly = !this.IsHostMode || this.CurrentPortalOnly;
            this.HostBrowserRootDirForImg.Text = this.HostBrowserRootDirForImg.ReadOnly ? allPortalsSettings.HostBrowserRootDirForImg : importedSettings.HostBrowserRootDirForImg;

            this.BrowserRootDirForImg.SelectedValue =
                 this.BrowserRootDirForImg.Items.FindByValue(importedSettings.BrowserRootDirForImgId.ToString()) != null
                     ? importedSettings.BrowserRootDirForImgId.ToString()
                     : "-1";

            this.HostUploadDir.ReadOnly = !this.IsHostMode || this.CurrentPortalOnly;
            this.HostUploadDir.Text = this.HostUploadDir.ReadOnly ? allPortalsSettings.HostUploadDir : importedSettings.HostUploadDir;

            this.UploadDir.SelectedValue = this.UploadDir.Items.FindByValue(importedSettings.UploadDirId.ToString())
                                           != null
                                               ? importedSettings.UploadDirId.ToString()
                                               : "-1";

            this.HostUploadDirForImg.ReadOnly = !this.IsHostMode || this.CurrentPortalOnly;
            this.HostUploadDirForImg.Text = this.HostUploadDirForImg.ReadOnly ? allPortalsSettings.HostUploadDirForImg : importedSettings.HostUploadDirForImg;

            this.UploadDirForImg.SelectedValue = this.UploadDirForImg.Items.FindByValue(importedSettings.UploadDirForImgId.ToString())
                                                 != null
                                               ? importedSettings.UploadDirForImgId.ToString()
                                               : "-1";

            var homeDirectory = this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;
            var configFolderInfo =
                Utility.ConvertFilePathToFolderInfo(
                    !string.IsNullOrEmpty(this.configFolder)
                        ? Path.Combine(homeDirectory, this.configFolder)
                        : homeDirectory,
                    this.portalSettings);

            this.ExportDir.SelectedValue = configFolderInfo != null
                                           &&
                                           this.ExportDir.Items.FindByValue(configFolderInfo.FolderID.ToString())
                                           != null
                                               ? configFolderInfo.FolderID.ToString()
                                               : "-1";

            this.ExportFileName.Text = $"CKEditorSettings-{importedSettings.SettingMode}.xml";

            switch (importedSettings.SettingMode)
            {
                case SettingsMode.Host:
                    this.ExportFileName.Text = "CKEditorSettings-Host.xml";
                    break;
                case SettingsMode.Portal:
                    this.ExportFileName.Text = $"CKEditorSettings-{importedSettings.SettingMode}-{this.portalSettings?.PortalId ?? Null.NullInteger}.xml";
                    break;
                case SettingsMode.Page:
                    this.ExportFileName.Text = $"CKEditorSettings-{importedSettings.SettingMode}-{this.CurrentOrSelectedTabId}.xml";
                    break;
                case SettingsMode.ModuleInstance:
                    this.ExportFileName.Text = $"CKEditorSettings-{importedSettings.SettingMode}-{this.ModuleId}.xml";
                    break;
            }

            this.txtResizeWidthUpload.Text = importedSettings.ResizeWidthUpload.ToString();

            this.txtResizeHeightUpload.Text = importedSettings.ResizeHeightUpload.ToString();

            this.txtResizeWidth.Text = importedSettings.ResizeWidth.ToString();

            this.txtResizeHeight.Text = importedSettings.ResizeHeight.ToString();

            this.InjectSyntaxJs.Checked = importedSettings.InjectSyntaxJs;

            if (Utility.IsUnit(importedSettings.Config.Width))
            {
                this.txtWidth.Text = importedSettings.Config.Width;
            }

            if (Utility.IsUnit(importedSettings.Config.Height))
            {
                this.txtHeight.Text = importedSettings.Config.Height;
            }

            this.txtBlanktext.Text = Convert.ToString(importedSettings.BlankText);

            this.FillFileUploadSettings(importedSettings.UploadSizeRoles);

            if (!string.IsNullOrEmpty(importedSettings.Config.ContentsCss))
            {
                this.CssUrl.Url = this.ReFormatURL(importedSettings.Config.ContentsCss);
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.Templates_Files))
            {
                this.TemplUrl.Url = this.ReFormatURL(importedSettings.Config.Templates_Files);
            }

            if (!string.IsNullOrEmpty(importedSettings.CustomJsFile))
            {
                this.CustomJsFile.Url = this.ReFormatURL(importedSettings.CustomJsFile);
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.CustomConfig))
            {
                this.ConfigUrl.Url = this.ReFormatURL(importedSettings.Config.CustomConfig);
            }

            this.BrowAllowFollowPerms.Checked = importedSettings.BrowserAllowFollowFolderPerms;

            if (!string.IsNullOrEmpty(importedSettings.BrowserRoles))
            {
                string sRoles = importedSettings.BrowserRoles;

                if (sRoles.Length >= 1 && sRoles.Contains(";"))
                {
                    string[] roles = sRoles.Split(';');

                    foreach (string sRoleName in roles)
                    {
                        if (Utility.IsNumeric(sRoleName))
                        {
                            if (this.chblBrowsGr.Items.FindByValue(sRoleName) != null)
                            {
                                this.chblBrowsGr.Items.FindByValue(sRoleName).Selected = true;
                            }
                        }
                        else
                        {
                            if (this.chblBrowsGr.Items.FindByText(sRoleName) != null)
                            {
                                this.chblBrowsGr.Items.FindByText(sRoleName).Selected = true;
                            }
                        }
                    }
                }
            }

            if (!changeMode)
            {
                return;
            }

            switch (importedSettings.SettingMode)
            {
                case SettingsMode.Portal:
                    this.rBlSetMode.SelectedIndex = 0;
                    break;
                case SettingsMode.Page:
                    this.rBlSetMode.SelectedIndex = 1;
                    break;
                case SettingsMode.ModuleInstance:
                    this.rBlSetMode.SelectedIndex = 2;
                    break;
            }
        }

        /// <summary>Bind User Groups to GridView.</summary>
        private void BindUserGroupsGridView()
        {
            var lic = new ListItemCollection();

            var portalId = this.portalSettings?.PortalId != Null.NullInteger ? this.portalSettings?.PortalId ?? Host.HostPortalID : Host.HostPortalID;
            foreach (var roleItem in
                from RoleInfo objRole in RoleController.Instance.GetRoles(portalId)
                select new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() })
            {
                lic.Add(roleItem);
            }

            lic.Add(new ListItem { Text = UnauthenticatedUsersRoleName, Value = "-1" });

            this.gvToolbars.DataSource = lic;
            this.gvToolbars.DataBind();

            this.InsertToolbars();

            var lblRole = (Label)this.gvToolbars.HeaderRow.FindControl("lblRole");
            var lblSelToolb = (Label)this.gvToolbars.HeaderRow.FindControl("lblSelToolb");

            lblRole.Text = Localization.GetString("lblRole.Text", this.ResXFile, this.LangCode);
            lblSelToolb.Text = Localization.GetString("lblSelToolb.Text", this.ResXFile, this.LangCode);
        }

        /// <summary>Delete Settings only for this Module Instance.</summary>
        private void DelModuleSettings()
        {
            this.moduleInstanceName = this.request.QueryString["minc"];
            string moduleKey = $"DNNCKMI#{this.ModuleId}#INS#{this.moduleInstanceName}#";

            var moduleController = new ModuleController();

            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
            {
                moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{info.Name}");
            }

            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.SKIN}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.CODEMIRRORTHEME}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.BROWSER}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.FILELISTPAGESIZE}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.FILELISTVIEWMODE}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.DEFAULTLINKMODE}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.USEANCHORSELECTOR}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.SHOWPAGELINKSTABFIRST}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.OVERRIDEFILEONUPLOAD}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.SUBDIRS}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.BROWSERROOTDIRID}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.BROWSERROOTDIRFORIMGID}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.UPLOADDIRID}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.UPLOADDIRFORIMGID}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.INJECTJS}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.WIDTH}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.HEIGHT}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.BLANKTEXT}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.CSS}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.TEMPLATEFILES}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.CUSTOMJSFILE}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.CONFIG}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.ROLES}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.RESIZEHEIGHTUPLOAD}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.RESIZEWIDTHUPLOAD}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.RESIZEHEIGHT}");
            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{SettingConstants.RESIZEWIDTH}");

            foreach (var objRole in RoleController.Instance.GetRoles(this.portalSettings?.PortalId ?? Host.HostPortalID))
            {
                moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}{objRole.RoleID}#{SettingConstants.TOOLB}");
            }

            moduleController.DeleteModuleSetting(this.ModuleId, $"{moduleKey}-1#{SettingConstants.TOOLB}");

            // Finally Clear Cache
            EditorController.ClearEditorCache();
        }

        /// <summary>Write Information.</summary>
        private void FillInformations()
        {
            var ckEditorPackage = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.Name.Equals("DotNetNuke.CKHtmlEditorProvider", StringComparison.OrdinalIgnoreCase));

            if (ckEditorPackage != null)
            {
                this.ProviderVersion.Text += ckEditorPackage.Version;
            }

            this.lblPortal.Text += !this.IsAllInstances ? this.portalSettings.PortalName : "Host";

            ModuleDefinitionInfo moduleDefinitionInfo = null;
            var portalId = this.portalSettings?.PortalId ?? Host.HostPortalID;
            var moduleInfo = new ModuleController().GetModuleByDefinition(
                portalId, "User Accounts");

            if (this.CurrentModule != null)
            {
                moduleDefinitionInfo =
                    ModuleDefinitionController.GetModuleDefinitionByID(this.CurrentModule.ModuleDefID);
            }

            if (moduleDefinitionInfo == null)
            {
                moduleDefinitionInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "User Accounts", moduleInfo.DesktopModuleID);
            }

            try
            {
                this.lblPage.Text += string.Format(
                    "{0} - TabID {1}",
                    new TabController().GetTab(this.CurrentOrSelectedTabId, portalId, false).TabName,
                    this.CurrentOrSelectedTabId);
            }
            catch (Exception)
            {
                this.lblPage.Text = string.Empty;
            }

            if (moduleDefinitionInfo != null)
            {
                this.lblModType.Text += moduleDefinitionInfo.FriendlyName;
                if (!this.IsHostMode && moduleDefinitionInfo.FriendlyName.Equals("User Accounts"))
                {
                    this.rBlSetMode.Items.RemoveAt(2);
                }
            }
            else
            {
                this.lblModType.Text = string.Empty;
            }

            this.lblModName.Text += this.CurrentModule?.ModuleTitle ?? moduleInfo.ModuleTitle;

            if (this.request.QueryString["minc"] != null)
            {
                this.lblModInst.Text += this.request.QueryString["minc"];
                this.moduleInstanceName = this.request.QueryString["minc"];
            }

            if (this.UserInfo != null)
            {
                this.lblUName.Text += this.UserInfo.Username;
            }
            else
            {
                this.lblUName.Text = string.Empty;
            }
        }

        /// <summary>Loads all DNN Roles.</summary>
        private void FillRoles()
        {
            this.chblBrowsGr.Items.Clear();

            var portalId = this.portalSettings?.PortalId ?? Host.HostPortalID;

            foreach (var objRole in this.GetRoles(portalId))
            {
                var isAdmin = objRole.RoleName.Equals(this.PortalSettings.AdministratorRoleName);
                var roleItem = new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() };
                roleItem.Selected = isAdmin || this.GetActiveRolesIds().Contains(objRole.RoleID);
                roleItem.Enabled = !isAdmin;

                this.chblBrowsGr.Items.Add(roleItem);
            }
        }

        /// <summary>Gets a sequence of currently active file browser roles.</summary>
        /// <returns>A sequence of role IDs.</returns>
        private IEnumerable<int> GetActiveRolesIds()
        {
            if (this.currentSettings?.BrowserRoles != null)
            {
                var rolesIds = this.currentSettings.BrowserRoles.Split(';');
                foreach (string roleId in rolesIds)
                {
                    int actualRoleId;

                    if (!int.TryParse(roleId, out actualRoleId))
                    {
                        var role = RoleController.Instance.GetRoleByName(this.PortalId, roleId);
                        actualRoleId = role != null ? role.RoleID : -1;
                    }

                    yield return actualRoleId;
                }
            }
            else
            {
                yield break;
            }
        }

        /// <summary>Gets the roles.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A sequence of <see cref="RoleInfo"/> instances.</returns>
        private IEnumerable<RoleInfo> GetRoles(int portalId)
        {
            portalId = portalId == Null.NullInteger ? Host.HostPortalID : portalId;
            var roles = RoleController.Instance.GetRoles(portalId);

            if (!this.IsAllInstances)
            {
                return roles;
            }

            return (from role in roles
                    let isCommon = PortalController.Instance.GetPortals().Cast<PortalInfo>().All(portal => RoleController.Instance.GetRoles(portal.PortalID).Any(r => r.RoleName == role.RoleName))
                    where isCommon
                    select role).ToList();
        }

        // Reload Settings based on the Selected Mode

        /// <summary>Loads the List of available Skins.</summary>
        private void FillSkinList()
        {
            this.ddlSkin.Items.Clear();

            DirectoryInfo objDir = new DirectoryInfo(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/skins");

            foreach (ListItem skinItem in
                objDir.GetDirectories().Select(
                    objSubFolder => new ListItem { Text = objSubFolder.Name, Value = objSubFolder.Name }))
            {
                this.ddlSkin.Items.Add(skinItem);
            }

            this.ddlSkin.SelectedValue = this.currentSettings?.Config?.Skin ?? string.Empty;

            // CodeMirror Themes
            this.CodeMirrorTheme.Items.Clear();

            if (Directory.Exists(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/plugins/codemirror/theme"))
            {
                var themesFolder = new DirectoryInfo(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.18.0/plugins/codemirror/theme");

                // add default theme
                this.CodeMirrorTheme.Items.Add(new ListItem { Text = "default", Value = "default" });

                foreach (
                    var skinItem in
                        themesFolder.GetFiles("*.css").Select(
                            themeCssFile =>
                            themeCssFile.Name.Replace(themeCssFile.Extension, string.Empty)).Select(
                                themeName => new ListItem { Text = themeName, Value = themeName }))
                {
                    this.CodeMirrorTheme.Items.Add(skinItem);
                }
            }
        }

        /// <summary>Loads the List of available Folders.</summary>
        private void FillFolders()
        {
            this.UploadDir.Items.Clear();
            this.UploadDirForImg.Items.Clear();
            this.BrowserRootDir.Items.Clear();
            this.BrowserRootDirForImg.Items.Clear();
            this.ExportDir.Items.Clear();

            var portalId = this.portalSettings?.PortalId ?? Null.NullInteger;
            foreach (var folder in FolderManager.Instance.GetFolders(portalId))
            {
                string text, value;

                if (folder.FolderPath == Null.NullString)
                {
                    text = "Standard";
                    value = "-1";
                }
                else
                {
                    text = folder.FolderPath;
                    value = folder.FolderID.ToString();
                }

                if (!FolderPermissionController.CanViewFolder(folder as FolderInfo))
                {
                    continue;
                }

                this.UploadDir.Items.Add(new ListItem(text, value));
                this.UploadDirForImg.Items.Add(new ListItem(text, value));
                this.BrowserRootDir.Items.Add(new ListItem(text, value));
                this.BrowserRootDirForImg.Items.Add(new ListItem(text, value));
                this.ExportDir.Items.Add(new ListItem(text, value));
            }

            this.UploadDir.SelectedValue = "-1";
            this.UploadDirForImg.SelectedValue = "-1";
            this.BrowserRootDir.SelectedValue = "-1";
            this.BrowserRootDirForImg.SelectedValue = "-1";
            this.ExportDir.SelectedValue = "-1";
        }

        /// <summary>Gets the portal settings.</summary>
        /// <returns>
        /// Returns the Current Portal Settings.
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            PortalSettings portalSettings;

            try
            {
                if (this.IsHostMode && this.CurrentPortalOnly)
                {
                    return this.PortalSettings;
                }

                if (this.IsHostMode)
                {
                    return new PortalSettings(this.CurrentOrSelectedPortalId);
                }

                if (!this.IsHostMode)
                {
                    var tabIdString = this.request.QueryString["tid"];
                    this.CurrentOrSelectedTabId = !string.IsNullOrWhiteSpace(tabIdString) ? int.Parse(tabIdString) : this.PortalSettings.ActiveTab.TabID;
                }

                if (!this.IsHostMode && this.request.QueryString["PortalID"] != null)
                {
                    this.CurrentOrSelectedPortalId = int.Parse(this.request.QueryString["PortalID"]);
                }

                var domainName = Globals.GetDomainName(this.Request, true);

                var portalAlias = PortalAliasController.GetPortalAliasByPortal(this.CurrentOrSelectedPortalId, domainName);

                portalSettings = new PortalSettings(this.CurrentOrSelectedTabId, PortalAliasController.Instance.GetPortalAlias(portalAlias));
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>Hide Add Toolbar Button if all Priorities are used.</summary>
        private void HideAddToolbar()
        {
            var bHideAll = !this.dDlToolbarPrio.Items.Cast<ListItem>().Any(item => item.Enabled);

            if (bHideAll)
            {
                this.iBAdd.Visible = false;
            }
        }

        /// <summary>Add new/Save Toolbar Set.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbAddClick(object sender, ImageClickEventArgs e)
        {
            if (string.IsNullOrEmpty(this.dnnTxtToolBName.Text))
            {
                this.ShowNotification(Localization.GetString("ToolbarNameMissing.Text", this.ResXFile, this.LangCode), "error");

                return;
            }

            if (string.IsNullOrEmpty(this.ToolbarSet.Value))
            {
                return;
            }

            var modifiedSet = ToolbarUtil.ConvertStringToToolbarSet(this.ToolbarSet.Value);

            // Save modified Toolbar Set
            if (this.iBAdd.ImageUrl.Contains("save.gif"))
            {
                var toolbarEdit = this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dnnTxtToolBName.Text));

                toolbarEdit.ToolbarGroups = modifiedSet.ToolbarGroups;
                toolbarEdit.Priority = int.Parse(this.dDlToolbarPrio.SelectedValue);

                var homeDirectory = this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;
                var homeDirPath = !string.IsNullOrEmpty(this.configFolder)
                                      ? Path.Combine(homeDirectory, this.configFolder)
                                      : homeDirectory;
                ToolbarUtil.SaveToolbarSets(this.listToolbars, homeDirPath);

                this.ShowNotification(
                    Localization.GetString("ToolbarSetSaved.Text", this.ResXFile, this.LangCode),
                    "success");
            }
            else
            {
                // Add New Toolbar Set
                var newToolbar = new ToolbarSet(this.dnnTxtToolBName.Text, int.Parse(this.dDlToolbarPrio.SelectedValue))
                {
                    ToolbarGroups = modifiedSet.ToolbarGroups,
                };

                this.listToolbars.Add(newToolbar);
                var homeDirPath = !string.IsNullOrEmpty(this.configFolder)
                                      ? Path.Combine(this.HomeDirectory, this.configFolder)
                                      : this.HomeDirectory;
                ToolbarUtil.SaveToolbarSets(this.listToolbars, homeDirPath);

                this.ShowNotification(
                    string.Format(
                        Localization.GetString("ToolbarSetCreated.Text", this.ResXFile, this.LangCode),
                        this.dnnTxtToolBName.Text),
                    "success");
            }

            // Hide Priority
            this.dDlToolbarPrio.SelectedItem.Enabled = false;

            this.BindUserGroupsGridView();

            this.dnnTxtToolBName.Text = string.Empty;
            this.ToolbarSet.Value = string.Empty;

            List<string> excludeButtons;

            var toolbarSet = new ToolbarSet();

            // Empty Toolbar
            toolbarSet.ToolbarGroups.Add(
                new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) });

            this.FillToolbarGroupsRepeater(toolbarSet, out excludeButtons);

            this.FillAvailableToolbarButtons(null);

            this.dnnTxtToolBName.Enabled = true;

            this.iBAdd.ImageUrl = this.ResolveUrl("~/images/add.gif");

            this.iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);
            this.iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);

            this.iBCancel.Visible = false;

            this.HideAddToolbar();
        }

        /// <summary>Cancel Edit Toolbar.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbCancelClick(object sender, ImageClickEventArgs e)
        {
            this.dnnTxtToolBName.Text = string.Empty;
            this.ToolbarSet.Value = string.Empty;

            List<string> excludeButtons;

            var toolbarSet = new ToolbarSet();

            // Empty Toolbar
            toolbarSet.ToolbarGroups.Add(
                new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) });

            this.FillToolbarGroupsRepeater(toolbarSet, out excludeButtons);

            this.FillAvailableToolbarButtons(null);

            this.dnnTxtToolBName.Enabled = true;

            this.dDlToolbarPrio.Items.FindByText(
                this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dDlCustomToolbars.SelectedValue)).Priority.ToString()).Enabled = false;

            this.iBAdd.ImageUrl = this.ResolveUrl("~/images/add.gif");

            this.iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);
            this.iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);

            this.iBCancel.Visible = false;

            this.HideAddToolbar();
        }

        /// <summary>Delete Selected Toolbar Set.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbDeleteClick(object sender, ImageClickEventArgs e)
        {
            if (this.dDlCustomToolbars.SelectedValue == null)
            {
                return;
            }

            var toolbarDelete =
                 this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dDlCustomToolbars.SelectedValue));

            var priority = toolbarDelete.Priority.ToString();

            if (priority.Length.Equals(1))
            {
                priority = string.Format("0{0}", priority);
            }

            this.dDlToolbarPrio.Items.FindByText(priority).Enabled = true;

            this.listToolbars.RemoveAll(toolbarSel => toolbarSel.Name.Equals(this.dDlCustomToolbars.SelectedValue));

            var homeDirPath = !string.IsNullOrEmpty(this.configFolder)
                                  ? Path.Combine(this.HomeDirectory, this.configFolder)
                                  : this.HomeDirectory;
            ToolbarUtil.SaveToolbarSets(this.listToolbars, homeDirPath);

            this.BindUserGroupsGridView();

            this.dnnTxtToolBName.Enabled = true;

            this.iBAdd.ImageUrl = this.ResolveUrl("~/images/add.gif");

            this.iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);
            this.iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);

            this.ShowNotification(
                Localization.GetString("ToolbarSetDeleted.Text", this.ResXFile, this.LangCode),
                "success");
        }

        /// <summary>Edit Selected Toolbar.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbEditClick(object sender, ImageClickEventArgs e)
        {
            if (this.dDlCustomToolbars.SelectedValue == null)
            {
                return;
            }

            this.iBAdd.Visible = true;

            var toolbarEdit = this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dDlCustomToolbars.SelectedValue));

            if (toolbarEdit != null)
            {
                this.dnnTxtToolBName.Text = toolbarEdit.Name;
                this.ToolbarSet.Value = ToolbarUtil.ConvertToolbarSetToString(toolbarEdit);

                List<string> excludeButtons;

                this.FillToolbarGroupsRepeater(toolbarEdit, out excludeButtons);

                this.FillAvailableToolbarButtons(excludeButtons);

                var priority = toolbarEdit.Priority.ToString();

                if (priority.Length.Equals(1))
                {
                    priority = $"0{priority}";
                }

                this.dDlToolbarPrio.Items.FindByText(priority).Enabled = true;
                this.dDlToolbarPrio.SelectedValue = priority;

                this.dnnTxtToolBName.Enabled = false;

                this.iBAdd.ImageUrl = this.ResolveUrl("~/images/save.gif");

                this.iBAdd.AlternateText = Localization.GetString("SaveToolbar.Text", this.ResXFile, this.LangCode);
                this.iBAdd.ToolTip = Localization.GetString("SaveToolbar.Text", this.ResXFile, this.LangCode);

                this.iBCancel.Visible = true;
            }

            this.HideAddToolbar();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            try
            {
                this.btnOk.Click += this.OK_Click;

                this.lnkRemove.Click += this.Remove_Click;
                this.lnkRemoveAll.Click += this.RemoveAll_Click;
                this.lnkRemoveChild.Click += this.RemoveChild_Click;
                this.CopyToAllChild.Click += this.CopyToAllChild_Click;

                this.iBAdd.Click += this.IbAddClick;
                this.iBCancel.Click += this.IbCancelClick;
                this.iBEdit.Click += this.IbEditClick;
                this.iBDelete.Click += this.IbDeleteClick;

                var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
                var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

                if (!string.IsNullOrEmpty(objProvider?.Attributes["ck_configFolder"]))
                {
                    this.configFolder = objProvider.Attributes["ck_configFolder"];
                }

                this.listToolbars = ToolbarUtil.GetToolbars(
                    this.HomeDirectory, this.configFolder);

                this.rBlSetMode.SelectedIndexChanged += this.SetMode_SelectedIndexChanged;

                this.ToolbarGroupsRepeater.ItemDataBound += this.ToolbarGroupsRepeater_ItemDataBound;
                this.gvToolbars.RowDataBound += this.On_gvToolbars_RowDataBound;

                this.RenderEditorConfigSettings();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, "error");
            }
        }

        private void On_gvToolbars_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            var licToolbars = new ListItemCollection();

            foreach (var toolbarItem in this.listToolbars.Select(toolbarSet => new ListItem { Text = toolbarSet.Name, Value = toolbarSet.Name }))
            {
                licToolbars.Add(toolbarItem);
            }

            var ddLToolB = (DropDownList)e.Row.FindControl("ddlToolbars");

            if (ddLToolB == null)
            {
                return;
            }

            ddLToolB.DataSource = licToolbars;
            ddLToolB.DataBind();

            var label = (Label)e.Row.Cells[0].FindControl("lblRoleName");

            if (label == null)
            {
                return;
            }

            var portalId = this.portalSettings?.PortalId != Null.NullInteger ? this.portalSettings?.PortalId ?? Host.HostPortalID : Host.HostPortalID;
            var objRole = RoleController.Instance.GetRoleByName(portalId, label.Text);

            if (objRole == null && label.Text != UnauthenticatedUsersRoleName)
            {
                return;
            }

            if (this.currentSettings == null)
            {
                var settingsDictionary = EditorController.GetEditorHostSettings();
                var pageKey = $"DNNCKT#{this.CurrentOrSelectedTabId}#";
                this.LoadSettings(SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey) ? 1 : 0);
            }

            var objRoleId = label.Text != UnauthenticatedUsersRoleName ? objRole.RoleID : -1;
            var currentToolbarSettings = this.currentSettings.ToolBarRoles.FirstOrDefault(o => o.RoleId == objRoleId);

            if (currentToolbarSettings != null)
            {
                ddLToolB.ClearSelection();

                if (ddLToolB.Items.FindByValue(currentToolbarSettings.Toolbar) != null)
                {
                    ddLToolB.SelectedValue = currentToolbarSettings.Toolbar;
                }
            }
        }

        /// <summary>Handles the ItemDataBound event of the ToolbarGroupsRepeater control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        private void ToolbarGroupsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            var groupListItem = (HiddenField)e.Item.FindControl("GroupListItem");
            var toolbarButtonsRepeater = (Repeater)e.Item.FindControl("ToolbarButtonsRepeater");

            if (toolbarButtonsRepeater == null && groupListItem == null)
            {
                return;
            }

            var groupName = groupListItem.Value;

            var toolbarButtonsTable = new DataTable();

            toolbarButtonsTable.Columns.Add(new DataColumn("Icon", typeof(string)));
            toolbarButtonsTable.Columns.Add(new DataColumn("Button", typeof(string)));

            if (this.allListButtons == null)
            {
                this.allListButtons =
                    ToolbarUtil.LoadToolBarButtons(
                        !string.IsNullOrEmpty(this.configFolder)
                            ? Path.Combine(this.HomeDirectory, this.configFolder)
                            : this.HomeDirectory);
            }

            foreach (var button in this.toolbarSets.ToolbarGroups.Where(@group => @group.name.Equals(groupName)).SelectMany(@group => @group.items))
            {
                if (this.allListButtons.Find(availButton => availButton.ToolbarName.Equals(button)) == null
                    && !button.Equals("/"))
                {
                    continue;
                }

                var groupRow = toolbarButtonsTable.NewRow();

                groupRow["Button"] = button;

                var buttonItem = this.allListButtons.Find(b => b.ToolbarName.Equals(button));

                groupRow["Icon"] = buttonItem != null ? buttonItem.ToolbarIcon : button;

                toolbarButtonsTable.Rows.Add(groupRow);
            }

            toolbarButtonsRepeater.DataSource = toolbarButtonsTable;
            toolbarButtonsRepeater.DataBind();
        }

        /// <summary>Insert Toolbar Names from Serialized Xml File.</summary>
        private void InsertToolbars()
        {
            ListItemCollection licToolbars = new ListItemCollection();

            foreach (var toolbarSet in this.listToolbars)
            {
                var toolbarItem = new ListItem { Text = toolbarSet.Name, Value = toolbarSet.Name };

                licToolbars.Add(toolbarItem);

                // Exclude used Prioritys from the DropDown
                if (this.dDlToolbarPrio.Items.FindByText(toolbarSet.Priority.ToString()) != null)
                {
                    this.dDlToolbarPrio.Items.FindByText(toolbarSet.Priority.ToString()).Enabled = false;
                }
            }

            this.HideAddToolbar();

            this.dDlCustomToolbars.DataSource = licToolbars;
            this.dDlCustomToolbars.DataBind();

            List<string> excludeButtons;

            var emptyToolbarSet = new ToolbarSet();

            // Empty Toolbar
            emptyToolbarSet.ToolbarGroups.Add(new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) });

            this.FillToolbarGroupsRepeater(emptyToolbarSet, out excludeButtons);

            // Load Toolbar Buttons
            this.FillAvailableToolbarButtons(null);
        }

        /// <summary>Fills the toolbar groups repeater.</summary>
        /// <param name="toolbarSet">The toolbar set.</param>
        /// <param name="excludeButtons">The exclude buttons list.</param>
        private void FillToolbarGroupsRepeater(ToolbarSet toolbarSet, out List<string> excludeButtons)
        {
            excludeButtons = new List<string>();

            var toolbarGroupsTable = new DataTable();

            toolbarGroupsTable.Columns.Add(new DataColumn("GroupName", typeof(string)));

            this.toolbarSets = toolbarSet;

            foreach (var group in this.toolbarSets.ToolbarGroups)
            {
                var groupRow = toolbarGroupsTable.NewRow();

                groupRow["GroupName"] = group.name;

                toolbarGroupsTable.Rows.Add(groupRow);

                // exclude existing buttons in the available list
                excludeButtons.AddRange(@group.items);
            }

            this.ToolbarGroupsRepeater.DataSource = toolbarGroupsTable;
            this.ToolbarGroupsRepeater.DataBind();
        }

        /// <summary>Fills the available toolbar buttons.</summary>
        /// <param name="excludeItems">The exclude items.</param>
        private void FillAvailableToolbarButtons(ICollection<string> excludeItems)
        {
            var toolbarButtonsTable = new DataTable();

            toolbarButtonsTable.Columns.Add(new DataColumn("Button", typeof(string)));
            toolbarButtonsTable.Columns.Add(new DataColumn("Icon", typeof(string)));

            if (this.listButtons == null)
            {
                this.listButtons =
                    ToolbarUtil.LoadToolBarButtons(
                        !string.IsNullOrEmpty(this.configFolder)
                            ? Path.Combine(this.HomeDirectory, this.configFolder)
                            : this.HomeDirectory);
            }

            var buttons = this.listButtons;

            if (excludeItems != null && excludeItems.Count > 0)
            {
                foreach (var excludeItem in excludeItems.Where(excludeItem => !excludeItem.Equals("-")))
                {
                    buttons.RemoveAll(button => button.ToolbarName.Equals(excludeItem));
                }
            }

            foreach (var button in buttons)
            {
                var buttonRow = toolbarButtonsTable.NewRow();

                buttonRow["Button"] = button.ToolbarName;
                buttonRow["Icon"] = button.ToolbarIcon;

                toolbarButtonsTable.Rows.Add(buttonRow);
            }

            this.AvailableToolbarButtons.DataSource = toolbarButtonsTable;
            this.AvailableToolbarButtons.DataBind();
        }

        /// <summary>Load Default Host Settings from 'web.config'.</summary>
        private void LoadDefaultSettings()
        {
            var ckeditorProvider = (Provider)this.provConfig.Providers[this.provConfig.DefaultProvider];

            if (ckeditorProvider == null)
            {
                return;
            }

            // Skin
            if (ckeditorProvider.Attributes["ck_skin"] != string.Empty
                && this.ddlSkin.Items.FindByValue(ckeditorProvider.Attributes["ck_skin"]) != null)
            {
                this.ddlSkin.ClearSelection();

                this.ddlSkin.SelectedValue = ckeditorProvider.Attributes["ck_skin"];
            }

            // FileBrowser
            if (ckeditorProvider.Attributes["ck_Browser"] != string.Empty)
            {
                this.ddlBrowser.SelectedValue = ckeditorProvider.Attributes["ck_Browser"];
            }

            // ImageButton
            if (ckeditorProvider.Attributes["ck_ImageButton"] != string.Empty)
            {
                this.ddlImageButton.SelectedValue = ckeditorProvider.Attributes["ck_ImageButton"];
            }

            if (ckeditorProvider.Attributes["ck_contentsCss"] != string.Empty)
            {
                this.CssUrl.Url = ckeditorProvider.Attributes["ck_contentsCss"];
            }

            if (ckeditorProvider.Attributes["ck_templates_files"] != string.Empty)
            {
                this.TemplUrl.Url = ckeditorProvider.Attributes["ck_templates_files"];
            }

            if (ckeditorProvider.Attributes["ck_customConfig"] != string.Empty)
            {
                this.ConfigUrl.Url = ckeditorProvider.Attributes["ck_customConfig"];
            }
        }

        /// <summary>Load All Editor Settings.</summary>
        /// <param name="currentMode">The current mode.</param>
        /// <param name="changeMode">if set to <see langword="true"/> [change mode].</param>
        private void LoadSettings(int currentMode, bool changeMode = true)
        {
            this.moduleInstanceName = this.request.QueryString["minc"];
            this.CurrentSettingsMode = (SettingsMode)Enum.Parse(typeof(SettingsMode), currentMode.ToString());

            this.lnkRemoveAll.Visible = !currentMode.Equals(0);
            this.lnkRemoveChild.Visible = !currentMode.Equals(0);
            this.CopyToAllChild.Visible = !currentMode.Equals(0);

            var modeText = currentMode != Null.NullInteger ? this.rBlSetMode.Items[currentMode].Text : "Host";
            this.lnkRemove.Text = string.Format(
                Localization.GetString("Remove.Text", this.ResXFile, this.LangCode),
                modeText);
            this.lnkRemoveAll.Text =
                string.Format(
                    Localization.GetString("RemoveAll.Text", this.ResXFile, this.LangCode),
                    modeText);

            this.lnkRemove.ToolTip = string.Format(
                Localization.GetString("Remove.Help", this.ResXFile, this.LangCode),
                modeText);
            this.lnkRemoveAll.ToolTip =
                string.Format(
                    Localization.GetString("RemoveAll.Help", this.ResXFile, this.LangCode),
                    modeText);

            this.LoadDefaultSettings();

            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(this.portalSettings?.PortalId ?? Host.HostPortalID);

            var hostKey = SettingConstants.HostKey;
            var portalKey = SettingConstants.PortalKey(this.portalSettings?.PortalId ?? Host.HostPortalID);
            var pageKey = $"DNNCKT#{this.CurrentOrSelectedTabId}#";
            var moduleKey = $"DNNCKMI#{this.ModuleId}#INS#{this.moduleInstanceName}#";

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var homeDirectory = this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;

            this.currentSettings = SettingsUtil.GetDefaultSettings(
                this.portalSettings,
                homeDirectory,
                objProvider.Attributes["ck_configFolder"],
                portalRoles);

            this.currentSettings.UploadSizeRoles = this.GetDefaultUploadFileSettings(portalRoles);

            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Host:
                    {
                        // Load Host Settings ?!
                        if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, hostKey))
                        {
                            this.currentSettings = new EditorProviderSettings();

                            this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                                this.portalSettings,
                                this.currentSettings,
                                settingsDictionary,
                                hostKey,
                                portalRoles);

                            this.currentSettings.SettingMode = SettingsMode.Host;

                            this.lnkRemove.Enabled = true;
                        }
                        else
                        {
                            this.lnkRemove.Enabled = false;
                        }
                    }

                    break;
                case SettingsMode.Portal:
                    {
                        // Load Portal Settings ?!
                        if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, portalKey))
                        {
                            this.currentSettings = new EditorProviderSettings();

                            this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                                this.portalSettings, this.currentSettings, settingsDictionary, portalKey, portalRoles);

                            // check if UploadSizeLimits have been set
                            if (this.currentSettings.UploadSizeRoles == null || this.currentSettings.UploadSizeRoles.Count == 0)
                            {
                                this.currentSettings.UploadSizeRoles = this.GetDefaultUploadFileSettings(portalRoles);
                            }

                            // Set Current Mode to Portal
                            this.currentSettings.SettingMode = SettingsMode.Portal;

                            this.lnkRemove.Enabled = true;
                        }
                        else
                        {
                            this.lnkRemove.Enabled = false;
                        }
                    }

                    break;
                case SettingsMode.Page:
                    {
                        // Load Page Settings ?!
                        if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey))
                        {
                            this.currentSettings = new EditorProviderSettings();

                            this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                                this.portalSettings, this.currentSettings, settingsDictionary, pageKey, portalRoles);

                            // Set Current Mode to Page
                            this.currentSettings.SettingMode = SettingsMode.Page;

                            this.lnkRemove.Enabled = true;
                        }
                        else
                        {
                            this.lnkRemove.Enabled = false;
                        }

                        var currentTab = new TabController().GetTab(
                            this.CurrentOrSelectedTabId, this.portalSettings?.PortalId ?? Host.HostPortalID, false);

                        this.lnkRemoveChild.Enabled = currentTab.HasChildren;

                        this.lnkRemoveChild.Text = Localization.GetString(
                                "RemovePageChild.Text", this.ResXFile, this.LangCode);
                        this.lnkRemoveChild.ToolTip = Localization.GetString(
                            "RemovePageChild.Help", this.ResXFile, this.LangCode);

                        this.CopyToAllChild.Enabled = currentTab.HasChildren;

                        this.CopyToAllChild.Text = Localization.GetString(
                                "CopyPageChild.Text", this.ResXFile, this.LangCode);
                        this.CopyToAllChild.ToolTip = Localization.GetString(
                            "CopyPageChild.Help", this.ResXFile, this.LangCode);
                    }

                    break;
                case SettingsMode.ModuleInstance:
                    {
                        // Load Module Settings ?!
                        if (SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.ModuleId))
                        {
                            this.currentSettings = new EditorProviderSettings();

                            this.currentSettings = SettingsUtil.LoadModuleSettings(
                                this.portalSettings, this.currentSettings, moduleKey, this.ModuleId, portalRoles);

                            this.currentSettings.SettingMode = SettingsMode.ModuleInstance;

                            this.lnkRemove.Enabled = true;
                        }
                        else
                        {
                            this.lnkRemove.Enabled = false;
                        }

                        this.lnkRemoveChild.Enabled = true;

                        this.lnkRemoveChild.Text = Localization.GetString(
                            "RemoveModuleChild.Text", this.ResXFile, this.LangCode);
                        this.lnkRemoveChild.ToolTip = Localization.GetString(
                            "RemoveModuleChild.Help", this.ResXFile, this.LangCode);
                    }

                    break;
            }

            if (this.currentSettings != null)
            {
                this.FillSettings(this.currentSettings, changeMode);
            }
        }

        /// <summary>Re-Formats Url from the Url Control.</summary>
        /// <param name="inputUrl">The input Url.</param>
        /// <returns>
        /// Returns the Formatted Url.
        /// </returns>
        private string ReFormatURL(string inputUrl)
        {
            if (inputUrl.StartsWith("http://") || inputUrl.StartsWith("FileID="))
            {
                return inputUrl;
            }

            return string.Format("FileID={0}", Utility.ConvertFilePathToFileId(inputUrl, this.portalSettings?.PortalId ?? Host.HostPortalID));
        }

        /// <summary>Renders the editor config settings.</summary>
        private void RenderEditorConfigSettings()
        {
            foreach (PropertyInfo info in SettingsUtil.GetEditorConfigProperties())
            {
                var description = info.GetCustomAttribute<DescriptionAttribute>(true);

                var isSubSetting = info.Name == "CodeMirror" || info.Name == "WordCount";

                var settingValueContainer = new HtmlGenericControl("div");
                settingValueContainer.Attributes.Add("class", "dnnFormItem");

                // TODO : Load Localized Setting Name
                if (!isSubSetting)
                {
                    var settingNameLabel = new Label { Text = string.Format("{0}:", info.Name) };
                    settingNameLabel.Attributes.Add("class", "dnnLabel");
                    settingValueContainer.Controls.Add(settingNameLabel);
                }

                switch (info.PropertyType.Name)
                {
                    case "String":
                        {
                            var settingValueInput = new TextBox { ID = info.Name, CssClass = "settingValueInput", TextMode = TextBoxMode.MultiLine, Rows = 5 };

                            if (description != null)
                            {
                                settingValueInput.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueInput);
                        }

                        break;
                    case "Decimal":
                    case "Int32":
                        {
                            var settingValueInput = new TextBox { ID = info.Name, CssClass = "settingValueInputNumeric" };

                            if (description != null)
                            {
                                settingValueInput.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueInput);
                        }

                        break;
                    case "Boolean":
                        {
                            var settingValueInput = new CheckBox { ID = info.Name };

                            if (description != null)
                            {
                                settingValueInput.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueInput);
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        {
                            var settingValueListBox = new DropDownList { ID = info.Name };

                            foreach (var item in Enum.GetValues(typeof(ToolBarLocation)))
                            {
                                settingValueListBox.Items.Add(new ListItem(item.ToString()));
                            }

                            if (description != null)
                            {
                                settingValueListBox.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueListBox);
                        }

                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        {
                            var settingValueListBox = new DropDownList { ID = info.Name };

                            foreach (var item in Enum.GetValues(typeof(EnterModus)))
                            {
                                settingValueListBox.Items.Add(new ListItem(item.ToString()));
                            }

                            if (description != null)
                            {
                                settingValueListBox.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueListBox);
                        }

                        break;
                    case "DefaultLinkType":
                        {
                            var settingValueListBox = new DropDownList { ID = info.Name };

                            foreach (var item in Enum.GetValues(typeof(LinkType)))
                            {
                                settingValueListBox.Items.Add(new ListItem(item.ToString()));
                            }

                            if (description != null)
                            {
                                settingValueListBox.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueListBox);
                        }

                        break;
                    case "ContentsLangDirection":
                        {
                            var settingValueListBox = new DropDownList { ID = info.Name };

                            foreach (var item in Enum.GetValues(typeof(LanguageDirection)))
                            {
                                settingValueListBox.Items.Add(new ListItem(item.ToString()));
                            }

                            if (description != null)
                            {
                                settingValueListBox.ToolTip = description.Description;
                            }

                            settingValueContainer.Controls.Add(settingValueListBox);
                        }

                        break;
                    case "CodeMirror":
                        {
                            foreach (
                           var codeMirrorInfo in
                               typeof(CodeMirror).GetProperties()
                                                 .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                            {
                                var codeMirrorDescription = codeMirrorInfo.GetCustomAttribute<DescriptionAttribute>(true);

                                var settingValueContainer2 = new HtmlGenericControl("div");
                                settingValueContainer2.Attributes.Add("class", "dnnFormItem");

                                var settingNameLabel2 = new Label { Text = string.Format("{0} - {1}:", info.Name, codeMirrorInfo.Name) };
                                settingNameLabel2.Attributes.Add("class", "dnnLabel");
                                settingValueContainer2.Controls.Add(settingNameLabel2);

                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var settingValueInput = new TextBox { ID = codeMirrorInfo.Name, CssClass = "settingValueInput", TextMode = TextBoxMode.MultiLine, Rows = 5 };

                                            if (description != null)
                                            {
                                                settingValueInput.ToolTip = codeMirrorDescription.Description;
                                            }

                                            settingValueContainer2.Controls.Add(settingValueInput);
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var settingValueInput = new CheckBox { ID = codeMirrorInfo.Name };

                                            if (description != null)
                                            {
                                                settingValueInput.ToolTip = codeMirrorDescription.Description;
                                            }

                                            settingValueContainer2.Controls.Add(settingValueInput);
                                        }

                                        break;
                                }

                                this.EditorConfigHolder.Controls.Add(settingValueContainer2);
                            }
                        }

                        break;
                    case "WordCount":
                        {
                            foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                            {
                                var wordCountDescription = wordCountInfo.GetCustomAttribute<DescriptionAttribute>(true);

                                var settingValueContainer2 = new HtmlGenericControl("div");
                                settingValueContainer2.Attributes.Add("class", "dnnFormItem");

                                var settingNameLabel2 = new Label { Text = $"{info.Name} - {wordCountInfo.Name}:" };
                                settingNameLabel2.Attributes.Add("class", "dnnLabel");
                                settingValueContainer2.Controls.Add(settingNameLabel2);

                                switch (wordCountInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var settingValueInput = new TextBox { ID = wordCountInfo.Name, CssClass = "settingValueInput", TextMode = TextBoxMode.MultiLine, Rows = 5 };

                                            if (description != null)
                                            {
                                                settingValueInput.ToolTip = wordCountDescription.Description;
                                            }

                                            settingValueContainer2.Controls.Add(settingValueInput);
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var settingValueInput = new CheckBox { ID = wordCountInfo.Name };

                                            if (description != null)
                                            {
                                                settingValueInput.ToolTip = wordCountDescription.Description;
                                            }

                                            settingValueContainer2.Controls.Add(settingValueInput);
                                        }

                                        break;
                                }

                                this.EditorConfigHolder.Controls.Add(settingValueContainer2);
                            }
                        }

                        break;
                }

                if (isSubSetting)
                {
                    continue;
                }

                this.EditorConfigHolder.Controls.Add(settingValueContainer);
            }
        }

        /// <summary>Save Settings only for this Module Instance.</summary>
        private void SaveModuleSettings()
        {
            this.moduleInstanceName = this.request.QueryString["minc"];
            string key = $"DNNCKMI#{this.ModuleId}#INS#{this.moduleInstanceName}#";

            var moduleController = new ModuleController();

            // Editor config settings
            foreach (var info in
                SettingsUtil.GetEditorConfigProperties())
            {
                switch (info.PropertyType.Name)
                {
                    case "Decimal":
                    case "Int32":
                    case "String":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{info.Name}", textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{info.Name}", checkBox.Checked.ToString());
                            }
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ContentsLangDirection":
                    case "EnterMode":
                    case "ShiftEnterMode":
                    case "ToolbarLocation":
                    case "DefaultLinkType":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList?.SelectedItem != null)
                            {
                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{info.Name}", dropDownList.SelectedValue);
                            }
                        }

                        break;
                    case "CodeMirror":
                        {
                            foreach (
                           var codeMirrorInfo in
                               typeof(CodeMirror).GetProperties()
                                                 .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                            {
                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{codeMirrorInfo.Name}", textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{codeMirrorInfo.Name}", checkBox.Checked.ToString());
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                    case "WordCount":
                        {
                            foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                            {
                                switch (wordCountInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{wordCountInfo.Name}", textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{wordCountInfo.Name}", checkBox.Checked.ToString());
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                }
            }
            ///////////////////

            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.SKIN}", this.ddlSkin.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.CODEMIRRORTHEME}", this.CodeMirrorTheme.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.BROWSER}", this.ddlBrowser.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.IMAGEBUTTON}", this.ddlImageButton.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.FILELISTVIEWMODE}", this.FileListViewMode.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.DEFAULTLINKMODE}", this.DefaultLinkMode.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.USEANCHORSELECTOR}", this.UseAnchorSelector.Checked.ToString());
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}", this.ShowPageLinksTabFirst.Checked.ToString());
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}", this.OverrideFileOnUpload.Checked.ToString());
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.SUBDIRS}", this.cbBrowserDirs.Checked.ToString());
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.BROWSERROOTDIRID}", this.BrowserRootDir.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.BROWSERROOTDIRFORIMGID}", this.BrowserRootDirForImg.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.UPLOADDIRID}", this.UploadDir.SelectedValue);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.UPLOADDIRFORIMGID}", this.UploadDirForImg.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.FILELISTPAGESIZE}", this.FileListPageSize.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidthUpload.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.RESIZEWIDTHUPLOAD}", this.txtResizeWidthUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeightUpload.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.RESIZEHEIGHTUPLOAD}", this.txtResizeHeightUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidth.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.RESIZEWIDTH}", this.txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeight.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.RESIZEHEIGHT}", this.txtResizeHeight.Text);
            }

            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.INJECTJS}", this.InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(this.txtWidth.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.WIDTH}", this.txtWidth.Text);
            }

            if (Utility.IsUnit(this.txtHeight.Text))
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.HEIGHT}", this.txtWidth.Text);
            }

            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.BROWSERALLOWFOLLOWFOLDERPERMS}", this.BrowAllowFollowPerms.Checked.ToString());

            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.BLANKTEXT}", this.txtBlanktext.Text);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.CSS}", this.CssUrl.Url);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.TEMPLATEFILES}", this.TemplUrl.Url);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.CUSTOMJSFILE}", this.CustomJsFile.Url);
            moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.CONFIG}", this.ConfigUrl.Url);

            string sRoles = this.chblBrowsGr.Items.Cast<ListItem>().Where(item => item.Selected).Aggregate(
                string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                moduleController.UpdateModuleSetting(this.ModuleId, $"{key}{SettingConstants.ROLES}", sRoles);
            }

            // Save Toolbar Setting for every Role
            for (var i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                var label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                var ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
                else
                {
                    var objRole = RoleController.Instance.GetRoleByName(this.portalSettings?.PortalId ?? Host.HostPortalID, label.Text);

                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (var i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                Label label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        $"{key}-1{SettingConstants.UPLOADFILELIMITS}",
                        sizeLimit.Text);
                }
                else
                {
                    var objRole = RoleController.Instance.GetRoleByName(this.portalSettings?.PortalId ?? Host.HostPortalID, label.Text);

                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{1}#{2}", key, objRole.RoleID, SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
            }
        }

        /// <summary>Save Settings for this Page Or Portal.</summary>
        /// <param name="key">
        /// The key.
        /// </param>
        private void SaveSettingsByKey(string key)
        {
            // Editor config settings
            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
            {
                switch (info.PropertyType.Name)
                {
                    case "Decimal":
                    case "Int32":
                    case "String":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                EditorController.AddOrUpdateEditorHostSetting($"{key}{info.Name}", textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                EditorController.AddOrUpdateEditorHostSetting($"{key}{info.Name}", checkBox.Checked.ToString());
                            }
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ContentsLangDirection":
                    case "EnterMode":
                    case "ShiftEnterMode":
                    case "ToolbarLocation":
                    case "DefaultLinkType":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList?.SelectedItem != null)
                            {
                                EditorController.AddOrUpdateEditorHostSetting($"{key}{info.Name}", dropDownList.SelectedValue);
                            }
                        }

                        break;
                    case "CodeMirror":
                        {
                            foreach (var codeMirrorInfo in
                                typeof(CodeMirror).GetProperties()
                                    .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                            {
                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(
                                                this.EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting($"{key}{codeMirrorInfo.Name}", textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                this.EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting($"{key}{codeMirrorInfo.Name}", checkBox.Checked.ToString());
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                    case "WordCount":
                        {
                            foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                            {
                                switch (wordCountInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(
                                                this.EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting($"{key}{wordCountInfo.Name}", textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                this.EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting($"{key}{wordCountInfo.Name}", checkBox.Checked.ToString());
                                            }
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                }
            }
            ///////////////////

            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.SKIN}", this.ddlSkin.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.CODEMIRRORTHEME}", this.CodeMirrorTheme.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.BROWSER}", this.ddlBrowser.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.IMAGEBUTTON}", this.ddlImageButton.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.FILELISTVIEWMODE}", this.FileListViewMode.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.DEFAULTLINKMODE}", this.DefaultLinkMode.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.USEANCHORSELECTOR}", this.UseAnchorSelector.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.SHOWPAGELINKSTABFIRST}", this.ShowPageLinksTabFirst.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.OVERRIDEFILEONUPLOAD}", this.OverrideFileOnUpload.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.SUBDIRS}", this.cbBrowserDirs.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.HOSTBROWSERROOTDIR}", this.HostBrowserRootDir.Text);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.BROWSERROOTDIRID}", this.BrowserRootDir.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.HOSTBROWSERROOTDIRFORIMG}", this.HostBrowserRootDirForImg.Text);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.BROWSERROOTDIRFORIMGID}", this.BrowserRootDirForImg.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.HOSTUPLOADDIR}", this.HostUploadDir.Text);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.UPLOADDIRID}", this.UploadDir.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.HOSTUPLOADDIRFORIMG}", this.HostUploadDirForImg.Text);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.UPLOADDIRFORIMGID}", this.UploadDirForImg.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.FILELISTPAGESIZE}", this.FileListPageSize.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidthUpload.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.RESIZEWIDTHUPLOAD}", this.txtResizeWidthUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeightUpload.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.RESIZEHEIGHTUPLOAD}", this.txtResizeHeightUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidth.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.RESIZEWIDTH}", this.txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeight.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.RESIZEHEIGHT}", this.txtResizeHeight.Text);
            }

            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.INJECTJS}", this.InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(this.txtWidth.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.WIDTH}", this.txtWidth.Text);
            }

            if (Utility.IsUnit(this.txtHeight.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.HEIGHT}", this.txtHeight.Text);
            }

            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.BLANKTEXT}", this.txtBlanktext.Text);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.CSS}", this.CssUrl.Url);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.TEMPLATEFILES}", this.TemplUrl.Url);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.CUSTOMJSFILE}", this.CustomJsFile.Url);
            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.CONFIG}", this.ConfigUrl.Url);

            EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.BROWSERALLOWFOLLOWFOLDERPERMS}", this.BrowAllowFollowPerms.Checked.ToString());

            var sRoles = this.chblBrowsGr.Items.Cast<ListItem>()
                .Where(item => item.Selected)
                .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                EditorController.AddOrUpdateEditorHostSetting($"{key}{SettingConstants.ROLES}", sRoles);
            }

            // Save Toolbar Setting for every Role
            for (var i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    EditorController.AddOrUpdateEditorHostSetting($"{key}toolb#{"-1"}", ddLToolB.SelectedValue);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(this.portalSettings?.PortalId != Null.NullInteger ? this.portalSettings?.PortalId ?? Host.HostPortalID : Host.HostPortalID, label.Text);

                    EditorController.AddOrUpdateEditorHostSetting($"{key}toolb#{objRole.RoleID}", ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (var i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    EditorController.AddOrUpdateEditorHostSetting($"{key}-1{SettingConstants.UPLOADFILELIMITS}", sizeLimit.Text);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(this.portalSettings?.PortalId != Null.NullInteger ? this.portalSettings?.PortalId ?? Host.HostPortalID : Host.HostPortalID, label.Text);

                    EditorController.AddOrUpdateEditorHostSetting($"{key}{objRole.RoleID}#{SettingConstants.UPLOADFILELIMITS}", sizeLimit.Text);
                }
            }
        }

        /// <summary>Save all Settings for the Current Selected Mode.</summary>
        private void SaveSettings()
        {
            ModuleDefinitionInfo objm;
            var db = new ModuleController();
            var moduleInfo = db.GetModuleByDefinition(this.portalSettings?.PortalId ?? Host.HostPortalID, "User Accounts");

            try
            {
                objm = ModuleDefinitionController.GetModuleDefinitionByID(this.CurrentModule.ModuleDefID);
            }
            catch (Exception)
            {
                objm = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "User Accounts", moduleInfo.DesktopModuleID);
            }

            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Host:
                    this.SaveSettingsByKey(SettingConstants.HostKey);
                    break;
                case SettingsMode.Portal:
                    this.SaveSettingsByKey(SettingConstants.PortalKey(this.portalSettings?.PortalId ?? Host.HostPortalID));
                    break;
                case SettingsMode.Page:
                    this.SaveSettingsByKey($"DNNCKT#{this.CurrentOrSelectedTabId}#");
                    break;
                default:
                    if (this.CurrentSettingsMode.Equals(SettingsMode.ModuleInstance) && !objm.FriendlyName.Equals("User Accounts"))
                    {
                        this.SaveModuleSettings();
                    }

                    break;
            }

            // Finally Clear Cache
            EditorController.ClearEditorCache();
        }

        /// <summary>Set Current Language.</summary>
        private void SetLanguage()
        {
            this.lblHeader.Text = Localization.GetString("lblHeader.Text", this.ResXFile, this.LangCode);

            this.ProviderVersion.Text = "<strong>DNN Connect CKEditor™ Provider</strong> ";

            this.lblPortal.Text = $"<strong>{Localization.GetString("lblPortal.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblPage.Text = $"<strong>{Localization.GetString("lblPage.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblModType.Text = $"<strong>{Localization.GetString("lblModType.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblModName.Text = $"<strong>{Localization.GetString("lblModName.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblModInst.Text = $"<strong>{Localization.GetString("lblModInst.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblUName.Text = $"<strong>{Localization.GetString("lblUName.Text", this.ResXFile, this.LangCode)}</strong> ";
            this.lblMainSet.Text = Localization.GetString("lblMainSet.Text", this.ResXFile, this.LangCode);
            this.lblSettings.Text = Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode);
            this.lblSetFor.Text = Localization.GetString("lblSetFor.Text", this.ResXFile, this.LangCode);
            this.lblBrowser.Text = Localization.GetString("lblBrowser.Text", this.ResXFile, this.LangCode);

            this.lblBlanktext.Text = Localization.GetString("lblBlanktext.Text", this.ResXFile, this.LangCode);
            this.txtBlanktext.ToolTip = Localization.GetString("BlanktextTT.Text", this.ResXFile, this.LangCode);

            this.lblBrowsSec.Text = Localization.GetString("lblBrowsSec.Text", this.ResXFile, this.LangCode);
            this.lblBrowAllow.Text = Localization.GetString("lblBrowAllow.Text", this.ResXFile, this.LangCode);
            this.BrowserRootFolder.Text = Localization.GetString("BrowserRootFolder.Text", this.ResXFile, this.LangCode);
            this.OverrideFileOnUploadLabel.Text = Localization.GetString("OverrideFileOnUploadLabel.Text", this.ResXFile, this.LangCode);
            this.lblBrowserDirs.Text = Localization.GetString("lblBrowserDirs.Text", this.ResXFile, this.LangCode);
            this.UploadFolderLabel.Text = Localization.GetString("UploadFolderLabel.Text", this.ResXFile, this.LangCode);
            this.lblCustomConfig.Text = Localization.GetString("lblCustomConfig.Text", this.ResXFile, this.LangCode);
            this.lblInjectSyntaxJs.Text = Localization.GetString("lblInjectSyntaxJs.Text", this.ResXFile, this.LangCode);
            this.lblWidth.Text = Localization.GetString("lblWidth.Text", this.ResXFile, this.LangCode);
            this.lblHeight.Text = Localization.GetString("lblHeight.Text", this.ResXFile, this.LangCode);
            this.lblEditorConfig.Text = Localization.GetString("lblEditorConfig.Text", this.ResXFile, this.LangCode);
            this.lblCssurl.Text = Localization.GetString("lblCSSURL.Text", this.ResXFile, this.LangCode);
            this.lblToolbars.Text = Localization.GetString("lblToolbars.Text", this.ResXFile, this.LangCode);
            this.UploadFileLimitLabel.Text = Localization.GetString("UploadFileLimitLabel.Text", this.ResXFile, this.LangCode);
            this.lblTemplFiles.Text = Localization.GetString("lblTemplFiles.Text", this.ResXFile, this.LangCode);
            this.CustomJsFileLabel.Text = Localization.GetString("CustomJsFileLabel.Text", this.ResXFile, this.LangCode);
            this.lblCustomToolbars.Text = Localization.GetString("lblCustomToolbars.Text", this.ResXFile, this.LangCode);
            this.lblToolbarList.Text = Localization.GetString("lblToolbarList.Text", this.ResXFile, this.LangCode);
            this.lblToolbName.Text = Localization.GetString("lblToolbName.Text", this.ResXFile, this.LangCode);
            this.lblToolbSet.Text = Localization.GetString("lblToolbSet.Text", this.ResXFile, this.LangCode);
            this.lblResizeWidthUpload.Text = Localization.GetString("lblResizeWidthUpload.Text", this.ResXFile, this.LangCode);
            this.lblResizeHeightUpload.Text = Localization.GetString("lblResizeHeightUpload.Text", this.ResXFile, this.LangCode);
            this.lblResizeWidth.Text = Localization.GetString("lblResizeWidth.Text", this.ResXFile, this.LangCode);
            this.lblResizeHeight.Text = Localization.GetString("lblResizeHeight.Text", this.ResXFile, this.LangCode);
            this.lblImport.Text = Localization.GetString("lnkImport.Text", this.ResXFile, this.LangCode);
            this.CreateGroupLabel.Text = Localization.GetString("CreateGroupLabel.Text", this.ResXFile, this.LangCode);
            this.AddRowBreakLabel.Text = Localization.GetString("AddRowBreakLabel.Text", this.ResXFile, this.LangCode);
            this.lblToolbarPriority.Text = Localization.GetString("lblToolbarPriority.Text", this.ResXFile, this.LangCode);
            this.ToolbarGroupsLabel.Text = Localization.GetString("ToolbarGroupsLabel.Text", this.ResXFile, this.LangCode);
            this.lblSkin.Text = Localization.GetString("lblSkin.Text", this.ResXFile, this.LangCode);
            this.CodeMirrorLabel.Text = Localization.GetString("CodeMirrorLabel.Text", this.ResXFile, this.LangCode);
            this.Wait.Text = Localization.GetString("Wait.Text", this.ResXFile, this.LangCode);
            this.WaitMessage.Text = Localization.GetString("WaitMessage.Text", this.ResXFile, this.LangCode);
            this.EditorConfigWarning.Text = Localization.GetString("EditorConfigWarning.Text", this.ResXFile, this.LangCode);

            this.FileListPageSizeLabel.Text = Localization.GetString("FileListPageSizeLabel.Text", this.ResXFile, this.LangCode);
            this.FileListViewModeLabel.Text = Localization.GetString("FileListViewModeLabel.Text", this.ResXFile, this.LangCode);
            this.lblUseAnchorSelector.Text = Localization.GetString("lblUseAnchorSelector.Text", this.ResXFile, this.LangCode);
            this.lblShowPageLinksTabFirst.Text = Localization.GetString("lblShowPageLinksTabFirst.Text", this.ResXFile, this.LangCode);

            this.iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);
            this.iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);

            this.iBCancel.AlternateText = Localization.GetString("CancelToolbar.Text", this.ResXFile, this.LangCode);
            this.iBCancel.ToolTip = Localization.GetString("CancelToolbar.Text", this.ResXFile, this.LangCode);

            this.iBEdit.AlternateText = Localization.GetString("EditToolbar.Text", this.ResXFile, this.LangCode);
            this.iBEdit.ToolTip = Localization.GetString("EditToolbar.Text", this.ResXFile, this.LangCode);

            this.iBDelete.AlternateText = Localization.GetString("DeleteToolbar.Text", this.ResXFile, this.LangCode);
            this.iBDelete.ToolTip = Localization.GetString("DeleteToolbar.Text", this.ResXFile, this.LangCode);

            this.lblExport.Text = Localization.GetString("lnkExport.Text", this.ResXFile, this.LangCode);

            this.ExportNow.Text = Localization.GetString("ExportNow.Text", this.ResXFile, this.LangCode);
            this.lnkImportNow.Text = Localization.GetString("ImportNow.Text", this.ResXFile, this.LangCode);

            this.btnOk.Text = Localization.GetString("btnOK.Text", this.ResXFile, this.LangCode);

            this.rBlSetMode.Items[0].Text = Localization.GetString("Portal.Text", this.ResXFile, this.LangCode);
            this.rBlSetMode.Items[1].Text = Localization.GetString("Page.Text", this.ResXFile, this.LangCode);

            if (this.rBlSetMode.Items.Count.Equals(3))
            {
                this.rBlSetMode.Items[2].Text = Localization.GetString("ModuleInstance.Text", this.ResXFile, this.LangCode);
            }

            this.FileListViewMode.Items[0].Text = Localization.GetString("DetailView.Text", this.ResXFile, this.LangCode);
            this.FileListViewMode.Items[1].Text = Localization.GetString("ListView.Text", this.ResXFile, this.LangCode);
            this.FileListViewMode.Items[2].Text = Localization.GetString("IconsView.Text", this.ResXFile, this.LangCode);

            this.DefaultLinkModeLabel.Text = Localization.GetString("DefaultLinkModeLabel.Text", this.ResXFile, this.LangCode);

            this.DefaultLinkMode.Items[0].Text = Localization.GetString("DefaultLinkMode0.Text", this.ResXFile, this.LangCode);
            this.DefaultLinkMode.Items[1].Text = Localization.GetString("DefaultLinkMode1.Text", this.ResXFile, this.LangCode);
            this.DefaultLinkMode.Items[2].Text = Localization.GetString("DefaultLinkMode2.Text", this.ResXFile, this.LangCode);
            this.DefaultLinkMode.Items[3].Text = Localization.GetString("DefaultLinkMode3.Text", this.ResXFile, this.LangCode);
        }

        /// <summary>Saves all Settings and Close Options.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OK_Click(object sender, EventArgs e)
        {
            this.SaveSettings();

            // check if toolbar set editor is not correctly saved
            if (this.iBAdd.ImageUrl.Contains("save.gif") && !string.IsNullOrEmpty(this.ToolbarSet.Value))
            {
                var modifiedSet = ToolbarUtil.ConvertStringToToolbarSet(this.ToolbarSet.Value);
                var toolbarEdit = this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dnnTxtToolBName.Text));

                toolbarEdit.ToolbarGroups = modifiedSet.ToolbarGroups;
                toolbarEdit.Priority = int.Parse(this.dDlToolbarPrio.SelectedValue);
                var homeDirectory = this.portalSettings?.HomeDirectoryMapPath ?? Globals.HostMapPath;
                var homeDirPath = !string.IsNullOrEmpty(this.configFolder)
                                      ? Path.Combine(homeDirectory, this.configFolder)
                                      : homeDirectory;
                ToolbarUtil.SaveToolbarSets(this.listToolbars, homeDirPath);
            }

            this.ShowNotification(Localization.GetString("lblInfo.Text", this.ResXFile, this.LangCode), "success");

            this.BindOptionsData(true);
        }

        /// <summary>Remove Current selected Settings.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Remove_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Host:
                    EditorController.DeleteAllHostSettings();
                    break;
                case SettingsMode.Portal:
                    EditorController.DeleteAllPortalSettings(this.portalSettings?.PortalId ?? Null.NullInteger);
                    break;
                case SettingsMode.Page:
                    EditorController.DeleteCurrentPageSettings(this.CurrentOrSelectedTabId);
                    break;
                case SettingsMode.ModuleInstance:
                    this.DelModuleSettings();
                    break;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>Remove selected all Settings.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void RemoveAll_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Host:
                    EditorController.DeleteAllHostSettings();
                    break;
                case SettingsMode.Portal:
                    EditorController.DeleteAllPortalSettings(this.portalSettings?.PortalId ?? Null.NullInteger);
                    break;
                case SettingsMode.Page:
                    EditorController.DeleteAllPageSettings(this.portalSettings?.PortalId ?? Null.NullInteger);
                    break;
                case SettingsMode.ModuleInstance:
                    EditorController.DeleteAllModuleSettings(this.portalSettings?.PortalId ?? Null.NullInteger);
                    break;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>Handles the Click event of the RemoveChild control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void RemoveChild_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Page:
                    {
                        // Delete all Page Setting for all Child Tabs
                        EditorController.DeleteAllChildPageSettings(this.CurrentOrSelectedTabId);
                    }

                    break;
                case SettingsMode.ModuleInstance:
                    {
                        // Delete all Module Instance Settings for the Current Tab
                        EditorController.DeleteAllModuleSettingsById(this.CurrentOrSelectedTabId);
                    }

                    break;
                default:
                    return;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>Copies the current Page Settings to all Child Pages.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CopyToAllChild_Click(object sender, EventArgs e)
        {
            var childTabs = TabController.GetTabsByParent(this.CurrentOrSelectedTabId, this.CurrentOrSelectedPortalId);

            foreach (var tab in childTabs)
            {
                // Sa Settings to tab
                this.SaveSettingsByKey($"DNNCKT#{tab.TabID}#");
            }

            // Finally Clear Cache
            EditorController.ClearEditorCache();

            this.ShowNotification(Localization.GetString("lblInfoCopyAll.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>Reloaded the Settings of the Selected Mode.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SetMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadSettings(this.rBlSetMode.SelectedIndex, false);
            this.BindUserGroupsGridView();
        }

        /// <summary>Shows the info notification.</summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        private void ShowNotification(string message, string type)
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                this.GetType(),
                $"notification_{Guid.NewGuid()}",
                string.Format(
                    "ShowNotificationBar({0},{1},{2});",
                    HttpUtility.JavaScriptStringEncode(message, addDoubleQuotes: true),
                    HttpUtility.JavaScriptStringEncode(type, addDoubleQuotes: true),
                    HttpUtility.JavaScriptStringEncode(this.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/images/"), addDoubleQuotes: true)),
                true);
        }

        /// <summary>Renders the URL controls.</summary>
        /// <param name="reloadControls">if set to <see langword="true"/> [reload controls].</param>
        private void RenderUrlControls(bool reloadControls = false)
        {
            // Assign Url Controls on the Page the Correct Portal Id
            this.ConfigUrl.PortalId = this.portalSettings?.PortalId ?? Null.NullInteger;
            this.TemplUrl.PortalId = this.portalSettings?.PortalId ?? Null.NullInteger;
            this.CustomJsFile.PortalId = this.portalSettings?.PortalId ?? Null.NullInteger;
            this.CssUrl.PortalId = this.portalSettings?.PortalId ?? Null.NullInteger;
            this.ImportFile.PortalId = this.portalSettings?.PortalId ?? Null.NullInteger;

            if (!reloadControls)
            {
                return;
            }

            this.TemplUrl.ReloadFiles = true;
            this.ConfigUrl.ReloadFiles = true;
            this.CustomJsFile.ReloadFiles = true;
            this.CssUrl.ReloadFiles = true;
            this.ImportFile.ReloadFiles = true;
        }

        /// <summary>Exports the settings.</summary>
        /// <returns>Returns the exported EditorProviderSettings.</returns>
        private EditorProviderSettings ExportSettings()
        {
            var exportSettings = new EditorProviderSettings { SettingMode = SettingsMode.Default };

            exportSettings.SettingMode = this.CurrentSettingsMode;

            // Export all Editor config settings
            foreach (var info in SettingsUtil.GetEditorConfigProperties())
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, textBox.Text, null);
                            }
                        }

                        break;
                    case "Int32":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, int.Parse(textBox.Text), null);
                            }
                        }

                        break;
                    case "Decimal":
                        {
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, decimal.Parse(textBox.Text), null);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            info.SetValue(exportSettings.Config, checkBox.Checked, null);
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList.SelectedItem != null)
                            {
                                info.SetValue(
                                    exportSettings.Config,
                                    (ToolBarLocation)Enum.Parse(typeof(ToolBarLocation), dropDownList.SelectedValue),
                                    null);
                            }
                        }

                        break;
                    case "DefaultLinkType":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList.SelectedItem != null)
                            {
                                info.SetValue(
                                    exportSettings.Config,
                                    (LinkType)Enum.Parse(typeof(LinkType), dropDownList.SelectedValue),
                                    null);
                            }
                        }

                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList.SelectedItem != null)
                            {
                                info.SetValue(
                                    exportSettings.Config,
                                    (EnterModus)Enum.Parse(typeof(EnterModus), dropDownList.SelectedValue),
                                    null);
                            }
                        }

                        break;
                    case "ContentsLangDirection":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList.SelectedItem != null)
                            {
                                info.SetValue(
                                    exportSettings.Config,
                                    (LanguageDirection)Enum.Parse(typeof(LanguageDirection), dropDownList.SelectedValue),
                                    null);
                            }
                        }

                        break;
                    case "CodeMirror":
                        {
                            foreach (var codeMirrorInfo in
                                typeof(CodeMirror).GetProperties()
                                    .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                            {
                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(
                                                this.EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (!string.IsNullOrEmpty(textBox.Text))
                                            {
                                                codeMirrorInfo.SetValue(
                                                    exportSettings.Config.CodeMirror,
                                                    textBox.Text,
                                                    null);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                this.EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            codeMirrorInfo.SetValue(
                                                exportSettings.Config.CodeMirror,
                                                checkBox.Checked,
                                                null);
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                    case "WordCount":
                        {
                            foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                            {
                                switch (wordCountInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(
                                                this.EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (!string.IsNullOrEmpty(textBox.Text))
                                            {
                                                wordCountInfo.SetValue(
                                                    exportSettings.Config.WordCount,
                                                    textBox.Text,
                                                    null);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                this.EditorConfigHolder,
                                                wordCountInfo.Name);

                                            wordCountInfo.SetValue(
                                                exportSettings.Config.WordCount,
                                                checkBox.Checked,
                                                null);
                                        }

                                        break;
                                }
                            }
                        }

                        break;
                }
            }
            ///////////////////

            exportSettings.Config.Skin = this.ddlSkin.SelectedValue;
            exportSettings.Config.CodeMirror.Theme = this.CodeMirrorTheme.SelectedValue;
            exportSettings.Browser = this.ddlBrowser.SelectedValue;
            exportSettings.BrowserAllowFollowFolderPerms = this.BrowAllowFollowPerms.Checked;
            exportSettings.ImageButton = this.ddlImageButton.SelectedValue;
            exportSettings.FileListViewMode =
                (FileListView)Enum.Parse(typeof(FileListView), this.FileListViewMode.SelectedValue);
            exportSettings.DefaultLinkMode = (LinkMode)Enum.Parse(typeof(LinkMode), this.DefaultLinkMode.SelectedValue);
            exportSettings.UseAnchorSelector = this.UseAnchorSelector.Checked;
            exportSettings.ShowPageLinksTabFirst = this.ShowPageLinksTabFirst.Checked;
            exportSettings.OverrideFileOnUpload = this.OverrideFileOnUpload.Checked;
            exportSettings.SubDirs = this.cbBrowserDirs.Checked;
            exportSettings.HostBrowserRootDir = this.HostBrowserRootDir.Text;
            exportSettings.BrowserRootDirId = int.Parse(this.BrowserRootDir.SelectedValue);
            exportSettings.HostBrowserRootDirForImg = this.HostBrowserRootDirForImg.Text;
            exportSettings.BrowserRootDirForImgId = int.Parse(this.BrowserRootDirForImg.SelectedValue);
            exportSettings.HostUploadDir = this.HostUploadDir.Text;
            exportSettings.UploadDirId = int.Parse(this.UploadDir.SelectedValue);
            exportSettings.HostUploadDirForImg = this.HostUploadDirForImg.Text;
            exportSettings.UploadDirForImgId = int.Parse(this.UploadDirForImg.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                exportSettings.FileListPageSize = int.Parse(this.FileListPageSize.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidthUpload.Text))
            {
                exportSettings.ResizeWidthUpload = int.Parse(this.txtResizeWidthUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeightUpload.Text))
            {
                exportSettings.ResizeHeightUpload = int.Parse(this.txtResizeHeightUpload.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidth.Text))
            {
                exportSettings.ResizeWidth = int.Parse(this.txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeight.Text))
            {
                exportSettings.ResizeHeight = int.Parse(this.txtResizeHeight.Text);
            }

            exportSettings.InjectSyntaxJs = this.InjectSyntaxJs.Checked;

            if (Utility.IsUnit(this.txtWidth.Text))
            {
                exportSettings.EditorWidth = this.txtWidth.Text;
            }

            if (Utility.IsUnit(this.txtHeight.Text))
            {
                exportSettings.EditorHeight = this.txtHeight.Text;
            }

            exportSettings.BlankText = this.txtBlanktext.Text;
            exportSettings.Config.ContentsCss = this.CssUrl.Url;
            exportSettings.Config.Templates_Files = this.TemplUrl.Url;
            exportSettings.CustomJsFile = this.CustomJsFile.Url;
            exportSettings.Config.CustomConfig = this.ConfigUrl.Url;

            this.SetBrowserRoles(exportSettings);
            this.SetToolbarRoles(exportSettings);
            this.SetUploadSizeRoles(exportSettings);

            return exportSettings;
        }

        /// <summary>Sets the browser roles.</summary>
        /// <param name="exportSettings">The export settings.</param>
        private void SetBrowserRoles(EditorProviderSettings exportSettings)
        {
            var sRoles = this.chblBrowsGr.Items.Cast<ListItem>()
                                    .Where(item => item.Selected)
                                    .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                exportSettings.BrowserRoles = sRoles;
            }
        }

        /// <summary>Sets the toolbar roles.</summary>
        /// <param name="exportSettings">The export settings.</param>
        private void SetToolbarRoles(EditorProviderSettings exportSettings)
        {
            var listToolbarRoles = new List<ToolbarRoles>();

            // Save Toolbar Setting for every Role
            for (var i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                var label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                var ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = ddLToolB.SelectedValue });
                }
                else
                {
                    this.AddOtherToolbarRoles(listToolbarRoles, label.Text, ddLToolB.SelectedValue);
                }
            }

            exportSettings.ToolBarRoles = listToolbarRoles;
        }

        /// <summary>Sets the upload size roles.</summary>
        /// <param name="exportSettings">The export settings.</param>
        private void SetUploadSizeRoles(EditorProviderSettings exportSettings)
        {
            var listUploadSizeRoles = new List<UploadSizeRoles>();
            this.UploadFileLimits.DataSource = exportSettings.UploadSizeRoles;

            // Save Upload File Limit Setting for every Role
            for (var i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals(UnauthenticatedUsersRoleName))
                {
                    listUploadSizeRoles.Add(new UploadSizeRoles { RoleId = -1, UploadFileLimit = Convert.ToInt32(sizeLimit.Text) });
                }
                else
                {
                    this.AddOtherUploadRoles(label.Text, listUploadSizeRoles, sizeLimit.Text);
                }
            }

            exportSettings.UploadSizeRoles = listUploadSizeRoles;
        }

        /// <summary>Adds the other upload roles.</summary>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="listUploadSizeRoles">The list upload size roles.</param>
        /// <param name="sizeLimit">The size limit.</param>
        private void AddOtherUploadRoles(string roleName, List<UploadSizeRoles> listUploadSizeRoles, string sizeLimit)
        {
            if (this.IsAllInstances)
            {
                listUploadSizeRoles.AddRange(
                    from IPortalInfo portal in PortalController.Instance.GetPortals()
                    select RoleController.Instance.GetRoleByName(portal.PortalId, roleName)
                    into objRole
                    select new UploadSizeRoles() { RoleId = objRole.RoleID, UploadFileLimit = Convert.ToInt32(sizeLimit) });
            }
            else
            {
                var objRole = RoleController.Instance.GetRoleByName(this.portalSettings.PortalId, roleName);
                listUploadSizeRoles.Add(new UploadSizeRoles { RoleId = objRole.RoleID, UploadFileLimit = Convert.ToInt32(sizeLimit) });
            }
        }

        /// <summary>Adds the other toolbar roles.</summary>
        /// <param name="listToolbarRoles">The list toolbar roles.</param>
        /// <param name="roleName">Name of the role.</param>
        /// <param name="value">The value.</param>
        private void AddOtherToolbarRoles(List<ToolbarRoles> listToolbarRoles, string roleName, string value)
        {
            if (this.IsAllInstances)
            {
                listToolbarRoles.AddRange(
                    from IPortalInfo portal in PortalController.Instance.GetPortals()
                    select RoleController.Instance.GetRoleByName(portal.PortalId, roleName)
                    into objRole
                    select new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = value });
            }
            else
            {
                var objRole = RoleController.Instance.GetRoleByName(this.portalSettings.PortalId, roleName);
                listToolbarRoles.Add(new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = value });
            }
        }

        /// <summary>Gets default upload size limits for each existent role.</summary>
        /// <param name="portalRoles">A list of roles.</param>
        /// <returns>A list of <see cref="UploadSizeRoles"/> instances.</returns>
        private List<UploadSizeRoles> GetDefaultUploadFileSettings(IList<RoleInfo> portalRoles)
        {
            return portalRoles.Select(role => new UploadSizeRoles()
            {
                RoleId = role.RoleID,
                RoleName = role.RoleName,
                UploadFileLimit = -1,
            }).ToList();
        }
    }
}
