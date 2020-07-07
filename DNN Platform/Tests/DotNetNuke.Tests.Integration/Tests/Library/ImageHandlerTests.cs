// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Library
{
    using System.Linq;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using DotNetNuke.Entities.Portals;
    using NUnit.Framework;

    [TestFixture]
    public class ImageHandlerTests : IntegrationTestBase
    {
        // we assume this GIF image exists in all DNN sites and it shouldn't be PNG
        private const string HandlerPath = "/DnnImageHandler.ashx?mode=file&w=1&url={0}/images/1x1.GIF";

        [Test]
        public void Using_Image_Handler_For_Foreign_Site_ShouldFail()
        {
            var session = WebApiTestHelper.GetAnnonymousConnector();
            var relativeUrl = string.Format(HandlerPath, "https://google.com");

            var response = session.GetContent(relativeUrl).Content.ReadAsStringAsync().Result;
            Assert.IsTrue(response.StartsWith("�PNG\r\n"), $"Content = {response}");
        }

        [Test]
        public void Using_Image_Handler_For_Main_Alias_ShouldPass()
        {
            var session = WebApiTestHelper.GetAnnonymousConnector();
            var relativeUrl = string.Format(HandlerPath, AppConfigHelper.SiteUrl);

            var response = session.GetContent(relativeUrl).Content.ReadAsStringAsync().Result;
            Assert.IsTrue(response.StartsWith("GIF89a"), $"Content = {response}");
        }

        [Test]
        public void Using_Image_Handler_From_All_Alias_ShouldPass()
        {
            PortalAliasInfo pai;
            var aliases = PortalAliasHelper.GetPortalAliaes().ToList();
            if (aliases.Count == 1)
            {
                var primary = aliases.First();
                pai = new PortalAliasInfo
                {
                    PortalID = primary.PortalID,
                    HTTPAlias = "my-" + primary.HTTPAlias,
                    CultureCode = primary.CultureCode,
                    Skin = primary.Skin,
                    BrowserType = primary.BrowserType,
                    IsPrimary = false,
                };
            }
            else
            {
                pai = aliases.First(a => a.PortalAliasID > 1);
            }

            var session = WebApiTestHelper.GetAnnonymousConnector();
            aliases = PortalAliasHelper.GetPortalAliaes().ToList();
            foreach (var alias in aliases)
            {
                var relativeUrl = string.Format(HandlerPath, AppConfigHelper.SiteUrl);
                var absoluteUrl = $"http://{alias.HTTPAlias}{relativeUrl}";
                LogText("Getting image from " + absoluteUrl);

                var response = session.GetContent(absoluteUrl).Content.ReadAsStringAsync().Result;
                Assert.IsTrue(response.StartsWith("GIF89a"), $"Url: {absoluteUrl} / Content = {response}");
            }
        }
    }
}
