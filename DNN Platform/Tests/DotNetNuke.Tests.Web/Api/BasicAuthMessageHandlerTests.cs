// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using DotNetNuke.Web.Api.Auth;
    using NUnit.Framework;

    [TestFixture]
    public class BasicAuthMessageHandlerTests
    {
        [Test]
        public void SetsWwwAuthenticateHeaderOn401()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Basic", response.Headers.WwwAuthenticate.First().Scheme);
            Assert.AreEqual("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
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
            var handler = new BasicAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Basic", response.Headers.WwwAuthenticate.First().Scheme);
            Assert.AreEqual("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void ResponseWithNullRequestReturnsUnauthorized()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = null };

            // Act
            var handler = new BasicAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, CancellationToken.None);

            // Assert
            Assert.AreEqual("Basic", response.Headers.WwwAuthenticate.First().Scheme);
            Assert.AreEqual("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        // todo unit test actual authentication code
        // very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        // need to write controllers with interfaces and servicelocator
    }
}
