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
    public class DeleteUserUnitTests : CommandTests<DeleteUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName => "Delete-User";

        [Test]
        public void Run_DeleteValidUserId_ReturnSuccessResponse()
        {
            // Arrange
            int userId = 2;

            UserInfo userInfo = this.GetUser(userId, false);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            this._userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(true);
            this._userControllerWrapperMock
                .Setup(u => u.GetUserById(this.testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_DeleteAlreadyDeletedUser_ReturnErrorResponse()
        {
            // Arrange
            int userId = 2;

            UserInfo userInfo = this.GetUser(userId, true);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteUserFailed_ReturnErrorResponse()
        {
            // Arrange
            int userId = 2;

            UserInfo userInfo = this.GetUser(userId, false);

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            this._userControllerWrapperMock
                .Setup(u => u.DeleteUserAndClearCache(ref userInfo, false, false))
                .Returns(false);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_DeleteNullUserId_ReturnErrorResponse()
        {
            // Arrange
            UserInfo userinfo;
            this.errorResultModel = new ConsoleErrorResultModel();
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        protected override DeleteUser CreateCommand()
        {
            return new DeleteUser(this._userValidatorMock.Object, this._userControllerWrapperMock.Object);
        }

        protected override void ChildSetup()
        {
            this._userValidatorMock = new Mock<IUserValidator>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }
    }
}
