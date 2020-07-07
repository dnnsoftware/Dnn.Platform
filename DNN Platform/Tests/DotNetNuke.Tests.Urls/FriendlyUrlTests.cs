// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Urls;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.HttpModules.UrlRewrite;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class FriendlyUrlTests : UrlTestBase
    {
        private const string _defaultPage = Globals.glbDefaultPage;
        private const string _aboutUsPageName = "About Us";
        private int _tabId;
        private string _redirectMode;
        private Locale _customLocale;
        private PortalAliasInfo _primaryAlias;

        public FriendlyUrlTests()
            : base(0)
        {
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            this.UpdateTabName(this._tabId, "About Us");
            CacheController.FlushPageIndexFromCache();
            this.GetDefaultAlias();
            this._redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", this.PortalId, "CANONICALURL");
            this._primaryAlias = null;
            this._customLocale = null;
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            var tab = TabController.Instance.GetTabByName(_aboutUsPageName, this.PortalId);
            if (tab == null)
            {
                this.CreateTab(_aboutUsPageName);
                tab = TabController.Instance.GetTabByName(_aboutUsPageName, this.PortalId);
            }

            this._tabId = tab.TabID;

            // Add Portal Aliases
            var aliasController = PortalAliasController.Instance;
            TestUtil.ReadStream(string.Format("{0}", "Aliases"), (line, header) =>
                        {
                            string[] fields = line.Split(',');
                            var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
                            if (alias == null)
                            {
                                alias = new PortalAliasInfo
                                {
                                    HTTPAlias = fields[0],
                                    PortalID = this.PortalId,
                                };
                                PortalAliasController.Instance.AddPortalAlias(alias);
                            }
                        });
            TestUtil.ReadStream(string.Format("{0}", "Users"), (line, header) =>
                        {
                            string[] fields = line.Split(',');

                            TestUtil.AddUser(this.PortalId, fields[0].Trim(), fields[1].Trim(), fields[2].Trim());
                        });
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            this.UpdateTabName(this._tabId, "About Us");

            if (this._customLocale != null)
            {
                Localization.RemoveLanguageFromPortals(this._customLocale.LanguageId, true);
                Localization.DeleteLanguage(this._customLocale, true);
            }

            if (this._primaryAlias != null)
            {
                PortalAliasController.Instance.DeletePortalAlias(this._primaryAlias);
            }

            this.SetDefaultAlias(this.DefaultAlias);
            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", this._redirectMode);

            foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(this.PortalId)))
            {
                TabController.Instance.DeleteTabUrl(tabUrl, this.PortalId, true);
            }
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            var aliasController = PortalAliasController.Instance;
            TestUtil.ReadStream(string.Format("{0}", "Aliases"), (line, header) =>
                        {
                            string[] fields = line.Split(',');
                            var alias = aliasController.GetPortalAlias(fields[0], this.PortalId);
                            PortalAliasController.Instance.DeletePortalAlias(alias);
                        });
            TestUtil.ReadStream(string.Format("{0}", "Users"), (line, header) =>
                        {
                            string[] fields = line.Split(',');

                            TestUtil.DeleteUser(this.PortalId, fields[0]);
                        });
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_BaseTestCases")]
        public void AdvancedUrlProvider_BaseFriendlyUrl(Dictionary<string, string> testFields)
        {
            this.ExecuteTest("Base", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ImprovedTestCases")]
        public void AdvancedUrlProvider_ImprovedFriendlyUrl(Dictionary<string, string> testFields)
        {
            this.ExecuteTest("Improved", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_SpaceEncodingTestCases")]
        public void AdvancedUrlProvider_SpaceEncoding(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "SpaceEncoding", this.PortalId);
            settings.ReplaceSpaceWith = " ";

            string spaceEncoding = testFields.GetValue("SpaceEncoding");

            if (!string.IsNullOrEmpty(spaceEncoding))
            {
                settings.SpaceEncodingValue = spaceEncoding;
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_PageExtensionTestCases")]
        public void AdvancedUrlProvider_PageExtension(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PageExtension", this.PortalId);

            string pageExtensionUsageType = testFields.GetValue("PageExtensionUsageType");
            string pageExtension = testFields.GetValue("PageExtension");

            if (!string.IsNullOrEmpty(pageExtension))
            {
                settings.PageExtension = pageExtension;
            }

            switch (pageExtensionUsageType)
            {
                case "AlwaysUse":
                    settings.PageExtensionUsageType = PageExtensionUsageType.AlwaysUse;
                    break;
                case "PageOnly":
                    settings.PageExtensionUsageType = PageExtensionUsageType.PageOnly;
                    break;
                case "Never":
                    settings.PageExtensionUsageType = PageExtensionUsageType.Never;
                    break;
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_PrimaryPortalAliasTestCases")]
        public void AdvancedUrlProvider_PrimaryPortalAlias(Dictionary<string, string> testFields)
        {
            string defaultAlias = testFields["DefaultAlias"];
            string redirectMode = testFields["RedirectMode"];

            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PrimaryPortalAlias", this.PortalId);

            string language = testFields["Language"].Trim();
            if (!string.IsNullOrEmpty(language))
            {
                this._customLocale = new Locale { Code = language, Fallback = "en-US" };
                this._customLocale.Text = CultureInfo.GetCultureInfo(this._customLocale.Code).NativeName;
                Localization.SaveLanguage(this._customLocale);
                Localization.AddLanguageToPortals(this._customLocale.LanguageId);

                // add new primary alias
                this._primaryAlias = new PortalAliasInfo
                {
                    PortalID = this.PortalId,
                    HTTPAlias = defaultAlias,
                    CultureCode = language,
                    IsPrimary = true,
                };
                this._primaryAlias.PortalAliasID = PortalAliasController.Instance.AddPortalAlias(this._primaryAlias);
            }
            else
            {
                this.SetDefaultAlias(defaultAlias);
            }

            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", redirectMode);

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_RegexTestCases")]
        public void AdvancedUrlProvider_Regex(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            string regexSetting = testFields["Setting"];
            string regexValue = testFields["Value"];
            if (!string.IsNullOrEmpty(regexValue))
            {
                switch (regexSetting)
                {
                    case "IgnoreRegex":
                        settings.IgnoreRegex = regexValue;
                        break;
                    case "DoNotRewriteRegex":
                        settings.DoNotRewriteRegex = regexValue;
                        break;
                    case "UseSiteUrlsRegex":
                        settings.UseSiteUrlsRegex = regexValue;
                        break;
                    case "DoNotRedirectRegex":
                        settings.DoNotRedirectRegex = regexValue;
                        break;
                    case "DoNotRedirectSecureRegex":
                        settings.DoNotRedirectSecureRegex = regexValue;
                        break;
                    case "ForceLowerCaseRegex":
                        settings.ForceLowerCaseRegex = regexValue;
                        break;
                    case "NoFriendlyUrlRegex":
                        settings.NoFriendlyUrlRegex = regexValue;
                        break;
                    case "DoNotIncludeInPathRegex":
                        settings.DoNotIncludeInPathRegex = regexValue;
                        break;
                    case "ValidExtensionlessUrlsRegex":
                        settings.ValidExtensionlessUrlsRegex = regexValue;
                        break;
                    case "RegexMatch":
                        settings.RegexMatch = regexValue;
                        break;
                }
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ReplaceCharsTestCases")]
        public void AdvancedUrlProvider_ReplaceChars(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            string testPageName = testFields.GetValue("TestPageName");
            TabInfo tab = null;
            if (!string.IsNullOrEmpty(testPageName))
            {
                var tabName = testFields["Page Name"];
                tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
                tab.TabName = testPageName;
                TabController.Instance.UpdateTab(tab);

                // Refetch tab from DB
                tab = TabController.Instance.GetTab(tab.TabID, tab.PortalID, false);
            }

            string autoAscii = testFields.GetValue("AutoAscii");

            if (!string.IsNullOrEmpty(autoAscii))
            {
                settings.AutoAsciiConvert = Convert.ToBoolean(autoAscii);
            }

            TestUtil.GetReplaceCharDictionary(testFields, settings.ReplaceCharacterDictionary);

            this.ExecuteTestForTab("Improved", tab, settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ReplaceSpaceTestCases")]
        public void AdvancedUrlProvider_ReplaceSpace(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            string replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
            if (!string.IsNullOrEmpty(replaceSpaceWith))
            {
                settings.ReplaceSpaceWith = replaceSpaceWith;
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_VanityUrlTestCases")]
        public void AdvancedUrlProvider_VanityUrl(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
            var userName = testFields.GetValue("UserName", string.Empty);
            var vanityUrlPrefix = testFields.GetValue("VanityUrlPrefix", string.Empty);
            if (!string.IsNullOrEmpty(vanityUrlPrefix))
            {
                settings.VanityUrlPrefix = vanityUrlPrefix;
            }

            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
                if (user != null)
                {
                    user.VanityUrl = vanityUrl;
                    UserController.UpdateUser(this.PortalId, user);
                }
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ForceLowerCaseTestCases")]
        public void AdvancedUrlProvider_ForceLowerCase(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            string forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");

            if (!string.IsNullOrEmpty(forceLowerCaseRegex))
            {
                settings.ForceLowerCaseRegex = forceLowerCaseRegex;
            }

            this.ExecuteTest("Improved", settings, testFields);
        }

        private void ExecuteTest(string test, Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], this.PortalId);

            this.SetDefaultAlias(testFields);

            this.ExecuteTest(test, settings, testFields);
        }

        private void ExecuteTest(string test, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var tabName = testFields["Page Name"];
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
            if (tab == null)
            {
                Assert.Fail($"TAB with name [{tabName}] is not found!");
            }

            this.ExecuteTestForTab(test, tab, settings, testFields);
        }

        private void ExecuteTestForTab(string test, TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var httpAlias = testFields["Alias"];
            var defaultAlias = testFields.GetValue("DefaultAlias", string.Empty);
            var tabName = testFields["Page Name"];
            var scheme = testFields["Scheme"];
            var parameters = testFields["Params"];
            var result = testFields["Expected Url"];
            var customPage = testFields.GetValue("Custom Page Name", _defaultPage);
            string vanityUrl = testFields.GetValue("VanityUrl", string.Empty);

            var httpAliasFull = scheme + httpAlias + "/";
            var expectedResult = result.Replace("{alias}", httpAlias)
                                        .Replace("{usealias}", defaultAlias)
                                        .Replace("{tabName}", tabName)
                                        .Replace("{tabId}", tab.TabID.ToString())
                                        .Replace("{vanityUrl}", vanityUrl)
                                        .Replace("{defaultPage}", _defaultPage);

            if (!string.IsNullOrEmpty(parameters) && !parameters.StartsWith("&"))
            {
                parameters = "&" + parameters;
            }

            var userName = testFields.GetValue("UserName", string.Empty);
            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
                if (user != null)
                {
                    expectedResult = expectedResult.Replace("{userId}", user.UserID.ToString());
                    parameters = parameters.Replace("{userId}", user.UserID.ToString());
                }
            }

            var baseUrl = httpAliasFull + "Default.aspx?TabId=" + tab.TabID + parameters;
            string testUrl;
            if (test == "Base")
            {
                testUrl = AdvancedFriendlyUrlProvider.BaseFriendlyUrl(
                    tab,
                    baseUrl,
                    customPage,
                    httpAlias,
                    settings);
            }
            else
            {
                testUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(
                    tab,
                    baseUrl,
                    customPage,
                    httpAlias,
                    true,
                    settings,
                    Guid.Empty);
            }

            Assert.IsTrue(expectedResult.Equals(testUrl, StringComparison.InvariantCultureIgnoreCase));
        }

        private void UpdateTabName(int tabId, string newName)
        {
            var tab = TabController.Instance.GetTab(tabId, this.PortalId, false);
            tab.TabName = newName;
            TabController.Instance.UpdateTab(tab);
        }
    }
}
