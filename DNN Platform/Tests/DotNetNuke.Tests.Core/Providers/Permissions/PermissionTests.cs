using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Permissions
{
    [TestFixture]
    public class PermissionTests
    {
        private const int UserId = 400;

        [Test]
        public void PortalSecurity_IsInRoles_Super_User_Is_Always_True()
        {
            var user = new UserInfo() { IsSuperUser = true, };
            const string roles = "";
            var portalSettings = SetupPortalSettings();

            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_All_Users_Role_Is_Always_True()
        {
            var user = new UserInfo() { IsSuperUser = false, };
            const string roles = Globals.glbRoleAllUsersName;
            var portalSettings = SetupPortalSettings();

            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdmin_IsInRole_Is_True()
        {
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            var mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { new UserRoleInfo() { RoleName = "SomeRoleName", Status = RoleStatus.Approved } });
            const string roles = "SomeRoleName";
            var portalSettings = SetupPortalSettings();
            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }


        [Test]
        public void PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False()
        {
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            const string roles = "!SomeRoleName";
            var portalSettings = SetupPortalSettings();
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void ModulePermissionController_HasModuleAccess_Super_User_Is_Always_True()
        {
            CreateUser(true, new List<string>() { "SomeRoleName" });
            Assert.IsTrue(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        }


        [Test]
        public void ModulePermissionController_HasModuleAccess_SecurityLevelAnonymous_Is_Always_True()
        {
            CreateUser(false, new List<string>() { "SomeRoleName" });
            Assert.IsTrue(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        }

        [Test]
        public void ModulePermissionController_HasModuleAccess_SecurityLevelHost_Is_False_For_Non_SuperUser()
        {
            CreateUser(false, new List<string>() { "SomeRoleName" });
            Assert.IsFalse(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.Host, "", new ModuleInfo()));
        }

        [Test]
        public void ModulePermissionController_HasModuleAccess_SecurityLevelView_Is_False_For_User_Without_View_Role()
        {
            CreateUser(false, new List<string>() { "RoleWithoutViewPermission" });
            var modulePermissionCollection = new ModulePermissionCollection();
            AddModulePermission(modulePermissionCollection, "View", Convert.ToInt32(SetupPortalSettings().AdministratorRoleId));
            var module = new ModuleInfo {InheritViewPermissions = false, ModulePermissions = modulePermissionCollection};

            Assert.IsFalse(ModulePermissionController.HasModuleAccess(SecurityAccessLevel.View, "", module));
        }

        private static PortalSettings SetupPortalSettings()
        {
            var mockPortalController = new Mock<IPortalController>();
            var portalSettings = new PortalSettings { PortalId = 0, AdministratorId = 1 };
            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(portalSettings);
            TestablePortalController.SetTestableInstance(mockPortalController.Object);
            return portalSettings;
        }

        private static void CreateUser(bool isSuperUser, IEnumerable<string> Roles)
        {
            var user = new UserInfo { IsSuperUser = isSuperUser, UserID = UserId };
            var mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            var userRoles = new List<UserRoleInfo>();
            foreach (var role in Roles)
            {
                userRoles.Add(new UserRoleInfo() { RoleName = role, Status = RoleStatus.Approved });
            }
            mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(userRoles);
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator();
            simulator.SimulateRequest();
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            HttpContextSource.RegisterInstance(httpContextBase);
            HttpContext.Current.Items["UserInfo"] = user;
        }

        private static void AddModulePermission(ModulePermissionCollection permissions, string key, int roleId)
        {
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", key)[0];
            var modulePermission = new ModulePermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };
            permissions.Add(modulePermission);
        }

    }
}
