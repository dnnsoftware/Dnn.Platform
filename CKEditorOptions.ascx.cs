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
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
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

namespace DNNConnect.CKEditorProvider
{

    /// <summary>
    /// The CKEditor options page.
    /// </summary>
    public partial class CKEditorOptions : PortalModuleBase
    {
        #region Constants and Fields

        /// <summary>
        ///   The provider type.
        /// </summary>
        private const string ProviderType = "htmlEditor";

        /// <summary>
        ///   The provider config.
        /// </summary>
        private readonly ProviderConfiguration provConfig = ProviderConfiguration.GetProviderConfiguration(ProviderType);

        /// <summary>
        ///   The request.
        /// </summary>
        private readonly HttpRequest request = HttpContext.Current.Request;

        /// <summary>
        ///   The _portal settings.
        /// </summary>
        private PortalSettings _portalSettings;

        /// <summary>
        /// Override Default Config Folder from Web.config
        /// </summary>
        private string configFolder = string.Empty;

        /// <summary>
        ///   The list toolbars.
        /// </summary>
        private List<ToolbarSet> listToolbars;

        /// <summary>
        ///   The list of all available toolbar buttons.
        /// </summary>
        private List<ToolbarButton> listButtons;

        /// <summary>
        ///   The list of all available toolbar buttons.
        /// </summary>
        private List<ToolbarButton> allListButtons;

        private EditorProviderSettings currentSettings;

        /// <summary>
        ///   The module instance name
        /// </summary>
        private string moduleInstanceName;

