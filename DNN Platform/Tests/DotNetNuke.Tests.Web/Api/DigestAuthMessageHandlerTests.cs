using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using DotNetNuke.Security.Membership;
using DotNetNuke.Tests.Utilities.Mocks;
using DotNetNuke.Web.Api.Internal.Auth;
using Moq;
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
            var handler = new DigestAuthMessageHandler();
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            Assert.AreEqual("Digest", response.Headers.WwwAuthenticate.First().Scheme);
            Assert.IsTrue(response.Headers.WwwAuthenticate.First().Parameter.Contains("realm=\"DNNAPI\""));
        }

        [Test]
        public void OmitsWwwAuthenticateHeaderOn401FromXmlHttpRequest()
        {
            //Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) { RequestMessage = new HttpRequestMessage() };
            response.RequestMessage.Headers.Add("X-REQUESTED-WITH", "XmlHttpRequest");

            //Act
            var handler = new DigestAuthMessageHandler();
            handler.OnOutboundResponse(response, new CancellationToken());

            //Assert
            CollectionAssert.IsEmpty(response.Headers.WwwAuthenticate);
        }

        //todo unit test actual authentication code
        //very hard to unit test inbound authentication code as it dips into untestable bits of usercontroller etc.
        //need to write controllers with interfaces and servicelocator
    }
}