#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Generic;
using System.Globalization;

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
    public class FriendlyUrlTests : UrlTestBase 
    {
        private const string _defaultPage = Globals.glbDefaultPage;
        private int _tabId;
        private const string _aboutUsPageName = "About Us";
        private string _redirectMode;
        private Locale _customLocale;
        private PortalAliasInfo _primaryAlias;

        public FriendlyUrlTests() : base(0) { }

        #region SetUp and TearDown

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            UpdateTabName(_tabId, "About Us");
            CacheController.FlushPageIndexFromCache();
            GetDefaultAlias();
            _redirectMode = PortalController.GetPortalSetting("PortalAliasMapping", PortalId, "CANONICALURL");
            _primaryAlias = null;
            _customLocale = null;
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

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();

            UpdateTabName(_tabId, "About Us");

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
            PortalController.UpdatePortalSetting(PortalId, "PortalAliasMapping", _redirectMode);

            foreach (var tabUrl in CBO.FillCollection<TabUrlInfo>(DataProvider.Instance().GetTabUrls(PortalId)))
            {
                TabController.Instance.DeleteTabUrl(tabUrl, PortalId, true);
            }
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            var aliasController =PortalAliasController.Instance;
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

        private void ExecuteTest(string test, Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

            SetDefaultAlias(testFields);

            ExecuteTest(test, settings, testFields);
        }

        private void ExecuteTest(string test, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var tabName = testFields["Page Name"];
            var tab = TabController.Instance.GetTabByName(tabName, PortalId);
            if (tab == null)
                Assert.Fail($"TAB with name [{tabName}] is not found!");

            ExecuteTestForTab(test, tab, settings, testFields);            
        }

        private void ExecuteTestForTab(string test, TabInfo tab, FriendlyUrlSettings settings, Dictionary<string, string> testFields)
        {
            var httpAlias = testFields["Alias"];
            var defaultAlias = testFields.GetValue("DefaultAlias", String.Empty);
            var tabName = testFields["Page Name"];
            var scheme = testFields["Scheme"];
            var parameters = testFields["Params"];
            var result = testFields["Expected Url"];
            var customPage = testFields.GetValue("Custom Page Name", _defaultPage);
            string vanityUrl = testFields.GetValue("VanityUrl", String.Empty);

            var httpAliasFull = scheme + httpAlias + "/";
            var expectedResult = result.Replace("{alias}", httpAlias)
                                        .Replace("{usealias}", defaultAlias)
                                        .Replace("{tabName}", tabName)
                                        .Replace("{tabId}", tab.TabID.ToString())
                                        .Replace("{vanityUrl}", vanityUrl)
                                        .Replace("{defaultPage}", _defaultPage);


            if (!String.IsNullOrEmpty(parameters) && !parameters.StartsWith("&"))
            {
                parameters = "&" + parameters;
            }

            var userName = testFields.GetValue("UserName", String.Empty);
            if (!String.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(PortalId, userName);
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
                testUrl = AdvancedFriendlyUrlProvider.BaseFriendlyUrl(tab,
                                                                        baseUrl,
                                                                        customPage,
                                                                        httpAlias,
                                                                        settings);
            }
            else
            {
                testUrl = AdvancedFriendlyUrlProvider.ImprovedFriendlyUrl(tab,
                                                                        baseUrl,
                                                                        customPage,
                                                                        httpAlias,
                                                                        true,
                                                                        settings,
                                                                        Guid.Empty);
            }

            Assert.AreEqual(expectedResult, testUrl);
        }

        private void UpdateTabName(int tabId, string newName)
        {
            var tab = TabController.Instance.GetTab(tabId, PortalId, false);
            tab.TabName = newName;
            TabController.Instance.UpdateTab(tab);
        }

        #endregion

        #region Tests

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_BaseTestCases")]
        public void AdvancedUrlProvider_BaseFriendlyUrl(Dictionary<string, string> testFields)
        {
            ExecuteTest("Base", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ImprovedTestCases")]
        public void AdvancedUrlProvider_ImprovedFriendlyUrl(Dictionary<string, string> testFields)
        {
            ExecuteTest("Improved", testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_SpaceEncodingTestCases")]
        public void AdvancedUrlProvider_SpaceEncoding(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "SpaceEncoding", PortalId);
            settings.ReplaceSpaceWith = " ";

            string spaceEncoding = testFields.GetValue("SpaceEncoding");

            if (!String.IsNullOrEmpty(spaceEncoding))
            {
                settings.SpaceEncodingValue = spaceEncoding;
            }

            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_PageExtensionTestCases")]
        public void AdvancedUrlProvider_PageExtension(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PageExtension", PortalId);

            string pageExtensionUsageType = testFields.GetValue("PageExtensionUsageType");
            string pageExtension = testFields.GetValue("PageExtension");

            if (!String.IsNullOrEmpty(pageExtension))
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

            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_PrimaryPortalAliasTestCases")]
        public void AdvancedUrlProvider_PrimaryPortalAlias(Dictionary<string, string> testFields)
        {
            string defaultAlias = testFields["DefaultAlias"];
            string redirectMode = testFields["RedirectMode"];

            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", "PrimaryPortalAlias", PortalId);

            string language = testFields["Language"].Trim();
            if (!String.IsNullOrEmpty(language))
            {
                _customLocale = new Locale { Code = language, Fallback = "en-US" };
                _customLocale.Text = CultureInfo.GetCultureInfo(_customLocale.Code).NativeName;
                Localization.SaveLanguage(_customLocale);
                Localization.AddLanguageToPortals(_customLocale.LanguageId);

                //add new primary alias
                _primaryAlias = new PortalAliasInfo
                {
                    PortalID = PortalId,
                    HTTPAlias = defaultAlias,
                    CultureCode = language,
                    IsPrimary = true
                };
                _primaryAlias.PortalAliasID = PortalAliasController.Instance.AddPortalAlias(_primaryAlias);
            }
            else
            {
                SetDefaultAlias(defaultAlias);
            }

            PortalController.UpdatePortalSetting(PortalId, "PortalAliasMapping", redirectMode);

            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_RegexTestCases")]
        public void AdvancedUrlProvider_Regex(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

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

            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ReplaceCharsTestCases")]
        public void AdvancedUrlProvider_ReplaceChars(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

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

            ExecuteTestForTab("Improved", tab, settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ReplaceSpaceTestCases")]
        public void AdvancedUrlProvider_ReplaceSpace(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

            string replaceSpaceWith = testFields.GetValue("ReplaceSpaceWith");
            if (!String.IsNullOrEmpty(replaceSpaceWith))
            {
                settings.ReplaceSpaceWith = replaceSpaceWith;
            }

            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_VanityUrlTestCases")]
        public void AdvancedUrlProvider_VanityUrl(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

            var vanityUrl = testFields.GetValue("VanityUrl", String.Empty);
            var userName = testFields.GetValue("UserName", String.Empty);
            var vanityUrlPrefix = testFields.GetValue("VanityUrlPrefix", String.Empty);
            if (!String.IsNullOrEmpty(vanityUrlPrefix))
            {
                settings.VanityUrlPrefix = vanityUrlPrefix;
            }

            if (!String.IsNullOrEmpty(userName))
            {
                var user = UserController.GetUserByName(PortalId, userName);
                if (user != null)
                {
                    user.VanityUrl = vanityUrl;
                    UserController.UpdateUser(PortalId, user);
                }
            }
            ExecuteTest("Improved", settings, testFields);
        }

        [Test]
        [TestCaseSource(typeof(UrlTestFactoryClass), "FriendlyUrl_ForceLowerCaseTestCases")]
        public void AdvancedUrlProvider_ForceLowerCase(Dictionary<string, string> testFields)
        {
            var settings = UrlTestFactoryClass.GetSettings("FriendlyUrl", testFields["TestName"], PortalId);

            string forceLowerCaseRegex = testFields.GetValue("ForceLowerCaseRegex");

            if (!String.IsNullOrEmpty(forceLowerCaseRegex))
            {
                settings.ForceLowerCaseRegex = forceLowerCaseRegex;
            }

            ExecuteTest("Improved", settings, testFields);
        }

        #endregion
    }
}