        /// <summary>
        ///   Gets or sets The Full Toolbar Set
        /// </summary>
        private ToolbarSet toolbarSets;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is host mode.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is host mode; otherwise, <c>false</c>.
        /// </value>
        public bool IsHostMode
        {
            get
            {
                return ViewState["IsHostMode"] != null && (bool)ViewState["IsHostMode"];
            }

            set
            {
                ViewState["IsHostMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [current portal only].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [current portal only]; otherwise, <c>false</c>.
        /// </value>
        public bool CurrentPortalOnly
        {
            get
            {
                return ViewState["CurrentPortalOnly"] != null && (bool)ViewState["CurrentPortalOnly"];
            }

            set
            {
                ViewState["CurrentPortalOnly"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the Current or selected Tab ID.
        /// </summary>
        public int CurrentOrSelectedTabId
        {
            get
            {
                var o = ViewState["CurrentTabId"];
                if (o != null)
                {
                    return (int)o;
                }

                return 1;
            }

            set
            {
                ViewState["CurrentTabId"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the Current or selected Portal ID.
        /// </summary>
        public int CurrentOrSelectedPortalId
        {
            get
            {
                return ViewState["CurrentPortalId"] != null ? (int)ViewState["CurrentPortalId"] : 0;
            }

            set
            {
                ViewState["CurrentPortalId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default host load mode.
        /// </summary>
        /// <value>
        /// The default host load mode.
        /// </value>
        public int DefaultHostLoadMode
        {
            get
            {
                return ViewState["DefaultHostLoadMode"] != null ? (int)ViewState["DefaultHostLoadMode"] : 0;
            }

            set
            {
                ViewState["DefaultHostLoadMode"] = value;
            }
        }

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LangCode
        {
            get
            {
                return !string.IsNullOrEmpty(request.QueryString["langCode"])
                            ? request.QueryString["langCode"]
                            : CultureInfo.CurrentCulture.Name;
            }
        }

        /// <summary>
        ///   Gets the Name for the Current Resource file name
        /// </summary>
        protected string ResXFile
        {
            get
            {
                return
                    ResolveUrl(
                        string.Format(
                            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/{0}/Options.aspx.resx",
                            Localization.LocalResourceDirectory));
            }
        }

        /// <summary>
        /// Gets or sets the current settings mode
        /// </summary>
        private SettingsMode CurrentSettingsMode
        {
            get
            {
                return (SettingsMode)ViewState["CurrentSettingsMode"];
            }

            set
            {
                ViewState["CurrentSettingsMode"] = value;
            }
        }

        /// <summary>
        ///   Gets the Config Url Control.
        /// </summary>
        private UrlControl ConfigUrl
        {
            get
            {
                return ctlConfigUrl;
            }
        }

        /// <summary>
        ///   Gets the CSS Url Control.
        /// </summary>
        private UrlControl CssUrl
        {
            get
            {
                return ctlCssurl;
            }
        }

        /// <summary>
        ///   Gets the Import File Url Control.
        /// </summary>
        private UrlControl ImportFile
        {
            get
            {
                return ctlImportFile;
            }
        }

        /// <summary>
        ///   Gets the Template Url Control.
        /// </summary>
        private UrlControl TemplUrl
        {
            get
            {
                return ctlTemplUrl;
            }
        }

        /// <summary>
        ///   Gets the Custom JS File Url Control.
        /// </summary>
        private UrlControl CustomJsFile
        {
            get
            {
                return ctlCustomJsFile;
            }
        }

        private ModuleInfo _currentModule;
        private ModuleInfo CurrentModule
        {
            get
            {
                if (_currentModule != null)
                {
                    return _currentModule;
                }

                if (ModuleConfiguration != null && !Null.IsNull(ModuleConfiguration.ModuleID))
                {
                    _currentModule = ModuleConfiguration;
                    return _currentModule;
                }

                _currentModule = new ModuleController().GetModule(
                    Request.QueryString.GetValueOrDefault("ModuleId", -1),
                    TabId,
                    false);

                return _currentModule;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the options data.
        /// </summary>
        /// <param name="reloadOptionsfromModule">if set to <c>true</c> [reload options from module].</param>
        internal void BindOptionsData(bool reloadOptionsfromModule = false)
        {
            // Check if Options Window is in Host Page
            if (IsHostMode)
            {
                if (!Page.IsPostBack)
                {
                    LastTabId.Value = "0";
                }

                _portalSettings = GetPortalSettings();

                FillFolders();

                RenderUrlControls(true);

                FillRoles();

                BindUserGroupsGridView();

                lblSetFor.Visible = false;
                rBlSetMode.Visible = false;
                lnkRemoveAll.Visible = false;
                InfoTabLi.Visible = false;
                InfoTabHolder.Visible = false;

                if (DefaultHostLoadMode.Equals(0))
                {
                    lblSettings.Text = string.Format(
                        "{0} - <em>{1} {2} - Portal ID: {3}</em>",
                        Localization.GetString("lblSettings.Text", ResXFile, LangCode),
                        Localization.GetString("lblPortal.Text", ResXFile, LangCode),
                        _portalSettings.PortalName,
                        CurrentOrSelectedPortalId);
                }
                else if (DefaultHostLoadMode.Equals(1))
                {
                    lblSettings.Text = string.Format(
                        "{0} - <em>{1} {2} - TabID: {3}</em>",
                        Localization.GetString("lblSettings.Text", ResXFile, LangCode),
                        Localization.GetString("lblPage.Text", ResXFile, LangCode),
                        new TabController().GetTab(CurrentOrSelectedTabId, _portalSettings.PortalId, false)
                                           .TabName,
                        CurrentOrSelectedTabId);
                }
                else
                {
                    lblSettings.Text = Localization.GetString("lblSettings.Text", ResXFile, LangCode);
                }

                LoadSettings(DefaultHostLoadMode);
            }
            else
            {
                var pageKey = string.Format("DNNCKT#{0}#", CurrentOrSelectedTabId);
                var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", ModuleId, moduleInstanceName);

                if (SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, ModuleId))
                {
                    LoadSettings(2);
                }
                else
                {
                    var settingsDictionary = EditorController.GetEditorHostSettings();

                    LoadSettings(SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey) ? 1 : 0);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            AddJavaScripts();

            if (Utility.IsInRoles(_portalSettings.AdministratorRoleName, _portalSettings))
            {
                if (Page.IsPostBack)
                {
                    return;
                }

                SetLanguage();

                FillInformations();

                // Load Skin List
                FillSkinList();

                FillFolders();

                RenderUrlControls();

                FillRoles();

                BindUserGroupsGridView();

                BindOptionsData();

                // Remove CKFinder from the Browser list if not installed
                if (
                    !File.Exists(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js"))
                {
                    ddlBrowser.Items.RemoveAt(2);
                }
            }
            else
            {
                Visible = false;

                Page.ClientScript.RegisterStartupScript(
                    GetType(),
                    "errorcloseScript",
                    string.Format(
                        "javascript:alert('{0}');self.close();",
                        Localization.GetString("Error1.Text", ResXFile, LangCode)),
                    true);
            }
            LocalResourceFile = ResXFile;
        }

        /// <summary>
        /// Adds the Java scripts.
        /// </summary>
        private void AddJavaScripts()
        {
            ClientResourceManager.RegisterStyleSheet(
                Page,
                ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/jquery.notification.css"));

            ClientResourceManager.RegisterStyleSheet(
                Page,
                ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/Options.css"));

            JavaScript.RegisterClientReference(Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "jquery.notification",
                ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/jquery.notification.js"));

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "OptionsJs",
                ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/Options.js"));
        }



        /// <summary>
        /// Import Current Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Import_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ImportFile.Url))
            {
                return;
            }

            string sXmlImport = ImportFile.Url;

            upOptions.Update();

            // RESET Dialog 
            ImportFile.Url = null;

            int imageFileId = int.Parse(sXmlImport.Substring(7));

            // FileInfo objFileInfo = objFileController.GetFileById(imageFileId, this._portalSettings.PortalId);
            var objFileInfo = FileManager.Instance.GetFile(imageFileId);

            sXmlImport = _portalSettings.HomeDirectoryMapPath + objFileInfo.Folder + objFileInfo.FileName;

            try
            {
                ImportXmlFile(sXmlImport);

                ShowNotification(Localization.GetString("Imported.Text", ResXFile, LangCode), "success");
            }
            catch (Exception)
            {
                ShowNotification(
                    Localization.GetString("BadImportXml.Text", ResXFile, LangCode), "error");
            }
        }

        /// <summary>
        /// Export Current Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Export_Click(object sender, EventArgs e)
        {
            var exportSettings = ExportSettings();

            // Save XML file
            try
            {
                var serializer = new XmlSerializer(typeof(EditorProviderSettings));

                var xmlFileName = !string.IsNullOrEmpty(ExportFileName.Text.Trim())
                                      ? ExportFileName.Text
                                      : string.Format("CKEditorSettings-{0}.xml", exportSettings.SettingMode);

                if (!xmlFileName.EndsWith(".xml"))
                {
                    xmlFileName += ".xml";
                }

                var exportFolderInfo = FolderManager.Instance.GetFolder(Convert.ToInt32(ExportDir.SelectedValue));

                var textWriter = ExportDir.SelectedValue.Equals("-1")
                                     ? new StreamWriter(
                                           Path.Combine(_portalSettings.HomeDirectoryMapPath, xmlFileName))
                                     : new StreamWriter(Path.Combine(exportFolderInfo.PhysicalPath, xmlFileName));

                serializer.Serialize(textWriter, exportSettings);

                textWriter.Close();

                ShowNotification(Localization.GetString("Export.Text", ResXFile, LangCode), "success");
            }
            catch (Exception exception)
            {
                ShowNotification(exception.Message, "error");
            }

            upOptions.Update();
        }

        /// <summary>
        /// Imports the XML file.
        /// </summary>
        /// <param name="xmlFilePath">The XML file path.</param>
        /// <param name="changeMode">if set to <c>true</c> [change mode].</param>
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

            FillSettings(importedSettings, changeMode);
        }

        /// <summary>
        /// Fills the setting controls with the loaded Setting Values.
        /// </summary>
        /// <param name="importedSettings">The imported settings.</param>
        /// <param name="changeMode">if set to <c>true</c> [change mode].</param>
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
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                textBox.Text = value.ToString();
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, info.Name);

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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

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
                                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                textBox.Text = value.ToString();
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, codeMirrorInfo.Name);

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
                                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                textBox.Text = value.ToString();
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, wordCountInfo.Name);

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
                && ddlSkin.Items.FindByValue(importedSettings.Config.Skin) != null)
            {
                ddlSkin.ClearSelection();
                ddlSkin.SelectedValue = importedSettings.Config.Skin;
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.CodeMirror.Theme)
                && CodeMirrorTheme.Items.FindByValue(importedSettings.Config.CodeMirror.Theme) != null)
            {
                CodeMirrorTheme.ClearSelection();
                CodeMirrorTheme.SelectedValue = importedSettings.Config.CodeMirror.Theme;
            }

            if (!string.IsNullOrEmpty(importedSettings.Browser)
                && ddlBrowser.Items.FindByValue(importedSettings.Browser) != null)
            {
                ddlBrowser.ClearSelection();
                ddlBrowser.SelectedValue = importedSettings.Browser;
            }

            FileListPageSize.Text = importedSettings.FileListPageSize.ToString();

            FileListViewMode.SelectedValue = importedSettings.FileListViewMode.ToString();
            DefaultLinkMode.SelectedValue = importedSettings.DefaultLinkMode.ToString();
            UseAnchorSelector.Checked = importedSettings.UseAnchorSelector;
            ShowPageLinksTabFirst.Checked = importedSettings.ShowPageLinksTabFirst;

            cbBrowserDirs.Checked = importedSettings.SubDirs;

            OverrideFileOnUpload.Checked = importedSettings.OverrideFileOnUpload;

            BrowserRootDir.SelectedValue =
                 BrowserRootDir.Items.FindByValue(importedSettings.BrowserRootDirId.ToString()) != null
                     ? importedSettings.BrowserRootDirId.ToString()
                     : "-1";

            UploadDir.SelectedValue = UploadDir.Items.FindByValue(importedSettings.UploadDirId.ToString())
                                           != null
                                               ? importedSettings.UploadDirId.ToString()
                                               : "-1";

            var configFolderInfo =
                Utility.ConvertFilePathToFolderInfo(
                    !string.IsNullOrEmpty(configFolder)
                        ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                        : _portalSettings.HomeDirectoryMapPath,
                    _portalSettings);

            ExportDir.SelectedValue = configFolderInfo != null
                                           &&
                                           ExportDir.Items.FindByValue(configFolderInfo.FolderID.ToString())
                                           != null
                                               ? configFolderInfo.FolderID.ToString()
                                               : "-1";

            ExportFileName.Text = string.Format("CKEditorSettings-{0}.xml", importedSettings.SettingMode);

            switch (importedSettings.SettingMode)
            {
                case SettingsMode.Portal:
                    ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, _portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, CurrentOrSelectedTabId);
                    break;
                case SettingsMode.ModuleInstance:
                    ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, ModuleId);
                    break;
            }

            txtResizeHeight.Text = importedSettings.ResizeWidth.ToString();

            txtResizeHeight.Text = importedSettings.ResizeHeight.ToString();

            InjectSyntaxJs.Checked = importedSettings.InjectSyntaxJs;

            if (Utility.IsUnit(importedSettings.Config.Width))
            {
                txtWidth.Text = importedSettings.Config.Width;
            }

            if (Utility.IsUnit(importedSettings.Config.Height))
            {
                txtHeight.Text = importedSettings.Config.Height;
            }

            if (!string.IsNullOrEmpty(importedSettings.BlankText))
            {
                txtBlanktext.Text = importedSettings.BlankText;
            }

            var imporUploadSizeRoles = importedSettings.UploadSizeRoles;

            // Load Upload Size Setting for Each Portal Role
            foreach (var uploadSizeRole in imporUploadSizeRoles)
            {
                if (uploadSizeRole.RoleId.Equals(-1))
                {
                    for (int i = 0; i < UploadFileLimits.Rows.Count; i++)
                    {
                        Label label = (Label)UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals("Unauthenticated Users"))
                        {
                            continue;
                        }

                        var sizeLimit =
                            (TextBox)UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                        sizeLimit.Text = uploadSizeRole.UploadFileLimit.ToString();
                    }
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleById(_portalSettings.PortalId, uploadSizeRole.RoleId);

                    if (objRole == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < UploadFileLimits.Rows.Count; i++)
                    {
                        Label label = (Label)UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals(objRole.RoleName))
                        {
                            continue;
                        }

                        var sizeLimit =
                            (TextBox)UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                        sizeLimit.Text = uploadSizeRole.UploadFileLimit.ToString();
                    }
                }
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.ContentsCss))
            {
                CssUrl.Url = ReFormatURL(importedSettings.Config.ContentsCss);
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.Templates_Files))
            {
                TemplUrl.Url = ReFormatURL(importedSettings.Config.Templates_Files);
            }

            if (!string.IsNullOrEmpty(importedSettings.CustomJsFile))
            {
                CustomJsFile.Url = ReFormatURL(importedSettings.CustomJsFile);
            }

            if (!string.IsNullOrEmpty(importedSettings.Config.CustomConfig))
            {
                ConfigUrl.Url = ReFormatURL(importedSettings.Config.CustomConfig);
            }

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
                            if (chblBrowsGr.Items.FindByValue(sRoleName) != null)
                            {
                                chblBrowsGr.Items.FindByValue(sRoleName).Selected = true;
                            }
                        }
                        else
                        {
                            if (chblBrowsGr.Items.FindByText(sRoleName) != null)
                            {
                                chblBrowsGr.Items.FindByText(sRoleName).Selected = true;
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
                    rBlSetMode.SelectedIndex = 0;
                    break;
                case SettingsMode.Page:
                    rBlSetMode.SelectedIndex = 1;
                    break;
                case SettingsMode.ModuleInstance:
                    rBlSetMode.SelectedIndex = 2;
                    break;
            }
        }

        /// <summary>
        /// Bind User Groups to GridView
        /// </summary>
        private void BindUserGroupsGridView()
        {
            var lic = new ListItemCollection();

            foreach (var roleItem in
                from RoleInfo objRole in RoleController.Instance.GetRoles(_portalSettings.PortalId)
                select new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() })
            {
                lic.Add(roleItem);
            }

            lic.Add(new ListItem { Text = "Unauthenticated Users", Value = "-1" });

            gvToolbars.DataSource = lic;
            gvToolbars.DataBind();

            InsertToolbars();

            var lblRole = (Label)gvToolbars.HeaderRow.FindControl("lblRole");
            var lblSelToolb = (Label)gvToolbars.HeaderRow.FindControl("lblSelToolb");

            lblRole.Text = Localization.GetString("lblRole.Text", ResXFile, LangCode);
            lblSelToolb.Text = Localization.GetString("lblSelToolb.Text", ResXFile, LangCode);

            // Bind User Groups to UploadFileLimits GridView
            UploadFileLimits.DataSource = lic;
            UploadFileLimits.DataBind();

            lblRole = (Label)UploadFileLimits.HeaderRow.FindControl("lblRole");
            lblSelToolb = (Label)UploadFileLimits.HeaderRow.FindControl("SizeLimitLabel");

            lblRole.Text = Localization.GetString("lblRole.Text", ResXFile, LangCode);
            lblSelToolb.Text = Localization.GetString("SizeLimitLabel.Text", ResXFile, LangCode);
        }

        /// <summary>
        /// Delete Settings only for this Module Instance
        /// </summary>
        private void DelModuleSettings()
        {
            moduleInstanceName = request.QueryString["minc"];
            string moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", ModuleId, moduleInstanceName);

            var moduleController = new ModuleController();

            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
            {
                moduleController.DeleteModuleSetting(ModuleId, string.Format("{0}{1}", moduleKey, info.Name));
            }

            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SKIN));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CODEMIRRORTHEME));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BROWSER));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.FILELISTPAGESIZE));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.FILELISTVIEWMODE));
            moduleController.DeleteModuleSetting(
                 ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.DEFAULTLINKMODE));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.USEANCHORSELECTOR));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SHOWPAGELINKSTABFIRST));
            moduleController.DeleteModuleSetting(
               ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.OVERRIDEFILEONUPLOAD));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SUBDIRS));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BROWSERROOTDIRID));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.UPLOADDIRID));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.INJECTJS));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.WIDTH));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.HEIGHT));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BLANKTEXT));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CSS));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.TEMPLATEFILES));
            moduleController.DeleteModuleSetting(
                ModuleId,
                string.Format("{0}{1}", moduleKey, SettingConstants.CUSTOMJSFILE));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CONFIG));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.ROLES));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.RESIZEHEIGHT));
            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.RESIZEWIDTH));

            foreach (RoleInfo objRole in RoleController.Instance.GetRoles(_portalSettings.PortalId))
            {
                moduleController.DeleteModuleSetting(
                    ModuleId, string.Format("{0}{2}#{1}", moduleKey, objRole.RoleID, SettingConstants.TOOLB));
            }

            moduleController.DeleteModuleSetting(
                ModuleId, string.Format("{0}{2}#{1}", moduleKey, "-1", SettingConstants.TOOLB));

            // Finally Clear Cache
            EditorController.ClearEditorCache();
        }

        /// <summary>
        /// Write Information
        /// </summary>
        private void FillInformations()
        {
            var ckEditorPackage = PackageController.GetPackageByName("DotNetNuke.CKHtmlEditorProvider");

            if (ckEditorPackage != null)
            {
                ProviderVersion.Text += ckEditorPackage.Version;
            }

            lblPortal.Text += _portalSettings.PortalName;

            ModuleDefinitionInfo moduleDefinitionInfo;
            var moduleInfo = new ModuleController().GetModuleByDefinition(
                _portalSettings.PortalId, "User Accounts");

            try
            {
                moduleDefinitionInfo =
                    ModuleDefinitionController.GetModuleDefinitionByID(CurrentModule.ModuleDefID);
            }
            catch (Exception)
            {
                moduleDefinitionInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "User Accounts", moduleInfo.DesktopModuleID);
            }

            try
            {
                lblPage.Text += string.Format(
                    "{0} - TabID {1}",
                    new TabController().GetTab(CurrentOrSelectedTabId, _portalSettings.PortalId, false).TabName,
                    CurrentOrSelectedTabId);
            }
            catch (Exception)
            {
                lblPage.Text = string.Empty;
            }

            if (moduleDefinitionInfo != null)
            {
                lblModType.Text += moduleDefinitionInfo.FriendlyName;
                if (!IsHostMode && moduleDefinitionInfo.FriendlyName.Equals("User Accounts"))
                {
                    rBlSetMode.Items.RemoveAt(2);
                }
            }
            else
            {
                lblModType.Text = string.Empty;
            }

            try
            {
                lblModName.Text += CurrentModule.ModuleTitle;
            }
            catch (Exception)
            {
                lblModName.Text += moduleInfo.ModuleTitle;
            }

            if (request.QueryString["minc"] != null)
            {
                lblModInst.Text += request.QueryString["minc"];
                moduleInstanceName = request.QueryString["minc"];
            }

            if (UserInfo != null)
            {
                lblUName.Text += UserInfo.Username;
            }
            else
            {
                lblUName.Text = string.Empty;
            }
        }

        /// <summary>
        /// Loads all DNN Roles
        /// </summary>
        private void FillRoles()
        {
            chblBrowsGr.Items.Clear();

            foreach (RoleInfo objRole in RoleController.Instance.GetRoles(_portalSettings.PortalId))
            {
                ListItem roleItem = new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() };

                if (objRole.RoleName.Equals(PortalSettings.AdministratorRoleName))
                {
                    roleItem.Selected = true;
                    roleItem.Enabled = false;
                }

                chblBrowsGr.Items.Add(roleItem);
            }
        }

        // Reload Settings based on the Selected Mode

        /// <summary>
        /// Loads the List of available Skins.
        /// </summary>
        private void FillSkinList()
        {
            ddlSkin.Items.Clear();

            DirectoryInfo objDir = new DirectoryInfo(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/skins");

            foreach (ListItem skinItem in
                objDir.GetDirectories().Select(
                    objSubFolder => new ListItem { Text = objSubFolder.Name, Value = objSubFolder.Name }))
            {
                ddlSkin.Items.Add(skinItem);
            }

            // CodeMirror Themes
            CodeMirrorTheme.Items.Clear();

            if (Directory.Exists(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/plugins/codemirror/theme"))
            {
                var themesFolder = new DirectoryInfo(Globals.ApplicationMapPath + "/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/plugins/codemirror/theme");

                // add default theme
                CodeMirrorTheme.Items.Add(new ListItem { Text = "default", Value = "default" });

                foreach (
                    var skinItem in
                        themesFolder.GetFiles("*.css").Select(
                            themeCssFile =>
                            themeCssFile.Name.Replace(themeCssFile.Extension, string.Empty)).Select(
                                themeName => new ListItem { Text = themeName, Value = themeName }))
                {
                    CodeMirrorTheme.Items.Add(skinItem);
                }
            }
        }

        /// <summary>
        /// Loads the List of available Folders.
        /// </summary>
        private void FillFolders()
        {
            UploadDir.Items.Clear();
            BrowserRootDir.Items.Clear();
            ExportDir.Items.Clear();

            foreach (var folder in FolderManager.Instance.GetFolders(_portalSettings.PortalId))
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

                UploadDir.Items.Add(new ListItem(text, value));
                BrowserRootDir.Items.Add(new ListItem(text, value));
                ExportDir.Items.Add(new ListItem(text, value));
            }

            UploadDir.SelectedValue = "-1";
            BrowserRootDir.SelectedValue = "-1";
            ExportDir.SelectedValue = "-1";
        }

        /// <summary>
        /// Gets the portal settings.
        /// </summary>
        /// <returns>
        /// Returns the Current Portal Settings.
        /// </returns>
        private PortalSettings GetPortalSettings()
        {
            PortalSettings portalSettings;

            try
            {
                if (IsHostMode && CurrentPortalOnly)
                {
                    return PortalSettings;
                }

                if (!IsHostMode && request.QueryString["tid"] != null)
                {
                    CurrentOrSelectedTabId = int.Parse(request.QueryString["tid"]);
                }

                if (!IsHostMode && request.QueryString["PortalID"] != null)
                {
                    CurrentOrSelectedPortalId = int.Parse(request.QueryString["PortalID"]);
                }

                var domainName = Globals.GetDomainName(Request, true);

                var portalAlias = PortalAliasController.GetPortalAliasByPortal(CurrentOrSelectedPortalId, domainName);

                portalSettings = new PortalSettings(CurrentOrSelectedTabId, PortalAliasController.Instance.GetPortalAlias(portalAlias));
            }
            catch (Exception)
            {
                portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return portalSettings;
        }

        /// <summary>
        /// Hide Add Toolbar Button if all Priorities are used
        /// </summary>
        private void HideAddToolbar()
        {
            bool bHideAll = !dDlToolbarPrio.Items.Cast<ListItem>().Any(item => item.Enabled);

            if (bHideAll)
            {
                iBAdd.Visible = false;
            }
        }

        /// <summary>
        /// Add new/Save Toolbar Set
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbAddClick(object sender, ImageClickEventArgs e)
        {
            if (string.IsNullOrEmpty(dnnTxtToolBName.Text))
            {
                ShowNotification(Localization.GetString("ToolbarNameMissing.Text", ResXFile, LangCode), "error");

                return;
            }

            if (string.IsNullOrEmpty(ToolbarSet.Value))
            {
                return;
            }

            var modifiedSet = ToolbarUtil.ConvertStringToToolbarSet(ToolbarSet.Value);

            // Save modified Toolbar Set
            if (iBAdd.ImageUrl.Contains("save.gif"))
            {
                var toolbarEdit = listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(dnnTxtToolBName.Text));

                toolbarEdit.ToolbarGroups = modifiedSet.ToolbarGroups;
                toolbarEdit.Priority = int.Parse(dDlToolbarPrio.SelectedValue);

                ToolbarUtil.SaveToolbarSets(
                    listToolbars,
                    !string.IsNullOrEmpty(configFolder)
                        ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                        : _portalSettings.HomeDirectoryMapPath);

                ShowNotification(
                    Localization.GetString("ToolbarSetSaved.Text", ResXFile, LangCode),
                    "success");
            }
            else
            {
                // Add New Toolbar Set
                var newToolbar = new ToolbarSet(dnnTxtToolBName.Text, int.Parse(dDlToolbarPrio.SelectedValue))
                    {
                        ToolbarGroups = modifiedSet.ToolbarGroups
                    };

                listToolbars.Add(newToolbar);

                ToolbarUtil.SaveToolbarSets(
                    listToolbars,
                    !string.IsNullOrEmpty(configFolder)
                        ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                        : _portalSettings.HomeDirectoryMapPath);

                ShowNotification(
                    string.Format(
                        Localization.GetString("ToolbarSetCreated.Text", ResXFile, LangCode),
                        dnnTxtToolBName.Text),
                    "success");
            }

            // Hide Priority
            dDlToolbarPrio.SelectedItem.Enabled = false;

            BindUserGroupsGridView();

            dnnTxtToolBName.Text = string.Empty;
            ToolbarSet.Value = string.Empty;

            List<string> excludeButtons;

            var toolbarSet = new ToolbarSet();

            // Empty Toolbar
            toolbarSet.ToolbarGroups.Add(
                new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", ResXFile, LangCode) });

            FillToolbarGroupsRepeater(toolbarSet, out excludeButtons);

            FillAvailableToolbarButtons(null);

            dnnTxtToolBName.Enabled = true;

            iBAdd.ImageUrl = ResolveUrl("~/images/add.gif");

            iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);
            iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);

            iBCancel.Visible = false;

            HideAddToolbar();
        }

        /// <summary>
        /// Cancel Edit Toolbar
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbCancelClick(object sender, ImageClickEventArgs e)
        {
            dnnTxtToolBName.Text = string.Empty;
            ToolbarSet.Value = string.Empty;

            List<string> excludeButtons;

            var toolbarSet = new ToolbarSet();

            // Empty Toolbar
            toolbarSet.ToolbarGroups.Add(
                new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", ResXFile, LangCode) });

            FillToolbarGroupsRepeater(toolbarSet, out excludeButtons);

            FillAvailableToolbarButtons(null);

            dnnTxtToolBName.Enabled = true;

            dDlToolbarPrio.Items.FindByText(
                listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(dDlCustomToolbars.SelectedValue)).Priority.ToString()).Enabled = false;

            iBAdd.ImageUrl = ResolveUrl("~/images/add.gif");

            iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);
            iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);

            iBCancel.Visible = false;

            HideAddToolbar();
        }

        /// <summary>
        /// Delete Selected Toolbar Set
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbDeleteClick(object sender, ImageClickEventArgs e)
        {
            if (dDlCustomToolbars.SelectedValue == null)
            {
                return;
            }

            var toolbarDelete =
                 listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(dDlCustomToolbars.SelectedValue));

            var priority = toolbarDelete.Priority.ToString();

            if (priority.Length.Equals(1))
            {
                priority = string.Format("0{0}", priority);
            }

            dDlToolbarPrio.Items.FindByText(priority).Enabled = true;

            listToolbars.RemoveAll(toolbarSel => toolbarSel.Name.Equals(dDlCustomToolbars.SelectedValue));

            ToolbarUtil.SaveToolbarSets(
                listToolbars,
                !string.IsNullOrEmpty(configFolder)
                    ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                    : _portalSettings.HomeDirectoryMapPath);

            BindUserGroupsGridView();

            dnnTxtToolBName.Enabled = true;

            iBAdd.ImageUrl = ResolveUrl("~/images/add.gif");

            iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);
            iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);

            ShowNotification(
                Localization.GetString("ToolbarSetDeleted.Text", ResXFile, LangCode),
                "success");
        }

        /// <summary>
        /// Edit Selected Toolbar
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbEditClick(object sender, ImageClickEventArgs e)
        {
            if (dDlCustomToolbars.SelectedValue == null)
            {
                return;
            }

            iBAdd.Visible = true;

            var toolbarEdit = listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(dDlCustomToolbars.SelectedValue));

            if (toolbarEdit != null)
            {
                dnnTxtToolBName.Text = toolbarEdit.Name;
                ToolbarSet.Value = ToolbarUtil.ConvertToolbarSetToString(toolbarEdit);

                List<string> excludeButtons;

                FillToolbarGroupsRepeater(toolbarEdit, out excludeButtons);

                FillAvailableToolbarButtons(excludeButtons);

                var priority = toolbarEdit.Priority.ToString();

                if (priority.Length.Equals(1))
                {
                    priority = string.Format("0{0}", priority);
                }

                dDlToolbarPrio.Items.FindByText(priority).Enabled = true;
                dDlToolbarPrio.SelectedValue = priority;

                dnnTxtToolBName.Enabled = false;

                iBAdd.ImageUrl = ResolveUrl("~/images/save.gif");

                iBAdd.AlternateText = Localization.GetString("SaveToolbar.Text", ResXFile, LangCode);
                iBAdd.ToolTip = Localization.GetString("SaveToolbar.Text", ResXFile, LangCode);

                iBCancel.Visible = true;
            }

            HideAddToolbar();
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        ///   the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            try
            {
                _portalSettings = GetPortalSettings();

                btnOk.Click += OK_Click;

                lnkRemove.Click += Remove_Click;
                lnkRemoveAll.Click += RemoveAll_Click;
                lnkRemoveChild.Click += RemoveChild_Click;
                CopyToAllChild.Click += CopyToAllChild_Click;

                iBAdd.Click += IbAddClick;
                iBCancel.Click += IbCancelClick;
                iBEdit.Click += IbEditClick;
                iBDelete.Click += IbDeleteClick;

                var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
                var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

                if (objProvider != null && !string.IsNullOrEmpty(objProvider.Attributes["ck_configFolder"]))
                {
                    configFolder = objProvider.Attributes["ck_configFolder"];
                }

                listToolbars = ToolbarUtil.GetToolbars(
                    _portalSettings.HomeDirectoryMapPath, configFolder);

                rBlSetMode.SelectedIndexChanged += SetMode_SelectedIndexChanged;

                ToolbarGroupsRepeater.ItemDataBound += ToolbarGroupsRepeater_ItemDataBound;
                gvToolbars.RowDataBound += gvToolbars_RowDataBound;

                RenderEditorConfigSettings();
            }
            catch (Exception ex)
            {
                ShowNotification(ex.Message, "error");
            }
        }

        void gvToolbars_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            ListItemCollection licToolbars = new ListItemCollection();

            foreach (var toolbarSet in listToolbars)
            {
                var toolbarItem = new ListItem {Text = toolbarSet.Name, Value = toolbarSet.Name};
                

                licToolbars.Add(toolbarItem);
            }


            DropDownList ddLToolB = (DropDownList)e.Row.FindControl("ddlToolbars");

            if (ddLToolB == null)
            {
                return;
            }

            ddLToolB.DataSource = licToolbars;
            ddLToolB.DataBind();

            Label label = (Label)e.Row.Cells[0].FindControl("lblRoleName");

            if (label == null)
            {
                return;
            }

            var objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

            if (objRole == null)
            {
                return;
            }

            if (currentSettings == null)
            {
                var settingsDictionary = EditorController.GetEditorHostSettings();
                var pageKey = string.Format("DNNCKT#{0}#", CurrentOrSelectedTabId);
                LoadSettings(SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey) ? 1 : 0);
            }
            
            var currentToolbarSettings = currentSettings.ToolBarRoles.FirstOrDefault(o => o.RoleId == objRole.RoleID);

            if (currentToolbarSettings != null)
            {
                ddLToolB.ClearSelection();

                if (ddLToolB.Items.FindByValue(currentToolbarSettings.Toolbar) != null)
                {
                    ddLToolB.SelectedValue = currentToolbarSettings.Toolbar;
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the ToolbarGroupsRepeater control.
        /// </summary>
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

            if (allListButtons == null)
            {
                allListButtons =
                    ToolbarUtil.LoadToolBarButtons(
                        !string.IsNullOrEmpty(configFolder)
                            ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                            : _portalSettings.HomeDirectoryMapPath);
            }

            foreach (var button in toolbarSets.ToolbarGroups.Where(@group => @group.name.Equals(groupName)).SelectMany(@group => @group.items))
            {

                if (allListButtons.Find(availButton => availButton.ToolbarName.Equals(button)) == null
                    && !button.Equals("/"))
                {
                    continue;
                }

                var groupRow = toolbarButtonsTable.NewRow();

                groupRow["Button"] = button;

                var buttonItem = allListButtons.Find(b => b.ToolbarName.Equals(button));

                groupRow["Icon"] = buttonItem != null ? buttonItem.ToolbarIcon : button;

                toolbarButtonsTable.Rows.Add(groupRow);
            }

            toolbarButtonsRepeater.DataSource = toolbarButtonsTable;
            toolbarButtonsRepeater.DataBind();
        }

        /// <summary>
        /// Insert Toolbar Names from Serialized Xml File
        /// </summary>
        private void InsertToolbars()
        {
            ListItemCollection licToolbars = new ListItemCollection();

            foreach (var toolbarSet in listToolbars)
            {
                var toolbarItem = new ListItem { Text = toolbarSet.Name, Value = toolbarSet.Name };

                licToolbars.Add(toolbarItem);

                // Exclude used Prioritys from the DropDown
                if (dDlToolbarPrio.Items.FindByText(toolbarSet.Priority.ToString()) != null)
                {
                    dDlToolbarPrio.Items.FindByText(toolbarSet.Priority.ToString()).Enabled = false;
                }
            }

            HideAddToolbar();

            dDlCustomToolbars.DataSource = licToolbars;
            dDlCustomToolbars.DataBind();

            List<string> excludeButtons;

            var emptyToolbarSet = new ToolbarSet();

            // Empty Toolbar
            emptyToolbarSet.ToolbarGroups.Add(new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", ResXFile, LangCode) });

            FillToolbarGroupsRepeater(emptyToolbarSet, out excludeButtons);

            // Load Toolbar Buttons
            FillAvailableToolbarButtons(null);
        }

        /// <summary>
        /// Fills the toolbar groups repeater.
        /// </summary>
        /// <param name="toolbarSet">The toolbar set.</param>
        /// <param name="excludeButtons">The exclude buttons list.</param>
        private void FillToolbarGroupsRepeater(ToolbarSet toolbarSet, out List<string> excludeButtons)
        {
            excludeButtons = new List<string>();

            var toolbarGroupsTable = new DataTable();

            toolbarGroupsTable.Columns.Add(new DataColumn("GroupName", typeof(string)));

            toolbarSets = toolbarSet;

            foreach (var group in toolbarSets.ToolbarGroups)
            {
                var groupRow = toolbarGroupsTable.NewRow();

                groupRow["GroupName"] = group.name;

                toolbarGroupsTable.Rows.Add(groupRow);

                // exclude existing buttons in the available list
                excludeButtons.AddRange(@group.items);
            }

            ToolbarGroupsRepeater.DataSource = toolbarGroupsTable;
            ToolbarGroupsRepeater.DataBind();
        }

        /// <summary>
        /// Fills the available toolbar buttons.
        /// </summary>
        /// <param name="excludeItems">The exclude items.</param>
        private void FillAvailableToolbarButtons(ICollection<string> excludeItems)
        {
            var toolbarButtonsTable = new DataTable();

            toolbarButtonsTable.Columns.Add(new DataColumn("Button", typeof(string)));
            toolbarButtonsTable.Columns.Add(new DataColumn("Icon", typeof(string)));

            if (listButtons == null)
            {
                listButtons =
                    ToolbarUtil.LoadToolBarButtons(
                        !string.IsNullOrEmpty(configFolder)
                            ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                            : _portalSettings.HomeDirectoryMapPath);
            }

            var buttons = listButtons;

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

            AvailableToolbarButtons.DataSource = toolbarButtonsTable;
            AvailableToolbarButtons.DataBind();
        }

        /// <summary>
        /// Load Default Host Settings from 'web.config'
        /// </summary>
        private void LoadDefaultSettings()
        {
            var ckeditorProvider = (Provider)provConfig.Providers[provConfig.DefaultProvider];

            if (ckeditorProvider == null)
            {
                return;
            }

            // Skin
            if (ckeditorProvider.Attributes["ck_skin"] != string.Empty
                && ddlSkin.Items.FindByValue(ckeditorProvider.Attributes["ck_skin"]) != null)
            {
                ddlSkin.ClearSelection();

                ddlSkin.SelectedValue = ckeditorProvider.Attributes["ck_skin"];
            }

            // FileBrowser
            if (ckeditorProvider.Attributes["ck_Browser"] != string.Empty)
            {
                ddlBrowser.SelectedValue = ckeditorProvider.Attributes["ck_Browser"];
            }

            if (ckeditorProvider.Attributes["ck_contentsCss"] != string.Empty)
            {
                CssUrl.Url = ckeditorProvider.Attributes["ck_contentsCss"];
            }

            if (ckeditorProvider.Attributes["ck_templates_files"] != string.Empty)
            {
                TemplUrl.Url = ckeditorProvider.Attributes["ck_templates_files"];
            }

            if (ckeditorProvider.Attributes["ck_customConfig"] != string.Empty)
            {
                ConfigUrl.Url = ckeditorProvider.Attributes["ck_customConfig"];
            }

            /*var configPathComplete = !string.IsNullOrEmpty(this.configFolder)
                                         ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                                         : this._portalSettings.HomeDirectoryMapPath;

            // Load Default Settings from XML
            if (File.Exists(Path.Combine(configPathComplete, "CKEditorDefaultSettings.xml")))
            {
                this.ImportXmlFile(Path.Combine(configPathComplete, "CKEditorDefaultSettings.xml"), changeMode);
            }*/
        }

        /// <summary>
        /// Load All Editor Settings
        /// </summary>
        /// <param name="currentMode">The current mode.</param>
        /// <param name="changeMode">if set to <c>true</c> [change mode].</param>
        private void LoadSettings(int currentMode, bool changeMode = true)
        {
            CurrentSettingsMode = (SettingsMode)Enum.Parse(typeof(SettingsMode), currentMode.ToString());

            lnkRemoveAll.Visible = !currentMode.Equals(0);
            lnkRemoveChild.Visible = !currentMode.Equals(0);
            CopyToAllChild.Visible = !currentMode.Equals(0);

            lnkRemove.Text = string.Format(
                Localization.GetString("Remove.Text", ResXFile, LangCode),
                rBlSetMode.Items[currentMode].Text);
            lnkRemoveAll.Text =
                string.Format(
                    Localization.GetString("RemoveAll.Text", ResXFile, LangCode),
                    rBlSetMode.Items[currentMode].Text);

            lnkRemove.ToolTip = string.Format(
                Localization.GetString("Remove.Help", ResXFile, LangCode),
                rBlSetMode.Items[currentMode].Text);
            lnkRemoveAll.ToolTip =
                string.Format(
                    Localization.GetString("RemoveAll.Help", ResXFile, LangCode),
                    rBlSetMode.Items[currentMode].Text);

            LoadDefaultSettings();

            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(_portalSettings.PortalId);

            var portalKey = string.Format("DNNCKP#{0}#", _portalSettings.PortalId);
            var pageKey = string.Format("DNNCKT#{0}#", CurrentOrSelectedTabId);
            var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", ModuleId, moduleInstanceName);

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            currentSettings = SettingsUtil.GetDefaultSettings(
                _portalSettings,
                _portalSettings.HomeDirectoryMapPath,
                objProvider.Attributes["ck_configFolder"],
                portalRoles);

            switch (CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    {
                        // Load Portal Settings ?!
                        if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, portalKey))
                        {
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                                _portalSettings, currentSettings, settingsDictionary, portalKey, portalRoles);

                            // Set Current Mode to Portal
                            currentSettings.SettingMode = SettingsMode.Portal;

                            lnkRemove.Enabled = true;
                        }
                        else
                        {
                            lnkRemove.Enabled = false;
                        }
                    }

                    break;
                case SettingsMode.Page:
                    {
                        // Load Page Settings ?!
                        if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey))
                        {
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                                _portalSettings, currentSettings, settingsDictionary, pageKey, portalRoles);

                            // Set Current Mode to Page
                            currentSettings.SettingMode = SettingsMode.Page;

                            lnkRemove.Enabled = true;
                        }
                        else
                        {
                            lnkRemove.Enabled = false;
                        }

                        var currentTab = new TabController().GetTab(
                            CurrentOrSelectedTabId, _portalSettings.PortalId, false);

                        lnkRemoveChild.Enabled = currentTab.HasChildren;

                        lnkRemoveChild.Text = Localization.GetString(
                                "RemovePageChild.Text", ResXFile, LangCode);
                        lnkRemoveChild.ToolTip = Localization.GetString(
                            "RemovePageChild.Help", ResXFile, LangCode);

                        CopyToAllChild.Enabled = currentTab.HasChildren;

                        CopyToAllChild.Text = Localization.GetString(
                                "CopyPageChild.Text", ResXFile, LangCode);
                        CopyToAllChild.ToolTip = Localization.GetString(
                            "CopyPageChild.Help", ResXFile, LangCode);
                    }

                    break;
                case SettingsMode.ModuleInstance:
                    {
                        // Load Module Settings ?!
                        if (SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, ModuleId))
                        {
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadModuleSettings(
                                _portalSettings, currentSettings, moduleKey, ModuleId, portalRoles);

                            currentSettings.SettingMode = SettingsMode.ModuleInstance;

                            lnkRemove.Enabled = true;
                        }
                        else
                        {
                            lnkRemove.Enabled = false;
                        }

                        lnkRemoveChild.Enabled = true;

                        lnkRemoveChild.Text = Localization.GetString(
                            "RemoveModuleChild.Text", ResXFile, LangCode);
                        lnkRemoveChild.ToolTip = Localization.GetString(
                            "RemoveModuleChild.Help", ResXFile, LangCode);
                    }

                    break;
            }

            if (currentSettings != null)
            {
                FillSettings(currentSettings, changeMode);
            }
        }

        /// <summary>
        /// Re-Formats Url from the Url Control
        /// </summary>
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

            return string.Format("FileID={0}", Utility.ConvertFilePathToFileId(inputUrl, _portalSettings.PortalId));
        }

        /// <summary>
        /// Renders the editor config settings.
        /// </summary>
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

                                EditorConfigHolder.Controls.Add(settingValueContainer2);
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

                                var settingNameLabel2 = new Label { Text = string.Format("{0} - {1}:", info.Name, wordCountInfo.Name) };
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

                                EditorConfigHolder.Controls.Add(settingValueContainer2);
                            }
                        }

                        break;
                }

                if (isSubSetting)
                {
                    continue;
                }

                EditorConfigHolder.Controls.Add(settingValueContainer);
            }
        }

        /// <summary>
        /// Save Settings only for this Module Instance
        /// </summary>
        private void SaveModuleSettings()
        {
            moduleInstanceName = request.QueryString["minc"];
            string key = string.Format("DNNCKMI#{0}#INS#{1}#", ModuleId, moduleInstanceName);

            var moduleController = new ModuleController();

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
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                moduleController.UpdateModuleSetting(
                                    ModuleId, string.Format("{0}{1}", key, info.Name), textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                moduleController.UpdateModuleSetting(
                                    ModuleId, string.Format("{0}{1}", key, info.Name), checkBox.Checked.ToString());
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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

                            if (dropDownList != null)
                            {
                                if (dropDownList.SelectedItem != null)
                                {
                                    moduleController.UpdateModuleSetting(
                                        ModuleId, string.Format("{0}{1}", key, info.Name), dropDownList.SelectedValue);
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
                                switch (codeMirrorInfo.PropertyType.Name)
                                {
                                    case "String":
                                        {
                                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    ModuleId, string.Format("{0}{1}", key, codeMirrorInfo.Name), textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    ModuleId,
                                                    string.Format("{0}{1}", key, codeMirrorInfo.Name),
                                                    checkBox.Checked.ToString());
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
                                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    ModuleId, string.Format("{0}{1}", key, wordCountInfo.Name), textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    ModuleId,
                                                    string.Format("{0}{1}", key, wordCountInfo.Name),
                                                    checkBox.Checked.ToString());
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

            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.SKIN), ddlSkin.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME),
                CodeMirrorTheme.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.BROWSER), ddlBrowser.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE),
                FileListViewMode.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE),
                DefaultLinkMode.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR),
                UseAnchorSelector.Checked.ToString());
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST),
                ShowPageLinksTabFirst.Checked.ToString());
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD),
                OverrideFileOnUpload.Checked.ToString());
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.SUBDIRS),
                cbBrowserDirs.Checked.ToString());
            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID),
                BrowserRootDir.SelectedValue);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID), UploadDir.SelectedValue);

            if (Utility.IsNumeric(FileListPageSize.Text))
            {
                moduleController.UpdateModuleSetting(
                    ModuleId,
                    string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE),
                    FileListPageSize.Text);
            }

            if (Utility.IsNumeric(txtResizeWidth.Text))
            {
                moduleController.UpdateModuleSetting(
                    ModuleId, string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH), txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(txtResizeHeight.Text))
            {
                moduleController.UpdateModuleSetting(
                    ModuleId,
                    string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT),
                    txtResizeHeight.Text);
            }

            moduleController.UpdateModuleSetting(
                ModuleId,
                string.Format("{0}{1}", key, SettingConstants.INJECTJS),
                InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(txtWidth.Text))
            {
                moduleController.UpdateModuleSetting(
                    ModuleId, string.Format("{0}{1}", key, SettingConstants.WIDTH), txtWidth.Text);
            }

            if (Utility.IsUnit(txtHeight.Text))
            {
                moduleController.UpdateModuleSetting(
                    ModuleId, string.Format("{0}{1}", key, SettingConstants.HEIGHT), txtWidth.Text);
            }

            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.BLANKTEXT), txtBlanktext.Text);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.CSS), CssUrl.Url);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES), TemplUrl.Url);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE), CustomJsFile.Url);
            moduleController.UpdateModuleSetting(
                ModuleId, string.Format("{0}{1}", key, SettingConstants.CONFIG), ConfigUrl.Url);

            string sRoles = chblBrowsGr.Items.Cast<ListItem>().Where(item => item.Selected).Aggregate(
                string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                moduleController.UpdateModuleSetting(
                    ModuleId, string.Format("{0}{1}", key, SettingConstants.ROLES), sRoles);
            }

            // Save Toolbar Setting for every Role
            for (int i = 0; i < gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    moduleController.UpdateModuleSetting(
                        ModuleId,
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    moduleController.UpdateModuleSetting(
                        ModuleId,
                        string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < UploadFileLimits.Rows.Count; i++)
            {
                Label label = (Label)UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    moduleController.UpdateModuleSetting(
                        ModuleId,
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    moduleController.UpdateModuleSetting(
                        ModuleId,
                        string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
            }
        }

        /// <summary>
        /// Save Settings for this Page Or Portal
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        private void SavePortalOrPageSettings(string key)
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
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                EditorController.AddOrUpdateEditorHostSetting(
                                    string.Format("{0}{1}", key, info.Name),
                                    textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                EditorController.AddOrUpdateEditorHostSetting(
                                    string.Format("{0}{1}", key, info.Name),
                                    checkBox.Checked.ToString());
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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

                            if (dropDownList != null)
                            {
                                if (dropDownList.SelectedItem != null)
                                {
                                    EditorController.AddOrUpdateEditorHostSetting(
                                        string.Format("{0}{1}", key, info.Name),
                                        dropDownList.SelectedValue);
                                }
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
                                                EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, codeMirrorInfo.Name),
                                                    textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, codeMirrorInfo.Name),
                                                    checkBox.Checked.ToString());
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
                                                EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, wordCountInfo.Name),
                                                    textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(
                                                EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                EditorController.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, wordCountInfo.Name),
                                                    checkBox.Checked.ToString());
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

            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SKIN),
                ddlSkin.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME),
                CodeMirrorTheme.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BROWSER),
                ddlBrowser.SelectedValue);

            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE),
                FileListViewMode.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE),
                DefaultLinkMode.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR),
                UseAnchorSelector.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST),
                ShowPageLinksTabFirst.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD),
                OverrideFileOnUpload.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SUBDIRS),
                cbBrowserDirs.Checked.ToString());
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID),
                BrowserRootDir.SelectedValue);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID),
                UploadDir.SelectedValue);

            if (Utility.IsNumeric(FileListPageSize.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE),
                    FileListPageSize.Text);
            }

            if (Utility.IsNumeric(txtResizeWidth.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH),
                    txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(txtResizeHeight.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT),
                    txtResizeHeight.Text);
            }

            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}injectjs", key),
                InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(txtWidth.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.WIDTH),
                    txtWidth.Text);
            }

            if (Utility.IsUnit(txtHeight.Text))
            {
                EditorController.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.HEIGHT),
                    txtHeight.Text);
            }

            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BLANKTEXT),
                txtBlanktext.Text);
            EditorController.AddOrUpdateEditorHostSetting(string.Format("{0}{1}", key, SettingConstants.CSS), CssUrl.Url);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES),
                TemplUrl.Url);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE),
                CustomJsFile.Url);
            EditorController.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CONFIG),
                ConfigUrl.Url);

            string sRoles = chblBrowsGr.Items.Cast<ListItem>()
                .Where(item => item.Selected)
                .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                EditorController.AddOrUpdateEditorHostSetting(string.Format("{0}{1}", key, SettingConstants.ROLES), sRoles);
            }

            // Save Toolbar Setting for every Role
            for (int i = 0; i < gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    EditorController.AddOrUpdateEditorHostSetting(
                        string.Format("{0}toolb#{1}", key, "-1"),
                        ddLToolB.SelectedValue);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    EditorController.AddOrUpdateEditorHostSetting(
                        string.Format("{0}toolb#{1}", key, objRole.RoleID),
                        ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    EditorController.AddOrUpdateEditorHostSetting(
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    EditorController.AddOrUpdateEditorHostSetting(
                        string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
            }
        }

        /// <summary>
        /// Save all Settings for the Current Selected Mode
        /// </summary>
        private void SaveSettings()
        {
            ModuleDefinitionInfo objm;
            ModuleController db = new ModuleController();
            ModuleInfo moduleInfo = db.GetModuleByDefinition(_portalSettings.PortalId, "User Accounts");

            try
            {
                objm = ModuleDefinitionController.GetModuleDefinitionByID(CurrentModule.ModuleDefID);
            }
            catch (Exception)
            {
                objm = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "User Accounts", moduleInfo.DesktopModuleID);
            }

            switch (CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    SavePortalOrPageSettings(string.Format("DNNCKP#{0}#", _portalSettings.PortalId));
                    break;
                case SettingsMode.Page:
                    SavePortalOrPageSettings(string.Format("DNNCKT#{0}#", CurrentOrSelectedTabId));
                    break;
                default:
                    if (CurrentSettingsMode.Equals(SettingsMode.ModuleInstance) && !objm.FriendlyName.Equals("User Accounts"))
                    {
                        SaveModuleSettings();
                    }

                    break;
            }

            // Finally Clear Cache
            EditorController.ClearEditorCache();
        }

        /// <summary>
        /// Set Current Language
        /// </summary>
        private void SetLanguage()
        {
            lblHeader.Text = Localization.GetString("lblHeader.Text", ResXFile, LangCode);

            ProviderVersion.Text = "<strong>DNN Connect CKEditor™ Provider</strong> ";

            lblPortal.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblPortal.Text", ResXFile, LangCode));
            lblPage.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblPage.Text", ResXFile, LangCode));
            lblModType.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModType.Text", ResXFile, LangCode));
            lblModName.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModName.Text", ResXFile, LangCode));
            lblModInst.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModInst.Text", ResXFile, LangCode));
            lblUName.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblUName.Text", ResXFile, LangCode));
            lblMainSet.Text = Localization.GetString("lblMainSet.Text", ResXFile, LangCode);
            lblSettings.Text = Localization.GetString("lblSettings.Text", ResXFile, LangCode);
            lblSetFor.Text = Localization.GetString("lblSetFor.Text", ResXFile, LangCode);
            lblBrowser.Text = Localization.GetString("lblBrowser.Text", ResXFile, LangCode);

            lblBlanktext.Text = Localization.GetString("lblBlanktext.Text", ResXFile, LangCode);
            txtBlanktext.ToolTip = Localization.GetString("BlanktextTT.Text", ResXFile, LangCode);

            lblBrowsSec.Text = Localization.GetString("lblBrowsSec.Text", ResXFile, LangCode);
            lblBrowAllow.Text = Localization.GetString("lblBrowAllow.Text", ResXFile, LangCode);
            BrowserRootFolder.Text = Localization.GetString("BrowserRootFolder.Text", ResXFile, LangCode);
            OverrideFileOnUploadLabel.Text = Localization.GetString("OverrideFileOnUploadLabel.Text", ResXFile, LangCode);
            lblBrowserDirs.Text = Localization.GetString("lblBrowserDirs.Text", ResXFile, LangCode);
            UploadFolderLabel.Text = Localization.GetString("UploadFolderLabel.Text", ResXFile, LangCode);
            lblCustomConfig.Text = Localization.GetString("lblCustomConfig.Text", ResXFile, LangCode);
            lblInjectSyntaxJs.Text = Localization.GetString("lblInjectSyntaxJs.Text", ResXFile, LangCode);
            lblWidth.Text = Localization.GetString("lblWidth.Text", ResXFile, LangCode);
            lblHeight.Text = Localization.GetString("lblHeight.Text", ResXFile, LangCode);
            lblEditorConfig.Text = Localization.GetString("lblEditorConfig.Text", ResXFile, LangCode);
            lblCssurl.Text = Localization.GetString("lblCSSURL.Text", ResXFile, LangCode);
            lblToolbars.Text = Localization.GetString("lblToolbars.Text", ResXFile, LangCode);
            UploadFileLimitLabel.Text = Localization.GetString("UploadFileLimitLabel.Text", ResXFile, LangCode);
            lblTemplFiles.Text = Localization.GetString("lblTemplFiles.Text", ResXFile, LangCode);
            CustomJsFileLabel.Text = Localization.GetString("CustomJsFileLabel.Text", ResXFile, LangCode);
            lblCustomToolbars.Text = Localization.GetString("lblCustomToolbars.Text", ResXFile, LangCode);
            lblToolbarList.Text = Localization.GetString("lblToolbarList.Text", ResXFile, LangCode);
            lblToolbName.Text = Localization.GetString("lblToolbName.Text", ResXFile, LangCode);
            lblToolbSet.Text = Localization.GetString("lblToolbSet.Text", ResXFile, LangCode);
            lblResizeWidth.Text = Localization.GetString("lblResizeWidth.Text", ResXFile, LangCode);
            lblResizeHeight.Text = Localization.GetString("lblResizeHeight.Text", ResXFile, LangCode);
            lblImport.Text = Localization.GetString("lnkImport.Text", ResXFile, LangCode);
            CreateGroupLabel.Text = Localization.GetString("CreateGroupLabel.Text", ResXFile, LangCode);
            AddRowBreakLabel.Text = Localization.GetString("AddRowBreakLabel.Text", ResXFile, LangCode);
            lblToolbarPriority.Text = Localization.GetString("lblToolbarPriority.Text", ResXFile, LangCode);
            ToolbarGroupsLabel.Text = Localization.GetString("ToolbarGroupsLabel.Text", ResXFile, LangCode);
            lblSkin.Text = Localization.GetString("lblSkin.Text", ResXFile, LangCode);
            CodeMirrorLabel.Text = Localization.GetString("CodeMirrorLabel.Text", ResXFile, LangCode);
            Wait.Text = Localization.GetString("Wait.Text", ResXFile, LangCode);
            WaitMessage.Text = Localization.GetString("WaitMessage.Text", ResXFile, LangCode);
            EditorConfigWarning.Text = Localization.GetString(
                "EditorConfigWarning.Text", ResXFile, LangCode);

            FileListPageSizeLabel.Text = Localization.GetString(
                "FileListPageSizeLabel.Text", ResXFile, LangCode);
            FileListViewModeLabel.Text = Localization.GetString(
                "FileListViewModeLabel.Text", ResXFile, LangCode);
            lblUseAnchorSelector.Text = Localization.GetString(
                "lblUseAnchorSelector.Text", ResXFile, LangCode);
            lblShowPageLinksTabFirst.Text = Localization.GetString(
                "lblShowPageLinksTabFirst.Text", ResXFile, LangCode);

            iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);
            iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", ResXFile, LangCode);

            iBCancel.AlternateText = Localization.GetString("CancelToolbar.Text", ResXFile, LangCode);
            iBCancel.ToolTip = Localization.GetString("CancelToolbar.Text", ResXFile, LangCode);

            iBEdit.AlternateText = Localization.GetString("EditToolbar.Text", ResXFile, LangCode);
            iBEdit.ToolTip = Localization.GetString("EditToolbar.Text", ResXFile, LangCode);

            iBDelete.AlternateText = Localization.GetString("DeleteToolbar.Text", ResXFile, LangCode);
            iBDelete.ToolTip = Localization.GetString("DeleteToolbar.Text", ResXFile, LangCode);

            lblExport.Text = Localization.GetString("lnkExport.Text", ResXFile, LangCode);

            ExportNow.Text = Localization.GetString("ExportNow.Text", ResXFile, LangCode);
            lnkImportNow.Text = Localization.GetString("ImportNow.Text", ResXFile, LangCode);

            btnOk.Text = Localization.GetString("btnOK.Text", ResXFile, LangCode);

            rBlSetMode.Items[0].Text = Localization.GetString("Portal.Text", ResXFile, LangCode);
            rBlSetMode.Items[1].Text = Localization.GetString("Page.Text", ResXFile, LangCode);

            if (rBlSetMode.Items.Count.Equals(3))
            {
                rBlSetMode.Items[2].Text = Localization.GetString(
                    "ModuleInstance.Text", ResXFile, LangCode);
            }

            FileListViewMode.Items[0].Text = Localization.GetString("DetailView.Text", ResXFile, LangCode);
            FileListViewMode.Items[1].Text = Localization.GetString("ListView.Text", ResXFile, LangCode);
            FileListViewMode.Items[2].Text = Localization.GetString("IconsView.Text", ResXFile, LangCode);

            DefaultLinkModeLabel.Text = Localization.GetString("DefaultLinkModeLabel.Text", ResXFile, LangCode);

            DefaultLinkMode.Items[0].Text = Localization.GetString("DefaultLinkMode0.Text", ResXFile, LangCode);
            DefaultLinkMode.Items[1].Text = Localization.GetString("DefaultLinkMode1.Text", ResXFile, LangCode);
            DefaultLinkMode.Items[2].Text = Localization.GetString("DefaultLinkMode2.Text", ResXFile, LangCode);
            DefaultLinkMode.Items[3].Text = Localization.GetString("DefaultLinkMode3.Text", ResXFile, LangCode);
        }

        /// <summary>
        /// Saves all Settings and Close Options
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void OK_Click(object sender, EventArgs e)
        {
            SaveSettings();

            // check if toolbar set editor is not correctly saved
            if (iBAdd.ImageUrl.Contains("save.gif") && !string.IsNullOrEmpty(ToolbarSet.Value))
            {
                var modifiedSet = ToolbarUtil.ConvertStringToToolbarSet(ToolbarSet.Value);
                var toolbarEdit = listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(dnnTxtToolBName.Text));

                toolbarEdit.ToolbarGroups = modifiedSet.ToolbarGroups;
                toolbarEdit.Priority = int.Parse(dDlToolbarPrio.SelectedValue);

                ToolbarUtil.SaveToolbarSets(
                    listToolbars,
                    !string.IsNullOrEmpty(configFolder)
                        ? Path.Combine(_portalSettings.HomeDirectoryMapPath, configFolder)
                        : _portalSettings.HomeDirectoryMapPath);
            }

            ShowNotification(Localization.GetString("lblInfo.Text", ResXFile, LangCode), "success");

            BindOptionsData(true);
        }

        /// <summary>
        /// Remove Current selected Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Remove_Click(object sender, EventArgs e)
        {
            switch (CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    EditorController.DeleteAllPortalSettings(_portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    EditorController.DeleteCurrentPageSettings(CurrentOrSelectedTabId);
                    break;
                case SettingsMode.ModuleInstance:
                    DelModuleSettings();
                    break;
            }

            ShowNotification(Localization.GetString("lblInfoDel.Text", ResXFile, LangCode), "success");
        }

        /// <summary>
        /// Remove selected all Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void RemoveAll_Click(object sender, EventArgs e)
        {
            switch (CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    EditorController.DeleteAllPortalSettings(_portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    EditorController.DeleteAllPageSettings(_portalSettings.PortalId);
                    break;
                case SettingsMode.ModuleInstance:
                    EditorController.DeleteAllModuleSettings(_portalSettings.PortalId);
                    break;
            }

            ShowNotification(Localization.GetString("lblInfoDel.Text", ResXFile, LangCode), "success");
        }

        /// <summary>
        /// Handles the Click event of the RemoveChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void RemoveChild_Click(object sender, EventArgs e)
        {
            switch (CurrentSettingsMode)
            {
                case SettingsMode.Page:
                    {
                        // Delete all Page Setting for all Child Tabs
                        EditorController.DeleteAllChildPageSettings(CurrentOrSelectedTabId);
                    }

                    break;
                case SettingsMode.ModuleInstance:
                    {
                        // Delete all Module Instance Settings for the Current Tab
                        EditorController.DeleteAllModuleSettingsById(CurrentOrSelectedTabId);
                    }

                    break;
                default:
                    return;
            }

            ShowNotification(Localization.GetString("lblInfoDel.Text", ResXFile, LangCode), "success");
        }

        /// <summary>
        /// Copies the current Page Settings to all Child Pages
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CopyToAllChild_Click(object sender, EventArgs e)
        {
            var childTabs = TabController.GetTabsByParent(CurrentOrSelectedTabId, CurrentOrSelectedPortalId);

            foreach (var tab in childTabs)
            {
                // Sa Settings to tab
                SavePortalOrPageSettings(string.Format("DNNCKT#{0}#", tab.TabID));
            }

            // Finally Clear Cache
            EditorController.ClearEditorCache();

            ShowNotification(
                Localization.GetString("lblInfoCopyAll.Text", ResXFile, LangCode),
                "success");
        }

        /// <summary>
        /// Reloaded the Settings of the Selected Mode
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SetMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindUserGroupsGridView();

            LoadSettings(rBlSetMode.SelectedIndex, false);
        }

        /// <summary>
        /// Shows the info notification.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        private void ShowNotification(string message, string type)
        {
            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                string.Format("notification_{0}", Guid.NewGuid()),
                string.Format(
                    "ShowNotificationBar('{0}','{1}','{2}');",
                    message,
                    type,
                    ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.5.3/images/")),
                true);
        }

        /// <summary>
        /// Renders the URL controls.
        /// </summary>
        /// <param name="reloadControls">if set to <c>true</c> [reload controls].</param>
        private void RenderUrlControls(bool reloadControls = false)
        {
            // Assign Url Controls on the Page the Correct Portal Id
            ConfigUrl.PortalId = _portalSettings.PortalId;
            TemplUrl.PortalId = _portalSettings.PortalId;
            CustomJsFile.PortalId = _portalSettings.PortalId;
            CssUrl.PortalId = _portalSettings.PortalId;
            ImportFile.PortalId = _portalSettings.PortalId;

            if (!reloadControls)
            {
                return;
            }

            TemplUrl.ReloadFiles = true;
            ConfigUrl.ReloadFiles = true;
            CustomJsFile.ReloadFiles = true;
            CssUrl.ReloadFiles = true;
            ImportFile.ReloadFiles = true;
        }

        /// <summary>
        /// Exports the settings.
        /// </summary>
        /// <returns>Returns the exported EditorProviderSettings</returns>
        private EditorProviderSettings ExportSettings()
        {
            var exportSettings = new EditorProviderSettings { SettingMode = SettingsMode.Default };

            exportSettings.SettingMode = CurrentSettingsMode;

            // Export all Editor config settings
            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        {
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, textBox.Text, null);
                            }
                        }

                        break;
                    case "Int32":
                        {
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, int.Parse(textBox.Text), null);
                            }
                        }

                        break;
                    case "Decimal":
                        {
                            var textBox = Utility.FindControl<TextBox>(EditorConfigHolder, info.Name);

                            if (!string.IsNullOrEmpty(textBox.Text))
                            {
                                info.SetValue(exportSettings.Config, decimal.Parse(textBox.Text), null);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(EditorConfigHolder, info.Name);

                            info.SetValue(exportSettings.Config, checkBox.Checked, null);
                        }

                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        {
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

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
                            var dropDownList = Utility.FindControl<DropDownList>(EditorConfigHolder, info.Name);

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
                                                EditorConfigHolder,
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
                                                EditorConfigHolder,
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
                                                EditorConfigHolder,
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
                                                EditorConfigHolder,
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

            exportSettings.Config.Skin = ddlSkin.SelectedValue;
            exportSettings.Config.CodeMirror.Theme = CodeMirrorTheme.SelectedValue;
            exportSettings.Browser = ddlBrowser.SelectedValue;
            exportSettings.FileListViewMode =
                (FileListView)Enum.Parse(typeof(FileListView), FileListViewMode.SelectedValue);
            exportSettings.DefaultLinkMode = (LinkMode)Enum.Parse(typeof(LinkMode), DefaultLinkMode.SelectedValue);
            exportSettings.UseAnchorSelector = UseAnchorSelector.Checked;
            exportSettings.ShowPageLinksTabFirst = ShowPageLinksTabFirst.Checked;
            exportSettings.OverrideFileOnUpload = OverrideFileOnUpload.Checked;
            exportSettings.SubDirs = cbBrowserDirs.Checked;
            exportSettings.BrowserRootDirId = int.Parse(BrowserRootDir.SelectedValue);
            exportSettings.UploadDirId = int.Parse(UploadDir.SelectedValue);

            if (Utility.IsNumeric(FileListPageSize.Text))
            {
                exportSettings.FileListPageSize = int.Parse(FileListPageSize.Text);
            }

            if (Utility.IsNumeric(txtResizeWidth.Text))
            {
                exportSettings.ResizeWidth = int.Parse(txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(txtResizeHeight.Text))
            {
                exportSettings.ResizeHeight = int.Parse(txtResizeHeight.Text);
            }

            exportSettings.InjectSyntaxJs = InjectSyntaxJs.Checked;

            if (Utility.IsUnit(txtWidth.Text))
            {
                exportSettings.EditorWidth = txtWidth.Text;
            }

            if (Utility.IsUnit(txtHeight.Text))
            {
                exportSettings.EditorHeight = txtHeight.Text;
            }

            exportSettings.BlankText = txtBlanktext.Text;
            exportSettings.Config.ContentsCss = CssUrl.Url;
            exportSettings.Config.Templates_Files = TemplUrl.Url;
            exportSettings.CustomJsFile = CustomJsFile.Url;
            exportSettings.Config.CustomConfig = ConfigUrl.Url;

            string sRoles = chblBrowsGr.Items.Cast<ListItem>()
                .Where(item => item.Selected)
                .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                exportSettings.BrowserRoles = sRoles;
            }

            var listToolbarRoles = new List<ToolbarRoles>();

            // Save Toolbar Setting for every Role
            for (int i = 0; i < gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = ddLToolB.SelectedValue });
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    listToolbarRoles.Add(new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = ddLToolB.SelectedValue });
                }
            }

            exportSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = new List<UploadSizeRoles>();

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    listUploadSizeRoles.Add(
                        new UploadSizeRoles { RoleId = -1, UploadFileLimit = Convert.ToInt32(sizeLimit.Text) });
                }
                else
                {
                    RoleInfo objRole = RoleController.Instance.GetRoleByName(_portalSettings.PortalId, label.Text);

                    listUploadSizeRoles.Add(
                        new UploadSizeRoles
                            {
                                RoleId = objRole.RoleID,
                                UploadFileLimit = Convert.ToInt32(sizeLimit.Text)
                            });
                }
            }

            exportSettings.UploadSizeRoles = listUploadSizeRoles;

            return exportSettings;
        }

        #endregion
    }
}
