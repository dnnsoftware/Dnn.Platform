#region Copyright

// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Dnn.EditBar.UI.Controllers;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer.Packages;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using DotNetNuke.Web.InternalServices;

namespace Dnn.EditBar.UI.Services
{
    [DnnAuthorize]
    [DnnPageEditor]
    public class ContentEditorController : DnnApiController
    {
        #region Fields

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentEditorController));

        private const string DefaultExtensionImage = "icon_extensions_32px.png";

        #endregion

        #region Properties

        private string LocalResourcesFile
        {
            get { return Path.Combine(ContentEditorManager.ControlFolder, "ContentEditorManager/App_LocalResources/SharedResources.resx"); }
        }

        #endregion

        #region API

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteModule([FromUri]int moduleId)
        {
            var module = ModuleController.Instance.GetModule(moduleId, PortalSettings.ActiveTab.TabID, false);
            if (module == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Status = 1, Message = LocalizeString("Service_ModuleNotExist") });
            }

            var tabId = PortalSettings.ActiveTab.TabID;
            ModuleController.Instance.DeleteTabModule(tabId, moduleId, false);

            //remove related modules
            ModuleController.Instance.GetTabModules(tabId).Values
                .Where(m => m.ModuleID > moduleId)
                .ForEach(m =>
                {
                    ModuleController.Instance.DeleteTabModule(tabId, m.ModuleID, false);
                });

            return Request.CreateResponse(HttpStatusCode.OK, new { Status = 0 });
        }

        [HttpGet]
        public HttpResponseMessage GetRecommendedModules()
        {
            var recommendedModuleNames = new List<string> ();
            var filteredList = DesktopModuleController.GetPortalDesktopModules(PortalSettings.PortalId)
                                        .Where(kvp => kvp.Value.DesktopModule.Category == "Recommended");

            var result = filteredList.Select(kvp => new ControlBarController.ModuleDefDTO
            {
                ModuleID = kvp.Value.DesktopModuleID,
                ModuleName = kvp.Key,
                ModuleImage = GetDeskTopModuleImage(kvp.Value.DesktopModuleID),
                Bookmarked = true,
                ExistsInBookmarkCategory = true
            }).ToList();

            foreach (var moduleName in recommendedModuleNames)
            {
                if (result.All(t => t.ModuleName != moduleName))
                {
                    result.Add(new ControlBarController.ModuleDefDTO
                    {
                        ModuleID = Null.NullInteger,
                        ModuleName = moduleName,
                        ModuleImage = GetDeskTopModuleImage(Null.NullInteger),
                        Bookmarked = true,
                        ExistsInBookmarkCategory = true
                    });
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, result.OrderBy(m => recommendedModuleNames.IndexOf(m.ModuleName)));
        }


        [HttpGet]
        public HttpResponseMessage LoadModuleScript(int desktopModuleId)
        {
            var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
            if (desktopModule == null)
            {
                throw new ArgumentException("Can't find the desktop module");
            }

            var moduleScriptPath = string.Format("{0}/DesktopModules/{1}/ClientScripts/ModuleEditor.js", Globals.ApplicationMapPath, desktopModule.FolderName);
            var moduleScriptContent = string.Empty;
            if (File.Exists(moduleScriptPath))
            {
                moduleScriptContent = File.ReadAllText(moduleScriptPath);
            }

            var moduleStylePath = string.Format("/DesktopModules/{0}/Css/ModuleEditor.css", desktopModule.FolderName);
            if (File.Exists(Globals.ApplicationMapPath + moduleStylePath))
            {
                moduleStylePath = Globals.ApplicationPath + moduleStylePath;
            }
            else
            {
                moduleStylePath = string.Empty;
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { Script = moduleScriptContent, StyleFile = moduleStylePath });
        }

        #endregion

        #region Private Methods

        private string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourcesFile);
        }

        private string GetDeskTopModuleImage(int moduleId)
        {
            var portalDesktopModules = DesktopModuleController.GetDesktopModules(PortalSettings.PortalId);
            var packages = PackageController.Instance.GetExtensionPackages(PortalSettings.PortalId);

            string imageUrl =
                    (from pkgs in packages
                     join portMods in portalDesktopModules on pkgs.PackageID equals portMods.Value.PackageID
                     where portMods.Value.DesktopModuleID == moduleId
                     select pkgs.IconFile).FirstOrDefault();

            imageUrl = String.IsNullOrEmpty(imageUrl) ? Globals.ImagePath + DefaultExtensionImage : imageUrl;
            return System.Web.VirtualPathUtility.ToAbsolute(imageUrl);
        }

        #endregion
    }

}