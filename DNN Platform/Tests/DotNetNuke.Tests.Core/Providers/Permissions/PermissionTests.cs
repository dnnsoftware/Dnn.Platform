// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Providers.Permissions
{
    using System.Collections.Generic;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PermissionTests : DnnUnitTest
    {
        private const int UserId = 400;

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
            RoleController.ClearInstance();
            RelationshipController.ClearInstance();
            UserController.ClearInstance();
        }

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

            var mockRoleController = new Mock<IRoleController>();
            mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { new UserRoleInfo() { RoleName = "SomeRoleName", Status = RoleStatus.Approved } });
            RoleController.SetTestableInstance(mockRoleController.Object);

            const string roles = "SomeRoleName";
            var portalSettings = SetupPortalSettings();
            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFollowerRoleAndRoleEntityIsFollowedByUser()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = RelationshipStatus.Accepted });
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRoleEntityIsNotFollowedByUser()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(() => null);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        [TestCase(RelationshipStatus.None)]
        [TestCase(RelationshipStatus.Pending)]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFollowerRoleAndRelationshipIsNotAccepted(RelationshipStatus relationshipStatus)
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FOLLOWER:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFollowerRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = relationshipStatus });
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsFriendRoleAndRoleEntityIsFriend()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = RelationshipStatus.Accepted });
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRoleEntityIsNotFriend()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(() => null);
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        [TestCase(RelationshipStatus.None)]
        [TestCase(RelationshipStatus.Pending)]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsFriendRoleAndRelationshipIsNotAccepted(RelationshipStatus relationshipStatus)
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = Constants.USER_TenId };
            var relatedUser = new UserInfo { IsSuperUser = false, UserID = Constants.USER_ValidId };
            string roles = "FRIEND:" + relatedUser.UserID;

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(uc => uc.GetUserById(It.IsAny<int>(), Constants.USER_ValidId)).Returns(relatedUser);
            UserController.SetTestableInstance(mockUserController.Object);

            var mockRelationShipController = new Mock<IRelationshipController>();
            mockRelationShipController.Setup(
                rsc =>
                    rsc.GetFriendRelationship(It.Is<UserInfo>(u => u.UserID == Constants.USER_TenId), It.Is<UserInfo>(u => u.UserID == Constants.USER_ValidId)))
                        .Returns(new UserRelationship() { Status = relationshipStatus });
            RelationshipController.SetTestableInstance(mockRelationShipController.Object);

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_True_WhenRoleIsOwnerRoleAndRoleEntityIsUser()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            string roles = "OWNER:" + UserId;

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsTrue(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdminUser_False_WhenRoleIsOwnerRoleAndRoleEntityIsNotUser()
        {
            // Arrange
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };
            string roles = "OWNER:" + UserId + 1;

            var portalSettings = SetupPortalSettings();

            // Act and Assert
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        [Test]
        public void PortalSecurity_IsInRoles_NonAdmin_In_Deny_Role_Is_False()
        {
            var user = new UserInfo { IsSuperUser = false, UserID = UserId };

            var mockRoleController = new Mock<IRoleController>();
            mockRoleController.Setup(rc => rc.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { new UserRoleInfo() { RoleName = "SomeRoleName", Status = RoleStatus.Approved } });
            RoleController.SetTestableInstance(mockRoleController.Object);

            const string roles = "!SomeRoleName";
            var portalSettings = SetupPortalSettings();
            Assert.IsFalse(PortalSecurity.IsInRoles(user, portalSettings, roles));
        }

        // [Test]
        // public void CorePermissionProvider_HasModuleAccess_Super_User_Is_Always_True()
        // {
        //    CreateUser(true, new List<string>() { "SomeRoleName" });

        // var permissionProvider = new CorePermissionProvider();
        //    Assert.IsTrue(permissionProvider.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        // }

        // [Test]
        // public void CorePermissionProvider_HasModuleAccess_SecurityLevelAnonymous_Is_Always_True()
        // {
        //    CreateUser(false, new List<string>() { "SomeRoleName" });

        // var permissionProvider = new CorePermissionProvider();
        //    Assert.IsTrue(permissionProvider.HasModuleAccess(SecurityAccessLevel.Anonymous, "", new ModuleInfo()));
        // }

        // [Test]
        // public void CorePermissionProvider_HasModuleAccess_SecurityLevelHost_Is_False_For_Non_SuperUser()
        // {
        //    CreateUser(false, new List<string>() { "SomeRoleName" });

        // var permissionProvider = new CorePermissionProvider();
        //    Assert.IsFalse(permissionProvider.HasModuleAccess(SecurityAccessLevel.Host, "", new ModuleInfo()));
        // }

        // [Test]
        // public void CorePermissionProvider_HasModuleAccess_SecurityLevelView_Is_False_For_User_Without_View_Role()
        // {
        //    CreateUser(false, new List<string>() { "RoleWithoutViewPermission" });
        //    var modulePermissionCollection = new ModulePermissionCollection();
        //    AddModulePermission(modulePermissionCollection, "View", Convert.ToInt32(SetupPortalSettings().AdministratorRoleId));
        //    var module = new ModuleInfo {InheritViewPermissions = false, ModulePermissions = modulePermissionCollection};

        // var permissionProvider = new CorePermissionProvider();
        //    Assert.IsFalse(permissionProvider.HasModuleAccess(SecurityAccessLevel.View, "", module));
        // }
        private static PortalSettings SetupPortalSettings()
        {
            var mockPortalController = new Mock<IPortalController>();
            var portalSettings = new PortalSettings { PortalId = 0, AdministratorId = 1 };
            mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(portalSettings);
            PortalController.SetTestableInstance(mockPortalController.Object);
            return portalSettings;
        }

        private static void AddModulePermission(ModulePermissionCollection permissions, string key, int roleId)
        {
            var permissionController = new PermissionController();
            var permission = (PermissionInfo)permissionController.GetPermissionByCodeAndKey("SYSTEM_MODULE_DEFINITION", key)[0];
            var modulePermission = new ModulePermissionInfo { PermissionID = permission.PermissionID, RoleID = roleId, AllowAccess = true };
            permissions.Add(modulePermission);
        }

        private void CreateUser(bool isSuperUser, IEnumerable<string> Roles)
        {
            var user = new UserInfo { IsSuperUser = isSuperUser, UserID = UserId };
            var mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            var userRoles = new List<UserRoleInfo>();
            foreach (var role in Roles)
            {
                userRoles.Add(new UserRoleInfo() { RoleName = role, Status = RoleStatus.Approved });
            }

            mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == UserId), It.IsAny<bool>())).Returns(userRoles);
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator(this.WebsitePhysicalAppPath);
            simulator.SimulateRequest();
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            HttpContextSource.RegisterInstance(httpContextBase);
            HttpContext.Current.Items["UserInfo"] = user;
        }
    }
}
