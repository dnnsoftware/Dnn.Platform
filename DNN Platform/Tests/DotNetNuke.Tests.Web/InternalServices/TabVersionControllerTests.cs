// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.InternalServices
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Tests.Utilities.Fakes;
    using DotNetNuke.Tests.Utilities.Mocks;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class TabVersionControllerTests
    {
        private const int UserId = 1;
        private const int TabId = 99;
        private static readonly DateTime ServerCreateOnDate = new DateTime(2018, 08, 15, 12, 0, 0, DateTimeKind.Unspecified);

        private Mock<ICBO> mockCBO;
        private Mock<IUserController> mockUserController;
        private Mock<IHostController> mockHostController;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            MockComponentProvider.ResetContainer();

            this.mockCBO = new Mock<ICBO>();
            this.mockUserController = new Mock<IUserController>();
            this.mockHostController = new Mock<IHostController>();
            this.mockHostController.As<IHostSettingsService>();

            this.SetupCBO();
            this.SetupHostController();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockCBO.Object);
                    services.AddSingleton(this.mockUserController.Object);
                    services.AddSingleton(this.mockHostController.Object);
                    services.AddSingleton((IHostSettingsService)this.mockHostController.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            this.serviceProvider.Dispose();
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), nameof(TestCaseFactory.TestCases))]
        public void GetTabVersions_Verify_User_Preferred_TimeZone(string userPreferredTimeZone, DateTime expectedDateTime)
        {
            // Arrange
            this.SetupUserController(userPreferredTimeZone);

            // Act
            var tabVersionController = new TabVersionControllerTestable();
            var tabVersions = tabVersionController.GetTabVersions(TabId);
            var tabVersion = tabVersions.First();
            var user = UserController.Instance.GetCurrentUserInfo();
            var userTimeZone = user.Profile.PreferredTimeZone;

            var localizedDate = TimeZoneInfo.ConvertTime(tabVersion.CreatedOnDate, DateUtilsTestable.GetDatabaseDateTimeOffset(), userTimeZone);

            // Assert
            Assert.That(expectedDateTime, Is.EqualTo(localizedDate));
        }

        private void SetupCBO()
        {
            this.mockCBO.Setup(c => c.GetCachedObject<List<TabVersion>>(It.IsAny<CacheItemArgs>(), It.IsAny<CacheItemExpiredCallback>(), It.IsAny<bool>()))
                .Returns(this.GetMockedTabVersions);
            CBO.SetTestableInstance(this.mockCBO.Object);
        }

        private void SetupUserController(string timeZoneId)
        {
            this.mockUserController.Setup(userController => userController.GetCurrentUserInfo()).Returns(this.GetMockedUser(timeZoneId));
            UserController.SetTestableInstance(this.mockUserController.Object);
        }

        private UserInfo GetMockedUser(string timeZoneId)
        {
            var profile = new UserProfile()
            {
                PreferredTimeZone = this.GetMockedUserTimeZone(timeZoneId),
            };

            profile.ProfileProperties.Add(new Entities.Profile.ProfilePropertyDefinition(99)
            {
                CreatedByUserID = UserId,
                PropertyDefinitionId = 20,
                PropertyCategory = "Preferences",
                PropertyName = "PreferredTimeZone",
                PropertyValue = this.GetMockedUserTimeZone(timeZoneId).Id,
            });
            var user = new UserInfo()
            {
                Profile = profile,
                UserID = UserId,
                PortalID = 99,
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
                TabId = TabId,
                TabVersionId = 1,
                Version = 1,
                CreatedByUserID = UserId,
            };
            tabVersion.GetType().BaseType.GetProperty("CreatedOnDate").SetValue(tabVersion, ServerCreateOnDate, null);

            return tabVersion;
        }

        private List<TabVersion> GetMockedTabVersions()
        {
            return new List<TabVersion>()
            {
                this.GetMockedTabVersion(),
            };
        }

        private void SetupHostController()
        {
            this.mockHostController.Setup(c => c.GetString(It.IsRegex("PerformanceSetting"))).Returns(Globals.PerformanceSettings.LightCaching.ToString());
        }

        private class TabVersionControllerTestable : TabVersionController
        {
        }

        private class DateUtilsTestable : DateUtils
        {
            public static new TimeZoneInfo GetDatabaseDateTimeOffset()
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

        // Assuming 12:00 Aug 15, 2018 server local time
    }
}
