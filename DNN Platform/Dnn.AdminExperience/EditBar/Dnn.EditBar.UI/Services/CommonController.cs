using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Dnn.EditBar.UI.Helpers;
using Dnn.EditBar.UI.Services.DTO;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;

namespace Dnn.EditBar.UI.Services
{
    [DnnPageEditor]
    public class CommonController : DnnApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage CheckAuthorized()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { success = IsPageEditor() });
        }

        [HttpGet]
        [DnnPageEditor]
        public HttpResponseMessage GetUserSetting(string key)
        {
            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(UserInfo.UserID, PortalSettings.PortalId);
            var value = personalization.Profile[key + PortalSettings.PortalId];

            if (value == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Value = false });
            }

            var userSetting = new UserSetting
            {
                Key = key,
                Value = value
            };

            return Request.CreateResponse(HttpStatusCode.OK, userSetting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnPageEditor]
        public HttpResponseMessage SetUserSetting(UserSetting setting)
        {
            var personalizationController = new DotNetNuke.Services.Personalization.PersonalizationController();
            var personalization = personalizationController.LoadProfile(UserInfo.UserID, PortalSettings.PortalId);
            personalization.Profile[setting.Key + PortalSettings.PortalId] = setting.Value;
            personalization.IsModified = true;
            personalizationController.SaveProfile(personalization);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private bool IsPageEditor()
        {
            return PagePermissionsAttributesHelper.HasTabPermission("EDIT,CONTENT,MANAGE") || IsModuleAdmin(PortalSettings);
        }

        private bool IsModuleAdmin(PortalSettings portalSettings)
        {
            bool isModuleAdmin = false;
            foreach (ModuleInfo objModule in TabController.CurrentPage.Modules)
            {
                if (!objModule.IsDeleted)
                {
                    bool blnHasModuleEditPermissions = ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Edit, Null.NullString, objModule);
                    if (blnHasModuleEditPermissions)
                    {
                        isModuleAdmin = true;
                        break;
                    }
                }
            }
            return portalSettings.ControlPanelSecurity == PortalSettings.ControlPanelPermission.ModuleEditor && isModuleAdmin;
        }

    }
}
