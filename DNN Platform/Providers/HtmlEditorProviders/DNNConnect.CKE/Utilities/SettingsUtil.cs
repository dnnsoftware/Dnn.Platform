using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Objects;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security;
using DotNetNuke.Security.Roles;
using DotNetNuke.Common.Utilities;

namespace DNNConnect.CKEditorProvider.Utilities
{
    using DotNetNuke.Entities.Host;

    /// <summary>
    /// Settings Base Helper Class
    /// </summary>
    public class SettingsUtil
    {
        #region Public Methods

        /// <summary>
        /// Checks the exists portal or page settings.
        /// </summary>
        /// <param name="editorHostSettings">The editor host settings.</param>
        /// <param name="key">The key.</param>
        /// <returns>
        /// Returns if Portal or Page Settings Exists
        /// </returns>
        internal static bool CheckSettingsExistByKey(List<EditorHostSetting> editorHostSettings, string key)
        {
            if (
                editorHostSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SKIN))))
            {
                return
                    !string.IsNullOrEmpty(
                        editorHostSettings.FirstOrDefault(
                            setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SKIN))).Value);
            }

            // No Settings Found
            return false;
        }

        /// <summary>
        /// Checks there are any Module Settings
        /// </summary>
        /// <param name="moduleKey">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <returns>Returns if The Module Settings Exists or not.</returns>
        internal static bool CheckExistsModuleSettings(string moduleKey, int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            if (module != null)
            {
                var hshModSet = module.ModuleSettings;
                return hshModSet != null && hshModSet.Keys.Cast<string>().Any(key => key.StartsWith(moduleKey));
            }
            return false;
        }

        /// <summary>
        /// Checks the exists of the module instance settings.
        /// </summary>
        /// <param name="moduleKey">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <returns>Returns if The Module Settings Exists or not.</returns>
        internal static bool CheckExistsModuleInstanceSettings(string moduleKey, int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            if (module != null)
            {
                var hshModSet = module.ModuleSettings;

                return hshModSet != null && !string.IsNullOrEmpty((string) hshModSet[string.Format("{0}skin", moduleKey)]);
            }
            return false;
        }

        /// <summary>
        /// Loads the portal or page settings.
        /// </summary>
        /// <param name="portalSettings">The current portal settings.</param>
        /// <param name="currentSettings">The current settings.</param>
        /// <param name="editorHostSettings">The editor host settings.</param>
        /// <param name="key">The Portal or Page key.</param>
        /// <param name="portalRoles">The Portal Roles</param>
        /// <returns>
        /// Returns the Filled Settings
        /// </returns>
        internal static EditorProviderSettings LoadEditorSettingsByKey(
            PortalSettings portalSettings,
            EditorProviderSettings currentSettings,
            List<EditorHostSetting> editorHostSettings,
            string key,
            IList<RoleInfo> portalRoles)
        {
            var roles = new ArrayList();

            // Import all Editor config settings
            var props = GetEditorConfigProperties().ToList();
            var filteredSettings = editorHostSettings.Where(s => s.Name.StartsWith(key)).ToList();

            foreach (PropertyInfo info in props)
            {
                if (!filteredSettings.Any(s => s.Name.Equals(string.Format("{0}{1}", key, info.Name))))
                {
                    if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount"))
                    {
                        continue;
                    }
                }

                var settingValue = string.Empty;
                if (!info.Name.Equals("CodeMirror") && !info.Name.Equals("WordCount"))
                {
                    settingValue =
                        filteredSettings.FirstOrDefault(
                            setting => setting.Name.Equals(string.Format("{0}{1}", key, info.Name))).Value;

                    if (string.IsNullOrEmpty(settingValue))
                    {
                        continue;
                    }

                    switch (info.PropertyType.Name)
                    {
                        case "String":
                            info.SetValue(currentSettings.Config, settingValue, null);
                            break;
                        case "Int32":
                            info.SetValue(currentSettings.Config, int.Parse(settingValue), null);
                            break;
                        case "Decimal":
                            info.SetValue(currentSettings.Config, decimal.Parse(settingValue), null);
                            break;
                        case "Boolean":
                            info.SetValue(currentSettings.Config, bool.Parse(settingValue), null);
                            break;
                    }
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)Enum.Parse(typeof(ToolBarLocation), settingValue),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)Enum.Parse(typeof(LinkType), settingValue),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)Enum.Parse(typeof(EnterModus), settingValue),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)Enum.Parse(typeof(LanguageDirection), settingValue),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in
                            typeof(CodeMirror).GetProperties()
                                .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            settingValue = filteredSettings.FirstOrDefault(setting => setting.Name.Equals(string.Format("{0}{1}", key, codeMirrorInfo.Name))).Value;
                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (
                                        filteredSettings.Any(
                                            s => s.Name.Equals(string.Format("{0}{1}", key, codeMirrorInfo.Name))))
                                    {
                                        codeMirrorInfo.SetValue(currentSettings.Config.CodeMirror, settingValue, null);
                                    }

                                    break;
                                case "Boolean":
                                    if (
                                        filteredSettings.Any(
                                            s => s.Name.Equals(string.Format("{0}{1}", key, codeMirrorInfo.Name))))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            bool.Parse(settingValue),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                    case "WordCount":
                        foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                        {
                            settingValue = filteredSettings.FirstOrDefault(setting => setting.Name.Equals(string.Format("{0}{1}", key, wordCountInfo.Name))).Value;
                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (
                                        filteredSettings.Any(
                                            s => s.Name.Equals(string.Format("{0}{1}", key, wordCountInfo.Name))))
                                    {
                                        wordCountInfo.SetValue(currentSettings.Config.WordCount, settingValue, null);
                                    }

                                    break;
                                case "Boolean":
                                    if (
                                        filteredSettings.Any(
                                            s => s.Name.Equals(string.Format("{0}{1}", key, wordCountInfo.Name))))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            bool.Parse(settingValue),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SKIN))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SKIN))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Skin = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.CodeMirror.Theme = settingValue;
                }
            }

            List<ToolbarRoles> listToolbarRoles = (from RoleInfo objRole in portalRoles
                                                   where
                                                       filteredSettings.Any(
                                                           setting =>
                                                           setting.Name.Equals(
                                                               string.Format(
                                                                   "{0}{2}#{1}",
                                                                   key,
                                                                   objRole.RoleID,
                                                                   SettingConstants.TOOLB)))
                                                   where
                                                       !string.IsNullOrEmpty(
                                                           filteredSettings.FirstOrDefault(
                                                               s =>
                                                               s.Name.Equals(
                                                                   string.Format(
                                                                       "{0}{2}#{1}",
                                                                       key,
                                                                       objRole.RoleID,
                                                                       SettingConstants.TOOLB))).Value)
                                                   let sToolbar =
                                                       filteredSettings.FirstOrDefault(
                                                           s =>
                                                           s.Name.Equals(
                                                               string.Format(
                                                                   "{0}{2}#{1}",
                                                                   key,
                                                                   objRole.RoleID,
                                                                   SettingConstants.TOOLB))).Value
                                                   select
                                                       new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar })
                .ToList();

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = settingValue });
                }
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = (from RoleInfo objRole in portalRoles
                                       where
                                           filteredSettings.Any(
                                               setting =>
                                               setting.Name.Equals(
                                                   string.Format(
                                                       "{0}{1}#{2}",
                                                       key,
                                                       objRole.RoleID,
                                                       SettingConstants.UPLOADFILELIMITS)))
                                       where
                                           !string.IsNullOrEmpty(
                                               filteredSettings.FirstOrDefault(
                                                   s =>
                                                   s.Name.Equals(
                                                       string.Format(
                                                           "{0}{1}#{2}",
                                                           key,
                                                           objRole.RoleID,
                                                           SettingConstants.UPLOADFILELIMITS))).Value)
                                       let uploadFileLimit =
                                           filteredSettings.FirstOrDefault(
                                               s =>
                                               s.Name.Equals(
                                                   string.Format(
                                                       "{0}{1}#{2}",
                                                       key,
                                                       objRole.RoleID,
                                                       SettingConstants.UPLOADFILELIMITS))).Value
                                       select
                                           new UploadSizeRoles { RoleId = objRole.RoleID, RoleName = objRole.RoleName, UploadFileLimit = Convert.ToInt32(uploadFileLimit) })
                .ToList();

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}#{2}", key, "-1", SettingConstants.UPLOADFILELIMITS))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}#{2}", key, "-1", SettingConstants.UPLOADFILELIMITS))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    listUploadSizeRoles.Add(new UploadSizeRoles { RoleId = -1, UploadFileLimit = Convert.ToInt32(settingValue) });
                }
            }

            currentSettings.UploadSizeRoles = listUploadSizeRoles;

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.ROLES))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.ROLES))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    string sRoles = settingValue;

                    currentSettings.BrowserRoles = sRoles;

                    string[] rolesA = sRoles.Split(';');

                    foreach (string sRoleName in rolesA)
                    {
                        if (Utility.IsNumeric(sRoleName))
                        {
                            RoleInfo roleInfo = RoleController.Instance.GetRoleById(portalSettings.PortalId, int.Parse(sRoleName));

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(sRoleName);
                        }
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BROWSER))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BROWSER))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Browser = settingValue;

                    switch (currentSettings.Browser)
                    {
                        case "ckfinder":
                            foreach (string sRoleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(sRoleName))
                                {
                                    currentSettings.BrowserMode = BrowserType.CKFinder;

                                    break;
                                }

                                currentSettings.BrowserMode = BrowserType.None;
                            }

                            break;
                        case "standard":
                            foreach (string sRoleName in roles)
                            {
                                if (PortalSecurity.IsInRoles(sRoleName))
                                {
                                    currentSettings.BrowserMode = BrowserType.StandardBrowser;

                                    break;
                                }

                                currentSettings.BrowserMode = BrowserType.None;
                            }

                            break;
                        case "none":
                            currentSettings.BrowserMode = BrowserType.None;
                            break;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.INJECTJS))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.INJECTJS))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.InjectSyntaxJs = bResult;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.WIDTH))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.WIDTH))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Width = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.HEIGHT))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.HEIGHT))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Height = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BLANKTEXT))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BLANKTEXT))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.BlankText = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CSS))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CSS))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.ContentsCss = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.Templates_Files = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.CustomJsFile = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CONFIG))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.CONFIG))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.Config.CustomConfig = settingValue;
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.FileListPageSize = int.Parse(settingValue);
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.FileListViewMode = (FileListView)Enum.Parse(typeof(FileListView), settingValue);
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    currentSettings.DefaultLinkMode = (LinkMode)Enum.Parse(typeof(LinkMode), settingValue);
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.UseAnchorSelector = bResult;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.ShowPageLinksTabFirst = bResult;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.OverrideFileOnUpload = bResult;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SUBDIRS))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.SUBDIRS))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    bool bResult;
                    if (bool.TryParse(settingValue, out bResult))
                    {
                        currentSettings.SubDirs = bResult;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.BrowserRootDirId = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.BrowserRootDirId = -1;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.UploadDirId = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.UploadDirId = -1;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.ResizeWidth = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.ResizeWidth = -1;
                    }
                }
            }

            if (
                filteredSettings.Any(
                    setting => setting.Name.Equals(string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT))))
            {
                var settingValue =
                    filteredSettings.FirstOrDefault(
                        s => s.Name.Equals(string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT))).Value;

                if (!string.IsNullOrEmpty(settingValue))
                {
                    try
                    {
                        currentSettings.ResizeHeight = int.Parse(settingValue);
                    }
                    catch (Exception)
                    {
                        currentSettings.ResizeHeight = -1;
                    }
                }
            }

            return currentSettings;
        }

        /// <summary>
        /// Loads the module settings.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="currentSettings">The current settings.</param>
        /// <param name="key">The module key.</param>
        /// <param name="moduleId">The module id.</param>
        /// <param name="portalRoles">The portal roles.</param>
        /// <returns>
        /// Returns the filled Module Settings
        /// </returns>
        internal static EditorProviderSettings LoadModuleSettings(PortalSettings portalSettings, EditorProviderSettings currentSettings, string key, int moduleId, IList<RoleInfo> portalRoles)
        {
            Hashtable hshModSet = null;
            var module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, false);
            if (module != null)
            {
                hshModSet = module.ModuleSettings;
            }

            hshModSet = hshModSet ?? new Hashtable();

            var roles = new ArrayList();

            // Import all Editor config settings
            foreach (
                PropertyInfo info in
                    GetEditorConfigProperties()
                        .Where(
                            info =>
                            hshModSet != null && !string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, info.Name)])
                /*|| info.Name.Equals("CodeMirror") || info.Name.Equals("WordCount")*/))
            {
                switch (info.PropertyType.Name)
                {
                    case "String":
                        info.SetValue(currentSettings.Config, hshModSet[string.Format("{0}{1}", key, info.Name)], null);
                        break;
                    case "Int32":
                        info.SetValue(
                            currentSettings.Config,
                            int.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Decimal":
                        info.SetValue(
                            currentSettings.Config,
                            decimal.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "Boolean":
                        info.SetValue(
                            currentSettings.Config,
                            bool.Parse((string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                }

                switch (info.Name)
                {
                    case "ToolbarLocation":
                        info.SetValue(
                            currentSettings.Config,
                            (ToolBarLocation)
                            Enum.Parse(
                                typeof(ToolBarLocation), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "DefaultLinkType":
                        info.SetValue(
                            currentSettings.Config,
                            (LinkType)
                            Enum.Parse(typeof(LinkType), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "EnterMode":
                    case "ShiftEnterMode":
                        info.SetValue(
                            currentSettings.Config,
                            (EnterModus)
                            Enum.Parse(typeof(EnterModus), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "ContentsLangDirection":
                        info.SetValue(
                            currentSettings.Config,
                            (LanguageDirection)
                            Enum.Parse(
                                typeof(LanguageDirection), (string)hshModSet[string.Format("{0}{1}", key, info.Name)]),
                            null);
                        break;
                    case "CodeMirror":
                        foreach (var codeMirrorInfo in
                            typeof(CodeMirror).GetProperties()
                                              .Where(codeMirrorInfo => !codeMirrorInfo.Name.Equals("Theme")))
                        {
                            switch (codeMirrorInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            hshModSet[string.Format("{0}{1}", key, codeMirrorInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, codeMirrorInfo.Name)))
                                    {
                                        codeMirrorInfo.SetValue(
                                            currentSettings.Config.CodeMirror,
                                            bool.Parse(
                                                (string)hshModSet[string.Format("{0}{1}", key, codeMirrorInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                    case "WordCount":
                        foreach (var wordCountInfo in typeof(WordCountConfig).GetProperties())
                        {
                            switch (wordCountInfo.PropertyType.Name)
                            {
                                case "String":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            hshModSet[string.Format("{0}{1}", key, wordCountInfo.Name)],
                                            null);
                                    }

                                    break;
                                case "Boolean":
                                    if (hshModSet.ContainsKey(string.Format("{0}{1}", key, wordCountInfo.Name)))
                                    {
                                        wordCountInfo.SetValue(
                                            currentSettings.Config.WordCount,
                                            bool.Parse(
                                                (string)hshModSet[string.Format("{0}{1}", key, wordCountInfo.Name)]),
                                            null);
                                    }

                                    break;
                            }
                        }

                        break;
                }
            }

            /////////////////

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SKIN)]))
            {
                currentSettings.Config.Skin = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SKIN)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)]))
            {
                currentSettings.Config.CodeMirror.Theme = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CODEMIRRORTHEME)];
            }

            List<ToolbarRoles> listToolbarRoles = (from RoleInfo objRole in portalRoles
                                                   where
                                                       !string.IsNullOrEmpty(
                                                           (string)
                                                           hshModSet[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)])
                                                   let sToolbar =
                                                       (string)
                                                       hshModSet[string.Format("{0}{2}#{1}", key, objRole.RoleID, SettingConstants.TOOLB)]
                                                   select
                                                       new ToolbarRoles { RoleId = objRole.RoleID, Toolbar = sToolbar })
                .ToList();

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)]))
            {
                string sToolbar = (string)hshModSet[string.Format("{0}{2}#{1}", key, "-1", SettingConstants.TOOLB)];

                listToolbarRoles.Add(new ToolbarRoles { RoleId = -1, Toolbar = sToolbar });
            }

            currentSettings.ToolBarRoles = listToolbarRoles;

            var listUploadSizeRoles = (from RoleInfo objRole in portalRoles
                                       where
                                           !string.IsNullOrEmpty(
                                               (string)
                                               hshModSet[string.Format("{0}{1}#{2}", key, objRole.RoleID, SettingConstants.UPLOADFILELIMITS)])
                                       let uploadFileLimit =
                                           (string)
                                           hshModSet[string.Format("{0}{1}#{2}", key, objRole.RoleID, SettingConstants.UPLOADFILELIMITS)]
                                       select
                                           new UploadSizeRoles { RoleId = objRole.RoleID, RoleName = objRole.RoleName, UploadFileLimit = Convert.ToInt32(uploadFileLimit) })
                .ToList();

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}#{2}", key, "-1", SettingConstants.UPLOADFILELIMITS)]))
            {
                listUploadSizeRoles.Add(
                    new UploadSizeRoles
                    {
                        RoleId = -1,
                        UploadFileLimit =
                            Convert.ToInt32(
                                hshModSet[
                                    string.Format(
                                        "{0}{2}#{1}",
                                        key,
                                        "-1",
                                        SettingConstants.UPLOADFILELIMITS)])
                    });
            }

            currentSettings.UploadSizeRoles = listUploadSizeRoles;

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.ROLES)]))
            {
                string sRoles = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.ROLES)];

                currentSettings.BrowserRoles = sRoles;

                string[] rolesA = sRoles.Split(';');

                foreach (string sRoleName in rolesA)
                {
                    if (Utility.IsNumeric(sRoleName))
                    {
                        RoleInfo roleInfo = RoleController.Instance.GetRoleById(portalSettings.PortalId, int.Parse(sRoleName));

                        if (roleInfo != null)
                        {
                            roles.Add(roleInfo.RoleName);
                        }
                    }
                    else
                    {
                        roles.Add(sRoleName);
                    }
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSER)]))
            {
                currentSettings.Browser = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSER)];

                switch (currentSettings.Browser)
                {
                    case "ckfinder":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                currentSettings.BrowserMode = BrowserType.CKFinder;

                                break;
                            }

                            currentSettings.BrowserMode = BrowserType.None;
                        }

                        break;
                    case "standard":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                currentSettings.BrowserMode = BrowserType.StandardBrowser;

                                break;
                            }

                            currentSettings.BrowserMode = BrowserType.None;
                        }

                        break;
                    case "none":
                        currentSettings.BrowserMode = BrowserType.None;
                        break;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.INJECTJS)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.INJECTJS)], out bResult))
                {
                    currentSettings.InjectSyntaxJs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.WIDTH)]))
            {
                currentSettings.Config.Width = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.WIDTH)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.HEIGHT)]))
            {
                currentSettings.Config.Height = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.HEIGHT)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)]))
            {
                currentSettings.BlankText = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BLANKTEXT)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CSS)]))
            {
                currentSettings.Config.ContentsCss = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CSS)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)]))
            {
                currentSettings.Config.Templates_Files = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.TEMPLATEFILES)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)]))
            {
                currentSettings.CustomJsFile = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CUSTOMJSFILE)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CONFIG)]))
            {
                currentSettings.Config.CustomConfig = (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.CONFIG)];
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]))
            {
                currentSettings.FileListPageSize = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTPAGESIZE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]))
            {
                currentSettings.FileListViewMode =
                    (FileListView)
                    Enum.Parse(typeof(FileListView), (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.FILELISTVIEWMODE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]))
            {
                currentSettings.DefaultLinkMode =
                    (LinkMode)
                    Enum.Parse(typeof(LinkMode), (string)hshModSet[string.Format("{0}{1}", key, SettingConstants.DEFAULTLINKMODE)]);
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.USEANCHORSELECTOR)], out bResult))
                {
                    currentSettings.UseAnchorSelector = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SHOWPAGELINKSTABFIRST)], out bResult))
                {
                    currentSettings.ShowPageLinksTabFirst = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.OVERRIDEFILEONUPLOAD)], out bResult))
                {
                    currentSettings.OverrideFileOnUpload = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)]))
            {
                bool bResult;
                if (bool.TryParse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.SUBDIRS)], out bResult))
                {
                    currentSettings.SubDirs = bResult;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]))
            {
                try
                {
                    currentSettings.BrowserRootDirId = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.BROWSERROOTDIRID)]);
                }
                catch (Exception)
                {
                    currentSettings.BrowserRootDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]))
            {
                try
                {
                    currentSettings.UploadDirId = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.UPLOADDIRID)]);
                }
                catch (Exception)
                {
                    currentSettings.UploadDirId = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]))
            {
                try
                {
                    currentSettings.ResizeWidth = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEWIDTH)]);
                }
                catch (Exception)
                {
                    currentSettings.ResizeWidth = -1;
                }
            }

            if (!string.IsNullOrEmpty((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]))
            {
                try
                {
                    currentSettings.ResizeHeight = int.Parse((string)hshModSet[string.Format("{0}{1}", key, SettingConstants.RESIZEHEIGHT)]);
                }
                catch (Exception)
                {
                    currentSettings.ResizeHeight = -1;
                }
            }

            return currentSettings;
        }

        /// <summary>
        /// Gets the default settings.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="homeDirPath">The home folder path.</param>
        /// <param name="alternateSubFolder">The alternate Sub Folder.</param>
        /// <param name="portalRoles">The portal roles.</param>
        /// <returns>
        /// Returns the Default Provider Settings
        /// </returns>
        internal static EditorProviderSettings GetDefaultSettings(PortalSettings portalSettings, string homeDirPath, string alternateSubFolder, IList<RoleInfo> portalRoles)
        {
            var roles = new ArrayList();

            if (!string.IsNullOrEmpty(alternateSubFolder))
            {
                var alternatePath = Path.Combine(homeDirPath, alternateSubFolder);

                if (!Directory.Exists(alternatePath))
                {
                    Directory.CreateDirectory(alternatePath);
                }

                homeDirPath = alternatePath;
            }

            // Check if old Settings File Exists
            if (File.Exists(Path.Combine(homeDirPath, SettingConstants.XmlDefaultFileName)))
            {
                // Import Old SettingsBase Xml File
                ImportSettingBaseXml(homeDirPath);
            }

            if (!File.Exists(Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName)))
            {
                if (!File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName)))
                {
                    CreateDefaultSettingsFile();
                }
                else
                {
                    // Import Old SettingBase Xml File
                    ImportSettingBaseXml(Globals.HostMapPath, true);
                }

                File.Copy(Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName), Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName));
            }

            var serializer = new XmlSerializer(typeof(EditorProviderSettings));
            var reader =
                new StreamReader(
                    new FileStream(
                        Path.Combine(homeDirPath, SettingConstants.XmlSettingsFileName), FileMode.Open, FileAccess.Read, FileShare.Read));

            var settings = (EditorProviderSettings)serializer.Deserialize(reader);

            if (!string.IsNullOrEmpty(settings.EditorWidth))
            {
                settings.Config.Width = settings.EditorWidth;
            }

            if (!string.IsNullOrEmpty(settings.EditorHeight))
            {
                settings.Config.Height = settings.EditorHeight;
            }

            // Get Browser Roles
            if (!string.IsNullOrEmpty(settings.BrowserRoles))
            {
                var rolesString = settings.BrowserRoles;

                if (rolesString.Length >= 1 && rolesString.Contains(";"))
                {
                    string[] rolesA = rolesString.Split(';');

                    foreach (string sRoleName in rolesA)
                    {
                        if (Utility.IsNumeric(sRoleName))
                        {
                            RoleInfo roleInfo = RoleController.Instance.GetRoleById(portalSettings?.PortalId ?? Host.HostPortalID, int.Parse(sRoleName));

                            if (roleInfo != null)
                            {
                                roles.Add(roleInfo.RoleName);
                            }
                        }
                        else
                        {
                            roles.Add(sRoleName);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(settings.Browser))
            {
                switch (settings.Browser)
                {
                    case "ckfinder":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                settings.BrowserMode = BrowserType.CKFinder;

                                break;
                            }

                            settings.BrowserMode = BrowserType.None;
                        }

                        break;
                    case "standard":
                        foreach (string sRoleName in roles)
                        {
                            if (PortalSecurity.IsInRoles(sRoleName))
                            {
                                settings.BrowserMode = BrowserType.StandardBrowser;

                                break;
                            }

                            settings.BrowserMode = BrowserType.None;
                        }

                        break;
                    case "none":
                        settings.BrowserMode = BrowserType.None;
                        break;
                }
            }

            reader.Close();

            return settings;
        }

        /// <summary>
        /// Creates the default settings file.
        /// </summary>
        internal static void CreateDefaultSettingsFile()
        {
            var newSettings = new EditorProviderSettings();

            var serializer = new XmlSerializer(typeof(EditorProviderSettings));

            var textWriter =
                new StreamWriter(
                    new FileStream(
                        Path.Combine(Globals.HostMapPath, SettingConstants.XmlDefaultFileName),
                        FileMode.OpenOrCreate,
                        FileAccess.ReadWrite,
                        FileShare.ReadWrite));

            serializer.Serialize(textWriter, newSettings);

            textWriter.Close();
        }

        /// <summary>
        /// Gets the editor config properties.
        /// </summary>
        /// <returns>Returns the EditorConfig Properties</returns>
        internal static IEnumerable<PropertyInfo> GetEditorConfigProperties()
        {
            return
                typeof(EditorConfig).GetProperties()
                    .Where(
                        info =>
                        !info.Name.Equals("Magicline_KeystrokeNext") && !info.Name.Equals("Magicline_KeystrokePrevious")
                        && !info.Name.Equals("Plugins") && !info.Name.Equals("Codemirror_Theme")
                        && !info.Name.Equals("Width") && !info.Name.Equals("Height") && !info.Name.Equals("ContentsCss")
                        && !info.Name.Equals("Templates_Files") && !info.Name.Equals("CustomConfig")
                        && !info.Name.Equals("Skin") && !info.Name.Equals("Templates_Files")
                        && !info.Name.Equals("Toolbar") && !info.Name.Equals("Language")
                        && !info.Name.Equals("FileBrowserWindowWidth") && !info.Name.Equals("FileBrowserWindowHeight")
                        && !info.Name.Equals("FileBrowserWindowWidth") && !info.Name.Equals("FileBrowserWindowHeight")
                        && !info.Name.Equals("FileBrowserUploadUrl") && !info.Name.Equals("FileBrowserImageUploadUrl")
                        && !info.Name.Equals("FilebrowserImageBrowseLinkUrl")
                        && !info.Name.Equals("FileBrowserImageBrowseUrl")
                        && !info.Name.Equals("FileBrowserFlashUploadUrl")
                        && !info.Name.Equals("FileBrowserFlashBrowseUrl") && !info.Name.Equals("FileBrowserBrowseUrl"));
        }

        /// <summary>
        /// Imports the old SettingsBase Xml File
        /// </summary>
        /// <param name="homeDirPath">The home folder path.</param>
        /// <param name="isDefaultXmlFile">if set to <c>true</c> [is default XML file].</param>
        internal static void ImportSettingBaseXml(string homeDirPath, bool isDefaultXmlFile = false)
        {
            var oldXmlPath = Path.Combine(homeDirPath, SettingConstants.XmlDefaultFileName);
            var oldSerializer = new XmlSerializer(typeof(SettingBase));
            var reader = new XmlTextReader(new FileStream(oldXmlPath, FileMode.Open, FileAccess.Read, FileShare.Read));

            if (!oldSerializer.CanDeserialize(reader))
            {
                reader.Close();

                return;
            }

            var oldDefaultSettings = (SettingBase)oldSerializer.Deserialize(reader);

            reader.Close();

            // Fix for old skins
            if (oldDefaultSettings.sSkin.Equals("office2003")
                            || oldDefaultSettings.sSkin.Equals("BootstrapCK-Skin")
                            || oldDefaultSettings.sSkin.Equals("chris")
                            || oldDefaultSettings.sSkin.Equals("v2"))
            {
                oldDefaultSettings.sSkin = "moono";
            }

            // Migrate Settings
            var importedSettings = new EditorProviderSettings
            {
                FileListPageSize = oldDefaultSettings.FileListPageSize,
                FileListViewMode = oldDefaultSettings.FileListViewMode,
                UseAnchorSelector = oldDefaultSettings.UseAnchorSelector,
                ShowPageLinksTabFirst = oldDefaultSettings.ShowPageLinksTabFirst,
                SubDirs = oldDefaultSettings.bSubDirs,
                InjectSyntaxJs = oldDefaultSettings.injectSyntaxJs,
                BrowserRootDirId = oldDefaultSettings.BrowserRootDirId,
                UploadDirId = oldDefaultSettings.UploadDirId,
                ResizeHeight = oldDefaultSettings.iResizeHeight,
                ResizeWidth = oldDefaultSettings.iResizeWidth,
                ToolBarRoles = oldDefaultSettings.listToolbRoles,
                BlankText = oldDefaultSettings.sBlankText,
                BrowserRoles = oldDefaultSettings.sBrowserRoles,
                Browser = oldDefaultSettings.sBrowser,
                Config =
                {
                    CustomConfig = oldDefaultSettings.sConfig,
                    ContentsCss = oldDefaultSettings.sCss,
                    Skin = oldDefaultSettings.sSkin,
                    Templates_Files = oldDefaultSettings.sTemplates,
                    Height = oldDefaultSettings.BrowserHeight,
                    Width = oldDefaultSettings.BrowserWidth,
                    AutoParagraph = true,
                    AutoUpdateElement = true,
                    BasicEntities = true,
                    BrowserContextMenuOnCtrl = true,
                    ColorButton_EnableMore = true,
                    DisableNativeSpellChecker = true,
                    DisableNativeTableHandles = true,
                    EnableTabKeyTools = true,
                    Entities = true,
                    Entities_Greek = true,
                    Entities_Latin = true,
                    FillEmptyBlocks = true,
                    IgnoreEmptyParagraph = true,
                    Image_RemoveLinkByEmptyURL = true,
                    PasteFromWordRemoveFontStyles = true,
                    PasteFromWordRemoveStyles = true,
                    Resize_Enabled = true,
                    StartupShowBorders = true,
                    ToolbarGroupCycling = true,
                    ToolbarStartupExpanded = true,
                    UseComputedState = true,
                    AutoGrow_BottomSpace = 0,
                    AutoGrow_MaxHeight = 0,
                    AutoGrow_MinHeight = 200,
                    BaseFloatZIndex = 10000,
                    Dialog_MagnetDistance = 20,
                    IndentOffset = 40,
                    Menu_SubMenuDelay = 400,
                    Resize_MaxHeight = 600,
                    Resize_MaxWidth = 3000,
                    Resize_MinHeight = 250,
                    Resize_MinWidth = 750,
                    Scayt_MaxSuggestions = 5,
                    Smiley_columns = 8,
                    SourceAreaTabSize = 20,
                    TabIndex = 0,
                    TabSpaces = 0,
                    UndoStackSize = 20
                }
            };

            // Delete Old File
            File.Delete(oldXmlPath);

            // Save new xml file
            var newSerializer = new XmlSerializer(typeof(EditorProviderSettings));

            using (
                var textWriter =
                    new StreamWriter(
                        new FileStream(
                            Path.Combine(
                                homeDirPath, isDefaultXmlFile ? SettingConstants.XmlDefaultFileName : SettingConstants.XmlSettingsFileName),
                            FileMode.OpenOrCreate,
                            FileAccess.ReadWrite,
                            FileShare.ReadWrite)))
            {
                newSerializer.Serialize(textWriter, importedSettings);

                textWriter.Close();
            }
        }

        /// <summary>
        /// Gets the size of the current user upload.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="httpRequest">The HTTP request.</param>
        /// <returns>Returns the MAX. upload file size for the current user</returns>
        internal static int GetCurrentUserUploadSize(EditorProviderSettings settings, PortalSettings portalSettings, HttpRequest httpRequest)
        {
            var uploadFileLimitForPortal = Convert.ToInt32(Utility.GetMaxUploadSize());

            if (settings.ToolBarRoles.Count <= 0)
            {
                return uploadFileLimitForPortal;
            }

            var listUserUploadFileSizes = new List<ToolbarSet>();

            foreach (var roleUploadSize in settings.UploadSizeRoles)
            {
                if (roleUploadSize.RoleId.Equals(-1) && !httpRequest.IsAuthenticated)
                {
                    return roleUploadSize.UploadFileLimit;
                }

                if (roleUploadSize.RoleId.Equals(-1))
                {
                    continue;
                }

                // Role
                var role = RoleController.Instance.GetRoleById(portalSettings.PortalId, roleUploadSize.RoleId);

                if (role == null)
                {
                    continue;
                }

                if (!PortalSecurity.IsInRole(role.RoleName))
                {
                    continue;
                }

                var toolbar = new ToolbarSet(role.RoleName, roleUploadSize.UploadFileLimit);

                listUserUploadFileSizes.Add(toolbar);
            }

            if (listUserUploadFileSizes.Count <= 0)
            {
                return uploadFileLimitForPortal;
            }

            // Compare The User Toolbars if the User is more then One Role, and apply the Toolbar with the Highest Priority
            int iHighestPrio = listUserUploadFileSizes.Max(toolb => toolb.Priority);

            return listUserUploadFileSizes.Find(toolbarSel => toolbarSel.Priority.Equals(iHighestPrio)).Priority;
        }

        #endregion
    }
}