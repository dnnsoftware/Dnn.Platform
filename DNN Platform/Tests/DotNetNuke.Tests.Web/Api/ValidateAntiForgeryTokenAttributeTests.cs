// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class ValidateAntiForgeryTokenAttributeTests
    {
        [TearDown]
        public void TearDown()
        {
            AntiForgery.ClearInstance();
        }

        [Test]
        [TestCase("{0}={1}; ")] // simple
        [TestCase("foo=fee; {0}={1}; key=value")] // normal
        [TestCase("bad name=value; {0}={1}; good_name=value; ")] // bad whitespace
        [TestCase("foo=fee; {0}={1}")] // no terminating semi-colon
        public void LocateCookies(string cookieFormat)
        {
            this.LocatesCookie(cookieFormat, "__RequestVerificationToken_thiscanbemany_characters_in_some_cases", "some text goes here");
        }

        [Test]

        // real world data from a request that fails when using the WebAPI .GetCookies method
        [TestCase("StayInEditMode=NO; ASP.NET_SessionId=yumytgeltkonngafni2h3i55; __RequestVerificationToken_LzYyMA__=dL23/UED9k98Xl4yO6jaXu6Oa29D6dLtjucfyukq7nV2ET6hBznoTS74x0yXuj8TS8qIoXcYa+6FkeE38qKQkR9KUq48oEeikLLdhIOZSkaDYsmgXQ7uKwNFK4k=; Panel-FriendlyUrl=false; dnnSitePanel-LoadFiles=true; dnnSitePanel-SEOSettings=true; dnnSitePanel-GeneralSettings=true; dnnSitePanel-EntryOptions=true; dnnSitePanel-AdvancedSettings=true; __RequestVerificationToken_L05lYnVsYQ__=C7z6azjbbWwBMjonTt/Np7Dm52/xvN3NS0G0mpCUy7cPiMB+1g6rt1ifKlaTDlRO8RsniY2DIAeMjet0nOPNwWJ6sByiWttFsPnbVlAV9qZ0YwFSsYge9dASbrQ=; SuggestionCustom=false; SuggestionTemplate=false; dnnTabs-dnnEditRole=0; dnnTabs-dnnBlogOptions=0; dnnSitePanel-RSSSettings=false; dnnSitePanel-CommentSettings=false; dnnTabs-dnnRecycleBin=0; dnnTabs-dnnManageUsers=0; Panel-Authentication Systems=false; .DOTNETNUKECTP270=63191FCE95736673FF53435768B2BB9838233821C4653EA0F3940432D05EF501878DE0A353C4B5474D52F5A28638BDDE1FB4269AB674C8283361C56A8CEC70243424414BB3871EB0918B840C6092CBD348A714BDD7D12A6B0441BC3BB96FC1DF963AA7A7; Panel-HostDetails=false; Panel-SMTP=false; .NEBULAX12=4C3B5C2B5C1DA13493A114CDE53CF487907FAF66AAAF04972CBF4E63E779580FE385D0759DDCA946775D0B0300A712052D4169DC93CC0B380D3970238B80E2850077573B22C9D1B8FB119867874667CA350057F392EE89E2205184FE53F3AB949578298E; __RequestVerificationToken_LzYyMw__=RtpaumwI56fNPCA7js90MRMECcEkR78oHrHn82YQOHXy3O3p53dEklX/485E+GMGGN/D/pzhN4UUI3eJs7be/1Tbkv8mwsMO9WAfFulwZazuktqwlmAyuI5BOj4=; dnnSitePanel-SiteDetails=false; dnnSitePanel-Appearance=true; dnnPanel-ViewLog=true; .ASPXANONYMOUS=N2HauUbbzQEkAAAAZWRmODRjODEtODBhOS00NGJhLTlhYTAtZDk5YTcxZTgzYzlm0; dnnSitePanel-Usability=true; dnnPanel-ExtensionPackageSettings=false; dnnSitePanel-Pages=true; dnnPanel-TabsOtherSettings=false; dnnPanel-TabsCacheSettings=false; dnnPanel-TabsAppearance=true; __RequestVerificationToken_L0NUUA2=rXptF4iP2vio-YQMbnxOp66oECpG2DQe6llUiWmiyjJvYjtWcx9XrjymOMwCheiGNogAbrv0RTPDGe2VGxswX0NVX02PZ-U2RwViRYhATUvcymdrc9jV3ygC0_w1; __RequestVerificationToken_LzcwMA2=LDfpU-HI96zNfH6FfLc45T9v8Ltr0OzwYWsaUVs-hGW1gVnLzYoVUm9xcQDkwjjrve_MbAQx6WuNq5Om2C8TEPRk_tXGDe7d3ibvXZVd0iUXYmSjoapYZ6FnjA41; undefined=false; dnn_ctr345_EditExtension_ModuleEditor_moduleSettingsHead=false; Panel-Widgets=false; Panel-Skin Objects=false; Panel-Skins=false; Panel-Providers=false; dnnPanel-LogSendExceptions=false; dnnPanel-ModuleAppearance=false; dnnPanel-ModuleOtherSettings=true; dnnTabs-dnnModuleSettings=3; dnnTabs-dnnHostSettings=0; Panel-Configuration=false; .DOTNETNUKE7PEBETA=197F580A056BEFB9E793BA8629133538298764AD055A68BABD9B93BD80F15A2B8734CD12D6CCA5B2BD8154319B932286A886074B57EAF06FA1E5E8DFA89FBF4192AA0E2F099554E5220DE12A632D5D58AA0272250CE67F93DD33CD3A4A411603252D0CB2; __RequestVerificationToken_LzdQRQ2=IKW7w2KnSu53WEJR0RKvhOcb2f_tyrxeSNuxOGoKmRIyvNFHnj4V_yZsE3J4cJILVeOP15sPmFbukzCpejZL7Ltmlx_XxDFRknrN-xJ4_5cPcppIGFCrl7o0akE1; .DOTNETNUKE=A43776EA5CEDF9E63B890A79E803F54FF72FD3718F9633C80C94C60EA43990EC24A7791B354E21A3CDDDF81F6AE0602A332770B6BA2AF48CED1973061A15F72CF78E8D9A1D33ED882B8E66E3CF9588325709E287FBA83CD11D58A84D03923716EC74B0FF; authentication=DNN; Panel-Libraries=false; dnnTabs-dnnExtensions=0; Panel-Modules=true; 51D=3155378975999999999; __RequestVerificationToken_L09yaW9u0=erx2uolpjtcgKYekq_Blw9bjJTsGB78wa-HREzGMXgpNM6fKGv1MvQ5mHj2KQLfBwyUzkHR3rC6XlTgf4NlcMhm2FyInWqaiPfU9w6lBj5EQeu-Wl3YXLuQrDgM1; .ORIONNEBULA7=1B487CE9A95CC1E925AEA02D9535E3E27F7536663EF7DD451996E3D7ADD474C410352960C16D7B38E19ECF2056927C2DEB23DABE85BB90EF0CC80226D00B6D90C21D45BA77F4D62C0C58FD5303672812C1A58783A50B58D34C245CC56694C176EA8F2541; __RequestVerificationToken_L1ZlaWw1=aAOphH6E9pSOu9yjGvTGhlaXHDL-Z5RQtPcb9-aAYSOJ674286gvW65ui7tm7iBr6COrjRN2UXeI6fQQyiL7Q8ctc59K5YQ5jM2K09l3fnag-3FZ1zz3W6d6iGE1; dnnTabs-dnnSiteSettings=1; dnnTabs-tabSettingsForm=0; .VeilNebula7=7DE1D60FF03633EF8611505BBB7201942C6C236AD8C289B22FE2BE48647D2626D55DF84ACA02094C3D253A1EBBABD48C0A008538E0C3AB4DA96ABC95EFA947DC2D74FF79D9478674D0BA35D3D2EAE94E2B5CDDC43915E11B90B361CEE799F3C421ECE499; language=en-US; LastPageId=0:64; dnnTabs-smMainContent=0", "__RequestVerificationToken_L05lYnVsYQ__", "C7z6azjbbWwBMjonTt/Np7Dm52/xvN3NS0G0mpCUy7cPiMB+1g6rt1ifKlaTDlRO8RsniY2DIAeMjet0nOPNwWJ6sByiWttFsPnbVlAV9qZ0YwFSsYge9dASbrQ=")]
        public void LocatesCookie(string cookieFormat, string cookieName, string cookieValue)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Add("Cookie", string.Format(cookieFormat, cookieName, cookieValue));
            request.Headers.Add("RequestVerificationToken", "anything_is_fine");
            var config = new HttpConfiguration();
            var controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            var actionContext = new HttpActionContext(controllerContext, new Mock<HttpActionDescriptor>().Object);
            var authFilterContext = new AuthFilterContext(actionContext, string.Empty);
            var mockAntiForgery = new Mock<IAntiForgery>();
            mockAntiForgery.Setup(x => x.CookieName).Returns(cookieName);
            AntiForgery.SetTestableInstance(mockAntiForgery.Object);

            // Act
            var vaft = new ValidateAntiForgeryTokenAttribute();

            // Assert.IsTrue(ValidateAntiForgeryTokenAttribute.IsAuthorized(authFilterContext));
            Assert.DoesNotThrow(() => { vaft.OnActionExecuting(authFilterContext.ActionContext); });

            // Assert
            mockAntiForgery.Verify(x => x.Validate(cookieValue, It.IsAny<string>()), Times.Once());
        }

        [Test]
        [TestCase("StayInEditMode=NO; ASP.NET_SessionId=yumytgeltkonngafni2h3i55; __RequestVerificationToken_LzYyMA__=dL23/UED9k98Xl4yO6jaXu6Oa29D6dLtjucfyukq7nV2ET6hBznoTS74x0yXuj8TS8qIoXcYa+6FkeE38qKQkR9KUq48oEeikLLdhIOZSkaDYsmgXQ7uKwNFK4k=; Panel-FriendlyUrl=false; dnnSitePanel-LoadFiles=true; dnnSitePanel-SEOSettings=true; dnnSitePanel-GeneralSettings=true; dnnSitePanel-EntryOptions=true; dnnSitePanel-AdvancedSettings=true; __RequestVerificationToken_L05lYnVsYQ__=C7z6azjbbWwBMjonTt/Np7Dm52/xvN3NS0G0mpCUy7cPiMB+1g6rt1ifKlaTDlRO8RsniY2DIAeMjet0nOPNwWJ6sByiWttFsPnbVlAV9qZ0YwFSsYge9dASbrQ=; SuggestionCustom=false; SuggestionTemplate=false; dnnTabs-dnnEditRole=0; dnnTabs-dnnBlogOptions=0; dnnSitePanel-RSSSettings=false; dnnSitePanel-CommentSettings=false; dnnTabs-dnnRecycleBin=0; dnnTabs-dnnManageUsers=0; Panel-Authentication Systems=false; .DOTNETNUKECTP270=63191FCE95736673FF53435768B2BB9838233821C4653EA0F3940432D05EF501878DE0A353C4B5474D52F5A28638BDDE1FB4269AB674C8283361C56A8CEC70243424414BB3871EB0918B840C6092CBD348A714BDD7D12A6B0441BC3BB96FC1DF963AA7A7; Panel-HostDetails=false; Panel-SMTP=false; .NEBULAX12=4C3B5C2B5C1DA13493A114CDE53CF487907FAF66AAAF04972CBF4E63E779580FE385D0759DDCA946775D0B0300A712052D4169DC93CC0B380D3970238B80E2850077573B22C9D1B8FB119867874667CA350057F392EE89E2205184FE53F3AB949578298E; __RequestVerificationToken_LzYyMw__=RtpaumwI56fNPCA7js90MRMECcEkR78oHrHn82YQOHXy3O3p53dEklX/485E+GMGGN/D/pzhN4UUI3eJs7be/1Tbkv8mwsMO9WAfFulwZazuktqwlmAyuI5BOj4=; dnnSitePanel-SiteDetails=false; dnnSitePanel-Appearance=true; dnnPanel-ViewLog=true; .ASPXANONYMOUS=N2HauUbbzQEkAAAAZWRmODRjODEtODBhOS00NGJhLTlhYTAtZDk5YTcxZTgzYzlm0; dnnSitePanel-Usability=true; dnnPanel-ExtensionPackageSettings=false; dnnSitePanel-Pages=true; dnnPanel-TabsOtherSettings=false; dnnPanel-TabsCacheSettings=false; dnnPanel-TabsAppearance=true; __RequestVerificationToken_L0NUUA2=rXptF4iP2vio-YQMbnxOp66oECpG2DQe6llUiWmiyjJvYjtWcx9XrjymOMwCheiGNogAbrv0RTPDGe2VGxswX0NVX02PZ-U2RwViRYhATUvcymdrc9jV3ygC0_w1; __RequestVerificationToken_LzcwMA2=LDfpU-HI96zNfH6FfLc45T9v8Ltr0OzwYWsaUVs-hGW1gVnLzYoVUm9xcQDkwjjrve_MbAQx6WuNq5Om2C8TEPRk_tXGDe7d3ibvXZVd0iUXYmSjoapYZ6FnjA41; undefined=false; dnn_ctr345_EditExtension_ModuleEditor_moduleSettingsHead=false; Panel-Widgets=false; Panel-Skin Objects=false; Panel-Skins=false; Panel-Providers=false; dnnPanel-LogSendExceptions=false; dnnPanel-ModuleAppearance=false; dnnPanel-ModuleOtherSettings=true; dnnTabs-dnnModuleSettings=3; dnnTabs-dnnHostSettings=0; Panel-Configuration=false; .DOTNETNUKE7PEBETA=197F580A056BEFB9E793BA8629133538298764AD055A68BABD9B93BD80F15A2B8734CD12D6CCA5B2BD8154319B932286A886074B57EAF06FA1E5E8DFA89FBF4192AA0E2F099554E5220DE12A632D5D58AA0272250CE67F93DD33CD3A4A411603252D0CB2; __RequestVerificationToken_LzdQRQ2=IKW7w2KnSu53WEJR0RKvhOcb2f_tyrxeSNuxOGoKmRIyvNFHnj4V_yZsE3J4cJILVeOP15sPmFbukzCpejZL7Ltmlx_XxDFRknrN-xJ4_5cPcppIGFCrl7o0akE1; .DOTNETNUKE=A43776EA5CEDF9E63B890A79E803F54FF72FD3718F9633C80C94C60EA43990EC24A7791B354E21A3CDDDF81F6AE0602A332770B6BA2AF48CED1973061A15F72CF78E8D9A1D33ED882B8E66E3CF9588325709E287FBA83CD11D58A84D03923716EC74B0FF; authentication=DNN; Panel-Libraries=false; dnnTabs-dnnExtensions=0; Panel-Modules=true; 51D=3155378975999999999; __RequestVerificationToken_L09yaW9u0=erx2uolpjtcgKYekq_Blw9bjJTsGB78wa-HREzGMXgpNM6fKGv1MvQ5mHj2KQLfBwyUzkHR3rC6XlTgf4NlcMhm2FyInWqaiPfU9w6lBj5EQeu-Wl3YXLuQrDgM1; .ORIONNEBULA7=1B487CE9A95CC1E925AEA02D9535E3E27F7536663EF7DD451996E3D7ADD474C410352960C16D7B38E19ECF2056927C2DEB23DABE85BB90EF0CC80226D00B6D90C21D45BA77F4D62C0C58FD5303672812C1A58783A50B58D34C245CC56694C176EA8F2541; __RequestVerificationToken_L1ZlaWw1=aAOphH6E9pSOu9yjGvTGhlaXHDL-Z5RQtPcb9-aAYSOJ674286gvW65ui7tm7iBr6COrjRN2UXeI6fQQyiL7Q8ctc59K5YQ5jM2K09l3fnag-3FZ1zz3W6d6iGE1; dnnTabs-dnnSiteSettings=1; dnnTabs-tabSettingsForm=0; .VeilNebula7=7DE1D60FF03633EF8611505BBB7201942C6C236AD8C289B22FE2BE48647D2626D55DF84ACA02094C3D253A1EBBABD48C0A008538E0C3AB4DA96ABC95EFA947DC2D74FF79D9478674D0BA35D3D2EAE94E2B5CDDC43915E11B90B361CEE799F3C421ECE499; language=en-US; LastPageId=0:64; dnnTabs-smMainContent=0", "__RequestVerificationToken_L05lYnVsYQ__", "C7z6azjbbWwBMjonTt/Np7Dm52/xvN3NS0G0mpCUy7cPiMB+1g6rt1ifKlaTDlRO8RsniY2DIAeMjet0nOPNwWJ6sByiWttFsPnbVlAV9qZ0YwFSsYge9dASbrQ=")]
        public void MissingTokenDoesnotPassValidationTest(string cookieFormat, string cookieName, string cookieValue)
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Add("Cookie", string.Format(cookieFormat, cookieName, cookieValue));
            var config = new HttpConfiguration();
            var controllerContext = new HttpControllerContext(config, new HttpRouteData(new HttpRoute()), request);
            var actionContext = new HttpActionContext(controllerContext, new Mock<HttpActionDescriptor>().Object);
            var authFilterContext = new AuthFilterContext(actionContext, string.Empty);
            var mockAntiForgery = new Mock<IAntiForgery>();
            mockAntiForgery.Setup(x => x.CookieName).Returns(cookieName);
            AntiForgery.SetTestableInstance(mockAntiForgery.Object);

            // Act, Assert
            var vaft = new ValidateAntiForgeryTokenAttribute();

            // Assert.IsFalse(ValidateAntiForgeryTokenAttribute.IsAuthorized(authFilterContext));
            Assert.Throws<UnauthorizedAccessException>(() => { vaft.OnActionExecuting(authFilterContext.ActionContext); });
        }
    }
}
