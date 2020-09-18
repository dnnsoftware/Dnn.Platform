// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
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

    [DnnAuthorize]
    public class ModuleServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModuleServiceController));

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
                portalId = this.FixPortalId(portalId);
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

                requiresWarning = moduleInfo.PortalID != this.PortalSettings.PortalId && desktopModule.Shareable == ModuleSharing.Unknown;
            }

            if (desktopModule == null)
            {
                var message = string.Format("Cannot find module ID {0} (tab ID {1}, portal ID {2})", moduleId, tabId, portalId);
                Logger.Error(message);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Shareable = desktopModule.Shareable.ToString(), RequiresWarning = requiresWarning });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage MoveModule(MoveModuleDTO postData)
        {
            var moduleOrder = postData.ModuleOrder;
            if (moduleOrder > 0)
            {
                // DNN-7099: the deleted modules won't show in page, so when the module index calculated from client, it will lost the
                // index count of deleted modules and will cause order issue.
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

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        /// <summary>
        /// Web method that deletes a tab module.
        /// </summary>
        /// <remarks>This has been introduced for integration testing purpuses.</remarks>
        /// <param name="deleteModuleDto">delete module dto.</param>
        /// <returns>Http response message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage DeleteModule(DeleteModuleDto deleteModuleDto)
        {
            ModuleController.Instance.DeleteTabModule(deleteModuleDto.TabId, deleteModuleDto.ModuleId, deleteModuleDto.SoftDelete);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private int FixPortalId(int portalId)
        {
            return this.UserInfo.IsSuperUser && this.PortalSettings.PortalId != portalId && PortalController.Instance.GetPortals()
                .OfType<PortalInfo>().Any(x => x.PortalID == portalId)
                ? portalId
                : this.PortalSettings.PortalId;
        }

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
    }
}
