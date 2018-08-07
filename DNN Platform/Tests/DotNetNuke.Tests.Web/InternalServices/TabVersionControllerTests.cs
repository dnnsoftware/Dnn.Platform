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

namespace DotNetNuke.Tests.Web.InternalServices
{
    [TestFixture]
    class TabVersionControllerTests
    {

        private class TabVersionControllerTestable : TabVersionController
        {
            protected override TimeZoneInfo GetDatabaseDateTimeOffset()
            {
                var timeZoneId = "UTC";
                return TimeZoneInfo.CreateCustomTimeZone(timeZoneId, new TimeSpan(0, 0, 0), timeZoneId, timeZoneId);
            }
        }

        private Mock<ICBO> _mockCBO;
        private Mock<IUserController> _mockUserController;
        private Mock<IHostController> _mockHostController;

        private const int UserID = 1;
        private const int TabID = 99;

        private int OffsetHours { get; set; }
        
        // Assuming 12:00 Aug 15, 2018 server local time
        private readonly DateTime ServerCreateOnDate = new DateTime(2018, 08, 15, 12, 0, 0, DateTimeKind.Unspecified);
        

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.ResetContainer();
            SetupCBO();
            SetupHostController();
        }

        [Test]
        [TestCase(-11, "2018-08-15 1:00:00.0")]
        [TestCase(-2, "2018-08-15 10:00:00.0")]
        [TestCase(12, "2018-08-16 0:00:00.0")]
        public void GetTabVersions_Verify_User_Preferred_TimeZone(int offsetHours, string expectedDateTimeString)
        {

            // Arrange
            OffsetHours = offsetHours;
            SetupUserController();

            // Act
            var tabVersionController = new TabVersionControllerTestable();
            var tabVersions = tabVersionController.GetTabVersions(TabID);
            var tabVersion = tabVersions.FirstOrDefault();
            var expectedDate = DateTime.Parse(expectedDateTimeString);
            
            // Assert
            Assert.AreEqual(tabVersion.CreateOnUserLocalDate, expectedDate);
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
            var profile = new UserProfile() {
                PreferredTimeZone = GetMockedUserTimeZone(OffsetHours)
            };

            profile.ProfileProperties.Add(new Entities.Profile.ProfilePropertyDefinition(99)
            {
                CreatedByUserID = UserID,
                PropertyDefinitionId = 20,
                PropertyCategory = "Preferences",
                PropertyName = "PreferredTimeZone",
                PropertyValue = GetMockedUserTimeZone(OffsetHours).Id
            });
            var user = new UserInfo()
            {
                Profile = profile,
                UserID = UserID,
                PortalID = 99
            };

            return user;
        }

        private TimeZoneInfo GetMockedUserTimeZone(int offsetHours)
        {
            var timeZoneKey = offsetHours != 0 ? GetTimeZoneId(offsetHours) : "UTC";
            var timeSpan = TimeSpan.FromHours(offsetHours);
            var timeZone = TimeZoneInfo.CreateCustomTimeZone(timeZoneKey, timeSpan, timeZoneKey, timeZoneKey);
            return timeZone;
        }

        private string GetTimeZoneId(int offsetHours)
        {
            var timeZoneOffset = Math.Abs(offsetHours).ToString().PadLeft(2, '0').PadLeft(3, offsetHours > 0 ? '+' : '-');
            return string.Format("UTC{0}", timeZoneOffset);
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
