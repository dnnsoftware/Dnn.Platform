using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using DotNetNuke.Tests.Integration.Framework;
using DotNetNuke.Tests.Integration.Framework.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DotNetNuke.Tests.Integration.Tests
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
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(180); 
#else
        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);
#endif

        private const string LoginQuery = "/DesktopModules/JwtAuth/API/mobile/login";
        public const string ExtendTokenQuery = "/DesktopModules/JwtAuth/API/mobile/extendtoken";
        private const string TestGetQuery = "/DesktopModules/JwtAuth/API/mobile/testget";
        private const string TestPostQuery = "/DesktopModules/JwtAuth/API/mobile/testpost";

        public JwtAuthTest()
        {
            var url = ConfigurationManager.AppSettings["siteUrl"];
            var siteUri = new Uri(url);
            _httpClient = new HttpClient { BaseAddress = siteUri, Timeout = _timeout };
            _hostName = ConfigurationManager.AppSettings["hostUsername"];
            _hostPass = ConfigurationManager.AppSettings["hostPassword"];
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
        public void RequestWithoutTokenShouldFail()
        {
            var result2 = _httpClient.GetAsync(TestGetQuery).Result;
            var content2 = result2.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content2 => " + content2);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result2.StatusCode);
        }

        [Test]
        public void ValidatedUserGetRequestShouldPass()
        {
            var token = GetAuthorizationTokenFor(_hostName, _hostPass);
            SetAuthHeaderToken(token.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content2 => " + content);
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
        public void UsingRenewedAccessTokenAfterRenewalShouldPass()
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
            Assert.LessOrEqual(expiryInToken, DateTime.UtcNow.AddMinutes(30));

            var record = DatabaseHelper.GetRecordById("JsonWebTokens", "TokenId", sessionId);
            var accessExpiry = (DateTime)record["TokenExpiry"];
            var renewalExpiry = (DateTime)record["RenewalExpiry"];
            Assert.AreEqual(accessExpiry, renewalExpiry);
            Assert.Less(DateTime.UtcNow, renewalExpiry);
            Assert.LessOrEqual(renewalExpiry, DateTime.UtcNow.AddMinutes(30));
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
            var query = "UPDATE {objectQualifier}JsonWebTokens SET TokenExpiry="+
                $"'{DateTime.UtcNow.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
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
                $"'{DateTime.UtcNow.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.GetAsync(TestGetQuery).Result;
            var content = result.Content.ReadAsStringAsync().Result;
            ShowInfo(@"content => " + content);
            Assert.AreEqual(HttpStatusCode.Unauthorized, result.StatusCode);
        }

        [Test]
        public void TryingToRenewWhenExpiredRenewalTokenShouldFail()
        {
            var token1 = GetAuthorizationTokenFor(_hostName, _hostPass);
            var parts = token1.AccessToken.Split('.');
            var decoded = DecodeBase64(parts[1]);
            dynamic claims = JsonConvert.DeserializeObject(decoded);
            string sessionId = claims.sid;
            var query = "UPDATE {objectQualifier}JsonWebTokens SET RenewalExpiry=" +
                $"'{DateTime.UtcNow.AddSeconds(-1).ToString("yyyy-MM-dd HH:mm:ss")}' WHERE TokenId='{sessionId}';";
            DatabaseHelper.ExecuteNonQuery(query);
            WebApiTestHelper.ClearHostCache();

            SetAuthHeaderToken(token1.AccessToken);
            var result = _httpClient.PostAsJsonAsync(ExtendTokenQuery, new { rtoken = token1.RenewalToken }).Result;
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
            // Don't write anything to cosole when we run in TeamCity
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

        private void SetAuthHeaderToken(string token, string scheme = "Bearer")
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                AuthenticationHeaderValue.Parse(scheme.Trim() + " " + token.Trim());
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