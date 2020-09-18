// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Users.Components;
    using Dnn.PersonaBar.Users.Components.Prompt.Commands;
    using DotNetNuke.Entities.Users;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class GetUserUnitTests : CommandTests<GetUser>
    {
        private int _userId;
        private UserInfo _userInfo;

        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName
        {
            get { return "Get-User"; }
        }

        [Test]
        public void Run_GetUserByEmailWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;

            this._userControllerWrapperMock
                .Setup(c => c.GetUsersByEmail(
                    this.testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(this._userId);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--email", "user1@g.com");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [TestCase]
        public void Run_GetUserByUserNameWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            var recCount = 0;

            this._userControllerWrapperMock
                .Setup(c => c.GetUsersByUserName(
                    this.testPortalId,
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    ref recCount,
                    It.IsAny<bool>(),
                    It.IsAny<bool>()))
                .Returns(this._userId);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--username", "user1");

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldSuccessResponse()
        {
            // Arrange
            this._userValidatorMock
             .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
             .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_GetUserWithValidCommand_ShouldErrorResponse()
        {
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            this._userValidatorMock
                .Setup(u => u.ValidateUser(this._userId, this.portalSettings, null, out this._userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(this._userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        protected override void ChildSetup()
        {
            this._userId = 3;
            this._userInfo = this.GetUser(this._userId, false);

            this._userValidatorMock = new Mock<IUserValidator>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }

        protected override GetUser CreateCommand()
        {
            return new GetUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }
    }
}
