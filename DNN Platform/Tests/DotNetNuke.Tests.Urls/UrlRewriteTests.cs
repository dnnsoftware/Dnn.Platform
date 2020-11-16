// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Urls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Web;

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
    public class UrlRewriteTests : UrlTestBase
    {
        private const string _defaultPage = Globals.glbDefaultPage;
        private const string _testPage = "Test Page";
        private const string _aboutUsPageName = "About Us";
        private int _tabId;
        private string _redirectMode;
        private Locale _customLocale;
        private string _securePageName;
        private PortalAliasInfo _primaryAlias;
        private bool _sslEnforced;
        private bool _sslEnabled;

        public UrlRewriteTests()
            : base(0)
        {
        }

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            this.DeleteTab(_testPage);
            this.CreateTab(_testPage);
            this.UpdateTabName(this._tabId, "About Us");
            this.UpdateTabSkin(this._tabId, string.Empty);
            CacheController.FlushPageIndexFromCache();
            this.GetDefaultAlias();
            this._redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", this.PortalId, "CANONICALURL");
            this._sslEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", this.PortalId, false);
            this._sslEnabled = PortalController.GetPortalSettingAsBoolean("SSLEnabled", this.PortalId, false);
            this._primaryAlias = null;
            this._customLocale = null;
            DataCache.ClearCache();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            this.DeleteTab(_testPage);
            this.UpdateTabName(this._tabId, "About Us");
            this.UpdateTabSkin(this._tabId, "[G]Skins/Xcillion/Inner.ascx");

            if (!string.IsNullOrEmpty(this._securePageName))
            {
                var tab = TabController.Instance.GetTabByName(this._securePageName, this.PortalId);
                if (tab != null)
                {
                    tab.IsSecure = false;

                    this.UpdateTab(tab);
                }
            }

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
            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", this._redirectMode, true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", this._sslEnforced.ToString(), true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnabled", this._sslEnabled.ToString(), true, "en-us");

            foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(this.PortalId)))
            {
                TabController.Instance.DeleteTabUrl(tabUrl, this.PortalId, true);
            }
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
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_BasicTestCases")]
        public void AdvancedUrlRewriter_BasicTest(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_DeletedTabHandlingTestCases")]
        public void AdvancedUrlRewriter_DeletedTabHandling(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            var tab = TabController.Instance.GetTabByName(_testPage, this.PortalId);
            if (Convert.ToBoolean(testFields["HardDeleted"]))
            {
                this.DeleteTab(_testPage);
                CacheController.FlushPageIndexFromCache();
            }
            else
            {
                tab.IsDeleted = Convert.ToBoolean(testFields["SoftDeleted"]);
                tab.DisableLink = Convert.ToBoolean(testFields["Disabled"]);
                if (Convert.ToBoolean(testFields["Expired"]))
                {
                    tab.EndDate = DateTime.Now - TimeSpan.FromDays(1);
                }

                this.UpdateTab(tab);
                CacheController.FlushPageIndexFromCache();
            }

            string deletedTabHandling = testFields.GetValue("DeletedTabHandling");

            if (!string.IsNullOrEmpty(deletedTabHandling))
            {
                switch (deletedTabHandling)
                {
                    case "Do404Error":
                        settings.DeletedTabHandlingType = DeletedTabHandlingType.Do404Error;
                        break;
                    default:
                        settings.DeletedTabHandlingType = DeletedTabHandlingType.Do301RedirectToPortalHome;
                        break;
                }
            }

            this.SetDefaultAlias(testFields);

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_DoNotRedirect")]
        public void AdvancedUrlRewriter_DoNotRedirect(Dictionary<string, string> testFields)
        {
            var tabName = testFields["Page Name"];
            var doNotRedirect = testFields["DoNotRedirect"];

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            this.UpdateTabSetting(tabName, "DoNotRedirect", doNotRedirect);
            settings.UseBaseFriendlyUrls = testFields["UseBaseFriendlyUrls"];

            this.ExecuteTest(settings, testFields, true);

            this.UpdateTabSetting(tabName, "DoNotRedirect", "False");
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ForwardExternalUrlTestCases")]
        public void AdvancedUrlRewriter_ForwardExternalUrls(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            var tab = TabController.Instance.GetTabByName(_testPage, this.PortalId);
            tab.Url = testFields["ExternalUrl"];
            TabController.Instance.UpdateTab(tab);

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ForceLowerCaseTestCases")]
        public void AdvancedUrlRewriter_ForceLowerCase(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            string forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");

            if (!string.IsNullOrEmpty(forceLowerCaseRegex))
            {
                settings.ForceLowerCaseRegex = forceLowerCaseRegex;
            }

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_RegexTestCases")]
        public void AdvancedUrlRewriter_Regex(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

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

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ReplaceCharsTestCases")]
        public void AdvancedUrlRewriter_ReplaceChars(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

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

            this.SetDefaultAlias(testFields);

            this.ExecuteTestForTab(tab, settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ReplaceSpaceTestCases")]
        public void AdvancedUrlRewriter_ReplaceSpace(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            string replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
            if (!string.IsNullOrEmpty(replaceSpaceWith))
            {
                settings.ReplaceSpaceWith = replaceSpaceWith;
            }

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_SiteRootRedirectTestCases")]
        public void AdvancedUrlRewriter_SiteRootRedirect(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "SiteRootRedirect", this.PortalId);

            string scheme = testFields["Scheme"];

            this.ExecuteTest(settings, testFields, true);
            if (testFields["TestName"].Contains("Resubmit"))
            {
                var httpAlias = testFields["Alias"];
                settings.DoNotRedirectRegex = scheme + httpAlias;

                this.ExecuteTest(settings, testFields, true);
            }
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_PrimaryPortalAliasTestCases")]
        public void AdvancedUrlRewriter_0_PrimaryPortalAlias(Dictionary<string, string> testFields)
        {
            string defaultAlias = testFields["DefaultAlias"];

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);

            string language = testFields["Language"].Trim();
            string skin = testFields["Skin"].Trim();
            if (!string.IsNullOrEmpty(language))
            {
                this._customLocale = new Locale { Code = language, Fallback = "en-US" };
                this._customLocale.Text = CultureInfo.GetCultureInfo(this._customLocale.Code).NativeName;
                Localization.SaveLanguage(this._customLocale);
                Localization.AddLanguageToPortals(this._customLocale.LanguageId);
            }

            if (testFields.ContainsKey("Final Url"))
            {
                testFields["Final Url"] = testFields["Final Url"].Replace("{useAlias}", defaultAlias);
            }

            PortalController.UpdatePortalSetting(this.PortalId, "PortalAliasMapping", "REDIRECT", true, "en-us");
            var alias = PortalAliasController.Instance.GetPortalAlias(defaultAlias, this.PortalId);
            if (alias == null)
            {
                alias = new PortalAliasInfo
                {
                    HTTPAlias = defaultAlias,
                    PortalID = this.PortalId,
                    IsPrimary = true,
                };
                if (!(string.IsNullOrEmpty(language) && string.IsNullOrEmpty(skin)))
                {
                    alias.CultureCode = language;
                    alias.Skin = skin;
                }

                PortalAliasController.Instance.AddPortalAlias(alias);
            }

            this.SetDefaultAlias(defaultAlias);
            this.ExecuteTest(settings, testFields, false);

            alias = PortalAliasController.Instance.GetPortalAlias(defaultAlias, this.PortalId);
            if (alias != null)
            {
                PortalAliasController.Instance.DeletePortalAlias(alias);
            }
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_VanityUrlTestCases")]
        public void AdvancedUrlRewriter_VanityUrl(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
            settings.DeletedTabHandlingType = DeletedTabHandlingType.Do301RedirectToPortalHome;

            var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
            var userName = testFields.GetValue("UserName", string.Empty);
            var redirectOld = testFields.GetValue("RedirectOldProfileUrl", string.Empty);

            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
                if (user != null)
                {
                    user.VanityUrl = vanityUrl;
                    UserController.UpdateUser(this.PortalId, user);
                }
            }

            if (!string.IsNullOrEmpty(redirectOld))
            {
                settings.RedirectOldProfileUrl = Convert.ToBoolean(redirectOld);
            }

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_SecureRedirectTestCases")]
        public void AdvancedUrlRewriter_SecureRedirect(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], this.PortalId);
            var isClient = Convert.ToBoolean(testFields["Client"]);

            this._securePageName = testFields["Page Name"].Trim();

            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnforced", testFields["Enforced"].Trim(), true, "en-us");
            PortalController.UpdatePortalSetting(this.PortalId, "SSLEnabled", testFields["Enabled"].Trim(), true, "en-us");

            var isSecure = Convert.ToBoolean(testFields["IsSecure"].Trim());

            if (isSecure)
            {
                var tab = TabController.Instance.GetTabByName(this._securePageName, this.PortalId);
                tab.IsSecure = true;

                this.UpdateTab(tab);
            }

            settings.SSLClientRedirect = isClient;

            this.ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_JiraTests")]
        public void AdvancedUrlRewriter_JiraTests(Dictionary<string, string> testFields)
        {
            var testName = testFields.GetValue("Test File", string.Empty);

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "Jira_Tests", testName + ".csv", this.PortalId);
            var dictionary = UrlTestFactoryClass.GetDictionary("UrlRewrite", "Jira_Tests", testName + "_dic.csv");

            int homeTabId = -1;
            foreach (var keyValuePair in dictionary)
            {
                switch (keyValuePair.Key)
                {
                    case "HomeTabId":
                        homeTabId = this.UpdateHomeTab(int.Parse(keyValuePair.Value));
                        break;
                    default:
                        break;
                }
            }

            this.ExecuteTest(settings, testFields, true);

            if (homeTabId != -1)
            {
                this.UpdateHomeTab(homeTabId);
            }
        }

        private void CreateSimulatedRequest(Uri url)
        {
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", this.WebsitePhysicalAppPath);
            simulator.SimulateRequest(url);

            var browserCaps = new HttpBrowserCapabilities { Capabilities = new Hashtable() };
            HttpContext.Current.Request.Browser = browserCaps;
        }

        private void ProcessRequest(FriendlyUrlSettings settings, UrlTestHelper testHelper)
        {
            var provider = new AdvancedUrlRewriter();

            provider.ProcessTestRequestWithContext(
                HttpContext.Current,
                HttpContext.Current.Request.Url,
                true,
                testHelper.Result,
                settings);
            testHelper.Response = HttpContext.Current.Response;
        }

        private string ReplaceTokens(Dictionary<string, string> testFields, string url, string tabId)
        {
            var defaultAlias = testFields.GetValue("DefaultAlias", string.Empty);
            var httpAlias = testFields.GetValue("Alias", string.Empty);
            var tabName = testFields["Page Name"];
            var vanityUrl = testFields.GetValue("VanityUrl", string.Empty);
            var homeTabId = testFields.GetValue("HomeTabId", string.Empty);

            var userName = testFields.GetValue("UserName", string.Empty);
            string userId = string.Empty;
            if (!string.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(this.PortalId, userName);
                if (user != null)
                {
                    userId = user.UserID.ToString();
                }
            }

            return url.Replace("{alias}", httpAlias)
                            .Replace("{usealias}", defaultAlias)
                            .Replace("{tabName}", tabName)
                            .Replace("{tabId}", tabId)
                            .Replace("{portalId}", this.PortalId.ToString())
                            .Replace("{vanityUrl}", vanityUrl)
                            .Replace("{userId}", userId)
                            .Replace("{defaultPage}", _defaultPage);
        }

        private void DeleteTab(string tabName)
        {
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);

            if (tab != null)
            {
                TabController.Instance.DeleteTab(tab.TabID, this.PortalId);
            }
        }

        private void ExecuteTestForTab(TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var httpAlias = testFields.GetValue("Alias", string.Empty);
            var scheme = testFields["Scheme"];
            var url = testFields["Test Url"];
            var result = testFields["Expected Url"];
            var expectedStatus = int.Parse(testFields["Status"]);
            var redirectUrl = testFields.GetValue("Final Url");
            var redirectReason = testFields.GetValue("RedirectReason");

            var tabID = (tab == null) ? "-1" : tab.TabID.ToString();

            var expectedResult = this.ReplaceTokens(testFields, result, tabID);
            var testurl = this.ReplaceTokens(testFields, url, tabID);
            var expectedRedirectUrl = this.ReplaceTokens(testFields, redirectUrl, tabID);

            this.CreateSimulatedRequest(new Uri(testurl));

            var request = HttpContext.Current.Request;
            var testHelper = new UrlTestHelper
            {
                HttpAliasFull = scheme + httpAlias + "/",
                Result = new UrlAction(request)
                {
                    IsSecureConnection = request.IsSecureConnection,
                    RawUrl = request.RawUrl,
                },
                RequestUri = new Uri(testurl),
                QueryStringCol = new NameValueCollection(),
            };

            this.ProcessRequest(settings, testHelper);

            // Test expected response status
            Assert.AreEqual(expectedStatus, testHelper.Response.StatusCode);

            switch (expectedStatus)
            {
                case 200:
                    // Test expected rewrite path
                    if (!string.IsNullOrEmpty(expectedResult))
                    {
                        Assert.AreEqual(expectedResult, testHelper.Result.RewritePath.TrimStart('/'));
                    }

                    break;
                case 301:
                case 302:
                    // Test for final Url if redirected
                    Assert.IsTrue(expectedRedirectUrl.Equals(testHelper.Result.FinalUrl.TrimStart('/'), StringComparison.InvariantCultureIgnoreCase));
                    Assert.AreEqual(redirectReason, testHelper.Result.Reason.ToString(), "Redirect reason incorrect");
                    break;
            }
        }

        private void ExecuteTest(FriendlyUrlSettings settings, Dictionary<string, string> testFields, bool setDefaultAlias)
        {
            var tabName = testFields["Page Name"];
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);

            if (setDefaultAlias)
            {
                this.SetDefaultAlias(testFields);
            }

            this.ExecuteTestForTab(tab, settings, testFields);
        }

        private void UpdateTab(TabInfo tab)
        {
            if (tab != null)
            {
                TabController.Instance.UpdateTab(tab);
            }
        }

        private void UpdateTabName(int tabId, string newName)
        {
            var tab = TabController.Instance.GetTab(tabId, this.PortalId, false);
            tab.TabName = newName;
            TabController.Instance.UpdateTab(tab);
        }

        private void UpdateTabSkin(int tabId, string newSkin)
        {
            var tab = TabController.Instance.GetTab(tabId, this.PortalId, false);
            tab.SkinSrc = newSkin;
            TabController.Instance.UpdateTab(tab);
        }

        private int UpdateHomeTab(int homeTabId)
        {
            var portalInfo = PortalController.Instance.GetPortal(this.PortalId);
            int oldHomeTabId = portalInfo.HomeTabId;
            portalInfo.HomeTabId = homeTabId;

            return oldHomeTabId;
        }

        private void UpdateTabSetting(string tabName, string settingName, string settingValue)
        {
            var tab = TabController.Instance.GetTabByName(tabName, this.PortalId);
            tab.TabSettings[settingName] = settingValue;
            TabController.Instance.UpdateTab(tab);
        }
    }
}
