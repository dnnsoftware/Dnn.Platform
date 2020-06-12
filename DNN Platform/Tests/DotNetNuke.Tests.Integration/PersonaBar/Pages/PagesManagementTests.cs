// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            this.UpdateSslSettings(true);

            this.UpdateContentLocalization(true);
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();

            this.UpdateSslSettings(false);

            this.UpdateContentLocalization(false);
        }

        #endregion

        #region Tests

        [Test]
        public void Page_Marked_As_Secure_Should_Able_To_Management_In_Insecure_Channel()
        {
            int tabId;
            var connector = this.CreateNewSecurePage(out tabId);

            // Try to request the GetLocalization API
            var response = connector.GetContent($"API/PersonaBar/Pages/GetTabLocalization?pageId={tabId}", null, true, false);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private void UpdateContentLocalization(bool enabled)
        {
            var connector = WebApiTestHelper.LoginHost();

            var postData = new
            {
                PortalId = this.PortalId,
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
                    ? $"API/PersonaBar/Languages/EnableLocalizedContent?portalId={this.PortalId}&translatePages=false"
                    : $"API/PersonaBar/Languages/DisableLocalizedContent?portalId={this.PortalId}", new { });
        }

        private void UpdateSslSettings(bool sslEnabled)
        {
            var connector = WebApiTestHelper.LoginHost();

            var postData = new { SSLEnabled = sslEnabled, SSLEnforced = false, SSLURL = "", STDURL = "", SSLOffloadHeader = ""};
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
