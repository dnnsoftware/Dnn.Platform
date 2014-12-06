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

namespace WatchersNET.CKEditor
{
    #region

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

    using WatchersNET.CKEditor.Constants;
    using WatchersNET.CKEditor.Controls;
    using WatchersNET.CKEditor.Extensions;
    using WatchersNET.CKEditor.Objects;
    using WatchersNET.CKEditor.Utilities;

    using DataCache = DotNetNuke.Common.Utilities.DataCache;
    using Globals = DotNetNuke.Common.Globals;

    #endregion

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
        ///   The role controller.
        /// </summary>
        private readonly RoleController objRoleController = new RoleController();

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
                return this.ViewState["IsHostMode"] != null && (bool)this.ViewState["IsHostMode"];
            }

            set
            {
                this.ViewState["IsHostMode"] = value;
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
                return this.ViewState["CurrentPortalOnly"] != null && (bool)this.ViewState["CurrentPortalOnly"];
            }

            set
            {
                this.ViewState["CurrentPortalOnly"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the Current or selected Tab ID.
        /// </summary>
        public int CurrentOrSelectedTabId
        {
            get
            {
                var o = this.ViewState["CurrentTabId"];
                if (o != null)
                {
                    return (int)o;
            }

                return 1;
            }

            set
            {
                this.ViewState["CurrentTabId"] = value;
            }
        }

        /// <summary>
        ///   Gets or sets the Current or selected Portal ID.
        /// </summary>
        public int CurrentOrSelectedPortalId
        {
            get
            {
                return this.ViewState["CurrentPortalId"] != null ? (int)this.ViewState["CurrentPortalId"] : 0;
            }

            set
            {
                this.ViewState["CurrentPortalId"] = value;
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
                return this.ViewState["DefaultHostLoadMode"] != null ? (int)this.ViewState["DefaultHostLoadMode"] : 0;
            }

            set
            {
                this.ViewState["DefaultHostLoadMode"] = value;
            }
        }

        /// <summary>
        ///   Gets Current Language from Url
        /// </summary>
        protected string LangCode
        {
            get
            {
               return !string.IsNullOrEmpty(this.request.QueryString["langCode"])
                           ? this.request.QueryString["langCode"]
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
                    this.ResolveUrl(
                        string.Format(
                            "~/Providers/HtmlEditorProviders/CKEditor/{0}/Options.aspx.resx",
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
                return (SettingsMode)this.ViewState["CurrentSettingsMode"];
            }

            set
            {
                this.ViewState["CurrentSettingsMode"] = value;
            }
        }

        /// <summary>
        ///   Gets the Config Url Control.
        /// </summary>
        private UrlControl ConfigUrl
        {
            get
            {
                return this.ctlConfigUrl;
            }
        }

        /// <summary>
        ///   Gets the CSS Url Control.
        /// </summary>
        private UrlControl CssUrl
        {
            get
            {
                return this.ctlCssurl;
            }
        }

        /// <summary>
        ///   Gets the Import File Url Control.
        /// </summary>
        private UrlControl ImportFile
        {
            get
            {
                return this.ctlImportFile;
            }
        }

        /// <summary>
        ///   Gets the Template Url Control.
        /// </summary>
        private UrlControl TemplUrl
        {
            get
            {
                return this.ctlTemplUrl;
            }
        }

        /// <summary>
        ///   Gets the Custom JS File Url Control.
        /// </summary>
        private UrlControl CustomJsFile
        {
            get
            {
                return this.ctlCustomJsFile;
            }
        }

        private ModuleInfo _currentModule;
        private ModuleInfo CurrentModule
        {
            get
            {
                if (this._currentModule != null)
                {
                    return this._currentModule;
                }

                if (this.ModuleConfiguration != null && !Null.IsNull(this.ModuleConfiguration.ModuleID))
                {
                    this._currentModule = this.ModuleConfiguration;
                    return this._currentModule;
                }

                this._currentModule = new ModuleController().GetModule(
                    this.Request.QueryString.GetValueOrDefault("ModuleId", -1),
                    this.TabId,
                    false);

                return this._currentModule;
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
            if (this.IsHostMode)
            {
                if (!Page.IsPostBack)
                {
                    this.LastTabId.Value = "0";
                }

                this._portalSettings = this.GetPortalSettings();

                this.FillFolders();

                this.RenderUrlControls(true);

                this.FillRoles();

                this.BindUserGroupsGridView();

                this.lblSetFor.Visible = false;
                this.rBlSetMode.Visible = false;
                this.lnkRemoveAll.Visible = false;
                this.InfoTabLi.Visible = false;
                this.InfoTabHolder.Visible = false;
                this.btnCancel.Visible = false;

                if (this.DefaultHostLoadMode.Equals(0))
                {
                    this.lblSettings.Text = string.Format(
                        "{0} - <em>{1} {2} - Portal ID: {3}</em>",
                        Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode),
                        Localization.GetString("lblPortal.Text", this.ResXFile, this.LangCode),
                        this._portalSettings.PortalName,
                        this.CurrentOrSelectedPortalId);
                }
                else if (this.DefaultHostLoadMode.Equals(1))
                {
                    this.lblSettings.Text = string.Format(
                        "{0} - <em>{1} {2} - TabID: {3}</em>",
                        Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode),
                        Localization.GetString("lblPage.Text", this.ResXFile, this.LangCode),
                        new TabController().GetTab(this.CurrentOrSelectedTabId, this._portalSettings.PortalId, false)
                                           .TabName,
                        this.CurrentOrSelectedTabId);
                }
                else
                {
                    this.lblSettings.Text = Localization.GetString("lblSettings.Text", this.ResXFile, this.LangCode);
                }

                this.LoadSettings(this.DefaultHostLoadMode);
            }
            else
            {
                var pageKey = string.Format("DNNCKT#{0}#", this.CurrentOrSelectedTabId);
                var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", this.ModuleId, this.moduleInstanceName);

                if (SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.ModuleId))
                {
                    this.LoadSettings(2);
                }
                else
                {
                    var settingsDictionary = Utility.GetEditorHostSettings();

                    this.LoadSettings(SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey) ? 1 : 0);
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
            this.InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            this.AddJavaScripts();

            if (Utility.IsInRoles(this._portalSettings.AdministratorRoleName, this._portalSettings))
            {
                if (this.Page.IsPostBack)
                {
                    return;
                }

                this.SetLanguage();

                this.FillInformations();

                // Load Skin List
                this.FillSkinList();

                this.FillFolders();

                this.RenderUrlControls();

                this.FillRoles();

                this.BindUserGroupsGridView();

                this.BindOptionsData();

                // Remove CKFinder from the Browser list if not installed
                if (
                    !File.Exists(
                        this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.js")))
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
                    string.Format(
                        "javascript:alert('{0}');self.close();",
                        Localization.GetString("Error1.Text", this.ResXFile, this.LangCode)),
                    true);
            }
        }

        /// <summary>
        /// Adds the Java scripts.
        /// </summary>
        private void AddJavaScripts()
        {
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/css/jquery.notification.css"));

            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/css/Options.css"));

            JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn_dom);
            JavaScript.RequestRegistration(CommonJs.jQuery);
            JavaScript.RequestRegistration(CommonJs.jQueryMigrate);
            JavaScript.RequestRegistration(CommonJs.jQueryUI);
            JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "jquery.notification",
                this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/js/jquery.notification.js"));

            ScriptManager.RegisterClientScriptInclude(
                this,
                typeof(Page),
                "OptionsJs",
                this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/js/Options.js"));
        }



        /// <summary>
        /// Import Current Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Import_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(this.ImportFile.Url))
            {
                return;
            }

            string sXmlImport = this.ImportFile.Url;

            this.upOptions.Update();

            // RESET Dialog 
            this.ImportFile.Url = null;

            int imageFileId = int.Parse(sXmlImport.Substring(7));

            // FileInfo objFileInfo = objFileController.GetFileById(imageFileId, this._portalSettings.PortalId);
            var objFileInfo = FileManager.Instance.GetFile(imageFileId);

            sXmlImport = this._portalSettings.HomeDirectoryMapPath + objFileInfo.Folder + objFileInfo.FileName;

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

        /// <summary>
        /// Export Current Settings
        /// </summary>
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
                                      : string.Format("CKEditorSettings-{0}.xml", exportSettings.SettingMode);

                if (!xmlFileName.EndsWith(".xml"))
                {
                    xmlFileName += ".xml";
                }

                var exportFolderInfo = FolderManager.Instance.GetFolder(Convert.ToInt32(this.ExportDir.SelectedValue));

                var textWriter = this.ExportDir.SelectedValue.Equals("-1")
                                     ? new StreamWriter(
                                           Path.Combine(this._portalSettings.HomeDirectoryMapPath, xmlFileName))
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

            this.FillSettings(importedSettings, changeMode);
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
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                textBox.Text = value.ToString();
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

            this.FileListPageSize.Text = importedSettings.FileListPageSize.ToString();

            this.FileListViewMode.SelectedValue = importedSettings.FileListViewMode.ToString();
            this.DefaultLinkMode.SelectedValue = importedSettings.DefaultLinkMode.ToString();
            this.UseAnchorSelector.Checked = importedSettings.UseAnchorSelector;
            this.ShowPageLinksTabFirst.Checked = importedSettings.ShowPageLinksTabFirst;

            this.cbBrowserDirs.Checked = importedSettings.SubDirs;

            this.OverrideFileOnUpload.Checked = importedSettings.OverrideFileOnUpload;

            this.BrowserRootDir.SelectedValue =
                 this.BrowserRootDir.Items.FindByValue(importedSettings.BrowserRootDirId.ToString()) != null
                     ? importedSettings.BrowserRootDirId.ToString()
                     : "-1";

            this.UploadDir.SelectedValue = this.UploadDir.Items.FindByValue(importedSettings.UploadDirId.ToString())
                                           != null
                                               ? importedSettings.UploadDirId.ToString()
                                               : "-1";

            var configFolderInfo =
                Utility.ConvertFilePathToFolderInfo(
                    !string.IsNullOrEmpty(this.configFolder)
                        ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                        : this._portalSettings.HomeDirectoryMapPath,
                    this._portalSettings);

            this.ExportDir.SelectedValue = configFolderInfo != null
                                           &&
                                           this.ExportDir.Items.FindByValue(configFolderInfo.FolderID.ToString())
                                           != null
                                               ? configFolderInfo.FolderID.ToString()
                                               : "-1";

            this.ExportFileName.Text = string.Format("CKEditorSettings-{0}.xml", importedSettings.SettingMode);

            switch (importedSettings.SettingMode)
            {
                case SettingsMode.Portal:
                    this.ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, this._portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    this.ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, this.CurrentOrSelectedTabId);
                    break;
                case SettingsMode.ModuleInstance:
                    this.ExportFileName.Text = string.Format(
                        "CKEditorSettings-{0}-{1}.xml", importedSettings.SettingMode, this.ModuleId);
                    break;
            }

            this.txtResizeHeight.Text = importedSettings.ResizeWidth.ToString();

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

            if (!string.IsNullOrEmpty(importedSettings.BlankText))
            {
                this.txtBlanktext.Text = importedSettings.BlankText;
            }

            List<ToolbarRoles> imporToolbarRoles = importedSettings.ToolBarRoles;

            // Load Toolbar Setting for Each Portal Role
            foreach (ToolbarRoles objToolbRoles in imporToolbarRoles)
            {
                if (objToolbRoles.RoleId.Equals(-1))
                {
                    for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
                    {
                        Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals("Unauthenticated Users"))
                        {
                            continue;
                        }

                        DropDownList ddLToolB =
                            (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                        ddLToolB.ClearSelection();

                        if (ddLToolB.Items.FindByValue(objToolbRoles.Toolbar) != null)
                        {
                            ddLToolB.SelectedValue = objToolbRoles.Toolbar;
                        }
                    }
                }
                else
                {
                    RoleInfo objRole = this.objRoleController.GetRole(
                        objToolbRoles.RoleId, this._portalSettings.PortalId);

                    if (objRole == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
                    {
                        Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals(objRole.RoleName))
                        {
                            continue;
                        }

                        DropDownList ddLToolB =
                            (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                        ddLToolB.ClearSelection();

                        if (ddLToolB.Items.FindByValue(objToolbRoles.Toolbar) != null)
                        {
                            ddLToolB.SelectedValue = objToolbRoles.Toolbar;
                        }
                    }
                }
            }

            var imporUploadSizeRoles = importedSettings.UploadSizeRoles;

            // Load Upload Size Setting for Each Portal Role
            foreach (var uploadSizeRole in imporUploadSizeRoles)
            {
                if (uploadSizeRole.RoleId.Equals(-1))
                {
                    for (int i = 0; i < this.UploadFileLimits.Rows.Count; i++)
                    {
                        Label label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                        if (label == null || !label.Text.Equals("Unauthenticated Users"))
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
                    RoleInfo objRole = this.objRoleController.GetRole(
                        uploadSizeRole.RoleId, this._portalSettings.PortalId);

                    if (objRole == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < this.UploadFileLimits.Rows.Count; i++)
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

        /// <summary>
        /// Bind User Groups to GridView
        /// </summary>
        private void BindUserGroupsGridView()
        {
            var lic = new ListItemCollection();

            foreach (var roleItem in
                from RoleInfo objRole in this.objRoleController.GetPortalRoles(this._portalSettings.PortalId)
                select new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() })
            {
                lic.Add(roleItem);
            }

            lic.Add(new ListItem { Text = "Unauthenticated Users", Value = "-1" });

            this.gvToolbars.DataSource = lic;
            this.gvToolbars.DataBind();

            this.InsertToolbars();

            var lblRole = (Label)this.gvToolbars.HeaderRow.FindControl("lblRole");
            var lblSelToolb = (Label)this.gvToolbars.HeaderRow.FindControl("lblSelToolb");

            lblRole.Text = Localization.GetString("lblRole.Text", this.ResXFile, this.LangCode);
            lblSelToolb.Text = Localization.GetString("lblSelToolb.Text", this.ResXFile, this.LangCode);

            // Bind User Groups to UploadFileLimits GridView
            this.UploadFileLimits.DataSource = lic;
            this.UploadFileLimits.DataBind();

            lblRole = (Label)this.UploadFileLimits.HeaderRow.FindControl("lblRole");
            lblSelToolb = (Label)this.UploadFileLimits.HeaderRow.FindControl("SizeLimitLabel");

            lblRole.Text = Localization.GetString("lblRole.Text", this.ResXFile, this.LangCode);
            lblSelToolb.Text = Localization.GetString("SizeLimitLabel.Text", this.ResXFile, this.LangCode);
        }

        /// <summary>
        /// Delete Settings only for this Module Instance
        /// </summary>
        private void DelModuleSettings()
        {
            this.moduleInstanceName = this.request.QueryString["minc"];
            string moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", this.ModuleId, this.moduleInstanceName);

            var moduleController = new ModuleController();

            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
            {
                moduleController.DeleteModuleSetting(this.ModuleId, string.Format("{0}{1}", moduleKey, info.Name));
            }

            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SKIN));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CODEMIRRORTHEME));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BROWSER));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.FILELISTPAGESIZE));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.FILELISTVIEWMODE));
            moduleController.DeleteModuleSetting(
                 this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.DEFAULTLINKMODE));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.USEANCHORSELECTOR));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SHOWPAGELINKSTABFIRST));
            moduleController.DeleteModuleSetting(
               this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.OVERRIDEFILEONUPLOAD));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.SUBDIRS));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BROWSERROOTDIRID));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.UPLOADDIRID));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.INJECTJS));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.WIDTH));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.HEIGHT));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.BLANKTEXT));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CSS));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.TEMPLATEFILES));
            moduleController.DeleteModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", moduleKey, SettingConstants.CUSTOMJSFILE));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.CONFIG));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.ROLES));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.RESIZEHEIGHT));
            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{1}", moduleKey, SettingConstants.RESIZEWIDTH));

            foreach (RoleInfo objRole in this.objRoleController.GetPortalRoles(this._portalSettings.PortalId))
            {
                moduleController.DeleteModuleSetting(
                    this.ModuleId, string.Format("{0}{2}#{1}", moduleKey, objRole.RoleID, SettingConstants.TOOLB));
            }

            moduleController.DeleteModuleSetting(
                this.ModuleId, string.Format("{0}{2}#{1}", moduleKey, "-1", SettingConstants.TOOLB));

            // Finally Clear Cache
            DataCache.RemoveCache("CKEditorHost");
        }

        /// <summary>
        /// Write Information
        /// </summary>
        private void FillInformations()
        {
            var ckEditorPackage = PackageController.GetPackageByName("DotNetNuke.CKHtmlEditorProvider");

            if (ckEditorPackage != null)
            {
                this.ProviderVersion.Text += ckEditorPackage.Version;
            }

            this.lblPortal.Text += this._portalSettings.PortalName;

            ModuleDefinitionInfo moduleDefinitionInfo;
            var moduleInfo = new ModuleController().GetModuleByDefinition(
                this._portalSettings.PortalId, "User Accounts");

            try
            {
                moduleDefinitionInfo =
                    ModuleDefinitionController.GetModuleDefinitionByID(this.CurrentModule.ModuleDefID);
            }
            catch (Exception)
            {
                moduleDefinitionInfo = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                    "User Accounts", moduleInfo.DesktopModuleID);
            }

            try
            {
                this.lblPage.Text += string.Format(
                    "{0} - TabID {1}",
                    new TabController().GetTab(this.CurrentOrSelectedTabId, this._portalSettings.PortalId, false).TabName,
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

            try
            {
                this.lblModName.Text += this.CurrentModule.ModuleTitle;
            }
            catch (Exception)
            {
                this.lblModName.Text += moduleInfo.ModuleTitle;
            }

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

        /// <summary>
        /// Loads all DNN Roles
        /// </summary>
        private void FillRoles()
        {
            this.chblBrowsGr.Items.Clear();

            foreach (RoleInfo objRole in this.objRoleController.GetPortalRoles(this._portalSettings.PortalId))
            {
                ListItem roleItem = new ListItem { Text = objRole.RoleName, Value = objRole.RoleID.ToString() };

                if (objRole.RoleName.Equals(this.PortalSettings.AdministratorRoleName))
                {
                    roleItem.Selected = true;
                    roleItem.Enabled = false;
                }

                this.chblBrowsGr.Items.Add(roleItem);
            }
        }

        // Reload Settings based on the Selected Mode

        /// <summary>
        /// Loads the List of available Skins.
        /// </summary>
        private void FillSkinList()
        {
            this.ddlSkin.Items.Clear();

            DirectoryInfo objDir =
                new DirectoryInfo(this.MapPath(this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/skins")));

            foreach (ListItem skinItem in
                objDir.GetDirectories().Select(
                    objSubFolder => new ListItem { Text = objSubFolder.Name, Value = objSubFolder.Name }))
            {
                this.ddlSkin.Items.Add(skinItem);
            }

            // CodeMirror Themes
            this.CodeMirrorTheme.Items.Clear();

            var themesFolder =
                new DirectoryInfo(
                    this.MapPath(this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/plugins/codemirror/theme")));

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

        /// <summary>
        /// Loads the List of available Folders.
        /// </summary>
        private void FillFolders()
        {
            this.UploadDir.Items.Clear();
            this.BrowserRootDir.Items.Clear();
            this.ExportDir.Items.Clear();

            foreach (FolderInfo folder in FolderManager.Instance.GetFolders(this._portalSettings.PortalId))
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

                if (!FolderPermissionController.CanViewFolder(folder))
                {
                    continue;
                }

                this.UploadDir.Items.Add(new ListItem(text, value));
                this.BrowserRootDir.Items.Add(new ListItem(text, value));
                this.ExportDir.Items.Add(new ListItem(text, value));
            }

            this.UploadDir.SelectedValue = "-1";
            this.BrowserRootDir.SelectedValue = "-1";
            this.ExportDir.SelectedValue = "-1";
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
                if (this.IsHostMode && this.CurrentPortalOnly)
                {
                    return this.PortalSettings;
                }

                if (!this.IsHostMode && this.request.QueryString["tid"] != null)
                {
                    this.CurrentOrSelectedTabId = int.Parse(this.request.QueryString["tid"]);
                }

                if (!this.IsHostMode && this.request.QueryString["PortalID"] != null)
                {
                    this.CurrentOrSelectedPortalId = int.Parse(this.request.QueryString["PortalID"]);
                }

                var domainName = Globals.GetDomainName(this.Request, true);

                var portalAlias = PortalAliasController.GetPortalAliasByPortal(this.CurrentOrSelectedPortalId, domainName);

                portalSettings = new PortalSettings(
                    this.CurrentOrSelectedTabId, PortalAliasController.GetPortalAliasInfo(portalAlias));
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
            bool bHideAll = !this.dDlToolbarPrio.Items.Cast<ListItem>().Any(item => item.Enabled);

            if (bHideAll)
            {
                this.iBAdd.Visible = false;
            }
        }

        /// <summary>
        /// Add new/Save Toolbar Set
        /// </summary>
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

                ToolbarUtil.SaveToolbarSets(
                    this.listToolbars,
                    !string.IsNullOrEmpty(this.configFolder)
                        ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                        : this._portalSettings.HomeDirectoryMapPath);

                this.ShowNotification(
                    Localization.GetString("ToolbarSetSaved.Text", this.ResXFile, this.LangCode),
                    "success");
            }
            else
            {
                // Add New Toolbar Set
                var newToolbar = new ToolbarSet(this.dnnTxtToolBName.Text, int.Parse(this.dDlToolbarPrio.SelectedValue))
                    {
                        ToolbarGroups = modifiedSet.ToolbarGroups
                    };

                this.listToolbars.Add(newToolbar);

                ToolbarUtil.SaveToolbarSets(
                    this.listToolbars,
                    !string.IsNullOrEmpty(this.configFolder)
                        ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                        : this._portalSettings.HomeDirectoryMapPath);

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

        /// <summary>
        /// Cancel Edit Toolbar
        /// </summary>
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

        /// <summary>
        /// Delete Selected Toolbar Set
        /// </summary>
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

            ToolbarUtil.SaveToolbarSets(
                this.listToolbars,
                !string.IsNullOrEmpty(this.configFolder)
                    ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                    : this._portalSettings.HomeDirectoryMapPath);

            this.BindUserGroupsGridView();

            this.dnnTxtToolBName.Enabled = true;

            this.iBAdd.ImageUrl = this.ResolveUrl("~/images/add.gif");

            this.iBAdd.AlternateText = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);
            this.iBAdd.ToolTip = Localization.GetString("AddToolbar.Text", this.ResXFile, this.LangCode);

            this.ShowNotification(
                Localization.GetString("ToolbarSetDeleted.Text", this.ResXFile, this.LangCode),
                "success");
        }

        /// <summary>
        /// Edit Selected Toolbar
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void IbEditClick(object sender, ImageClickEventArgs e)
        {
            if (this.dDlCustomToolbars.SelectedValue == null)
            {
                return;
            }

            this.iBAdd.Visible = true;

            var toolbarEdit =
                this.listToolbars.Find(toolbarSel => toolbarSel.Name.Equals(this.dDlCustomToolbars.SelectedValue));

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
                    priority = string.Format("0{0}", priority);
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
                this._portalSettings = this.GetPortalSettings();

                this.btnCancel.Click += this.Cancel_Click;
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

                if (objProvider != null && !string.IsNullOrEmpty(objProvider.Attributes["ck_configFolder"]))
                {
                    this.configFolder = objProvider.Attributes["ck_configFolder"];
                }

                this.listToolbars = ToolbarUtil.GetToolbars(
                    this._portalSettings.HomeDirectoryMapPath, this.configFolder);

                this.rBlSetMode.SelectedIndexChanged += this.SetMode_SelectedIndexChanged;

                this.ToolbarGroupsRepeater.ItemDataBound += this.ToolbarGroupsRepeater_ItemDataBound; 
                
                this.RenderEditorConfigSettings();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex.Message, "error");
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

            if (this.listButtons == null)
            {
                this.listButtons =
                    ToolbarUtil.LoadToolBarButtons(
                        !string.IsNullOrEmpty(this.configFolder)
                            ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                            : this._portalSettings.HomeDirectoryMapPath);
            }

            foreach (var button in this.toolbarSets.ToolbarGroups.Where(@group => @group.name.Equals(groupName)).SelectMany(@group => @group.items))
            {
                if (this.listButtons == null)
                {
                    this.listButtons =
                        ToolbarUtil.LoadToolBarButtons(
                            !string.IsNullOrEmpty(this.configFolder)
                                ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                                : this._portalSettings.HomeDirectoryMapPath);
                }

                if (this.listButtons.Find(availButton => availButton.ToolbarName.Equals(button)) == null
                    && !button.Equals("/"))
                {
                    continue;
                }

                var groupRow = toolbarButtonsTable.NewRow();

                groupRow["Button"] = button;

                var buttonItem = this.listButtons.Find(b => b.ToolbarName.Equals(button));

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

            for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                DropDownList ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (ddLToolB == null)
                {
                    continue;
                }

                ddLToolB.DataSource = licToolbars;
                ddLToolB.DataBind();
            }

            this.dDlCustomToolbars.DataSource = licToolbars;
            this.dDlCustomToolbars.DataBind();

            List<string> excludeButtons;

            var emptyToolbarSet = new ToolbarSet();

            // Empty Toolbar
            emptyToolbarSet.ToolbarGroups.Add(
                new ToolbarGroup { name = Localization.GetString("NewGroupName.Text", this.ResXFile, this.LangCode) });

            this.FillToolbarGroupsRepeater(emptyToolbarSet, out excludeButtons);

            // Load Toolbar Buttons
            this.FillAvailableToolbarButtons(null);
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

        /// <summary>
        /// Fills the available toolbar buttons.
        /// </summary>
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
                            ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                            : this._portalSettings.HomeDirectoryMapPath);
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

        /// <summary>
        /// Load Default Host Settings from 'web.config'
        /// </summary>
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
            this.CurrentSettingsMode = (SettingsMode)Enum.Parse(typeof(SettingsMode), currentMode.ToString());

            this.lnkRemoveAll.Visible = !currentMode.Equals(0);
            this.lnkRemoveChild.Visible = !currentMode.Equals(0);
            this.CopyToAllChild.Visible = !currentMode.Equals(0);

            this.lnkRemove.Text = string.Format(
                Localization.GetString("Remove.Text", this.ResXFile, this.LangCode),
                this.rBlSetMode.Items[currentMode].Text);
            this.lnkRemoveAll.Text =
                string.Format(
                    Localization.GetString("RemoveAll.Text", this.ResXFile, this.LangCode),
                    this.rBlSetMode.Items[currentMode].Text);

            this.lnkRemove.ToolTip = string.Format(
                Localization.GetString("Remove.Help", this.ResXFile, this.LangCode),
                this.rBlSetMode.Items[currentMode].Text);
            this.lnkRemoveAll.ToolTip =
                string.Format(
                    Localization.GetString("RemoveAll.Help", this.ResXFile, this.LangCode),
                    this.rBlSetMode.Items[currentMode].Text);

            this.LoadDefaultSettings();

            var settingsDictionary = Utility.GetEditorHostSettings();
            var portalRoles = new RoleController().GetPortalRoles(this._portalSettings.PortalId);

            var portalKey = string.Format("DNNCKP#{0}#", this._portalSettings.PortalId);
            var pageKey = string.Format("DNNCKT#{0}#", this.CurrentOrSelectedTabId);
            var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", this.ModuleId, this.moduleInstanceName);

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            var currentSettings = SettingsUtil.GetDefaultSettings(
                this._portalSettings,
                this._portalSettings.HomeDirectoryMapPath,
                objProvider.Attributes["ck_configFolder"],
                portalRoles);

            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    {
                        // Load Portal Settings ?!
                        if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, portalKey))
                        {
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                                this._portalSettings, currentSettings, settingsDictionary, portalKey, portalRoles);

                            // Set Current Mode to Portal
                            currentSettings.SettingMode = SettingsMode.Portal;

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
                        if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey))
                        {
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                                this._portalSettings, currentSettings, settingsDictionary, pageKey, portalRoles);

                            // Set Current Mode to Page
                            currentSettings.SettingMode = SettingsMode.Page;

                            this.lnkRemove.Enabled = true;
                        }
                        else
                        {
                            this.lnkRemove.Enabled = false;
                        }

                        var currentTab = new TabController().GetTab(
                            this.CurrentOrSelectedTabId, this._portalSettings.PortalId, false);

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
                            currentSettings = new EditorProviderSettings();

                            currentSettings = SettingsUtil.LoadModuleSettings(
                                this._portalSettings, currentSettings, moduleKey, this.ModuleId, portalRoles);

                            currentSettings.SettingMode = SettingsMode.ModuleInstance;

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

            if (currentSettings != null)
            {
                this.FillSettings(currentSettings, changeMode);
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

            return string.Format("FileID={0}", Utility.ConvertFilePathToFileId(inputUrl, this._portalSettings.PortalId));
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

        /// <summary>
        /// Save Settings only for this Module Instance
        /// </summary>
        private void SaveModuleSettings()
        {
            this.moduleInstanceName = this.request.QueryString["minc"];
            string key = string.Format("DNNCKMI#{0}#INS#{1}#", this.ModuleId, this.moduleInstanceName);

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
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                moduleController.UpdateModuleSetting(
                                    this.ModuleId, string.Format("{0}{1}", key, info.Name), textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                moduleController.UpdateModuleSetting(
                                    this.ModuleId, string.Format("{0}{1}", key, info.Name), checkBox.Checked.ToString());
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
                                if (dropDownList.SelectedItem != null)
                                {
                                    moduleController.UpdateModuleSetting(
                                        this.ModuleId, string.Format("{0}{1}", key, info.Name), dropDownList.SelectedValue);
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
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    this.ModuleId, string.Format("{0}{1}", key, codeMirrorInfo.Name), textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, codeMirrorInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    this.ModuleId,
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
                                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    this.ModuleId, string.Format("{0}{1}", key, wordCountInfo.Name), textBox.Text);
                                            }
                                        }

                                        break;

                                    case "Boolean":
                                        {
                                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, wordCountInfo.Name);

                                            if (checkBox != null)
                                            {
                                                moduleController.UpdateModuleSetting(
                                                    this.ModuleId,
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
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.SKIN), this.ddlSkin.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME),
                this.CodeMirrorTheme.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.BROWSER), this.ddlBrowser.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE),
                this.FileListViewMode.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE),
                this.DefaultLinkMode.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR),
                this.UseAnchorSelector.Checked.ToString());
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST),
                this.ShowPageLinksTabFirst.Checked.ToString());
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD),
                this.OverrideFileOnUpload.Checked.ToString());
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.SUBDIRS),
                this.cbBrowserDirs.Checked.ToString());
            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID),
                this.BrowserRootDir.SelectedValue);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID), this.UploadDir.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId,
                    string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE),
                    this.FileListPageSize.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidth.Text))
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId, string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH), this.txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeight.Text))
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId,
                    string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT),
                    this.txtResizeHeight.Text);
            }

            moduleController.UpdateModuleSetting(
                this.ModuleId,
                string.Format("{0}{1}", key, SettingConstants.INJECTJS),
                this.InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(this.txtWidth.Text))
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId, string.Format("{0}{1}", key, SettingConstants.WIDTH), this.txtWidth.Text);
            }

            if (Utility.IsUnit(this.txtHeight.Text))
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId, string.Format("{0}{1}", key, SettingConstants.HEIGHT), this.txtWidth.Text);
            }

            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.BLANKTEXT), this.txtBlanktext.Text);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.CSS), this.CssUrl.Url);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES), this.TemplUrl.Url);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE), this.CustomJsFile.Url);
            moduleController.UpdateModuleSetting(
                this.ModuleId, string.Format("{0}{1}", key, SettingConstants.CONFIG), this.ConfigUrl.Url);

            string sRoles = this.chblBrowsGr.Items.Cast<ListItem>().Where(item => item.Selected).Aggregate(
                string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                moduleController.UpdateModuleSetting(
                    this.ModuleId, string.Format("{0}{1}", key, SettingConstants.ROLES), sRoles);
            }

            // Save Toolbar Setting for every Role
            for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
                else
                {
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB),
                        ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                Label label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
                else
                {
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

                    moduleController.UpdateModuleSetting(
                        this.ModuleId,
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
                            var textBox = Utility.FindControl<TextBox>(this.EditorConfigHolder, info.Name);

                            if (textBox != null)
                            {
                                Utility.AddOrUpdateEditorHostSetting(
                                    string.Format("{0}{1}", key, info.Name),
                                    textBox.Text);
                            }
                        }

                        break;
                    case "Boolean":
                        {
                            var checkBox = Utility.FindControl<CheckBox>(this.EditorConfigHolder, info.Name);

                            if (checkBox != null)
                            {
                                Utility.AddOrUpdateEditorHostSetting(
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
                            var dropDownList = Utility.FindControl<DropDownList>(this.EditorConfigHolder, info.Name);

                            if (dropDownList != null)
                            {
                                if (dropDownList.SelectedItem != null)
                                {
                                    Utility.AddOrUpdateEditorHostSetting(
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
                                                this.EditorConfigHolder,
                                                codeMirrorInfo.Name);

                                            if (textBox != null)
                                            {
                                                Utility.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, codeMirrorInfo.Name),
                                                    textBox.Text);
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
                                                Utility.AddOrUpdateEditorHostSetting(
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
                                                this.EditorConfigHolder,
                                                wordCountInfo.Name);

                                            if (textBox != null)
                                            {
                                                Utility.AddOrUpdateEditorHostSetting(
                                                    string.Format("{0}{1}", key, wordCountInfo.Name),
                                                    textBox.Text);
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
                                                Utility.AddOrUpdateEditorHostSetting(
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

            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SKIN),
                this.ddlSkin.SelectedValue);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME),
                this.CodeMirrorTheme.SelectedValue);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BROWSER),
                this.ddlBrowser.SelectedValue);

            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE),
                this.FileListViewMode.SelectedValue);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE),
                this.DefaultLinkMode.SelectedValue);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR),
                this.UseAnchorSelector.Checked.ToString());
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST),
                this.ShowPageLinksTabFirst.Checked.ToString());
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD),
                this.OverrideFileOnUpload.Checked.ToString());
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.SUBDIRS),
                this.cbBrowserDirs.Checked.ToString());
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID),
                this.BrowserRootDir.SelectedValue);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID),
                this.UploadDir.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                Utility.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE),
                    this.FileListPageSize.Text);
            }

            if (Utility.IsNumeric(this.txtResizeWidth.Text))
            {
                Utility.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH),
                    this.txtResizeWidth.Text);
            }

            if (Utility.IsNumeric(this.txtResizeHeight.Text))
            {
                Utility.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT),
                    this.txtResizeHeight.Text);
            }

            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}injectjs", key),
                this.InjectSyntaxJs.Checked.ToString());

            if (Utility.IsUnit(this.txtWidth.Text))
            {
                Utility.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.WIDTH),
                    this.txtWidth.Text);
            }

            if (Utility.IsUnit(this.txtHeight.Text))
            {
                Utility.AddOrUpdateEditorHostSetting(
                    string.Format("{0}{1}", key, SettingConstants.HEIGHT),
                    this.txtHeight.Text);
            }

            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.BLANKTEXT),
                this.txtBlanktext.Text);
            Utility.AddOrUpdateEditorHostSetting(string.Format("{0}{1}", key, SettingConstants.CSS), this.CssUrl.Url);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES),
                this.TemplUrl.Url);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE),
                this.CustomJsFile.Url);
            Utility.AddOrUpdateEditorHostSetting(
                string.Format("{0}{1}", key, SettingConstants.CONFIG),
                this.ConfigUrl.Url);

            string sRoles = this.chblBrowsGr.Items.Cast<ListItem>()
                .Where(item => item.Selected)
                .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                Utility.AddOrUpdateEditorHostSetting(string.Format("{0}{1}", key, SettingConstants.ROLES), sRoles);
            }

            // Save Toolbar Setting for every Role
            for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

                if (label == null || ddLToolB == null)
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    Utility.AddOrUpdateEditorHostSetting(
                        string.Format("{0}toolb#{1}", key, "-1"),
                        ddLToolB.SelectedValue);
                }
                else
                {
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

                    Utility.AddOrUpdateEditorHostSetting(
                        string.Format("{0}toolb#{1}", key, objRole.RoleID),
                        ddLToolB.SelectedValue);
                }
            }

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

                if (label == null || string.IsNullOrEmpty(sizeLimit.Text))
                {
                    continue;
                }

                if (label.Text.Equals("Unauthenticated Users"))
                {
                    Utility.AddOrUpdateEditorHostSetting(
                        string.Format("{0}{2}#{1}", key, "-1", SettingConstants.UPLOADFILELIMITS),
                        sizeLimit.Text);
                }
                else
                {
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

                    Utility.AddOrUpdateEditorHostSetting(
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
            ModuleInfo moduleInfo = db.GetModuleByDefinition(this._portalSettings.PortalId, "User Accounts");

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
                case SettingsMode.Portal:
                    this.SavePortalOrPageSettings(string.Format("DNNCKP#{0}#", this._portalSettings.PortalId));
                    break;
                case SettingsMode.Page:
                    this.SavePortalOrPageSettings(string.Format("DNNCKT#{0}#", this.CurrentOrSelectedTabId));
                    break;
                default:
                    if (this.CurrentSettingsMode.Equals(SettingsMode.ModuleInstance) && !objm.FriendlyName.Equals("User Accounts"))
                    {
                        this.SaveModuleSettings();
                    }

                    break;
            }

            // Finally Clear Cache
            DataCache.RemoveCache("CKEditorHost");
        }

        /// <summary>
        /// Set Current Language
        /// </summary>
        private void SetLanguage()
        {
            this.lblHeader.Text = Localization.GetString("lblHeader.Text", this.ResXFile, this.LangCode);

            this.ProviderVersion.Text = "<strong>WatchersNET CKEditor™ Provider</strong> ";

            this.lblPortal.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblPortal.Text", this.ResXFile, this.LangCode));
            this.lblPage.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblPage.Text", this.ResXFile, this.LangCode));
            this.lblModType.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModType.Text", this.ResXFile, this.LangCode));
            this.lblModName.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModName.Text", this.ResXFile, this.LangCode));
            this.lblModInst.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblModInst.Text", this.ResXFile, this.LangCode));
            this.lblUName.Text = string.Format(
                "<strong>{0}</strong> ", Localization.GetString("lblUName.Text", this.ResXFile, this.LangCode));
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
            this.EditorConfigWarning.Text = Localization.GetString(
                "EditorConfigWarning.Text", this.ResXFile, this.LangCode);

            this.FileListPageSizeLabel.Text = Localization.GetString(
                "FileListPageSizeLabel.Text", this.ResXFile, this.LangCode);
            this.FileListViewModeLabel.Text = Localization.GetString(
                "FileListViewModeLabel.Text", this.ResXFile, this.LangCode);
            this.lblUseAnchorSelector.Text = Localization.GetString(
                "lblUseAnchorSelector.Text", this.ResXFile, this.LangCode);
            this.lblShowPageLinksTabFirst.Text = Localization.GetString(
                "lblShowPageLinksTabFirst.Text", this.ResXFile, this.LangCode);

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
            this.btnCancel.Text = Localization.GetString("btnCancel.Text", this.ResXFile, this.LangCode);

            this.rBlSetMode.Items[0].Text = Localization.GetString("Portal.Text", this.ResXFile, this.LangCode);
            this.rBlSetMode.Items[1].Text = Localization.GetString("Page.Text", this.ResXFile, this.LangCode);

            if (this.rBlSetMode.Items.Count.Equals(3))
            {
                this.rBlSetMode.Items[2].Text = Localization.GetString(
                    "ModuleInstance.Text", this.ResXFile, this.LangCode);
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

        /// <summary>
        /// Exit Dialog
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ImageClickEventArgs" /> instance containing the event data.</param>
        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Page.ClientScript.RegisterStartupScript(
                this.GetType(), "closeScript", "javascript:self.close();", true);
        }

        /// <summary>
        /// Saves all Settings and Close Options
        /// </summary>
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

                ToolbarUtil.SaveToolbarSets(
                    this.listToolbars,
                    !string.IsNullOrEmpty(this.configFolder)
                        ? Path.Combine(this._portalSettings.HomeDirectoryMapPath, this.configFolder)
                        : this._portalSettings.HomeDirectoryMapPath);
            }

            this.ShowNotification(Localization.GetString("lblInfo.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>
        /// Remove Current selected Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Remove_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    Utility.DeleteAllPortalSettings(this._portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    Utility.DeleteCurrentPageSettings(this.CurrentOrSelectedTabId);
                    break;
                case SettingsMode.ModuleInstance:
                    this.DelModuleSettings();
                    break;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>
        /// Remove selected all Settings
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void RemoveAll_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Portal:
                    Utility.DeleteAllPortalSettings(this._portalSettings.PortalId);
                    break;
                case SettingsMode.Page:
                    Utility.DeleteAllPageSettings(this._portalSettings.PortalId);
                    break;
                case SettingsMode.ModuleInstance:
                    Utility.DeleteAllModuleSettings(this._portalSettings.PortalId);
                    break;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>
        /// Handles the Click event of the RemoveChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void RemoveChild_Click(object sender, EventArgs e)
        {
            switch (this.CurrentSettingsMode)
            {
                case SettingsMode.Page:
                    {
                        // Delete all Page Setting for all Child Tabs
                        Utility.DeleteAllChildPageSettings(this.CurrentOrSelectedTabId);
                    }

                    break;
                case SettingsMode.ModuleInstance:
                    {
                        // Delete all Module Instance Settings for the Current Tab
                        Utility.DeleteAllModuleSettingsById(this.CurrentOrSelectedTabId);
                    }

                    break;
                default:
                    return;
            }

            this.ShowNotification(Localization.GetString("lblInfoDel.Text", this.ResXFile, this.LangCode), "success");
        }

        /// <summary>
        /// Copies the current Page Settings to all Child Pages
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CopyToAllChild_Click(object sender, EventArgs e)
        {
            var childTabs = TabController.GetTabsByParent(this.CurrentOrSelectedTabId, this.CurrentOrSelectedPortalId);

            foreach (var tab in childTabs)
            {
                // Sa Settings to tab
                this.SavePortalOrPageSettings(string.Format("DNNCKT#{0}#", tab.TabID));
            }

            // Finally Clear Cache
            DataCache.RemoveCache("CKEditorHost");

            this.ShowNotification(
                Localization.GetString("lblInfoCopyAll.Text", this.ResXFile, this.LangCode),
                "success");
        }

        /// <summary>
        /// Reloaded the Settings of the Selected Mode
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SetMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindUserGroupsGridView();

            this.LoadSettings(this.rBlSetMode.SelectedIndex, false);
        }

        /// <summary>
        /// Shows the info notification.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="type">The type.</param>
        private void ShowNotification(string message, string type)
        {
            ScriptManager.RegisterStartupScript(
                this.Page,
                this.GetType(),
                string.Format("notification_{0}", Guid.NewGuid()),
                string.Format(
                    "ShowNotificationBar('{0}','{1}','{2}');",
                    message,
                    type,
                    this.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/images/")),
                true);
        }

        /// <summary>
        /// Renders the URL controls.
        /// </summary>
        /// <param name="reloadControls">if set to <c>true</c> [reload controls].</param>
        private void RenderUrlControls(bool reloadControls = false)
        {
            // Assign Url Controls on the Page the Correct Portal Id
            this.ConfigUrl.PortalId = this._portalSettings.PortalId;
            this.TemplUrl.PortalId = this._portalSettings.PortalId;
            this.CustomJsFile.PortalId = this._portalSettings.PortalId;
            this.CssUrl.PortalId = this._portalSettings.PortalId;
            this.ImportFile.PortalId = this._portalSettings.PortalId;

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

        /// <summary>
        /// Exports the settings.
        /// </summary>
        /// <returns>Returns the exported EditorProviderSettings</returns>
        private EditorProviderSettings ExportSettings()
        {
            var exportSettings = new EditorProviderSettings { SettingMode = SettingsMode.Default };

            exportSettings.SettingMode = this.CurrentSettingsMode;

            // Export all Editor config settings
            foreach (PropertyInfo info in
                SettingsUtil.GetEditorConfigProperties())
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
            exportSettings.FileListViewMode =
                (FileListView)Enum.Parse(typeof(FileListView), this.FileListViewMode.SelectedValue);
            exportSettings.DefaultLinkMode = (LinkMode)Enum.Parse(typeof(LinkMode), this.DefaultLinkMode.SelectedValue);
            exportSettings.UseAnchorSelector = this.UseAnchorSelector.Checked;
            exportSettings.ShowPageLinksTabFirst = this.ShowPageLinksTabFirst.Checked;
            exportSettings.OverrideFileOnUpload = this.OverrideFileOnUpload.Checked;
            exportSettings.SubDirs = this.cbBrowserDirs.Checked;
            exportSettings.BrowserRootDirId = int.Parse(this.BrowserRootDir.SelectedValue);
            exportSettings.UploadDirId = int.Parse(this.UploadDir.SelectedValue);

            if (Utility.IsNumeric(this.FileListPageSize.Text))
            {
                exportSettings.FileListPageSize = int.Parse(this.FileListPageSize.Text);
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

            string sRoles = this.chblBrowsGr.Items.Cast<ListItem>()
                .Where(item => item.Selected)
                .Aggregate(string.Empty, (current, item) => current + (item.Value + ";"));

            if (sRoles != string.Empty)
            {
                exportSettings.BrowserRoles = sRoles;
            }

            var listToolbarRoles = new List<ToolbarRoles>();

            // Save Toolbar Setting for every Role
            for (int i = 0; i < this.gvToolbars.Rows.Count; i++)
            {
                Label label = (Label)this.gvToolbars.Rows[i].Cells[0].FindControl("lblRoleName");

                DropDownList ddLToolB = (DropDownList)this.gvToolbars.Rows[i].Cells[1].FindControl("ddlToolbars");

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
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

                    listToolbarRoles.Add(new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = ddLToolB.SelectedValue });
                }
            }

            exportSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = new List<UploadSizeRoles>();

            // Save Upload File Limit Setting for every Role
            for (int i = 0; i < this.UploadFileLimits.Rows.Count; i++)
            {
                var label = (Label)this.UploadFileLimits.Rows[i].Cells[0].FindControl("lblRoleName");

                var sizeLimit = (TextBox)this.UploadFileLimits.Rows[i].Cells[1].FindControl("SizeLimit");

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
                    RoleInfo objRole = this.objRoleController.GetRoleByName(this._portalSettings.PortalId, label.Text);

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
