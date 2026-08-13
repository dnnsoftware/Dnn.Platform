// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Sitemap
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Sitemap;

    using Moq;
    using NUnit.Framework;

    using NewBrowserTypes = DotNetNuke.Abstractions.Urls.BrowserTypes;

    [TestFixture]
    public class CoreSitemapProviderTests
    {
        private Mock<IGlobals> globals;
        private PortalSettings portalSettings;
        private TestCoreSitemapProvider provider;

        [SetUp]
        public void SetUp()
        {
            this.globals = new Mock<IGlobals>();
            this.portalSettings = new PortalSettings();
            this.provider = new TestCoreSitemapProvider();
            TestableGlobals.SetTestableInstance(this.globals.Object);
        }

        [TearDown]
        public void TearDown()
        {
            TestableGlobals.ClearInstance();
        }

        [Test]
        public void IsTabEligibleForSitemap_WhenAllowIndexIsFalse_ReturnsFalse()
        {
            TabInfo tab = CreateTab("fr-FR", allowIndex: false);

            bool result = this.provider.IsTabEligibleForSitemap(tab, DateTime.UtcNow);

            Assert.That(result, Is.False);
        }

        [Test]
        public void GetAlternateUrls_ExcludesLocalizedPagesThatDisallowIndexing()
        {
            TabInfo defaultTab = CreateTab("en-US");
            TabInfo indexableTab = CreateTab("fr-FR");
            TabInfo noIndexTab = CreateTab("es-ES", allowIndex: false);
            this.SetupUrl("en-US", "https://example.com/page");
            this.SetupUrl("fr-FR", "https://example.fr/page");

            List<AlternateUrl> result = this.provider.GetAlternateUrls(
                defaultTab,
                new[] { indexableTab, noIndexTab },
                this.portalSettings,
                DateTime.UtcNow);

            Assert.Multiple(() =>
            {
                Assert.That(result.Select(alternate => alternate.Language), Is.EqualTo(new[] { "fr-FR", "en-US" }));
                Assert.That(result, Has.None.Matches<AlternateUrl>(alternate => alternate.Language == "es-ES"));
                this.globals.Verify(
                    global => global.NavigateURL(
                        It.IsAny<int>(),
                        It.IsAny<bool>(),
                        It.IsAny<PortalSettings>(),
                        It.IsAny<string>(),
                        "es-ES",
                        It.IsAny<string[]>()),
                    Times.Never);
            });
        }

        [Test]
        public void GetAlternateUrls_UsesCultureSpecificUrls()
        {
            TabInfo defaultTab = CreateTab("en-US");
            TabInfo localizedTab = CreateTab("fr-FR");
            this.SetupUrl("en-US", "https://www.example.com/page");
            this.SetupUrl("fr-FR", "https://www.example.fr/page");

            List<AlternateUrl> result = this.provider.GetAlternateUrls(
                defaultTab,
                new[] { localizedTab },
                this.portalSettings,
                DateTime.UtcNow);

            Assert.Multiple(() =>
            {
                Assert.That(result, Has.Count.EqualTo(2));
                Assert.That(result[0].Language, Is.EqualTo("fr-FR"));
                Assert.That(result[0].Url, Is.EqualTo("https://www.example.fr/page"));
                Assert.That(result[1].Language, Is.EqualTo("en-US"));
                Assert.That(result[1].Url, Is.EqualTo("https://www.example.com/page"));
            });
        }

        [Test]
        public void GetAlternateUrls_WhenDefaultPageDisallowsIndexingAndOnlyOneLocalizedPageIsEligible_ReturnsNoAlternates()
        {
            TabInfo defaultTab = CreateTab("en-US", allowIndex: false);
            TabInfo localizedTab = CreateTab("fr-FR");
            this.SetupUrl("fr-FR", "https://www.example.fr/page");

            List<AlternateUrl> result = this.provider.GetAlternateUrls(
                defaultTab,
                new[] { localizedTab },
                this.portalSettings,
                DateTime.UtcNow);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Empty);
                this.globals.Verify(
                    global => global.NavigateURL(
                        It.IsAny<int>(),
                        It.IsAny<bool>(),
                        It.IsAny<PortalSettings>(),
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<string[]>()),
                    Times.Never);
            });
        }

        [Test]
        public void GetAlternateUrls_WhenDefaultPageDisallowsIndexingAndTwoLocalizedPagesAreEligible_ReturnsLocalizedAlternates()
        {
            TabInfo defaultTab = CreateTab("en-US", allowIndex: false);
            TabInfo frenchTab = CreateTab("fr-FR");
            TabInfo spanishTab = CreateTab("es-ES");
            this.SetupUrl("fr-FR", "https://www.example.fr/page");
            this.SetupUrl("es-ES", "https://www.example.es/page");

            List<AlternateUrl> result = this.provider.GetAlternateUrls(
                defaultTab,
                new[] { frenchTab, spanishTab },
                this.portalSettings,
                DateTime.UtcNow);

            Assert.Multiple(() =>
            {
                Assert.That(result.Select(alternate => alternate.Language), Is.EqualTo(new[] { "fr-FR", "es-ES" }));
                Assert.That(result, Has.None.Matches<AlternateUrl>(alternate => alternate.Language == "en-US"));
            });
        }

        [Test]
        public void GetAliasByPortalIdAndSettings_WithCultureSpecificAlias_SelectsMatchingDomain()
        {
            PortalAliasInfo defaultAlias = CreatePortalAlias("www.example.com", "en-US", isPrimary: true);
            PortalAliasInfo frenchAlias = CreatePortalAlias("www.example.fr", "fr-FR", isPrimary: true);
            var urlAction = new UrlAction("http", string.Empty, string.Empty)
            {
                HttpAlias = "www.example.com",
                PortalId = 0,
            };

            PortalAliasInfo result = new[] { defaultAlias, frenchAlias }.GetAliasByPortalIdAndSettings(
                0,
                urlAction,
                "fr-FR",
                NewBrowserTypes.Normal);

            Assert.That(((IPortalAliasInfo)result).HttpAlias, Is.EqualTo("www.example.fr"));
        }

        private static TabInfo CreateTab(string cultureCode, bool allowIndex = true)
        {
            var tab = new TabInfo
            {
                CultureCode = cultureCode,
                IsVisible = true,
                HasBeenPublished = true,
                Url = string.Empty,
            };
            var settings = new Hashtable
            {
                ["AllowIndex"] = allowIndex.ToString(),
            };

            SetPrivateField(tab, "settings", settings);
            SetPrivateField(tab, "permissions", new TabPermissionCollection());
            return tab;
        }

        private static void SetPrivateField(TabInfo tab, string fieldName, object value)
        {
            FieldInfo field = typeof(TabInfo).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(tab, value);
        }

        private static PortalAliasInfo CreatePortalAlias(string httpAlias, string cultureCode, bool isPrimary)
        {
            var alias = new PortalAliasInfo
            {
                CultureCode = cultureCode,
                IsPrimary = isPrimary,
            };
            var aliasInfo = (IPortalAliasInfo)alias;
            aliasInfo.HttpAlias = httpAlias;
            aliasInfo.PortalId = 0;
            aliasInfo.BrowserType = NewBrowserTypes.Normal;
            return alias;
        }

        private void SetupUrl(string cultureCode, string url)
        {
            this.globals
                .Setup(global => global.NavigateURL(
                    It.IsAny<int>(),
                    It.IsAny<bool>(),
                    It.IsAny<PortalSettings>(),
                    string.Empty,
                    cultureCode,
                    It.IsAny<string[]>()))
                .Returns(url);
        }

        private class TestCoreSitemapProvider : CoreSitemapProvider
        {
            public override bool IsTabPublic(TabPermissionCollection objTabPermissions)
            {
                return true;
            }
        }
    }
}
