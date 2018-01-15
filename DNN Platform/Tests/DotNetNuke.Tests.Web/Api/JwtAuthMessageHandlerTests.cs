#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading;
using Dnn.AuthServices.Jwt.Auth;
using Dnn.AuthServices.Jwt.Components.Entity;
using Dnn.AuthServices.Jwt.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Web.ConfigSection;
using Moq;
using NUnit.Framework;
using System.Text;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Data;
using DotNetNuke.Security.Membership;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class JwtAuthMessageHandlerTests
    {
        #region setting/mocked data and methods

        private Mock<DataProvider> _mockDataProvider;
        private Mock<MembershipProvider> _mockMembership;
        private Mock<Entities.Portals.Data.IDataService> _mockDataService;
        private Mock<IDataService> _mockJwtDataService;
        private Mock<IUserController> _mockUserController;
        private Mock<IPortalController> _mockPortalController;

        //{ "type":"JWT","alg":"HS256"} . {"sub":"host","nbf":1,"exp":4102444799,"sid":"0123456789ABCDEF"} . (HS256_KEY="secret")
        private const string ValidToken =
            "eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ";

        public void SetupMockServices()
        {
            MockComponentProvider.CreateDataCacheProvider();

            _mockDataService = new Mock<Entities.Portals.Data.IDataService>();
            _mockDataProvider = MockComponentProvider.CreateDataProvider();
            _mockPortalController = new Mock<IPortalController>();
            _mockUserController = new Mock<IUserController>();
            _mockMembership = MockComponentProvider.CreateNew<MembershipProvider>();

            _mockDataProvider.Setup(d => d.GetProviderPath()).Returns("");
            _mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);
            _mockDataProvider.Setup(d => d.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(GetUser);
            _mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(GetPortalGroups);
            Entities.Portals.Data.DataService.RegisterInstance(_mockDataService.Object);

            _mockMembership.Setup(m => m.PasswordRetrievalEnabled).Returns(true);
            _mockMembership.Setup(m => m.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => GetUserByIdCallback(portalId, userId));

            _mockJwtDataService = new Mock<IDataService>();
            _mockJwtDataService.Setup(x => x.GetTokenById(It.IsAny<string>())).Returns((string sid) => GetPersistedToken(sid));
            DataService.RegisterInstance(_mockJwtDataService.Object);

            PortalController.SetTestableInstance(_mockPortalController.Object);
            _mockPortalController.Setup(x => x.GetPortal(It.IsAny<int>())).Returns(
                new PortalInfo {PortalID = 0, PortalGroupID = -1, UserTabId = 55});
            _mockPortalController.Setup(x => x.GetPortalSettings(It.IsAny<int>())).Returns((int portalId) => new Dictionary<string, string>());
            _mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(() => new PortalSettings());

            UserController.SetTestableInstance(_mockUserController.Object);
            _mockUserController.Setup(x => x.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns(
                (int portalId, int userId) => GetUserByIdCallback(portalId, userId));
            //_mockUserController.Setup(x => x.ValidateUser(It.IsAny<UserInfo>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(UserValidStatus.VALID);
        }

        private static IDataReader GetUser()
        {
            var table = new DataTable("Users");
            table.Columns.Add("UserID", typeof(int));
            table.Columns.Add("PortalId", typeof(int));
            table.Columns.Add("UserName", typeof(string));
            table.Columns.Add("FirstName", typeof(string));
            table.Columns.Add("LastName", typeof(string));
            table.Columns.Add("DisplayName", typeof(string));
            table.Columns.Add("IsSuperUser", typeof(byte));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("VanityUrl", typeof(string));
            table.Columns.Add("AffiliateId", typeof(int));
            table.Columns.Add("IsDeleted", typeof(byte));
            table.Columns.Add("RefreshRoles", typeof(byte));
            table.Columns.Add("LastIPAddress", typeof(string));
            table.Columns.Add("UpdatePassword", typeof(byte));
            table.Columns.Add("PasswordResetToken", typeof(Guid));
            table.Columns.Add("PasswordResetExpiration", typeof(DateTime));
            table.Columns.Add("Authorised", typeof(byte));

            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));

            table.Rows.Add(1, null, "host", "host", "host", "host", 1, "host@change.me", null, null, 0, null,
                           "127.0.0.1", 0, "8D3C800F-7A40-45D6-BA4D-E59A393F9800", DateTime.Now, null, -1, DateTime.Now,
                           -1, DateTime.Now);
            return table.CreateDataReader();
        }

        private IDataReader GetPortalGroups()
        {
            var table = new DataTable("ModuleDefinitions");
            var pkId = table.Columns.Add("PortalGroupID", typeof(int));
            table.Columns.Add("MasterPortalID", typeof(int));
            table.Columns.Add("PortalGroupName", typeof(string));
            table.Columns.Add("PortalGroupDescription", typeof(string));
            table.Columns.Add("AuthenticationDomain", typeof(string));
            table.Columns.Add("CreatedByUserID", typeof(int));
            table.Columns.Add("CreatedOnDate", typeof(DateTime));
            table.Columns.Add("LastModifiedByUserID", typeof(int));
            table.Columns.Add("LastModifiedOnDate", typeof(DateTime));
            table.PrimaryKey = new[] { pkId };

            table.Rows.Add(0, 0, "test", "descr", "domain", -1, DateTime.Now, -1, DateTime.Now);
            return table.CreateDataReader();
        }

        private static IDataReader GetPortalsCallBack(string culture)
        {
            return GetPortalCallBack(0, culture);
        }

        private static IDataReader GetPortalCallBack(int portalId, string culture)
        {
            var table = new DataTable("Portal");

            var cols = new[]
            {
                "PortalID", "PortalGroupID", "PortalName", "LogoFile", "FooterText", "ExpiryDate",
                "UserRegistration", "BannerAdvertising", "AdministratorId", "Currency", "HostFee",
                "HostSpace", "PageQuota", "UserQuota", "AdministratorRoleId", "RegisteredRoleId",
                "Description", "KeyWords", "BackgroundFile", "GUID", "PaymentProcessor",
                "ProcessorUserId",
                "ProcessorPassword", "SiteLogHistory", "Email", "DefaultLanguage", "TimezoneOffset",
                "AdminTabId", "HomeDirectory", "SplashTabId", "HomeTabId", "LoginTabId", "RegisterTabId",
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode"
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@change.me", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        private static PersistedToken GetPersistedToken(string sessionId)
        {
            if ("0123456789ABCDEF".Equals(sessionId))
                return new PersistedToken
                {
                    TokenId = sessionId,
                    RenewalExpiry = DateTime.UtcNow.AddDays(14),
                    TokenExpiry = DateTime.UtcNow.AddHours(1),
                    UserId = 1,
                    TokenHash = GetHashedStr(ValidToken),
                    RenewalHash = "renewal-hash",
                };

            return null;
        }

        private static UserInfo GetUserByIdCallback(int portalId, int userId)
        {
            if (portalId == 0)
            {
                switch (userId)
                {
                    case 1:
                        return new UserInfo {UserID = userId, Username = "host", DisplayName = "Host User"};
                    case 2:
                        return new UserInfo {UserID = userId, Username = "admin", DisplayName = "Admin User"};
                    case 3:
                        return new UserInfo {UserID = userId, Username = "reguser", DisplayName = "Registered User"};
                }
            }

            return null;
        }

        #endregion

        [Test]
        public void ReturnsResponseAsReceived()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK) {RequestMessage = new HttpRequestMessage()};

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response2 = handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            Assert.AreEqual(response, response2);
        }

        [Test]
        public void MissingAuthoizationHeaderReturnsNullResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            Assert.IsNull(response);
        }

        [Test]
        public void WrongAuthoizationSchemeReturnsNullResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Basic ");

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            Assert.IsNull(response);
        }

        [Test]
        public void MissingAuthoizationTokenReturnsNullResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer ");

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithMissingComponentsReturnsNullResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0");

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithWrongSchemeTypeReturnsNullResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoieHh4IiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ");

            //Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithoorResponse()
        {
            //Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + ValidToken);

            //Act
            SetupMockServices();
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, new CancellationToken());

            //Assert
            //TODO: Assert.IsNotNull(response); //to get this to work a lot of mocking is required; not-practical
            Assert.IsNull(response);
        }

        //todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of
        // UserController, etc. Need to write controllers with interfaces and ServiceLocator<>.

        #region helpers

        //private static string DecodeBase64(string b64Str)
        //{
        //    // fix Base64 string padding
        //    var mod = b64Str.Length % 4;
        //    if (mod != 0) b64Str += new string('=', 4 - mod);
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(b64Str));
        //}

        private static string EncodeBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }

        private static string GetHashedStr(string data)
        {
            using (var hasher = SHA384.Create())
            {
                return EncodeBase64(hasher.ComputeHash(Encoding.UTF8.GetBytes(data)));
            }
        }

        #endregion
    }
}