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

namespace WatchersNET.CKEditor.Web
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web;
    using System.Web.Script.Serialization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Xml.Serialization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.Web.Client;

    using WatchersNET.CKEditor.Constants;
    using WatchersNET.CKEditor.Extensions;
    using WatchersNET.CKEditor.Objects;
    using WatchersNET.CKEditor.Utilities;

    using Assembly = System.Reflection.Assembly;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    #endregion

    /// <summary>
    /// The CKEditor control.
    /// </summary>
    public class CKEditorControl : WebControl, IPostBackDataHandler
    {
        #region Constants and Fields

        /// <summary>
        /// The provider type.
        /// </summary>
        private const string ProviderType = "htmlEditor";

        /// <summary>
        /// Has MS Ajax Installed?
        /// </summary>
        private static bool? _hasMsAjax;

        /// <summary>
        /// The portal settings.
        /// </summary>
        private readonly PortalSettings _portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];

        /// <summary>
        /// Check if the Settings Collection 
        /// is Merged with all Settings
        /// </summary>
        private bool isMerged;

        /// <summary>
        /// The settings collection.
        /// </summary>
        private NameValueCollection _settings;

        /// <summary>
        /// Current Settings Base
        /// </summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>
        /// The tool bar name override
        /// </summary>
        private string toolBarNameOverride; // EL 20101006

        /// <summary>
        /// The parent module that contains the editor.
        /// </summary>
        private PortalModuleBase myParModule;

        /// <summary>
        /// The Parent Module ID
        /// </summary>
        private int parentModulId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CKEditorControl"/> class.
        /// </summary>
        public CKEditorControl()
        {
            this.LoadConfigSettings();

            this.Init += this.CKEditorInit;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether IsRendered.
        /// </summary>
        public bool IsRendered { get; private set; }

        /// <summary>
        /// Gets Settings.
        /// </summary>
        public NameValueCollection Settings
        {
            get
            {
                if (this.isMerged)
                {
                    return this._settings;
                }

                // Override local settings with attributes
                foreach (string key in this.Attributes.Keys)
                {
                    this._settings[key] = this.Attributes[key];
                }

                // Inject all Editor Config 
                foreach (
                    PropertyInfo info in
                        SettingsUtil.GetEditorConfigProperties())
                {
                    XmlAttributeAttribute xmlAttributeAttribute = null;
                    var settingValue = string.Empty;

                    if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount"))
                    {
                        if (info.GetValue(this.currentSettings.Config, null) == null)
                        {
                            continue;
                        }

                        var rawValue = info.GetValue(this.currentSettings.Config, null);

                        settingValue = info.PropertyType.Name.Equals("Double")
                                           ? Convert.ToDouble(rawValue)
                                                 .ToString(CultureInfo.InvariantCulture)
                                           : rawValue.ToString();

                        if (string.IsNullOrEmpty(settingValue))
                        {
                            continue;
                        }

                        xmlAttributeAttribute = info.GetCustomAttribute<XmlAttributeAttribute>(true);
                    }

                    if (info.PropertyType.Name == "Boolean")
                    {
                        this._settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                    }
                    else
                    {
                        switch (info.Name)
                        {
                            case "ToolbarLocation":
                                this._settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                                break;
                            case "EnterMode":
                            case "ShiftEnterMode":
                                switch (settingValue)
                                {
                                    case "P":
                                        this._settings[xmlAttributeAttribute.AttributeName] = "1";
                                        break;
                                    case "BR":
                                        this._settings[xmlAttributeAttribute.AttributeName] = "2";
                                        break;
                                    case "DIV":
                                        this._settings[xmlAttributeAttribute.AttributeName] = "3";
                                        break;
                                } 

                                break;
                            case "ContentsLangDirection":
                                {
                                    switch (settingValue)
                                    {
                                        case "LeftToRight":
                                            this._settings[xmlAttributeAttribute.AttributeName] = "ltr";
                                            break;
                                        case "RightToLeft":
                                            this._settings[xmlAttributeAttribute.AttributeName] = "rtl";
                                            break;
                                        default:
                                            this._settings[xmlAttributeAttribute.AttributeName] = string.Empty;
                                            break;
                                    } 
                                }

                                break;
                            case "CodeMirror":
                                {
                                    var codeMirrorArray = new StringBuilder();

                                    foreach (var codeMirrorInfo in
                                        typeof(CodeMirror).GetProperties())
                                    {
                                        var xmlAttribute =
                                            codeMirrorInfo.GetCustomAttribute<XmlAttributeAttribute>(true);
                                        var rawSettingValue = codeMirrorInfo.GetValue(
                                            this.currentSettings.Config.CodeMirror, null);

                                        var codeMirrorSettingValue = rawSettingValue.ToString();

                                        if (string.IsNullOrEmpty(codeMirrorSettingValue))
                                        {
                                            continue;
                                        }
                                             
                                        switch (codeMirrorInfo.PropertyType.Name)
                                        {
                                            case "String":
                                                codeMirrorArray.AppendFormat("{0}: '{1}',", xmlAttribute.AttributeName, codeMirrorSettingValue);
                                                break;
                                            case "Boolean":
                                                codeMirrorArray.AppendFormat("{0}: {1},", xmlAttribute.AttributeName, codeMirrorSettingValue.ToLower());
                                                break;
                                        }
                                    }

                                    var codemirrorSettings = codeMirrorArray.ToString();

                                    this._settings["codemirror"] = string.Format(
                                        "{{ {0} }}", codemirrorSettings.Remove(codemirrorSettings.Length - 1, 1));
                                }

                                break;
                            case "WordCount":
                                {
                                    var wordcountArray = new StringBuilder();

                                    foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                                    {
                                        var xmlAttribute =
                                            wordCountInfo.GetCustomAttribute<XmlAttributeAttribute>(true);

                                        var rawSettingValue =
                                            wordCountInfo.GetValue(this.currentSettings.Config.WordCount, null);

                                        var wordCountSettingValue = rawSettingValue.ToString();

                                        if (string.IsNullOrEmpty(wordCountSettingValue))
                                        {
                                            continue;
                                        }

                                        switch (wordCountInfo.PropertyType.Name)
                                        {
                                            case "String":
                                                wordcountArray.AppendFormat("{0}: '{1}',", xmlAttribute.AttributeName, wordCountSettingValue);
                                                break;
                                            case "Boolean":
                                                wordcountArray.AppendFormat("{0}: {1},", xmlAttribute.AttributeName, wordCountSettingValue.ToLower());
                                                break;
                                        }
                                    }

                                    var wordcountSettings = wordcountArray.ToString();

                                    this._settings["wordcount"] = string.Format(
                                        "{{ {0} }}", wordcountSettings.Remove(wordcountSettings.Length - 1, 1));
                                }

                                break;
                            default:
                                this._settings[xmlAttributeAttribute.AttributeName] = settingValue;
                                break;
                        }
                    }
                }

                try
                {
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;

                    this._settings["language"] = currentCulture.Name.ToLowerInvariant();

                    if (string.IsNullOrEmpty(this.currentSettings.Config.Scayt_sLang))
                    {
                        this._settings["scayt_sLang"] = currentCulture.Name.ToLowerInvariant();
                    }
                }
                catch (Exception)
                {
                    this._settings["language"] = "en";
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.CustomConfig))
                {
                    this._settings["customConfig"] = this.FormatUrl(this.currentSettings.Config.CustomConfig);
                }
                else
                {
                    this._settings["customConfig"] = string.Empty;
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Skin))
                {
                    if (this.currentSettings.Config.Skin.Equals("office2003")
                        || this.currentSettings.Config.Skin.Equals("BootstrapCK-Skin")
                        || this.currentSettings.Config.Skin.Equals("chris")
                        || this.currentSettings.Config.Skin.Equals("v2"))
                    {
                        this._settings["skin"] = "moono";
                    }
                    else
                    {
                        this._settings["skin"] = this.currentSettings.Config.Skin;
                    }
                }
                var cssFiles = new List<string>();
                var skinSrc = this.GetSkinSource();
                var containerSrc = this.GetContainerSource();

                cssFiles.Add("~/portals/_default/default.css");
                cssFiles.Add(skinSrc.Replace(skinSrc.Substring(skinSrc.LastIndexOf('/'), skinSrc.Length - skinSrc.Substring(0, skinSrc.LastIndexOf('/')).Length), "/skin.css"));
                cssFiles.Add(containerSrc.Replace(containerSrc.Substring(containerSrc.LastIndexOf('/'), containerSrc.Length - containerSrc.Substring(0, containerSrc.LastIndexOf('/')).Length), "/container.css"));
                cssFiles.Add("~/DesktopModules/" + this.myParModule.ModuleConfiguration.DesktopModule.FolderName + "/module.css");
                cssFiles.Add("~" + this._portalSettings.HomeDirectory + "portal.css");

                var resolvedCssFiles = cssFiles.Where(cssFile => File.Exists(this.MapPathSecure(cssFile))).Select(Globals.ResolveUrl).ToList();

                if (!string.IsNullOrEmpty(this.currentSettings.Config.ContentsCss))
                {
                    var customCss = Globals.ResolveUrl(this.ReFormatURL(this.currentSettings.Config.ContentsCss));
                    resolvedCssFiles.Add(customCss);
                }

                var serializer = new JavaScriptSerializer();
                this._settings["contentsCss"] = serializer.Serialize(resolvedCssFiles);

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Templates_Files))
                {
                    var templateUrl = this.FormatUrl(this.currentSettings.Config.Templates_Files);

                    this._settings["templates_files"] = string.Format(
                        "[ '{0}' ]",
                        templateUrl.EndsWith(".xml") ? string.Format("xml:{0}", templateUrl) : templateUrl);
                }

                if (!string.IsNullOrEmpty(this.toolBarNameOverride))
                {
                    this._settings["toolbar"] = this.toolBarNameOverride;
                }
                else
                {
                    var toolbarName = this.SetUserToolbar(this._settings["configFolder"]);

                    var listToolbarSets = ToolbarUtil.GetToolbars(this._portalSettings.HomeDirectoryMapPath, this._settings["configFolder"]);

                    var toolbarSet = listToolbarSets.FirstOrDefault(toolbar => toolbar.Name.Equals(toolbarName));

                    var toolbarSetString = ToolbarUtil.ConvertToolbarSetToString(toolbarSet, true);

                    this._settings["toolbar"] = string.Format(
                        "[{0}]", toolbarSetString);
                }

                // Editor Width
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Width))
                {
                    this._settings["width"] = this.currentSettings.Config.Width;
                }
                else
                {
                    if (this.Width.Value > 0)
                    {
                        this._settings["width"] = this.Width.ToString();
                    }
                }

                // Editor Height
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Height))
                {
                    this._settings["height"] = this.currentSettings.Config.Height;
                }
                else
                {
                    if (this.Height.Value > 0)
                    {
                        this._settings["height"] = this.Height.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(this._settings["extraPlugins"])
                    && this._settings["extraPlugins"].Contains("xmlstyles"))
                {
                    this._settings["extraPlugins"] = this._settings["extraPlugins"].Replace(",xmlstyles", string.Empty);
                }

                // fix oEmbed/oembed issue and other bad settings
                if (!string.IsNullOrEmpty(this._settings["extraPlugins"])
                    && this._settings["extraPlugins"].Contains("oEmbed"))
                {
                    this._settings["extraPlugins"] = this._settings["extraPlugins"].Replace("oEmbed", "oembed");
                }

                if (this._settings["PasteFromWordCleanupFile"] != null
                    && this._settings["PasteFromWordCleanupFile"].Equals("default"))
                {
                    this._settings["PasteFromWordCleanupFile"] = string.Empty;
                }

                if (this._settings["menu_groups"] != null
                    && this._settings["menu_groups"].Equals("clipboard,table,anchor,link,image"))
                {
                    this._settings["menu_groups"] =
                        "clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div";
                }

                // Inject maxFileSize
                this._settings["maxFileSize"] = Utility.GetMaxUploadSize().ToString();

                HttpContext.Current.Session["CKDNNtabid"] = this._portalSettings.ActiveTab.TabID;
                HttpContext.Current.Session["CKDNNporid"] = this._portalSettings.PortalId;

                // Add FileBrowser
                switch (this.currentSettings.BrowserMode)
                {
                    case Browser.StandardBrowser:
                        {
                            this._settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Link&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            this._settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Image&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            this._settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Type=Flash&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this._portalSettings))
                            {
                                this._settings["filebrowserUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=FileUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId,
                                            this.parentModulId,
                                            this.ID,
                                            this.currentSettings.SettingMode,
                                            CultureInfo.CurrentCulture.Name));
                                this._settings["filebrowserFlashUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=FlashUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId,
                                            this.parentModulId,
                                            this.ID,
                                            this.currentSettings.SettingMode,
                                            CultureInfo.CurrentCulture.Name));
                                this._settings["filebrowserImageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/Browser/Browser.aspx?Command=ImageUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId,
                                            this.parentModulId,
                                            this.ID,
                                            this.currentSettings.SettingMode,
                                            CultureInfo.CurrentCulture.Name));
                            }

                            this._settings["filebrowserWindowWidth"] = "870";
                            this._settings["filebrowserWindowHeight"] = "800";

                            // Set Browser Authorize 
                            const bool CKDNNIsAuthorized = true;

                            HttpContext.Current.Session["CKDNNIsAuthorized"] = CKDNNIsAuthorized;

                            DataCache.SetCache("CKDNNIsAuthorized", CKDNNIsAuthorized);
                        }

                        break;
                    case Browser.CKFinder:
                        {
                            this._settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?tabid={0}&PortalID={1}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId));
                            this._settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?type=Images&tabid={0}&PortalID={1}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId));
                            this._settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.html?type=Flash&tabid={0}&PortalID={1}",
                                        this._portalSettings.ActiveTab.TabID,
                                        this._portalSettings.PortalId));

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this._portalSettings))
                            {
                                this._settings["filebrowserUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files&tabid={0}&PortalID={1}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId));
                                this._settings["filebrowserFlashUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash&tabid={0}&PortalID={1}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId));
                                this._settings["filebrowserImageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/CKEditor/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images&tabid={0}&PortalID={1}",
                                            this._portalSettings.ActiveTab.TabID,
                                            this._portalSettings.PortalId));
                            }

                            HttpContext.Current.Session["CKDNNSubDirs"] = this.currentSettings.SubDirs;

                            HttpContext.Current.Session["CKDNNRootDirId"] = this.currentSettings.BrowserRootDirId;
                            HttpContext.Current.Session["CKDNNUpDirId"] = this.currentSettings.UploadDirId;

                            // Set Browser Authorize 
                            const bool CKDNNIsAuthorized = true;

                            HttpContext.Current.Session["CKDNNIsAuthorized"] = CKDNNIsAuthorized;

                            DataCache.SetCache("CKDNNIsAuthorized", CKDNNIsAuthorized);
                        }

                        break;
                }

                this.isMerged = true;

                return this._settings;
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

        /// <summary>Gets the container source.</summary>
        /// <returns>The container source path</returns>
        private string GetContainerSource()
        {
            var containerSource = this._portalSettings.ActiveTab.ContainerSrc ?? this._portalSettings.DefaultPortalContainer;
            containerSource = this.ResolveSourcePath(containerSource);
            return containerSource;
        }

        /// <summary>Gets the skin source.</summary>
        /// <returns>The skin source path</returns>
        private string GetSkinSource()
        {
            var skinSource = this._portalSettings.ActiveTab.SkinSrc ?? this._portalSettings.DefaultPortalSkin;
            skinSource = this.ResolveSourcePath(skinSource);
            return skinSource;
        }

        /// <summary>Resolves the source path.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The source path of the select resource</returns>
        private string ResolveSourcePath(string source)
        {
            source = "~" + source;
            return source;
        }

        /// <summary>
        ///  Gets or sets The ToolBarName defined in config to override all other Toolbars
        /// </summary>
        public string ToolBarName
        {
            // EL 20101006
            get
            {
                return this.toolBarNameOverride;
            }

            set
            {
                this.toolBarNameOverride = value;
            }
        }

        /// <summary>
        /// Gets or sets Value.
        /// </summary>
        [DefaultValue("")]
        public string Value
        {
            get
            {
                object o = this.ViewState["Value"];

                return o == null ? string.Empty : Convert.ToString(o);
            }

            set
            {
                this.ViewState["Value"] = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether Has Microsoft Ajax is installed.
        /// </summary>
        private static bool HasMsAjax
        {
            get
            {
                if (_hasMsAjax != null)
                {
                    return _hasMsAjax.Value;
                }

                _hasMsAjax = false;

                var appAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                foreach (Assembly asm in
                    appAssemblies.Where(asm => asm.ManifestModule.Name == "System.Web.Extensions.dll"))
                {
                    try
                    {
                        var scriptManager = asm.GetType("System.Web.UI.ScriptManager");

                        if (scriptManager != null)
                        {
                            _hasMsAjax = true;
                        }
                    }
                    catch
                    {
                        _hasMsAjax = false;
                    }

                    break;
                }

                return _hasMsAjax.Value;
            }
        }

        /// <summary>
        ///   Gets Name for the Current Resource file name
        /// </summary>
        private static string SResXFile
        {
            get
            {
                return
                    Globals.ResolveUrl(
                        string.Format("~/Providers/HtmlEditorProviders/CKEditor/{0}/Options.aspx.resx", Localization.LocalResourceDirectory));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Finds the module instance.
        /// </summary>
        /// <param name="editorControl">The editor control.</param>
        /// <returns>
        /// The Instances found
        /// </returns>
        public static Control FindModuleInstance(Control editorControl)
        {
            Control ctl = editorControl.Parent;
            Control selectedCtl = null;
            Control possibleCtl = null;

            while (ctl != null)
            {
                var portalModuleBase = ctl as PortalModuleBase;

                if (portalModuleBase != null)
                {
                    if (portalModuleBase.TabModuleId == Null.NullInteger)
                    {
                        possibleCtl = ctl;
                    }
                    else
                    {
                        selectedCtl = ctl;
                        break;
                    }
                }

                ctl = ctl.Parent;
            }

            if (selectedCtl == null & possibleCtl != null)
            {
                selectedCtl = possibleCtl;
            }

            return selectedCtl;
        }

        /// <summary>
        /// The has rendered text area.
        /// </summary>
        /// <param name="control">
        /// The control.
        /// </param>
        /// <returns>
        /// Returns if it has rendered text area.
        /// </returns>
        public bool HasRenderedTextArea(Control control)
        {
            if (control is CKEditorControl && ((CKEditorControl)control).IsRendered)
            {
                return true;
            }

            return control.Controls.Cast<Control>().Any(this.HasRenderedTextArea);
        }

        #endregion

        #region Implemented Interfaces

        #region IPostBackDataHandler

        /// <summary>
        /// The load post data.
        /// </summary>
        /// <param name="postDataKey">
        /// The post data key.
        /// </param>
        /// <param name="postCollection">
        /// The post collection.
        /// </param>
        /// <returns>
        /// Returns if the PostData are loaded.
        /// </returns>
        public bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            try
            {
                string currentValue = this.Value;
                string postedValue = postCollection[postDataKey];

                if (currentValue == null | !postedValue.Equals(currentValue))
                {
                    if (this.currentSettings.InjectSyntaxJs)
                    {
                        if (postedValue.Contains("<pre class=\"brush:") && !postedValue.Contains("shCore.js"))
                        {
                            // Add Syntax Highlighter Plugin
                            postedValue =
                                string.Format(
                                    "<!-- Injected Syntax Highlight Code --><script type=\"text/javascript\" src=\"{0}plugins/syntaxhighlight/scripts/shCore.js\"></script><link type=\"text/css\" rel=\"stylesheet\" href=\"{0}plugins/syntaxhighlight/styles/shCore.css\"/><script type=\"text/javascript\">SyntaxHighlighter.all();</script>{1}",
                                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/"),
                                    postedValue);
                        }

                        if (postedValue.Contains("<span class=\"math-tex\">") && !postedValue.Contains("MathJax.js"))
                        {
                            // Add MathJax Plugin
                            postedValue =
                                string.Format(
                                    "<!-- Injected MathJax Code --><script type=\"text/javascript\" src=\"//cdn.mathjax.org/mathjax/2.2-latest/MathJax.js?config=TeX-AMS_HTML\"></script>{0}",
                                    postedValue);
                        }
                    }

                    this.Value = postedValue;

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// The raise post data changed event.
        /// </summary>
        public void RaisePostDataChangedEvent()
        {
            // Do nothing
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Update the Editor after the Post back
        /// And Create Main Script to Render the Editor
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (HasMsAjax)
            {
                return;
            }

            this.RegisterCKEditorLibrary();

            this.GenerateEditorLoadScript();
        }

        /// <summary>
        /// The render.
        /// </summary>
        /// <param name="outWriter">
        /// The out writer.
        /// </param>
        protected override void Render(HtmlTextWriter outWriter)
        {
            outWriter.Write("<div>");
            outWriter.Write("<noscript>");
            outWriter.Write("<p>");
            outWriter.Write(Localization.GetString("NoJava.Text", SResXFile));
            outWriter.Write("</p>");
            outWriter.Write("</noscript>");
            outWriter.Write("</div>");

            outWriter.Write(
                "<input type=\"hidden\" name=\"CKDNNporid\" id=\"CKDNNporid\" value=\"{0}\">",
                this._portalSettings.PortalId);

            outWriter.Write(outWriter.NewLine);

            var styleWidth = !string.IsNullOrEmpty(this.currentSettings.Config.Width)
                                 ? string.Format(" style=\"width:{0};\"", this.currentSettings.Config.Width)
                                 : string.Empty;

            outWriter.Write("<div{0}>", styleWidth);

            // Write text area
            outWriter.AddAttribute("id", this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty));
            outWriter.AddAttribute("name", this.UniqueID);

            outWriter.AddAttribute("cols", "80");
            outWriter.AddAttribute("rows", "10");

            outWriter.AddAttribute("class", "editor");

            outWriter.AddAttribute("style", "visibility: hidden; display: none;");

            outWriter.RenderBeginTag("textarea");

            if (string.IsNullOrEmpty(this.Value))
            {
                if (!string.IsNullOrEmpty(this.currentSettings.BlankText))
                {
                    outWriter.Write(this.Context.Server.HtmlEncode(this.currentSettings.BlankText));
                }
            }
            else
            {
                outWriter.Write(this.Context.Server.HtmlEncode(this.Value));
            }

            outWriter.RenderEndTag();

            outWriter.Write("</div>");

            this.IsRendered = true;

            /////////////////

            if (!this.HasRenderedTextArea(this.Page))
            {
                return;
            }

            outWriter.Write("<p style=\"text-align:center;\">");

            if (PortalSecurity.IsInRoles(this._portalSettings.AdministratorRoleName))
            {
                outWriter.Write(
                    "<a href=\"javascript:void(0)\" onclick=\"window.open('{0}','Options','width=850,height=750,resizable=yes')\" class=\"CommandButton\" id=\"{1}\">{2}</a>",
                    string.Format(
                        "{0}?mid={1}&amp;tid={2}&amp;minc={3}&amp;PortalID={4}&amp;langCode={5}",
                        Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/Options.aspx"),
                        this.parentModulId,
                        this._portalSettings.ActiveTab.TabID,
                        this.ID,
                        this._portalSettings.PortalId,
                        CultureInfo.CurrentCulture.Name),
                    string.Format("{0}_ckoptions", this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty)),
                    Localization.GetString("Options.Text", SResXFile));
            }

            /////////////////
        }

        /// <summary>
        /// Initializes the Editor
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void CKEditorInit(object sender, EventArgs e)
        {
            if (this.Page != null)
            {
                this.Page.RegisterRequiresPostBack(this); // Ensures that postback is handled
            }

            this.myParModule = (PortalModuleBase)FindModuleInstance(this);

            if (this.myParModule == null || this.myParModule.ModuleId == -1)
            {
                // Get Parent ModuleID From this ClientID
                string sClientId = this.ClientID.Substring(this.ClientID.IndexOf("ctr") + 3);

                sClientId = sClientId.Remove(this.ClientID.IndexOf("_"));

                try
                {
                    this.parentModulId = int.Parse(sClientId);
                }
                catch (Exception)
                {
                    // The is no real module, then use the "User Accounts" module (Profile editor)
                    ModuleController db = new ModuleController();
                    ModuleInfo objm = db.GetModuleByDefinition(this._portalSettings.PortalId, "User Accounts");

                    this.parentModulId = objm.TabModuleID;
                }
            }
            else
            {
                this.parentModulId = this.myParModule.ModuleId;
            }

            this.CheckFileBrowser();

            this.LoadAllSettings();

            if (!HasMsAjax)
            {
                return;
            }

            this.RegisterCKEditorLibrary();

            this.GenerateEditorLoadScript();
        }

        /// <summary>
        /// The check file browser.
        /// </summary>
        private void CheckFileBrowser()
        {
            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if (objProvider == null || string.IsNullOrEmpty(objProvider.Attributes["ck_browser"]))
            {
                return;
            }

            switch (objProvider.Attributes["ck_browser"])
            {
                case "ckfinder":
                    this.currentSettings.BrowserMode = Browser.CKFinder;
                    break;
                case "standard":
                    this.currentSettings.BrowserMode = Browser.StandardBrowser;
                    break;
                case "none":
                    this.currentSettings.BrowserMode = Browser.None;
                    break;
            }
        }

        /// <summary>
        /// Load Portal/Page/Module Settings
        /// </summary>
        private void LoadAllSettings()
        {
            var settingsDictionary = Utility.GetEditorHostSettings();
            var portalRoles = new RoleController().GetPortalRoles(this._portalSettings.PortalId);

            // Load Default Settings
            this.currentSettings = SettingsUtil.GetDefaultSettings(
                this._portalSettings,
                this._portalSettings.HomeDirectoryMapPath,
                this._settings["configFolder"],
                portalRoles);

            // Set Current Mode to Default
            this.currentSettings.SettingMode = SettingsMode.Default;

            var portalKey = string.Format("DNNCKP#{0}#", this._portalSettings.PortalId);
            var pageKey = string.Format("DNNCKT#{0}#", this._portalSettings.ActiveTab.TabID);
            var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", this.parentModulId, this.ID);

            // Load Portal Settings ?!
            if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, portalKey))
            {
               /* throw new ApplicationException(settingsDictionary.FirstOrDefault(
                            setting => setting.Name.Equals(string.Format("{0}{1}", portalKey, "StartupMode"))).Value);*/

                this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                    this._portalSettings, this.currentSettings, settingsDictionary, portalKey, portalRoles);

                // Set Current Mode to Portal
                this.currentSettings.SettingMode = SettingsMode.Portal;
            }

            // Load Page Settings ?!
            if (SettingsUtil.CheckExistsPortalOrPageSettings(settingsDictionary, pageKey))
            {
                this.currentSettings = SettingsUtil.LoadPortalOrPageSettings(
                    this._portalSettings, this.currentSettings, settingsDictionary, pageKey, portalRoles);

                // Set Current Mode to Page
                this.currentSettings.SettingMode = SettingsMode.Page;
            }

            // Load Module Settings ?!
            if (!SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.parentModulId))
            {
                return;
            }

            this.currentSettings = SettingsUtil.LoadModuleSettings(
                this._portalSettings, this.currentSettings, moduleKey, this.parentModulId, portalRoles);

            // Set Current Mode to Module Instance
            this.currentSettings.SettingMode = SettingsMode.ModuleInstance;
        }

        /// <summary>
        /// Format the Url from FileID to File Path Url
        /// </summary>
        /// <param name="sInputUrl">
        /// The s Input Url.
        /// </param>
        /// <returns>
        /// The format url.
        /// </returns>
        private string FormatUrl(string sInputUrl)
        {
            string sImageUrl = string.Empty;

            if (sInputUrl.Equals(string.Empty))
            {
                return sImageUrl;
            }

            if (sInputUrl.StartsWith("http://"))
            {
                sImageUrl = sInputUrl;
            }
            else if (sInputUrl.StartsWith("FileID="))
            {
                int iFileId = int.Parse(sInputUrl.Substring(7));

               // FileController objFileController = new FileController();
               var objFileInfo = FileManager.Instance.GetFile(iFileId);

                // FileInfo objFileInfo = objFileController.GetFileById(iFileId, this._portalSettings.PortalId);
                sImageUrl = this._portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }

            return sImageUrl;
        }

        /// <summary>
        /// Load the Settings from the web.config file
        /// </summary>
        private void LoadConfigSettings()
        {
            this._settings = new NameValueCollection();

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if (objProvider == null)
            {
                return;
            }

            foreach (string key in objProvider.Attributes)
            {
                if (!key.ToLower().StartsWith("ck_"))
                {
                    continue;
                }

                string sAdjustedKey = key.Substring(3, key.Length - 3).ToLower();

                if (sAdjustedKey != string.Empty)
                {
                    this._settings[sAdjustedKey] = objProvider.Attributes[key];
                }
            }
        }

        /// <summary>
        /// This registers a startup JavaScript with compatibility with the Microsoft Ajax
        /// </summary>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <param name="script">
        /// The script.
        /// </param>
        /// <param name="addScriptTags">
        /// The add Script Tags.
        /// </param>
        private void RegisterStartupScript(string key, string script, bool addScriptTags)
        {
            if (HasMsAjax)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, addScriptTags);
            }
            else
            {
                this.Page.ClientScript.RegisterStartupScript(this.GetType(), key, script, true);
            } 
        }

        /// <summary>
        /// Registers the on submit statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        private void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            if (HasMsAjax)
            {
                ScriptManager.RegisterOnSubmitStatement(this, type, key, script);
            }
            else
            {
                Page.ClientScript.RegisterOnSubmitStatement(type, key, script);
            }
        }

        /// <summary>
        /// Set Toolbar based on Current User
        /// </summary>
        /// <param name="alternateConfigSubFolder">
        /// The alternate config sub folder.
        /// </param>
        /// <returns>
        /// Toolbar Name
        /// </returns>
        private string SetUserToolbar(string alternateConfigSubFolder)
        {
            string toolbarName = HttpContext.Current.Request.IsAuthenticated ? "Full" : "Basic";

            var listToolbarSets = ToolbarUtil.GetToolbars(
                this._portalSettings.HomeDirectoryMapPath, alternateConfigSubFolder);

            var listUserToolbarSets = new List<ToolbarSet>();

            var roleController = new RoleController();

            if (this.currentSettings.ToolBarRoles.Count <= 0)
            {
                return toolbarName;
            }

            foreach (var roleToolbar in this.currentSettings.ToolBarRoles)
            {
                if (roleToolbar.RoleId.Equals(-1) && !HttpContext.Current.Request.IsAuthenticated)
                {
                    return roleToolbar.Toolbar;
                }

                if (roleToolbar.RoleId.Equals(-1))
                {
                    continue;
                }

                // Role
                var role = roleController.GetRole(roleToolbar.RoleId, this._portalSettings.PortalId);

                if (role == null)
                {
                    continue;
                }

                if (!PortalSecurity.IsInRole(role.RoleName))
                {
                    continue;
                }

                // Handle Different Roles
                if (!listToolbarSets.Any(toolbarSel => toolbarSel.Name.Equals(roleToolbar.Toolbar)))
                {
                    continue;
                }

                var toolbar = listToolbarSets.Find(toolbarSel => toolbarSel.Name.Equals(roleToolbar.Toolbar));

                listUserToolbarSets.Add(toolbar);

                /*if (roleToolbar.RoleId.Equals(this._portalSettings.AdministratorRoleId) && HttpContext.Current.Request.IsAuthenticated)
                    {
                        if (PortalSecurity.IsInRole(roleName))
                        {
                            return roleToolbar.Toolbar;
                        }
                    }*/
            }

            if (listUserToolbarSets.Count <= 0)
            {
                return toolbarName;
            }

            // Compare The User Toolbars if the User is more then One Role, and apply the Toolbar with the Highest Priority
            int iHighestPrio = listUserToolbarSets.Max(toolb => toolb.Priority);

            return ToolbarUtil.FindHighestToolbar(listUserToolbarSets, iHighestPrio).Name;
        }

        /// <summary>
        /// Registers the CKEditor library.
        /// </summary>
        private void RegisterCKEditorLibrary()
        {
            ClientResourceManager.RegisterStyleSheet(
                this.Page,
                Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/editor.css"));

            ClientScriptManager cs = this.Page.ClientScript;

            Type csType = this.GetType();

            const string CsName = "CKEdScript";
            const string CsAdaptName = "CKAdaptScript";
            const string CsFindName = "CKFindScript";

            jQuery.RequestRegistration();


            // Inject jQuery if editor is loaded in a RadWindow
            if (HttpContext.Current.Request.QueryString["rwndrnd"] != null)
            {
                ScriptManager.RegisterClientScriptInclude(
                    this, csType, "jquery_registered", "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js");
            }

            if (File.Exists(this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js"))
                && !cs.IsClientScriptIncludeRegistered(csType, CsName))
            {
                cs.RegisterClientScriptInclude(
                    csType, CsName, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/ckeditor.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath(
                        "~/Providers/HtmlEditorProviders/CKEditor/js/jquery.ckeditor.adapter.js"))
                && !cs.IsClientScriptIncludeRegistered(csType, CsAdaptName))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    CsAdaptName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/js/jquery.ckeditor.adapter.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.js")) &&
                !cs.IsClientScriptIncludeRegistered(csType, CsFindName) && this.currentSettings.BrowserMode.Equals(Browser.CKFinder))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    CsFindName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/CKEditor/ckfinder/ckfinder.js"));
            }

            // Load Custom JS File
            if (!string.IsNullOrEmpty(this.currentSettings.CustomJsFile)
                && !cs.IsClientScriptIncludeRegistered(csType, "CKCustomJSFile"))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    "CKCustomJSFile",
                    this.FormatUrl(this.currentSettings.CustomJsFile)); 
            }
        }

        /// <summary>
        /// Generates the editor load script.
        /// </summary>
        private void GenerateEditorLoadScript()
        {
            var editorVar = string.Format(
                "editor{0}",
                this.ClientID.Substring(this.ClientID.LastIndexOf("_", StringComparison.Ordinal) + 1).Replace(
                    "-", string.Empty));

            var editorFixedId = this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty);

            if (HasMsAjax)
            {
                var postBackScript =
                    string.Format(
                        @"if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement(); CKEDITOR.instances.{0}.destroy(); }}",
                        editorFixedId);

                this.RegisterOnSubmitStatement(
                    this.GetType(), string.Format("CKEditor_OnAjaxSubmit_{0}", editorFixedId), postBackScript);
            }

            var editorScript = new StringBuilder();

            editorScript.AppendFormat(
                "Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(LoadCKEditorInstance_{0});", editorFixedId);

            editorScript.AppendFormat("function LoadCKEditorInstance_{0}(sender,args) {{", editorFixedId);

            editorScript.AppendFormat(
                @"if (jQuery(""[id*='UpdatePanel']"").length == 0 && CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();}}",
                editorFixedId);

            editorScript.AppendFormat(
                "if (document.getElementById('{0}') == null){{return;}}",
                editorFixedId);

            // Render EditorConfig
            var test = new StringBuilder();
            test.AppendFormat("var editorConfig{0} = {{", editorVar);

            var keysCount = this.Settings.Keys.Count;
            var currentCount = 0;

            // Write options
            foreach (string key in this.Settings.Keys)
            {
                var value = this.Settings[key];

                currentCount++;

                // Is boolean state or string
                if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase)
                    || value.Equals("false", StringComparison.InvariantCultureIgnoreCase) || value.StartsWith("[")
                    || value.StartsWith("{") || Utility.IsNumeric(value))
                {
                    if (value.Equals("True"))
                    {
                        value = "true";
                    }
                    else if (value.Equals("False"))
                    {
                        value = "false";
                    } 

                    test.AppendFormat("{0}:{1}", key, value);

                    test.Append(currentCount == keysCount ? "};" : ",");
                }
                else
                {
                    if (key == "browser")
                    {
                        continue;
                    }

                    test.AppendFormat("{0}:\'{1}\'", key, value);

                    test.Append(currentCount == keysCount ? "};" : ",");
                }
            }

            editorScript.Append(test);
            editorScript.AppendFormat(
                "if (CKEDITOR.instances.{0}){{return;}}",
                editorFixedId);
            
            // Check if we can use jQuery or $, and if both fail use ckeditor without the adapter
            editorScript.Append("if (jQuery().ckeditor) {");

            editorScript.AppendFormat("var {0} = jQuery('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else if ($.ckeditor) {");

            editorScript.AppendFormat("var {0} = $('#{1}').ckeditor(editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("} else {");

            editorScript.AppendFormat("var {0} = CKEDITOR.replace( '{1}', editorConfig{0});", editorVar, editorFixedId);

            editorScript.Append("}");

            // firefox maximize fix
            editorScript.Append("CKEDITOR.on('instanceReady', function (ev) {");
            editorScript.Append("ev.editor.on('maximize', function () {");
            editorScript.Append("if (ev.editor.commands.maximize.state == 1) {");
            editorScript.Append("var mainDocument = CKEDITOR.document;");
            editorScript.Append("CKEDITOR.env.gecko && mainDocument.getDocumentElement().setStyle( 'position', 'fixed' );");
            editorScript.Append("}");
            editorScript.Append("});");
            editorScript.Append("});");

            // End of LoadScript
            editorScript.Append("}");

            this.RegisterStartupScript(string.Format(@"{0}_CKE_Startup", editorFixedId), editorScript.ToString(), true);
        }

        #endregion
    }
}