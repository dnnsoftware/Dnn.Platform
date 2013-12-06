#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.UI.Skins;
using DotNetNuke.UI.Skins.Controls;
using DotNetNuke.UI.Utilities;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Framework.JavaScriptLibraries
{
    public class JavaScript
    {
        private const string ScriptPreix = "JSL.";
        private const string LegacyPrefix = "LEGACY.";

        #region Public Methods

        /// <summary>
        ///     checks whether the script file is a known javascript library
        /// </summary>
        /// <param name="jsname">script identifier</param>
        /// <returns></returns>
        public static bool IsInstalled(String jsname)
        {
            JavaScriptLibrary library = JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == jsname);
            return library != null;
        }

        /// <summary>
        ///     determine whether to use the debug script for a file
        /// </summary>
        /// <returns>whether to use the debug script</returns>
        public static bool UseDebugScript()
        {
            if (Globals.Status != Globals.UpgradeStatus.None)
            {
                return false;
            }
            return HttpContext.Current.IsDebuggingEnabled;
        }

        /// <summary>
        ///     returns the version of a javascript library from the database
        /// </summary>
        /// <param name="jsname">the library name</param>
        /// <returns></returns>
        public static string Version(String jsname)
        {
            JavaScriptLibrary library =
                JavaScriptLibraryController.Instance.GetLibrary(l => l.LibraryName == jsname);
            return library != null ? Convert.ToString(library.Version) : String.Empty;
        }

        /// <summary>
        ///     adds a request for a script into the page items collection
        /// </summary>
        /// <param name="jsname">the library name</param>
        public static void RequestRegistration(String jsname)
        {
            //handle case where script has no javascript library
            switch (jsname)
            {
                case CommonJs.DnnPlugins:
                case CommonJs.HoverIntent:
                case CommonJs.jQueryFileUpload:
                    AddPreInstallorLegacyItemRequest(jsname);
                    return;
            }

            JavaScriptLibrary library = GetHighestVersionLibrary(jsname);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
            }
            else
            {
                //covers case where upgrading to 7.2.0 and JSL's are not installed
                AddPreInstallorLegacyItemRequest(jsname);
            }
        }

        /// <summary>
        ///     adds a request for a script into the page items collection
        /// </summary>
        /// <param name="jsname">the library name</param>
        public static void RequestRegistration(String jsname, Version version)
        {
            JavaScriptLibrary library = JavaScriptLibraryController.Instance.GetLibrary(l => l.Version == version);
            if (library != null)
            {
                AddItemRequest(library.JavaScriptLibraryID);
            }
            else
            {
                //this will only occur if a specific library is requested and not available
                LogCollision(String.Format("Missing Library request - {0} : {1}", jsname, version));
            }
        }

        /// <summary>
        ///     adds a request for a script into the page items collection
        /// </summary>
        /// <param name="jsname">the library name</param>
        public static void RequestRegistration(String jsname, Version version, SpecificVersion specific)
        {
            JavaScriptLibrary library;
            bool isProcessed = false;
            switch (specific)
            {
                case SpecificVersion.Latest:
                    library = GetHighestVersionLibrary(jsname);
                    AddItemRequest(library.JavaScriptLibraryID);
                    isProcessed = true;
                    break;
                case SpecificVersion.LatestMajor:
                    library =
                        JavaScriptLibraryController.Instance.GetLibraries(l => l.LibraryName == jsname)
                            .OrderByDescending(l => l.Version).FirstOrDefault(l => l.Version.Major >= version.Major);
                    if (library != null)
                    {
                        AddItemRequest(library.JavaScriptLibraryID);
                    }
                    else
                    {
                        //unable to find a higher major version
                        library = GetHighestVersionLibrary(jsname);
                        AddItemRequest(library.JavaScriptLibraryID);
                        LogCollision("Requested:" + jsname + ":" + version + ":" + specific + ".Resolved:" +
                                     library.Version);
                    }
                    isProcessed = true;
                    break;
                case SpecificVersion.LatestMinor:
                    library =
                        JavaScriptLibraryController.Instance.GetLibraries(l => l.LibraryName == jsname)
                            .OrderByDescending(l => l.Version).FirstOrDefault(l => l.Version.Minor >= version.Minor);
                    if (library != null)
                    {
                        AddItemRequest(library.JavaScriptLibraryID);
                    }
                    else
                    {
                        //unable to find a higher minor version
                        library = GetHighestVersionLibrary(jsname);
                        AddItemRequest(library.JavaScriptLibraryID);
                        LogCollision("Requested:" + jsname + ":" + version + ":" + specific + ".Resolved:" +
                                     library.Version);
                    }
                    isProcessed = true;
                    break;
            }
            if (isProcessed == false)
            {
                //this should only occur if packages are incorrect or a RequestRegistration call has a typo
                LogCollision(String.Format("Missing specific version library - {0},{1},{2}", jsname, version, specific));
            }
        }

        /// <summary>
        ///     method is called once per page event cycle and will
        ///     load all scripts requested during that page processing cycle
        /// </summary>
        /// <param name="page">reference to the current page</param>
        public static void Register(Page page)
        {
            HandlePreInstallorLegacyItemRequests(page);
            IEnumerable<string> scripts = GetScriptVersions();
            IEnumerable<JavaScriptLibrary> finalScripts = GetFinalScripts(scripts);
            foreach (JavaScriptLibrary jsl in finalScripts)
            {
                RegisterScript(page, jsl);
            }
        }

        #endregion

        #region Contructors

        protected JavaScript()
        {
        }

        #endregion

        #region Private Methods

        private static void AddItemRequest(int javaScriptLibraryId)
        {
            HttpContext.Current.Items[ScriptPreix + javaScriptLibraryId] = true;
        }

        private static void AddPreInstallorLegacyItemRequest(string jsl)
        {
            HttpContext.Current.Items[LegacyPrefix + jsl] = true;
        }

        private static IEnumerable<JavaScriptLibrary> GetFinalScripts(IEnumerable<string> scripts)
        {
            var finalScripts = new List<JavaScriptLibrary>();
            foreach (string item in scripts)
            {
                JavaScriptLibrary processingLibrary =
                    JavaScriptLibraryController.Instance.GetLibrary(
                        l => l.JavaScriptLibraryID.ToString(CultureInfo.InvariantCulture) == item);

                JavaScriptLibrary existingLatestLibrary =
                    finalScripts.FindAll(lib => lib.LibraryName == processingLibrary.LibraryName)
                        .OrderByDescending(l => l.Version)
                        .SingleOrDefault();
                if (existingLatestLibrary != null)
                {
                    //determine previous registration for same JSL
                    if (existingLatestLibrary.Version > processingLibrary.Version)
                    {
                        //skip new library & log
                        LogCollision(existingLatestLibrary.LibraryName + "-" + existingLatestLibrary.Version + " -> " +
                                     processingLibrary.LibraryName + "-" + processingLibrary.Version);
                    }
                    else
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

        private static JavaScriptLibrary GetHighestVersionLibrary(String jsname)
        {
            try
            {
                IEnumerable<JavaScriptLibrary> librarys = JavaScriptLibraryController.Instance.GetLibraries(l => l.LibraryName == jsname)
                    .OrderByDescending(l => l.Version);
                if (librarys.Any())
                {
                    return librarys.First();
                }
            }
            catch (Exception)
            {
                //no library found (install or upgrade)
                return null;
            }
            return null;
        }

        private static string GetScriptPath(JavaScriptLibrary js)
        {
            if (Host.CdnEnabled)
            {
                //cdn enabled but jsl does not have one defined
                if (!String.IsNullOrEmpty(js.CDNPath))
                {
                    return js.CDNPath;
                }
            }
            return ("~/Resources/libraries/" + js.LibraryName + "/" + js.Version + "/" + js.FileName);
        }

        private static string GetScriptLocation(JavaScriptLibrary js)
        {
            switch (js.PreferredScriptLocation)
            {
                case ScriptLocation.PageHead:
                    return "DnnPageHeaderProvider";
                case ScriptLocation.BodyBottom:
                    return "DnnFormBottomProvider";
                case ScriptLocation.BodyTop:
                    return "DnnBodyProvider";
            }

            return String.Empty;
        }

        private static IEnumerable<string> GetScriptVersions()
        {
            List<string> orderedScripts = (from object item in HttpContext.Current.Items.Keys
                                           where item.ToString().StartsWith(ScriptPreix)
                                           select item.ToString().Substring(4)).ToList();
            orderedScripts.Sort();
            List<string> finalScripts = orderedScripts.ToList();
            foreach (string orderedScript in orderedScripts)
            {
                //find dependencies

                JavaScriptLibrary library =
                    JavaScriptLibraryController.Instance.GetLibrary(
                        l => l.JavaScriptLibraryID.ToString() == orderedScript);
                if (library != null)
                {
                    PackageInfo package = PackageController.Instance.GetExtensionPackage(Null.NullInteger,
                        p => p.PackageID == library.PackageID);
                    if (package.Dependencies.Any())
                    {
                        foreach (PackageDependencyInfo dependency in package.Dependencies)
                        {
                            JavaScriptLibrary dependantlibrary = GetHighestVersionLibrary(dependency.PackageName);
                            if (HttpContext.Current.Items[ScriptPreix + "." + dependantlibrary.JavaScriptLibraryID] ==
                                null)
                            {
                                finalScripts.Add(dependantlibrary.JavaScriptLibraryID.ToString());
                            }
                        }
                    }
                }
            }
            return finalScripts;
        }

        private static void LogCollision(string collisionText)
        {
            //need to log an event
            var objEventLog = new EventLogController();
            objEventLog.AddLog("Javascript Libraries",
                collisionText,
                PortalController.GetCurrentPortalSettings(),
                UserController.GetCurrentUserInfo().UserID,
                EventLogController.EventLogType.SCRIPT_COLLISION);
            string strMessage = Localization.GetString("ScriptCollision", Localization.SharedResourceFile);
            var page = HttpContext.Current.Handler as Page;
            if (page != null)
            {
                Skin.AddPageMessage(page, "", strMessage, ModuleMessage.ModuleMessageType.YellowWarning);
            }
        }

        private static void RegisterScript(Page page, JavaScriptLibrary jsl)
        {
            ClientResourceManager.RegisterScript(page, GetScriptPath(jsl), GetFileOrder(jsl), GetScriptLocation(jsl));

            //workaround to support IE specific script unti we move to IE version that no longer requires this
            if (jsl.LibraryName == CommonJs.jQueryFileUpload)
            {
                ClientResourceManager.RegisterScript(page,
                    "~/Resources/Shared/Scripts/jquery/jquery.iframe-transport.js");
            }

            if (Host.CdnEnabled && !String.IsNullOrEmpty(jsl.ObjectName))
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
                var fallback = new DnnJsIncludeFallback(jsl.ObjectName,
                    VirtualPathUtility.ToAbsolute("~/Resources/libraries/" + jsl.LibraryName + "/" + jsl.Version + "/" +
                                                  jsl.FileName));
                if (scriptloader != null)
                {
                    scriptloader.Controls.Add(fallback);
                }
            }
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
                    return jsl.PackageID + (int) FileOrder.Js.DefaultPriority;

            }
        }

        private static void HandlePreInstallorLegacyItemRequests(Page page)
        {
            List<string> legacyScripts = (from object item in HttpContext.Current.Items.Keys
                where item.ToString().StartsWith(LegacyPrefix)
                select item.ToString().Substring(7)).ToList();
            foreach (string legacyScript in legacyScripts)
            {
                switch (legacyScript)
                {
                    case CommonJs.jQuery:
                        if (GetHighestVersionLibrary(CommonJs.jQuery) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryScriptReference(),
                                FileOrder.Js.jQuery, "DnnPageHeaderProvider");
                        }
                        if (GetHighestVersionLibrary(CommonJs.jQueryMigrate) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryMigrateScriptReference(),
                                FileOrder.Js.jQueryMigrate, "DnnPageHeaderProvider");
                        }
                        break;
                    case CommonJs.jQueryUI:
                        //register dependency
                        if (GetHighestVersionLibrary(CommonJs.jQuery) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryScriptReference(),
                                FileOrder.Js.jQuery, "DnnPageHeaderProvider");
                        }
                        if (GetHighestVersionLibrary(CommonJs.jQueryMigrate) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryMigrateScriptReference(),
                                FileOrder.Js.jQueryMigrate, "DnnPageHeaderProvider");
                        }
                        //actual jqueryui
                        if (GetHighestVersionLibrary(CommonJs.jQueryUI) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryUIScriptReference(),
                                FileOrder.Js.jQueryUI, "DnnPageHeaderProvider");
                        }
                        break;
                    case CommonJs.DnnPlugins:
                        //This method maybe called when Page.Form hasn't initialized yet, in that situation if needed should reference dnn js manually.
                        //such as call jQuery.RegisterDnnJQueryPlugins in Control.OnInit.
                        if (page.Form != null)
                        {
                            ClientAPI.RegisterClientReference(page, ClientAPI.ClientNamespaceReferences.dnn);
                        }

                        //register dependency

                        if (GetHighestVersionLibrary(CommonJs.jQuery) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryScriptReference(),
                                FileOrder.Js.jQuery, "DnnPageHeaderProvider");
                        }

                        if (GetHighestVersionLibrary(CommonJs.jQueryMigrate) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryMigrateScriptReference(),
                                FileOrder.Js.jQueryMigrate, "DnnPageHeaderProvider");
                        }


                        //actual jqueryui
                        if (GetHighestVersionLibrary(CommonJs.jQueryUI) == null)
                        {
                            ClientResourceManager.RegisterScript(page, jQuery.GetJQueryUIScriptReference(),
                                FileOrder.Js.jQueryUI, "DnnPageHeaderProvider");
                        }

                        if (GetHighestVersionLibrary(CommonJs.HoverIntent) == null)
                        {
                            ClientResourceManager.RegisterScript(page,
                                "~/Resources/Shared/Scripts/jquery/jquery.hoverIntent.min.js", FileOrder.Js.HoverIntent);
                        }
                        //no package for this - CRM will deduplicate
                        ClientResourceManager.RegisterScript(page, "~/Resources/Shared/Scripts/dnn.jquery.js");
                        break;
                    case CommonJs.jQueryFileUpload:
                        ClientResourceManager.RegisterScript(page,
                            "~/Resources/Shared/Scripts/jquery/jquery.iframe-transport.js");
                        ClientResourceManager.RegisterScript(page,
                            "~/Resources/Shared/Scripts/jquery/jquery.fileupload.js");
                        break;
                    case CommonJs.HoverIntent:
                        if (GetHighestVersionLibrary(CommonJs.HoverIntent) == null)
                        {
                            ClientResourceManager.RegisterScript(page,
                                "~/Resources/Shared/Scripts/jquery/jquery.hoverIntent.min.js", FileOrder.Js.HoverIntent);
                        }
                        break;
                }
            }
        }

        #endregion

        #region Legacy methods and preinstall support

        private const string jQueryUIDebugFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.js";
        private const string jQueryUIMinFile = "~/Resources/Shared/Scripts/jquery/jquery-ui.min.js";


        public static string JQueryUIFile(bool getMinFile)
        {
            string jfile = jQueryUIDebugFile;
            if (getMinFile)
            {
                jfile = jQueryUIMinFile;
            }
            return jfile;
        }

        public static string GetJQueryScriptReference()
        {
            string scriptsrc = jQuery.HostedUrl;
            if (!jQuery.UseHostedScript)
            {
                scriptsrc = jQuery.JQueryFile(!jQuery.UseDebugScript);
            }
            return scriptsrc;
        }

        #endregion
    }
}