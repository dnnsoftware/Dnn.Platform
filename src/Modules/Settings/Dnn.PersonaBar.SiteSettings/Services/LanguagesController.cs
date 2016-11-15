using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.UI;
using System.Xml;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.DTO.Tabs;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.SiteSettings.Components.Constants;
using Dnn.PersonaBar.SiteSettings.Components;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SiteSettings.Services
{
    [ServiceScope(Scope = ServiceScope.Admin)]
    public class LanguagesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesController));
        private const string LocalResourcesFile = "~/admin/Dnn.PersonaBar/App_LocalResources/SiteSettings.resx";

        // Sample matches:
        // MyResources.ascx.en-US.resx
        // MyResources.ascx.en-US.Host.resx
        // MyResources.ascx.en-US.Portal-123.resx
        internal static readonly Regex FileInfoRegex = new Regex(
            @"\.([A-Z]{2}\-[A-Z]{2})((\.Host)?|(\.Portal-\d+)?)\.resx$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        private string _selectedResourceFile;

        #region -------------------------------- PUBLIC API METHODS SEPARATOR --------------------------------
        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [P] to expand source code folding
        #endregion

        // GET /api/personabar/languages/GetRootResourcesFolders?mode=Site
        [HttpGet]
        public HttpResponseMessage GetRootResourcesFolders(string mode)
        {
            try
            {
                LanguageResourceMode resourceMode;
                Enum.TryParse(mode ?? "", false, out resourceMode);

                var folders = new List<KeyValuePair<string, string>>();
                var files = new List<KeyValuePair<string, string>>();
                var server = HttpContext.Current.Server;

                switch (resourceMode)
                {
                    case LanguageResourceMode.Portal:
                        {
                            folders.AddRange(new[]
                            {
                            "Admin",
                            "Controls",
                            "DesktopModules",
                            "Install",
                            "Providers"
                        }.Select(s => new KeyValuePair<string, string>(s, server.MapPath("~/" + s))));

                            const string skins = "Skins";
                            var skinsPath = Path.Combine(Globals.ApplicationMapPath, skins);

                            if (Directory.Exists(skinsPath) && HasLocalResources(skinsPath))
                            {
                                folders.Add(new KeyValuePair<string, string>(LocalizeString("HostSkins"), skinsPath));
                            }

                            var portalSkinFolder = Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, skins);
                            if (Directory.Exists(portalSkinFolder) && PortalSettings.ActiveTab.ParentId == PortalSettings.AdminTabId)
                            {
                                folders.Add(new KeyValuePair<string, string>(
                                    LocalizeString("PortalSkins"), Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, skins)));
                            }
                            break;
                        }
                    case LanguageResourceMode.Host:
                        folders.Add(new KeyValuePair<string, string>(
                            LocalizeString("GlobalResources"), server.MapPath("~/App_GlobalResources")));
                        files.AddRange(GetResxFiles(server.MapPath("~/App_GlobalResources")));
                        break;
                    case LanguageResourceMode.System:
                        folders.Add(new KeyValuePair<string, string>(
                            LocalizeString("SiteTemplates"), server.MapPath("~/Portals/_default")));
                        files.AddRange(GetResxFiles(server.MapPath("~/Portals/_default")));
                        break;
                    default:
                        // old system uses "Convert.ToString(Personalization.GetProfile("LanguageEditor", "Mode" + PortalId)));"
                        // value for this but it is not maintained in PersonaBar pages
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "MissingParams");
                }

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Folders = folders.MapEntries(),
                        Files = files.MapEntries(),
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/personabar/languages/GetSubRootResources?currentFolder=DesktopModules
        [HttpGet]
        public HttpResponseMessage GetSubRootResources(string currentFolder = null)
        {
            try
            {
                var folders = new List<KeyValuePair<string, string>>();
                var files = new List<KeyValuePair<string, string>>();
                var server = HttpContext.Current.Server;

                if (string.IsNullOrEmpty(currentFolder))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "MissingResourcesFolder");
                }

                folders.AddRange(GetResxDirectories(server.MapPath("~/" + currentFolder)));
                files.AddRange(GetResxFiles(server.MapPath("~/" + currentFolder)));

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Folders = folders.MapEntries(),
                        Files = files.MapEntries()
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/personabar/languages/GetResxEntries?mode=Portal&locale=de-DE&resourceFile=App_GlobalResources%2fFileUpload.resx
        [HttpGet]
        public HttpResponseMessage GetResxEntries(string mode, string locale, string resourceFile, bool highlight = false)
        {
            try
            {
                LanguageResourceMode resourceMode;
                Enum.TryParse(mode, false, out resourceMode);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        {
                            //this old behaviour is not maintained in the PersonaBar pages

                            //var dbMode = Convert.ToString(Personalization.GetProfile("LanguageEditor", "Mode" + PortalId));
                            //if (dbMode != resourceMode.ToString())
                            //    Personalization.SetProfile("LanguageEditor", "Mode" + PortalId, resourceMode.ToString());
                            //
                            //var dbHighlight = Convert.ToString(Personalization.GetProfile("LanguageEditor", "HighLight" + PortalId));
                            //if (dbHighlight != highlight.ToString())
                            //    Personalization.SetProfile("LanguageEditor", "HighLight" + PortalId, highlight.ToString());
                            break;
                        }
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

                var language = LocaleController.Instance.GetLocale(locale);
                if (language == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(LocalizeString("InvalidLocale.ErrorMessage"), locale));
                }

                _selectedResourceFile = !string.IsNullOrEmpty(resourceFile)
                    ? HttpContext.Current.Server.MapPath("~/" + resourceFile)
                    : HttpContext.Current.Server.MapPath(Localization.GlobalResourceFile);

                var editTable = LoadFile(resourceMode, "Edit", locale);
                var defaultTable = LoadFile(resourceMode, "Default", locale);

                var fullPath = Path.GetFileName(ResourceFile(locale, resourceMode).Replace(Globals.ApplicationMapPath, ""));
                var folder = ResourceFile(locale, resourceMode).Replace(Globals.ApplicationMapPath, "").Replace("\\" + resourceFile, "");

                // check edit table and if empty, just use default
                if (editTable.Count == 0)
                {
                    editTable = defaultTable;
                }
                else
                {
                    //remove obsolete keys
                    var toBeDeleted = new ArrayList();
                    foreach (string key in editTable.Keys)
                    {
                        if (!defaultTable.Contains(key))
                        {
                            toBeDeleted.Add(key);
                        }
                    }
                    if (toBeDeleted.Count > 0)
                    {
                        Logger.Warn(LocalizeString("Obsolete"));
                        foreach (string key in toBeDeleted)
                        {
                            editTable.Remove(key);
                        }
                    }

                    //add missing keys
                    foreach (string key in defaultTable.Keys)
                    {
                        if (!editTable.Contains(key))
                        {
                            editTable.Add(key, defaultTable[key]);
                        }
                        else
                        {
                            // Update default value
                            var p = (Pair)editTable[key];
                            p.Second = ((Pair)defaultTable[key]).First;
                            editTable[key] = p;
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    File = fullPath,
                    Folder = folder,
                    LanguageCode = language.Code,
                    Translations = new SortedList(editTable),
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/personabar/languages/SaveResxEntries
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveResxEntries(UpdateTransaltionsRequest request)
        {
            try
            {
                LanguageResourceMode resourceMode;
                Enum.TryParse(request.Mode, false, out resourceMode);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        break;
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

                var language = LocaleController.Instance.GetLocale(request.Locale);
                if (language == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(LocalizeString("InvalidLocale.ErrorMessage"), request.Locale));
                }

                if (string.IsNullOrEmpty(request.ResourceFile))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(LocalizeString("MissingResourceFileName"), request.Locale));
                }

                _selectedResourceFile = HttpContext.Current.Server.MapPath("~/" + request.ResourceFile);
                var message = SaveResourceFileFile(resourceMode, request.Locale, request.Entries);
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = message });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/personabar/languages/EnableLocalizedContent?translatePages=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage EnableLocalizedContent([FromUri] bool translatePages)
        {
            try
            {
                var progress = new LocalizationProgress { InProgress = true, };
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                LanguagesControllerTasks.LocalizeSitePages(
                    progress, PortalId, translatePages, portalSettings.DefaultLanguage ?? Localization.SystemLocale);
                return Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/personabar/languages/GetLocalizationProgress
        [HttpGet]
        public HttpResponseMessage GetLocalizationProgress()
        {
            try
            {
                var progress = LanguagesControllerTasks.ReadProgressFile();
                return Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/personabar/languages/DisableLocalizedContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DisableLocalizedContent()
        {
            try
            {
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
                var portalDefault = portalSettings.DefaultLanguage;
                foreach (var locale in LocaleController.Instance.GetLocales(PortalId).Values)
                {
                    if (locale.Code != portalDefault)
                    {
                        LocaleController.Instance.PublishLanguage(PortalId, locale.Code, false);
                        TabController.Instance.DeleteTranslatedTabs(PortalId, locale.Code, false);
                        PortalController.Instance.RemovePortalLocalization(PortalId, locale.Code, false);
                    }
                }

                TabController.Instance.EnsureNeutralLanguage(PortalId, portalDefault, false);
                PortalController.UpdatePortalSetting(PortalId, "ContentLocalizationEnabled", "False");
                DataCache.ClearPortalCache(PortalId, true);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/personabar/languages/PublishPages?languageId=5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage PublishPage(int languageId)
        {
            try
            {
                var locale = new LocaleController().GetLocale(languageId);
                if (locale == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidLanguageID");
                }

                LocaleController.Instance.PublishLanguage(PortalId, locale.Code, true);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/personabar/languages/GetTabsForTranslation?cultureCode=fr-FR
        [HttpGet]
        public HttpResponseMessage GetTabsForTranslation(string cultureCode)
        {
            try
            {
                var tabsController = new TabsController();
                var nonTranslatedTabs = tabsController.GetTabsForTranslation(cultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, nonTranslatedTabs);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // GET /api/personabar/languages/GetTabLocalization?tabId=123
        /// <summary>
        /// Gets the view data that used to be in the old ControlBar's localization tab
        /// under Page Settings ( /{page}/ctl/Tab/action/edit/activeTab/settingTab ).
        /// </summary>
        /// <param name="tabId">The ID of the tab to get localization for.</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetTabLocalization(int tabId)
        {
            try
            {
                var pages = GetNonLocalizedPages(tabId);
                return Request.CreateResponse(HttpStatusCode.OK, pages);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        // POST /api/personabar/languages/UpdateTabLocalization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage UpdateTabLocalization(DnnPagesDto pages)
        {
            try
            {
                SaveNonLocalizedPages(pages);
                return Request.CreateResponse(HttpStatusCode.OK, pages);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #region -------------------------------- PRIVATE METHODS SEPARATOR --------------------------------
        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [P] to expand source code folding
        #endregion

        private static IEnumerable<KeyValuePair<string, string>> GetResxDirectories(string path)
        {
            if (!Directory.Exists(path))
            {
                return new List<KeyValuePair<string, string>>();
            }

            return Directory.GetDirectories(path)
                .Select(folder => new DirectoryInfo(folder))
                .Where(folderInfo => HasLocalResources(folderInfo.FullName))
                .Select(folderInfo => new KeyValuePair<string, string>(folderInfo.Name, folderInfo.FullName));
        }

        private static IEnumerable<KeyValuePair<string, string>> GetResxFiles(string path)
        {
            var sysLocale = Localization.SystemLocale.ToLowerInvariant();
            return
                from file in Directory.GetFiles(path, "*.resx")
                select new FileInfo(file) into fileInfo
                let match = FileInfoRegex.Match(fileInfo.Name)
                where !match.Success || match.Groups[1].Value.ToLowerInvariant() == sysLocale
                select new KeyValuePair<string, string>(Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.FullName);
        }

        private static bool HasLocalResources(string path)
        {
            var folderInfo = new DirectoryInfo(path);

            if (path.ToLowerInvariant().EndsWith(Localization.LocalResourceDirectory))
            {
                return true;
            }

            if (!Directory.Exists(path))
            {
                return false;
            }

            var hasResources = false;
            foreach (var folder in Directory.GetDirectories(path))
            {
                if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                {
                    folderInfo = new DirectoryInfo(folder);
                    hasResources = hasResources || HasLocalResources(folderInfo.FullName);
                }
            }
            return hasResources || folderInfo.GetFiles("*.resx").Length > 0;

        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourcesFile);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads resources from file
        /// </summary>
        /// <param name = "mode">Active editor mode</param>
        /// <param name = "type">Resource being loaded (edit or default)</param>
        /// <param name="locale">The locale of the file being edited</param>
        /// <returns></returns>
        /// <remarks>
        ///   Depending on the editor mode, resources will be overrided using default DNN schema.
        ///   "Edit" resources will only load selected file.
        ///   When loading "Default" resources (to be used on the editor as helpers) fallback resource
        ///   chain will be used in order for the editor to be able to correctly see what
        ///   is the current default value for the any key. This process depends on the current active
        ///   editor mode:
        ///   - System: when editing system base resources on en-US needs to be loaded
        ///   - Host: base en-US, and base locale especific resource
        ///   - Portal: base en-US, host override for en-US, base locale especific resource, and host override
        ///   for locale
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private Hashtable LoadFile(LanguageResourceMode mode, string type, string locale)
        {
            string file;
            var ht = new Hashtable();

            if (type == "Edit")
            {
                // Only load resources from the file being edited
                file = ResourceFile(locale, mode);
                LoadResource(ht, file);
            }
            else if (type == "Default")
            {
                // Load system default
                file = ResourceFile(Localization.SystemLocale, LanguageResourceMode.System);
                LoadResource(ht, file);

                if (mode == LanguageResourceMode.Host)
                {
                    // Load base file for selected locale
                    file = ResourceFile(locale, LanguageResourceMode.System);
                    LoadResource(ht, file);
                }
                else if (mode == LanguageResourceMode.Portal)
                {
                    //Load host override for default locale
                    file = ResourceFile(Localization.SystemLocale, LanguageResourceMode.Host);
                    LoadResource(ht, file);

                    if (locale != Localization.SystemLocale)
                    {
                        // Load base file for locale
                        file = ResourceFile(locale, LanguageResourceMode.System);
                        LoadResource(ht, file);

                        //Load host override for selected locale
                        file = ResourceFile(locale, LanguageResourceMode.Host);
                        LoadResource(ht, file);
                    }
                }
            }

            return ht;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///  Loads resources from file into the HastTable
        /// </summary>
        /// <param name = "ht">Current resources HashTable</param>
        /// <param name = "filepath">Resources file</param>
        /// <returns>Base table updated with new resources </returns>
        /// <remarks>
        ///   Returned hashtable uses resourcekey as key.
        ///   Value contains a Pair object where:
        ///   First=>value to be edited
        ///   Second=>default value
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private static void LoadResource(IDictionary ht, string filepath)
        {
            var d = new XmlDocument();
            bool xmlLoaded;
            try
            {
                d.Load(filepath);
                xmlLoaded = true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                var nLoopVariables = d.SelectNodes("root/data");
                if (nLoopVariables != null)
                    foreach (XmlNode nLoopVariable in nLoopVariables)
                    {
                        var n = nLoopVariable;
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            var selectSingleNode = n.SelectSingleNode("value");
                            if (selectSingleNode != null)
                            {
                                var val = selectSingleNode.InnerXml;
                                if (n.Attributes != null)
                                {
                                    if (ht[n.Attributes["name"].Value] == null)
                                    {
                                        ht.Add(n.Attributes["name"].Value, new Pair(val, val));
                                    }
                                    else
                                    {
                                        ht[n.Attributes["name"].Value] = new Pair(val, val);
                                    }
                                }
                            }
                        }
                    }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the resource file name for a given resource and language
        /// </summary>
        /// <param name="language">Language Name.</param>
        /// <param name = "mode">Identifies the resource being searched (System, Host, Portal)</param>
        /// <returns>Localized File Name</returns>
        /// -----------------------------------------------------------------------------
        private string ResourceFile(string language, LanguageResourceMode mode)
        {
            return Localization.GetResourceFileName(_selectedResourceFile, language, mode.ToString(), PortalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Updates all values from the datagrid
        /// </summary>
        /// -----------------------------------------------------------------------------
        private string SaveResourceFileFile(LanguageResourceMode mode, string locale, IEnumerable<LocalizationEntry> entries)
        {
            var resDoc = new XmlDocument();
            var defDoc = new XmlDocument();

            var filename = ResourceFile(locale, mode);
            resDoc.Load(File.Exists(filename)
                ? filename :
                ResourceFile(Localization.SystemLocale, LanguageResourceMode.System));

            defDoc.Load(ResourceFile(Localization.SystemLocale, LanguageResourceMode.System));

            //store all changed resources
            var changedResources = new Dictionary<string, string>();

            // only items different from default will be saved
            foreach (var entry in entries)
            {
                var resourceKey = entry.Name;
                var txtValue = entry.NewValue;

                var node = resDoc.SelectSingleNode(GetResourceKeyXPath(resourceKey) + "/value");
                switch (mode)
                {
                    case LanguageResourceMode.System:
                        // this will save all items
                        if (node == null)
                        {
                            node = AddResourceKey(resDoc, resourceKey);
                        }
                        node.InnerXml = HttpUtility.HtmlEncode(txtValue);
                        if (txtValue != entry.DefaultValue)
                            changedResources.Add(resourceKey, txtValue);
                        break;
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        // only items different from default will be saved
                        if (txtValue != entry.DefaultValue)
                        {
                            if (node == null)
                            {
                                node = AddResourceKey(resDoc, resourceKey);
                            }
                            node.InnerXml = HttpUtility.HtmlEncode(txtValue);
                            changedResources.Add(resourceKey, txtValue);
                        }
                        else
                        {
                            // remove item = default
                            var parent = node?.ParentNode;
                            if (parent != null)
                            {
                                resDoc.SelectSingleNode("//root")?.RemoveChild(parent);
                            }
                        }
                        break;
                }
            }

            // remove obsolete keys
            var nodeLoopVariables = resDoc.SelectNodes("//root/data");
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null &&
                        defDoc.SelectSingleNode(GetResourceKeyXPath(node.Attributes["name"].Value)) == null)
                    {
                        node.ParentNode?.RemoveChild(node);
                    }
                }
            }

            // remove duplicate keys
            nodeLoopVariables = resDoc.SelectNodes("//root/data");
            if (nodeLoopVariables != null)
            {
                foreach (XmlNode node in nodeLoopVariables)
                {
                    if (node.Attributes != null)
                    {
                        var xmlNodeList = resDoc.SelectNodes(GetResourceKeyXPath(node.Attributes["name"].Value));
                        if (xmlNodeList != null && xmlNodeList.Count > 1)
                        {
                            node.ParentNode?.RemoveChild(node);
                        }
                    }
                }
            }

            switch (mode)
            {
                case LanguageResourceMode.System:
                    resDoc.Save(filename);
                    break;
                case LanguageResourceMode.Host:
                case LanguageResourceMode.Portal:
                    var xmlNodeList = resDoc.SelectNodes("//root/data");
                    if (xmlNodeList != null && xmlNodeList.Count > 0)
                    {
                        // there's something to save
                        resDoc.Save(filename);
                    }
                    else if (File.Exists(filename))
                    {
                        // nothing to be saved, if file exists delete
                        File.Delete(filename);
                    }
                    break;
            }

            if (changedResources.Count > 0)
            {
                var values = string.Join("; ", changedResources.Select(x => x.Key + "=" + x.Value));
                var log = new LogInfo { LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString() };
                log.LogProperties.Add(new LogDetailInfo(LocalizeString("ResourceUpdated"), ResourceFile(locale, mode)));
                log.LogProperties.Add(new LogDetailInfo("Updated Values", values));
                LogController.Instance.AddLog(log);
            }

            return string.Format(LocalizeString("Updated"), ResourceFile(locale, mode));
        }

        private static string GetResourceKeyXPath(string resourceKeyName)
        {
            return "//root/data[@name=" + XmlUtils.XPathLiteral(resourceKeyName) + "]";
        }

        private static XmlNode AddResourceKey(XmlDocument resourceDoc, string resourceKey)
        {
            // missing entry
            XmlNode nodeData = resourceDoc.CreateElement("data");
            var attr = resourceDoc.CreateAttribute("name");
            attr.Value = resourceKey;
            nodeData.Attributes?.Append(attr);
            var selectSingleNode = resourceDoc.SelectSingleNode("//root");
            selectSingleNode?.AppendChild(nodeData);
            return nodeData.AppendChild(resourceDoc.CreateElement("value"));
        }

        private DnnPagesDto GetNonLocalizedPages(int tabId)
        {
            var currentTab = TabController.Instance.GetTab(tabId, PortalId, false);

            //Unique id of default language page
            var uniqueId = currentTab.DefaultLanguageGuid != Null.NullGuid
                ? currentTab.DefaultLanguageGuid
                : currentTab.UniqueId;

            // get all non admin pages and not deleted
            var allPages = TabController.Instance.GetTabsByPortal(PortalId).Values.Where(
                t => t.TabID != PortalSettings.AdminTabId && (Null.IsNull(t.ParentId) || t.ParentId != PortalSettings.AdminTabId));
            allPages = allPages.Where(t => t.IsDeleted == false);
            // get all localized pages of current page
            var tabInfos = allPages as IList<TabInfo> ?? allPages.ToList();
            var localizedPages = tabInfos.Where(
                t => t.DefaultLanguageGuid == uniqueId || t.UniqueId == uniqueId).OrderBy(t => t.DefaultLanguageGuid).ToList();
            Dictionary<string, TabInfo> localizedTabs = null;

            // we are going to build up a list of locales
            // this is a bit more involved, since we want the default language to be first.
            // also, we do not want to add any locales the user has no access to
            var locales = new List<LocaleInfoDto>();
            var localeController = new LocaleController();
            var localeDict = localeController.GetLocales(PortalId);
            if (localeDict.Count == 0)
            {
                locales.Add(new LocaleInfoDto(""));
            }
            else
            {
                if (localizedPages.Count == 1 && localizedPages.First().CultureCode == "")
                {
                    // locale neutral page
                    locales.Add(new LocaleInfoDto(""));
                }
                else if (localizedPages.Count == 1 && localizedPages.First().CultureCode != PortalSettings.DefaultLanguage)
                {
                    var first = localizedPages.First();
                    locales.Add(new LocaleInfoDto(first.CultureCode));
                    //localizedTabs = new Dictionary<string, TabInfo> { { first.CultureCode, first } };
                }
                else
                {
                    //force sort order, so first add default language
                    locales.Add(new LocaleInfoDto(PortalSettings.DefaultLanguage));

                    // build up a list of localized tabs.
                    // depending on whether or not the selected page is in the default langauge
                    // we will add the localized tabs from the current page
                    // or from the defaultlanguage page
                    if (currentTab.CultureCode == PortalSettings.DefaultLanguage)
                    {
                        localizedTabs = currentTab.LocalizedTabs;
                    }
                    else
                    {
                        // selected page is not in default language
                        // add localizedtabs from defaultlanguage page
                        if (currentTab.DefaultLanguageTab != null)
                        {
                            localizedTabs = currentTab.DefaultLanguageTab.LocalizedTabs;
                        }
                    }

                    if (localizedTabs != null)
                    {
                        // only add locales from tabs the user has at least view permissions to.
                        // we will handle the edit permissions at a later stage
                        locales.AddRange(
                            from localizedTab in localizedTabs
                            where TabPermissionController.CanViewPage(localizedTab.Value)
                            select new LocaleInfoDto(localizedTab.Value.CultureCode));
                    }
                }
            }

            var dnnPages = new DnnPagesDto(locales);

            // filter the list of localized pages to only those that have a culture we want to see
            var viewableLocalizedPages = localizedPages.Where(
                localizedPage => locales.Find(locale => locale.CultureCode == localizedPage.CultureCode) != null).ToList();

            foreach (var tabInfo in viewableLocalizedPages)
            {
                var localTabInfo = tabInfo;
                var dnnPage = dnnPages.Page(localTabInfo.CultureCode);
                if (!TabPermissionController.CanViewPage(tabInfo))
                {
                    dnnPages.RemoveLocale(localTabInfo.CultureCode);
                    dnnPages.Pages.Remove(dnnPage);
                    break;
                }
                dnnPage.TabId = localTabInfo.TabID;
                dnnPage.TabName = localTabInfo.TabName;
                dnnPage.Title = localTabInfo.Title;
                dnnPage.Description = localTabInfo.Description;
                dnnPage.Path = localTabInfo.TabPath.Substring(0, localTabInfo.TabPath.LastIndexOf("//", StringComparison.Ordinal)).Replace("//", "");
                dnnPage.HasChildren = (TabController.Instance.GetTabsByPortal(PortalId).WithParentId(tabInfo.TabID).Count != 0);
                dnnPage.CanAdminPage = TabPermissionController.CanAdminPage(tabInfo);
                dnnPage.CanViewPage = TabPermissionController.CanViewPage(tabInfo);
                dnnPage.LocalResourceFile = LocalResourcesFile;
                dnnPage.PageUrl = Globals.NavigateURL(localTabInfo.TabID, false, PortalSettings, "", localTabInfo.CultureCode, new string[] { });

                // calculate position in the form of 1.3.2...
                var siblingTabs = tabInfos.Where(t => t.ParentId == localTabInfo.ParentId && t.CultureCode == localTabInfo.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture);
                var parentTabId = localTabInfo.ParentId;
                while (parentTabId > 0)
                {
                    var parentTab = tabInfos.Single(t => t.TabID == parentTabId);
                    var id = parentTabId;
                    siblingTabs = tabInfos.Where(t => t.ParentId == id && t.CultureCode == localTabInfo.CultureCode || t.CultureCode == null).OrderBy(t => t.TabOrder).ToList();
                    dnnPage.Position = (siblingTabs.IndexOf(localTabInfo) + 1).ToString(CultureInfo.InvariantCulture) + "." + dnnPage.Position;
                    parentTabId = parentTab.ParentId;
                }

                dnnPage.DefaultLanguageGuid = localTabInfo.DefaultLanguageGuid;
                dnnPage.IsTranslated = localTabInfo.IsTranslated;
                dnnPage.IsPublished = TabController.Instance.IsTabPublished(localTabInfo);
                // generate modules information
                var moduleController = ModuleController.Instance;
                foreach (var moduleInfo in moduleController.GetTabModules(localTabInfo.TabID).Values)
                {
                    var guid = moduleInfo.DefaultLanguageGuid == Null.NullGuid ? moduleInfo.UniqueId : moduleInfo.DefaultLanguageGuid;

                    var dnnModules = dnnPages.Module(guid); // modules of each language
                    var dnnModule = dnnModules.Module(localTabInfo.CultureCode);
                    // detect error : 2 modules with same uniqueId on the same page
                    dnnModule.LocalResourceFile = LocalResourcesFile;
                    if (dnnModule.TabModuleId > 0)
                    {
                        dnnModule.ErrorDuplicateModule = true;
                        dnnPages.ErrorExists = true;
                        continue;
                    }

                    dnnModule.ModuleTitle = moduleInfo.ModuleTitle;
                    dnnModule.DefaultLanguageGuid = moduleInfo.DefaultLanguageGuid;
                    dnnModule.TabId = localTabInfo.TabID;
                    dnnModule.TabModuleId = moduleInfo.TabModuleID;
                    dnnModule.ModuleId = moduleInfo.ModuleID;
                    dnnModule.CanAdminModule = ModulePermissionController.CanAdminModule(moduleInfo);
                    dnnModule.CanViewModule = ModulePermissionController.CanViewModule(moduleInfo);
                    dnnModule.IsDeleted = moduleInfo.IsDeleted;
                    if (moduleInfo.DefaultLanguageGuid != Null.NullGuid)
                    {
                        var defaultLanguageModule = moduleController.GetModuleByUniqueID(moduleInfo.DefaultLanguageGuid);
                        if (defaultLanguageModule != null)
                        {
                            dnnModule.DefaultModuleId = defaultLanguageModule.ModuleID;
                            if (defaultLanguageModule.ParentTab.UniqueId != moduleInfo.ParentTab.DefaultLanguageGuid)
                                dnnModule.DefaultTabName = defaultLanguageModule.ParentTab.TabName;
                        }
                    }
                    dnnModule.SetModuleInfoHelp();
                    dnnModule.IsTranslated = moduleInfo.IsTranslated;
                    dnnModule.IsLocalized = moduleInfo.IsLocalized;

                    dnnModule.IsShared = TabController.Instance.GetTabsByModuleID(moduleInfo.ModuleID).Values.Count(t => t.CultureCode == moduleInfo.CultureCode) > 1;

                    // detect error : the default language module is on an other page
                    dnnModule.ErrorDefaultOnOtherTab = moduleInfo.DefaultLanguageGuid != Null.NullGuid && moduleInfo.DefaultLanguageModule == null;

                    // detect error : different culture on tab and module
                    dnnModule.ErrorCultureOfModuleNotCultureOfTab = moduleInfo.CultureCode != localTabInfo.CultureCode;

                    dnnPages.ErrorExists |= dnnModule.ErrorDefaultOnOtherTab || dnnModule.ErrorCultureOfModuleNotCultureOfTab;
                }
            }

            return dnnPages;
        }

        private void SaveNonLocalizedPages(DnnPagesDto pages)
        {
            // check all pages except the default "en-US" one
            var tabController = TabController.Instance;
            foreach (var page in pages.Pages)
            {
                var tabInfo = tabController.GetTab(page.TabId, PortalId, true);
                if (tabInfo != null &&
                    (tabInfo.TabName != page.TabName ||
                     tabInfo.Title != page.Title ||
                     tabInfo.Description != page.Description))
                {
                    tabInfo.TabName = page.TabName;
                    tabInfo.Title = page.Title;
                    tabInfo.Description = page.Description;
                    tabController.UpdateTab(tabInfo);
                }
            }

            var tabsToPublish = new List<TabInfo>();
            var moduleTranslateOverrides = new Dictionary<int, bool>();
            var moduleController = ModuleController.Instance;

            // manage all actions we need to take for all modules on all pages
            foreach (var modulesCollection in pages.Modules)
            {
                foreach (var moduleDto in modulesCollection.Modules)
                {
                    var tabModule = moduleController.GetTabModule(moduleDto.TabModuleId);
                    if (tabModule != null)
                    {
                        if (tabModule.ModuleTitle != moduleDto.ModuleTitle)
                        {
                            tabModule.ModuleTitle = moduleDto.ModuleTitle;
                            moduleController.UpdateModule(tabModule);
                        }

                        if (tabModule.DefaultLanguageGuid != Null.NullGuid &&
                            tabModule.IsLocalized != moduleDto.IsLocalized)
                        {
                            var locale = LocaleController.Instance.GetLocale(tabModule.CultureCode);
                            if (moduleDto.IsLocalized)
                                moduleController.LocalizeModule(tabModule, locale);
                            else
                                moduleController.DeLocalizeModule(tabModule);
                        }

                        bool moduleTranslateOverride;
                        moduleTranslateOverrides.TryGetValue(tabModule.TabID, out moduleTranslateOverride);

                        if (!moduleTranslateOverride && tabModule.IsTranslated != moduleDto.IsTranslated)
                        {
                            moduleController.UpdateTranslationStatus(tabModule, moduleDto.IsTranslated);
                        }
                    }
#if false
                    else
                    {
                        // There was a logic here in the original ControlBar
                        // functionality that is not maintained in the new PB.
                    }
#endif
                }
            }

            foreach (var page in pages.Pages)
            {
                var tabInfo = tabController.GetTab(page.TabId, PortalId, true);
                if (tabInfo != null)
                {
                    var moduleTranslateOverride = false;
                    if (!tabInfo.IsDefaultLanguage)
                    {
                        if (tabInfo.IsTranslated != page.IsTranslated)
                        {
                            tabController.UpdateTranslationStatus(tabInfo, page.IsTranslated);
                            if (page.IsTranslated)
                            {
                                moduleTranslateOverride = true;
                                var tabModules = moduleController.GetTabModules(tabInfo.TabID)
                                    .Where(moduleKvp =>
                                        moduleKvp.Value.DefaultLanguageModule != null &&
                                        moduleKvp.Value.LocalizedVersionGuid != moduleKvp.Value.DefaultLanguageModule.LocalizedVersionGuid);

                                foreach (var moduleKvp in tabModules)
                                {
                                    moduleController.UpdateTranslationStatus(moduleKvp.Value, true);
                                }
                            }
                        }

                        if (page.IsPublished)
                        {
                            tabsToPublish.Add(tabInfo);
                        }
                    }

                    moduleTranslateOverrides.Add(page.TabId, moduleTranslateOverride);
                }
            }

            // if we have tabs to publish, do it.
            // marks all modules as translated, marks page as translated
            foreach (var tabInfo in tabsToPublish)
            {
                //First mark all modules as translated
                foreach (var module in moduleController.GetTabModules(tabInfo.TabID).Values)
                {
                    moduleController.UpdateTranslationStatus(module, true);
                }

                //Second mark tab as translated
                tabController.UpdateTranslationStatus(tabInfo, true);

                //Third publish Tab (update Permissions)
                tabController.PublishTab(tabInfo);
            }

            // manage translated status of tab. In order to do that, we need to check if all modules on the page are translated
            var tabTranslatedStatus = true;
            foreach (var page in pages.Pages)
            {
                var tabInfo = tabController.GetTab(page.TabId, PortalId, true);
                if (tabInfo != null)
                {
                    if (tabInfo.ChildModules.Any(moduleKvp => !moduleKvp.Value.IsTranslated))
                    {
                        tabTranslatedStatus = false;
                    }

                    if (tabTranslatedStatus && !tabInfo.IsTranslated)
                    {
                        tabController.UpdateTranslationStatus(tabInfo, true);
                    }
                }
            }
        }
    }

    internal static class KpvExtension
    {
        public static IEnumerable<LocalizationEntry> MapEntries(this IEnumerable<KeyValuePair<string, string>> list)
        {
            var appPath = Globals.ApplicationMapPath;
            var appPathLen = appPath.Length;
            if (!appPath.EndsWith(@"\")) appPathLen++;

            return list.Select(kpv => new LocalizationEntry
            {
                Name = kpv.Key,
                NewValue = (kpv.Value.StartsWith(appPath) ? kpv.Value.Substring(appPathLen) : kpv.Value).Replace(@"\", @"/")
            });
        }
    }
}