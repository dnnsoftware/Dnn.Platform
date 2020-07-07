// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Common
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class NavigationManagerTests
    {
        private const int TabID = 100;
        private const int PortalID = 7;
        private const string DefaultURLPattern = "/Default.aspx?tabid={0}";
        private const string DefaultSuperTabPattern = "&portalid={0}";
        private const string ControlKeyPattern = "&ctl={0}";
        private const string LanguagePattern = "&language={0}";
        private INavigationManager _navigationManager;

        [TestFixtureSetUp]
        public void Setup()
        {
            this._navigationManager = new NavigationManager(PortalControllerMock());
            TabController.SetTestableInstance(TabControllerMock());
            LocaleController.SetTestableInstance(LocaleControllerMock());

            IPortalController PortalControllerMock()
            {
                var mockPortalController = new Mock<IPortalController>();
                mockPortalController
                    .Setup(x => x.GetCurrentPortalSettings())
                    .Returns(PortalSettingsMock());
                mockPortalController
                    .Setup(x => x.GetCurrentSettings())
                    .Returns(PortalSettingsMock());

                return mockPortalController.Object;

                PortalSettings PortalSettingsMock()
                {
                    var portalSettings = new PortalSettings
                    {
                        PortalId = PortalID,
                        ActiveTab = new TabInfo
                        {
                            TabID = TabID
                        },
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
                        CultureCode = "en-US",
                    });

                return mockTabController.Object;
            }

            ILocaleController LocaleControllerMock()
            {
                var mockLocaleController = new Mock<ILocaleController>();
                mockLocaleController
                    .Setup(x => x.GetLocales(It.IsAny<int>()))
                    .Returns(new Dictionary<string, Locale>
                    {
                        { "en-US", new Locale() },
                        { "TEST", new Locale() },
                    });

                return mockLocaleController.Object;
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            this._navigationManager = null;
            TabController.ClearInstance();
            LocaleController.ClearInstance();
        }

        [Test]
        public void NavigateUrlTest()
        {
            var expected = string.Format(DefaultURLPattern, TabID);
            var actual = this._navigationManager.NavigateURL();

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_CustomTabID(int tabId)
        {
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this._navigationManager.NavigateURL(tabId);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NavigateUrl_CustomTab_NotSuperTab()
        {
            var customTabId = 55;
            var expected = string.Format(DefaultURLPattern, customTabId);
            var actual = this._navigationManager.NavigateURL(customTabId, false);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_CustomTab_IsSuperTab(int tabId)
        {
            var expected = string.Format(DefaultURLPattern, tabId) + string.Format(DefaultSuperTabPattern, PortalID);
            var actual = this._navigationManager.NavigateURL(tabId, true);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [Ignore]
        public void NavigateUrl_ControlKey_AccessDenied()
        {
            // TODO - We can't properly test this until we migrate
            // Globals.AccessDeniedURL to an interface in the abstraction
            // project. The dependencies go very deep and make it very
            // difficult to properly test just the NavigationManager logic.
            var actual = this._navigationManager.NavigateURL("Access Denied");
        }

        [Test]
        public void NavigateUrl_ControlKey()
        {
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, TabID) + string.Format(ControlKeyPattern, controlKey);
            var actual = this._navigationManager.NavigateURL(controlKey);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NavigateUrl_ControlKey_EmptyAdditionalParameter()
        {
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, TabID) + string.Format(ControlKeyPattern, controlKey);
            var actual = this._navigationManager.NavigateURL(controlKey, new string[0]);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void NavigateUrl_ControlKey_SingleAdditionalParameter()
        {
            var controlKey = "My-Control-Key";
            var parameters = new string[] { "My-Parameter" };
            var expected = string.Format(DefaultURLPattern, TabID) +
                string.Format(ControlKeyPattern, controlKey) +
                $"&{parameters[0]}";
            var actual = this._navigationManager.NavigateURL(controlKey, parameters);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void NavigateUrl_ControlKey_MultipleAdditionalParameter(int count)
        {
            string[] parameters = new string[count];
            for (int index = 0; index < count; index++)
            {
                parameters[index] = $"My-Parameter{index}";
            }

            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, TabID) +
                string.Format(ControlKeyPattern, controlKey) +
                parameters.Select(s => $"&{s}").Aggregate((x, y) => $"{x}{y}");
            var actual = this._navigationManager.NavigateURL(controlKey, parameters);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_ControlKey(int tabId)
        {
            var controlKey = "My-Control-Key";
            var expected = string.Format(DefaultURLPattern, tabId) + string.Format(ControlKeyPattern, controlKey);
            var actual = this._navigationManager.NavigateURL(tabId, controlKey);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_EmptyControlKey(int tabId)
        {
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this._navigationManager.NavigateURL(tabId, string.Empty);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        [TestCase(11)]
        public void NavigateUrl_TabID_NullControlKey(int tabId)
        {
            var expected = string.Format(DefaultURLPattern, tabId);
            var actual = this._navigationManager.NavigateURL(tabId, string.Empty);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabID_ControlKey_Parameter(int count, string controlKey)
        {
            string[] parameters = new string[count];
            for (int index = 0; index < count; index++)
            {
                parameters[index] = $"My-Parameter{index}";
            }

            var customTabId = 51;
            var expected = string.Format(DefaultURLPattern, customTabId) +
                string.Format(ControlKeyPattern, controlKey);

            if (parameters.Length > 0)
            {
                expected += parameters.Select(s => $"&{s}").Aggregate((x, y) => $"{x}{y}");
            }

            var actual = this._navigationManager.NavigateURL(customTabId, controlKey, parameters);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabID_ControlKey_NullParameter(int tabId, string controlKey)
        {
            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey);

            var actual = this._navigationManager.NavigateURL(tabId, controlKey, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabId_NullSettings_ControlKey(int tabId, string controlKey)
        {
            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey);

            var actual = this._navigationManager.NavigateURL(tabId, default(IPortalSettings), controlKey, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }

        [TestCase(0, "My-Control-Key-0")]
        [TestCase(1, "My-Control-Key-1")]
        [TestCase(2, "My-Control-Key-2")]
        [TestCase(3, "My-Control-Key-3")]
        [TestCase(4, "My-Control-Key-4")]
        [TestCase(5, "My-Control-Key-5")]
        [TestCase(6, "My-Control-Key-6")]
        [TestCase(7, "My-Control-Key-7")]
        [TestCase(8, "My-Control-Key-8")]
        [TestCase(9, "My-Control-Key-9")]
        [TestCase(10, "My-Control-Key-10")]
        public void NavigateUrl_TabId_Settings_ControlKey(int tabId, string controlKey)
        {
            var mockSettings = new Mock<IPortalSettings>();
            mockSettings
                .Setup(x => x.ContentLocalizationEnabled)
                .Returns(true);

            var expected = string.Format(DefaultURLPattern, tabId) +
                string.Format(ControlKeyPattern, controlKey) +
                string.Format(LanguagePattern, "en-US");

            var actual = this._navigationManager.NavigateURL(tabId, mockSettings.Object, controlKey, null);

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected, actual);
        }
    }
}
