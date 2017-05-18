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
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web;
using DotNetNuke.Tests.Integration.Framework;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Tests.Jwt
{
    [TestFixture]
    public class JwtAuthTest : IntegrationTestBase
    {
        #region private data

        private readonly string _hostName;
        private readonly string _hostPass;
        private readonly HttpClient _httpClient;

        private static readonly Encoding TextEncoder = Encoding.UTF8;
#if DEBUG
        // for degugging and setting breakpoints
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(300);
#else
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
#endif

        private const string LoginQuery = "/API/JwtAuth/mobile/login";
        private const string LogoutQuery = "/API/JwtAuth/mobile/logout";
        public const string ExtendTokenQuery = "/API/JwtAuth/mobile/extendtoken";
        private const string TestGetQuery = "/API/JwtAuth/mobile/testget";
        private const string TestPostQuery = "/API/JwtAuth/mobile/testpost";
        private const string GetMonikerQuery = "/API/web/mobilehelper/monikers?moduleList=";
        private const string GetModuleDetailsQuery = "/API/web/mobilehelper/moduledetails?moduleList=";

        public JwtAuthTest()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
            _httpClient = new HttpClient { BaseAddress = siteUri, Timeout = _timeout };
            _hostName = ConfigurationManager.AppSettings["hostUsername"];
            _hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion

        #region tests

        [Test]
        public void InvalidUserLoginShouldFail()
        {
            var credentials = new { u = _hostName, p = _hostPass + "." };
            var result = _httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ValidUserLoginShouldPass()
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            Assert.IsNotNull(token.UserId);
            Assert.IsNotNull(token.AccessToken);
            Assert.IsNotNull(token.DisplayName);
            Assert.IsNotNull(token.RenewalToken);

            var parts = token.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            long claimExpiry = claims.exp;
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
            Assert.Less(DateTime.UtcNow, expiryInToken);
            Assert.LessOrEqual(expiryInToken, DateTime.UtcNow.AddHours(1));
        }

        [Test]
        public void RequestUsingInvaldatedTokenAfterLogoutShouldFail()
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);

            SetAuthHeaderToken(token.AccessToken);
            var result1 = _httpClient.GetAsync(TestGetQuery).Result;
            var content1 = result1.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content1 => " + content1);
            Assert.AreEqual(HttpStatusCode.OK, result1.StatusCode);

            LogoutUser(token.AccessToken);

            SetAuthHeaderToken(token.AccessToken);
            var result2 = _httpClient.GetAsync(TestGetQuery).Result;
            var content2 = result2.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content2 => " + content2);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result2.StatusCode);
        }

        [Test]
        public void RequestWithoutTokenShouldFail()
        {
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ValidatedUserGetRequestShouldPass()
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.Less(0, content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal));
        }

        [Test]
        public void ValidatedUserPostRequestShouldPass()
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            var result = _httpClient.PostAsJsonAsync(TestPostQuery, new { text = "Integraton Testing Rocks!" }).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.Less(0, content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal));
            Assert.Less(0, content.IndexOf("You said: (Integraton Testing Rocks!)", StringComparison.Ordinal));
        }

        [Test]
        public void RenewValidTokenShouldPass()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var token2 = RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);
        }

        [Test]
        public void UsingOriginalAccessTokenAfterRenewalShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var token2 = RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            RenewAuthorizationToken(token1);

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void RenewMultipleTimesShouldPass()
        {
            RenewAuthorizationToken(
                RenewAuthorizationToken(
                    RenewAuthorizationToken(
                        GetAuthorizationTokenFor(_hostName, _hostPass))));
        }

        [Test]
        public void UsingTheNewAccessTokenAfterRenewalShouldPass()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var token2 = RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);

            SetAuthHeaderToken(token2.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void TamperedTokenShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);

            // tampered header
            SetAuthHeaderToken("x" + token1.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);

            // tampered signature
            SetAuthHeaderToken(token1.AccessToken + "y");
            result = _httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);

            // tampered claims
            var idx = token1.AccessToken.IndexOf('.');
            var tampered = token1.AccessToken.Substring(0, idx + 10) + "z" + token1.AccessToken.Substring(idx + 10);
            SetAuthHeaderToken(tampered);
            result = _httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            var token2 = RenewAuthorizationToken(token1);
            parts = token2.AccessToken.Split('.');
            decoded = DecodeBase64(parts[1]);
            claims = JsonConvert.DeserializeObject(decoded);
            long claimExpiry = claims.exp;
            var expiryInToken = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(claimExpiry);
            Assert.Less(DateTime.UtcNow, expiryInToken);
            Assert.LessOrEqual(expiryInToken, DateTime.UtcNow.AddMinutes(31)); // appears the library rounds the time

            var record = DatabaseHelper.GetRecordById("JsonWebTokens", "TokenId", sessionId);
            var accessExpiry = (DateTime)record["TokenExpiry"];
            var renewalExpiry = (DateTime)record["RenewalExpiry"];
            Assert.AreEqual(accessExpiry, renewalExpiry);
            Assert.Less(DateTime.UtcNow, renewalExpiry);
            Assert.LessOrEqual(renewalExpiry, DateTime.UtcNow.AddMinutes(31));
            Assert.AreEqual(accessExpiry, expiryInToken);
        }

        [Test]
        public void UsingExpiredAccessTokenShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET TokenExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void UsingExpiredRenewalTokenShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void TryingToRenewUsingAnExpiredRenewalTokenShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForLoggedinUserShouldReturnSuccess(string query)
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            var result = _httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.NotNull(content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void ValidatingSuccessWhenUsingExistingMoniker()
        {
            //Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
            Assert.Greater(tabModuleId, 0);

            // These will set a moniker for the Activity Feed module of the user profile
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}UpdateTabModuleSetting " + tabModuleId + @", 'Moniker', 'myjournal', 1");
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            SetMonikerHeader("myjournal");
            var postItem = new {ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1};
            var result = _httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void ValidatingFailureWhenUsingNonExistingMoniker()
        {
            //Arrange
            const string query1 =
                 @"SELECT TOP(1) TabModuleId FROM {objectQualifier}TabModules
	                WHERE TabId IN (SELECT TabId FROM {objectQualifier}Tabs WHERE TabName='Activity Feed')
	                  AND ModuleTitle='Journal';";
            var tabModuleId = DatabaseHelper.ExecuteScalar<int>(query1);
            Assert.Greater(tabModuleId, 0);

            // These will set a moniker for the Activity Feed module of the user profile
            DatabaseHelper.ExecuteNonQuery(@"EXEC {objectQualifier}DeleteTabModuleSetting " + tabModuleId + @", 'Moniker'");
            WebApiTestHelper.ClearHostCache();

            // Act
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            SetMonikerHeader("myjournal");
            var postItem = new {ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1};
            var result = _httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        // template to help in copy/paste of new test methods
        /*
        [Test]
        public void TemplateMethod()
        {
            Assert.Fail();
        }
         */

        #endregion

        #region helpers

        private static void ShowInfo(string info)
        {
            // Don't write anything to console when we run in TeamCity
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                Console.WriteLine(info);
        }

        private LoginResultData GetAuthorizationTokenFor(string uname, string upass)
        {
            var credentials = new { u = uname, p = upass };
            var result = _httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.IsNotNull(token);
            ShowInfo(@"AuthToken => " + JsonConvert.SerializeObject(token));
            _httpClient.DefaultRequestHeaders.Clear();
            return token;
        }

        private LoginResultData RenewAuthorizationToken(LoginResultData currentToken)
        {
            Thread.Sleep(1000); // must delay at least 1 second so the expiry time is different
            SetAuthHeaderToken(currentToken.AccessToken);
            var result = _httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = currentToken.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.IsNotNull(token);
            ShowInfo(@"RenewedToken => " + JsonConvert.SerializeObject(token));
            return token;
        }

        private void LogoutUser(string accessToken)
        {
            SetAuthHeaderToken(accessToken);
            var result = _httpClient.GetAsync(LogoutQuery).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private void SetAuthHeaderToken(string token, string scheme = "Bearer")
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(scheme.Trim() + " " + token.Trim());
        }

        private void SetMonikerHeader(string monikerValue)
        {
            _httpClient.DefaultRequestHeaders.Add("X-DNN-MONIKER", monikerValue);
        }

        private static string DecodeBase64(string b64Str)
        {
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0) b64Str += new string('=', 4 - mod);
            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        #endregion

        #region supporting classes

        [JsonObject]
        public class LoginResultData
        {
            [JsonProperty("userId")]
            public int UserId { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("accessToken")]
            public string AccessToken { get; set; }

            [JsonProperty("renewalToken")]
            public string RenewalToken { get; set; }
        }

        #endregion
    }
}