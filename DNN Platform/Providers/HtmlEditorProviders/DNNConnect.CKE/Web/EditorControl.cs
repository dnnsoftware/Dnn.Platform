// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Web
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
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

    using DNNConnect.CKEditorProvider.Constants;
    using DNNConnect.CKEditorProvider.Extensions;
    using DNNConnect.CKEditorProvider.Objects;
    using DNNConnect.CKEditorProvider.Utilities;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    /// <summary>
    /// The CKEditor control.
    /// </summary>
    public class EditorControl : WebControl, IPostBackDataHandler
    {
        /// <summary>
        /// The provider type.
        /// </summary>
        private const string ProviderType = "htmlEditor";

        /// <summary>
        /// The portal settings.
        /// </summary>
        private readonly PortalSettings portalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];

        /// <summary>
        /// Check if the Settings Collection
        /// is Merged with all Settings.
        /// </summary>
        private bool isMerged;

        /// <summary>
        /// The settings collection.
        /// </summary>
        private NameValueCollection settings;

        /// <summary>
        /// Current Settings Base.
        /// </summary>
        private EditorProviderSettings currentSettings = new EditorProviderSettings();

        /// <summary>
        /// The tool bar name override.
        /// </summary>
        private string toolBarNameOverride; // EL 20101006

        /// <summary>
        /// The parent module that contains the editor.
        /// </summary>
        private PortalModuleBase myParModule;

        /// <summary>
        /// The Parent Module ID.
        /// </summary>
        private int parentModulId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorControl"/> class.
        /// </summary>
        public EditorControl()
        {
            this.LoadConfigSettings();

            this.Init += this.CKEditorInit;
        }

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
                    return this.settings;
                }

                // Override local settings with attributes
                foreach (string key in this.Attributes.Keys)
                {
                    this.settings[key] = this.Attributes[key];
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
                        this.settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                    }
                    else
                    {
                        switch (info.Name)
                        {
                            case "ToolbarLocation":
                                this.settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                                break;
                            case "EnterMode":
                            case "ShiftEnterMode":
                                switch (settingValue)
                                {
                                    case "P":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "1";
                                        break;
                                    case "BR":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "2";
                                        break;
                                    case "DIV":
                                        this.settings[xmlAttributeAttribute.AttributeName] = "3";
                                        break;
                                }

                                break;
                            case "ContentsLangDirection":
                                {
                                    switch (settingValue)
                                    {
                                        case "LeftToRight":
                                            this.settings[xmlAttributeAttribute.AttributeName] = "ltr";
                                            break;
                                        case "RightToLeft":
                                            this.settings[xmlAttributeAttribute.AttributeName] = "rtl";
                                            break;
                                        default:
                                            this.settings[xmlAttributeAttribute.AttributeName] = string.Empty;
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

                                    this.settings["codemirror"] = string.Format(
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

                                    this.settings["wordcount"] = string.Format(
                                        "{{ {0} }}", wordcountSettings.Remove(wordcountSettings.Length - 1, 1));
                                }

                                break;
                            default:
                                this.settings[xmlAttributeAttribute.AttributeName] = settingValue;
                                break;
                        }
                    }
                }

                try
                {
                    var currentCulture = Thread.CurrentThread.CurrentUICulture;

                    this.settings["language"] = currentCulture.Name.ToLowerInvariant();

                    if (string.IsNullOrEmpty(this.currentSettings.Config.Scayt_sLang))
                    {
                        // 'en-us' is not a language code that is supported, the correct is 'en_US'
                        // https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-scayt_sLang
                        this.settings["scayt_sLang"] = currentCulture.Name.Replace("-", "_");
                    }
                }
                catch (Exception)
                {
                    this.settings["language"] = "en";
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.CustomConfig))
                {
                    this.settings["customConfig"] = this.FormatUrl(this.currentSettings.Config.CustomConfig);
                }
                else
                {
                    this.settings["customConfig"] = string.Empty;
                }

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Skin))
                {
                    if (this.currentSettings.Config.Skin.Equals("office2003")
                        || this.currentSettings.Config.Skin.Equals("BootstrapCK-Skin")
                        || this.currentSettings.Config.Skin.Equals("chris")
                        || this.currentSettings.Config.Skin.Equals("v2"))
                    {
                        this.settings["skin"] = "moono";
                    }
                    else
                    {
                        this.settings["skin"] = this.currentSettings.Config.Skin;
                    }
                }

                this.settings["linkDefaultProtocol"] = this.currentSettings.DefaultLinkProtocol.ToSettingValue();

                var cssFiles = new List<string>();
                var skinSrc = this.GetSkinSource();
                var containerSrc = this.GetContainerSource();

                cssFiles.Add("~/portals/_default/default.css");
                cssFiles.Add(skinSrc.Replace(skinSrc.Substring(skinSrc.LastIndexOf('/'), skinSrc.Length - skinSrc.Substring(0, skinSrc.LastIndexOf('/')).Length), "/skin.css"));
                cssFiles.Add(containerSrc.Replace(containerSrc.Substring(containerSrc.LastIndexOf('/'), containerSrc.Length - containerSrc.Substring(0, containerSrc.LastIndexOf('/')).Length), "/container.css"));
                if (this.myParModule != null && this.myParModule.ModuleId > -1)
                {
                    cssFiles.Add("~/DesktopModules/" + this.myParModule.ModuleConfiguration.DesktopModule.FolderName + "/module.css");
                }

                cssFiles.Add("~" + this.portalSettings.HomeDirectory + "portal.css");
                cssFiles.Add("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CkEditorContents.css");

                var resolvedCssFiles = cssFiles.Where(cssFile => File.Exists(this.MapPathSecure(cssFile))).Select(Globals.ResolveUrl).ToList();

                if (!string.IsNullOrEmpty(this.currentSettings.Config.ContentsCss))
                {
                    var customCss = Globals.ResolveUrl(this.FormatUrl(this.currentSettings.Config.ContentsCss));
                    resolvedCssFiles.Add(customCss);
                }

                var serializer = new JavaScriptSerializer();
                this.settings["contentsCss"] = serializer.Serialize(resolvedCssFiles);

                if (!string.IsNullOrEmpty(this.currentSettings.Config.Templates_Files))
                {
                    var templateUrl = this.FormatUrl(this.currentSettings.Config.Templates_Files);

                    this.settings["templates_files"] = string.Format(
                        "[ '{0}' ]",
                        templateUrl.EndsWith(".xml") ? string.Format("xml:{0}", templateUrl) : templateUrl);
                }

                if (!string.IsNullOrEmpty(this.toolBarNameOverride))
                {
                    this.settings["toolbar"] = this.toolBarNameOverride;
                }
                else
                {
                    var toolbarName = this.SetUserToolbar(this.settings["configFolder"]);

                    var listToolbarSets = ToolbarUtil.GetToolbars(this.portalSettings.HomeDirectoryMapPath, this.settings["configFolder"]);

                    var toolbarSet = listToolbarSets.FirstOrDefault(toolbar => toolbar.Name.Equals(toolbarName));

                    var toolbarSetString = ToolbarUtil.ConvertToolbarSetToString(toolbarSet, true);

                    this.settings["toolbar"] = string.Format(
                        "[{0}]", toolbarSetString);
                }

                // Easy Image Upload
                if (this.currentSettings.ImageButtonMode == ImageButtonType.EasyImageButton)
                {
                    // replace 'Image' Plugin with 'EasyImage'
                    this.settings["toolbar"] = this.settings["toolbar"].Replace("'Image'", "'EasyImageUpload'");

                    // add the plugin in extraPlugins
                    if (string.IsNullOrEmpty(this.settings["extraPlugins"]) || !this.settings["extraPlugins"].Split(',').Contains("easyimage"))
                    {
                        if (!string.IsNullOrEmpty(this.settings["extraPlugins"]))
                        {
                            this.settings["extraPlugins"] += ",";
                        }

                        this.settings["extraPlugins"] += "easyimage";
                    }

                    // change the easyimage toolbar
                    this.settings["easyimage_toolbar"] = "['EasyImageAlt']";

                    // remove the image plugin in removePlugins
                    if (string.IsNullOrEmpty(this.settings["removePlugins"]) || !this.settings["removePlugins"].Split(',').Contains("image"))
                    {
                        if (!string.IsNullOrEmpty(this.settings["removePlugins"]))
                        {
                            this.settings["removePlugins"] += ",";
                        }

                        this.settings["removePlugins"] += "image";
                    }

                    this.settings.Add("cloudServices_uploadUrl", Globals.ResolveUrl(
                        string.Format(
                            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=EasyImageUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                            this.portalSettings.ActiveTab.TabID,
                            this.portalSettings.PortalId,
                            this.parentModulId,
                            this.ID,
                            this.currentSettings.SettingMode,
                            CultureInfo.CurrentCulture.Name)));
                }
                else
                {
                    // remove the easyimage plugin in removePlugins
                    if (string.IsNullOrEmpty(this.settings["removePlugins"]) || !this.settings["removePlugins"].Split(',').Contains("easyimage"))
                    {
                        if (!string.IsNullOrEmpty(this.settings["removePlugins"]))
                        {
                            this.settings["removePlugins"] += ",";
                        }

                        this.settings["removePlugins"] += "easyimage";
                    }
                }

                // cloudservices variables need to be set regardless
                this.settings.Add("cloudServices_tokenUrl", "/API/CKEditorProvider/CloudServices/GetToken");

                // Editor Width
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Width))
                {
                    this.settings["width"] = this.currentSettings.Config.Width;
                }
                else
                {
                    if (this.Width.Value > 0)
                    {
                        this.settings["width"] = this.Width.ToString();
                    }
                }

                // Editor Height
                if (!string.IsNullOrEmpty(this.currentSettings.Config.Height))
                {
                    this.settings["height"] = this.currentSettings.Config.Height;
                }
                else
                {
                    if (this.Height.Value > 0)
                    {
                        this.settings["height"] = this.Height.ToString();
                    }
                }

                if (!string.IsNullOrEmpty(this.settings["extraPlugins"])
                    && this.settings["extraPlugins"].Contains("xmlstyles"))
                {
                    this.settings["extraPlugins"] = this.settings["extraPlugins"].Replace(",xmlstyles", string.Empty);
                }

                // fix oEmbed/oembed issue and other bad settings
                if (!string.IsNullOrEmpty(this.settings["extraPlugins"])
                    && this.settings["extraPlugins"].Contains("oEmbed"))
                {
                    this.settings["extraPlugins"] = this.settings["extraPlugins"].Replace("oEmbed", "oembed");
                }

                if (this.settings["PasteFromWordCleanupFile"] != null
                    && this.settings["PasteFromWordCleanupFile"].Equals("default"))
                {
                    this.settings["PasteFromWordCleanupFile"] = string.Empty;
                }

                if (this.settings["menu_groups"] != null
                    && this.settings["menu_groups"].Equals("clipboard,table,anchor,link,image"))
                {
                    this.settings["menu_groups"] =
                        "clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div";
                }

                // Inject maxFileSize
                this.settings["maxFileSize"] = Utility.GetMaxUploadSize().ToString();

                HttpContext.Current.Session["CKDNNtabid"] = this.portalSettings.ActiveTab.TabID;
                HttpContext.Current.Session["CKDNNporid"] = this.portalSettings.PortalId;

                // Add FileBrowser
                switch (this.currentSettings.BrowserMode)
                {
                    case BrowserType.StandardBrowser:
                        {
                            this.settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Link&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            this.settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Image&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            this.settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Flash&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId,
                                        this.parentModulId,
                                        this.ID,
                                        this.currentSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this.portalSettings))
                            {
                                //this.settings["filebrowserUploadUrl"] =
                                //    Globals.ResolveUrl(
                                //        string.Format(
                                //            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FileUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                //            this.portalSettings.ActiveTab.TabID,
                                //            this.portalSettings.PortalId,
                                //            this.parentModulId,
                                //            this.ID,
                                //            this.currentSettings.SettingMode,
                                //            CultureInfo.CurrentCulture.Name));
                                //this.settings["filebrowserFlashUploadUrl"] =
                                //    Globals.ResolveUrl(
                                //        string.Format(
                                //            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FlashUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                //            this.portalSettings.ActiveTab.TabID,
                                //            this.portalSettings.PortalId,
                                //            this.parentModulId,
                                //            this.ID,
                                //            this.currentSettings.SettingMode,
                                //            CultureInfo.CurrentCulture.Name));
                                //this.settings["filebrowserImageUploadUrl"] =
                                //    Globals.ResolveUrl(
                                //        string.Format(
                                //            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=ImageUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                //            this.portalSettings.ActiveTab.TabID,
                                //            this.portalSettings.PortalId,
                                //            this.parentModulId,
                                //            this.ID,
                                //            this.currentSettings.SettingMode,
                                //            CultureInfo.CurrentCulture.Name));
                            }

                            this.settings["filebrowserWindowWidth"] = "870";
                            this.settings["filebrowserWindowHeight"] = "800";

                            // Set Browser Authorize
                            const bool isAuthorized = true;

                            HttpContext.Current.Session["CKE_DNNIsAuthorized"] = isAuthorized;

                            DataCache.SetCache("CKE_DNNIsAuthorized", isAuthorized);
                        }

                        break;
                    case BrowserType.CKFinder:
                        {
                            this.settings["filebrowserBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?tabid={0}&PortalID={1}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId));
                            this.settings["filebrowserImageBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?type=Images&tabid={0}&PortalID={1}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId));
                            this.settings["filebrowserFlashBrowseUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?type=Flash&tabid={0}&PortalID={1}",
                                        this.portalSettings.ActiveTab.TabID,
                                        this.portalSettings.PortalId));

                            if (Utility.CheckIfUserHasFolderWriteAccess(this.currentSettings.UploadDirId, this.portalSettings))
                            {
                                this.settings["filebrowserUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files&tabid={0}&PortalID={1}",
                                            this.portalSettings.ActiveTab.TabID,
                                            this.portalSettings.PortalId));
                                this.settings["filebrowserFlashUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash&tabid={0}&PortalID={1}",
                                            this.portalSettings.ActiveTab.TabID,
                                            this.portalSettings.PortalId));
                                this.settings["filebrowserImageUploadUrl"] =
                                    Globals.ResolveUrl(
                                        string.Format(
                                            "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images&tabid={0}&PortalID={1}",
                                            this.portalSettings.ActiveTab.TabID,
                                            this.portalSettings.PortalId));
                            }

                            HttpContext.Current.Session["CKDNNSubDirs"] = this.currentSettings.SubDirs;

                            HttpContext.Current.Session["CKDNNRootDirId"] = this.currentSettings.BrowserRootDirId;
                            HttpContext.Current.Session["CKDNNUpDirId"] = this.currentSettings.UploadDirId;

                            // Set Browser Authorize
                            const bool isAuthorized = true;

                            HttpContext.Current.Session["CKE_DNNIsAuthorized"] = isAuthorized;

                            DataCache.SetCache("CKE_DNNIsAuthorized", isAuthorized);
                        }

                        break;
                }

                this.isMerged = true;

                return this.settings;
            }
        }

        /// <summary>
        ///  Gets or sets The ToolBarName defined in config to override all other Toolbars.
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
        ///   Gets Name for the Current Resource file name.
        /// </summary>
        private static string SResXFile
        {
            get
            {
                return
                    Globals.ResolveUrl(
                        string.Format("~/Providers/HtmlEditorProviders/DNNConnect.CKE/{0}/Options.aspx.resx", Localization.LocalResourceDirectory));
            }
        }

        /// <summary>
        /// Finds the module instance.
        /// </summary>
        /// <param name="editorControl">The editor control.</param>
        /// <returns>
        /// The Instances found.
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
            if (control is EditorControl && ((EditorControl)control).IsRendered)
            {
                return true;
            }

            return control.Controls.Cast<Control>().Any(this.HasRenderedTextArea);
        }

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
                                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/CKEditor/"),
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

        /// <summary>
        /// Update the Editor after the Post back
        /// And Create Main Script to Render the Editor.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // if (HasMsAjax)
            // {
            //    return;
            // }

            // RegisterCKEditorLibrary();

            // GenerateEditorLoadScript();
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
            outWriter.AddAttribute("aria-label", "editor");

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

            if (PortalSecurity.IsInRoles(this.portalSettings.AdministratorRoleName))
            {
                var editorUrl = Globals.NavigateURL(
                    "CKEditorOptions",
                    "ModuleId=" + this.parentModulId,
                    "minc=" + this.ID,
                    "PortalID=" + this.portalSettings.PortalId,
                    "langCode=" + CultureInfo.CurrentCulture.Name,
                    "popUp=true");

                outWriter.Write(
                    "<a href=\"javascript:void(0)\" onclick='window.open({0},\"Options\", \"width=850,height=750,resizable=yes\")' class=\"CommandButton\" id=\"{1}\">{2}</a>",
                    HttpUtility.HtmlAttributeEncode(HttpUtility.JavaScriptStringEncode(editorUrl, true)),
                    string.Format("{0}_ckoptions", this.ClientID.Replace("-", string.Empty).Replace(".", string.Empty)),
                    Localization.GetString("Options.Text", SResXFile));
            }

            outWriter.Write("</p>");
            /////////////////
        }

        /// <summary>
        /// Re-Formats Url from the Url Control.
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

            return string.Format("FileID={0}", Utility.ConvertFilePathToFileId(inputUrl, this.portalSettings.PortalId));
        }

        /// <summary>Gets the container source.</summary>
        /// <returns>The container source path.</returns>
        private string GetContainerSource()
        {
            var containerSource = this.portalSettings.ActiveTab.ContainerSrc ?? this.portalSettings.DefaultPortalContainer;
            containerSource = this.ResolveSourcePath(containerSource);
            return containerSource;
        }

        /// <summary>Gets the skin source.</summary>
        /// <returns>The skin source path.</returns>
        private string GetSkinSource()
        {
            var skinSource = this.portalSettings.ActiveTab.SkinSrc ?? this.portalSettings.DefaultPortalSkin;
            skinSource = this.ResolveSourcePath(skinSource);
            return skinSource;
        }

        /// <summary>Resolves the source path.</summary>
        /// <param name="source">The source.</param>
        /// <returns>The source path of the select resource.</returns>
        private string ResolveSourcePath(string source)
        {
            source = "~" + source;
            return source;
        }

        /// <summary>
        /// Initializes the Editor.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void CKEditorInit(object sender, EventArgs e)
        {
            this.Page?.RegisterRequiresPostBack(this); // Ensures that postback is handled

            this.myParModule = (PortalModuleBase)FindModuleInstance(this);

            if (this.myParModule == null || this.myParModule.ModuleId == -1)
            {
                // Get Parent ModuleID From this ClientID
                string sClientId = this.ClientID.Substring(this.ClientID.IndexOf("ctr") + 3);

                sClientId = sClientId.Remove(this.ClientID.IndexOf("_"));

                if (!int.TryParse(sClientId, out this.parentModulId))
                {
                    // The is no real module, then use the "User Accounts" module (Profile editor)
                    ModuleController db = new ModuleController();
                    ModuleInfo objm = db.GetModuleByDefinition(this.portalSettings.PortalId, "User Accounts");

                    this.parentModulId = objm.TabModuleID;
                }
            }
            else
            {
                this.parentModulId = this.myParModule.ModuleId;
            }

            this.CheckFileBrowser();

            this.LoadAllSettings();

            ////if (!HasMsAjax)
            ////{
            ////   return;
            ////}

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
                    this.currentSettings.BrowserMode = BrowserType.CKFinder;
                    break;
                case "standard":
                    this.currentSettings.BrowserMode = BrowserType.StandardBrowser;
                    break;
                case "none":
                    this.currentSettings.BrowserMode = BrowserType.None;
                    break;
            }
        }

        /// <summary>
        /// The check image browser.
        /// </summary>
        private void CheckImageButton()
        {
            ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            Provider objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

            if (objProvider == null || string.IsNullOrEmpty(objProvider.Attributes["ck_imagebutton"]))
            {
                return;
            }

            switch (objProvider.Attributes["ck_imagebutton"])
            {
                case "easyimage":
                    this.currentSettings.ImageButtonMode = ImageButtonType.EasyImageButton;
                    break;
                case "standard":
                    this.currentSettings.ImageButtonMode = ImageButtonType.StandardButton;
                    break;
            }
        }

        /// <summary>
        /// Load Portal/Page/Module Settings.
        /// </summary>
        private void LoadAllSettings()
        {
            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(this.portalSettings.PortalId);

            // Load Default Settings
            this.currentSettings = SettingsUtil.GetDefaultSettings(
                this.portalSettings,
                this.portalSettings.HomeDirectoryMapPath,
                this.settings["configFolder"],
                portalRoles);

            // Set Current Mode to Default
            this.currentSettings.SettingMode = SettingsMode.Default;

            const string hostKey = "DNNCKH#";
            var portalKey = string.Format("DNNCKP#{0}#", this.portalSettings.PortalId);
            var pageKey = string.Format("DNNCKT#{0}#", this.portalSettings.ActiveTab.TabID);
            var moduleKey = string.Format("DNNCKMI#{0}#INS#{1}#", this.parentModulId, this.ID);

            // Load Host Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, hostKey))
            {
                var hostPortalRoles = RoleController.Instance.GetRoles(Host.HostPortalID);
                this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    this.currentSettings,
                    settingsDictionary,
                    hostKey,
                    hostPortalRoles);

                // Set Current Mode to Host
                this.currentSettings.SettingMode = SettingsMode.Host;

                // reset the roles to the correct portal
                if (this.portalSettings.PortalId != Host.HostPortalID)
                {
                    foreach (var toolbarRole in this.currentSettings.ToolBarRoles)
                    {
                        var roleName = hostPortalRoles.FirstOrDefault(role => role.RoleID == toolbarRole.RoleId)?.RoleName ?? string.Empty;
                        var roleId = portalRoles.FirstOrDefault(role => role.RoleName.Equals(roleName))?.RoleID ?? Null.NullInteger;
                        toolbarRole.RoleId = roleId;
                    }

                    foreach (var uploadRoles in this.currentSettings.UploadSizeRoles)
                    {
                        var roleName = hostPortalRoles.FirstOrDefault(role => role.RoleID == uploadRoles.RoleId)?.RoleName ?? string.Empty;
                        var roleId = portalRoles.FirstOrDefault(role => role.RoleName.Equals(roleName))?.RoleID ?? Null.NullInteger;
                        uploadRoles.RoleId = roleId;
                    }
                }
            }

            // Load Portal Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, portalKey))
            {
                /* throw new ApplicationException(settingsDictionary.FirstOrDefault(
                             setting => setting.Name.Equals(string.Format("{0}{1}", portalKey, "StartupMode"))).Value);*/

                this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings,
                    this.currentSettings,
                    settingsDictionary,
                    portalKey,
                    portalRoles);

                // Set Current Mode to Portal
                this.currentSettings.SettingMode = SettingsMode.Portal;
            }

            // Load Page Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey))
            {
                this.currentSettings = SettingsUtil.LoadEditorSettingsByKey(
                    this.portalSettings, this.currentSettings, settingsDictionary, pageKey, portalRoles);

                // Set Current Mode to Page
                this.currentSettings.SettingMode = SettingsMode.Page;
            }

            // Load Module Settings ?!
            if (!SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, this.parentModulId))
            {
                return;
            }

            this.currentSettings = SettingsUtil.LoadModuleSettings(
                this.portalSettings, this.currentSettings, moduleKey, this.parentModulId, portalRoles);

            // Set Current Mode to Module Instance
            this.currentSettings.SettingMode = SettingsMode.ModuleInstance;
        }

        /// <summary>
        /// Format the URL from FileID to File Path URL.
        /// </summary>
        /// <param name="inputUrl">
        /// The Input URL.
        /// </param>
        /// <returns>
        /// The formatted URL.
        /// </returns>
        private string FormatUrl(string inputUrl)
        {
            var formattedUrl = string.Empty;

            if (string.IsNullOrEmpty(inputUrl))
            {
                return formattedUrl;
            }

            if (inputUrl.StartsWith("http://") || inputUrl.StartsWith("https://") || inputUrl.StartsWith("//"))
            {
                formattedUrl = inputUrl;
            }
            else if (inputUrl.StartsWith("FileID="))
            {
                var fileId = int.Parse(inputUrl.Substring(7));

                var objFileInfo = FileManager.Instance.GetFile(fileId);

                formattedUrl = this.portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }
            else
            {
                formattedUrl = this.portalSettings.HomeDirectory + inputUrl;
            }

            return formattedUrl;
        }

        /// <summary>
        /// Load the Settings from the web.config file.
        /// </summary>
        private void LoadConfigSettings()
        {
            this.settings = new NameValueCollection();

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);
            if (providerConfiguration.Providers.ContainsKey(providerConfiguration.DefaultProvider))
            {
                var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

                foreach (string key in objProvider.Attributes)
                {
                    if (key.IndexOf("ck_", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        string sAdjustedKey = key.Substring(3, key.Length - 3);

                        // Do not ToLower settingKey, because CKConfig is case-Sensitive, exp: image2_prefillDimension
                        ////.ToLower();

                        if (!string.IsNullOrEmpty(sAdjustedKey))
                        {
                            this.settings[sAdjustedKey] = objProvider.Attributes[key];
                        }
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException(string.Format(
                    "Configuration error: default provider {0} doesn't exist in {1} providers",
                    providerConfiguration.DefaultProvider,
                    ProviderType));
            }
        }

        /// <summary>
        /// This registers a startup JavaScript with compatibility with the Microsoft Ajax.
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
            ScriptManager.RegisterStartupScript(this, this.GetType(), key, script, addScriptTags);
        }

        private void RegisterScript(string key, string script, bool addScriptTags)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), key, script, addScriptTags);
        }

        /// <summary>
        /// Registers the on submit statement.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="key">The key.</param>
        /// <param name="script">The script.</param>
        private void RegisterOnSubmitStatement(Type type, string key, string script)
        {
            ScriptManager.RegisterOnSubmitStatement(this, type, key, script);
        }

        /// <summary>
        /// Set Toolbar based on Current User.
        /// </summary>
        /// <param name="alternateConfigSubFolder">
        /// The alternate config sub folder.
        /// </param>
        /// <returns>
        /// Toolbar Name.
        /// </returns>
        private string SetUserToolbar(string alternateConfigSubFolder)
        {
            string toolbarName = this.CanUseFullToolbarAsDefault() ? "Full" : "Basic";

            var listToolbarSets = ToolbarUtil.GetToolbars(
                this.portalSettings.HomeDirectoryMapPath, alternateConfigSubFolder);

            var listUserToolbarSets = new List<ToolbarSet>();

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
                var role = RoleController.Instance.GetRoleById(this.portalSettings.PortalId, roleToolbar.RoleId);

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
            }

            if (listUserToolbarSets.Count <= 0)
            {
                return toolbarName;
            }

            // Compare The User Toolbars if the User is more then One Role, and apply the Toolbar with the Highest Priority
            int iHighestPrio = listUserToolbarSets.Max(toolb => toolb.Priority);

            return ToolbarUtil.FindHighestToolbar(listUserToolbarSets, iHighestPrio).Name;
        }

        private bool CanUseFullToolbarAsDefault()
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return false;
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            return currentUser.IsSuperUser || PortalSecurity.IsInRole(this.portalSettings.AdministratorRoleName);
        }

        /// <summary>
        /// Registers the CKEditor library.
        /// </summary>
        private void RegisterCKEditorLibrary()
        {
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorToolBars.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CKEditorOverride.css"));
            ClientResourceManager.RegisterStyleSheet(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.15.1/editor.css"));

            ClientScriptManager cs = this.Page.ClientScript;

            Type csType = this.GetType();

            const string CsName = "CKEdScript";
            const string CsAdaptName = "CKAdaptScript";
            const string CsFindName = "CKFindScript";

            JavaScript.RequestRegistration(CommonJs.jQuery);

            // Inject jQuery if editor is loaded in a RadWindow
            if (HttpContext.Current.Request.QueryString["rwndrnd"] != null)
            {
                ScriptManager.RegisterClientScriptInclude(
                    this, csType, "jquery_registered", "//ajax.googleapis.com/ajax/libs/jquery/1/jquery.min.js");
            }

            if (File.Exists(this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.15.1/ckeditor.js"))
                && !cs.IsClientScriptIncludeRegistered(csType, CsName))
            {
                cs.RegisterClientScriptInclude(
                    csType, CsName, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/ckeditor/4.15.1/ckeditor.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath(
                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/jquery.ckeditor.adapter.js"))
                && !cs.IsClientScriptIncludeRegistered(csType, CsAdaptName))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    CsAdaptName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/jquery.ckeditor.adapter.js"));
            }

            if (
                File.Exists(
                    this.Context.Server.MapPath("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js")) &&
                !cs.IsClientScriptIncludeRegistered(csType, CsFindName) && this.currentSettings.BrowserMode.Equals(BrowserType.CKFinder))
            {
                cs.RegisterClientScriptInclude(
                    csType,
                    CsFindName,
                    Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.js"));
            }

            ClientResourceManager.RegisterScript(this.Page, Globals.ResolveUrl("~/Providers/HtmlEditorProviders/DNNConnect.CKE/js/editorOverride.js"));

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

            var postBackScript = string.Format(
                @" if (CKEDITOR && CKEDITOR.instances && CKEDITOR.instances.{0}) {{ CKEDITOR.instances.{0}.updateElement();  if (typeof Page_IsValid !== 'undefined' && Page_IsValid) CKEDITOR.instances.{0}.destroy(); }}",
                editorFixedId);

            this.RegisterOnSubmitStatement(
                this.GetType(), string.Format("CKEditor_OnAjaxSubmit_{0}", editorFixedId), postBackScript);

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
            var editorConfigScript = new StringBuilder();
            editorConfigScript.AppendFormat("var editorConfig{0} = {{", editorVar);

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

                    editorConfigScript.AppendFormat("{0}:{1}", key, value);

                    editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
                }
                else
                {
                    if (key == "browser")
                    {
                        continue;
                    }

                    editorConfigScript.AppendFormat("{0}:\'{1}\'", key, value);

                    editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
                }
            }

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

            editorScript.Append("if(CKEDITOR && CKEDITOR.config){");
            editorScript.Append("  CKEDITOR.config.portalId = " + this.portalSettings.PortalId);
            editorScript.Append("};");

            // End of LoadScript
            editorScript.Append("}");

            this.RegisterScript(string.Format(@"{0}_CKE_Config", editorFixedId), editorConfigScript.ToString(), true);
            this.RegisterStartupScript(string.Format(@"{0}_CKE_Startup", editorFixedId), editorScript.ToString(), true);
        }
    }
}
