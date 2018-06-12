using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using DotNetNuke.Security.Membership;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Web.Api.Auth;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
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
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };

            //Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");

            //Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            CollectionAssert.IsEmpty(response.Headers.WwwAuthenticate);
        }

        [Test]
        public void MissingXmlHttpRequestValueDoesntThrowNullException()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "");

            //Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        [Test]
        public void ResponseWithNullRequestReturnsUnauthorized()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = null };

            //Act
            var handler = new DigestAuthMessageHandler(true, false);
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            StringAssert.Contains("realm=\"DNNAPI\"", response.Headers.WwwAuthenticate.First().Parameter);
        }

        //todo unit test actual authentication code
        //very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        //need to write controllers with interfaces and servicelocator
    }
}