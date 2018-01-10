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

using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using DNN.Integration.Test.Framework;
using DNN.Integration.Test.Framework.Helpers;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Tests.Integration.Executers;
using DotNetNuke.Tests.Integration.Executers.Builders;
using DotNetNuke.Tests.Integration.Executers.Dto;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.PersonaBar.Pages
{
    [TestFixture]
    public class PagesManagementTests : IntegrationTestBase
    {
        #region Fields

        private readonly string _hostName;
        private readonly string _hostPass;

        private readonly int PortalId = 0;

        #endregion

        #region SetUp

        public PagesManagementTests()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            _hostName = ConfigurationManager.AppSettings["hostUsername"];
            _hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            UpdateSslSettings(true);

            UpdateContentLocalization(true);
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            UpdateSslSettings(false);

            UpdateContentLocalization(false);
        }

        #endregion

        #region Tests

        [Test]
        public void Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()
        {
            int tabId;
            var connector = CreateNewSecurePage(out tabId);

            //Try to request the GetLocalization API
            var response = connector.GetContent($"API/PersonaBar/Pages/GetTabLocalization?pageId={tabId}", null, true, false);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private void UpdateContentLocalization(bool enabled)
        {
            var connector = WebApiTestHelper.LoginHost();

            var postData = new
            {
                PortalId = PortalId,
                ContentLocalizationEnabled = false,
                SystemDefaultLanguage = "English (United States)",
                SystemDefaultLanguageIcon = "/images/Flags/en-US.gif",
                SiteDefaultLanguage = "en-US",
                LanguageDisplayMode = "NATIVE",
                EnableUrlLanguage = true,
                EnableBrowserLanguage = true,
                AllowUserUICulture = false,
                CultureCode = "en-US",
                AllowContentLocalization = enabled
            };

            connector.PostJson("API/PersonaBar/SiteSettings/UpdateLanguageSettings", postData);

            connector.PostJson(
                enabled
                    ? $"API/PersonaBar/Languages/EnableLocalizedContent?portalId={PortalId}&translatePages=false"
                    : $"API/PersonaBar/Languages/DisableLocalizedContent?portalId={PortalId}", new {});
        }

        private void UpdateSslSettings(bool sslEnabled)
        {
            var connector = WebApiTestHelper.LoginHost();

            var postData = new {SSLEnabled = sslEnabled, SSLEnforced = false, SSLURL = "", STDURL = "", SSLOffloadHeader = ""};
            connector.PostJson("API/PersonaBar/Security/UpdateSslSettings", postData);
        }

        #endregion

        #region Private Methods

        private IWebApiConnector CreateNewSecurePage(out int tabId)
        {
            var pagesExecuter = new PagesExecuter { Connector = WebApiTestHelper.LoginHost() };

            var pageSettingsBuilder = new PageSettingsBuilder();
            pageSettingsBuilder.WithPermission(new TabPermissionsBuilder().Build());
            pageSettingsBuilder.WithSecure(true);

            var pageDetail = pagesExecuter.SavePageDetails(pageSettingsBuilder.Build());

            Assert.NotNull(pageDetail.Page, "The system must create the page and return its details in the response");

            tabId = (int)pageDetail.Page.id;

            return pagesExecuter.Connector;
        }

        #endregion
    }
}
