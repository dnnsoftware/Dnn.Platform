// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Social
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Entities.Users.Social.Data;
    using DotNetNuke.Entities.Users.Social.Internal;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    /// <summary>
    ///  Testing various aspects of RelationshipController.
    /// </summary>
    [TestFixture]
    public class RelationshipControllerTests
    {
        private Mock<CachingProvider> mockCachingProvider;
        private Mock<IPortalController> _portalController;
        private Mock<IPortalGroupController> _portalGroupController;

        private DataTable dtRelationshipTypes;
        private DataTable dtRelationships;
        private DataTable dtUserRelationships;
        private DataTable dtUserRelationshipPreferences;

        [SetUp]
        public void SetUp()
        {
            ComponentFactory.Container = new SimpleContainer();
            var mockDataProvider = MockComponentProvider.CreateDataProvider();
            mockDataProvider.Setup(dp => dp.GetProviderPath()).Returns(string.Empty);

            this.mockCachingProvider = MockComponentProvider.CreateDataCacheProvider();
            MockComponentProvider.CreateEventLogController();

            this._portalController = new Mock<IPortalController>();
            PortalController.SetTestableInstance(this._portalController.Object);

            this._portalGroupController = new Mock<IPortalGroupController>();
            PortalGroupController.RegisterInstance(this._portalGroupController.Object);

            var mockHostController = new Mock<IHostController>();
            mockHostController.Setup(c => c.GetString("PerformanceSetting")).Returns("0");
            HostController.RegisterInstance(mockHostController.Object);

            var mockUserController = new Mock<IUserController>();
            mockUserController.Setup(c => c.GetCurrentUserInfo()).Returns(new UserInfo() { UserID = 1 });
            UserController.SetTestableInstance(mockUserController.Object);

            this.CreateLocalizationProvider();

            this.SetupDataTables();
        }

        [TearDown]
        public void TearDown()
        {
            ComponentFactory.Container = null;
            PortalController.ClearInstance();
            UserController.ClearInstance();
        }

        [Test]
        public void RelationshipController_Constructor_Throws_On_Null_DataService()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new RelationshipControllerImpl(null, mockEventLogController.Object));
        }

        [Test]
        public void RelationshipController_Constructor_Throws_On_Null_EventLogController()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();

            // Act, Assert
            Assert.Throws<ArgumentNullException>(() => new RelationshipControllerImpl(mockDataService.Object, null));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_DeleteRelationshipType_Throws_On_Null_RelationshipType()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.DeleteRelationshipType(null);
        }

        [Test]
        public void RelationshipController_DeleteRelationshipType_Calls_DataService()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
            };

            // Act
            relationshipController.DeleteRelationshipType(relationshipType);

            // Assert
            mockDataService.Verify(d => d.DeleteRelationshipType(Constants.SOCIAL_FollowerRelationshipTypeID));
        }

        [Test]
        public void RelationshipController_DeleteRelationshipType_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();
            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
                Name = Constants.SOCIAL_RelationshipTypeName,
            };

            // Act
            relationshipController.DeleteRelationshipType(relationshipType);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_RelationshipType_Deleted, Constants.SOCIAL_RelationshipTypeName, Constants.SOCIAL_FollowerRelationshipTypeID);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        public void RelationshipController_DeleteRelationshipType_Calls_DataCache_RemoveCache()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();
            var cacheKey = CachingProvider.GetCacheKey(DataCache.RelationshipTypesCacheKey);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
            };

            // Act
            relationshipController.DeleteRelationshipType(relationshipType);

            // Assert
            this.mockCachingProvider.Verify(e => e.Remove(cacheKey));
        }

        [Test]
        public void RelationshipController_GetAllRelationshipTypes_Calls_DataService()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationshipTypes = relationshipController.GetAllRelationshipTypes();

            // Assert
            mockDataService.Verify(d => d.GetAllRelationshipTypes());
        }

        [Test]
        public void RelationshipController_GetRelationshipType_Calls_DataService_If_Not_Cached()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationshipTypes = relationshipController.GetRelationshipType(Constants.SOCIAL_FriendRelationshipTypeID);

            // Assert
            mockDataService.Verify(d => d.GetAllRelationshipTypes());
        }

        [Test]
        [TestCase(Constants.SOCIAL_FriendRelationshipTypeID)]
        [TestCase(Constants.SOCIAL_FollowerRelationshipTypeID)]
        public void RelationshipController_GetRelationshipType_Returns_RelationshipType_For_Valid_ID(int relationshipTypeId)
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationshipType = relationshipController.GetRelationshipType(relationshipTypeId);

            // Assert
            Assert.AreEqual(relationshipTypeId, relationshipType.RelationshipTypeId);
        }

        [Test]
        public void RelationshipController_GetRelationshipType_Returns_Null_For_InValid_ID()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationshipType = relationshipController.GetRelationshipType(Constants.SOCIAL_InValidRelationshipType);

            // Assert
            Assert.IsNull(relationshipType);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_SaveRelationshipType_Throws_On_Null_RelationshipType()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.SaveRelationshipType(null);
        }

        [Test]
        public void RelationshipController_SaveRelationshipType_Calls_DataService()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
            };

            // Act
            relationshipController.SaveRelationshipType(relationshipType);

            // Assert
            mockDataService.Verify(d => d.SaveRelationshipType(relationshipType, It.IsAny<int>()));
        }

        [Test]
        public void RelationshipController_SaveRelationshipType_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
                Name = Constants.SOCIAL_RelationshipTypeName,
            };

            // Act
            relationshipController.SaveRelationshipType(relationshipType);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_RelationshipType_Updated, Constants.SOCIAL_RelationshipTypeName);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        public void RelationshipController_SaveRelationshipType_Calls_DataCache_RemoveCache()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();
            var cacheKey = CachingProvider.GetCacheKey(DataCache.RelationshipTypesCacheKey);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
            };

            // Act
            relationshipController.SaveRelationshipType(relationshipType);

            // Assert
            this.mockCachingProvider.Verify(e => e.Remove(cacheKey));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_DeleteRelationship_Throws_On_Null_Relationship()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.DeleteRelationship(null);
        }

        [Test]
        public void RelationshipController_DeleteRelationship_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var relationship = new Relationship()
            {
                RelationshipId = Constants.SOCIAL_FollowerRelationshipID,
            };

            // Act
            relationshipController.DeleteRelationship(relationship);

            // Assert
            mockDataService.Verify(d => d.DeleteRelationship(Constants.SOCIAL_FollowerRelationshipID));
        }

        [Test]
        public void RelationshipController_DeleteRelationship_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var relationship = new Relationship()
            {
                RelationshipId = Constants.SOCIAL_FollowerRelationshipID,
                Name = Constants.SOCIAL_RelationshipName,
            };

            // Act
            relationshipController.DeleteRelationship(relationship);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_Relationship_Deleted, Constants.SOCIAL_RelationshipName, Constants.SOCIAL_FollowerRelationshipID);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        public void RelationshipController_DeleteRelationship_Calls_DataCache_RemoveCache()
        {
            // Arrange
            var portalId = 1;
            var relationshipController = this.CreateRelationshipController();
            var cacheKey = CachingProvider.GetCacheKey(string.Format(DataCache.RelationshipByPortalIDCacheKey, portalId));
            var relationship = new Relationship()
            {
                RelationshipId = Constants.SOCIAL_FollowerRelationshipID,
                PortalId = portalId,
                UserId = -1,
            };

            // Act
            relationshipController.DeleteRelationship(relationship);

            // Assert
            this.mockCachingProvider.Verify(e => e.Remove(cacheKey));
        }

        [Test]
        [TestCase(Constants.SOCIAL_FriendRelationshipID, DefaultRelationshipTypes.Friends)]
        [TestCase(Constants.SOCIAL_FollowerRelationshipID, DefaultRelationshipTypes.Followers)]
        public void RelationshipController_GetRelationship_Returns_Relationship_For_Valid_ID(int relationshipId, DefaultRelationshipTypes defaultType)
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            this.dtRelationships.Rows.Add(relationshipId, defaultType, defaultType.ToString(), defaultType.ToString(), Constants.PORTAL_Zero, Constants.USER_Null, RelationshipStatus.None);
            mockDataService.Setup(md => md.GetRelationship(relationshipId)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationship = relationshipController.GetRelationship(relationshipId);

            // Assert
            Assert.AreEqual(relationshipId, relationship.RelationshipId);
        }

        [Test]
        public void RelationshipController_GetRelationship_Returns_Null_For_InValid_ID()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            mockDataService.Setup(md => md.GetRelationship(It.IsAny<int>())).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationship = relationshipController.GetRelationship(Constants.SOCIAL_InValidRelationship);

            // Assert
            Assert.IsNull(relationship);
        }

        [Test]
        public void RelationshipController_GetRelationshipsByUserID_Returns_List_Of_Relationships_For_Valid_User()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            for (int i = 1; i <= 5; i++)
            {
                this.dtRelationships.Rows.Add(i, DefaultRelationshipTypes.Friends, DefaultRelationshipTypes.Friends.ToString(),
                                            DefaultRelationshipTypes.Friends.ToString(),
                                            Constants.PORTAL_Zero,
                                            Constants.USER_ValidId,
                                            RelationshipStatus.None);
            }

            mockDataService.Setup(md => md.GetRelationshipsByUserId(Constants.USER_ValidId)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationships = relationshipController.GetRelationshipsByUserId(Constants.USER_ValidId);

            // Assert
            Assert.IsInstanceOf<IList<Relationship>>(relationships);
            Assert.AreEqual(5, relationships.Count);
        }

        [Test]
        public void RelationshipController_GetRelationshipsByUserID_Returns_EmptyList_Of_Relationships_For_InValid_User()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            mockDataService.Setup(md => md.GetRelationshipsByUserId(Constants.USER_InValidId)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var relationships = relationshipController.GetRelationshipsByUserId(Constants.USER_InValidId);

            // Assert
            Assert.IsInstanceOf<IList<Relationship>>(relationships);
            Assert.AreEqual(0, relationships.Count);
        }

        [Test]
        public void RelationshipController_GetRelationshipsByPortalID_Returns_List_Of_Relationships_For_Valid_Portal()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            for (int i = 1; i <= 5; i++)
            {
                this.dtRelationships.Rows.Add(i, DefaultRelationshipTypes.Friends, DefaultRelationshipTypes.Friends.ToString(),
                                            DefaultRelationshipTypes.Friends.ToString(),
                                            Constants.PORTAL_Zero,
                                            Constants.USER_Null,
                                            RelationshipStatus.None);
            }

            mockDataService.Setup(md => md.GetRelationshipsByPortalId(Constants.PORTAL_Zero)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            // Act
            var relationships = relationshipController.GetRelationshipsByPortalId(Constants.PORTAL_Zero);

            // Assert
            Assert.IsInstanceOf<IList<Relationship>>(relationships);
            Assert.AreEqual(5, relationships.Count);
        }

        [Test]
        public void RelationshipController_GetRelationshipsByPortalID_Returns_List_Of_Relationships_For_Valid_Portal_When_Portal_Is_In_Group()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            for (int i = 1; i <= 5; i++)
            {
                this.dtRelationships.Rows.Add(i, DefaultRelationshipTypes.Friends, DefaultRelationshipTypes.Friends.ToString(),
                                            DefaultRelationshipTypes.Friends.ToString(),
                                            Constants.PORTAL_Zero,
                                            Constants.USER_Null,
                                            RelationshipStatus.None);
            }

            mockDataService.Setup(md => md.GetRelationshipsByPortalId(Constants.PORTAL_Zero)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Zero, Constants.PORTALGROUP_ValidPortalGroupId);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Zero)).Returns(mockPortalInfo);

            List<PortalGroupInfo> portalGroups = new List<PortalGroupInfo>() { CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero) }; // CreatePortalGroupInfo(Constants.PORTALGROUP_ValidPortalGroupId, Constants.PORTAL_Zero);
            this._portalGroupController.Setup(pgc => pgc.GetPortalGroups()).Returns(portalGroups);

            // Act
            var relationships = relationshipController.GetRelationshipsByPortalId(Constants.PORTAL_Zero);

            // Assert
            Assert.IsInstanceOf<IList<Relationship>>(relationships);
            Assert.AreEqual(5, relationships.Count);
        }

        [Test]
        public void RelationshipController_GetRelationshipsByPortalID_Returns_EmptyList_Of_Relationships_For_InValid_Portal()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtRelationships.Clear();
            mockDataService.Setup(md => md.GetRelationshipsByPortalId(Constants.PORTAL_Null)).Returns(this.dtRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            var mockPortalInfo = CreatePortalInfo(Constants.PORTAL_Null, Null.NullInteger);
            this._portalController.Setup(pc => pc.GetPortal(Constants.PORTAL_Null)).Returns(mockPortalInfo);

            // Act
            var relationships = relationshipController.GetRelationshipsByPortalId(Constants.PORTAL_Null);

            // Assert
            Assert.IsInstanceOf<IList<Relationship>>(relationships);
            Assert.AreEqual(0, relationships.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_SaveRelationship_Throws_On_Null_Relationship()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.SaveRelationship(null);
        }

        [Test]
        public void RelationshipController_SaveRelationship_Calls_DataService()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var relationship = new Relationship
            {
                RelationshipId = Constants.SOCIAL_FollowerRelationshipID,
            };

            // Act
            relationshipController.SaveRelationship(relationship);

            // Assert
            mockDataService.Verify(d => d.SaveRelationship(relationship, It.IsAny<int>()));
        }

        [Test]
        public void RelationshipController_SaveRelationship_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var relationship = new Relationship
            {
                RelationshipId = Constants.SOCIAL_FollowerRelationshipID,
                Name = Constants.SOCIAL_RelationshipName,
            };

            // Act
            relationshipController.SaveRelationship(relationship);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_Relationship_Updated, Constants.SOCIAL_RelationshipName);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        public void RelationshipController_SaveRelationship_Calls_DataCache_RemoveCache()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();
            var cacheKey = CachingProvider.GetCacheKey(DataCache.RelationshipTypesCacheKey);
            var relationshipType = new RelationshipType()
            {
                RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID,
            };

            // Act
            relationshipController.SaveRelationshipType(relationshipType);

            // Assert
            this.mockCachingProvider.Verify(e => e.Remove(cacheKey));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_DeleteUserRelationship_Throws_On_Null_UserRelationship()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.DeleteUserRelationship(null);
        }

        [Test]
        public void RelationshipController_DeleteUserRelationship_Calls_DataService()
        {
            // Arrange
            var mockDataService = this.CreateMockDataServiceWithRelationshipTypes();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var userRelationship = new UserRelationship()
            {
                UserRelationshipId = Constants.SOCIAL_UserRelationshipIDUser10User11,
            };

            // Act
            relationshipController.DeleteUserRelationship(userRelationship);

            // Assert
            mockDataService.Verify(d => d.DeleteUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11));
        }

        [Test]
        public void RelationshipController_DeleteUserRelationship_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var userRelationship = new UserRelationship
            {
                UserRelationshipId = Constants.SOCIAL_UserRelationshipIDUser10User11,
                UserId = Constants.USER_ElevenId,
                RelatedUserId = Constants.USER_TenId,
            };

            // Act
            relationshipController.DeleteUserRelationship(userRelationship);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_UserRelationship_Deleted, Constants.SOCIAL_UserRelationshipIDUser10User11, Constants.USER_ElevenId, Constants.USER_TenId);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        [TestCase(Constants.SOCIAL_UserRelationshipIDUser10User11, Constants.USER_TenId, Constants.USER_ElevenId)]
        [TestCase(Constants.SOCIAL_UserRelationshipIDUser12User13, 12, 13)]
        public void RelationshipController_GetUserRelationship_Returns_Relationship_For_Valid_ID(int userRelationshipId, int userId, int relatedUserId)
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtUserRelationships.Clear();
            this.dtUserRelationships.Rows.Add(userRelationshipId, userId, relatedUserId, Constants.SOCIAL_FriendRelationshipID, RelationshipStatus.None);
            mockDataService.Setup(md => md.GetUserRelationship(userRelationshipId)).Returns(this.dtUserRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.GetUserRelationship(userRelationshipId);

            // Assert
            Assert.AreEqual(userRelationshipId, userRelationship.UserRelationshipId);
        }

        [Test]
        public void RelationshipController_GetUserRelationship_Returns_Null_For_InValid_ID()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtUserRelationships.Clear();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>())).Returns(this.dtUserRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.GetUserRelationship(Constants.SOCIAL_InValidUserRelationship);

            // Assert
            Assert.IsNull(userRelationship);
        }

        [Test]
        public void RelationshipController_GetUserRelationships_Returns_List_Of_UserRelationships_For_Valid_User()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtUserRelationships.Clear();
            for (int i = 1; i <= 5; i++)
            {
                this.dtUserRelationships.Rows.Add(i, Constants.USER_ValidId, Constants.USER_TenId,
                                                Constants.SOCIAL_FriendRelationshipID, RelationshipStatus.None);
            }

            mockDataService.Setup(md => md.GetUserRelationships(Constants.USER_ValidId)).Returns(this.dtUserRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var user = new UserInfo { UserID = Constants.USER_ValidId };
            var userRelationships = relationshipController.GetUserRelationships(user);

            // Assert
            Assert.IsInstanceOf<IList<UserRelationship>>(userRelationships);
            Assert.AreEqual(5, userRelationships.Count);
        }

        [Test]
        public void RelationshipController_GetUserRelationships_Returns_EmptyList_Of_UserRelationships_For_InValid_User()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            this.dtUserRelationships.Clear();

            mockDataService.Setup(md => md.GetUserRelationships(Constants.USER_InValidId)).Returns(this.dtUserRelationships.CreateDataReader());
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var user = new UserInfo { UserID = Constants.USER_InValidId };
            var userRelationships = relationshipController.GetUserRelationships(user);

            // Assert
            Assert.IsInstanceOf<IList<UserRelationship>>(userRelationships);
            Assert.AreEqual(0, userRelationships.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_SaveUserRelationship_Throws_On_Null_UserRelationship()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.SaveUserRelationship(null);
        }

        [Test]
        public void RelationshipController_SaveUserRelationship_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var userRelationship = new UserRelationship()
            {
                UserRelationshipId = Constants.SOCIAL_UserRelationshipIDUser10User11,
            };

            // Act
            relationshipController.SaveUserRelationship(userRelationship);

            // Assert
            mockDataService.Verify(d => d.SaveUserRelationship(userRelationship, It.IsAny<int>()));
        }

        [Test]
        public void RelationshipController_SaveUserRelationship_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.SaveUserRelationship(It.IsAny<UserRelationship>(), It.IsAny<int>()))
                                .Returns(Constants.SOCIAL_UserRelationshipIDUser10User11);
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = new RelationshipControllerImpl(mockDataService.Object, mockEventLogController.Object);
            var userRelationship = new UserRelationship
            {
                UserRelationshipId = Constants.SOCIAL_UserRelationshipIDUser10User11,
                UserId = Constants.USER_ElevenId,
                RelatedUserId = Constants.USER_TenId,
            };

            // Act
            relationshipController.SaveUserRelationship(userRelationship);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_UserRelationship_Updated, Constants.SOCIAL_UserRelationshipIDUser10User11, Constants.USER_ElevenId, Constants.USER_TenId);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_DeleteUserRelationshipPreference_Throws_On_Null_UserRelationshipPreference()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.DeleteUserRelationshipPreference(null);
        }

        [Test]
        public void RelationshipController_DeleteUserRelationshipPreference_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var preference = new UserRelationshipPreference()
            {
                PreferenceId = Constants.SOCIAL_PrefereceIDForUser11,
            };

            // Act
            relationshipController.DeleteUserRelationshipPreference(preference);

            // Assert
            mockDataService.Verify(d => d.DeleteUserRelationshipPreference(Constants.SOCIAL_PrefereceIDForUser11));
        }

        [Test]
        public void RelationshipController_DeleteUserRelationshipPreference_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = this.CreateRelationshipController(mockEventLogController);
            var preference = new UserRelationshipPreference()
            {
                PreferenceId = Constants.SOCIAL_PrefereceIDForUser11,
                UserId = Constants.USER_ElevenId,
                RelationshipId = Constants.SOCIAL_FriendRelationshipID,
            };

            // Act
            relationshipController.DeleteUserRelationshipPreference(preference);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_UserRelationshipPreference_Deleted, Constants.SOCIAL_PrefereceIDForUser11, Constants.USER_ElevenId, Constants.SOCIAL_FriendRelationshipID);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        public void RelationshipController_GetUserRelationshipPreference_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetUserRelationshipPreferenceById(It.IsAny<int>()))
                            .Returns(this.dtUserRelationshipPreferences.CreateDataReader);
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var preference = relationshipController.GetUserRelationshipPreference(Constants.SOCIAL_PrefereceIDForUser11);

            // Assert
            mockDataService.Verify(d => d.GetUserRelationshipPreferenceById(Constants.SOCIAL_PrefereceIDForUser11));
        }

        [Test]
        public void RelationshipController_GetUserRelationshipPreference_Overload_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetUserRelationshipPreference(It.IsAny<int>(), It.IsAny<int>()))
                            .Returns(this.dtUserRelationshipPreferences.CreateDataReader);
            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var preference = relationshipController.GetUserRelationshipPreference(Constants.USER_ValidId, Constants.SOCIAL_FriendRelationshipID);

            // Assert
            mockDataService.Verify(d => d.GetUserRelationshipPreference(Constants.USER_ValidId, Constants.SOCIAL_FriendRelationshipID));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RelationshipController_SaveUserRelationshipPreference_Throws_On_Null_UserRelationshipPreference()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();

            // Act, Assert
            relationshipController.SaveUserRelationshipPreference(null);
        }

        [Test]
        public void RelationshipController_SaveUserRelationshipPreference_Calls_DataService()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var relationshipController = this.CreateRelationshipController(mockDataService);
            var preference = new UserRelationshipPreference()
            {
                PreferenceId = Constants.SOCIAL_PrefereceIDForUser11,
                UserId = Constants.USER_ElevenId,
                RelationshipId = Constants.SOCIAL_FriendRelationshipID,
            };

            // Act
            relationshipController.SaveUserRelationshipPreference(preference);

            // Assert
            mockDataService.Verify(d => d.SaveUserRelationshipPreference(preference, It.IsAny<int>()));
        }

        [Test]
        public void RelationshipController_SaveUserRelationshipPreference_Calls_EventLogController_AddLog()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.SaveUserRelationshipPreference(It.IsAny<UserRelationshipPreference>(), It.IsAny<int>()))
                                .Returns(Constants.SOCIAL_PrefereceIDForUser11);
            var mockEventLogController = new Mock<IEventLogController>();
            mockEventLogController.Setup(c => c.AddLog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EventLogController.EventLogType>()));
            this.CreateLocalizationProvider();

            var relationshipController = new RelationshipControllerImpl(mockDataService.Object, mockEventLogController.Object);
            var preference = new UserRelationshipPreference()
            {
                PreferenceId = Constants.SOCIAL_PrefereceIDForUser11,
                UserId = Constants.USER_ElevenId,
                RelationshipId = Constants.SOCIAL_FriendRelationshipID,
            };

            // Act
            relationshipController.SaveUserRelationshipPreference(preference);

            // Assert
            var logContent = string.Format(Constants.LOCALIZATION_UserRelationshipPreference_Updated, Constants.SOCIAL_PrefereceIDForUser11, Constants.USER_ElevenId, Constants.SOCIAL_FriendRelationshipID);
            mockEventLogController.Verify(e => e.AddLog("Message", logContent, EventLogController.EventLogType.ADMIN_ALERT));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void RelationshipController_InitiateUserRelationship_Throws_On_Negative_RelationshipID()
        {
            // Arrange
            var relationshipController = this.CreateRelationshipController();
            var initiatingUser = new UserInfo { UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };
            var targetUser = new UserInfo { UserID = Constants.USER_ElevenId, PortalID = Constants.PORTAL_Zero };
            var relationship = new Relationship();

            // Act, Assert
            relationshipController.InitiateUserRelationship(initiatingUser, targetUser, relationship);
        }

        [Test]
        public void RelationshipController_InitiateUserRelationship_Returns_Status_Accepted_When_Default_Relationship_Action_Is_Accepted()
        {
            // Arrange
            var initiatingUser = new UserInfo { UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };
            var targetUser = new UserInfo { UserID = Constants.USER_ElevenId, PortalID = Constants.PORTAL_Zero };
            var relationship = new Relationship { RelationshipId = Constants.SOCIAL_FollowerRelationshipID, RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID, DefaultResponse = RelationshipStatus.Accepted };

            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Clear();

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<RelationshipDirection>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.GetUserRelationshipPreference(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtUserRelationshipPreferences.CreateDataReader());
            mockDataService.Setup(md => md.GetAllRelationshipTypes()).Returns(this.dtRelationshipTypes.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.InitiateUserRelationship(initiatingUser, targetUser, relationship);

            // Assert
            Assert.AreEqual(userRelationship.Status, RelationshipStatus.Accepted);
        }

        [Test]
        public void RelationshipController_InitiateUserRelationship_Returns_Status_Initiated_When_Default_Relationship_Action_Is_None()
        {
            // Arrange
            var initiatingUser = new UserInfo { UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };
            var targetUser = new UserInfo { UserID = Constants.USER_ElevenId, PortalID = Constants.PORTAL_Zero };
            var relationship = new Relationship { RelationshipId = Constants.SOCIAL_FollowerRelationshipID, RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID, DefaultResponse = RelationshipStatus.None };

            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Clear();

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<RelationshipDirection>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.GetUserRelationshipPreference(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtUserRelationshipPreferences.CreateDataReader());
            mockDataService.Setup(md => md.GetAllRelationshipTypes()).Returns(this.dtRelationshipTypes.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.InitiateUserRelationship(initiatingUser, targetUser, relationship);

            // Assert
            Assert.AreEqual(userRelationship.Status, RelationshipStatus.Pending);
        }

        [Test]
        public void RelationshipController_InitiateUserRelationship_Returns_Status_Accepted_When_TargetUsers_Relationship_Action_Is_Accepted()
        {
            // Arrange
            var initiatingUser = new UserInfo { UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };
            var targetUser = new UserInfo { UserID = Constants.USER_ElevenId, PortalID = Constants.PORTAL_Zero };
            var relationship = new Relationship { RelationshipId = Constants.SOCIAL_FollowerRelationshipID, RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID, DefaultResponse = RelationshipStatus.Accepted };

            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Add(Constants.SOCIAL_PrefereceIDForUser11, Constants.USER_TenId, Constants.USER_ElevenId, RelationshipStatus.Accepted);

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<RelationshipDirection>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.GetUserRelationshipPreference(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtUserRelationshipPreferences.CreateDataReader());
            mockDataService.Setup(md => md.GetAllRelationshipTypes()).Returns(this.dtRelationshipTypes.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.InitiateUserRelationship(initiatingUser, targetUser, relationship);

            // Assert
            Assert.AreEqual(userRelationship.Status, RelationshipStatus.Accepted);
        }

        [Test]
        public void RelationshipController_InitiateUserRelationship_Returns_Status_Initiated_When_TargetUsers_Relationship_Action_Is_None()
        {
            // Arrange
            var initiatingUser = new UserInfo { UserID = Constants.USER_TenId, PortalID = Constants.PORTAL_Zero };
            var targetUser = new UserInfo { UserID = Constants.USER_ElevenId, PortalID = Constants.PORTAL_Zero };
            var relationship = new Relationship { RelationshipId = Constants.SOCIAL_FollowerRelationshipID, RelationshipTypeId = Constants.SOCIAL_FollowerRelationshipTypeID, DefaultResponse = RelationshipStatus.Accepted };

            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Clear();
            this.dtUserRelationshipPreferences.Rows.Add(Constants.SOCIAL_PrefereceIDForUser11, Constants.USER_TenId, Constants.USER_ElevenId, RelationshipStatus.None);

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<RelationshipDirection>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.GetUserRelationshipPreference(It.IsAny<int>(), It.IsAny<int>())).Returns(this.dtUserRelationshipPreferences.CreateDataReader());
            mockDataService.Setup(md => md.GetAllRelationshipTypes()).Returns(this.dtRelationshipTypes.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            var userRelationship = relationshipController.InitiateUserRelationship(initiatingUser, targetUser, relationship);

            // Assert
            Assert.AreEqual(userRelationship.Status, RelationshipStatus.Pending);
        }

        [Test]
        [ExpectedException(typeof(UserRelationshipDoesNotExistException))]
        public void RelationshipController_RemoveUserRelationship_Throws_On_NonExistent_Relationship()
        {
            // Arrange

            // No UserRelationship between user10 and user11
            this.dtUserRelationships.Rows.Clear();

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>())).Returns(this.dtUserRelationships.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act, Assert
            relationshipController.RemoveUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11);
        }

        [Test]
        [ExpectedException(typeof(UserRelationshipDoesNotExistException))]
        public void RelationshipController_AcceptRelationship_Throws_On_NonExistent_Relationship()
        {
            // Arrange

            // No UserRelationship between user10 and user11
            this.dtUserRelationships.Rows.Clear();

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>())).Returns(this.dtUserRelationships.CreateDataReader());

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act, Assert
            relationshipController.AcceptUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11);
        }

        [Test]
        public void RelationshipController_AcceptUserRelationship_Calls_DataService_On_Valid_RelationshipID()
        {
            // Arrange

            // Any UserRelationship between user10 and user11
            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationships.Rows.Add(Constants.SOCIAL_UserRelationshipIDUser10User11, Constants.USER_TenId, Constants.USER_ElevenId, Constants.SOCIAL_FriendRelationshipID, RelationshipStatus.None);

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.SaveUserRelationship(It.IsAny<UserRelationship>(), It.IsAny<int>()));

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            relationshipController.AcceptUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11);

            // Assert
            mockDataService.Verify(ds => ds.SaveUserRelationship(It.IsAny<UserRelationship>(), It.IsAny<int>()));
        }

        [Test]
        public void RelationshipController_RemoveUserRelationship_Calls_DataService_On_Valid_RelationshipID()
        {
            // Arrange

            // Any UserRelationship between user10 and user11
            this.dtUserRelationships.Rows.Clear();
            this.dtUserRelationships.Rows.Add(Constants.SOCIAL_UserRelationshipIDUser10User11, Constants.USER_TenId, Constants.USER_ElevenId, Constants.SOCIAL_FriendRelationshipID, RelationshipStatus.None);

            // setup mock DataService
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetUserRelationship(It.IsAny<int>())).Returns(this.dtUserRelationships.CreateDataReader());
            mockDataService.Setup(md => md.DeleteUserRelationship(It.IsAny<int>()));

            var relationshipController = this.CreateRelationshipController(mockDataService);

            // Act
            relationshipController.RemoveUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11);

            // Assert
            mockDataService.Verify(ds => ds.DeleteUserRelationship(Constants.SOCIAL_UserRelationshipIDUser10User11));
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

        private Mock<IDataService> CreateMockDataServiceWithRelationshipTypes()
        {
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(md => md.GetAllRelationshipTypes()).Returns(this.dtRelationshipTypes.CreateDataReader());
            mockDataService.Setup(md => md.GetRelationshipsByPortalId(It.IsAny<int>())).Returns(this.dtRelationships.CreateDataReader());
            return mockDataService;
        }

        private void CreateLocalizationProvider()
        {
            var mockProvider = MockComponentProvider.CreateLocalizationProvider();
            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_RelationshipType_Deleted_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_RelationshipType_Deleted);

            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_RelationshipType_Updated_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_RelationshipType_Updated);

            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_Relationship_Deleted_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_Relationship_Deleted);
            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_Relationship_Updated_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_Relationship_Updated);

            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_UserRelationshipPreference_Deleted_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_UserRelationshipPreference_Deleted);
            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_UserRelationshipPreference_Updated_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_UserRelationshipPreference_Updated);

            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_UserRelationship_Deleted_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_UserRelationship_Deleted);
            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_UserRelationship_Added_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_UserRelationship_Added);
            mockProvider.Setup(l => l.GetString(Constants.LOCALIZATION_UserRelationship_Updated_Key, It.IsAny<string>()))
                        .Returns(Constants.LOCALIZATION_UserRelationship_Updated);
        }

        private RelationshipControllerImpl CreateRelationshipController()
        {
            var mockDataService = new Mock<IDataService>();
            return this.CreateRelationshipController(mockDataService);
        }

        private RelationshipControllerImpl CreateRelationshipController(Mock<IDataService> mockDataService)
        {
            var mockEventLogController = new Mock<IEventLogController>();
            return new RelationshipControllerImpl(mockDataService.Object, mockEventLogController.Object);
        }

        private RelationshipControllerImpl CreateRelationshipController(Mock<IEventLogController> mockEventLogController)
        {
            var mockDataService = new Mock<IDataService>();
            return new RelationshipControllerImpl(mockDataService.Object, mockEventLogController.Object);
        }

        private void SetupDataTables()
        {
            // RelationshipTypes
            this.dtRelationshipTypes = new DataTable("RelationshipTypes");
            var pkRelationshipTypeID = this.dtRelationshipTypes.Columns.Add("RelationshipTypeID", typeof(int));
            this.dtRelationshipTypes.Columns.Add("Name", typeof(string));
            this.dtRelationshipTypes.Columns.Add("Description", typeof(string));
            this.dtRelationshipTypes.Columns.Add("Direction", typeof(int));
            this.dtRelationshipTypes.Columns.Add("CreatedByUserID", typeof(int));
            this.dtRelationshipTypes.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtRelationshipTypes.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtRelationshipTypes.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            this.dtRelationshipTypes.PrimaryKey = new[] { pkRelationshipTypeID };

            this.dtRelationshipTypes.Rows.Add(DefaultRelationshipTypes.Friends, DefaultRelationshipTypes.Friends.ToString(), DefaultRelationshipTypes.Friends.ToString(), RelationshipDirection.TwoWay);
            this.dtRelationshipTypes.Rows.Add(DefaultRelationshipTypes.Followers, DefaultRelationshipTypes.Followers.ToString(), DefaultRelationshipTypes.Followers.ToString(), RelationshipDirection.OneWay);

            // Relationships
            this.dtRelationships = new DataTable("Relationships");
            var pkRelationshipID = this.dtRelationships.Columns.Add("RelationshipID", typeof(int));
            this.dtRelationships.Columns.Add("RelationshipTypeID", typeof(int));
            this.dtRelationships.Columns.Add("Name", typeof(string));
            this.dtRelationships.Columns.Add("Description", typeof(string));
            this.dtRelationships.Columns.Add("PortalID", typeof(int));
            this.dtRelationships.Columns.Add("UserID", typeof(int));
            this.dtRelationships.Columns.Add("DefaultResponse", typeof(int));
            this.dtRelationships.Columns.Add("CreatedByUserID", typeof(int));
            this.dtRelationships.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtRelationships.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtRelationships.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtRelationships.PrimaryKey = new[] { pkRelationshipID };

            // Create default Friend and Social Relationships
            this.dtRelationships.Rows.Add(Constants.SOCIAL_FriendRelationshipID, DefaultRelationshipTypes.Friends, DefaultRelationshipTypes.Friends.ToString(), DefaultRelationshipTypes.Friends.ToString(), Constants.PORTAL_Zero, Constants.USER_Null, RelationshipStatus.None);
            this.dtRelationships.Rows.Add(Constants.SOCIAL_FollowerRelationshipID, DefaultRelationshipTypes.Followers, DefaultRelationshipTypes.Followers.ToString(), DefaultRelationshipTypes.Followers.ToString(), Constants.PORTAL_Zero, Constants.USER_Null, RelationshipStatus.None);

            // UserRelationships
            this.dtUserRelationships = new DataTable("UserRelationships");
            var pkUserRelationshipID = this.dtUserRelationships.Columns.Add("UserRelationshipID", typeof(int));
            this.dtUserRelationships.Columns.Add("UserID", typeof(int));
            this.dtUserRelationships.Columns.Add("RelatedUserID", typeof(int));
            this.dtUserRelationships.Columns.Add("RelationshipID", typeof(int));
            this.dtUserRelationships.Columns.Add("Status", typeof(int));
            this.dtUserRelationships.Columns.Add("CreatedByUserID", typeof(int));
            this.dtUserRelationships.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtUserRelationships.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtUserRelationships.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtUserRelationships.PrimaryKey = new[] { pkUserRelationshipID };

            // UserRelationshipPreferences
            this.dtUserRelationshipPreferences = new DataTable("UserRelationshipPreferences");
            var pkPreferenceID = this.dtUserRelationshipPreferences.Columns.Add("PreferenceID", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("UserID", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("RelationshipID", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("DefaultResponse", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("CreatedByUserID", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("CreatedOnDate", typeof(DateTime));
            this.dtUserRelationshipPreferences.Columns.Add("LastModifiedByUserID", typeof(int));
            this.dtUserRelationshipPreferences.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            this.dtUserRelationshipPreferences.PrimaryKey = new[] { pkPreferenceID };
        }
    }
}
