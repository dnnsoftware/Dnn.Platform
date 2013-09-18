#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

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
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Social.Messaging.Data;
using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Services.Social.Messaging.Exceptions;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Tests.Utilities;
using DotNetNuke.Tests.Utilities.Mocks;

using Moq;

using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace DotNetNuke.Tests.Core.Controllers.Messaging
{
    /// <summary>
    ///  Testing various aspects of MessagingController
    /// </summary>
    [TestFixture]
    public class MessagingControllerTests
    {
        #region "Private Properties"

        private Mock<IDataService> _mockDataService;
        private MessagingController _messagingController;
        private InternalMessagingControllerImpl _internalMessagingController;
        private Mock<MessagingController> _mockMessagingController;
        private Mock<InternalMessagingControllerImpl> _mockInternalMessagingController;
        private Mock<DataProvider> _dataProvider;
        private Mock<IPortalSettings> _portalSettingsWrapper;
        private Mock<RoleProvider> _mockRoleProvider;
        private Mock<CachingProvider> _mockCacheProvider;

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

        #endregion

        #region "Set Up"

        [SetUp]
        public void SetUp()
        {

            ComponentFactory.Container = new SimpleContainer();
            _mockDataService = new Mock<IDataService>();
            _dataProvider = MockComponentProvider.CreateDataProvider();
            _mockRoleProvider = MockComponentProvider.CreateRoleProvider();
            _mockCacheProvider = MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            _messagingController = new MessagingController(_mockDataService.Object);
            _internalMessagingController = new InternalMessagingControllerImpl(_mockDataService.Object);
            _mockMessagingController = new Mock<MessagingController> { CallBase = true };
            _mockInternalMessagingController = new Mock<InternalMessagingControllerImpl> { CallBase = true };

            _portalSettingsWrapper = new Mock<IPortalSettings>();
            TestablePortalSettings.RegisterInstance(_portalSettingsWrapper.Object);

            DataService.RegisterInstance(_mockDataService.Object);

            SetupDataProvider();
            SetupRoleProvider();
            SetupDataTables();
            SetupUsers();
            SetupPortalSettingsWrapper();
            SetupCachingProvider();
        }

        [TearDown]
        public void TearDown()
        {
            ComponentFactory.Container = null;
        }

        private void SetupDataProvider()
        {
            //Standard DataProvider Path for Logging
            _dataProvider.Setup(d => d.GetProviderPath()).Returns("");

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

            _dataProvider.Setup(x => x.GetLanguages()).Returns(dataTable.CreateDataReader());
        }

        private void SetupUsers()
        {
            _adminUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Admin, UserID = Constants.UserID_Admin, Roles = new[] { Constants.RoleName_Administrators } };
            _hostUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_Host, UserID = Constants.UserID_Host, IsSuperUser = true };
            _user12UserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_User12, UserID = Constants.UserID_User12 };
            _groupOwnerUserInfo = new UserInfo { DisplayName = Constants.UserDisplayName_FirstSocialGroupOwner, UserID = Constants.UserID_FirstSocialGroupOwner };
        }

        private void SetupPortalSettingsWrapper()
        {
            _portalSettingsWrapper.Setup(ps => ps.AdministratorRoleName).Returns(Constants.RoleName_Administrators);
        }

        private void SetupCachingProvider()
        {
            _mockCacheProvider.Setup(c => c.GetItem(It.IsAny<string>())).Returns<string>((key =>
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
            }));
        }

        private void SetupRoleProvider()
        {
            var adminRoleInfoForAdministrators = new UserRoleInfo { RoleName = Constants.RoleName_Administrators, RoleID = Constants.RoleID_Administrators, UserID = Constants.UserID_Admin };
            var adminRoleInfoforRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var user12RoleInfoforRegisteredUsers = new UserRoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers, UserID = Constants.UserID_User12 };
            var userFirstSocialGroupOwner = new UserRoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup, UserID = Constants.UserID_FirstSocialGroupOwner, IsOwner = true };

            _mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_Admin), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { adminRoleInfoForAdministrators, adminRoleInfoforRegisteredUsers });
            _mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_User12), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { user12RoleInfoforRegisteredUsers });
            _mockRoleProvider.Setup(rp => rp.GetUserRoles(It.Is<UserInfo>(u => u.UserID == Constants.UserID_FirstSocialGroupOwner), It.IsAny<bool>())).Returns(new List<UserRoleInfo> { userFirstSocialGroupOwner });
        }

        #endregion

        #region Constructor Tests

        [Test]
        public void MessagingController_Constructor_Throws_On_Null_DataService()
        {
            //Arrange            

            //Act, Assert
            Assert.Throws<ArgumentNullException>(() => new MessagingController(null));
        }

        #endregion

        #region Easy Wrapper APIs Tests

        #region AttachmentsAllowed

        [Test]
        public void AttachmentsAllowed_Returns_True_When_MessagingAllowAttachments_Setting_Is_YES()
        {
            _mockMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("YES");
            var result = _mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.IsTrue(result);
        }

        [Test]
        public void AttachmentsAllowed_Returns_False_When_MessagingAllowAttachments_Setting_Is_Not_YES()
        {
            _mockInternalMessagingController.Setup(mc => mc.GetPortalSetting("MessagingAllowAttachments", Constants.CONTENT_ValidPortalId, "YES")).Returns("NO");
            var result = _mockInternalMessagingController.Object.AttachmentsAllowed(Constants.CONTENT_ValidPortalId);
            Assert.IsFalse(result);
        }

        #endregion

        #region GetArchivedMessages

        [Test]
        public void GetArchivedMessages_Calls_DataService_GetArchiveBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(_mockDataService.Object);

            _user12UserInfo.PortalID = Constants.PORTAL_Zero;

            _mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(_user12UserInfo);

            _dtMessageConversationView.Clear();

            _mockDataService
                .Setup(ds => ds.GetArchiveBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(_dtMessageConversationView.CreateDataReader())
                .Verifiable();

            _mockInternalMessagingController.Object.GetArchivedMessages(Constants.UserID_User12, 0, 0);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region GetMessageThread

        [Test]
        public void GetMessageThread_Calls_DataService_GetMessageThread()
        {
            var totalRecords = 0;

            DataService.RegisterInstance(_mockDataService.Object);

            _dtMessageThreadsView.Clear();

            _mockDataService.Setup(ds => ds.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), ref totalRecords)).Returns(_dtMessageThreadsView.CreateDataReader()).Verifiable();
            _mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, "", false, ref totalRecords);
            _mockDataService.Verify();
        }

        [Test]
        public void GetMessageThread_Calls_Overload_With_Default_Values()
        {
            int[] totalRecords = { 0 };
            _mockInternalMessagingController
                .Setup(mc => mc.GetMessageThread(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending, ref totalRecords[0]))
                .Verifiable();

            _mockInternalMessagingController.Object.GetMessageThread(0, 0, 0, 0, ref totalRecords[0]);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region GetRecentSentbox

        [Test]
        public void GetRecentSentbox_Calls_Overload_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetRecentSentbox(Constants.UserID_User12, MessagingController.ConstDefaultPageIndex, MessagingController.ConstDefaultPageSize))
                .Verifiable();

            _mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);

            _mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentSentbox_Calls_GetSentbox_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetSentbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending))
                .Verifiable();

            _mockInternalMessagingController.Object.GetRecentSentbox(Constants.UserID_User12);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region GetSentbox

        [Test]
        public void GetSentbox_Calls_DataService_GetSentBoxView_With_Default_Values()
        {
            DataService.RegisterInstance(_mockDataService.Object);

            _user12UserInfo.PortalID = Constants.PORTAL_Zero;

            _mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(_user12UserInfo);

            _dtMessageConversationView.Clear();

            _mockDataService
                .Setup(ds => ds.GetSentBoxView(
                    Constants.UserID_User12,
                    Constants.PORTAL_Zero,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .Returns(_dtMessageConversationView.CreateDataReader())
                .Verifiable();

            _mockInternalMessagingController.Object.GetSentbox(
                Constants.UserID_User12,
                0,
                0,
                "",
                false,
                MessageReadStatus.Any,
                MessageArchivedStatus.Any);

            _mockDataService.Verify();
        }

        [Test]
        public void GetSentbox_Calls_Overload_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetSentbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived))
                .Verifiable();

            _mockInternalMessagingController.Object.GetSentbox(Constants.UserID_User12, 0, 0, "", false);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region RecipientLimit

        [Test]
        public void RecipientLimit_Returns_MessagingRecipientLimit_Setting()
        {
            const int expected = 10;
            _mockInternalMessagingController
                .Setup(mc => mc.GetPortalSettingAsInteger("MessagingRecipientLimit", Constants.CONTENT_ValidPortalId, It.IsAny<int>()))
                .Returns(expected);

            var actual = _mockInternalMessagingController.Object.RecipientLimit(Constants.CONTENT_ValidPortalId);

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region CreateMessageTests

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Message()
        {
            //Act, Assert
            _messagingController.SendMessage(null, new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Body_And_Subject()
        {
            //Act, Assert
            _messagingController.SendMessage(new Message(), new List<RoleInfo>(), new List<UserInfo>(), new List<int>(), _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Roles_And_Users()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            //Act, Assert
            _messagingController.SendMessage(message, null, null, null, _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Empty_Roles_And_Users_Lists()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            //Act, Assert
            _messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo>(), null, _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Roles_And_Users_With_No_DisplayNames()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };

            //Act, Assert
            _messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Large_Subject()
        {
            //Arrange
            var subject = new StringBuilder();
            for (var i = 0; i <= 40; i++)
            {
                subject.Append("1234567890");
            }

            //Arrange
            var message = new Message { Subject = subject.ToString(), Body = "body" };

            //Act, Assert
            _messagingController.SendMessage(message, new List<RoleInfo> { new RoleInfo() }, new List<UserInfo> { new UserInfo() }, null, _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Large_To()
        {
            //Arrange            
            var message = new Message { Subject = "subject", Body = "body" };
            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>();

            for (var i = 0; i <= 100; i++)
            {
                roles.Add(new RoleInfo { RoleName = "1234567890" });
                users.Add(new UserInfo { DisplayName = "1234567890" });
            }

            //Act, Assert
            _messagingController.SendMessage(message, roles, users, null, _user12UserInfo);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Null_Sender()
        {
            //Arrange            
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            //Act
            _messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_Negative_SenderID()
        {
            //Arrange            
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var sender = new UserInfo { DisplayName = "user11" };

            //Act
            _messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_CreateMessage_Throws_On_SendingToRole_ByNonAdmin()
        {
            //Arrange            
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
			var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            _dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(_dtMessageRecipients.CreateDataReader());

            //Act
            messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _user12UserInfo);
        }

        //[Test]
        [ExpectedException(typeof(AttachmentsNotAllowed))]
        public void MessagingController_CreateMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            //disable caching
            _mockCacheProvider.Setup(mc => mc.GetItem(It.IsAny<string>())).Returns(null);

            _dtPortalSettings.Clear();
            _dtPortalSettings.Rows.Add(Constants.PORTALSETTING_MessagingAllowAttachments_Name, Constants.PORTALSETTING_MessagingAllowAttachments_Value_NO, Constants.CULTURE_EN_US);
            _dataProvider.Setup(d => d.GetPortalSettings(It.IsAny<int>(), It.IsAny<string>())).Returns(_dtPortalSettings.CreateDataReader());

            _dtMessageRecipients.Clear();
            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(_dtMessageRecipients.CreateDataReader());

            //Act
            messagingController.SendMessage(message, null, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _user12UserInfo);
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessage_On_Valid_Message()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.SaveMessage(It.Is<Message>(v => v.PortalID == Constants.PORTAL_Zero && v.Subject == "subject"
                                                                             && v.Body == "body"
                                                                             && v.To == "role1,user1"
                                                                             && v.SenderUserID == _adminUserInfo.UserID)
                                                               , It.IsAny<int>()
                                                               , It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_Filters_Input_When_ProfanityFilter_Is_Enabled()
        {
            //Arrange
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var message = new Message { Subject = "subject", Body = "body" };

            _dtMessageRecipients.Clear();
            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            _mockMessagingController.Setup(mc => mc.InputFilter("subject")).Returns("subject_filtered");
            _mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            _mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));
            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //Assert
            Assert.AreEqual("subject_filtered", message.Subject);
            Assert.AreEqual("body_filtered", message.Body);
        }

        [Test]
        public void MessagingController_CreateMessage_For_CommonUser_Calls_DataService_SaveSocialMessageRecipient_Then_CreateSocialMessageRecipientsForRole()
        {
            //Arrange
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };
            var message = new Message { Subject = "subject", Body = "body" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            //this pattern is based on: http://dpwhelan.com/blog/software-development/moq-sequences/
            var callingSequence = 0;

            //Arrange for Assert
            _mockDataService.Setup(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.EqualTo(0)));
            _mockDataService.Setup(ds => ds.SaveMessageRecipient(It.IsAny<MessageRecipient>(), It.IsAny<int>())).Callback(() => Assert.That(callingSequence++, Is.GreaterThan(0)));

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //SaveMessageRecipient is called twice, one for sent message and second for receive                        
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_One_User()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = Constants.USER_TenName, UserID = Constants.USER_TenId };
            var sender = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_ElevenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, null, sender);

            //Assert
            Assert.AreEqual(message.To, Constants.USER_TenName);
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_Two_Users()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user10 = new UserInfo { DisplayName = Constants.USER_TenName, UserID = Constants.USER_TenId };
            var user11 = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var sender = new UserInfo { DisplayName = Constants.USER_ElevenName, UserID = Constants.USER_ElevenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            _dtMessageRecipients.Clear();
            var recipientId = 0;
            //_dtMessageRecipients.Rows.Add(Constants.Messaging_RecipientId_2, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>()))
                .Callback(() => _dtMessageRecipients.Rows.Add(recipientId++, Constants.USER_Null, Constants.Messaging_UnReadMessage, Constants.Messaging_UnArchivedMessage))
                .Returns(() => _dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user10, user11 }, null, sender);

            //Assert
            Assert.AreEqual(message.To, Constants.USER_TenName + "," + Constants.USER_ElevenName);
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_One_Role()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_Administrators };

            _dtMessageRecipients.Clear();
            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo>(), null, _adminUserInfo);

            //Assert
            Assert.AreEqual(message.To, Constants.RoleName_Administrators);
        }

        [Test]
        public void MessagingController_CreateMessage_Trims_Comma_For_Two_Roles()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role1 = new RoleInfo { RoleName = Constants.RoleName_Administrators };
            var role2 = new RoleInfo { RoleName = Constants.RoleName_Subscribers };

            _dtMessageRecipients.Clear();
            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), It.IsAny<int>())).Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo>(), null, _adminUserInfo);

            //Assert
            Assert.AreEqual(message.To, Constants.RoleName_Administrators + "," + Constants.RoleName_Subscribers);
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageAttachment_On_Passing_Attachments()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.SaveMessageAttachment(It.Is<MessageAttachment>(v => v.MessageID == message.MessageID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByAdmin()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _user12UserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { _user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers.ToString(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Role_ByHost()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var role = new RoleInfo { RoleName = Constants.RoleName_RegisteredUsers, RoleID = Constants.RoleID_RegisteredUsers };

            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _user12UserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _hostUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            messagingController.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { _user12UserInfo }, new List<int> { Constants.FOLDER_ValidFileId }, _hostUserInfo);

            //Assert
            mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers.ToString(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Roles()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role1 = new RoleInfo { RoleName = "role1", RoleID = Constants.RoleID_RegisteredUsers };
            var role2 = new RoleInfo { RoleName = "role2", RoleID = Constants.RoleID_Administrators };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role1, role2 }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(message.MessageID, Constants.RoleID_RegisteredUsers + "," + Constants.RoleID_Administrators, It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_CreateSocialMessageRecipientsForRole_On_Passing_Roles_ByRoleOwner()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = Constants.RoleName_FirstSocialGroup, RoleID = Constants.RoleID_FirstSocialGroup };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockInternalMessagingController.Setup(mc => mc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>()));

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _groupOwnerUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.CreateMessageRecipientsForRole(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Calls_DataService_SaveSocialMessageRecipient_On_Passing_Users()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == user.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Sets_ReplyAll_To_False_On_Passing_Roles()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1", RoleID = Constants.RoleID_RegisteredUsers };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _adminUserInfo);

            //Assert
            Assert.AreEqual(message.ReplyAllAllowed, false);
        }

        [Test]
        public void MessagingController_CreateMessage_Sets_ReplyAll_To_True_On_Passing_User()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var mockDataService = new Mock<IDataService>();
            var messagingController = new MessagingController(mockDataService.Object);

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            messagingController.SendMessage(message, new List<RoleInfo>(), new List<UserInfo> { user }, new List<int> { Constants.FOLDER_ValidFileId }, _adminUserInfo);

            //Assert
            Assert.AreEqual(message.ReplyAllAllowed, true);
        }

        [Test]
        public void MessagingController_CreateMessage_Adds_Sender_As_Recipient_When_Not_Aready_A_Recipient()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //Assert
            _mockDataService.Verify(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(v => v.MessageID == message.MessageID && v.UserID == _adminUserInfo.UserID), It.IsAny<int>()));
        }

        [Test]
        public void MessagingController_CreateMessage_Marks_Message_As_Dispatched_For_Sender_When_Not_Already_A_Recipient()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            _mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == _adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user }, null, _adminUserInfo);

            //Assert
            _mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        [Test]
        public void MessagingController_CreateMessage_Does_Not_Mark_Message_As_Dispatched_For_Sender_When_Already_A_Recipient()
        {
            //Arrange
            var message = new Message { Subject = "subject", Body = "body" };
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };
            var role = new RoleInfo { RoleName = "role1" };

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), Constants.USER_TenId))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockDataService.Setup(md => md.GetMessageRecipientByMessageAndUser(It.IsAny<int>(), _adminUserInfo.UserID))
                .Callback(SetupDataTables)
                .Returns(_dtMessageRecipients.CreateDataReader());

            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(ims => ims.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns((MessageRecipient)null);

            _mockDataService.Setup(ds => ds.SaveMessageRecipient(It.Is<MessageRecipient>(mr => mr.UserID == _adminUserInfo.UserID), It.IsAny<int>())).Returns(Constants.Messaging_RecipientId_1);

            //Act
            _mockMessagingController.Object.SendMessage(message, new List<RoleInfo> { role }, new List<UserInfo> { user, _adminUserInfo }, null, _adminUserInfo);

            //Assert
            _mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1), Times.Never());
        }

        #endregion

        #region ReplyMessageTests

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Null_Sender()
        {
            //Arrange

            //Act
            _internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Negative_SenderID()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11" };

            //Act            
            _internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Null_Subject()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            //Act, Assert
            _internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, null, null, sender);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void MessagingController_ReplyMessage_Throws_On_Empty_Subject()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId };

            //Act, Assert
            _internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "", null, sender);
        }

        [Test]
        [ExpectedException(typeof(AttachmentsNotAllowed))]
        public void MessagingController_ReplyMessage_Throws_On_Passing_Attachments_When_Its_Not_Enabled()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            InternalMessagingController.SetTestableInstance(_mockInternalMessagingController.Object);
            _mockInternalMessagingController.Setup(imc => imc.AttachmentsAllowed(Constants.PORTAL_Zero)).Returns(false);

            //Act, Assert
            _internalMessagingController.ReplyMessage(Constants.Messaging_MessageId_1, "body", new List<int> { Constants.FOLDER_ValidFileId }, sender);
        }

        [Test]
        public void MessagingController_ReplyMessage_Filters_Input_When_ProfanityFilter_Is_Enabled()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(_mockDataService.Object);

            _mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            _mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            _mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("YES");

            _mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            _mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            _mockDataService.Verify(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        [ExpectedException(typeof(MessageOrRecipientNotFoundException))]
        public void MessagingController_ReplyMessage_Throws_When_Message_Or_Recipient_Are_Not_Found()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(_mockDataService.Object);

            _mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            _mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            _mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            _mockDataService.Setup(ds => ds.CreateMessageReply(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>())).Returns(-1);

            _mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Read_By_Sender()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(_mockDataService.Object);

            _mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            _mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            _mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            _mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient());

            _mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            _mockInternalMessagingController.Verify(imc => imc.MarkRead(It.IsAny<int>(), sender.UserID));
        }

        [Test]
        public void MessagingController_ReplyMessage_Marks_Message_As_Dispatched_For_Sender()
        {
            //Arrange
            var sender = new UserInfo { DisplayName = "user11", UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };

            DataService.RegisterInstance(_mockDataService.Object);

            _mockMessagingController.Setup(mc => mc.InputFilter("body")).Returns("body_filtered");
            _mockMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(new UserInfo());

            _mockMessagingController
                .Setup(mc => mc.GetPortalSetting("MessagingProfanityFilters", It.IsAny<int>(), It.IsAny<string>()))
                .Returns("NO");

            _mockInternalMessagingController.Setup(imc => imc.GetMessageRecipient(It.IsAny<int>(), It.IsAny<int>())).Returns(new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_1 });

            _mockInternalMessagingController.Object.ReplyMessage(0, "body", null, sender);

            _mockInternalMessagingController.Verify(imc => imc.MarkMessageAsDispatched(It.IsAny<int>(), Constants.Messaging_RecipientId_1));
        }

        #endregion

        #region Setting Message Status Tests

        [Test]
        public void MessagingController_SetReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            //Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            //Act
            _internalMessagingController.MarkRead(messageInstance.ConversationId, user.UserID);

            //Assert
            _mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnReadMessage_Calls_DataService_UpdateSocialMessageReadStatus()
        {
            //Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            //Act
            _internalMessagingController.MarkUnRead(messageInstance.ConversationId, user.UserID);

            //Assert
            _mockDataService.Verify(ds => ds.UpdateMessageReadStatus(messageInstance.ConversationId, user.UserID, false));
        }

        [Test]
        public void MessagingController_SetArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            //Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            //Act
            _internalMessagingController.MarkArchived(messageInstance.ConversationId, user.UserID);

            //Assert
            _mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, true));
        }

        [Test]
        public void MessagingController_SetUnArchivedMessage_Calls_DataService_UpdateSocialMessageArchivedStatus()
        {
            //Arrange
            var messageInstance = CreateValidMessage();
            var user = new UserInfo { DisplayName = "user1", UserID = Constants.USER_TenId };

            //Act
            _internalMessagingController.MarkUnArchived(messageInstance.ConversationId, user.UserID);

            //Assert
            _mockDataService.Verify(ds => ds.UpdateMessageArchivedStatus(messageInstance.ConversationId, user.UserID, false));
        }

        #endregion

        #region GetMessageRecipient

        [Test]
        public void GetSocialMessageRecipient_Calls_DataService_GetSocialMessageRecipientByMessageAndUser()
        {
            _dtMessageRecipients.Clear();
            _mockDataService
                .Setup(ds => ds.GetMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12))
                .Returns(_dtMessageRecipients.CreateDataReader())
                .Verifiable();

            _internalMessagingController.GetMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);

            _mockDataService.Verify();
        }

        #endregion

        #region WaitTimeForNextMessage

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WaitTimeForNextMessage_Throws_On_Null_Sender()
        {
            _internalMessagingController.WaitTimeForNextMessage(null);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_MessagingThrottlingInterval_Is_Zero()
        {
            _user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            _mockMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), _user12UserInfo.PortalID, Null.NullInteger)).Returns(0);

            var result = _mockInternalMessagingController.Object.WaitTimeForNextMessage(_user12UserInfo);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_Sender_Is_Admin_Or_Host()
        {
            _adminUserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            _mockMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), _adminUserInfo.PortalID, Null.NullInteger)).Returns(1);
            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(true);

            var result = _mockInternalMessagingController.Object.WaitTimeForNextMessage(_adminUserInfo);

            Assert.AreEqual(0, result);
        }

        [Test]
        public void WaitTimeForNextMessage_Returns_Zero_When_The_User_Has_No_Previous_Conversations()
        {
            _user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;

            _mockMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), _user12UserInfo.PortalID, Null.NullInteger)).Returns(1);
            _mockMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(false);

            _mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(_user12UserInfo)).Returns((Message) null);

            var result = _mockInternalMessagingController.Object.WaitTimeForNextMessage(_user12UserInfo);

            Assert.AreEqual(0, result);
        }

        [Test]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 1, 0)]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 2, 60)]
        [TestCase("2/16/2012 12:15:12 PM", "2/16/2012 12:14:12 PM", 10, 540)]
        public void WaitTimeForNextMessage_Returns_The_Number_Of_Seconds_Since_Last_Message_Sent(string actualDateString, string lastMessageDateString, int throttlingInterval, int expected)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            var actualDate = DateTime.Parse(actualDateString, culture);
            var lastMessageDate = DateTime.Parse(lastMessageDateString, culture);
            _user12UserInfo.PortalID = Constants.CONTENT_ValidPortalId;
            _mockInternalMessagingController.Setup(mc => mc.GetPortalSettingAsInteger(It.IsAny<string>(), _user12UserInfo.PortalID, Null.NullInteger)).Returns(throttlingInterval);
            _mockInternalMessagingController.Setup(mc => mc.IsAdminOrHost(_adminUserInfo)).Returns(false);
            _dtMessages.Clear();
            _dtMessages.Rows.Add(-1, 1, "", "", "", "", -1, -1, -1, -1, lastMessageDate, -1, Null.NullDate);
            var dr = _dtMessages.CreateDataReader();
            var message = CBO.FillObject<Message>(dr);
            _mockInternalMessagingController.Setup(mc => mc.GetLastSentMessage(_user12UserInfo)).Returns(message);
            _mockInternalMessagingController.Setup(mc => mc.GetDateTimeNow()).Returns(actualDate);
            var result = _mockInternalMessagingController.Object.WaitTimeForNextMessage(_user12UserInfo);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region GetInbox

        [Test]
        public void GetInbox_Calls_DataService_GetMessageBoxView()
        {
            DataService.RegisterInstance(_mockDataService.Object);

            _dtMessageConversationView.Clear();

            _user12UserInfo.PortalID = Constants.PORTAL_Zero;

            _mockInternalMessagingController.Setup(mc => mc.GetCurrentUserInfo()).Returns(_user12UserInfo);
            _mockDataService.Setup(ds => ds.GetInBoxView(It.IsAny<int>(), Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.Any, MessageSentStatus.Received)).Returns(_dtMessageConversationView.CreateDataReader()).Verifiable();
            _mockInternalMessagingController.Object.GetInbox(0, 0, 0, "", false, MessageReadStatus.Any, MessageArchivedStatus.Any);
            _mockDataService.Verify();
        }

        [Test]
        public void GetInbox_Calls_Overload_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetInbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), MessageReadStatus.Any, MessageArchivedStatus.UnArchived))
                .Verifiable();

            _mockInternalMessagingController.Object.GetInbox(Constants.UserID_User12, 0, 0, "", false);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region GetRecentInbox

        [Test]
        public void GetRecentInbox_Calls_GetInbox_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetInbox(Constants.UserID_User12, It.IsAny<int>(), It.IsAny<int>(), MessagingController.ConstSortColumnDate, !MessagingController.ConstAscending))
                .Verifiable();

            _mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12, 0, 0);

            _mockInternalMessagingController.Verify();
        }

        [Test]
        public void GetRecentInbox_Calls_Overload_With_Default_Values()
        {
            _mockInternalMessagingController
                .Setup(mc => mc.GetRecentInbox(Constants.UserID_User12, MessagingController.ConstDefaultPageIndex, MessagingController.ConstDefaultPageSize))
                .Verifiable();

            _mockInternalMessagingController.Object.GetRecentInbox(Constants.UserID_User12);

            _mockInternalMessagingController.Verify();
        }

        #endregion

        #region CountArchivedMessagesByConversation

        [Test]
        public void CountArchivedMessagesByConversation_Calls_DataService_CountArchivedMessagesByConversation()
        {
            _mockDataService.Setup(ds => ds.CountArchivedMessagesByConversation(It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountArchivedMessagesByConversation(1);
            _mockDataService.Verify();
        }

        #endregion

        #region CountMessagesByConversation

        [Test]
        public void CountMessagesByConversation_Calls_DataService_CountMessagesByConversation()
        {
            _mockDataService.Setup(ds => ds.CountMessagesByConversation(It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountMessagesByConversation(1);
            _mockDataService.Verify();
        }

        #endregion

        #region CountConversations

        [Test]
        public void CountConversations_Calls_DataService_CountTotalConversations()
        {
            _mockDataService.Setup(ds => ds.CountTotalConversations(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountConversations(Constants.UserID_User12, Constants.PORTAL_Zero);
            _mockDataService.Verify();
        }

        #endregion

        #region CountUnreadMessages

        [Test]
        public void CountUnreadMessages_Calls_DataService_CountNewThreads()
        {
            _mockDataService.Setup(ds => ds.CountNewThreads(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountUnreadMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            _mockDataService.Verify();
        }

        #endregion

        #region CountSentMessages

        [Test]
        public void CountSentMessages_Calls_DataService_CountSentMessages()
        {
            _mockDataService.Setup(ds => ds.CountSentMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountSentMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            _mockDataService.Verify();
        }

        #endregion

        #region CountArchivedMessages

        [Test]
        public void CountArchivedMessages_Calls_DataService_CountArchivedMessages()
        {
            _mockDataService.Setup(ds => ds.CountArchivedMessages(It.IsAny<int>(), It.IsAny<int>())).Verifiable();
            _internalMessagingController.CountArchivedMessages(Constants.UserID_User12, Constants.PORTAL_Zero);
            _mockDataService.Verify();
        }

        #endregion

        #region DeleteMessageRecipient

        [Test]
        public void DeleteMessageRecipient_Calls_DataService_DeleteMessageRecipientByMessageAndUser()
        {
            _mockDataService.Setup(ds => ds.DeleteMessageRecipientByMessageAndUser(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            _internalMessagingController.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            _mockDataService.Verify();
        }

        #endregion

        #region GetMessageRecipients

        [Test]
        public void GetMessageRecipients_Calls_DataService_GetMessageRecipientsByMessage()
        {
            _dtMessageRecipients.Clear();

            _mockDataService
                .Setup(ds => ds.GetMessageRecipientsByMessage(Constants.Messaging_MessageId_1))
                .Returns(_dtMessageRecipients.CreateDataReader())
                .Verifiable();
            _internalMessagingController.GetMessageRecipients(Constants.Messaging_MessageId_1);
            _mockDataService.Verify();
        }

        #endregion

        #endregion

        #region "Private Methods"

        private static Message CreateValidMessage()
        {
            var message = new Message
            {
                MessageID = 2,
                Subject = "test",
                Body = "body",
                ConversationId = 1,
                ReplyAllAllowed = false,
                SenderUserID = 1
            };
            return message;
        }

        private void SetupDataTables()
        {
            //Messages
            _dtMessages = new DataTable("Messages");
            var pkMessagesMessageID = _dtMessages.Columns.Add("MessageID", typeof(int));
            _dtMessages.Columns.Add("PortalId", typeof(int));
            _dtMessages.Columns.Add("To", typeof(string));
            _dtMessages.Columns.Add("From", typeof(string));
            _dtMessages.Columns.Add("Subject", typeof(string));
            _dtMessages.Columns.Add("Body", typeof(string));
            _dtMessages.Columns.Add("ConversationId", typeof(int));
            _dtMessages.Columns.Add("ReplyAllAllowed", typeof(bool));
            _dtMessages.Columns.Add("SenderUserID", typeof(int));
            _dtMessages.Columns.Add("CreatedByUserID", typeof(int));
            _dtMessages.Columns.Add("CreatedOnDate", typeof(DateTime));
            _dtMessages.Columns.Add("LastModifiedByUserID", typeof(int));
            _dtMessages.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            _dtMessages.PrimaryKey = new[] { pkMessagesMessageID };

            //MessageRecipients
            _dtMessageRecipients = new DataTable("MessageRecipients");
            var pkMessageRecipientID = _dtMessageRecipients.Columns.Add("RecipientID", typeof(int));
            _dtMessageRecipients.Columns.Add("MessageID", typeof(int));
            _dtMessageRecipients.Columns.Add("UserID", typeof(int));
            _dtMessageRecipients.Columns.Add("Read", typeof(bool));
            _dtMessageRecipients.Columns.Add("Archived", typeof(bool));
            _dtMessageRecipients.Columns.Add("CreatedByUserID", typeof(int));
            _dtMessageRecipients.Columns.Add("CreatedOnDate", typeof(DateTime));
            _dtMessageRecipients.Columns.Add("LastModifiedByUserID", typeof(int));
            _dtMessageRecipients.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            _dtMessageRecipients.PrimaryKey = new[] { pkMessageRecipientID };

            //MessageAttachments
            _dtMessageAttachment = new DataTable("MessageAttachments");
            var pkMessageAttachmentID = _dtMessageAttachment.Columns.Add("MessageAttachmentID", typeof(int));
            _dtMessageAttachment.Columns.Add("MessageID", typeof(int));
            _dtMessageAttachment.Columns.Add("FileID", typeof(int));
            _dtMessageAttachment.Columns.Add("CreatedByUserID", typeof(int));
            _dtMessageAttachment.Columns.Add("CreatedOnDate", typeof(DateTime));
            _dtMessageAttachment.Columns.Add("LastModifiedByUserID", typeof(int));
            _dtMessageAttachment.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            _dtMessageAttachment.PrimaryKey = new[] { pkMessageAttachmentID };

            //Portal Settings
            _dtPortalSettings = new DataTable("PortalSettings");
            _dtPortalSettings.Columns.Add("SettingName", typeof(string));
            _dtPortalSettings.Columns.Add("SettingValue", typeof(string));
            _dtPortalSettings.Columns.Add("CultureCode", typeof(string));
            _dtPortalSettings.Columns.Add("CreatedByUserID", typeof(int));
            _dtPortalSettings.Columns.Add("CreatedOnDate", typeof(DateTime));
            _dtPortalSettings.Columns.Add("LastModifiedByUserID", typeof(int));
            _dtPortalSettings.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            //Message Conversation
            _dtMessageConversationView = new DataTable();
            _dtMessageConversationView.Columns.Add("RowNumber", typeof(int));
            _dtMessageConversationView.Columns.Add("AttachmentCount", typeof(int));
            _dtMessageConversationView.Columns.Add("NewThreadCount", typeof(int));
            _dtMessageConversationView.Columns.Add("ThreadCount", typeof(int));
            _dtMessageConversationView.Columns.Add("MessageID", typeof(int));
            _dtMessageConversationView.Columns.Add("To", typeof(string));
            _dtMessageConversationView.Columns.Add("From", typeof(string));
            _dtMessageConversationView.Columns.Add("Subject", typeof(string));
            _dtMessageConversationView.Columns.Add("Body", typeof(string));
            _dtMessageConversationView.Columns.Add("ConversationID", typeof(int));
            _dtMessageConversationView.Columns.Add("ReplyAllAllowed", typeof(bool));
            _dtMessageConversationView.Columns.Add("SenderUserID", typeof(int));
            _dtMessageConversationView.Columns.Add("CreatedByUserID", typeof(int));
            _dtMessageConversationView.Columns.Add("CreatedOnDate", typeof(DateTime));
            _dtMessageConversationView.Columns.Add("LastModifiedByUserID", typeof(int));
            _dtMessageConversationView.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            //Thread View
            _dtMessageThreadsView = new DataTable();
            _dtMessageThreadsView.Columns.Add("TotalThreads", typeof(int));
            _dtMessageThreadsView.Columns.Add("TotalNewThreads", typeof(int));
            _dtMessageThreadsView.Columns.Add("TotalArchivedThreads", typeof(int));
        }

        #endregion
    }
}
// ReSharper restore InconsistentNaming