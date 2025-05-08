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
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class AddRolesUnitTests : CommandTests<AddRoles>
    {
        private Mock<IUserValidator> userValidatorMock;
        private Mock<IUsersController> usersControllerMock;
        private Mock<IRolesController> rolesControllerMock;
        private FakeServiceProvider serviceProvider;

        protected override string CommandName => "Add-roles";

        [TestCase]
        public void Run_AddRolesWithValidArguments_ReturnSuccessResponse()
        {
            // Arrange
            var userId = 2;
            var userInfo = this.GetUser(userId, true);

            this.userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true,
                        },
                    });

            var total = 1;
            this.usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester",
                        },
                    });

            this.rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, It.IsAny<int>(), It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            var result = this.RunCommand(userId.ToString(), "--roles", "Tester");

            // Assert
            Assert.That(result.IsError, Is.False);
        }

        [TestCase]
        public void Run_AddRolesWhenUserNotValid_ReturnErrorResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userInfo = null;

            this.errorResultModel = new ConsoleErrorResultModel("Invalid userId");

            this.userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.That(result.IsError, Is.True);
        }

        [TestCase]
        public void Run_AddRolesWhenRoleNotValid_ThrowsException()
        {
            // Arrange
            var userId = 2;
            var userInfo = this.GetUser(userId, true);

            this.userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            var userInfoList = new List<UserRoleInfo>(
                    new[]
                    {
                        new UserRoleInfo
                        {
                            RoleID = 1,
                            PortalID = this.testPortalId,
                            IsPublic = true,
                        },
                    });

            var total = 1;
            this.usersControllerMock
                .Setup(u => u.GetUserRoles(userInfo, string.Empty, out total, -1, -1))
                .Returns(userInfoList);

            var rolesList = new List<RoleInfo>(
                    new[]
                    {
                        new RoleInfo
                        {
                            RoleID = 1,
                            RoleName = "Tester",
                        },
                    });

            this.rolesControllerMock
                .Setup(r => r.GetRolesByNames(this.portalSettings, -1, It.IsAny<IList<string>>()))
                .Returns(rolesList);

            // Act
            TestDelegate ex = () => this.RunCommand(userId.ToString(), "--roles", "Not Tester");

            // Assert
            Assert.Throws<Exception>(ex, "Should throw exception");
        }

        protected override void ChildSetup()
        {
            this.userValidatorMock = new Mock<IUserValidator>();
            this.usersControllerMock = new Mock<IUsersController>();
            this.rolesControllerMock = new Mock<IRolesController>();
            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.userValidatorMock.Object);
                    services.AddSingleton(this.usersControllerMock.Object);
                    services.AddSingleton(this.rolesControllerMock.Object);
                });
        }

        [TearDown]
        protected override void ChildTearDown()
        {
            this.serviceProvider.Dispose();
        }

        protected override AddRoles CreateCommand()
        {
            return new AddRoles(this.userValidatorMock.Object, this.usersControllerMock.Object, this.rolesControllerMock.Object);
        }
    }
}
