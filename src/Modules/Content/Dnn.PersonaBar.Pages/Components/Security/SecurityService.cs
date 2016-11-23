using System;
using Dnn.PersonaBar.Library.Model;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Security.Permissions;
using Newtonsoft.Json.Linq;

namespace Dnn.PersonaBar.Pages.Components.Security
{
    public class SecurityService : ServiceLocator<ISecurityService, SecurityService>, ISecurityService
    {

        public virtual bool IsVisible(MenuItem menuItem)
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            var isSuperUser = user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
            if (isSuperUser)
            {
                return true;
            }

            return IsPageAdmin();
        }

        private bool IsPageAdmin()
        {
            return //TabPermissionController.CanAddContentToPage() ||
                    TabPermissionController.CanAddPage()
                    || TabPermissionController.CanAdminPage()
                    || TabPermissionController.CanCopyPage()
                    || TabPermissionController.CanDeletePage()
                    || TabPermissionController.CanExportPage()
                    || TabPermissionController.CanImportPage()
                    || TabPermissionController.CanManagePage();
        }

        public virtual JObject GetCurrentPagePermissions()
        {
            var permissions = new JObject
            {
                {"addContentToPage", TabPermissionController.CanAddContentToPage()},
                {"addPage", TabPermissionController.CanAddPage()},
                {"adminPage", TabPermissionController.CanAdminPage()},
                {"copyPage", TabPermissionController.CanCopyPage()},
                {"deletePage", TabPermissionController.CanDeletePage()},
                {"exportPage", TabPermissionController.CanExportPage()},
                {"importPage", TabPermissionController.CanImportPage()},
                {"managePage", TabPermissionController.CanManagePage()}
            };

            return permissions;
        }

        public virtual bool IsPageAdminUser()
        {
            var user = UserController.Instance.GetCurrentUserInfo();
            return user.IsSuperUser || user.IsInRole(PortalSettings.Current?.AdministratorRoleName);
        }

        protected override Func<ISecurityService> GetFactory()
        {
            return () => new SecurityService();
        }

        public virtual bool IsUserAllowed(string permission)
        {
            return IsPageAdminUser() || (bool) GetCurrentPagePermissions().GetValue(permission);
        }

        public virtual bool CanManagePage(TabInfo tab)
        {
            return IsPageAdminUser() || tab == null || TabPermissionController.CanManagePage(tab);
        }

        public virtual bool CanAdminPage(TabInfo tab)
        {
            return IsPageAdminUser() || tab == null || TabPermissionController.CanAdminPage(tab);
        }

        public virtual bool CanDeletePage(TabInfo tab)
        {
            return IsPageAdminUser() || tab == null || TabPermissionController.CanDeletePage(tab);
        }

    }
}