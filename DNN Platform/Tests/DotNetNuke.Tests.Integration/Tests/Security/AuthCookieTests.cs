// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Security;

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
        Assert.That(session.IsLoggedIn, Is.True);

        // clone the cookies as after closing they will be removed from the container
        var cookies = new CookieContainer();
        cookies.Add(session.SessionCookies.GetCookies(session.Domain));

        // make sure the request succeeds when the user is logged in
        // var result1 = session.GetContent(GetPortaslApi, null, false); -- use same method to validate
        var result1 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
        Assert.Multiple(() =>
        {
            Assert.That(result1.IsSuccessStatusCode, Is.True);
            Assert.That(result1.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });

        session.Logout();
        Assert.That(session.IsLoggedIn, Is.False);

        // make sure the request fails when using the same cookies before logging out
        var result2 = this.SendDirectGetRequest(session.Domain, GetPortaslApi, session.Timeout, cookies);
        Assert.Multiple(() =>
        {
            Assert.That(result2.IsSuccessStatusCode, Is.False);
            Assert.That(result2.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
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
