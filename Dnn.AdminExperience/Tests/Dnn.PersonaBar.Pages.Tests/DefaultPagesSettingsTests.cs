// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Pages.Tests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.SiteSettings.Services;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Security;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class DefaultPagesSettingsTests
    {
        private const int PortalId = 0;
        private const string CultureCode = "en-US";
        private const string HeaderContent = "<meta name=\"description\" content=\"Migrated header\" />";

        private Dictionary<string, string> portalSettings;
        private FakeServiceProvider serviceProvider;
        private HttpConfiguration configuration;
        private SiteSettingsController controller;

        [SetUp]
        public void SetUp()
        {
            this.portalSettings = new Dictionary<string, string>();
            var portalController = new Mock<IPortalController>();
            portalController.Setup(c => c.GetCurrentPortalSettings()).Returns(new PortalSettings { PortalId = PortalId });
            portalController.Setup(c => c.GetPortal(PortalId, CultureCode)).Returns(new PortalInfo { PortalID = PortalId, CultureCode = CultureCode });
            portalController.Setup(c => c.GetPortalSettings(PortalId, CultureCode)).Returns(this.portalSettings);
            var userController = new Mock<IUserController>();
            userController.Setup(c => c.GetCurrentUserInfo()).Returns(new UserInfo { IsSuperUser = true });
            var localeController = new Mock<ILocaleController>();
            localeController.Setup(c => c.GetLocale(PortalId, CultureCode)).Returns(new Locale { Code = CultureCode });

            PortalController.SetTestableInstance(portalController.Object);
            UserController.SetTestableInstance(userController.Object);
            LocaleController.SetTestableInstance(localeController.Object);
            this.serviceProvider = FakeServiceProvider.Setup(services =>
            {
                services.AddSingleton(portalController.Object);
                services.AddSingleton(userController.Object);
                services.AddSingleton(localeController.Object);
                services.AddSingleton<IApplicationStatusInfo>(new FakeApplicationStatusInfo());
            });

            var hostSettings = Mock.Of<IHostSettings>();
            var cryptographyProvider = Mock.Of<ICryptographyProvider>();
            var eventLogger = Mock.Of<IEventLogger>();
            this.configuration = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/API/PersonaBar/SiteSettings/GetDefaultPagesSettings?portalId=0&cultureCode=en-US");
            request.SetConfiguration(this.configuration);
            this.controller = new SiteSettingsController(
                Mock.Of<INavigationManager>(),
                Mock.Of<IApplicationStatusInfo>(),
                new PersonalizationController(hostSettings, cryptographyProvider),
                hostSettings,
                new ListController(eventLogger, hostSettings),
                portalController.Object,
                Mock.Of<IPortalGroupController>(),
                cryptographyProvider,
                Mock.Of<IHostSettingsService>(),
                eventLogger,
                Mock.Of<IPortalAliasService>(),
                Mock.Of<RoleProvider>(),
                Mock.Of<ITabController>())
            {
                Request = request,
            };
        }

        [TearDown]
        public void TearDown()
        {
            this.controller?.Dispose();
            this.configuration?.Dispose();
            this.serviceProvider?.Dispose();
            PortalController.ClearInstance();
            UserController.ClearInstance();
            LocaleController.ClearInstance();
        }

        [TestCase(false)]
        [TestCase(true)]
        public async Task GetDefaultPagesSettings_WhenPageHeadTextIsMissing_ReturnsSettings(bool hasMigratedHeader)
        {
            if (hasMigratedHeader)
            {
                this.portalSettings["PageHeaderTag_Default"] = HeaderContent;
            }

            using var response = this.controller.GetDefaultPagesSettings(PortalId, CultureCode);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var settings = JObject.Parse(await response.Content.ReadAsStringAsync())["Settings"];
            using (Assert.EnterMultipleScope())
            {
                Assert.That((int)settings["PortalId"], Is.EqualTo(PortalId));
                Assert.That((string)settings["CultureCode"], Is.EqualTo(CultureCode));
                Assert.That((string)settings["PageHeadText"], Is.Empty);
                Assert.That((JArray)settings["PageHeaderTags"], Has.Count.EqualTo(hasMigratedHeader ? 1 : 0));
                Assert.That(this.portalSettings.ContainsKey("PageHeadText"), Is.False);
            }

            if (hasMigratedHeader)
            {
                var header = settings["PageHeaderTags"][0];
                using (Assert.EnterMultipleScope())
                {
                    Assert.That((string)header["name"], Is.EqualTo("Default"));
                    Assert.That((string)header["content"], Is.EqualTo(HeaderContent));
                }
            }
        }

        [TestCase("<meta name=\"description\" content=\"Legacy header\" />")]
        [TestCase("false")]
        [TestCase("")]
        [TestCase(null)]
        public async Task GetDefaultPagesSettings_WhenPageHeadTextExists_PreservesItsValue(string pageHeadText)
        {
            this.portalSettings["PageHeadText"] = pageHeadText;
            this.portalSettings["PageHeaderTag_Default"] = HeaderContent;

            using var response = this.controller.GetDefaultPagesSettings(PortalId, CultureCode);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var settings = JObject.Parse(await response.Content.ReadAsStringAsync())["Settings"];
            using (Assert.EnterMultipleScope())
            {
                Assert.That((string)settings["PageHeadText"], Is.EqualTo(pageHeadText));
                Assert.That(this.portalSettings["PageHeadText"], Is.EqualTo(pageHeadText));
                Assert.That((string)settings["PageHeaderTags"][0]["content"], Is.EqualTo(HeaderContent));
            }
        }
    }
}
