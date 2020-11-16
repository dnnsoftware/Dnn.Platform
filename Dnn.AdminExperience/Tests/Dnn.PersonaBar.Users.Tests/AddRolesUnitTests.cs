// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using System;
    using System.Collections.Generic;

    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Roles.Components;
    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Prompt.Commands;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AddRolesUnitTests : CommandTests<AddRoles>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUsersController> _usersControllerMock;
        private Mock<IRolesController> _rolesControllerMock;

        protected override string CommandName
        {
            get { return "Add-roles"; }
        }

        [TestCase]
        public void Run_AddRolesWithValidArguments_ReturnSuccessResponse()
        {
            // Arrange
            var userId = 2;
            var userInfo = this.GetUser(userId, true);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true
                        },
                    });

            var total = 1;
            this._usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester"
                        },
                    });

            this._rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, It.IsAny<int>(), It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            var result = this.RunCommand(userId.ToString(), "--roles", "Tester");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [TestCase]
        public void Run_AddRolesWhenUserNotValid_ReturnErrorResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userInfo = null;

            this.errorResultModel = new ConsoleErrorResultModel("Invalid userId");

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [TestCase]
        public void Run_AddRolesWhenRoleNotValid_ThrowsException()
        {
            // Arrange
            var userId = 2;
            var userInfo = this.GetUser(userId, true);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true
                        },
                    });

            var total = 1;
            this._usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester"
                        },
                    });

            this._rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, -1, It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            TestDelegate ex = () => this.RunCommand(userId.ToString(), "--roles", "Not Tester");

            // Assert
            Assert.Throws<Exception>(ex, "Should throw exception");
        }

        protected override void ChildSetup()
        {
            this._userValidatorMock = new Mock<IUserValidator>();
            this._usersControllerMock = new Mock<IUsersController>();
            this._rolesControllerMock = new Mock<IRolesController>();
        }

        protected override AddRoles CreateCommand()
        {
            return new AddRoles(this._userValidatorMock.Object, this._usersControllerMock.Object, this._rolesControllerMock.Object);
        }
    }
}
