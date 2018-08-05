using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;
using Moq;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities;

namespace DotNetNuke.Tests.Web.InternalServices
{
    [TestFixture]
    class TabVersionControllerTests
    {

        private Mock<ICBO> _mockCBO;
        private Mock<IUserController> _mockUserController;
        private Mock<IHostController> _mockHostController;

        private const int UserID = 1;
        private const int TabID = 99;

        // Rome TimeZone GMT +1, Daylight saving time period (+1 hour)
        private readonly TimeZoneInfo MockedServerTimeZone = TimeZoneInfo.CreateCustomTimeZone("ServerTimeZone", new TimeSpan(2, 0, 0), "MockedServerTimeZone", "MockedServerTime");

        // Ho Chi Minh City TimeZone GMT +7, No Daylight saving time
        private readonly TimeZoneInfo MockedUserTimeZone = TimeZoneInfo.CreateCustomTimeZone("ServerTimeZone", new TimeSpan(7, 0, 0), "MockedUserTimeZone", "MockedUserTime");

        private readonly DateTime ServerCreateOnDate = new DateTime(2018, 8, 4, 12, 0, 0);
        private readonly DateTime ExpectedCreateOnUserLocalDate = new DateTime(2018, 8, 4, 17, 0, 0);

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.ResetContainer();
            SetupCBO();
            SetupUserController();
            SetupHostController();
        }

        [Test]
        public void GetTabVersions_Verify_User_Preferred_TimeZone()
        {
            var tabVersions = TabVersionController.Instance.GetTabVersions(TabID);

            var tabVersion = tabVersions.FirstOrDefault();

            Assert.AreEqual(tabVersion.CreateOnUserLocalDate, ExpectedCreateOnUserLocalDate);
        }

        private void SetupCBO()
        {
            _mockCBO = new Mock<ICBO>();
            _mockCBO.Setup(c => c.GetCachedObject<List<TabVersion>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), It.IsAny<bool>()))
                .Returns(() =>
                {
                    return GetMockedTabVersions();
                });
            CBO.SetTestableInstance(_mockCBO.Object);
        }

        private void SetupUserController()
        {
            _mockUserController = new Mock<IUserController>();
            _mockUserController.Setup<UserInfo>(userController => userController.GetCurrentUserInfo()).Returns(GetMockedUser());
            UserController.SetTestableInstance(_mockUserController.Object);
        }

        private UserInfo GetMockedUser()
        {
            var _mockUser = new Mock<UserInfo>();
            _mockUser.Object.UserID = UserID;
            _mockUser.Object.PortalID = 99;
            _mockUser.Object.Profile = new UserProfile() { PreferredTimeZone = MockedUserTimeZone };

            return _mockUser.Object;
        }

        private TabVersion GetMockedTabVersion()
        {
            var tabVersion = new TabVersion
            {
                IsPublished = true,
                TabId = TabID,
                TabVersionId = 1,
                Version = 1,
                CreatedByUserID = UserID
            };
            tabVersion.GetType().BaseType.GetProperty("CreatedOnDate").SetValue(tabVersion, ServerCreateOnDate, null);

            return tabVersion;
        }

        private List<TabVersion> GetMockedTabVersions()
        {
            return new List<TabVersion>()
            {
                GetMockedTabVersion()
            };
        }

        private void SetupHostController()
        {
            _mockHostController = new Mock<IHostController>();
            _mockHostController.Setup(c => c.GetString(It.IsRegex("PerformanceSetting"))).Returns(Globals.PerformanceSettings.LightCaching.ToString());
            HostController.RegisterInstance(_mockHostController.Object);
        }
    }

}
