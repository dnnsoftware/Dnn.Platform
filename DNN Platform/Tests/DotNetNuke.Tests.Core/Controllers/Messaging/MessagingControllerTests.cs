// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Data;
    using DotNetNuke.Services.Social.Messaging.Exceptions;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    /// <summary> Testing various aspects of MessagingController.</summary>
    [TestFixture]
    public class MessagingControllerTests
    {
        private Mock<IDataService> mockDataService;
        private MessagingController messagingController;
        private InternalMessagingControllerImpl internalMessagingController;
        private Mock<MessagingController> mockMessagingController;
        private Mock<InternalMessagingControllerImpl> mockInternalMessagingController;
        private Mock<DataProvider> dataProvider;
        private Mock<IPortalController> portalController;
        private Mock<RoleProvider> mockRoleProvider;
        private Mock<CachingProvider> mockCacheProvider;
        private Mock<ILocalizationProvider> mockLocalizationProvider;
        private Mock<IFolderManager> folderManager;
        private Mock<IFileManager> fileManager;
        private Mock<IFolderPermissionController> folderPermissionController;
        private FakeServiceProvider serviceProvider;

        private DataTable dtMessages;
        private DataTable dtMessageAttachment;
        private DataTable dtMessageRecipients;
        private DataTable dtPortalSettings;
        private DataTable dtMessageConversationView;
        private DataTable dtMessageThreadsView;

        private UserInfo adminUserInfo;
        private UserInfo hostUserInfo;
        private UserInfo user12UserInfo;
        private UserInfo groupOwnerUserInfo;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this.mockDataService = new Mock<IDataService>();
            this.dataProvider = MockComponentProvider.CreateDataProvider();
            this.mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            this.mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            this.mockLocalizationProvider = MockComponentProvider.CreateLocalizationProvider();
            this.mockLocalizationProvider.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns("{0}_{1}");

            this.messagingController = new MessagingController(this.mockDataService.Object);
            this.internalMessagingController = new InternalMessagingControllerImpl(this.mockDataService.Object);
            this.mockMessagingController = new Mock<MessagingController> { CallBase = true };
            this.mockInternalMessagingController = new Mock<InternalMessagingControllerImpl> { CallBase = true };

            this.portalController = new Mock<IPortalController>();
            this.portalController.Setup(c => c.GetPortalSettings(It.IsAny<int>())).Returns(new Dictionary<string, string>());
            PortalController.SetTestableInstance(this.portalController.Object);

            DataService.RegisterInstance(this.mockDataService.Object);

            this.folderManager = new Mock<IFolderManager>();
            this.fileManager = new Mock<IFileManager>();
            this.folderPermissionController = new Mock<IFolderPermissionController>();

            FolderManager.RegisterInstance(this.folderManager.Object);
            FileManager.RegisterInstance(this.fileManager.Object);
            FolderPermissionController.SetTestableInstance(this.folderPermissionController.Object);

            this.SetupDataProvider();
            this.SetupRoleProvider();
            this.SetupDataTables();
            this.SetupUsers();
            this.SetupPortalSettings();
            this.SetupCachingProvider();
            this.SetupFileControllers();

            this.mockInternalMessagingController.Setup(m => m.GetLastSentMessage(It.IsAny<UserInfo>())).Returns((Message)null);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockDataService.Object);
                    services.AddSingleton(this.dataProvider.Object);
                    services.AddSingleton(this.portalController.Object);
                    services.AddSingleton(this.mockRoleProvider.Object);
                    services.AddSingleton(this.mockCacheProvider.Object);
                    services.AddSingleton(this.mockLocalizationProvider.Object);
                    services.AddSingleton(this.folderManager.Object);
                    services.AddSingleton(this.fileManager.Object);
                    services.AddSingleton(this.folderPermissionController.Object);
                    services.AddSingleton<IRoleController>(new RoleController(this.mockRoleProvider.Object, Mock.Of<IHostSettings>(), Mock.Of<IEventLogger>(), this.portalController.Object, Mock.Of<IUserController>(), Mock.Of<IEventManager>(), this.fileManager.Object, this.folderManager.Object, this.dataProvider.Object));
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
            ComponentFactory.Container = null;
            PortalController.ClearInstance();
            this.dtMessages?.Dispose();
            this.dtMessageAttachment?.Dispose();
            this.dtMessageRecipients?.Dispose();
            this.dtPortalSettings?.Dispose();
            this.dtMessageConversationView?.Dispose();
            this.dtMessageThreadsView?.Dispose();
        }

        [Test]
        public void MessagingController_Constructor_Throws_On_Null_DataService()
        {
            // Arrange

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new MessagingController(null));
        }

        [Test]
        public void AttachmentsAllowed_Returns_True_When_MessagingAllowAttachments_Setting_Is_YES()
        {
            this.mockMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("YES");
            var result = this.mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.That(result, Is.True);
        }

        [Test]
        public void IncludeAttachments_Returns_True_When_MessagingIncludeAttachments_Setting_Is_YES()
        {
            this.mockMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("YES");
            var result = this.mockInternalMessagingController.Object.IncludeAttachments(Constants.CONTENT_ValidPortalId);
            Assert.That(result, Is.True);
        }

        [Test]
        public void AttachmentsAllowed_Returns_False_When_MessagingAllowAttachments_Setting_Is_Not_YES()
        {
            this.mockInternalMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("NO");
            var result = this.mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.That(result, Is.False);
        }

        [Test]
        public void GetArchivedMessages_Calls_DataService_GetArchiveBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(this.mockDataService.Object);

            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);

            this.dtMessageConversationView.Clear();

            this.mockDataService
                .Setup(ds => ds.GetArchiveBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this.dtMessageConversationView.CreateDataReader())
                .Verifiable();

            this.mockInternalMessagingController.Object.GetArchivedMessages(Constants.UserID_User12, 0, 0);

            this.mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetMessageThread_Calls_DataService_GetMessageThread()
        {
            var totalRecords = 0;

            DataService.RegisterInstance(this.mockDataService.Object);

            this.dtMessageThreadsView.Clear();

            this.mockDataService.Setup(ds => ds.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), ref totalRecords)).Returns(this.dtMessageThreadsView.CreateDataReader()).Verifiable();
            this.mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, string.Empty, false, ref totalRecords);
            this.mockDataService.Verify();
        }

        [Test]
        public void GetMessageThread_Calls_Overload_With_Default_Values()
        {
            // Arrange
            var totalRecords = 0;
            DataService.RegisterInstance(this.mockDataService.Object);
            this.dtMessageThreadsView.Clear();
            this.mockDataService.Setup(ds => ds.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), ref totalRecords)).Returns(this.dtMessageThreadsView.CreateDataReader()).Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, ref totalRecords);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void GetRecentSentbox_Calls_Overload_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.dtMessageConversationView.Clear();
            this.mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this.dtMessageConversationView.CreateDataReader())
                .Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);


            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void GetRecentSentbox_Calls_GetSentbox_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.dtMessageConversationView.Clear();
            this.mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this.dtMessageConversationView.CreateDataReader())
                .Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void GetSentbox_Calls_DataService_GetSentBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(this.mockDataService.Object);

            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);

            this.dtMessageConversationView.Clear();

            this.mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this.dtMessageConversationView.CreateDataReader())
                .Verifiable();

            this.mockInternalMessagingController.Object.GetSentbox(
                Constants.UserID_User12,
                0,
                0,
                string.Empty,
                false,
                MessageReadStatus.Any,
                MessageArchivedStatus.Any);

            this.mockDataService.Verify();
        }

        [Test]
        public void GetSentbox_Calls_Overload_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.dtMessageConversationView.Clear();
            this.mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this.dtMessageConversationView.CreateDataReader())
                .Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetSentbox(Constants.UserID_User12, 0, 0, string.Empty, false);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void RecipientLimit_Returns_MessagingRecipientLimit_Setting()
        {
            const int expected = 10;
            this.mockInternalMessagingController
                .Setup(mc => mc.GetPortalSettingAsInteger("MessagingRecipientLimit", Constants.CONTENT_ValidPortalId, It.IsAny<int>()))
                .Returns(expected);

            var actual = this.mockInternalMessagingController.Object.RecipientLimit(Constants.CONTENT_ValidPortalId);

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Null_Message()
        {
            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(null, new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Null_Body_And_Subject()
        {
            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(new Message(), new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Null_Roles_And_Users()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, null, null, null, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Empty_Roles_And_Users_Lists()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo>(), null, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Roles_And_Users_With_No_DisplayNames()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Large_Subject()
        {
            // Arrange
            var subject = new StringBuilder();
            for (var i = 0; i <= 40; i++)
            {
                subject.Append("1234567890");
            }

            // Arrange
            var message = new Message { Subject = subject.ToString(), Body = "body" };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Large_To()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>();

            for (var i = 0; i <= 100; i++)
            {
                roles.Add(new RoleInfo { RoleName = "1234567890" });
                users.Add(new UserInfo { DisplayName = "1234567890" });
            }

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, roles, users, null, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Null_Sender()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            // Act
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, null));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_Negative_SenderID()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var sender = new UserInfo { DisplayName = "user11" };

            // Act
            Assert.Throws<ArgumentException>(() => this.messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, sender));
        }

        [Test]
        public void MessagingController_CreateMessage_Throws_On_SendingToRole_ByNonAdmin()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            this.dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtMessageRecipients.CreateDataReader());

            // Act
            Assert.Throws<ArgumentException>(() => messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.user12UserInfo));
        }

        [Test]
        [Ignore("🤷")]
        public void MessagingController_CreateMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            // disable caching
            this.mockCacheProvider.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            this.dtPortalSettings.Clear();
            this.dtPortalSettings.Rows.Add(Constants.PORTALSETTING_MessagingAllowAttachments_Name, Constants.PORTALSETTING_MessagingAllowAttachments_Value_NO, Constants.CULTURE_EN_US);
            this.dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns(this.dtPortalSettings.CreateDataReader());

            this.dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtMessageRecipients.CreateDataReader());

            // Act
            Assert.Throws<AttachmentsNotAllowed>(() => messagingController.SendMessage(message, null, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.user12UserInfo));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessage_On_Valid_Message()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.SaveMessage(
                It.Is<Message>(v => v.PortalID == Constants.PORTAL_Zero && v.Subject == "subject"
                                                                             && v.Body == "body"
                                                                             && v.To == "role1,user1"
                                                                             && v.SenderUserID == this.adminUserInfo.UserID),
                It.IsAny<int>(),
                It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_Filters_Input_When_ProfanityFilter_Is_Enabled()
        {
            // Arrange
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var message = new Message { Subject = "subject", Body = "body" };

            this.dtMessageRecipients.Clear();
            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            this.mockMessagingController.Setup(mc => mc.InputFilter("subject")).Returns("subject_filtered");
            this.mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));
            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(message.Subject, Is.EqualTo("subject_filtered"));
                Assert.That(message.Body, Is.EqualTo("body_filtered"));
            });
        }

        [Test]
        public void MessagingController_CreateMessage_For_CommonUser_Calls_DataService_SaveSocialMessageRecipient_Then_CreateSocialMessageRecipientsForRole()
        {
            // Arrange
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var message = new Message { Subject = "subject", Body = "body" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            // this pattern is based on: http://dpwhelan.com/blog/software-development/moq-sequences/
            var callingSequence = 0;

            // Arrange for Assert
            this.mockDataService.Setup(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.EqualTo(0)));
            this.mockDataService.Setup(ds => ds.SaveMessageRecipient(It.IsAny<MessageRecipient>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.GreaterThan(0)));

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            // SaveMessageRecipient is called twice, one for sent message and second for receive
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_One_User()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_TenName, UserID = Constants.USER_TenId };
            var sender = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            this.mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this.user12UserInfo.PortalID, It.IsAny<double>())).Returns(0);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_ElevenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, null, sender);

            // Assert
            Assert.That(message.To, Is.EqualTo(Constants.USER_TenName));
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_Two_Users()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user10 = new UserInfo { DisplayName = Constants.USER_TenName, UserID = Constants.USER_TenId };
            var user11 = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var sender = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            this.mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this.user12UserInfo.PortalID, It.IsAny<double>())).Returns(0);

            this.dtMessageRecipients.Clear();
            var recipientId = 0;

            // dtMessageRecipients.Rows.Add(Constants.Messaging_RecipientId_2, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage);
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => this.dtMessageRecipients.Rows.Add(recipientId++, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage))
                .Returns(() => this.dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user10, user11 }, null, sender);

            // Assert
            Assert.That(message.To, Is.EqualTo(Constants.USER_TenName + "," + Constants.USER_ElevenName));
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_One_Role()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_Administrators };

            this.dtMessageRecipients.Clear();
            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo>(), null, this.adminUserInfo);

            // Assert
            Assert.That(message.To, Is.EqualTo(Constants.RoleName_Administrators));
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_Two_Roles()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role1 = new RoleInfo { RoleName = Constants.RoleName_Administrators };
            var role2 = new RoleInfo { RoleName = Constants.RoleName_Subscribers };

            this.dtMessageRecipients.Clear();
            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo>(), null, this.adminUserInfo);

            // Assert
            Assert.That(message.To, Is.EqualTo(Constants.RoleName_Administrators + "," + Constants.RoleName_Subscribers));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageAttachment_On_Passing_Attachments()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            this.folderManager.Setup(fm => fm.GetUserFolder(It.IsAny<UserInfo>())).Returns(new FolderInfo { FolderID = -1, FolderPath = "path" });

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.SaveMessageAttachment(It.Is<MessageAttachment>(v => v.MessageID == message.MessageID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByAdmin()
        {
            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);

            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.user12UserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { this.user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers.ToString(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByHost()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.user12UserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.hostUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { this.user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, this.hostUserInfo);

            // Assert
            mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers.ToString(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Roles()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role1 = new RoleInfo { RoleName = "role1", RoleID = Constants.RoleID_RegisteredUsers };
            var role2 = new RoleInfo { RoleName = "role2", RoleID = Constants.RoleID_Administrators };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers + "," + Constants.RoleID_Administrators, It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Roles_ByRoleOwner()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.groupOwnerUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageRecipient_On_Passing_Users()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == user.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Sets_ReplyAll_To_False_On_Passing_Roles()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1", RoleID = Constants.RoleID_RegisteredUsers };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.adminUserInfo);

            // Assert
            Assert.That(message.ReplyAllAllowed, Is.EqualTo(false));
        }

        [Test]
        public void MessagingController_CreateMessage_Sets_ReplyAll_To_True_On_Passing_User()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this.adminUserInfo);

            // Assert
            Assert.That(message.ReplyAllAllowed, Is.EqualTo(true));
        }

        [Test]
        public void MessagingController_CreateMessage_Adds_Sender_As_Recipient_When_Not_Already_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            // Assert
            this.mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == this.adminUserInfo.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Marks_Message_As_Dispatched_For_Sender_When_Not_Already_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            this.mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == this.adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this.adminUserInfo);

            // Assert
            this.mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        [Test]
        public void MessagingController_CreateMessage_Does_Not_Mark_Message_As_Dispatched_For_Sender_When_Already_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this.adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this.dtMessageRecipients.CreateDataReader());

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            this.mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == this.adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            // Act
            this.mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user, this.adminUserInfo }, null, this.adminUserInfo);

            // Assert
            this.mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1), Times.Never());
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_On_Null_Sender()
        {
            // Arrange

            // Act
            Assert.Throws<ArgumentException>(() => this.internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, null));
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_On_Negative_SenderID()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11" };

            // Act
            Assert.Throws<ArgumentException>(() => this.internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, sender));
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_On_Null_Subject()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, null, null, sender));
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_On_Empty_Subject()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            // Act, Assert
            Assert.Throws<ArgumentException>(() => this.internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, string.Empty, null, sender));
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            InternalMessagingController.SetTestableInstance(this.mockInternalMessagingController.Object);
            this.mockInternalMessagingController.Setup(imc => imc.AttachmentsAllowed(Constants.PORTAL_Zero)).Returns(false);

            // Act, Assert
            Assert.Throws<AttachmentsNotAllowed>(() => this.internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", new List<int> { Constants.FOLDER_ValidFileId }, sender));
        }

        [Test]
        public void MessagingController_ReplyMessage_Filters_Input_When_ProfanityFilter_Is_Enabled()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this.mockDataService.Object);

            this.mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this.mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this.mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            this.mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            this.mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this.mockDataService.Verify(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_ReplyMessage_Throws_When_Message_Or_Recipient_Are_Not_Found()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this.mockDataService.Object);

            this.mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this.mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this.mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this.mockDataService.Setup(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Returns(-1);

            Assert.Throws<MessageOrRecipientNotFoundException>(() => this.mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender));
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Read_By_Sender()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this.mockDataService.Object);

            this.mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this.mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this.mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this.mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            this.mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this.mockInternalMessagingController.Verify(imc => imc.MarkRead(It.IsAny<int>(), sender.UserID));
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Dispatched_For_Sender()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this.mockDataService.Object);

            this.mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this.mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this.mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this.mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_1 });

            this.mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this.mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        [Test]
        public void MessagingController_SetReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this.internalMessagingController.MarkRead(messageInstance.ConversationId, user.UserID);

            // Assert
            this.mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this.internalMessagingController.MarkUnRead(messageInstance.ConversationId, user.UserID);

            // Assert
            this.mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, false));
        }

        [Test]
        public void MessagingController_SetArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this.internalMessagingController.MarkArchived(messageInstance.ConversationId, user.UserID);

            // Assert
            this.mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this.internalMessagingController.MarkUnArchived(messageInstance.ConversationId, user.UserID);

            // Assert
            this.mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, false));
        }

        [Test]
        public void GetSocialMessageRecipient_Calls_DataService_GetSocialMessageRecipientByMessageAndUser()
        {
            this.dtMessageRecipients.Clear();
            this.mockDataService
                .Setup(ds => ds.GetMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12))
                .Returns(this.dtMessageRecipients.CreateDataReader())
                .Verifiable();

            this.internalMessagingController.GetMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);

            this.mockDataService.Verify();
        }

        [Test]
        public void WaitTimeForNextMessage_Throws_On_Null_Sender()
        {
            Assert.Throws<ArgumentNullException>(() => this.internalMessagingController.WaitTimeForNextMessage(null));
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_MessagingThrottlingInterval_Is_Zero()
        {
            this.user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            this.mockMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this.user12UserInfo.PortalID, 0.5)).Returns(0.5);

            var result = this.mockInternalMessagingController.Object.WaitTimeForNextMessage(this.user12UserInfo);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_Sender_Is_Admin_Or_Host()
        {
            this.adminUserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(true);

            var result = this.mockInternalMessagingController.Object.WaitTimeForNextMessage(this.adminUserInfo);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_The_User_Has_No_Previous_Conversations()
        {
            this.user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            this.mockMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(false);

            this.mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(this.user12UserInfo)).Returns((Message)null);

            var result = this.mockInternalMessagingController.Object.WaitTimeForNextMessage(this.user12UserInfo);

            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 1, 0)]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 2, 60)]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 10, 540)]
        public void WaitTimeForNextMessage_Returns_The_Number_Of_Seconds_Since_Last_Message_Sent(string actualDateString, string lastMessageDateString, int throttlingInterval, int expected)
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var actualDate = DateTime.Parse(actualDateString, culture);
            var lastMessageDate = DateTime.Parse(lastMessageDateString, culture);
            this.user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            this.mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this.user12UserInfo.PortalID, It.IsAny<double>())).Returns(throttlingInterval);
            this.mockInternalMessagingController.Setup(mc => mc.IsAdminOrHost(this.adminUserInfo)).Returns(false);
            this.dtMessages.Clear();
            this.dtMessages.Rows.Add(-1, 1, 1, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1, -1, -1, lastMessageDate, -1, Null.NullDate);
            var dr = this.dtMessages.CreateDataReader();
            var message = CBO.FillObject<Message>(dr);
            this.mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(It.IsAny<UserInfo>())).Returns(message);
            this.mockInternalMessagingController.Setup(mc => mc.GetDateTimeNow()).Returns(actualDate);
            var result = this.mockInternalMessagingController.Object.WaitTimeForNextMessage(this.user12UserInfo);

            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void GetInbox_Calls_DataService_GetMessageBoxView()
        {
            DataService.RegisterInstance(this.mockDataService.Object);

            this.dtMessageConversationView.Clear();

            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.Any, MessageSentStatus.Received)).Returns(this.dtMessageConversationView.CreateDataReader()).Verifiable();
            this.mockInternalMessagingController.Object.GetInbox(0, 0, 0, string.Empty, false, MessageReadStatus.Any, MessageArchivedStatus.Any);
            this.mockDataService.Verify();
        }

        [Test]
        public void GetInbox_Calls_Overload_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.dtMessageConversationView.Clear();
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived, MessageSentStatus.Received)).Returns(this.dtMessageConversationView.CreateDataReader()).Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetInbox(this.user12UserInfo.UserID, 0, 0, string.Empty, false);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void GetRecentInbox_Calls_GetInbox_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.dtMessageConversationView.Clear();
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived, MessageSentStatus.Received)).Returns(this.dtMessageConversationView.CreateDataReader()).Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12, 0, 0);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void GetRecentInbox_Calls_Overload_With_Default_Values()
        {
            // Arrange
            DataService.RegisterInstance(this.mockDataService.Object);
            this.dtMessageConversationView.Clear();
            this.user12UserInfo.PortalID = Constants.PORTAL_Zero;
            this.mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this.user12UserInfo);
            this.mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived, MessageSentStatus.Received)).Returns(this.dtMessageConversationView.CreateDataReader()).Verifiable();

            // Act
            this.mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12);

            // Assert
            this.mockDataService.Verify();
        }

        [Test]
        public void CountArchivedMessagesByConversation_Calls_DataService_CountArchivedMessagesByConversation()
        {
            this.mockDataService.Setup(ds => ds.CountArchivedMessagesByConversation(It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountArchivedMessagesByConversation(1);
            this.mockDataService.Verify();
        }

        [Test]
        public void CountMessagesByConversation_Calls_DataService_CountMessagesByConversation()
        {
            this.mockDataService.Setup(ds => ds.CountMessagesByConversation(It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountMessagesByConversation(1);
            this.mockDataService.Verify();
        }

        [Test]
        public void CountConversations_Calls_DataService_CountTotalConversations()
        {
            this.mockDataService.Setup(ds => ds.CountTotalConversations(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountConversations(Constants.UserID_User12, Constants.PORTAL_Zero);
            this.mockDataService.Verify();
        }

        [Test]
        public void CountUnreadMessages_Calls_DataService_CountNewThreads()
        {
            this.mockDataService.Setup(ds => ds.CountNewThreads(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountUnreadMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this.mockDataService.Verify();
        }

        [Test]
        public void CountSentMessages_Calls_DataService_CountSentMessages()
        {
            this.mockDataService.Setup(ds => ds.CountSentMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountSentMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this.mockDataService.Verify();
        }

        [Test]
        public void CountArchivedMessages_Calls_DataService_CountArchivedMessages()
        {
            this.mockDataService.Setup(ds => ds.CountArchivedMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this.internalMessagingController.CountArchivedMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this.mockDataService.Verify();
        }

        [Test]
        public void DeleteMessageRecipient_Calls_DataService_DeleteMessageRecipientByMessageAndUser()
        {
            this.mockDataService.Setup(ds => ds.DeleteMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this.internalMessagingController.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this.mockDataService.Verify();
        }

        [Test]
        public void GetMessageRecipients_Calls_DataService_GetMessageRecipientsByMessage()
        {
            this.dtMessageRecipients.Clear();

            this.mockDataService
                .Setup(ds => ds.GetMessageRecipientsByMessage(Constants.Messaging_MessageId_1))
                .Returns(this.dtMessageRecipients.CreateDataReader())
                .Verifiable();
            this.internalMessagingController.GetMessageRecipients(Constants.Messaging_MessageId_1);
            this.mockDataService.Verify();
        }

        private static Message CreateValidMessage()
        {
            var message = new Message
            {
                MessageID = 2,
                Subject = "test",
                Body = "body",
                ConversationId = 1,
                ReplyAllAllowed = false,
                SenderUserID = 1,
                NotificationTypeID = 1,
            };
            return message;
        }

        private void SetupDataProvider()
        {
            // Standard DataProvider Path for Logging
            this.dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

            var dataTable = new DataTable("Languages");
            var pkId = dataTable.Columns.Add("LanguageID", typeof(int));
            dataTable.Columns.Add("CultureCode", typeof(string));
            dataTable.Columns.Add("CultureName", typeof(string));
            dataTable.Columns.Add("FallbackCulture", typeof(string));
            dataTable.Columns.Add("CreatedByUserID", typeof(int));
            dataTable.Columns.Add("CreatedOnDate", typeof(DateTime));
            dataTable.Columns.Add("LastModifiedByUserID", typeof(int));
            dataTable.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            dataTable.PrimaryKey = new[] { pkId };
            dataTable.Rows.Add(1, "en-US", "English (United States)", null, -1, "2011-05-04 09:42:11.530", -1, "2011-05-04 09:42:11.530");

            this.dataProvider.Setup(x => x.GetLanguages()).Returns(dataTable.CreateDataReader());
        }

        private void SetupUsers()
        {
            this.adminUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Admin, UserID = Constants.UserID_Admin, Roles = new[] { Constants.RoleName_Administrators } };
            this.hostUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Host, UserID = Constants.UserID_Host, IsSuperUser = true };
            this.user12UserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_User12, UserID = Constants.UserID_User12 };
            this.groupOwnerUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_FirstSocialGroupOwner, UserID = Constants.UserID_FirstSocialGroupOwner };
        }

        private void SetupPortalSettings()
        {
            var portalSettings = new PortalSettings
            {
                AdministratorRoleName = Constants.RoleName_Administrators,
            };

            this.portalController.Setup(pc => pc.GetCurrentSettings()).Returns(portalSettings);
        }

        private void SetupCachingProvider()
        {
            this.mockCacheProvider.Setup(c => c.GetItem(It.IsAny<string>())).Returns<string>(key =>
            {
                if (key.Contains("Portal-1_"))
                {
                    var portals = new List<PortalInfo>();
                    portals.Add(new PortalInfo() { PortalID = 0 });

                    return portals;
                }
                else if (key.Contains("PortalGroups"))
                {
                    return new List<PortalGroupInfo>();
                }

                return null;
            });
        }

        private void SetupRoleProvider()
        {
            var adminRoleInfoForAdministrators = new UserRoleInfo { RoleName = Constants.RoleName_Administrators, RoleID = Constants.RoleID_Administrators, UserID = Constants.UserID_Admin };
            var adminRoleInfoForRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var user12RoleInfoForRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var userFirstSocialGroupOwner = new UserRoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup, UserID = Constants.UserID_FirstSocialGroupOwner, IsOwner = true };

            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_Admin), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { adminRoleInfoForAdministrators, adminRoleInfoForRegisteredUsers });
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_User12), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user12RoleInfoForRegisteredUsers });
            this.mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_FirstSocialGroupOwner), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { userFirstSocialGroupOwner });
        }

        private void SetupFileControllers()
        {
            this.folderManager.Setup(f => f.GetFolder(It.IsAny<int>())).Returns(new FolderInfo());
            this.fileManager.Setup(f => f.GetFile(It.IsAny<int>())).Returns(new FileInfo());
            this.folderPermissionController.Setup(f => f.CanViewFolder(It.IsAny<IFolderInfo>())).Returns(true);
        }

        private void SetupDataTables()
        {
            // Messages
            this.dtMessages = new DataTable("Messages");
            var pkMessagesMessageID = this.dtMessages.Columns.Add("MessageID", typeof(int));
            this.dtMessages.Columns.Add("PortalId", typeof(int));
            this.dtMessages.Columns.Add("NotificationTypeID", typeof(int));
            this.dtMessages.Columns.Add("To", typeof(string));
            this.dtMessages.Columns.Add("From", typeof(string));
            this.dtMessages.Columns.Add("Subject", typeof(string));
            this.dtMessages.Columns.Add("Body", typeof(string));
            this.dtMessages.Columns.Add("ConversationId", typeof(int));
            this.dtMessages.Columns.Add("ReplyAllAllowed", typeof(bool));
            this.dtMessages.Columns.Add("SenderUserID", typeof(int));
            this.dtMessages.Columns.Add("CreatedByUserID", typeof(int));
            this.dtMessages.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtMessages.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtMessages.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtMessages.PrimaryKey = new[] { pkMessagesMessageID };

            // MessageRecipients
            this.dtMessageRecipients = new DataTable("MessageRecipients");
            var pkMessageRecipientID = this.dtMessageRecipients.Columns.Add("RecipientID", typeof(int));
            this.dtMessageRecipients.Columns.Add("MessageID", typeof(int));
            this.dtMessageRecipients.Columns.Add("UserID", typeof(int));
            this.dtMessageRecipients.Columns.Add("Read", typeof(bool));
            this.dtMessageRecipients.Columns.Add("Archived", typeof(bool));
            this.dtMessageRecipients.Columns.Add("CreatedByUserID", typeof(int));
            this.dtMessageRecipients.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtMessageRecipients.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtMessageRecipients.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtMessageRecipients.PrimaryKey = new[] { pkMessageRecipientID };

            // MessageAttachments
            this.dtMessageAttachment = new DataTable("MessageAttachments");
            var pkMessageAttachmentID = this.dtMessageAttachment.Columns.Add("MessageAttachmentID", typeof(int));
            this.dtMessageAttachment.Columns.Add("MessageID", typeof(int));
            this.dtMessageAttachment.Columns.Add("FileID", typeof(int));
            this.dtMessageAttachment.Columns.Add("CreatedByUserID", typeof(int));
            this.dtMessageAttachment.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtMessageAttachment.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtMessageAttachment.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtMessageAttachment.PrimaryKey = new[] { pkMessageAttachmentID };

            // Portal Settings
            this.dtPortalSettings = new DataTable("PortalSettings");
            this.dtPortalSettings.Columns.Add("SettingName", typeof(string));
            this.dtPortalSettings.Columns.Add("SettingValue", typeof(string));
            this.dtPortalSettings.Columns.Add("CultureCode", typeof(string));
            this.dtPortalSettings.Columns.Add("CreatedByUserID", typeof(int));
            this.dtPortalSettings.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtPortalSettings.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtPortalSettings.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Message Conversation
            this.dtMessageConversationView = new DataTable();
            this.dtMessageConversationView.Columns.Add("RowNumber", typeof(int));
            this.dtMessageConversationView.Columns.Add("AttachmentCount", typeof(int));
            this.dtMessageConversationView.Columns.Add("NewThreadCount", typeof(int));
            this.dtMessageConversationView.Columns.Add("ThreadCount", typeof(int));
            this.dtMessageConversationView.Columns.Add("MessageID", typeof(int));
            this.dtMessageConversationView.Columns.Add("To", typeof(string));
            this.dtMessageConversationView.Columns.Add("From", typeof(string));
            this.dtMessageConversationView.Columns.Add("Subject", typeof(string));
            this.dtMessageConversationView.Columns.Add("Body", typeof(string));
            this.dtMessageConversationView.Columns.Add("ConversationID", typeof(int));
            this.dtMessageConversationView.Columns.Add("ReplyAllAllowed", typeof(bool));
            this.dtMessageConversationView.Columns.Add("SenderUserID", typeof(int));
            this.dtMessageConversationView.Columns.Add("CreatedByUserID", typeof(int));
            this.dtMessageConversationView.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtMessageConversationView.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtMessageConversationView.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Thread View
            this.dtMessageThreadsView = new DataTable();
            this.dtMessageThreadsView.Columns.Add("TotalThreads", typeof(int));
            this.dtMessageThreadsView.Columns.Add("TotalNewThreads", typeof(int));
            this.dtMessageThreadsView.Columns.Add("TotalArchivedThreads", typeof(int));
        }
    }
}

// ReSharper restore InconsistentNaming
