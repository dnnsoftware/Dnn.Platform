// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable InconsistentNaming
namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Internal;
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
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///  Testing various aspects of MessagingController.
    /// </summary>
    [TestFixture]
    public class MessagingControllerTests
    {
        private Mock<IDataService> _mockDataService;
        private MessagingController _messagingController;
        private InternalMessagingControllerImpl _internalMessagingController;
        private Mock<MessagingController> _mockMessagingController;
        private Mock<InternalMessagingControllerImpl> _mockInternalMessagingController;
        private Mock<DataProvider> _dataProvider;
        private Mock<IPortalController> _portalController;
        private Mock<RoleProvider> _mockRoleProvider;
        private Mock<CachingProvider> _mockCacheProvider;
        private Mock<ILocalizationProvider> _mockLocalizationProvider;
        private Mock<IFolderManager> _folderManager;
        private Mock<IFileManager> _fileManager;
        private Mock<IFolderPermissionController> _folderPermissionController;

        private DataTable _dtMessages;
        private DataTable _dtMessageAttachment;
        private DataTable _dtMessageRecipients;
        private DataTable _dtPortalSettings;
        private DataTable _dtMessageConversationView;
        private DataTable _dtMessageThreadsView;

        private UserInfo _adminUserInfo;
        private UserInfo _hostUserInfo;
        private UserInfo _user12UserInfo;
        private UserInfo _groupOwnerUserInfo;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            this._mockDataService = new Mock<IDataService>();
            this._dataProvider = MockComponentProvider.CreateDataProvider();
            this._mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            this._mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            this._mockLocalizationProvider = MockComponentProvider.CreateLocalizationProvider();
            this._mockLocalizationProvider.Setup(l => l.GetString(It.IsAny<string>(), It.IsAny<string>())).Returns("{0}_{1}");

            this._messagingController = new MessagingController(this._mockDataService.Object);
            this._internalMessagingController = new InternalMessagingControllerImpl(this._mockDataService.Object);
            this._mockMessagingController = new Mock<MessagingController> { CallBase = true };
            this._mockInternalMessagingController = new Mock<InternalMessagingControllerImpl> { CallBase = true };

            this._portalController = new Mock<IPortalController>();
            this._portalController.Setup(c => c.GetPortalSettings(It.IsAny<int>())).Returns(new Dictionary<string, string>());
            PortalController.SetTestableInstance(this._portalController.Object);

            DataService.RegisterInstance(this._mockDataService.Object);

            this._folderManager = new Mock<IFolderManager>();
            this._fileManager = new Mock<IFileManager>();
            this._folderPermissionController = new Mock<IFolderPermissionController>();

            FolderManager.RegisterInstance(this._folderManager.Object);
            FileManager.RegisterInstance(this._fileManager.Object);
            FolderPermissionController.SetTestableInstance(this._folderPermissionController.Object);

            this.SetupDataProvider();
            this.SetupRoleProvider();
            this.SetupDataTables();
            this.SetupUsers();
            this.SetupPortalSettings();
            this.SetupCachingProvider();
            this.SetupFileControllers();

            this._mockInternalMessagingController.Setup(m => m.GetLastSentMessage(It.IsAny<UserInfo>())).Returns((Message)null);
        }

        [TearDown]
        public void TearDown()
        {
            ComponentFactory.Container = null;
            PortalController.ClearInstance();
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
            this._mockMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("YES");
            var result = this._mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.IsTrue(result);
        }

        [Test]
        public void IncludeAttachments_Returns_True_When_MessagingIncludeAttachments_Setting_Is_YES()
        {
            this._mockMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("YES");
            var result = this._mockInternalMessagingController.Object.IncludeAttachments(Constants.CONTENT_ValidPortalId);
            Assert.IsTrue(result);
        }

        [Test]
        public void AttachmentsAllowed_Returns_False_When_MessagingAllowAttachments_Setting_Is_Not_YES()
        {
            this._mockInternalMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("NO");
            var result = this._mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.IsFalse(result);
        }

        [Test]
        public void GetArchivedMessages_Calls_DataService_GetArchiveBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(this._mockDataService.Object);

            this._user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this._mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this._user12UserInfo);

            this._dtMessageConversationView.Clear();

            this._mockDataService
                .Setup(ds => ds.GetArchiveBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this._dtMessageConversationView.CreateDataReader())
                .Verifiable();

            this._mockInternalMessagingController.Object.GetArchivedMessages(Constants.UserID_User12, 0, 0);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetMessageThread_Calls_DataService_GetMessageThread()
        {
            var totalRecords = 0;

            DataService.RegisterInstance(this._mockDataService.Object);

            this._dtMessageThreadsView.Clear();

            this._mockDataService.Setup(ds => ds.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), ref totalRecords)).Returns(this._dtMessageThreadsView.CreateDataReader()).Verifiable();
            this._mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, string.Empty, false, ref totalRecords);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetMessageThread_Calls_Overload_With_Default_Values()
        {
            int[] totalRecords = { 0 };
            this._mockInternalMessagingController
                .Setup(mc => mc.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending, ref totalRecords[0]))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, ref totalRecords[0]);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentSentbox_Calls_Overload_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetRecentSentbox(Constants.UserID_User12, MessagingController.ConstDefaultPageIndex, MessagingController.ConstDefaultPageSize))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentSentbox_Calls_GetSentbox_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetSentbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetSentbox_Calls_DataService_GetSentBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(this._mockDataService.Object);

            this._user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this._mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this._user12UserInfo);

            this._dtMessageConversationView.Clear();

            this._mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(this._dtMessageConversationView.CreateDataReader())
                .Verifiable();

            this._mockInternalMessagingController.Object.GetSentbox(
                Constants.UserID_User12,
                0,
                0,
                string.Empty,
                false,
                MessageReadStatus.Any,
                MessageArchivedStatus.Any);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetSentbox_Calls_Overload_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetSentbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetSentbox(Constants.UserID_User12, 0, 0, string.Empty, false);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void RecipientLimit_Returns_MessagingRecipientLimit_Setting()
        {
            const int expected = 10;
            this._mockInternalMessagingController
                .Setup(mc => mc.GetPortalSettingAsInteger("MessagingRecipientLimit", Constants.CONTENT_ValidPortalId, It.IsAny<int>()))
                .Returns(expected);

            var actual = this._mockInternalMessagingController.Object.RecipientLimit(Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Message()
        {
            // Act, Assert
            this._messagingController.SendMessage(null, new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Body_And_Subject()
        {
            // Act, Assert
            this._messagingController.SendMessage(new Message(), new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Roles_And_Users()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            this._messagingController.SendMessage(message, null, null, null, this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Empty_Roles_And_Users_Lists()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            this._messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo>(), null, this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Roles_And_Users_With_No_DisplayNames()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            // Act, Assert
            this._messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
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
            this._messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
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
            this._messagingController.SendMessage(message, roles, users, null, this._user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Sender()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            // Act
            this._messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Negative_SenderID()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var sender = new UserInfo { DisplayName = "user11" };

            // Act
            this._messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_SendingToRole_ByNonAdmin()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            this._dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this._dtMessageRecipients.CreateDataReader());

            // Act
            messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._user12UserInfo);
        }

        // [Test]
        [ExpectedException(typeof(AttachmentsNotAllowed))]
        public void MessagingController_CreateMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            // disable caching
            this._mockCacheProvider.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            this._dtPortalSettings.Clear();
            this._dtPortalSettings.Rows.Add(Constants.PORTALSETTING_MessagingAllowAttachments_Name, Constants.PORTALSETTING_MessagingAllowAttachments_Value_NO, Constants.CULTURE_EN_US);
            this._dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns(this._dtPortalSettings.CreateDataReader());

            this._dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this._dtMessageRecipients.CreateDataReader());

            // Act
            messagingController.SendMessage(message, null, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._user12UserInfo);
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessage_On_Valid_Message()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.SaveMessage(
                It.Is<Message>(v => v.PortalID == Constants.PORTAL_Zero && v.Subject == "subject"
                                                                             && v.Body == "body"
                                                                             && v.To == "role1,user1"
                                                                             && v.SenderUserID == this._adminUserInfo.UserID),
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

            this._dtMessageRecipients.Clear();
            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            this._mockMessagingController.Setup(mc => mc.InputFilter("subject")).Returns("subject_filtered");
            this._mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));
            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

            // Assert
            Assert.AreEqual("subject_filtered", message.Subject);
            Assert.AreEqual("body_filtered", message.Body);
        }

        [Test]
        public void MessagingController_CreateMessage_For_CommonUser_Calls_DataService_SaveSocialMessageRecipient_Then_CreateSocialMessageRecipientsForRole()
        {
            // Arrange
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var message = new Message { Subject = "subject", Body = "body" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            // this pattern is based on: http://dpwhelan.com/blog/software-development/moq-sequences/
            var callingSequence = 0;

            // Arrange for Assert
            this._mockDataService.Setup(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.EqualTo(0)));
            this._mockDataService.Setup(ds => ds.SaveMessageRecipient(It.IsAny<MessageRecipient>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.GreaterThan(0)));

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

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

            this._mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this._user12UserInfo.PortalID, It.IsAny<double>())).Returns(0);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_ElevenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, null, sender);

            // Assert
            Assert.AreEqual(message.To, Constants.USER_TenName);
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

            this._mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this._user12UserInfo.PortalID, It.IsAny<double>())).Returns(0);

            this._dtMessageRecipients.Clear();
            var recipientId = 0;

            // _dtMessageRecipients.Rows.Add(Constants.Messaging_RecipientId_2, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage);
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => this._dtMessageRecipients.Rows.Add(recipientId++, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage))
                .Returns(() => this._dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user10, user11 }, null, sender);

            // Assert
            Assert.AreEqual(message.To, Constants.USER_TenName + "," + Constants.USER_ElevenName);
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_One_Role()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_Administrators };

            this._dtMessageRecipients.Clear();
            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo>(), null, this._adminUserInfo);

            // Assert
            Assert.AreEqual(message.To, Constants.RoleName_Administrators);
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_Two_Roles()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role1 = new RoleInfo { RoleName = Constants.RoleName_Administrators };
            var role2 = new RoleInfo { RoleName = Constants.RoleName_Subscribers };

            this._dtMessageRecipients.Clear();
            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo>(), null, this._adminUserInfo);

            // Assert
            Assert.AreEqual(message.To, Constants.RoleName_Administrators + "," + Constants.RoleName_Subscribers);
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageAttachment_On_Passing_Attachments()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.SaveMessageAttachment(It.Is<MessageAttachment>(v => v.MessageID == message.MessageID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByAdmin()
        {
            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);

            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._user12UserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { this._user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers.ToString(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByHost()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._user12UserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._hostUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { this._user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, this._hostUserInfo);

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

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers + "," + Constants.RoleID_Administrators, It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Roles_ByRoleOwner()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._groupOwnerUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageRecipient_On_Passing_Users()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == user.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Sets_ReplyAll_To_False_On_Passing_Roles()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1", RoleID = Constants.RoleID_RegisteredUsers };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._adminUserInfo);

            // Assert
            Assert.AreEqual(message.ReplyAllAllowed, false);
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
                .Returns(this._dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, this._adminUserInfo);

            // Assert
            Assert.AreEqual(message.ReplyAllAllowed, true);
        }

        [Test]
        public void MessagingController_CreateMessage_Adds_Sender_As_Recipient_When_Not_Aready_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

            // Assert
            this._mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == this._adminUserInfo.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Marks_Message_As_Dispatched_For_Sender_When_Not_Already_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            this._mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == this._adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, this._adminUserInfo);

            // Assert
            this._mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        [Test]
        public void MessagingController_CreateMessage_Does_Not_Mark_Message_As_Dispatched_For_Sender_When_Already_A_Recipient()
        {
            // Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), this._adminUserInfo.UserID))
                .Callback(this.SetupDataTables)
                .Returns(this._dtMessageRecipients.CreateDataReader());

            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            this._mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == this._adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            // Act
            this._mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user, this._adminUserInfo }, null, this._adminUserInfo);

            // Assert
            this._mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1), Times.Never());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Null_Sender()
        {
            // Arrange

            // Act
            this._internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Negative_SenderID()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11" };

            // Act
            this._internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Null_Subject()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            // Act, Assert
            this._internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, null, null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Empty_Subject()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            // Act, Assert
            this._internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, string.Empty, null, sender);
        }

        [Test]
        [ExpectedException(typeof(AttachmentsNotAllowed))]
        public void MessagingController_ReplyMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);
            this._mockInternalMessagingController.Setup(imc => imc.AttachmentsAllowed(Constants.PORTAL_Zero)).Returns(false);

            // Act, Assert
            this._internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", new List<int> { Constants.FOLDER_ValidFileId }, sender);
        }

        [Test]
        public void MessagingController_ReplyMessage_Filters_Input_When_ProfanityFilter_Is_Enabled()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this._mockDataService.Object);

            this._mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this._mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this._mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            this._mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            this._mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this._mockDataService.Verify(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        [ExpectedException(typeof(MessageOrRecipientNotFoundException))]
        public void MessagingController_ReplyMessage_Throws_When_Message_Or_Recipient_Are_Not_Found()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this._mockDataService.Object);

            this._mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this._mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this._mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this._mockDataService.Setup(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Returns(-1);

            this._mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Read_By_Sender()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this._mockDataService.Object);

            this._mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this._mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this._mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this._mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            this._mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this._mockInternalMessagingController.Verify(imc => imc.MarkRead(It.IsAny<int>(), sender.UserID));
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Dispatched_For_Sender()
        {
            // Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(this._mockDataService.Object);

            this._mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            this._mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            this._mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            this._mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_1 });

            this._mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            this._mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        [Test]
        public void MessagingController_SetReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this._internalMessagingController.MarkRead(messageInstance.ConversationId, user.UserID);

            // Assert
            this._mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this._internalMessagingController.MarkUnRead(messageInstance.ConversationId, user.UserID);

            // Assert
            this._mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, false));
        }

        [Test]
        public void MessagingController_SetArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this._internalMessagingController.MarkArchived(messageInstance.ConversationId, user.UserID);

            // Assert
            this._mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            // Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            // Act
            this._internalMessagingController.MarkUnArchived(messageInstance.ConversationId, user.UserID);

            // Assert
            this._mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, false));
        }

        [Test]
        public void GetSocialMessageRecipient_Calls_DataService_GetSocialMessageRecipientByMessageAndUser()
        {
            this._dtMessageRecipients.Clear();
            this._mockDataService
                .Setup(ds => ds.GetMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12))
                .Returns(this._dtMessageRecipients.CreateDataReader())
                .Verifiable();

            this._internalMessagingController.GetMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);

            this._mockDataService.Verify();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitTimeForNextMessage_Throws_On_Null_Sender()
        {
            this._internalMessagingController.WaitTimeForNextMessage(null);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_MessagingThrottlingInterval_Is_Zero()
        {
            this._user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            this._mockMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this._user12UserInfo.PortalID, 0.5)).Returns(0.5);

            var result = this._mockInternalMessagingController.Object.WaitTimeForNextMessage(this._user12UserInfo);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_Sender_Is_Admin_Or_Host()
        {
            this._adminUserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            this._mockMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), this._adminUserInfo.PortalID, Null.NullInteger)).Returns(1);
            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(true);

            var result = this._mockInternalMessagingController.Object.WaitTimeForNextMessage(this._adminUserInfo);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_The_User_Has_No_Previous_Conversations()
        {
            this._user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            this._mockMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), this._user12UserInfo.PortalID, Null.NullInteger)).Returns(1);
            this._mockMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(false);

            this._mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(this._user12UserInfo)).Returns((Message)null);

            var result = this._mockInternalMessagingController.Object.WaitTimeForNextMessage(this._user12UserInfo);

            Assert.AreEqual(0, result);
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
            this._user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            this._mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsDouble(It.IsAny<string>(), this._user12UserInfo.PortalID, It.IsAny<double>())).Returns(throttlingInterval);
            this._mockInternalMessagingController.Setup(mc => mc.IsAdminOrHost(this._adminUserInfo)).Returns(false);
            this._dtMessages.Clear();
            this._dtMessages.Rows.Add(-1, 1, 1, string.Empty, string.Empty, string.Empty, string.Empty, -1, -1, -1, -1, lastMessageDate, -1, Null.NullDate);
            var dr = this._dtMessages.CreateDataReader();
            var message = CBO.FillObject<Message>(dr);
            this._mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(It.IsAny<UserInfo>())).Returns(message);
            this._mockInternalMessagingController.Setup(mc => mc.GetDateTimeNow()).Returns(actualDate);
            var result = this._mockInternalMessagingController.Object.WaitTimeForNextMessage(this._user12UserInfo);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void GetInbox_Calls_DataService_GetMessageBoxView()
        {
            DataService.RegisterInstance(this._mockDataService.Object);

            this._dtMessageConversationView.Clear();

            this._user12UserInfo.PortalID = Constants.PORTAL_Zero;

            this._mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(this._user12UserInfo);
            this._mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.Any, MessageSentStatus.Received)).Returns(this._dtMessageConversationView.CreateDataReader()).Verifiable();
            this._mockInternalMessagingController.Object.GetInbox(0, 0, 0, string.Empty, false, MessageReadStatus.Any, MessageArchivedStatus.Any);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetInbox_Calls_Overload_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetInbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetInbox(Constants.UserID_User12, 0, 0, string.Empty, false);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentInbox_Calls_GetInbox_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetInbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12, 0, 0);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentInbox_Calls_Overload_With_Default_Values()
        {
            this._mockInternalMessagingController
                .Setup(mc => mc.GetRecentInbox(Constants.UserID_User12, MessagingController.ConstDefaultPageIndex, MessagingController.ConstDefaultPageSize))
                .Verifiable();

            this._mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12);

            this._mockInternalMessagingController.Verify();
        }

        [Test]
        public void CountArchivedMessagesByConversation_Calls_DataService_CountArchivedMessagesByConversation()
        {
            this._mockDataService.Setup(ds => ds.CountArchivedMessagesByConversation(It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountArchivedMessagesByConversation(1);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountMessagesByConversation_Calls_DataService_CountMessagesByConversation()
        {
            this._mockDataService.Setup(ds => ds.CountMessagesByConversation(It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountMessagesByConversation(1);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountConversations_Calls_DataService_CountTotalConversations()
        {
            this._mockDataService.Setup(ds => ds.CountTotalConversations(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountConversations(Constants.UserID_User12, Constants.PORTAL_Zero);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountUnreadMessages_Calls_DataService_CountNewThreads()
        {
            this._mockDataService.Setup(ds => ds.CountNewThreads(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountUnreadMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountSentMessages_Calls_DataService_CountSentMessages()
        {
            this._mockDataService.Setup(ds => ds.CountSentMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountSentMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountArchivedMessages_Calls_DataService_CountArchivedMessages()
        {
            this._mockDataService.Setup(ds => ds.CountArchivedMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            this._internalMessagingController.CountArchivedMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            this._mockDataService.Verify();
        }

        [Test]
        public void DeleteMessageRecipient_Calls_DataService_DeleteMessageRecipientByMessageAndUser()
        {
            this._mockDataService.Setup(ds => ds.DeleteMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this._internalMessagingController.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetMessageRecipients_Calls_DataService_GetMessageRecipientsByMessage()
        {
            this._dtMessageRecipients.Clear();

            this._mockDataService
                .Setup(ds => ds.GetMessageRecipientsByMessage(Constants.Messaging_MessageId_1))
                .Returns(this._dtMessageRecipients.CreateDataReader())
                .Verifiable();
            this._internalMessagingController.GetMessageRecipients(Constants.Messaging_MessageId_1);
            this._mockDataService.Verify();
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
            this._dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);

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

            this._dataProvider.Setup(x => x.GetLanguages()).Returns(dataTable.CreateDataReader());
        }

        private void SetupUsers()
        {
            this._adminUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Admin, UserID = Constants.UserID_Admin, Roles = new[] { Constants.RoleName_Administrators } };
            this._hostUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Host, UserID = Constants.UserID_Host, IsSuperUser = true };
            this._user12UserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_User12, UserID = Constants.UserID_User12 };
            this._groupOwnerUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_FirstSocialGroupOwner, UserID = Constants.UserID_FirstSocialGroupOwner };
        }

        private void SetupPortalSettings()
        {
            var portalSettings = new PortalSettings
            {
                AdministratorRoleName = Constants.RoleName_Administrators,
            };

            this._portalController.Setup(pc => pc.GetCurrentPortalSettings()).Returns(portalSettings);
        }

        private void SetupCachingProvider()
        {
            this._mockCacheProvider.Setup(c => c.GetItem(It.IsAny<string>())).Returns<string>(key =>
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
            var adminRoleInfoforRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var user12RoleInfoforRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var userFirstSocialGroupOwner = new UserRoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup, UserID = Constants.UserID_FirstSocialGroupOwner, IsOwner = true };

            this._mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_Admin), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { adminRoleInfoForAdministrators, adminRoleInfoforRegisteredUsers });
            this._mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_User12), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user12RoleInfoforRegisteredUsers });
            this._mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_FirstSocialGroupOwner), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { userFirstSocialGroupOwner });
        }

        private void SetupFileControllers()
        {
            this._folderManager.Setup(f => f.GetFolder(It.IsAny<int>())).Returns(new FolderInfo());
            this._fileManager.Setup(f => f.GetFile(It.IsAny<int>())).Returns(new FileInfo());
            this._folderPermissionController.Setup(f => f.CanViewFolder(It.IsAny<IFolderInfo>())).Returns(true);
        }

        private void SetupDataTables()
        {
            // Messages
            this._dtMessages = new DataTable("Messages");
            var pkMessagesMessageID = this._dtMessages.Columns.Add("MessageID", typeof(int));
            this._dtMessages.Columns.Add("PortalId", typeof(int));
            this._dtMessages.Columns.Add("NotificationTypeID", typeof(int));
            this._dtMessages.Columns.Add("To", typeof(string));
            this._dtMessages.Columns.Add("From", typeof(string));
            this._dtMessages.Columns.Add("Subject", typeof(string));
            this._dtMessages.Columns.Add("Body", typeof(string));
            this._dtMessages.Columns.Add("ConversationId", typeof(int));
            this._dtMessages.Columns.Add("ReplyAllAllowed", typeof(bool));
            this._dtMessages.Columns.Add("SenderUserID", typeof(int));
            this._dtMessages.Columns.Add("CreatedByUserID", typeof(int));
            this._dtMessages.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtMessages.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtMessages.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this._dtMessages.PrimaryKey = new[] { pkMessagesMessageID };

            // MessageRecipients
            this._dtMessageRecipients = new DataTable("MessageRecipients");
            var pkMessageRecipientID = this._dtMessageRecipients.Columns.Add("RecipientID", typeof(int));
            this._dtMessageRecipients.Columns.Add("MessageID", typeof(int));
            this._dtMessageRecipients.Columns.Add("UserID", typeof(int));
            this._dtMessageRecipients.Columns.Add("Read", typeof(bool));
            this._dtMessageRecipients.Columns.Add("Archived", typeof(bool));
            this._dtMessageRecipients.Columns.Add("CreatedByUserID", typeof(int));
            this._dtMessageRecipients.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtMessageRecipients.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtMessageRecipients.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this._dtMessageRecipients.PrimaryKey = new[] { pkMessageRecipientID };

            // MessageAttachments
            this._dtMessageAttachment = new DataTable("MessageAttachments");
            var pkMessageAttachmentID = this._dtMessageAttachment.Columns.Add("MessageAttachmentID", typeof(int));
            this._dtMessageAttachment.Columns.Add("MessageID", typeof(int));
            this._dtMessageAttachment.Columns.Add("FileID", typeof(int));
            this._dtMessageAttachment.Columns.Add("CreatedByUserID", typeof(int));
            this._dtMessageAttachment.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtMessageAttachment.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtMessageAttachment.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this._dtMessageAttachment.PrimaryKey = new[] { pkMessageAttachmentID };

            // Portal Settings
            this._dtPortalSettings = new DataTable("PortalSettings");
            this._dtPortalSettings.Columns.Add("SettingName", typeof(string));
            this._dtPortalSettings.Columns.Add("SettingValue", typeof(string));
            this._dtPortalSettings.Columns.Add("CultureCode", typeof(string));
            this._dtPortalSettings.Columns.Add("CreatedByUserID", typeof(int));
            this._dtPortalSettings.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtPortalSettings.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtPortalSettings.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Message Conversation
            this._dtMessageConversationView = new DataTable();
            this._dtMessageConversationView.Columns.Add("RowNumber", typeof(int));
            this._dtMessageConversationView.Columns.Add("AttachmentCount", typeof(int));
            this._dtMessageConversationView.Columns.Add("NewThreadCount", typeof(int));
            this._dtMessageConversationView.Columns.Add("ThreadCount", typeof(int));
            this._dtMessageConversationView.Columns.Add("MessageID", typeof(int));
            this._dtMessageConversationView.Columns.Add("To", typeof(string));
            this._dtMessageConversationView.Columns.Add("From", typeof(string));
            this._dtMessageConversationView.Columns.Add("Subject", typeof(string));
            this._dtMessageConversationView.Columns.Add("Body", typeof(string));
            this._dtMessageConversationView.Columns.Add("ConversationID", typeof(int));
            this._dtMessageConversationView.Columns.Add("ReplyAllAllowed", typeof(bool));
            this._dtMessageConversationView.Columns.Add("SenderUserID", typeof(int));
            this._dtMessageConversationView.Columns.Add("CreatedByUserID", typeof(int));
            this._dtMessageConversationView.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtMessageConversationView.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtMessageConversationView.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Thread View
            this._dtMessageThreadsView = new DataTable();
            this._dtMessageThreadsView.Columns.Add("TotalThreads", typeof(int));
            this._dtMessageThreadsView.Columns.Add("TotalNewThreads", typeof(int));
            this._dtMessageThreadsView.Columns.Add("TotalArchivedThreads", typeof(int));
        }
    }
}

// ReSharper restore InconsistentNaming
