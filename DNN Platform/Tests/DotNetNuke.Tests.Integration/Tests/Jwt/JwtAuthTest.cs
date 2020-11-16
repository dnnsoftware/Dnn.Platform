// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Tests.Jwt
{
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Web;

    using DNN.Integration.Test.Framework;
    using DNN.Integration.Test.Framework.Helpers;
    using Newtonsoft.Json;
    using NUnit.Framework;

    [TestFixture]
    public class JwtAuthTest : IntegrationTestBase
    {
        public const string ExtendTokenQuery = "/API/JwtAuth/mobile/extendtoken";

        private const string LoginQuery = "/API/JwtAuth/mobile/login";
        private const string LogoutQuery = "/API/JwtAuth/mobile/logout";
        private const string TestGetQuery = "/API/JwtAuth/mobile/testget";
        private const string TestPostQuery = "/API/JwtAuth/mobile/testpost";
        private const string GetMonikerQuery = "/API/web/mobilehelper/monikers?moduleList=";
        private const string GetModuleDetailsQuery = "/API/web/mobilehelper/moduledetails?moduleList=";
        private static readonly Encoding TextEncoder = Encoding.UTF8;
        private readonly string _hostName;
        private readonly string _hostPass;
        private readonly HttpClient _httpClient;
#if DEBUG
        // for degugging and setting breakpoints
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(300);
#else
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
#endif

        public JwtAuthTest()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
            this._httpClient = new HttpClient { BaseAddress = siteUri, Timeout = this._timeout };
            this._hostName = ConfigurationManager.AppSettings["hostUsername"];
            this._hostPass = ConfigurationManager.AppSettings["hostPassword"];
        }

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            try
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TEAMCITY_VERSION")))
                {
                    DatabaseHelper.ExecuteNonQuery("TRUNCATE TABLE {objectQualifier}JsonWebTokens");
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        [Test]
        public void InvalidUserLoginShouldFail()
        {
            var credentials = new { u = this._hostName, p = this._hostPass + "." };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ValidUserLoginShouldPass()
        {
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
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
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

            this.SetAuthHeaderToken(token.AccessToken);
            var result1 = this._httpClient.GetAsync(TestGetQuery).Result;
            var content1 = result1.Content.ReadAsStringAsync().Result;
            LogText(@"content1 => " + content1);
            Assert.AreEqual(HttpStatusCode.OK, result1.StatusCode);

            this.LogoutUser(token.AccessToken);

            this.SetAuthHeaderToken(token.AccessToken);
            var result2 = this._httpClient.GetAsync(TestGetQuery).Result;
            var content2 = result2.Content.ReadAsStringAsync().Result;
            LogText(@"content2 => " + content2);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result2.StatusCode);
        }

        [Test]
        public void RequestWithoutTokenShouldFail()
        {
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ValidatedUserGetRequestShouldPass()
        {
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.Less(0, content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal));
        }

        [Test]
        public void ValidatedUserPostRequestShouldPass()
        {
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(TestPostQuery, new { text = "Integraton Testing Rocks!" }).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            Assert.Less(0, content.IndexOf("You are authenticated through JWT", StringComparison.Ordinal));
            Assert.Less(0, content.IndexOf("You said: (Integraton Testing Rocks!)", StringComparison.Ordinal));
        }

        [Test]
        public void RenewValidTokenShouldPass()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);
        }

        [Test]
        public void UsingOriginalAccessTokenAfterRenewalShouldFail()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void TryingToRenewUsingSameTokenMoreTHanOneTimeShouldFail()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.RenewAuthorizationToken(token1);

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void RenewMultipleTimesShouldPass()
        {
            this.RenewAuthorizationToken(
                this.RenewAuthorizationToken(
                    this.RenewAuthorizationToken(
                        this.GetAuthorizationTokenFor(this._hostName, this._hostPass))));
        }

        [Test]
        public void UsingTheNewAccessTokenAfterRenewalShouldPass()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var token2 = this.RenewAuthorizationToken(token1);
            Assert.AreNotEqual(token1.AccessToken, token2.AccessToken);
            Assert.AreEqual(token1.RenewalToken, token2.RenewalToken);
            Assert.AreEqual(token1.DisplayName, token2.DisplayName);

            this.SetAuthHeaderToken(token2.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void TamperedTokenShouldFail()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);

            // tampered header
            this.SetAuthHeaderToken("x" + token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);

            // tampered signature
            this.SetAuthHeaderToken(token1.AccessToken + "y");
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);

            // tampered claims
            var idx = token1.AccessToken.IndexOf('.');
            var tampered = token1.AccessToken.Substring(0, idx + 10) + "z" + token1.AccessToken.Substring(idx + 10);
            this.SetAuthHeaderToken(tampered);
            result = this._httpClient.GetAsync(TestGetQuery).Result;
            content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void ExtendingTokenWithinLastHourExtendsUpToRenewalExpiry()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            var token2 = this.RenewAuthorizationToken(token1);
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
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET TokenExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void UsingExpiredRenewalTokenShouldFail()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void TryingToRenewUsingAnExpiredRenewalTokenShouldFail()
        {
            var token1 = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            this.SetAuthHeaderToken(token1.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        [TestCase(GetMonikerQuery)]
        [TestCase(GetModuleDetailsQuery)]
        public void CallingHelperForLoggedinUserShouldReturnSuccess(string query)
        {
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
            var result = this._httpClient.GetAsync(query + HttpUtility.UrlEncode("ViewProfile")).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.NotNull(content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void ValidatingSuccessWhenUsingExistingMoniker()
        {
            // Arrange
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
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        [Test]
        public void ValidatingFailureWhenUsingNonExistingMoniker()
        {
            // Arrange
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
            var token = this.GetAuthorizationTokenFor(this._hostName, this._hostPass);
            this.SetAuthHeaderToken(token.AccessToken);
            this.SetMonikerHeader("myjournal");
            var postItem = new { ProfileId = 1, GroupId = -1, RowIndex = 0, MaxRows = 1 };
            var result = this._httpClient.PostAsJsonAsync(
                "/API/Journal/Services/GetListForProfile", postItem).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            LogText(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        private static string DecodeBase64(string b64Str)
        {
            // fix Base64 string padding
            var mod = b64Str.Length % 4;
            if (mod != 0)
            {
                b64Str += new string('=', 4 - mod);
            }

            return TextEncoder.GetString(Convert.FromBase64String(b64Str));
        }

        // template to help in copy/paste of new test methods
        /*
        [Test]
        public void TemplateMethod()
        {
            Assert.Fail();
        }
         */

        private LoginResultData GetAuthorizationTokenFor(string uname, string upass)
        {
            var credentials = new { u = uname, p = upass };
            var result = this._httpClient.PostAsJsonAsync(LoginQuery, credentials).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.IsNotNull(token);
            LogText(@"AuthToken => " + JsonConvert.SerializeObject(token));
            this._httpClient.DefaultRequestHeaders.Clear();
            return token;
        }

        private LoginResultData RenewAuthorizationToken(LoginResultData currentToken)
        {
            Thread.Sleep(1000); // must delay at least 1 second so the expiry time is different
            this.SetAuthHeaderToken(currentToken.AccessToken);
            var result = this._httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = currentToken.RenewalToken }).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var token = result.Content.ReadAsAsync<LoginResultData>().Result;
            Assert.IsNotNull(token);
            LogText(@"RenewedToken => " + JsonConvert.SerializeObject(token));
            return token;
        }

        private void LogoutUser(string accessToken)
        {
            this.SetAuthHeaderToken(accessToken);
            var result = this._httpClient.GetAsync(LogoutQuery).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var content = result.Content.ReadAsStringAsync().Result;
            dynamic response = !string.IsNullOrEmpty(content) ? JsonConvert.DeserializeObject(content) : null;
            Assert.IsTrue(Convert.ToBoolean(response?.success));
        }

        private void SetAuthHeaderToken(string token, string scheme = "Bearer")
        {
            this._httpClient.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(scheme.Trim() + " " + token.Trim());
        }

        private void SetMonikerHeader(string monikerValue)
        {
            this._httpClient.DefaultRequestHeaders.Add("X-DNN-MONIKER", monikerValue);
        }

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
    }
}
