using DotNetNuke.Web.Api.Internal;
using Moq;
using NUnit.Framework;
using System;
using System.Web.Http.Dependencies;

namespace DotNetNuke.Tests.Web.Api.Internals
{
    [TestFixture]
    public class DnnDependencyResolverTests
    {
        private IDependencyResolver _dependencyResolver;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ITestService)))
                .Returns(new TestService());

            _dependencyResolver = new DnnDependencyResolver(mockServiceProvider.Object);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _dependencyResolver = null;
        }

        [Test]
        public void NotNull()
        {
            Assert.NotNull(_dependencyResolver);
        }

        [Test]
        public void IsOfTypeDnnDependencyResolver()
        {
            Assert.IsInstanceOf<DnnDependencyResolver>(_dependencyResolver);
        }

        [Test]
        public void GetTestService()
        {
            var expected = new TestService();
            var actual = _dependencyResolver.GetService(typeof(ITestService));

            Assert.NotNull(actual);
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        private interface ITestService { }
        private class TestService : ITestService { }

    }
}
