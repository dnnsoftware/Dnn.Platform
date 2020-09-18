// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    using Dnn.AuthServices.Jwt.Auth;
    using Dnn.AuthServices.Jwt.Components.Entity;
    using Dnn.AuthServices.Jwt.Data;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.ConfigSection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class JwtAuthMessageHandlerTests
    {
        // { "type":"JWT","alg":"HS256"} . {"sub":"host","nbf":1,"exp":4102444799,"sid":"0123456789ABCDEF"} . (HS256_KEY="secret")
        private const string ValidToken =
            "eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ";

        private Mock<DataProvider> _mockDataProvider;
        private Mock<MembershipProvider> _mockMembership;
        private Mock<Entities.Portals.Data.IDataService> _mockDataService;
        private Mock<IDataService> _mockJwtDataService;
        private Mock<IUserController> _mockUserController;
        private Mock<IPortalController> _mockPortalController;

        public void SetupMockServices()
        {
            MockComponentProvider.CreateDataCacheProvider();

            this._mockDataService = new Mock<Entities.Portals.Data.IDataService>();
            this._mockDataProvider = MockComponentProvider.CreateDataProvider();
            this._mockPortalController = new Mock<IPortalController>();
            this._mockUserController = new Mock<IUserController>();
            this._mockMembership = MockComponentProvider.CreateNew<MembershipProvider>();

            this._mockDataProvider.Setup(d => d.GetProviderPath()).Returns(string.Empty);
            this._mockDataProvider.Setup(d => d.GetPortals(It.IsAny<string>())).Returns<string>(GetPortalsCallBack);
            this._mockDataProvider.Setup(d => d.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns(GetUser);
            this._mockDataService.Setup(ds => ds.GetPortalGroups()).Returns(this.GetPortalGroups);
            Entities.Portals.Data.DataService.RegisterInstance(this._mockDataService.Object);

            this._mockMembership.Setup(m => m.PasswordRetrievalEnabled).Returns(true);
            this._mockMembership.Setup(m => m.GetUser(It.IsAny<int>(), It.IsAny<int>())).Returns((int portalId, int userId) => GetUserByIdCallback(portalId, userId));

            this._mockJwtDataService = new Mock<IDataService>();
            this._mockJwtDataService.Setup(x => x.GetTokenById(It.IsAny<string>())).Returns((string sid) => GetPersistedToken(sid));
            DataService.RegisterInstance(this._mockJwtDataService.Object);

            PortalController.SetTestableInstance(this._mockPortalController.Object);
            this._mockPortalController.Setup(x => x.GetPortal(It.IsAny<int>())).Returns(
                new PortalInfo { PortalID = 0, PortalGroupID = -1, UserTabId = 55 });
            this._mockPortalController.Setup(x => x.GetPortalSettings(It.IsAny<int>())).Returns((int portalId) => new Dictionary<string, string>());
            this._mockPortalController.Setup(x => x.GetCurrentPortalSettings()).Returns(() => new PortalSettings());

            UserController.SetTestableInstance(this._mockUserController.Object);
            this._mockUserController.Setup(x => x.GetUserById(It.IsAny<int>(), It.IsAny<int>())).Returns(
                (int portalId, int userId) => GetUserByIdCallback(portalId, userId));

            // _mockUserController.Setup(x => x.ValidateUser(It.IsAny<UserInfo>(), It.IsAny<int>(), It.IsAny<bool>())).Returns(UserValidStatus.VALID);
        }

        [Test]
        public void ReturnsResponseAsReceived()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK) { RequestMessage = new HttpRequestMessage() };

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response2 = handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual(response, response2);
        }

        [Test]
        public void MissingAuthoizationHeaderReturnsNullResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void WrongAuthoizationSchemeReturnsNullResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Basic ");

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void MissingAuthoizationTokenReturnsNullResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer ");

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithMissingComponentsReturnsNullResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoiSldUIiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0");

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithWrongSchemeTypeReturnsNullResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(
                "Bearer eyJ0eXBlIjoieHh4IiwiYWxnIjoiSFMyNTYifQ.eyJzdWIiOiJ1c2VybmFtZSIsIm5iZiI6MSwiZXhwIjo0MTAyNDQ0Nzk5LCJzaWQiOiIwMTIzNDU2Nzg5QUJDREVGIn0.nfWCOVNk5M7L7EPDe3i3j4aAPRerbxgmcjOxaC-LWUQ");

            // Act
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
        }

        [Test]
        public void AuthoizationTokenWithoorResponse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/anyuri");
            request.Headers.Authorization = AuthenticationHeaderValue.Parse("Bearer " + ValidToken);

            // Act
            this.SetupMockServices();
            var handler = new JwtAuthMessageHandler(true, false);
            var response = handler.OnInboundRequest(request, CancellationToken.None);

            // Assert
            Assert.IsNull(response);
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

            table.Rows.Add(1, null, "host", "host", "host", "host", 1, "host@changeme.invalid", null, null, 0, null,
                           "127.0.0.1", 0, "8D3C800F-7A40-45D6-BA4D-E59A393F9800", DateTime.Now, null, -1, DateTime.Now,
                           -1, DateTime.Now);
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
                "UserTabId", "SearchTabId", "Custom404TabId", "Custom500TabId", "TermsTabId", "PrivacyTabId", "SuperTabId",
                "CreatedByUserID", "CreatedOnDate", "LastModifiedByUserID", "LastModifiedOnDate",
                "CultureCode",
            };

            foreach (var col in cols)
            {
                table.Columns.Add(col);
            }

            const int homePage = 1;
            table.Rows.Add(portalId, null, "My Website", "Logo.png", "Copyright (c) 2018 DNN Corp.", null,
                "2", "0", "2", "USD", "0", "0", "0", "0", "0", "1", "My Website",
                "DotNetNuke, DNN, Content, Management, CMS", null, "1057AC7A-3C08-4849-A3A6-3D2AB4662020",
                null, null, null, "0", "admin@changeme.invalid", "en-US", "-8", "58", "Portals/0", null,
                homePage.ToString(), null, null, "57", "56", "-1", "-1", null, null, "7", "-1", "2011-08-25 07:34:11",
                "-1", "2011-08-25 07:34:29", culture);

            return table.CreateDataReader();
        }

        private static PersistedToken GetPersistedToken(string sessionId)
        {
            if ("0123456789ABCDEF".Equals(sessionId))
            {
                return new PersistedToken
                {
                    TokenId = sessionId,
                    RenewalExpiry = DateTime.UtcNow.AddDays(14),
                    TokenExpiry = DateTime.UtcNow.AddHours(1),
                    UserId = 1,
                    TokenHash = GetHashedStr(ValidToken),
                    RenewalHash = "renewal-hash",
                };
            }

            return null;
        }

        private static UserInfo GetUserByIdCallback(int portalId, int userId)
        {
            if (portalId == 0)
            {
                switch (userId)
                {
                    case 1:
                        return new UserInfo { UserID = userId, Username = "host", DisplayName = "Host User" };
                    case 2:
                        return new UserInfo { UserID = userId, Username = "admin", DisplayName = "Admin User" };
                    case 3:
                        return new UserInfo { UserID = userId, Username = "reguser", DisplayName = "Registered User" };
                }
            }

            return null;
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of
        // UserController, etc. Need to write controllers with interfaces and ServiceLocator<>.

        // private static string DecodeBase64(string b64Str)
        // {
        //    // fix Base64 string padding
        //    var mod = b64Str.Length % 4;
        //    if (mod != 0) b64Str += new string('=', 4 - mod);
        //    return Encoding.UTF8.GetString(Convert.FromBase64String(b64Str));
        // }
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
    }
}
