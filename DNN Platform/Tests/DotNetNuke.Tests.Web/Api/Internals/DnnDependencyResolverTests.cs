using DotNetNuke.Web.Api.Internal;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Linq;
using System.Web.Http.Dependencies;

namespace DotNetNuke.Tests.Web.Api.Internals
{
    [TestFixture]
    public class DnnDependencyResolverTests
    {
        private IServiceProvider _serviceProvider;
        private IDependencyResolver _dependencyResolver;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var services = new ServiceCollection();
            services.AddScoped(typeof(ITestService), typeof(TestService));

            _serviceProvider = services.BuildServiceProvider();
            _dependencyResolver = new DnnDependencyResolver(_serviceProvider);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            _dependencyResolver = null;

            if (_serviceProvider is IDisposable disposable)
                disposable.Dispose();

            _serviceProvider = null;
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

        [Test]
        public void GetTestServices()
        {
            var expected = new TestService();
            var actual = _dependencyResolver.GetServices(typeof(ITestService)).ToArray();

            Assert.NotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(expected.GetType(), actual[0].GetType());
        }

        [Test]
        public void BeginScope()
        {
            var actual = _dependencyResolver.BeginScope();

            Assert.NotNull(actual);
            Assert.IsInstanceOf<DnnDependencyResolver>(actual);
        }

        [Test]
        public void BeginScope_GetService()
        {
            var scope = _dependencyResolver.BeginScope();

            var expected = new TestService();
            var actual = scope.GetService(typeof(ITestService));

            Assert.NotNull(actual);
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        [Test]
        public void BeginScope_GetServices()
        {
            var scope = _dependencyResolver.BeginScope();

            var expected = new TestService();
            var actual = scope.GetServices(typeof(ITestService)).ToArray();

            Assert.NotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(expected.GetType(), actual[0].GetType());
        }

        private interface ITestService { }
        private class TestService : ITestService { }
    }
}
