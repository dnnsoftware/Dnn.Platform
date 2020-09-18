// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Tests
{
    using Dnn.PersonaBar.Library.Prompt.Models;
    using Dnn.PersonaBar.Recyclebin.Components;
    using Dnn.PersonaBar.Recyclebin.Components.Prompt.Commands;
    using Dnn.PersonaBar.Users.Components;
    using DotNetNuke.Entities.Users;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class RestoreUserUnitTests : CommandTests<RestoreUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IRecyclebinController> _recyclebinControllerMock;

        protected override string CommandName
        {
            get { return "Restore-User"; }
        }

        [Test]
        public void Run_RestoreValidUserId_ReturnSuccessResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userInfo = this.GetUser(userId, true);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(true);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsFalse(result.IsError);
        }

        [Test]
        public void Run_RecycleBinControllerRestoringError_ReturnErrorResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userInfo = this.GetUser(userId, true);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            var message = string.Empty;
            this._recyclebinControllerMock.Setup(r => r.RestoreUser(userInfo, out message)).Returns(false);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_RestoreNotDeletedUser_ReturnErrorResponse()
        {
            // Arrange
            var userId = 2;
            UserInfo userinfo = this.GetUser(userId, false);
            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand(userId.ToString());

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [Test]
        public void Run_RestoreNullUserId_ReturnErrorResponse()
        {
            // Arrange
            this.errorResultModel = new ConsoleErrorResultModel();

            UserInfo userinfo;
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userinfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand();

            // Assert
            Assert.IsTrue(result.IsError);
        }

        protected override RestoreUser CreateCommand()
        {
            return new RestoreUser(this._userValidatorMock.Object, this._recyclebinControllerMock.Object);
        }

        [SetUp]
        protected override void ChildSetup()
        {
            this._userValidatorMock = new Mock<IUserValidator>();
            this._recyclebinControllerMock = new Mock<IRecyclebinController>();
        }
    }
}
