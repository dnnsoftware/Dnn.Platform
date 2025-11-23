namespace DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ResourceManager;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using Microsoft.Extensions.DependencyInjection;

    public class MvcJavaScript
    {
        private const string ScriptPrefix = "JSL.";
        private const string LegacyPrefix = "LEGACY.";

        private const string JQueryUIDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.js";
        private const string JQueryUIMinFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.min.js";

        /// <summary>Initializes a new instance of the <see cref="MvcJavaScript"/> class.</summary>
        protected MvcJavaScript()
        {
        }

        /// <summary>checks whether the script file is a known javascript library.</summary>
        /// <param name="jsname">script identifier.</param>
        /// <returns><see langword="true"/> if a library with the given name is installed, otherwise <see langword="false"/>.</returns>
        public static bool IsInstalled(string jsname)
        {
            JavaScriptLibrary library = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName.Equals(jsname, StringComparison.OrdinalIgnoreCase));
            return library != null;
        }

        /// <summary>determine whether to use the debug script for a file.</summary>
        /// <returns>whether to use the debug script.</returns>
        public static bool UseDebugScript()
        {
            if (DotNetNuke.Common.Globals.Status != DotNetNuke.Common.Globals.UpgradeStatus.None)
            {
                return false;
            }

            return HttpContextSource.Current.IsDebuggingEnabled;
        }

        /// <summary>Returns the version of a javascript library from the database.</summary>
        /// <param name="jsname">the library name.</param>
        /// <returns>The highest version number of the library or <see cref="string.Empty"/>.</returns>
        public static string Version(string jsname)
        {
            JavaScriptLibrary library = GetHighestVersionLibrary(jsname);
            return library != null ? Convert.ToString(library.Version) : string.Empty;
        }

        /// <summary>Requests a script to be added to the page.</summary>
        /// <param name="jsname">the library name.</param>
        public static void RequestRegistration(string jsname)
        {
            // handle case where script has no javascript library
            switch (jsname)
            {
                case CommonJs.jQuery:
                    // RequestRegistration(CommonJs.jQueryMigrate);
                    break;
            }

            RequestRegistration(jsname, null, SpecificVersion.Latest);
        }

        /// <summary>Requests a script to be added to the page.</summary>
        /// <param name="jsname">the library name.</param>
        /// <param name="version">the library's version.</param>
        public static void RequestRegistration(string jsname, Version version)
        {
            RequestRegistration(jsname, version, SpecificVersion.Exact);
        }

        /// <summary>Requests a script to be added to the page.</summary>
        /// <param name="jsname">the library name.</param>
        /// <param name="version">the library's version.</param>
        /// <param name="specific">
        /// how much of the <paramref name="version"/> to pay attention to.
        /// When <see cref="SpecificVersion.Latest"/> is passed, ignore the <paramref name="version"/>.
        /// When <see cref="SpecificVersion.LatestMajor"/> is passed, match the major version.
        /// When <see cref="SpecificVersion.LatestMinor"/> is passed, match the major and minor versions.
        /// When <see cref="SpecificVersion.Exact"/> is passed, match all parts of the version.
        /// </param>
        public static void RequestRegistration(string jsname, Version version, SpecificVersion specific)
        {
            switch (specific)
            {
                case SpecificVersion.Latest:
                    RequestHighestVersionLibraryRegistration(jsname);
                    return;
                case SpecificVersion.LatestMajor:
                case SpecificVersion.LatestMinor:
                    if (RequestLooseVersionLibraryRegistration(jsname, version, specific))
                    {
                        return;
                    }

                    break;
                case SpecificVersion.Exact:
                    RequestSpecificVersionLibraryRegistration(jsname, version);
                    return;
            }

            // this should only occur if packages are incorrect or a RequestRegistration call has a typo
            LogCollision(string.Format("Missing specific version library - {0},{1},{2}", jsname, version, specific));
        }

        /// <summary>method is called once per page event cycle and will load all scripts requested during that page processing cycle.</summary>
        /// <param name="page">reference to the current page.</param>
        public static void Register(ControllerContext page)
        {
            IEnumerable<string> scripts = GetScriptVersions();
            IEnumerable<JavaScriptLibrary> finalScripts = ResolveVersionConflicts(scripts);
            foreach (JavaScriptLibrary jsl in finalScripts)
            {
                if (jsl.LibraryName != "jQuery-Migrate")
                {
                    RegisterScript(page, jsl);
                }
            }
        }

        public static string JQueryUIFile(bool getMinFile)
        {
            return GetScriptPath(CommonJs.jQueryUI);
        }

        public static string GetJQueryScriptReference()
        {
            return GetScriptPath(CommonJs.jQuery);
        }

        public static string GetScriptPath(string libraryName)
        {
            var library = JavaScriptLibraryController.Instance.GetLibrary(jsl => jsl.LibraryName.Equals(libraryName, StringComparison.OrdinalIgnoreCase));
            if (library == null)
            {
                return null;
            }

            return GetScriptPath(library, HttpContextSource.Current?.Request);
        }

        public static void RegisterClientReference(ControllerContext page, ClientAPI.ClientNamespaceReferences reference)
        {
            var controller = GetClientResourcesController();

            switch (reference)
            {
                case ClientAPI.ClientNamespaceReferences.dnn:
                case ClientAPI.ClientNamespaceReferences.dnn_dom:
                    if (HttpContextSource.Current.Items.Contains(LegacyPrefix + "dnn.js"))
                    {
                        break;
                    }

                    // MvcClientResourceManager.RegisterScript(page, ClientAPI.ScriptPath + "MicrosoftAjax.js", 10);
                    controller.CreateScript(ClientAPI.ScriptPath + "mvc.js")
                              .SetPriority(11)
                              .Register();
                    controller.CreateScript(ClientAPI.ScriptPath + "dnn.js")
                              .SetPriority(12)
                              .Register();

                    HttpContextSource.Current.Items.Add(LegacyPrefix + "dnn.js", true);

                    if (!ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.SingleCharDelimiters))
                    {
                        MvcClientAPI.RegisterClientVariable("__scdoff", "1", true);
                    }

                    if (!ClientAPI.UseExternalScripts)
                    {
                        MvcClientAPI.RegisterEmbeddedResource("dnn.scripts.js", typeof(ClientAPI));
                    }

                    break;
                case ClientAPI.ClientNamespaceReferences.dnn_dom_positioning:
                    RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
                    controller.CreateScript(ClientAPI.ScriptPath + "dnn.dom.positioning.js")
                              .SetPriority(13)
                              .Register();
                    break;
            }
        }

        private static void RequestHighestVersionLibraryRegistration(string jsname)
        {
            var library = GetHighestVersionLibrary(jsname);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
            }
            else
            {
                // covers case where upgrading to 7.2.0 and JSL's are not installed
                AddPreInstallorLegacyItemRequest(jsname);
            }
        }

        private static bool RequestLooseVersionLibraryRegistration(string jsname, Version version, SpecificVersion specific)
        {
            Func<JavaScriptLibrary, bool> isValidLibrary = specific == SpecificVersion.LatestMajor
                ? (Func<JavaScriptLibrary, bool>)(l => l.Version.Major == version.Major && l.Version.Minor >= version.Minor)
                : l => l.Version.Major == version.Major && l.Version.Minor == version.Minor && l.Version.Build >= version.Build;
            var library = JavaScriptLibraryController.Instance.GetLibraries(l => l.LibraryName.Equals(jsname, StringComparison.OrdinalIgnoreCase))
                                                              .OrderByDescending(l => l.Version)
                                                              .FirstOrDefault(isValidLibrary);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
                return true;
            }

            // unable to find a higher major version
            library = GetHighestVersionLibrary(jsname);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
                LogCollision("Requested:" + jsname + ":" + version + ":" + specific + ".Resolved:" + library.Version);
                return true;
            }

            return false;
        }

        private static void RequestSpecificVersionLibraryRegistration(string jsname, Version version)
        {
            JavaScriptLibrary library = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName.Equals(jsname, StringComparison.OrdinalIgnoreCase) && l.Version == version);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
            }
            else
            {
                // this will only occur if a specific library is requested and not available
                LogCollision(string.Format("Missing Library request - {0} : {1}", jsname, version));
            }
        }

        private static void AddItemRequest(int javaScriptLibraryId)
        {
            HttpContextSource.Current.Items[ScriptPrefix + javaScriptLibraryId] = true;
        }

        private static void AddPreInstallorLegacyItemRequest(string jsl)
        {
            HttpContextSource.Current.Items[LegacyPrefix + jsl] = true;
        }

        private static IEnumerable<JavaScriptLibrary> ResolveVersionConflicts(IEnumerable<string> scripts)
        {
            var finalScripts = new List<JavaScriptLibrary>();
            foreach (string libraryId in scripts)
            {
                var processingLibrary = JavaScriptLibraryController.Instance.GetLibrary(l => l.JavaScriptLibraryID.ToString(CultureInfo.InvariantCulture) == libraryId);

                var existingLatestLibrary = finalScripts.FindAll(lib => lib.LibraryName.Equals(processingLibrary.LibraryName, StringComparison.OrdinalIgnoreCase))
                                                        .OrderByDescending(l => l.Version)
                                                        .SingleOrDefault();
                if (existingLatestLibrary != null)
                {
                    // determine previous registration for same JSL
                    if (existingLatestLibrary.Version > processingLibrary.Version)
                    {
                        // skip new library & log
                        var collisionText = string.Format(
                            CultureInfo.CurrentCulture,
                            "{0}-{1} -> {2}-{3}",
                            existingLatestLibrary.LibraryName,
                            existingLatestLibrary.Version,
                            processingLibrary.LibraryName,
                            processingLibrary.Version);
                        LogCollision(collisionText);
                    }
                    else if (existingLatestLibrary.Version != processingLibrary.Version)
                    {
                        finalScripts.Remove(existingLatestLibrary);
                        finalScripts.Add(processingLibrary);
                    }
                }
                else
                {
                    finalScripts.Add(processingLibrary);
                }
            }

            return finalScripts;
        }

        private static JavaScriptLibrary GetHighestVersionLibrary(string jsname)
        {
            if (DotNetNuke.Common.Globals.Status == DotNetNuke.Common.Globals.UpgradeStatus.Install)
            {
                // if in install process, then do not use JSL but all use the legacy versions.
                return null;
            }

            try
            {
                return JavaScriptLibraryController.Instance.GetLibraries(l => l.LibraryName.Equals(jsname, StringComparison.OrdinalIgnoreCase))
                                                           .OrderByDescending(l => l.Version)
                                                           .FirstOrDefault();
            }
            catch (Exception)
            {
                // no library found (install or upgrade)
                return null;
            }
        }

        private static string GetScriptPath(JavaScriptLibrary js, HttpRequestBase request)
        {
            if (Host.CdnEnabled)
            {
                // load custom CDN path setting
                var customCdn = HostController.Instance.GetString("CustomCDN_" + js.LibraryName);
                if (!string.IsNullOrEmpty(customCdn))
                {
                    return customCdn;
                }

                // cdn enabled but jsl does not have one defined
                if (!string.IsNullOrEmpty(js.CDNPath))
                {
                    var cdnPath = js.CDNPath;
                    if (cdnPath.StartsWith("//"))
                    {
                        cdnPath = $"{(UrlUtils.IsSecureConnectionOrSslOffload(request) ? "https" : "http")}:{cdnPath}";
                    }

                    return cdnPath;
                }
            }

            return "~/Resources/libraries/" + js.LibraryName + "/" + DotNetNuke.Common.Globals.FormatVersion(js.Version, "00", 3, "_") + "/" + js.FileName;
        }

        private static IEnumerable<string> GetScriptVersions()
        {
            List<string> orderedScripts = (from object item in HttpContextSource.Current.Items.Keys
                                           where item.ToString().StartsWith(ScriptPrefix)
                                           select item.ToString().Substring(4)).ToList();
            orderedScripts.Sort();
            List<string> finalScripts = orderedScripts.ToList();
            foreach (string libraryId in orderedScripts)
            {
                // find dependencies
                var library = JavaScriptLibraryController.Instance.GetLibrary(l => l.JavaScriptLibraryID.ToString() == libraryId);
                if (library == null)
                {
                    continue;
                }

                foreach (var dependencyLibrary in GetAllDependencies(library).Distinct())
                {
                    if (HttpContextSource.Current.Items[ScriptPrefix + "." + dependencyLibrary.JavaScriptLibraryID] == null)
                    {
                        finalScripts.Add(dependencyLibrary.JavaScriptLibraryID.ToString());
                    }
                }
            }

            return finalScripts;
        }

        private static IEnumerable<JavaScriptLibrary> GetAllDependencies(JavaScriptLibrary library)
        {
            var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == library.PackageID);
            foreach (var dependency in package.Dependencies)
            {
                var dependencyLibrary = GetHighestVersionLibrary(dependency.PackageName);
                yield return dependencyLibrary;

                foreach (var childDependency in GetAllDependencies(dependencyLibrary))
                {
                    yield return childDependency;
                }
            }
        }

        private static void LogCollision(string collisionText)
        {
            // need to log an event
            EventLogController.Instance.AddLog(
                "Javascript Libraries",
                collisionText,
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.SCRIPT_COLLISION);
            string strMessage = Localization.GetString("ScriptCollision", Localization.SharedResourceFile);
            /*
            var page = HttpContextSource.Current.Handler as Page;
            if (page != null)
            {
                Skin.AddPageMessage(page, string.Empty, strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
            }
            */
        }

        private static void RegisterScript(ControllerContext page, JavaScriptLibrary jsl)
        {
            if (string.IsNullOrEmpty(jsl.FileName))
            {
                return;
            }

            var controller = GetClientResourcesController();
            controller.CreateScript(GetScriptPath(jsl, HttpContextSource.Current?.Request))
                  .SetPriority(GetFileOrder(jsl))
                  .SetProvider(GetProvider(jsl))
                  .SetNameAndVersion(jsl.LibraryName, jsl.Version.ToString(3), false)
                  .Register();

            /*
            if (Host.CdnEnabled && !string.IsNullOrEmpty(jsl.ObjectName))
            {
                string pagePortion;
                switch (jsl.PreferredScriptLocation)
                {
                    case ScriptLocation.PageHead:

                        pagePortion = "ClientDependencyHeadJs";
                        break;
                    case ScriptLocation.BodyBottom:
                        pagePortion = "ClientResourcesFormBottom";
                        break;
                    case ScriptLocation.BodyTop:
                        pagePortion = "BodySCRIPTS";
                        break;
                    default:
                        pagePortion = "BodySCRIPTS";
                        break;
                }

                Control scriptloader = page.FindControl(pagePortion);
                var fallback = new DnnJsIncludeFallback(jsl.ObjectName, VirtualPathUtility.ToAbsolute("~/Resources/libraries/" + jsl.LibraryName + "/" + Globals.FormatVersion(jsl.Version, "00", 3, "_") + "/" + jsl.FileName));
                if (scriptloader != null)
                {
                    // add the fallback control after script loader.
                    var index = scriptloader.Parent.Controls.IndexOf(scriptloader);
                    scriptloader.Parent.Controls.AddAt(index + 1, fallback);
                }
            }
            */
        }

        private static string GetProvider(JavaScriptLibrary jsl)
        {
            if (jsl.PreferredScriptLocation== ScriptLocation.PageHead)
            {
                return "DnnPageHeaderProvider";
            }
            else if (jsl.PreferredScriptLocation == ScriptLocation.PageHead)
            {
                return "DnnBodyProvider";
            }
            else if (jsl.PreferredScriptLocation == ScriptLocation.PageHead)
            {
                return "DnnFormBottomProvider";
            }
            return string.Empty;
        }

        private static int GetFileOrder(JavaScriptLibrary jsl)
        {
            switch (jsl.LibraryName)
            {
                case CommonJs.jQuery:
                    return (int)FileOrder.Js.jQuery;
                case CommonJs.jQueryMigrate:
                    return (int)FileOrder.Js.jQueryMigrate;
                case CommonJs.jQueryUI:
                    return (int)FileOrder.Js.jQueryUI;
                case CommonJs.HoverIntent:
                    return (int)FileOrder.Js.HoverIntent;
                default:
                    return jsl.PackageID + (int)FileOrder.Js.DefaultPriority;
            }
        }

        private static IClientResourceController GetClientResourcesController()
        {
            var serviceProvider = DotNetNuke.Common.Globals.GetCurrentServiceProvider();
            return serviceProvider.GetRequiredService<IClientResourceController>();
        }
    }
}
