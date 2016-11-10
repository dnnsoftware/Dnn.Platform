using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using Dnn.PersonaBar.SiteSettings.Components.Constants;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;
using System.Web.UI;
using Dnn.PersonaBar.SiteSettings.Services.Dto;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Web.Api;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services
{
    [ServiceScope(Scope = ServiceScope.Admin, Identifier = "Languages")]
    public class LanguagesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(LanguagesController));

        private const string LocalResourceFile = "~/DesktopModules/Admin/Dnn.PersonaBar/App_LocalResources/SiteSettings.resx";
        private const string LocalizationProgressFile = "PersonaBarLocalizationProgress.txt";

        // sample formats:
        // MyResources.ascx.en-US.resx
        // MyResources.ascx.en-US.Host.resx
        // MyResources.ascx.en-US.Portal-123.resx
        protected static readonly Regex FileInfoRegex = new Regex(
            @"\.(\w\w\-\w\w\w?)(\.Host)?(\.Portal-(\d+))?\.resx$", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

        private string _selectedResourceFile;

        #region -------------------------------- PUBLIC API METHODS SEPARATOR --------------------------------
        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [L] to expand source code folding
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
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "MissingParams");
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
                        string.Format(Localization.GetString("InvalidLocale.ErrorMessage", LocalResourceFile), locale));
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
                        Logger.Warn(Localization.GetString("Obsolete", LocalResourceFile));
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
                        string.Format(Localization.GetString("InvalidLocale.ErrorMessage", LocalResourceFile), request.Locale));
                }

                if (string.IsNullOrEmpty(request.ResourceFile))
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        string.Format(Localization.GetString("InvalidLocale.ErrorMessage", LocalResourceFile), request.Locale));
                }

                //TODO
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "TODO; NotImplemented");
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
                LocalizeSitePages(progress, PortalId, translatePages, portalSettings.DefaultLanguage);
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
                var progress = ReadProgressFile();
                return Request.CreateResponse(HttpStatusCode.OK, progress);
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
        public HttpResponseMessage DiableLocalizedContent()
        {
            try
            {
                //TODO
                return Request.CreateResponse(HttpStatusCode.OK, new { failed = true});
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #region -------------------------------- PRIVATE METHODS SEPARATOR --------------------------------
        // From inside Visual Studio editor press [CTRL]+[M] then [O] to collapse source code to definition
        // From inside Visual Studio editor press [CTRL]+[M] then [L] to expand source code folding
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
            return Localization.GetString(key, LocalResourceFile);
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
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private string ResourceFile(string language, LanguageResourceMode mode)
        {
            return Localization.GetResourceFileName(_selectedResourceFile, language, mode.ToString(), PortalId);
        }

        /*
        private string GetResourceKeyXPath(string resourceKeyName)
        {
            return "//root/data[@name=" + XmlUtils.XPathLiteral(resourceKeyName) + "]";
        }

        private XmlNode AddResourceKey(XmlDocument resourceDoc, string resourceKey)
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
        */

        private static void LocalizeSitePages(LocalizationProgress progress, int portalId, bool translatePages, string defaultLocale)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var languageCount = LocaleController.Instance.GetLocales(portalId).Count;
                    var pageList = GetPages(portalId);
                    var languageCounter = 0;

                    if (translatePages)
                    {
                        ProcessLanguage(pageList, LocaleController.Instance.GetLocale(defaultLocale),
                            defaultLocale, languageCounter, languageCount, progress);
                    }

                    PublishLanguage(defaultLocale, portalId, true);

                    PortalController.UpdatePortalSetting(portalId, "ContentLocalizationEnabled", "True");

                    // populate other languages
                    var defaultLanguage = PortalController.Instance.GetCurrentPortalSettings().DefaultLanguage;
                    foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values)
                    {
                        if (locale.Code != defaultLocale)
                        {
                            languageCounter++;
                            pageList = GetPages(portalId).Where(p => p.CultureCode == defaultLanguage).ToList();

                            //add translator role
                            Localization.AddTranslatorRole(portalId, locale);

                            //populate pages
                            ProcessLanguage(pageList, locale, defaultLocale, languageCounter, languageCount, progress);

                            //Map special pages
                            PortalController.Instance.MapLocalizedSpecialPages(portalId, locale.Code);
                        }
                    }

                    //clear portal cache
                    DataCache.ClearPortalCache(portalId, true);
                    progress.Reset();
                    SaveProgressToFile(progress);
                }
                catch (Exception ex)
                {
                    try
                    {
                        Logger.Error(ex);
                        progress.Reset().CurrentOperationText = ex.Message;
                        SaveProgressToFile(progress);
                    }
                    catch (Exception)
                    {
                        //ignore
                    }
                }
            });
        }

        private static void ProcessLanguage(ICollection<TabInfo> pageList, Locale locale,
            string defaultLocale, int languageCount, int totalLanguages, LocalizationProgress progress)
        {
            progress.PrimaryTotal = totalLanguages;
            progress.PrimaryValue = languageCount;

            var total = pageList.Count;
            if (total == 0)
            {
                progress.SecondaryTotal = 0;
                progress.SecondaryValue = 0;
                progress.SecondaryPercent = 100;
            }

            for (var i = 0; i < total; i++)
            {
                var currentTab = pageList.ElementAt(i);
                var stepNo = i + 1;

                progress.SecondaryTotal = total;
                progress.SecondaryValue = stepNo;
                progress.SecondaryPercent = Convert.ToInt32((float)stepNo / total * 100);
                progress.PrimaryPercent =
                    Convert.ToInt32((languageCount + (float)stepNo / total) / totalLanguages * 100);

                progress.CurrentOperationText = string.Format(Localization.GetString(
                    "ProcessingPage", LocalResourceFile), locale.Code, stepNo, total, currentTab.TabName);

                progress.TimeEstimated = (total - stepNo) * 100;

                SaveProgressToFile(progress);

                if (locale.Code == defaultLocale)
                {
                    TabController.Instance.LocalizeTab(currentTab, locale, true);
                }
                else
                {
                    TabController.Instance.CreateLocalizedCopy(currentTab, locale, false);
                }
            }
        }

        private static void SaveProgressToFile(LocalizationProgress progress)
        {
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
            var text = JsonConvert.SerializeObject(progress);
#if false
            // this could have file locking issues from multiple threads
            File.WriteAllText(path, text);
#else
            using (var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 256))
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                file.Write(bytes, 0, bytes.Length);
                file.Flush();
            }
