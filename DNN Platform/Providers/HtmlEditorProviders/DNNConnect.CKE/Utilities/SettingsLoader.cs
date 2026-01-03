// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
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
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Framework.Providers;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;

    /// <summary>
    /// Settings Loader class.
    /// </summary>
    public class SettingsLoader
    {
        /// <summary>
        /// Loads the settings.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="parentModuleId">The parent module identifier.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <param name="configFolder">The configuration folder.</param>
        /// <returns>The EditorProviderSettings.</returns>
        public static EditorProviderSettings LoadSettings(PortalSettings portalSettings, int parentModuleId, string controlId, string configFolder)
        {
            var currentEditorSettings = new EditorProviderSettings();

            // Set File Browser Mode
            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration("htmlEditor");
            if (providerConfiguration != null && providerConfiguration.Providers.ContainsKey(providerConfiguration.DefaultProvider))
            {
                var objProvider = (Provider)providerConfiguration.Providers[providerConfiguration.DefaultProvider];

                if (objProvider != null && !string.IsNullOrEmpty(objProvider.Attributes["ck_browser"]))
                {
                    switch (objProvider.Attributes["ck_browser"])
                    {
                        case "ckfinder":
                            currentEditorSettings.BrowserMode = BrowserType.CKFinder;
                            break;
                        case "standard":
                            currentEditorSettings.BrowserMode = BrowserType.StandardBrowser;
                            break;
                        case "none":
                            currentEditorSettings.BrowserMode = BrowserType.None;
                            break;
                    }
                }
            }

            var settingsDictionary = EditorController.GetEditorHostSettings();
            var portalRoles = RoleController.Instance.GetRoles(portalSettings.PortalId);

            // Load Default Settings
            currentEditorSettings = SettingsUtil.GetDefaultSettings(
                portalSettings,
                portalSettings.HomeDirectoryMapPath,
                configFolder,
                portalRoles);

            // Set Current Mode to Default
            currentEditorSettings.SettingMode = SettingsMode.Default;

            var hostKey = SettingConstants.HostKey;
            var portalKey = SettingConstants.PortalKey(portalSettings.PortalId);
            var pageKey = $"DNNCKT#{portalSettings.ActiveTab.TabID}#";
            var moduleKey = $"DNNCKMI#{parentModuleId}#INS#{controlId}#";

            // Load Host Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, hostKey))
            {
                var hostPortalRoles = RoleController.Instance.GetRoles(Host.HostPortalID);
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings,
                    currentEditorSettings,
                    settingsDictionary,
                    hostKey,
                    hostPortalRoles);

                // Set Current Mode to Host
                currentEditorSettings.SettingMode = SettingsMode.Host;

                // reset the roles to the correct portal
                if (portalSettings.PortalId != Host.HostPortalID)
                {
                    foreach (var toolbarRole in currentEditorSettings.ToolBarRoles)
                    {
                        var roleName = hostPortalRoles.FirstOrDefault(role => role.RoleID == toolbarRole.RoleId)?.RoleName ?? string.Empty;
                        var roleId = portalRoles.FirstOrDefault(role => role.RoleName.Equals(roleName))?.RoleID ?? Null.NullInteger;
                        toolbarRole.RoleId = roleId;
                    }

                    foreach (var uploadRoles in currentEditorSettings.UploadSizeRoles)
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
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings,
                    currentEditorSettings,
                    settingsDictionary,
                    portalKey,
                    portalRoles);

                // Set Current Mode to Portal
                currentEditorSettings.SettingMode = SettingsMode.Portal;
            }

            // Load Page Settings ?!
            if (SettingsUtil.CheckSettingsExistByKey(settingsDictionary, pageKey))
            {
                currentEditorSettings = SettingsUtil.LoadEditorSettingsByKey(
                    portalSettings, currentEditorSettings, settingsDictionary, pageKey, portalRoles);

                // Set Current Mode to Page
                currentEditorSettings.SettingMode = SettingsMode.Page;
            }

            // Load Module Settings ?!
            if (!SettingsUtil.CheckExistsModuleInstanceSettings(moduleKey, parentModuleId))
            {
                return currentEditorSettings;
            }

            currentEditorSettings = SettingsUtil.LoadModuleSettings(
                portalSettings, currentEditorSettings, moduleKey, parentModuleId, portalRoles);

            // Set Current Mode to Module Instance
            currentEditorSettings.SettingMode = SettingsMode.ModuleInstance;

            return currentEditorSettings;
        }

        /// <summary>Load the Settings from the web.config file.</summary>
        /// <param name="providerType">The provider type.</param>
        /// <returns>The settings collection.</returns>
        public static NameValueCollection LoadConfigSettings(string providerType)
        {
            var settings = new NameValueCollection();

            var providerConfiguration = ProviderConfiguration.GetProviderConfiguration(providerType);
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
                            settings[sAdjustedKey] = objProvider.Attributes[key];
                        }
                    }
                }
            }
            else
            {
                throw new ConfigurationErrorsException(string.Format(
                    "Configuration error: default provider {0} doesn't exist in {1} providers",
                    providerConfiguration.DefaultProvider,
                    providerType));
            }

            return settings;
        }

        /// <summary>
        /// Populates the settings.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="currentEditorSettings">The current editor settings.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="moduleConfiguration">The module configuration.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="controlId">The control identifier.</param>
        /// <param name="parentModuleId">The parent module identifier.</param>
        /// <param name="toolBarNameOverride">The tool bar name override.</param>
        public static void PopulateSettings(
            NameValueCollection settings,
            EditorProviderSettings currentEditorSettings,
            PortalSettings portalSettings,
            ModuleInfo moduleConfiguration,
            NameValueCollection attributes,
            Unit width,
            Unit height,
            string controlId,
            int parentModuleId,
            string toolBarNameOverride)
        {
            // Override local settings with attributes
            foreach (string key in attributes.Keys)
            {
                settings[key] = attributes[key];
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
                    if (info.GetValue(currentEditorSettings.Config, null) == null)
                    {
                        continue;
                    }

                    var rawValue = info.GetValue(currentEditorSettings.Config, null);

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
                    settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                }
                else
                {
                    switch (info.Name)
                    {
                        case "ToolbarLocation":
                            settings[xmlAttributeAttribute.AttributeName] = settingValue.ToLower();
                            break;
                        case "EnterMode":
                        case "ShiftEnterMode":
                            switch (settingValue)
                            {
                                case "P":
                                    settings[xmlAttributeAttribute.AttributeName] = "1";
                                    break;
                                case "BR":
                                    settings[xmlAttributeAttribute.AttributeName] = "2";
                                    break;
                                case "DIV":
                                    settings[xmlAttributeAttribute.AttributeName] = "3";
                                    break;
                            }

                            break;
                        case "ContentsLangDirection":
                            {
                                switch (settingValue)
                                {
                                    case "LeftToRight":
                                        settings[xmlAttributeAttribute.AttributeName] = "ltr";
                                        break;
                                    case "RightToLeft":
                                        settings[xmlAttributeAttribute.AttributeName] = "rtl";
                                        break;
                                    default:
                                        settings[xmlAttributeAttribute.AttributeName] = string.Empty;
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
                                        currentEditorSettings.Config.CodeMirror, null);

                                    var codeMirrorSettingValue = rawSettingValue.ToString();

                                    if (string.IsNullOrEmpty(codeMirrorSettingValue))
                                    {
                                        continue;
                                    }

                                    switch (codeMirrorInfo.PropertyType.Name)
                                    {
                                        case "String":
                                            codeMirrorArray.AppendFormat("{0}: '{1}',", xmlAttribute.AttributeName, HttpUtility.JavaScriptStringEncode(codeMirrorSettingValue));
                                            break;
                                        case "Boolean":
                                            codeMirrorArray.AppendFormat("{0}: {1},", xmlAttribute.AttributeName, codeMirrorSettingValue.ToLower());
                                            break;
                                    }
                                }

                                var codemirrorSettings = codeMirrorArray.ToString();

                                settings["codemirror"] =
                                    $"{{ {codemirrorSettings.Remove(codemirrorSettings.Length - 1, 1)} }}";
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
                                        wordCountInfo.GetValue(currentEditorSettings.Config.WordCount, null);

                                    var wordCountSettingValue = rawSettingValue.ToString();

                                    if (string.IsNullOrEmpty(wordCountSettingValue))
                                    {
                                        continue;
                                    }

                                    switch (wordCountInfo.PropertyType.Name)
                                    {
                                        case "String":
                                            wordcountArray.AppendFormat("{0}: '{1}',", xmlAttribute.AttributeName, HttpUtility.JavaScriptStringEncode(wordCountSettingValue));
                                            break;
                                        case "Boolean":
                                            wordcountArray.AppendFormat("{0}: {1},", xmlAttribute.AttributeName, wordCountSettingValue.ToLower());
                                            break;
                                    }
                                }

                                var wordcountSettings = wordcountArray.ToString();

                                settings["wordcount"] =
                                    $"{{ {wordcountSettings.Remove(wordcountSettings.Length - 1, 1)} }}";
                            }

                            break;
                        default:
                            settings[xmlAttributeAttribute.AttributeName] = settingValue;
                            break;
                    }
                }
            }

            try
            {
                var currentCulture = Thread.CurrentThread.CurrentUICulture;

                settings["language"] = currentCulture.Name.ToLowerInvariant();

                if (string.IsNullOrEmpty(currentEditorSettings.Config.Scayt_sLang))
                {
                    // 'en-us' is not a language code that is supported, the correct is 'en_US'
                    // https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html#cfg-scayt_sLang
                    settings["scayt_sLang"] = currentCulture.Name.Replace("-", "_");
                }
            }
            catch (Exception)
            {
                settings["language"] = "en";
            }

            if (!string.IsNullOrEmpty(currentEditorSettings.Config.CustomConfig))
            {
                settings["customConfig"] = FormatUrl(portalSettings, currentEditorSettings.Config.CustomConfig);
            }
            else
            {
                settings["customConfig"] = string.Empty;
            }

            if (!string.IsNullOrEmpty(currentEditorSettings.Config.Skin))
            {
                if (currentEditorSettings.Config.Skin.Equals("office2003")
                    || currentEditorSettings.Config.Skin.Equals("BootstrapCK-Skin")
                    || currentEditorSettings.Config.Skin.Equals("chris")
                    || currentEditorSettings.Config.Skin.Equals("v2"))
                {
                    settings["skin"] = "moono";
                }
                else
                {
                    settings["skin"] = currentEditorSettings.Config.Skin;
                }
            }

            settings["linkDefaultProtocol"] = currentEditorSettings.DefaultLinkProtocol.ToSettingValue();

            var cssFiles = new List<string>();
            var skinSrc = GetSkinSourcePath(portalSettings);
            var containerSrc = GetContainerSourcePath(portalSettings);

            cssFiles.Add("~/portals/_default/default.css");
            cssFiles.Add(skinSrc.Replace(skinSrc.Substring(skinSrc.LastIndexOf('/'), skinSrc.Length - skinSrc.Substring(0, skinSrc.LastIndexOf('/')).Length), "/skin.css"));
            cssFiles.Add(containerSrc.Replace(containerSrc.Substring(containerSrc.LastIndexOf('/'), containerSrc.Length - containerSrc.Substring(0, containerSrc.LastIndexOf('/')).Length), "/container.css"));
            if (moduleConfiguration != null && moduleConfiguration.ModuleID > -1)
            {
                cssFiles.Add("~/DesktopModules/" + moduleConfiguration.DesktopModule.FolderName + "/module.css");
            }

            cssFiles.Add("~" + portalSettings.HomeDirectory + "portal.css");
            cssFiles.Add("~/Providers/HtmlEditorProviders/DNNConnect.CKE/css/CkEditorContents.css");

            var resolvedCssFiles = cssFiles.Where(cssFile => File.Exists(Globals.ApplicationMapPath + Globals.ResolveUrl(cssFile).Replace("/", "\\"))).Select(Globals.ResolveUrl).ToList();

            if (!string.IsNullOrEmpty(currentEditorSettings.Config.ContentsCss))
            {
                var customCss = Globals.ResolveUrl(FormatUrl(portalSettings, currentEditorSettings.Config.ContentsCss));
                resolvedCssFiles.Add(customCss);
            }

            var serializer = new JavaScriptSerializer();
            settings["contentsCss"] = serializer.Serialize(resolvedCssFiles);

            if (!string.IsNullOrEmpty(currentEditorSettings.Config.Templates_Files))
            {
                var templateUrl = FormatUrl(portalSettings, currentEditorSettings.Config.Templates_Files);

                var templateFile = templateUrl.EndsWith(".xml") ? $"xml:{templateUrl}" : templateUrl;
                settings["templates_files"] =
                    $"[ '{HttpUtility.JavaScriptStringEncode(templateFile)}' ]";
            }

            if (!string.IsNullOrEmpty(toolBarNameOverride))
            {
                settings["toolbar"] = toolBarNameOverride;
            }
            else
            {
                var toolbarName = SetUserToolbar(settings["configFolder"], portalSettings, currentEditorSettings);

                var listToolbarSets = ToolbarUtil.GetToolbars(portalSettings.HomeDirectoryMapPath, settings["configFolder"]);

                var toolbarSet = listToolbarSets.FirstOrDefault(toolbar => toolbar.Name.Equals(toolbarName));

                var toolbarSetString = ToolbarUtil.ConvertToolbarSetToString(toolbarSet, true);

                settings["toolbar"] = $"[{toolbarSetString}]";
            }

            // Easy Image Upload
            if (currentEditorSettings.ImageButtonMode == ImageButtonType.EasyImageButton)
            {
                // replace 'Image' Plugin with 'EasyImage'
                settings["toolbar"] = settings["toolbar"].Replace("'Image'", "'EasyImageUpload'");

                // add the plugin in extraPlugins
                if (string.IsNullOrEmpty(settings["extraPlugins"]) || !settings["extraPlugins"].Split(',').Contains("easyimage"))
                {
                    if (!string.IsNullOrEmpty(settings["extraPlugins"]))
                    {
                        settings["extraPlugins"] += ",";
                    }

                    settings["extraPlugins"] += "easyimage";
                }

                // change the easyimage toolbar
                settings["easyimage_toolbar"] = "['EasyImageAlt']";

                // remove the image plugin in removePlugins
                if (string.IsNullOrEmpty(settings["removePlugins"]) || !settings["removePlugins"].Split(',').Contains("image"))
                {
                    if (!string.IsNullOrEmpty(settings["removePlugins"]))
                    {
                        settings["removePlugins"] += ",";
                    }

                    settings["removePlugins"] += "image";
                }

                settings.Add("cloudServices_uploadUrl", Globals.ResolveUrl(
                    string.Format(
                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=EasyImageUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                        portalSettings.ActiveTab.TabID,
                        portalSettings.PortalId,
                        parentModuleId,
                        controlId,
                        currentEditorSettings.SettingMode,
                        CultureInfo.CurrentCulture.Name)));
            }
            else
            {
                // remove the easyimage plugin in removePlugins
                if (string.IsNullOrEmpty(settings["removePlugins"]) || !settings["removePlugins"].Split(',').Contains("easyimage"))
                {
                    if (!string.IsNullOrEmpty(settings["removePlugins"]))
                    {
                        settings["removePlugins"] += ",";
                    }

                    settings["removePlugins"] += "easyimage";
                }
            }

            // cloudservices variables need to be set regardless
            var tokenUrl = ServicesFramework.GetServiceFrameworkRoot() + "API/CKEditorProvider/CloudServices/GetToken";
            settings.Add("cloudServices_tokenUrl", tokenUrl);

            // Editor Width
            if (!string.IsNullOrEmpty(currentEditorSettings.Config.Width))
            {
                settings["width"] = currentEditorSettings.Config.Width;
            }
            else
            {
                if (width.Value > 0)
                {
                    settings["width"] = width.ToString();
                }
            }

            // Editor Height
            if (!string.IsNullOrEmpty(currentEditorSettings.Config.Height))
            {
                settings["height"] = currentEditorSettings.Config.Height;
            }
            else
            {
                if (height.Value > 0)
                {
                    settings["height"] = height.ToString();
                }
            }

            if (!string.IsNullOrEmpty(settings["extraPlugins"])
                && settings["extraPlugins"].Contains("xmlstyles"))
            {
                settings["extraPlugins"] = settings["extraPlugins"].Replace(",xmlstyles", string.Empty);
            }

            // fix oEmbed/oembed issue and other bad settings
            if (!string.IsNullOrEmpty(settings["extraPlugins"])
                && settings["extraPlugins"].Contains("oEmbed"))
            {
                settings["extraPlugins"] = settings["extraPlugins"].Replace("oEmbed", "oembed");
            }

            if (settings["PasteFromWordCleanupFile"] != null
                && settings["PasteFromWordCleanupFile"].Equals("default"))
            {
                settings["PasteFromWordCleanupFile"] = string.Empty;
            }

            if (settings["menu_groups"] != null
                && settings["menu_groups"].Equals("clipboard,table,anchor,link,image"))
            {
                settings["menu_groups"] =
                    "clipboard,tablecell,tablecellproperties,tablerow,tablecolumn,table,anchor,link,image,flash,checkbox,radio,textfield,hiddenfield,imagebutton,button,select,textarea,div";
            }

            // Inject maxFileSize
            settings["maxFileSize"] = Utility.GetMaxUploadSize().ToString();

            HttpContext.Current.Session["CKDNNtabid"] = portalSettings.ActiveTab.TabID;
            HttpContext.Current.Session["CKDNNporid"] = portalSettings.PortalId;

            // Add FileBrowser
            switch (currentEditorSettings.BrowserMode)
            {
                case BrowserType.StandardBrowser:
                    {
                        settings["filebrowserBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Link&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId,
                                    parentModuleId,
                                    controlId,
                                    currentEditorSettings.SettingMode,
                                    CultureInfo.CurrentCulture.Name));
                        settings["filebrowserImageBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Image&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId,
                                    parentModuleId,
                                    controlId,
                                    currentEditorSettings.SettingMode,
                                    CultureInfo.CurrentCulture.Name));
                        settings["filebrowserFlashBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Type=Flash&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId,
                                    parentModuleId,
                                    controlId,
                                    currentEditorSettings.SettingMode,
                                    CultureInfo.CurrentCulture.Name));

                        if (currentEditorSettings.ImageButtonMode == ImageButtonType.StandardButton && Utility.CheckIfUserHasFolderWriteAccess(currentEditorSettings.UploadDirId, portalSettings))
                        {
                            settings["filebrowserUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FileUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId,
                                        parentModuleId,
                                        controlId,
                                        currentEditorSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            settings["filebrowserFlashUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=FlashUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId,
                                        parentModuleId,
                                        controlId,
                                        currentEditorSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                            settings["filebrowserImageUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/Browser/Browser.aspx?Command=ImageUpload&tabid={0}&PortalID={1}&mid={2}&ckid={3}&mode={4}&lang={5}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId,
                                        parentModuleId,
                                        controlId,
                                        currentEditorSettings.SettingMode,
                                        CultureInfo.CurrentCulture.Name));
                        }

                        settings["filebrowserWindowWidth"] = "870";
                        settings["filebrowserWindowHeight"] = "800";

                        // Set Browser Authorize
                        const bool isAuthorized = true;

                        HttpContext.Current.Session["CKE_DNNIsAuthorized"] = isAuthorized;

                        DataCache.SetCache("CKE_DNNIsAuthorized", isAuthorized);
                    }

                    break;
                case BrowserType.CKFinder:
                    {
                        settings["filebrowserBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?tabid={0}&PortalID={1}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId));
                        settings["filebrowserImageBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?type=Images&tabid={0}&PortalID={1}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId));
                        settings["filebrowserFlashBrowseUrl"] =
                            Globals.ResolveUrl(
                                string.Format(
                                    "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/ckfinder.html?type=Flash&tabid={0}&PortalID={1}",
                                    portalSettings.ActiveTab.TabID,
                                    portalSettings.PortalId));

                        if (Utility.CheckIfUserHasFolderWriteAccess(currentEditorSettings.UploadDirId, portalSettings))
                        {
                            settings["filebrowserUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Files&tabid={0}&PortalID={1}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId));
                            settings["filebrowserFlashUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Flash&tabid={0}&PortalID={1}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId));
                            settings["filebrowserImageUploadUrl"] =
                                Globals.ResolveUrl(
                                    string.Format(
                                        "~/Providers/HtmlEditorProviders/DNNConnect.CKE/ckfinder/core/connector/aspx/connector.aspx?command=QuickUpload&type=Images&tabid={0}&PortalID={1}",
                                        portalSettings.ActiveTab.TabID,
                                        portalSettings.PortalId));
                        }

                        HttpContext.Current.Session["CKDNNSubDirs"] = currentEditorSettings.SubDirs;

                        HttpContext.Current.Session["CKDNNRootDirId"] = currentEditorSettings.BrowserRootDirId;
                        HttpContext.Current.Session["CKDNNRootDirForImgId"] = currentEditorSettings.BrowserRootDirId;
                        HttpContext.Current.Session["CKDNNUpDirId"] = currentEditorSettings.UploadDirId;
                        HttpContext.Current.Session["CKDNNUpDirForImgId"] = currentEditorSettings.UploadDirId;

                        // Set Browser Authorize
                        const bool isAuthorized = true;

                        HttpContext.Current.Session["CKE_DNNIsAuthorized"] = isAuthorized;

                        DataCache.SetCache("CKE_DNNIsAuthorized", isAuthorized);
                    }

                    break;
            }
        }

        /// <summary>
        /// Gets the editor configuration script.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="editorVar">The editor variable name.</param>
        /// <returns>The editor configuration script.</returns>
        public static string GetEditorConfigScript(NameValueCollection settings, string editorVar)
        {
            var editorConfigScript = new StringBuilder();
            editorConfigScript.AppendFormat("var editorConfig{0} = {{", editorVar);

            var keysCount = settings.Keys.Count;
            var currentCount = 0;

            foreach (string key in settings.Keys)
            {
                var value = settings[key];

                currentCount++;

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

                    editorConfigScript.AppendFormat("{0}:'{1}'", key, HttpUtility.JavaScriptStringEncode(value));

                    editorConfigScript.Append(currentCount == keysCount ? "};" : ",");
                }
            }

            return editorConfigScript.ToString();
        }

        private static string FormatUrl(PortalSettings portalSettings, string inputUrl)
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

                formattedUrl = portalSettings.HomeDirectory + objFileInfo.Folder + objFileInfo.FileName;
            }
            else
            {
                formattedUrl = portalSettings.HomeDirectory + inputUrl;
            }

            return formattedUrl;
        }

        private static string GetSkinSourcePath(PortalSettings portalSettings)
        {
            var skinSource = portalSettings.ActiveTab.SkinSrc ?? portalSettings.DefaultPortalSkin;
            skinSource = ResolveSourcePath(skinSource);
            return skinSource;
        }

        private static string GetContainerSourcePath(PortalSettings portalSettings)
        {
            var containerSource = portalSettings.ActiveTab.ContainerSrc ?? portalSettings.DefaultPortalContainer;
            containerSource = ResolveSourcePath(containerSource);
            return containerSource;
        }

        private static string ResolveSourcePath(string source)
        {
            source = "~" + source;
            return source;
        }

        private static string SetUserToolbar(string alternateConfigSubFolder, PortalSettings portalSettings, EditorProviderSettings currentEditorSettings)
        {
            string toolbarName = CanUseFullToolbarAsDefault(portalSettings) ? "Full" : "Basic";

            var listToolbarSets = ToolbarUtil.GetToolbars(
                portalSettings.HomeDirectoryMapPath, alternateConfigSubFolder);

            var listUserToolbarSets = new List<ToolbarSet>();

            if (currentEditorSettings.ToolBarRoles.Count <= 0)
            {
                return toolbarName;
            }

            foreach (var roleToolbar in currentEditorSettings.ToolBarRoles)
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
                var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleToolbar.RoleId);

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

        private static bool CanUseFullToolbarAsDefault(PortalSettings portalSettings)
        {
            if (!HttpContext.Current.Request.IsAuthenticated)
            {
                return false;
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            return currentUser.IsSuperUser || PortalSecurity.IsInRole(portalSettings.AdministratorRoleName);
        }
    }
}
