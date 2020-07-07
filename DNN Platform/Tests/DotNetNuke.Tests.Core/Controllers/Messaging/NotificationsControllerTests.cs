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
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Exceptions;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Social.Notifications.Data;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class NotificationsControllerTests
    {
        private Mock<IDataService> _mockDataService;
        private Mock<IPortalController> _portalController;
        private Mock<IPortalGroupController> _portalGroupController;
        private Mock<DotNetNuke.Services.Social.Messaging.Data.IDataService> _mockMessagingDataService;
        private Mock<IMessagingController> _mockMessagingController;
        private Mock<IInternalMessagingController> _mockInternalMessagingController;
        private NotificationsController _notificationsController;
        private Mock<NotificationsController> _mockNotificationsController;
        private Mock<DataProvider> _dataProvider;
        private Mock<CachingProvider> _cachingProvider;
        private DataTable _dtNotificationTypes;
        private DataTable _dtNotificationTypeActions;
        private DataTable _dtNotificationActions;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();

            this._mockDataService = new Mock<IDataService>();
            this._portalController = new Mock<IPortalController>();
            this._portalGroupController = new Mock<IPortalGroupController>();

            this._mockMessagingDataService = new Mock<DotNetNuke.Services.Social.Messaging.Data.IDataService>();
            this._dataProvider = MockComponentProvider.CreateDataProvider();
            this._cachingProvider = MockComponentProvider.CreateDataCacheProvider();

            this._notificationsController = new NotificationsController(this._mockDataService.Object, this._mockMessagingDataService.Object);
            this._mockNotificationsController = new Mock<NotificationsController> { CallBase = true };

            this._mockMessagingController = new Mock<IMessagingController>();
            MessagingController.SetTestableInstance(this._mockMessagingController.Object);
            PortalController.SetTestableInstance(this._portalController.Object);
            PortalGroupController.RegisterInstance(this._portalGroupController.Object);

            this._mockInternalMessagingController = new Mock<IInternalMessagingController>();
            InternalMessagingController.SetTestableInstance(this._mockInternalMessagingController.Object);

            DataService.RegisterInstance(this._mockDataService.Object);
            DotNetNuke.Services.Social.Messaging.Data.DataService.RegisterInstance(this._mockMessagingDataService.Object);

            this.SetupDataProvider();
            this.SetupDataTables();
        }

        [TearDown]
        public void TearDown()
        {
            ComponentFactory.Container = null;
            MessagingController.ClearInstance();
            PortalController.ClearInstance();
            InternalMessagingController.ClearInstance();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNotificationType_Throws_On_Null_NotificationType()
        {
            this._notificationsController.CreateNotificationType(null);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNotificationType_Throws_On_Null_Or_Empty_Name(string name)
        {
            var notificationType = CreateNewNotificationType();
            notificationType.Name = name;

            this._notificationsController.CreateNotificationType(notificationType);
        }

        [Test]
        public void CreateNotificationType_Calls_DataService_CreateNotificationType()
        {
            this._mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this._mockDataService
                .Setup(ds => ds.CreateNotificationType(Constants.Messaging_NotificationTypeName, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), Constants.UserID_User12, It.IsAny<bool>()))
                .Verifiable();

            this._mockNotificationsController.Object.CreateNotificationType(CreateNewNotificationType());

            this._mockDataService.Verify();
        }

        [Test]
        [TestCase(int.MaxValue, int.MaxValue)]
        [TestCase(1, 1)]
        [TestCase(0, 0)]
        public void CreateNotificationType_Returns_Object_With_Valid_TimeToLive(int actualTimeToLiveTotalMinutes, int expectedTimeToLiveTotalMinutes)
        {
            var actualTimeToLive = TimeSpan.FromMinutes(actualTimeToLiveTotalMinutes);

            var notificationType = CreateNewNotificationType();
            notificationType.TimeToLive = actualTimeToLive;
            this._notificationsController.CreateNotificationType(notificationType);

            Assert.AreEqual(expectedTimeToLiveTotalMinutes, (int)notificationType.TimeToLive.TotalMinutes);
        }

        [Test]
        public void CreateNotificationType_Makes_Valid_Object()
        {
            var expectedNotificationType = CreateValidNotificationType();

            this._mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this._mockDataService
                .Setup(ds => ds.CreateNotificationType(
                    expectedNotificationType.Name,
                    expectedNotificationType.Description,
                    (int)expectedNotificationType.TimeToLive.TotalMinutes,
                    expectedNotificationType.DesktopModuleId,
                    Constants.UserID_User12, false))
                .Returns(Constants.Messaging_NotificationTypeId);

            var actualNotificationType = CreateNewNotificationType();
            this._mockNotificationsController.Object.CreateNotificationType(actualNotificationType);

            Assert.IsTrue(new NotificationTypeComparer().Equals(expectedNotificationType, actualNotificationType));
        }

        [Test]
        public void DeleteNotificationType_Calls_DataService_DeleteNotificationType()
        {
            this._mockDataService.Setup(ds => ds.DeleteNotificationType(Constants.Messaging_NotificationTypeId)).Verifiable();
            this._mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeCache());
            this._mockNotificationsController.Object.DeleteNotificationType(Constants.Messaging_NotificationTypeId);
            this._mockDataService.Verify();
        }

        [Test]
        public void DeleteNotificationType_Removes_Cache_Object()
        {
            this._mockDataService.Setup(ds => ds.DeleteNotificationType(Constants.Messaging_NotificationTypeId));
            this._mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeCache()).Verifiable();
            this._mockNotificationsController.Object.DeleteNotificationType(Constants.Messaging_NotificationTypeId);
            this._mockNotificationsController.Verify();
        }

        [Test]
        public void GetNotificationType_By_Id_Gets_Object_From_Cache()
        {
            this._cachingProvider.Object.PurgeCache();
            this._cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);
            this._cachingProvider.Verify();
        }

        [Test]
        public void GetNotificationType_By_Id_Calls_DataService_GetNotificationType_When_Object_Is_Not_In_Cache()
        {
            this._cachingProvider.Object.PurgeCache();

            var messageTypeDataTable = new DataTable();

            this._mockDataService
                .Setup(ds => ds.GetNotificationType(Constants.Messaging_NotificationTypeId))
                .Returns(messageTypeDataTable.CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotificationType_By_Id_Returns_Valid_Object()
        {
            this._cachingProvider.Object.PurgeCache();

            var expectedNotificationType = CreateValidNotificationType();

            this._dtNotificationTypes.Rows.Clear();

            this._dtNotificationTypes.Rows.Add(
                expectedNotificationType.NotificationTypeId,
                expectedNotificationType.Name,
                expectedNotificationType.Description,
                (int)expectedNotificationType.TimeToLive.TotalMinutes,
                expectedNotificationType.DesktopModuleId);

            this._mockDataService
                .Setup(ds => ds.GetNotificationType(Constants.Messaging_NotificationTypeId))
                .Returns(this._dtNotificationTypes.CreateDataReader());

            var actualNotificationType = this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeId);

            Assert.IsTrue(new NotificationTypeComparer().Equals(expectedNotificationType, actualNotificationType));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNotificationType_By_Name_Throws_On_Null_Or_Empty_Name(string name)
        {
            this._notificationsController.GetNotificationType(name);
        }

        [Test]
        public void GetNotificationType_By_Name_Gets_Object_From_Cache()
        {
            this._cachingProvider.Object.PurgeCache();
            this._cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);
            this._cachingProvider.Verify();
        }

        [Test]
        public void GetNotificationType_By_Name_Calls_DataService_GetNotificationTypeByName_When_Object_Is_Not_In_Cache()
        {
            this._cachingProvider.Object.PurgeCache();

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeByName(Constants.Messaging_NotificationTypeName))
                .Returns(this._dtNotificationTypes.CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotificationType_By_Name_Returns_Valid_Object()
        {
            this._cachingProvider.Object.PurgeCache();

            var expectedNotificationType = CreateValidNotificationType();

            this._dtNotificationTypes.Rows.Clear();

            this._dtNotificationTypes.Rows.Add(
                expectedNotificationType.NotificationTypeId,
                expectedNotificationType.Name,
                expectedNotificationType.Description,
                (int)expectedNotificationType.TimeToLive.TotalMinutes,
                expectedNotificationType.DesktopModuleId);

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeByName(Constants.Messaging_NotificationTypeName))
                .Returns(this._dtNotificationTypes.CreateDataReader());

            var actualNotificationType = this._notificationsController.GetNotificationType(Constants.Messaging_NotificationTypeName);

            Assert.IsTrue(new NotificationTypeComparer().Equals(expectedNotificationType, actualNotificationType));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetNotificationTypeActions_Throws_On_Null()
        {
            this._notificationsController.SetNotificationTypeActions(null, Constants.Messaging_NotificationTypeId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetNotificationTypeActions_Throws_On_EmptyList()
        {
            this._notificationsController.SetNotificationTypeActions(new List<NotificationTypeAction>(), Constants.Messaging_NotificationTypeId);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void SetNotificationTypeActions_Throws_On_Null_Or_Empty_Name(string name)
        {
            var action = CreateNewNotificationTypeAction();
            action.NameResourceKey = name;
            this._notificationsController.SetNotificationTypeActions(new[] { action }, Constants.Messaging_NotificationTypeId);
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void SetNotificationTypeActions_Throws_On_Null_Or_Empty_APICall(string apiCall)
        {
            var action = CreateNewNotificationTypeAction();
            action.APICall = apiCall;
            this._notificationsController.SetNotificationTypeActions(new[] { action }, Constants.Messaging_NotificationTypeId);
        }

        [Test]
        public void SetNotificationTypeActions_Calls_DataService_AddNotificationTypeAction_For_Each_Of_Two_Actions()
        {
            this._mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            // _mockDataService
            //                .Setup(ds => ds.AddNotificationTypeAction(
            //                    Constants.Messaging_NotificationTypeId,
            //                    Constants.Messaging_NotificationTypeActionNameResourceKey,
            //                    Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
            //                    Constants.Messaging_NotificationTypeActionConfirmResourceKey,
            //                    Constants.Messaging_NotificationTypeActionAPICall,
            //                    Constants.UserID_User12))
            //                .Verifiable();
            this._mockNotificationsController.Setup(nc => nc.GetNotificationTypeAction(It.IsAny<int>()));

            this._mockNotificationsController.Object.SetNotificationTypeActions(
                new[] { CreateNewNotificationTypeAction(), CreateNewNotificationTypeAction() },
                Constants.Messaging_NotificationTypeId);

            this._mockDataService.Verify(
                x => x.AddNotificationTypeAction(
                Constants.Messaging_NotificationTypeId,
                Constants.Messaging_NotificationTypeActionNameResourceKey,
                Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
                Constants.Messaging_NotificationTypeActionConfirmResourceKey,
                Constants.Messaging_NotificationTypeActionAPICall,
                Constants.UserID_User12), Times.Exactly(2));
        }

        [Test]
        public void SetNotificationTypeActions_Sets_NotificationTypeActionId_And_NotificationTypeId()
        {
            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

            this._mockNotificationsController.Setup(nc => nc.GetCurrentUserId()).Returns(Constants.UserID_User12);

            this._mockDataService
                .Setup(ds => ds.AddNotificationTypeAction(
                    expectedNotificationTypeAction.NotificationTypeId,
                    expectedNotificationTypeAction.NameResourceKey,
                    expectedNotificationTypeAction.DescriptionResourceKey,
                    expectedNotificationTypeAction.ConfirmResourceKey,
                    expectedNotificationTypeAction.APICall,
                    Constants.UserID_User12))
                .Returns(expectedNotificationTypeAction.NotificationTypeActionId);

            this._mockNotificationsController
                .Setup(nc => nc.GetNotificationTypeAction(expectedNotificationTypeAction.NotificationTypeActionId))
                .Returns(expectedNotificationTypeAction);

            var action = CreateNewNotificationTypeAction();
            this._mockNotificationsController.Object.SetNotificationTypeActions(new[] { action }, expectedNotificationTypeAction.NotificationTypeId);

            Assert.IsTrue(new NotificationTypeActionComparer().Equals(expectedNotificationTypeAction, action));
        }

        [Test]
        public void DeleteNotificationTypeAction_Calls_DataService_DeleteNotificationTypeAction()
        {
            this._mockDataService.Setup(ds => ds.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId)).Verifiable();
            this._mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeActionCache());
            this._mockNotificationsController.Object.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this._mockDataService.Verify();
        }

        [Test]
        public void DeleteNotificationTypeAction_Removes_Cache_Object()
        {
            this._mockDataService.Setup(ds => ds.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId));
            this._mockNotificationsController.Setup(nc => nc.RemoveNotificationTypeActionCache()).Verifiable();
            this._mockNotificationsController.Object.DeleteNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this._mockNotificationsController.Verify();
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Gets_Object_From_Cache()
        {
            this._cachingProvider.Object.PurgeCache();
            this._cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);
            this._cachingProvider.Verify();
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Calls_DataService_GetNotificationTypeAction_When_Object_Is_Not_In_Cache()
        {
            this._cachingProvider.Object.PurgeCache();

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId))
                .Returns(this._dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotificationTypeAction_By_Id_Returns_Valid_Object()
        {
            this._cachingProvider.Object.PurgeCache();

            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

            this._dtNotificationTypeActions.Clear();

            this._dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId))
                .Returns(this._dtNotificationTypeActions.CreateDataReader);

            var actualNotificationTypeAction = this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeActionId);

            Assert.IsTrue(new NotificationTypeActionComparer().Equals(expectedNotificationTypeAction, actualNotificationTypeAction));
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [ExpectedException(typeof(ArgumentException))]
        public void GetNotificationTypeAction_By_Name_Throws_On_Null_Or_Empty_Name(string name)
        {
            this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, name);
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Gets_Object_From_Cache()
        {
            this._cachingProvider.Object.PurgeCache();
            this._cachingProvider.Setup(cp => cp.GetItem(It.IsAny<string>())).Verifiable();
            this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);
            this._cachingProvider.Verify();
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Calls_DataService_GetNotificationTypeActionByName_When_Object_Is_Not_In_Cache()
        {
            this._cachingProvider.Object.PurgeCache();

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeActionByName(
                    Constants.Messaging_NotificationTypeId,
                    Constants.Messaging_NotificationTypeActionNameResourceKey))
                .Returns(this._dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotificationTypeAction_By_Name_Returns_Valid_Object()
        {
            this._cachingProvider.Object.PurgeCache();

            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

            this._dtNotificationTypeActions.Clear();

            this._dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeActionByName(
                    Constants.Messaging_NotificationTypeId,
                    Constants.Messaging_NotificationTypeActionNameResourceKey))
                .Returns(this._dtNotificationTypeActions.CreateDataReader());

            var actualNotificationTypeAction = this._notificationsController.GetNotificationTypeAction(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationTypeActionNameResourceKey);

            Assert.IsTrue(new NotificationTypeActionComparer().Equals(expectedNotificationTypeAction, actualNotificationTypeAction));
        }

        [Test]
        public void GetNotificationTypeActions_Calls_DataService_GetNotificationTypeActions()
        {
            this._mockDataService
                .Setup(ds => ds.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId))
                .Returns(this._dtNotificationTypeActions.CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId);

            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotificationTypeActions_Returns_Valid_Object()
        {
            var expectedNotificationTypeAction = CreateValidNotificationTypeAction();

            this._dtNotificationTypeActions.Clear();

            this._dtNotificationTypeActions.Rows.Add(
                expectedNotificationTypeAction.NotificationTypeActionId,
                expectedNotificationTypeAction.NotificationTypeId,
                expectedNotificationTypeAction.NameResourceKey,
                expectedNotificationTypeAction.DescriptionResourceKey,
                expectedNotificationTypeAction.ConfirmResourceKey,
                expectedNotificationTypeAction.Order,
                expectedNotificationTypeAction.APICall);

            this._mockDataService.Setup(ds => ds.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId)).Returns(this._dtNotificationTypeActions.CreateDataReader());

            var actualNotificationTypeActions = this._notificationsController.GetNotificationTypeActions(Constants.Messaging_NotificationTypeId);

            Assert.AreEqual(1, actualNotificationTypeActions.Count);
            Assert.IsTrue(new NotificationTypeActionComparer().Equals(expectedNotificationTypeAction, actualNotificationTypeActions[0]));
        }

        [Test]
        public void SendNotification_Sets_Empty_SenderUserId_With_Admin()
        {
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.CONTENT_ValidPortalId,
            };

            this._mockNotificationsController.Setup(nc => nc.GetAdminUser()).Returns(adminUser);

            this._mockNotificationsController
                .Setup(nc => nc.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero,
                    It.IsAny<IList<RoleInfo>>(),
                    It.IsAny<IList<UserInfo>>()));

            var notification = CreateUnsavedNotification();

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                new List<RoleInfo>(),
                new List<UserInfo>());

            Assert.AreEqual(adminUser.UserID, notification.SenderUserID);
        }

        [Test]
        [TestCase(null, null)]
        [TestCase(null, "")]
        [TestCase("", null)]
        [TestCase("", "")]
        [ExpectedException(typeof(ArgumentException))]
        public void SendNotification_Throws_On_Null_Or_Empty_Subject_And_Body(string subject, string body)
        {
            var notification = CreateUnsavedNotification();
            notification.Subject = subject;
            notification.Body = body;

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                new List<RoleInfo>(),
                new List<UserInfo>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SendNotification_Throws_On_Null_Roles_And_Users()
        {
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.SendNotification(
                CreateUnsavedNotification(),
                Constants.PORTAL_Zero,
                null,
                null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SendNotification_Throws_On_Large_Subject()
        {
            var notification = CreateUnsavedNotification();
            var subject = new StringBuilder();
            for (var i = 0; i <= 40; i++)
            {
                subject.Append("1234567890");
            }

            notification.Subject = subject.ToString();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                new List<RoleInfo>(),
                new List<UserInfo>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SendNotification_Throws_On_Roles_And_Users_With_No_DisplayNames()
        {
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.SendNotification(
                CreateUnsavedNotification(),
                Constants.PORTAL_Zero,
                new List<RoleInfo>(),
                new List<UserInfo>());
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SendNotification_Throws_On_Large_To_List()
        {
            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>();

            for (var i = 0; i <= 100; i++)
            {
                roles.Add(new RoleInfo { RoleName = "1234567890" });
                users.Add(new UserInfo { DisplayName = "1234567890" });
            }

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.SendNotification(
                CreateUnsavedNotification(),
                Constants.PORTAL_Zero,
                roles,
                users);
        }

        [Test]
        public void SendNotification_Calls_DataService_On_Valid_Notification()
        {
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this._mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12
                                    },
                            };

            this._mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Verifiable();

            this._mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
            notification.SendToast = false;

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this._mockDataService.Verify();
        }

        [Test]
        public void SendNotification_Calls_DataService_On_Valid_Notification_When_Portal_Is_In_Group()
        {
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this._mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Constants.PORTALGROUP_ValidPortalGroupId);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            List<PortalGroupInfo> portalGroups = new List<PortalGroupInfo>() { CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero) }; // CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero);
            this._portalGroupController.Setup(pgc => pgc.GetPortalGroups()).Returns(portalGroups);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12
                                    },
                            };

            this._mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Verifiable();

            this._mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
            notification.SendToast = false;

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this._mockDataService.Verify();
        }

        [Test]
        public void SendNotification_Calls_Messaging_DataService_CreateSocialMessageRecipientsForRole_When_Passing_Roles()
        {
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this._mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            var roles = new List<RoleInfo>
                            {
                                new RoleInfo
                                    {
                                        RoleID = Constants.RoleID_RegisteredUsers,
                                        RoleName = Constants.RoleName_RegisteredUsers
                                    },
                            };
            var users = new List<UserInfo>();

            this._mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this._mockMessagingDataService
                .Setup(mds => mds.CreateMessageRecipientsForRole(
                    Constants.Messaging_MessageId_1,
                    Constants.RoleID_RegisteredUsers.ToString(CultureInfo.InvariantCulture),
                    It.IsAny<int>()))
                .Verifiable();

            this._mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
            notification.SendToast = false;

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this._mockMessagingDataService.Verify();
        }

        [Test]
        public void SendNotification_Calls_Messaging_DataService_SaveSocialMessageRecipient_When_Passing_Users()
        {
            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this._mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);
            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12
                                    },
                            };

            this._mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this._mockInternalMessagingController
                .Setup(mc => mc.GetMessageRecipient(
                    Constants.Messaging_MessageId_1,
                    Constants.UserID_User12))
                .Returns((MessageRecipient)null);

            this._mockMessagingDataService
                .Setup(mds => mds.SaveMessageRecipient(
                    It.Is<MessageRecipient>(mr =>
                                            mr.MessageID == Constants.Messaging_MessageId_1 &&
                                            mr.UserID == Constants.UserID_User12 &&
                                            mr.Read == false &&
                                            mr.RecipientID == Null.NullInteger),
                    It.IsAny<int>()))
                .Verifiable();

            this._mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
            notification.SendToast = false;

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            this._mockMessagingDataService.Verify();
        }

        [Test]
        public void SendNotification_Returns_Valid_Object()
        {
            var expectedNotification = CreateValidNotification();

            var adminUser = new UserInfo
            {
                UserID = Constants.UserID_Admin,
                DisplayName = Constants.UserDisplayName_Admin,
                PortalID = Constants.PORTAL_Zero,
            };

            this._mockInternalMessagingController.Setup(mc => mc.RecipientLimit(adminUser.PortalID)).Returns(10);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            var roles = new List<RoleInfo>();
            var users = new List<UserInfo>
                            {
                                new UserInfo
                                    {
                                        UserID = Constants.UserID_User12,
                                        DisplayName = Constants.UserDisplayName_User12
                                    },
                            };

            this._mockDataService
                .Setup(ds => ds.SendNotification(
                    It.IsAny<Notification>(),
                    Constants.PORTAL_Zero))
                .Returns(Constants.Messaging_MessageId_1);

            this._mockMessagingDataService
                .Setup(mds => mds.CreateMessageRecipientsForRole(
                    Constants.Messaging_MessageId_1,
                    Constants.RoleID_RegisteredUsers.ToString(CultureInfo.InvariantCulture),
                    It.IsAny<int>()));

            this._mockInternalMessagingController
                .Setup(mc => mc.GetMessageRecipient(
                    Constants.Messaging_MessageId_1,
                    Constants.UserID_User12))
                .Returns((MessageRecipient)null);

            this._mockMessagingDataService
                .Setup(mds => mds.SaveMessageRecipient(
                    It.Is<MessageRecipient>(mr =>
                                            mr.MessageID == Constants.Messaging_MessageId_1 &&
                                            mr.UserID == Constants.UserID_User12 &&
                                            mr.Read == false &&
                                            mr.RecipientID == Null.NullInteger),
                    It.IsAny<int>()));

            this._mockNotificationsController.Setup(nc => nc.GetExpirationDate(It.IsAny<int>())).Returns(DateTime.MinValue);

            var notification = CreateUnsavedNotification();
            notification.SenderUserID = adminUser.UserID;
            notification.SendToast = false;

            this._mockNotificationsController.Object.SendNotification(
                notification,
                Constants.PORTAL_Zero,
                roles,
                users);

            Assert.IsTrue(new NotificationComparer().Equals(expectedNotification, notification));
        }

        [Test]
        public void DeleteNotification_Calls_DataService_DeleteNotification()
        {
            var messageRecipients = new List<MessageRecipient>
                                        {
                                            new MessageRecipient(),
                                        };

            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(messageRecipients);

            this._mockDataService.Setup(ds => ds.DeleteNotification(Constants.Messaging_MessageId_1)).Verifiable();
            this._notificationsController.DeleteNotification(Constants.Messaging_MessageId_1);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotifications()
        {
            this._mockDataService
                .Setup(ds => ds.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            this._notificationsController.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, 0, 10);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotifications_When_Portal_Is_In_Group()
        {
            this._mockDataService
                .Setup(ds => ds.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Constants.PORTALGROUP_ValidPortalGroupId);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            List<PortalGroupInfo> portalGroups = new List<PortalGroupInfo>() { CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero) }; // CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero);
            this._portalGroupController.Setup(pgc => pgc.GetPortalGroups()).Returns(portalGroups);

            this._notificationsController.GetNotifications(Constants.UserID_User12, Constants.PORTAL_Zero, 0, 10);
            this._mockDataService.Verify();
        }

        [Test]
        public void GetNotifications_Calls_DataService_GetNotificationByContext()
        {
            this._mockDataService
                .Setup(ds => ds.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext))
                .Returns(new DataTable().CreateDataReader())
                .Verifiable();

            this._notificationsController.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext);
            this._mockDataService.Verify();
        }

        [Test]
        public void CountNotifications_Calls_DataService_CountNotifications()
        {
            this._mockDataService.Setup(ds => ds.CountNotifications(Constants.UserID_User12, Constants.PORTAL_Zero)).Verifiable();
            this._notificationsController.CountNotifications(Constants.UserID_User12, Constants.PORTAL_Zero);
            this._mockDataService.Verify();
        }

        [Test]
        public void DeleteNotificationRecipient_Calls_MessagingController_DeleteMessageRecipient()
        {
            this._mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this._notificationsController.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this._mockMessagingController.Verify();
        }

        [Test]
        public void DeleteNotificationRecipientByContext_Calls_DeleteMessageRecipient()
        {
            this._mockNotificationsController.Setup(mc => mc.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12)).Verifiable();
            this._mockNotificationsController.Setup(mc => mc.GetNotificationByContext(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext))
                .Returns(new List<Notification> { new Notification { NotificationID = Constants.Messaging_MessageId_1 } });
            this._mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_NotificationTypeId, Constants.Messaging_NotificationContext, Constants.UserID_User12);
            this._mockMessagingController.Verify();
        }

        [Test]
        public void DeleteNotificationRecipient_Does_Not_Delete_Notification_When_There_Are_More_Recipients()
        {
            var messageRecipients = new List<MessageRecipient>
                                        {
                                            new MessageRecipient(),
                                        };

            this._mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12));
            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(messageRecipients);
            this._mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);

            this._mockNotificationsController.Verify(nc => nc.DeleteNotification(Constants.Messaging_MessageId_1), Times.Never());
        }

        [Test]
        public void DeleteNotificationRecipient_Deletes_Notification_When_There_Are_No_More_Recipients()
        {
            this._mockInternalMessagingController.Setup(mc => mc.DeleteMessageRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12));
            this._mockInternalMessagingController.Setup(mc => mc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this._mockNotificationsController.Object.DeleteNotificationRecipient(Constants.Messaging_MessageId_1, Constants.UserID_User12);
            this._mockNotificationsController.Verify(nc => nc.DeleteNotification(Constants.Messaging_MessageId_1));
        }

        [Test]
        public void DeleteAllNotificationRecipients_Calls_DeleteNotificationRecipient_For_Each_Recipient()
        {
            var recipients = new List<MessageRecipient>
                                 {
                                     new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_1 },
                                     new MessageRecipient { RecipientID = Constants.Messaging_RecipientId_2 },
                                 };

            this._mockInternalMessagingController.Setup(imc => imc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(recipients);

            this._mockNotificationsController.Object.DeleteAllNotificationRecipients(Constants.Messaging_MessageId_1);

            this._mockNotificationsController.Verify(nc => nc.DeleteNotificationRecipient(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(2));
        }

        [Test]
        public void DeleteAllNotificationRecipients_Does_Not_Call_DeleteNotificationRecipient_When_Notification_Has_No_Recipients()
        {
            this._mockInternalMessagingController.Setup(imc => imc.GetMessageRecipients(Constants.Messaging_MessageId_1)).Returns(new List<MessageRecipient>());
            this._mockNotificationsController.Object.DeleteAllNotificationRecipients(Constants.Messaging_MessageId_1);
            this._mockNotificationsController.Verify(nc => nc.DeleteNotificationRecipient(It.IsAny<int>(), It.IsAny<int>()), Times.Never());
        }

        private static Notification CreateUnsavedNotification()
        {
            return new Notification
            {
                NotificationTypeID = Constants.Messaging_NotificationTypeId,
                Subject = Constants.Messaging_NotificationSubject,
                Body = Constants.Messaging_NotificationBody,
                To = Constants.UserDisplayName_User12,
                From = Constants.UserDisplayName_Admin,
                SenderUserID = Constants.UserID_Admin,
                Context = Constants.Messaging_NotificationContext,
                SendToast = false,
            };
        }

        private static Notification CreateValidNotification()
        {
            var notification = CreateUnsavedNotification();
            notification.NotificationID = Constants.Messaging_MessageId_1;

            return notification;
        }

        private static NotificationType CreateValidNotificationType()
        {
            var nt = CreateNewNotificationType();
            nt.NotificationTypeId = Constants.Messaging_NotificationTypeId;
            return nt;
        }

        private static NotificationType CreateNewNotificationType()
        {
            return new NotificationType
            {
                Name = Constants.Messaging_NotificationTypeName,
                Description = Constants.Messaging_NotificationTypeDescription,
                TimeToLive = new TimeSpan(0, Constants.Messaging_NotificationTypeTTL, 0),
                DesktopModuleId = Constants.Messaging_NotificationTypeDesktopModuleId,
                IsTask = false,
            };
        }

        private static NotificationTypeAction CreateValidNotificationTypeAction()
        {
            var action = CreateNewNotificationTypeAction();

            action.NotificationTypeActionId = Constants.Messaging_NotificationTypeActionId;
            action.NotificationTypeId = Constants.Messaging_NotificationTypeId;

            return action;
        }

        private static NotificationTypeAction CreateNewNotificationTypeAction()
        {
            return new NotificationTypeAction
            {
                APICall = Constants.Messaging_NotificationTypeActionAPICall,
                ConfirmResourceKey = Constants.Messaging_NotificationTypeActionConfirmResourceKey,
                DescriptionResourceKey = Constants.Messaging_NotificationTypeActionDescriptionResourceKey,
                NameResourceKey = Constants.Messaging_NotificationTypeActionNameResourceKey,
            };
        }

        private static PortalInfo CreatePortalInfo(int portalId, int portalGroupId)
        {
            var mockPortalInfo = new PortalInfo { PortalID = portalId, PortalGroupID = portalGroupId };
            return mockPortalInfo;
        }

        private static PortalGroupInfo CreatePortalGroupInfo(int portalGroupId, int masterPortalId)
        {
            var mockPortalGroupInfo = new PortalGroupInfo
            {
                PortalGroupId = portalGroupId,
                MasterPortalId = masterPortalId,
                PortalGroupName = Constants.PORTALGROUP_ValidName,
                PortalGroupDescription = Constants.PORTALGROUP_ValidDescription,
            };

            return mockPortalGroupInfo;
        }

        private void SetupDataProvider()
        {
            // Standard DataProvider Path for Logging
            this._dataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
        }

        private void SetupDataTables()
        {
            this._dtNotificationTypes = new DataTable();
            this._dtNotificationTypes.Columns.Add("NotificationTypeID", typeof(int));
            this._dtNotificationTypes.Columns.Add("Name", typeof(string));
            this._dtNotificationTypes.Columns.Add("Description", typeof(string));
            this._dtNotificationTypes.Columns.Add("TTL", typeof(int));
            this._dtNotificationTypes.Columns.Add("DesktopModuleID", typeof(int));
            this._dtNotificationTypes.Columns.Add("CreatedByUserID", typeof(int));
            this._dtNotificationTypes.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtNotificationTypes.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtNotificationTypes.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this._dtNotificationTypes.Columns.Add("IsTask", typeof(bool));

            this._dtNotificationTypeActions = new DataTable();
            this._dtNotificationTypeActions.Columns.Add("NotificationTypeActionID", typeof(int));
            this._dtNotificationTypeActions.Columns.Add("NotificationTypeID", typeof(int));
            this._dtNotificationTypeActions.Columns.Add("NameResourceKey", typeof(string));
            this._dtNotificationTypeActions.Columns.Add("DescriptionResourceKey", typeof(string));
            this._dtNotificationTypeActions.Columns.Add("ConfirmResourceKey", typeof(string));
            this._dtNotificationTypeActions.Columns.Add("Order", typeof(int));
            this._dtNotificationTypeActions.Columns.Add("APICall", typeof(string));
            this._dtNotificationTypeActions.Columns.Add("CreatedByUserID", typeof(int));
            this._dtNotificationTypeActions.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtNotificationTypeActions.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtNotificationTypeActions.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            this._dtNotificationActions = new DataTable();
            this._dtNotificationActions.Columns.Add("NotificationActionID");
            this._dtNotificationActions.Columns.Add("MessageID");
            this._dtNotificationActions.Columns.Add("NotificationTypeActionID");
            this._dtNotificationActions.Columns.Add("Key");
            this._dtNotificationActions.Columns.Add("CreatedByUserID", typeof(int));
            this._dtNotificationActions.Columns.Add("CreatedOnDate", typeof(DateTime));
            this._dtNotificationActions.Columns.Add("LastModifiedByUserID", typeof(int));
            this._dtNotificationActions.Columns.Add("LastModifiedOnDate", typeof(DateTime));
        }

        private class NotificationTypeComparer : IEqualityComparer<NotificationType>
        {
            public bool Equals(NotificationType x, NotificationType y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return
                    x.NotificationTypeId == y.NotificationTypeId &&
                    x.Name == y.Name &&
                    x.Description == y.Description &&
                    x.TimeToLive == y.TimeToLive &&
                    x.IsTask == y.IsTask &&
                    x.DesktopModuleId == y.DesktopModuleId;
            }

            public int GetHashCode(NotificationType obj)
            {
                throw new NotImplementedException();
            }
        }

        private class NotificationTypeActionComparer : IEqualityComparer<NotificationTypeAction>
        {
            public bool Equals(NotificationTypeAction x, NotificationTypeAction y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return
                    x.NotificationTypeActionId == y.NotificationTypeActionId &&
                    x.NotificationTypeId == y.NotificationTypeId &&
                    x.NameResourceKey == y.NameResourceKey &&
                    x.DescriptionResourceKey == y.DescriptionResourceKey &&
                    x.ConfirmResourceKey == y.ConfirmResourceKey &&
                    x.APICall == y.APICall;
            }

            public int GetHashCode(NotificationTypeAction obj)
            {
                throw new NotImplementedException();
            }
        }

        private class NotificationComparer : IEqualityComparer<Notification>
        {
            public bool Equals(Notification x, Notification y)
            {
                if (x == null)
                {
                    return y == null;
                }

                if (y == null)
                {
                    return false;
                }

                return
                    x.NotificationID == y.NotificationID &&
                    x.NotificationTypeID == y.NotificationTypeID &&
                    x.Subject == y.Subject &&
                    x.Body == y.Body &&
                    x.To == y.To &&
                    x.From == y.From &&
                    x.SenderUserID == y.SenderUserID &&
                    x.Context == y.Context &&
                    x.IncludeDismissAction == y.IncludeDismissAction;
            }

            public int GetHashCode(Notification obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}

// ReSharper restore InconsistentNaming
