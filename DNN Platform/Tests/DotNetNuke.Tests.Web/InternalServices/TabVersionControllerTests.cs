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
using System.Collections;

namespace DotNetNuke.Tests.Web.InternalServices
{
    [TestFixture]
    public class TabVersionControllerTests
    {

        private class TabVersionControllerTestable : TabVersionController
        {
        }

        private class DateUtilsTestable : DateUtils
        {
            public new static TimeZoneInfo GetDatabaseDateTimeOffset()
            {
                var timeZoneId = "UTC";
                return TimeZoneInfo.CreateCustomTimeZone(timeZoneId, new TimeSpan(0, 0, 0), timeZoneId, timeZoneId);
            }
        }

        private class TestCaseFactory
        {
            public static IEnumerable TestCases()
            {
                yield return new TestCaseData("Central European Standard Time", new DateTime(2018, 08, 15, 14, 0, 0));
                yield return new TestCaseData("Russian Standard Time", new DateTime(2018, 08, 15, 15, 0, 0));
                yield return new TestCaseData("SE Asia Standard Time", new DateTime(2018, 08, 15, 19, 0, 0));
            }
        }

        private Mock<ICBO> _mockCBO;
        private Mock<IUserController> _mockUserController;
        private Mock<IHostController> _mockHostController;

        private const int UserID = 1;
        private const int TabID = 99;
        
        // Assuming 12:00 Aug 15, 2018 server local time
        private readonly DateTime ServerCreateOnDate = new DateTime(2018, 08, 15, 12, 0, 0, DateTimeKind.Unspecified);
        

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.ResetContainer();
            SetupCBO();
            SetupHostController();
        }

        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void GetTabVersions_Verify_User_Preferred_TimeZone(string userPreferredTimeZone, DateTime expectedDateTime)
        {

            // Arrange
            SetupUserController(userPreferredTimeZone);

            // Act
            var tabVersionController = new TabVersionControllerTestable();
            var tabVersions = tabVersionController.GetTabVersions(TabID);
            var tabVersion = tabVersions.FirstOrDefault();
            var user = UserController.Instance.GetCurrentUserInfo();
            var userTimeZone = user.Profile.PreferredTimeZone;

            var localizedDate = TimeZoneInfo.ConvertTime(tabVersion.CreatedOnDate, DateUtilsTestable.GetDatabaseDateTimeOffset(), userTimeZone);

            // Assert
            Assert.AreEqual(localizedDate, expectedDateTime);
        }

        private void SetupCBO()
        {
            _mockCBO = new Mock<ICBO>();
            _mockCBO.Setup(c => c.GetCachedObject<List<TabVersion>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), It.IsAny<bool>()))
                .Returns(GetMockedTabVersions);
            CBO.SetTestableInstance(_mockCBO.Object);
        }

        private void SetupUserController(string timeZoneId)
        {
            _mockUserController = new Mock<IUserController>();
            _mockUserController.Setup<UserInfo>(userController => userController.GetCurrentUserInfo()).Returns(GetMockedUser(timeZoneId));
            UserController.SetTestableInstance(_mockUserController.Object);
        }

        private UserInfo GetMockedUser(string timeZoneId)
        {
            var profile = new UserProfile() {
                PreferredTimeZone = GetMockedUserTimeZone(timeZoneId)
            };

            profile.ProfileProperties.Add(new Entities.Profile.ProfilePropertyDefinition(99)
            {
                CreatedByUserID = UserID,
                PropertyDefinitionId = 20,
                PropertyCategory = "Preferences",
                PropertyName = "PreferredTimeZone",
                PropertyValue = GetMockedUserTimeZone(timeZoneId).Id
            });
            var user = new UserInfo()
            {
                Profile = profile,
                UserID = UserID,
                PortalID = 99
            };

            return user;
        }

        private TimeZoneInfo GetMockedUserTimeZone(string timeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
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
