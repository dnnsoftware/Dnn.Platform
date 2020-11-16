// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.Security.Membership;
    using DotNetNuke.Tests.Utilities.Mocks;
    using DotNetNuke.Web.Api.Auth;
    using NUnit.Framework;

    [TestFixture]
    public class DigestAuthMessageHandlerTests
    {
        [SetUp]
        public void Setup()
        {
            var mockMembership = MockComponentProvider.CreateNew<MembershipProvider>();
            mockMembership.Setup(m => m.PasswordRetrievalEnabled).Returns(true);
        }

        [Test]
        public void SetsWwwAuthenticateHeaderOn401()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            CollectionAssert.IsEmpty(response.Headers.WwwAuthenticate);
        }

        [Test]
        public void MissingXmlHttpRequestValueDoesntThrowNullException()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", string.Empty);

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void ResponseWithNullRequestReturnsUnauthorized()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = null };

            // Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        // need to write controllers with interfaces and servicelocator
    }
}
