using DotNetNuke.Abstractions;
using DotNetNuke.Abstractions.Portals;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Tests.Core.Common
{
    [TestFixture]
    public class NavigationManagerTests
    {
        private INavigationManager _navigationManager;
        private const int TabID = 100;

        [TestFixtureSetUp]
        public void Setup()
        {

            _navigationManager = new NavigationManager(PortalControllerMock());
            TabController.SetTestableInstance(TabControllerMock());
            LocaleController.SetTestableInstance(LocaleControllerMock());

            IPortalController PortalControllerMock()
            {
                var mockPortalController = new Mock<IPortalController>();
                mockPortalController
                    .Setup(x => x.GetCurrentPortalSettings())
                    .Returns(PortalSettingsMock());

                return mockPortalController.Object;

                PortalSettings PortalSettingsMock()
                {
                    var portalSettings = new PortalSettings();
                    portalSettings.ActiveTab = new TabInfo
                    {
                        TabID = TabID
                    };

                    return portalSettings;
                }
            }
            ITabController TabControllerMock()
            {
                var mockTabController = new Mock<ITabController>();
                mockTabController
                    .Setup(x => x.GetTabsByPortal(Null.NullInteger))
                    .Returns(default(TabCollection));
                mockTabController
                    .Setup(x => x.GetTab(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .Returns(new TabInfo
                    {
                        CultureCode = "en-US"
                    });

                return mockTabController.Object;
            }
            ILocaleController LocaleControllerMock()
            {
                var mockLocaleController = new Mock<ILocaleController>();
                mockLocaleController
                    .Setup(x => x.GetLocales(It.IsAny<int>()))
                    .Returns(new Dictionary<string, Locale>());

                return mockLocaleController.Object;
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            _navigationManager = null;
            TabController.ClearInstance();
            LocaleController.ClearInstance();
        }

        [Test]
        public void NavigateUrlTest()
        {
            var expected = "/Default.aspx?tabid=100";
            var actual = _navigationManager.NavigateURL();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
