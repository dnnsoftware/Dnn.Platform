// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Portal
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Data;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Tests.Utilities.Mocks;
    using Moq;
    using NUnit.Framework;

    // ReSharper disable InconsistentNaming
    [TestFixture]
    public class PortalGroupControllerTests
    {
        private Mock<DataProvider> _mockData;
#pragma warning disable 649
        private UserCopiedCallback userCopied;
#pragma warning restore 649

        [SetUp]
        public void SetUp()
        {
            this._mockData = MockComponentProvider.CreateDataProvider();
            DataTable hostSettingsTable = new DataTable("HostSettings");

            var nameCol = hostSettingsTable.Columns.Add("SettingName");
            hostSettingsTable.Columns.Add("SettingValue");
            hostSettingsTable.Columns.Add("SettingIsSecure");
            hostSettingsTable.PrimaryKey = new[] { nameCol };

            hostSettingsTable.Rows.Add("PerformanceSetting", "0", false);
            this._mockData.Setup(c => c.GetHostSettings()).Returns(hostSettingsTable.CreateDataReader());
        }

        [TearDown]
        public void TearDown()
        {
            MockComponentProvider.ResetContainer();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_Constructor_Throws_On_Null_DataService()
        {
            // Arrange
            var mockPortalController = new Mock<IPortalController>();

            // Act, Assert
            new PortalGroupController(null, mockPortalController.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_Constructor_Throws_On_Null_PortalController()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();

            // Act, Assert
            new PortalGroupController(mockDataService.Object, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Null_PortalGroup()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            // Act, Assert
            controller.AddPortalToGroup(portal, null, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Null_Portal()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
            var portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            controller.AddPortalToGroup(null, portalGroup, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalGroupId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = -1 };

            // Act, Assert
            controller.AddPortalToGroup(portal, portalGroup, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PortalGroupController_AddPortalToGroup_Throws_On_Negative_PortalId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portal = new PortalInfo { PortalID = -1 };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            controller.AddPortalToGroup(portal, portalGroup, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_AddPortalGroup_Throws_On_Null_PortalGroup()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act, Assert
            controller.AddPortalGroup(null);
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Calls_DataService_On_Valid_Arguments()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.AddPortalGroup(portalGroup, It.IsAny<int>()));
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Calls_PortalController_On_Valid_Arguments()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            var masterPortal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };
            var mockPortalController = new Mock<IPortalController>();
            mockPortalController.Setup(pc => pc.GetPortal(Constants.PORTAL_ValidPortalId))
                        .Returns(masterPortal);
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            mockPortalController.Verify(pc => pc.GetPortal(portalGroup.MasterPortalId));
            mockPortalController.Verify(pc => pc.UpdatePortalInfo(masterPortal));
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Returns_ValidId_On_Valid_PortalGroup()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddPortalGroup(It.IsAny<PortalGroupInfo>(), It.IsAny<int>())).Returns(Constants.PORTALGROUP_AddPortalGroupId);
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            int portalGroupId = controller.AddPortalGroup(portalGroup);

            // Assert
            Assert.AreEqual(Constants.PORTALGROUP_AddPortalGroupId, portalGroupId);
        }

        [Test]
        public void PortalGroupController_AddPortalGroup_Sets_PortalGroupId_Property_On_Valid_PortalGroup()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.AddPortalGroup(It.IsAny<PortalGroupInfo>(), It.IsAny<int>())).Returns(Constants.PORTALGROUP_AddPortalGroupId);
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId;

            // Act
            controller.AddPortalGroup(portalGroup);

            // Assert
            Assert.AreEqual(Constants.PORTALGROUP_AddPortalGroupId, portalGroup.PortalGroupId);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_DeletePortalGroup_Throws_On_Null_PortalGroup()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act, Assert
            controller.DeletePortalGroup(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PortalGroupController_DeletePortalGroup_Throws_On_Negative_PortalGroupId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Null.NullInteger;

            // Act, Assert
            controller.DeletePortalGroup(portalGroup);
        }

        [Test]
        public void PortalGroupController_DeletePortalGroup_Calls_DataService_On_Valid_PortalGroupId()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            PortalGroupInfo portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_DeletePortalGroupId;

            // Act
            controller.DeletePortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.DeletePortalGroup(portalGroup));
        }

        [Test]

        public void PortalGroupController_GetPortalGroups_Calls_DataService()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            var mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(0, Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act
            controller.GetPortalGroups();

            // Assert
            mockDataService.Verify(ds => ds.GetPortalGroups());
        }

        [Test]
        public void PortalGroupController_GetPortalGroups_Returns_EmptyList_If_No_Items()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(0, Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act
            IEnumerable<PortalGroupInfo> portalGroups = controller.GetPortalGroups();

            // Assert
            Assert.AreEqual(0, portalGroups.Count());
        }

        [Test]
        public void PortalGroupController_GetPortalGroups_Returns_List_Of_PortalGroups()
        {
            // Arrange
            var mockCache = MockComponentProvider.CreateNew<CachingProvider>();
            mockCache.Setup(c => c.GetItem(CachingProvider.GetCacheKey(DataCache.PortalGroupsCacheKey))).Returns(null);

            Mock<IDataService> mockDataService = new Mock<IDataService>();
            mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(CreateValidPortalGroupsReader(
                Constants.PORTALGROUP_ValidPortalGroupCount,
                Constants.USER_ValidId));

            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act
            IEnumerable<PortalGroupInfo> portalGroups = controller.GetPortalGroups();

            // Assert
            Assert.AreEqual(Constants.PORTALGROUP_ValidPortalGroupCount, portalGroups.Count());
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Null_PortalGroup()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            // Act, Assert
            controller.RemovePortalFromGroup(portal, null, false, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Null_Portal()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);
            var portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            controller.RemovePortalFromGroup(null, portalGroup, false, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalGroupId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portal = new PortalInfo { PortalID = Constants.PORTAL_ValidPortalId };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = -1 };

            // Act, Assert
            controller.RemovePortalFromGroup(portal, portalGroup, false, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void PortalGroupController_RemovePortalFromGroup_Throws_On_Negative_PortalId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portal = new PortalInfo { PortalID = -1 };

            PortalGroupInfo portalGroup = new PortalGroupInfo { PortalGroupId = Constants.PORTALGROUP_ValidPortalGroupId };

            // Act, Assert
            controller.RemovePortalFromGroup(portal, portalGroup, false, this.userCopied);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PortalGroupController_UpdatePortalGroup_Throws_On_Null_PortalGroup()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            // Act, Assert
            controller.UpdatePortalGroup(null);
        }

        [Test]
        public void PortalGroupController_UpdatePortalGroup_Throws_On_Negative_PortalGroupId()
        {
            // Arrange
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portalGroup = new PortalGroupInfo();
            portalGroup.PortalGroupId = Null.NullInteger;

            Assert.Throws<ArgumentOutOfRangeException>(() => controller.UpdatePortalGroup(portalGroup));
        }

        [Test]
        public void PortalGroupController_UpdatePortalGroup_Calls_DataService_On_Valid_PortalGroup()
        {
            // Arrange
            MockComponentProvider.CreateNew<CachingProvider>();
            var mockDataService = new Mock<IDataService>();
            var mockPortalController = new Mock<IPortalController>();
            var controller = new PortalGroupController(mockDataService.Object, mockPortalController.Object);

            var portalGroup = CreateValidPortalGroup();
            portalGroup.PortalGroupId = Constants.PORTALGROUP_UpdatePortalGroupId;
            portalGroup.PortalGroupName = Constants.PORTALGROUP_UpdateName;
            portalGroup.PortalGroupDescription = Constants.PORTALGROUP_UpdateDescription;

            // Act
            controller.UpdatePortalGroup(portalGroup);

            // Assert
            mockDataService.Verify(ds => ds.UpdatePortalGroup(portalGroup, It.IsAny<int>()));
        }

        private static DataTable CreatePortalGroupTable()
        {
            // Create Categories table.
            DataTable table = new DataTable();

            // Create columns, ID and Name.
            DataColumn idColumn = table.Columns.Add("PortalGroupID", typeof(int));
            table.Columns.Add("MasterPortalID", typeof(int));
            table.Columns.Add("PortalGroupName", typeof(string));
            table.Columns.Add("PortalGroupDescription", typeof(string));
            table.Columns.Add("AuthenticationDomain", typeof(string));
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            // Set the ID column as the primary key column.
            table.PrimaryKey = new[] { idColumn };

            return table;
        }

        private static string GetName(int i)
        {
            return string.Format(string.Format(Constants.PORTALGROUP_ValidNameFormat, i));
        }

        private static string GetDescription(int i)
        {
            return string.Format(string.Format(Constants.PORTALGROUP_ValidDescriptionFormat, i));
        }

        private static PortalGroupInfo CreateValidPortalGroup()
        {
            var portalGroup = new PortalGroupInfo
            {
                PortalGroupName = Constants.PORTALGROUP_ValidName,
                PortalGroupDescription = Constants.PORTALGROUP_ValidDescription,
                MasterPortalId = Constants.PORTAL_ValidPortalId,
            };
            return portalGroup;
        }

        private static IDataReader CreateValidPortalGroupsReader(int count, int startUserId)
        {
            DataTable table = CreatePortalGroupTable();
            for (int i = Constants.PORTALGROUP_ValidPortalGroupId; i < Constants.PORTALGROUP_ValidPortalGroupId + count; i++)
            {
                string name = (count == 1) ? Constants.PORTALGROUP_ValidName : GetName(i);
                string description = (count == 1) ? Constants.PORTALGROUP_ValidDescription : GetDescription(i);
                const string domain = "mydomain.com";
                int userId = (startUserId == Null.NullInteger) ? Constants.USER_ValidId + i : startUserId;

                table.Rows.Add(new object[]
                                   {
                                       i,
                                       -1,
                                       name,
                                       description,
                                       domain,
                                       userId,
                                   });
            }

            return table.CreateDataReader();
        }
    }

    // ReSharper restore InconsistentNaming
}