#endif
        }

        private static LocalizationProgress ReadProgressFile()
        {
            var path = Path.Combine(Globals.ApplicationMapPath, "App_Data", LocalizationProgressFile);
#if true
            var text = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<LocalizationProgress>(text);
#else
            using (var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 256))
            {
                var bytes = new byte[file.Length];
                file.Read(bytes, 0, bytes.Length);
                var text = Encoding.UTF8.GetString(bytes);
                return JsonConvert.DeserializeObject<LocalizationProgress>(text);
            }
#endif
        }

        private static IList<TabInfo> GetPages(int portalId)
        {
            return (
                from kvp in TabController.Instance.GetTabsByPortal(portalId)
                where !kvp.Value.TabPath.StartsWith("//Admin")
                      && !kvp.Value.IsDeleted
                      && !kvp.Value.IsSystem
                select kvp.Value
                ).ToList();
        }

        private static void PublishLanguage(string cultureCode, int portalId, bool publish)
        {
            var enabledLanguages = LocaleController.Instance.GetLocales(portalId);
            Locale enabledlanguage;
            if (enabledLanguages.TryGetValue(cultureCode, out enabledlanguage))
            {
                enabledlanguage.IsPublished = publish;
                LocaleController.Instance.UpdatePortalLocale(enabledlanguage);
            }
        }

    }

    public static class KpvExtension
    {
        public static IEnumerable<NameValueEntry> MapEntries(this IEnumerable<KeyValuePair<string, string>> list)
        {
            var appPath = Globals.ApplicationMapPath;
            var appPathLen = appPath.Length;
            if (!appPath.EndsWith(@"\")) appPathLen++;

            return list.Select(kpv => new NameValueEntry
            {
                Name = kpv.Key,
                Value = (kpv.Value.StartsWith(appPath) ? kpv.Value.Substring(appPathLen) : kpv.Value).Replace(@"\", @"/")
            });
        }
    }
}