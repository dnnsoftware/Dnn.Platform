// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Security
{
    using System;
    using System.Net;
    using System.Net.Http;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using NUnit.Framework;

    [TestFixture]
    public class AuthCookieTests : IntegrationTestBase
    {
        private const string GetPortaslApi = "/API/PersonaBar/SiteSettings/GetPortals";

        [Test]
        public void Using_Logged_Out_Cookie_Should_Be_Unauthorized()
        {
            var session = WebApiTestHelper.LoginHost();
            Assert.IsTrue(session.IsLoggedIn);

            // clone the cookies as after closing they will be removed from the container
            var cookies = new CookieContainer();
            cookies.Add(session.SessionCookies.GetCookies(session.Domain));

            // make sure the request succeeds when the user is logged in
            // var result1 = session.GetContent(GetPortaslApi, null, false); -- use same method to validate
            var result1 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
            Assert.IsTrue(result1.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.OK, result1.StatusCode);

            session.Logout();
            Assert.IsFalse(session.IsLoggedIn);

            // make sure the request fails when using the same cookies before logging out
            var result2 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
            Assert.IsFalse(result2.IsSuccessStatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result2.StatusCode);
        }

        private static HttpClient CreateHttpClient(Uri domain, TimeSpan timeout, CookieContainer cookies)
        {
            var clientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = cookies,
            };

            var client = new HttpClient(clientHandler)
            {
                BaseAddress = domain,
                Timeout = timeout,
            };

            return client;
        }

        private HttpResponseMessage SendDirectGetRequest(Uri domain, string path, TimeSpan timeout, CookieContainer cookies)
        {
            var client = CreateHttpClient(domain, timeout, cookies);
            return client.GetAsync(path).Result;
        }
    }
}
