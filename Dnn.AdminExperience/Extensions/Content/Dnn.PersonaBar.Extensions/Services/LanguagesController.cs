using System;
using System.Collections;
using System.Collections.Generic;
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
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.SiteSettings.Components.Constants;
using Dnn.PersonaBar.SiteSettings.Components;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.SiteSettings.Services
{
    [MenuPermission(Scope = ServiceScope.Admin)]
    public class LanguagesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesController));
        private const string LocalResourcesFile = "~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.SiteSettings/App_LocalResources/SiteSettings.resx";
        private const string AuthFailureMessage = "Authorization has been denied for this request.";

        // Sample matches:
        // MyResources.ascx.en-US.resx
        // MyResources.ascx.en-US.Host.resx
        // MyResources.ascx.en-US.Portal-123.resx
        internal static readonly Regex FileInfoRegex = new Regex(
            @"\.([a-z]{2,3}\-[0-9A-Z]{2,4}(-[A-Z]{2})?)(\.(Host|Portal-\d+))?\.resx$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        private ITabController _tabController = TabController.Instance;
        private ILocaleController _localeController = LocaleController.Instance;
        private IPortalController _portalController = PortalController.Instance;

        private string _selectedResourceFile;

        #region -------------------------------- PUBLIC API METHODS SEPARATOR --------------------------------
        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [P] to expand source code folding
        #endregion

        // GET /api/personabar/pages/GetTabsForTranslation?portalId=&cultureCode=fr-FR
        [HttpGet]
        public HttpResponseMessage GetTabsForTranslation(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var nonTranslatedTabs = GetTabsForTranslationInternal(pid, cultureCode);
                return Request.CreateResponse(HttpStatusCode.OK, nonTranslatedTabs);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetRootResourcesFolders
        [HttpGet]
        public HttpResponseMessage GetRootResourcesFolders()
        {
            try
            {
                var folders = new List<KeyValuePair<string, string>>();
                var server = HttpContext.Current.Server;

                folders.Add(new KeyValuePair<string, string>(LocalizeString("LocalResources"), "_"));
                folders.Add(new KeyValuePair<string, string>(LocalizeString("GlobalResources"), server.MapPath("~/App_GlobalResources")));
                folders.Add(new KeyValuePair<string, string>(LocalizeString("SiteTemplates"), server.MapPath("~/Portals/_default")));

                return Request.CreateResponse(HttpStatusCode.OK,
                    new
                    {
                        Folders = folders.MapEntries()
                    });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
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
                    folders.AddRange(new[]
                    {
                        "Admin",
                        "Controls",
                        "DesktopModules",
                        "Install",
                        "Providers"
                    }.Select(s => new KeyValuePair<string, string>(s, server.MapPath("~/" + "_/" + s))));

                    const string skins = "Skins";
                    var skinsPath = Path.Combine(Globals.ApplicationMapPath, skins);

                    if (Directory.Exists(skinsPath) && HasLocalResources(skinsPath))
                    {
                        folders.Add(new KeyValuePair<string, string>(LocalizeString("HostSkins"), skinsPath));
                    }

                    var portalSkinFolder = Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, skins);
                    if (Directory.Exists(portalSkinFolder) &&
                        PortalSettings.ActiveTab.ParentId == PortalSettings.AdminTabId)
                    {
                        folders.Add(new KeyValuePair<string, string>(
                            LocalizeString("PortalSkins"),
                            Path.Combine(PortalSettings.HomeSystemDirectoryMapPath, skins)));
                    }
                }
                else
                {
                    string foldername = currentFolder;
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        foldername = foldername.Substring(2);
                    }
                    var directories = GetResxDirectories(server.MapPath("~/" + foldername));
                    var directoryFiles = GetResxFiles(server.MapPath("~/" + foldername));
                    if (currentFolder.IndexOf("_/", StringComparison.Ordinal) == 0)
                    {
                        folders.AddRange(directories.Select(
                                s => new KeyValuePair<string, string>(s.Key, s.Value.Replace(foldername.Replace("/", "\\"), currentFolder))));
                        files.AddRange(directoryFiles.Select(
                                f => new KeyValuePair<string, string>(f.Key, f.Value.Replace(foldername.Replace("/", "\\"), currentFolder))));
                    }
                    else
                    {
                        folders.AddRange(directories);
                        files.AddRange(directoryFiles);
                    }
                }

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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // GET /api/personabar/languages/GetResxEntries?portalId=&mode=Portal&locale=de-DE&resourceFile=App_GlobalResources%2fFileUpload.resx
        [HttpGet]
        public HttpResponseMessage GetResxEntries(int? portalId, string mode, string locale, string resourceFile, bool highlight = false)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                LanguageResourceMode resourceMode;
                Enum.TryParse(mode, false, out resourceMode);

                if (!UserInfo.IsSuperUser && (resourceMode == LanguageResourceMode.Host || resourceMode == LanguageResourceMode.System))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = _localeController.GetLocale(locale);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        {
                            
                            break;
                        }
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

                if (language == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(LocalizeString("InvalidLocale.ErrorMessage"), locale));
                }

                _selectedResourceFile = !string.IsNullOrEmpty(resourceFile)
                    ? HttpContext.Current.Server.MapPath("~/" + resourceFile)
                    : HttpContext.Current.Server.MapPath(Localization.GlobalResourceFile);

                var editTable = LoadFile(pid, resourceMode, "Edit", locale);
                var defaultTable = LoadFile(pid, resourceMode, "Default", locale);

                var fullPath = Path.GetFileName(ResourceFile(pid, locale, resourceMode).Replace(Globals.ApplicationMapPath, ""));
                var folder = ResourceFile(pid, locale, resourceMode).Replace(Globals.ApplicationMapPath, "").Replace("\\" + resourceFile, "");

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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/SaveResxEntries
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveResxEntries(UpdateTransaltionsRequest request)
        {
            try
            {
                var pid = request.PortalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                LanguageResourceMode resourceMode;
                Enum.TryParse(request.Mode, false, out resourceMode);

                if (!UserInfo.IsSuperUser && (resourceMode == LanguageResourceMode.Host || resourceMode == LanguageResourceMode.System))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var language = _localeController.GetLocale(request.Locale);

                switch (resourceMode)
                {
                    case LanguageResourceMode.System:
                    case LanguageResourceMode.Host:
                    case LanguageResourceMode.Portal:
                        break;
                    default:
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "UnsupportedMode");
                }

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
                var message = SaveResourceFileFile(pid, resourceMode, request.Locale, request.Entries);
                return Request.CreateResponse(HttpStatusCode.OK, new { Message = message });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/EnableLocalizedContent?portalId=&translatePages=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage EnableLocalizedContent([FromUri] int? portalId, [FromUri] bool translatePages)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var progress = new LocalizationProgress { InProgress = true, };
                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
                LanguagesControllerTasks.LocalizeSitePages(
                    progress, pid, translatePages, portalSettings.DefaultLanguage ?? Localization.SystemLocale);
                return Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/LocalizedContent?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage LocalizedContent([FromUri] int? portalId, [FromUri] string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var progress = new LocalizationProgress { InProgress = true, };
                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
                LanguagesControllerTasks.LocalizeLanguagePages(
                    progress, pid, cultureCode, portalSettings.DefaultLanguage ?? Localization.SystemLocale);
                return Request.CreateResponse(HttpStatusCode.OK, progress);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
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
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/DisableLocalizedContent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DisableLocalizedContent([FromUri] int? portalId)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                var portal = PortalController.Instance.GetPortal(pid);
                var portalSettings = new PortalSettings(portal);
                var portalDefault = portalSettings.DefaultLanguage;
                foreach (var locale in _localeController.GetLocales(pid).Values.Where(l => l.Code != portalDefault))
                {
                    _localeController.PublishLanguage(pid, locale.Code, false);
                    _tabController.DeleteTranslatedTabs(pid, locale.Code, false);
                    _portalController.RemovePortalLocalization(pid, locale.Code, false);
                    DataProvider.Instance().EnsureLocalizationExists(pid, locale.Code);
                }

                _tabController.EnsureNeutralLanguage(pid, portalDefault, false);
                PortalController.UpdatePortalSetting(pid, "ContentLocalizationEnabled", "False");
                DataCache.ClearPortalCache(pid, true);

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/MarkAllPagesTranslated?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkAllPagesTranslated([FromUri] int? portalId, [FromUri] string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = _localeController.GetLocale(pid, cultureCode);
                if (locale == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var nonTranslatedTabs =
                    from t in _tabController.GetTabsByPortal(pid).WithCulture(locale.Code, false).Values
                    where !t.IsTranslated && !t.IsDeleted
                    select t;

                foreach (var page in nonTranslatedTabs)
                {
                    page.LocalizedVersionGuid = page.DefaultLanguageTab.LocalizedVersionGuid;
                    _tabController.UpdateTab(page);
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/ActivateLanguage?portalId=&cultureCode=de-DE&enable=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ActivateLanguage([FromUri] int? portalId, [FromUri] string cultureCode, [FromUri] bool enable)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = _localeController.GetLocale(pid, cultureCode);
                if (locale == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                _localeController.ActivateLanguage(pid, locale.Code, enable);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/PublishAllPages?portalId=&cultureCode=de-DE&enable=true
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage PublishAllPages([FromUri] int? portalId, [FromUri] string cultureCode, [FromUri] bool enable)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = _localeController.GetLocale(pid, cultureCode);
                if (locale == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                _localeController.PublishLanguage(pid, locale.Code, enable);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        // POST /api/personabar/languages/DeleteLanguagePages?portalId=&cultureCode=de-DE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteLanguagePages(int? portalId, string cultureCode)
        {
            try
            {
                var pid = portalId ?? PortalId;
                if (!UserInfo.IsSuperUser && pid != PortalId)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
                }

                if (IsDefaultLanguage(pid, cultureCode))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                var locale = _localeController.GetLocale(pid, cultureCode);
                if (locale == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "InvalidCulture");
                }

                _tabController.DeleteTranslatedTabs(pid, locale.Code, false);
                _portalController.RemovePortalLocalization(pid, locale.Code, false);
                _localeController.PublishLanguage(pid, locale.Code, false);

                DataCache.ClearPortalCache(pid, true);
                return Request.CreateResponse(HttpStatusCode.OK, new { Success = true });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.ToString());
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

        private static string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourcesFile);
        }

        private bool IsDefaultLanguage(int portalId, string cultureCode)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalSettings = new PortalSettings(portal);
            return string.Equals(cultureCode, portalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsLanguageEnabled(int portalId, string cultureCode)
        {
            return _localeController.GetLocales(portalId).ContainsKey(cultureCode);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Loads resources from file
        /// </summary>
        /// <param name = "portalId">Portal Id</param>
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
        private Hashtable LoadFile(int portalId, LanguageResourceMode mode, string type, string locale)
        {
            string file;
            var ht = new Hashtable();

            if (type == "Edit")
            {
                // Only load resources from the file being edited
                file = ResourceFile(portalId, locale, mode);
                LoadResource(ht, file);
            }
            else if (type == "Default")
            {
                // Load system default
                file = ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System);
                LoadResource(ht, file);

                if (mode == LanguageResourceMode.Host)
                {
                    // Load base file for selected locale
                    file = ResourceFile(portalId, locale, LanguageResourceMode.System);
                    LoadResource(ht, file);
                }
                else if (mode == LanguageResourceMode.Portal)
                {
                    //Load host override for default locale
                    file = ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.Host);
                    LoadResource(ht, file);

                    if (locale != Localization.SystemLocale)
                    {
                        // Load base file for locale
                        file = ResourceFile(portalId, locale, LanguageResourceMode.System);
                        LoadResource(ht, file);

                        //Load host override for selected locale
                        file = ResourceFile(portalId, locale, LanguageResourceMode.Host);
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
            var d = new XmlDocument { XmlResolver = null };
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
                {
                    foreach (XmlNode nLoopVariable in nLoopVariables)
                    {
                        var n = nLoopVariable;
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            var selectSingleNode = n.SelectSingleNode("value");
                            if (selectSingleNode != null)
                            {
                                var val = selectSingleNode.InnerText;
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
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Returns the resource file name for a given resource and language
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="language">Language Name.</param>
        /// <param name = "mode">Identifies the resource being searched (System, Host, Portal)</param>
        /// <returns>Localized File Name</returns>
        /// -----------------------------------------------------------------------------
        private string ResourceFile(int portalId, string language, LanguageResourceMode mode)
        {
            return Localization.GetResourceFileName(_selectedResourceFile, language, mode.ToString(), portalId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Updates all values from the datagrid
        /// </summary>
        /// -----------------------------------------------------------------------------
        private string SaveResourceFileFile(int portalId, LanguageResourceMode mode, string locale, IEnumerable<LocalizationEntry> entries)
        {
            var resDoc = new XmlDocument { XmlResolver = null };
            var defDoc = new XmlDocument { XmlResolver = null };

            var filename = ResourceFile(portalId, locale, mode);
            resDoc.Load(File.Exists(filename)
                ? filename :
                ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System));

            defDoc.Load(ResourceFile(portalId, Localization.SystemLocale, LanguageResourceMode.System));

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
                        node.InnerText = txtValue;
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
                            node.InnerText = txtValue;
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
                log.LogProperties.Add(new LogDetailInfo(LocalizeString("ResourceUpdated"), ResourceFile(portalId, locale, mode)));
                log.LogProperties.Add(new LogDetailInfo("Updated Values", values));
                LogController.Instance.AddLog(log);
            }

            return string.Format(LocalizeString("Updated"), ResourceFile(portalId, locale, mode));
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

        private IList<LanguageTabDto> GetTabsForTranslationInternal(int portalId, string cultureCode)
        {
            var locale = new LocaleController().GetLocale(portalId, cultureCode);
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalSettings = new PortalSettings(portal);
            var pages = new List<LanguageTabDto>();
            if (locale != null && locale.Code != portalSettings.DefaultLanguage)
            {
                var portalTabs = _tabController.GetTabsByPortal(portalId).WithCulture(locale.Code, false).Values;
                var nonTranslated = (from t in portalTabs where !t.IsTranslated && !t.IsDeleted select t);
                pages.AddRange(
                    nonTranslated.Select(page => new LanguageTabDto()
                    {
                        PageId = page.TabID,
                        PageName = page.TabName,
                        ViewUrl = Globals.NavigateURL(page.TabID),
                    }));
            }
            return pages;
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