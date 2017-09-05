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

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class ModuleServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleServiceController));
        public class MoveModuleDTO
        {
            public int ModuleId { get; set; }
            public int ModuleOrder { get; set; }
            public string Pane { get; set; }
            public int TabId { get; set; }
        }

        public class DeleteModuleDto
        {
            public int ModuleId { get; set; }
            public int TabId { get; set; }
            public bool SoftDelete { get; set; }
        }

        [HttpGet]
        [DnnAuthorize(StaticRoles = "Registered Users")]
        public HttpResponseMessage GetModuleShareable(int moduleId, int tabId, int portalId = -1)
        {
            var requiresWarning = false;
            if (portalId <= -1)
            {
                var portalDict = PortalController.GetPortalDictionary();
                portalId = portalDict[tabId];
            }
            else
            {
                portalId = FixPortalId(portalId);
            }

            DesktopModuleInfo desktopModule;
            if (tabId < 0)
            {
                desktopModule = DesktopModuleController.GetDesktopModule(moduleId, portalId);
            }
            else
            {
                var moduleInfo = ModuleController.Instance.GetModule(moduleId, tabId, false);

                desktopModule = moduleInfo.DesktopModule;

                requiresWarning = moduleInfo.PortalID != PortalSettings.PortalId && desktopModule.Shareable == ModuleSharing.Unknown;
            }

            if (desktopModule == null)
            {
                var message = string.Format("Cannot find module ID {0} (tab ID {1}, portal ID {2})", moduleId, tabId, portalId);
                Logger.Error(message);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new { Shareable = desktopModule.Shareable.ToString(), RequiresWarning = requiresWarning });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage MoveModule(MoveModuleDTO postData)
        {
            var moduleOrder = postData.ModuleOrder;
            if (moduleOrder > 0)
            {
                //DNN-7099: the deleted modules won't show in page, so when the module index calculated from client, it will lost the 
                //index count of deleted modules and will cause order issue.
                var deletedModules = ModuleController.Instance.GetTabModules(postData.TabId).Values.Where(m => m.IsDeleted);
                foreach (var module in deletedModules)
                {
                    if (module.ModuleOrder < moduleOrder && module.PaneName == postData.Pane)
                    {
                        moduleOrder += 2;
                    }
                }
            }
            ModuleController.Instance.UpdateModuleOrder(postData.TabId, postData.ModuleId, moduleOrder, postData.Pane);
            ModuleController.Instance.UpdateTabModuleOrder(postData.TabId);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Web method that deletes a tab module.
        /// </summary>
        /// <remarks>This has been introduced for integration testing purpuses.</remarks>
        /// <param name="deleteModuleDto">delete module dto</param>
        /// <returns>Http response message</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage DeleteModule(DeleteModuleDto deleteModuleDto)
        {
            ModuleController.Instance.DeleteTabModule(deleteModuleDto.TabId, deleteModuleDto.ModuleId, deleteModuleDto.SoftDelete);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private int FixPortalId(int portalId)
        {
            return UserInfo.IsSuperUser && PortalSettings.PortalId != portalId && PortalController.Instance.GetPortals()
                       .Cast<PortalInfo>().Any(x => x.PortalID == portalId)
                ? portalId
                : PortalSettings.PortalId;
        }
    }
}