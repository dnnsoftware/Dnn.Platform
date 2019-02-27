#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Users;
using DotNetNuke.HttpModules.UrlRewrite;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;

namespace DotNetNuke.Tests.Urls
{
    [TestFixture]
    public class UrlRewriteTests : UrlTestBase 
    {
        private const string _defaultPage = Globals.glbDefaultPage;
        private int _tabId;
        private const string _testPage = "Test Page";
        private const string _aboutUsPageName = "About Us";
        private string _redirectMode;
        private Locale _customLocale;
        private string _securePageName;
        private PortalAliasInfo _primaryAlias;
        private bool _sslEnforced;
        private bool _sslEnabled;

        public UrlRewriteTests() : base(0) { }

        #region Private Methods

        private void CreateSimulatedRequest(Uri url)
        {
            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", WebsitePhysicalAppPath);
            simulator.SimulateRequest(url);

            var browserCaps = new HttpBrowserCapabilities { Capabilities = new Hashtable() };
            HttpContext.Current.Request.Browser = browserCaps;
        }

        private void ProcessRequest(FriendlyUrlSettings settings, UrlTestHelper testHelper)
        {
            var provider = new AdvancedUrlRewriter();

            provider.ProcessTestRequestWithContext(HttpContext.Current,
                                    HttpContext.Current.Request.Url,
                                    true,
                                    testHelper.Result,
                                    settings);
            testHelper.Response = HttpContext.Current.Response;
        }

        private string ReplaceTokens(Dictionary<string, string> testFields, string url, string tabId)
        {
            var defaultAlias = testFields.GetValue("DefaultAlias", String.Empty);
            var httpAlias = testFields.GetValue("Alias", String.Empty);
            var tabName = testFields["Page Name"];
            var vanityUrl = testFields.GetValue("VanityUrl", String.Empty);
            var homeTabId = testFields.GetValue("HomeTabId", String.Empty);

            var userName = testFields.GetValue("UserName", String.Empty);
            string userId = String.Empty;
            if (!String.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(PortalId, userName);
                if (user != null)
                {
                    userId = user.UserID.ToString();
                }
            }

            return url.Replace("{alias}", httpAlias)
                            .Replace("{usealias}", defaultAlias)
                            .Replace("{tabName}", tabName)
                            .Replace("{tabId}", tabId)
                            .Replace("{portalId}", PortalId.ToString())
                            .Replace("{vanityUrl}", vanityUrl)
                            .Replace("{userId}", userId)
                            .Replace("{defaultPage}", _defaultPage);
        }

        #endregion

        #region SetUp and TearDown

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            DeleteTab(_testPage);
            CreateTab(_testPage);
            UpdateTabName(_tabId, "About Us");
            UpdateTabSkin(_tabId,  "");
            CacheController.FlushPageIndexFromCache();
            GetDefaultAlias();
            _redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", PortalId, "CANONICALURL");
            _sslEnforced = PortalController.GetPortalSettingAsBoolean("SSLEnforced", PortalId, false);
            _sslEnabled = PortalController.GetPortalSettingAsBoolean("SSLEnabled", PortalId, false);
            _primaryAlias = null;
            _customLocale = null;
            DataCache.ClearCache();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            DeleteTab(_testPage);
            UpdateTabName(_tabId, "About Us");
            UpdateTabSkin(_tabId, "[G]Skins/Xcillion/Inner.ascx");

            if (!String.IsNullOrEmpty(_securePageName))
            {
                var tab = TabController.Instance.GetTabByName(_securePageName, PortalId);
                if (tab != null)
                {
                    tab.IsSecure = false;

                    UpdateTab(tab);
                }
            }

            if (_customLocale != null)
            {
                Localization.RemoveLanguageFromPortals(_customLocale.LanguageId, true);
                Localization.DeleteLanguage(_customLocale, true);
            }
            if (_primaryAlias != null)
            {
                PortalAliasController.Instance.DeletePortalAlias(_primaryAlias);
            }

            SetDefaultAlias(DefaultAlias);
            PortalController.UpdatePortalSetting(PortalId, "PortalAliasMapping", _redirectMode, true, "en-us");
            PortalController.UpdatePortalSetting(PortalId, "SSLEnforced", _sslEnforced.ToString(), true, "en-us");
            PortalController.UpdatePortalSetting(PortalId, "SSLEnabled", _sslEnabled.ToString(), true, "en-us");

            foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(PortalId)))
            {
                TabController.Instance.DeleteTabUrl(tabUrl, PortalId, true);
            }

        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            var tab = TabController.Instance.GetTabByName(_aboutUsPageName, PortalId);
            if (tab == null)
            {
                CreateTab(_aboutUsPageName);
                tab = TabController.Instance.GetTabByName(_aboutUsPageName, PortalId);
            }
            _tabId = tab.TabID;

            //Add Portal Aliases
            var aliasController = PortalAliasController.Instance;
            TestUtil.ReadStream(String.Format("{0}", "Aliases"), (line, header) =>
                            {
                                string[] fields = line.Split(',');
                                var alias = aliasController.GetPortalAlias(fields[0], PortalId);
                                if (alias == null)
                                {
                                    alias = new PortalAliasInfo
                                    {
                                        HTTPAlias = fields[0],
                                        PortalID = PortalId
                                    };
                                    PortalAliasController.Instance.AddPortalAlias(alias);
                                }
                            });
            TestUtil.ReadStream(String.Format("{0}", "Users"), (line, header) =>
                                {
                                    string[] fields = line.Split(',');

                                    TestUtil.AddUser(PortalId, fields[0].Trim(), fields[1].Trim(), fields[2].Trim());
                                });
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            var aliasController = PortalAliasController.Instance;
            TestUtil.ReadStream(String.Format("{0}", "Aliases"), (line, header) =>
                            {
                                string[] fields = line.Split(',');
                                var alias = aliasController.GetPortalAlias(fields[0], PortalId);
                                PortalAliasController.Instance.DeletePortalAlias(alias);
                            });
            TestUtil.ReadStream(String.Format("{0}", "Users"), (line, header) =>
                            {
                                string[] fields = line.Split(',');

                                TestUtil.DeleteUser(PortalId, fields[0]);
                            });

        }

        #endregion

        #region Private Methods

        private void DeleteTab(string tabName)
        {
            var tab = TabController.Instance.GetTabByName(tabName, PortalId);

            if (tab != null)
            {
                TabController.Instance.DeleteTab(tab.TabID, PortalId);
            }
        }

        private void ExecuteTestForTab(TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var httpAlias = testFields.GetValue("Alias", String.Empty);
            var scheme = testFields["Scheme"];
            var url = testFields["Test Url"];
            var result = testFields["Expected Url"];
            var expectedStatus = Int32.Parse(testFields["Status"]);
            var redirectUrl = testFields.GetValue("Final Url");
            var redirectReason = testFields.GetValue("RedirectReason");

            var tabID = (tab == null) ? "-1" : tab.TabID.ToString();

            var expectedResult = ReplaceTokens(testFields, result, tabID);
            var testurl = ReplaceTokens(testFields, url, tabID);
            var expectedRedirectUrl = ReplaceTokens(testFields, redirectUrl, tabID);

            CreateSimulatedRequest(new Uri(testurl));

            var request = HttpContext.Current.Request;
            var testHelper = new UrlTestHelper
                    {
                        HttpAliasFull = scheme + httpAlias + "/",
                        Result = new UrlAction(request)
                                        {
                                            IsSecureConnection = request.IsSecureConnection,
                                            RawUrl = request.RawUrl
                                        },
                        RequestUri = new Uri(testurl),
                        QueryStringCol = new NameValueCollection()
                    };

            ProcessRequest(settings, testHelper);

            //Test expected response status
            Assert.AreEqual(expectedStatus, testHelper.Response.StatusCode);

            switch (expectedStatus)
            {
                case 200:
                    //Test expected rewrite path
                    if (!String.IsNullOrEmpty(expectedResult))
                    {
                        Assert.AreEqual(expectedResult, testHelper.Result.RewritePath.TrimStart('/'));
                    }
                    break;
                case 301:
                case 302:
                    //Test for final Url if redirected
                    Assert.IsTrue(expectedRedirectUrl.Equals(testHelper.Result.FinalUrl.TrimStart('/'), StringComparison.InvariantCultureIgnoreCase));
                    Assert.AreEqual(redirectReason, testHelper.Result.Reason.ToString(), "Redirect reason incorrect");
                    break;
            }
        }

        private void ExecuteTest(FriendlyUrlSettings settings, Dictionary<string, string> testFields, bool setDefaultAlias)
        {
            var tabName = testFields["Page Name"];
            var tab = TabController.Instance.GetTabByName(tabName, PortalId);

            if (setDefaultAlias)
            {
                SetDefaultAlias(testFields);
            }

            ExecuteTestForTab(tab, settings, testFields);
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
            var tab = TabController.Instance.GetTab(tabId, PortalId, false);
            tab.TabName = newName;
            TabController.Instance.UpdateTab(tab);
        }

        private void UpdateTabSkin(int tabId, string newSkin)
        {
            var tab = TabController.Instance.GetTab(tabId, PortalId, false);
            tab.SkinSrc = newSkin;
            TabController.Instance.UpdateTab(tab);
        }

        #endregion

        #region Tests

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_BasicTestCases")]
        public void AdvancedUrlRewriter_BasicTest(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_DeletedTabHandlingTestCases")]
        public void AdvancedUrlRewriter_DeletedTabHandling(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            var tab = TabController.Instance.GetTabByName(_testPage, PortalId);
            if (Convert.ToBoolean(testFields["HardDeleted"]))
            {
                DeleteTab(_testPage);
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
                UpdateTab(tab);
                CacheController.FlushPageIndexFromCache();
            }

            string deletedTabHandling = testFields.GetValue("DeletedTabHandling");

            if (!String.IsNullOrEmpty(deletedTabHandling))
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

            SetDefaultAlias(testFields);

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_DoNotRedirect")]
        public void AdvancedUrlRewriter_DoNotRedirect(Dictionary<string, string> testFields)
        {
            var tabName = testFields["Page Name"];
            var doNotRedirect = testFields["DoNotRedirect"];

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            UpdateTabSetting(tabName, "DoNotRedirect", doNotRedirect);
            settings.UseBaseFriendlyUrls = testFields["UseBaseFriendlyUrls"];

            ExecuteTest(settings, testFields, true);

            UpdateTabSetting(tabName, "DoNotRedirect", "False");
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ForwardExternalUrlTestCases")]
        public void AdvancedUrlRewriter_ForwardExternalUrls(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            var tab = TabController.Instance.GetTabByName(_testPage, PortalId);
            tab.Url = testFields["ExternalUrl"];
            TabController.Instance.UpdateTab(tab);

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ForceLowerCaseTestCases")]
        public void AdvancedUrlRewriter_ForceLowerCase(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            string forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");

            if (!String.IsNullOrEmpty(forceLowerCaseRegex))
            {
                settings.ForceLowerCaseRegex = forceLowerCaseRegex;
            }

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_RegexTestCases")]
        public void AdvancedUrlRewriter_Regex(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            string regexSetting = testFields["Setting"];
            string regexValue = testFields["Value"];
            if (!String.IsNullOrEmpty(regexValue))
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

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ReplaceCharsTestCases")]
        public void AdvancedUrlRewriter_ReplaceChars(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            string testPageName = testFields.GetValue("TestPageName");
            TabInfo tab = null;
            if (!String.IsNullOrEmpty(testPageName))
            {
                var tabName = testFields["Page Name"];
                tab = TabController.Instance.GetTabByName(tabName, PortalId);
                tab.TabName = testPageName;
                TabController.Instance.UpdateTab(tab);

                //Refetch tab from DB
                tab = TabController.Instance.GetTab(tab.TabID, tab.PortalID, false);
            }

            string autoAscii = testFields.GetValue("AutoAscii");

            if (!String.IsNullOrEmpty(autoAscii))
            {
                settings.AutoAsciiConvert = Convert.ToBoolean(autoAscii);
            }

            TestUtil.GetReplaceCharDictionary(testFields, settings.ReplaceCharacterDictionary);

            SetDefaultAlias(testFields);

            ExecuteTestForTab(tab, settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_ReplaceSpaceTestCases")]
        public void AdvancedUrlRewriter_ReplaceSpace(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            string replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
            if (!String.IsNullOrEmpty(replaceSpaceWith))
            {
                settings.ReplaceSpaceWith = replaceSpaceWith;
            }

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_SiteRootRedirectTestCases")]
        public void AdvancedUrlRewriter_SiteRootRedirect(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "SiteRootRedirect", PortalId);

            string scheme = testFields["Scheme"];

            ExecuteTest(settings, testFields, true);
            if (testFields["TestName"].Contains("Resubmit"))
            {
                var httpAlias = testFields["Alias"];
                settings.DoNotRedirectRegex = scheme + httpAlias;

                ExecuteTest(settings, testFields, true);
            }
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_PrimaryPortalAliasTestCases")]
        public void AdvancedUrlRewriter_0_PrimaryPortalAlias(Dictionary<string, string> testFields)
        {
            string defaultAlias = testFields["DefaultAlias"];

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);

            string language = testFields["Language"].Trim();
            string skin = testFields["Skin"].Trim();
            if (!String.IsNullOrEmpty(language))
            {
                _customLocale = new Locale { Code = language, Fallback = "en-US" };
                _customLocale.Text = CultureInfo.GetCultureInfo(_customLocale.Code).NativeName;
                Localization.SaveLanguage(_customLocale);
                Localization.AddLanguageToPortals(_customLocale.LanguageId);

            }

            if (testFields.ContainsKey("Final Url"))
            {
                testFields["Final Url"] = testFields["Final Url"].Replace("{useAlias}", defaultAlias);
            }

            PortalController.UpdatePortalSetting(PortalId, "PortalAliasMapping", "REDIRECT", true, "en-us");
            var alias = PortalAliasController.Instance.GetPortalAlias(defaultAlias, PortalId);
            if (alias == null)
            {
                alias = new PortalAliasInfo
                {
                    HTTPAlias = defaultAlias,
                    PortalID = PortalId,
                    IsPrimary = true
                };
                if (!(String.IsNullOrEmpty(language) && String.IsNullOrEmpty(skin)))
                {
                    alias.CultureCode = language;
                    alias.Skin = skin;
                }
               PortalAliasController.Instance.AddPortalAlias(alias);
            }
            SetDefaultAlias(defaultAlias);
            ExecuteTest(settings, testFields, false);


            alias = PortalAliasController.Instance.GetPortalAlias(defaultAlias, PortalId);
            if (alias != null)
            {
                PortalAliasController.Instance.DeletePortalAlias(alias);
            }
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_VanityUrlTestCases")]
        public void AdvancedUrlRewriter_VanityUrl(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);
            settings.DeletedTabHandlingType = DeletedTabHandlingType.Do301RedirectToPortalHome;

            var vanityUrl = testFields.GetValue("VanityUrl", String.Empty);
            var userName = testFields.GetValue("UserName", String.Empty);
            var redirectOld = testFields.GetValue("RedirectOldProfileUrl", String.Empty);

            if (!String.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(PortalId, userName);
                if (user != null)
                {
                    user.VanityUrl = vanityUrl;
                    UserController.UpdateUser(PortalId, user);
                }
            }

            if (!String.IsNullOrEmpty(redirectOld))
            {
                settings.RedirectOldProfileUrl = Convert.ToBoolean(redirectOld);
            }
            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_SecureRedirectTestCases")]
        public void AdvancedUrlRewriter_SecureRedirect(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", testFields["TestName"], PortalId);
            var isClient = Convert.ToBoolean(testFields["Client"]);

            _securePageName = testFields["Page Name"].Trim();

            PortalController.UpdatePortalSetting(PortalId, "SSLEnforced", testFields["Enforced"].Trim(), true, "en-us");
            PortalController.UpdatePortalSetting(PortalId, "SSLEnabled", testFields["Enabled"].Trim(), true, "en-us");

            var isSecure = Convert.ToBoolean(testFields["IsSecure"].Trim());

            if (isSecure)
            {
                var tab = TabController.Instance.GetTabByName(_securePageName, PortalId);
                tab.IsSecure = true;

                UpdateTab(tab);
            }

            settings.SSLClientRedirect = isClient;

            ExecuteTest(settings, testFields, true);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "UrlRewrite_JiraTests")]
        public void AdvancedUrlRewriter_JiraTests(Dictionary<string, string> testFields)
        {
            var testName = testFields.GetValue("Test File", String.Empty);

            var settings = UrlTestFactoryClass.GetSettings("UrlRewrite", "Jira_Tests", testName + ".csv", PortalId);
            var dictionary = UrlTestFactoryClass.GetDictionary("UrlRewrite", "Jira_Tests", testName + "_dic.csv");

            int homeTabId = -1;
            foreach (var keyValuePair in dictionary)
            {
                switch (keyValuePair.Key)
                {
                    case "HomeTabId":
                        homeTabId = UpdateHomeTab(Int32.Parse(keyValuePair.Value));
                        break;
                    default:
                        break;
                }
            }

            ExecuteTest(settings, testFields, true);

            if (homeTabId != -1)
            {
                UpdateHomeTab(homeTabId);
            }

        }

        private int UpdateHomeTab(int homeTabId)
        {
            var portalInfo = PortalController.Instance.GetPortal(PortalId);
            int oldHomeTabId = portalInfo.HomeTabId;
            portalInfo.HomeTabId = homeTabId;

            return oldHomeTabId;
        }

        private void UpdateTabSetting(string tabName, string settingName, string settingValue)
        {
            var tab = TabController.Instance.GetTabByName(tabName, PortalId);
            tab.TabSettings[settingName] = settingValue;
            TabController.Instance.UpdateTab(tab);
        }

        #endregion
    }
}
