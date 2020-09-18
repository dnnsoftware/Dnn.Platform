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
    public class SetUserUnitTests : CommandTests<SetUser>
    {
        private Mock<IUserValidator> _userValidatorMock;
        private Mock<IUsersController> _usersControllerMock;
        private Mock<IUserControllerWrapper> _userControllerWrapperMock;

        protected override string CommandName => "Set-User";

        [Test]
        public void Run_UserIdNull_ReturnErrorResponse()
        {
            // Arrange
            UserInfo userInfo;
            this.errorResultModel = new ConsoleErrorResultModel();
            this._userValidatorMock
                .Setup(u => u.ValidateUser(-1, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);

            // Act
            var result = this.RunCommand("--username", "testusername", "--firstname", "testfirstname", "--lastname", "testlastname");

            // Assert
            Assert.IsTrue(result.IsError);
        }

        [TestCase("--email", "user@gm.com")]
        [TestCase("--password", "pass1233")]
        [TestCase("--username", "user4pmt")]
        [TestCase("--displayname", "user4displayname")]
        [TestCase("--approved", "true")]
        public void Run_ValidCommand_ReturnSuccessResponse(string attributeName, string attributeValue)
        {
            // Arrange
            var userId = 4;
            var userInfo = this.GetUser(userId, false);
            userInfo.FirstName = "userFirstName";
            userInfo.LastName = "userLastName";
            userInfo.Email = "user@email.com";

            this._userValidatorMock
                .Setup(u => u.ValidateUser(userId, this.portalSettings, null, out userInfo))
                .Returns(this.errorResultModel);
            this._userControllerWrapperMock
                .Setup(w => w.GetUserById(this.testPortalId, userId))
                .Returns(userInfo);

            // Act
            var result = this.RunCommand(userId.ToString(), "--firstname", "user4", "--lastname", "user4", attributeName, attributeValue);

            // Assert
            Assert.IsFalse(result.IsError);
            Assert.AreEqual(1, result.Records);
        }

        protected override void ChildSetup()
        {
            this._userValidatorMock = new Mock<IUserValidator>();
            this._usersControllerMock = new Mock<IUsersController>();
            this._userControllerWrapperMock = new Mock<IUserControllerWrapper>();
        }

        protected override SetUser CreateCommand()
        {
            return new SetUser(this._userValidatorMock.Object, this._usersControllerMock.Object, this._userControllerWrapperMock.Object);
        }
    }
}
