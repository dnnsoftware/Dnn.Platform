// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System;
    using System.Linq;
    using System.Web.Http.Dependencies;

    using DotNetNuke.Services.DependencyInjection;
    using DotNetNuke.Web.Api.Internal;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class DnnDependencyResolverTests
    {
        private IServiceProvider serviceProvider;
        private IDependencyResolver dependencyResolver;

        private interface ITestService
        {
        }

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IScopeAccessor>(sp => new FakeScopeAccessor(sp.CreateScope()));
            services.AddScoped(typeof(ITestService), typeof(TestService));

            this.serviceProvider = services.BuildServiceProvider();
            this.dependencyResolver = new DnnDependencyResolver(this.serviceProvider);
        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            this.dependencyResolver = null;

            if (this.serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            this.serviceProvider = null;
        }

        [Test]
        public void NotNull()
        {
            Assert.NotNull(this.dependencyResolver);
        }

        [Test]
        public void IsOfTypeDnnDependencyResolver()
        {
            Assert.IsInstanceOf<DnnDependencyResolver>(this.dependencyResolver);
        }

        [Test]
        public void GetTestService()
        {
            var expected = new TestService();
            var actual = this.dependencyResolver.GetService(typeof(ITestService));

            Assert.NotNull(actual);
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        [Test]
        public void GetTestServices()
        {
            var expected = new TestService();
            var actual = this.dependencyResolver.GetServices(typeof(ITestService)).ToArray();

            Assert.NotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(expected.GetType(), actual[0].GetType());
        }

        [Test]
        public void BeginScope()
        {
            var actual = this.dependencyResolver.BeginScope();

            Assert.NotNull(actual);
            Assert.IsInstanceOf<DnnDependencyResolver>(actual);
        }

        [Test]
        public void BeginScope_GetService()
        {
            var scope = this.dependencyResolver.BeginScope();

            var expected = new TestService();
            var actual = scope.GetService(typeof(ITestService));

            Assert.NotNull(actual);
            Assert.AreEqual(expected.GetType(), actual.GetType());
        }

        [Test]
        public void BeginScope_GetServices()
        {
            var scope = this.dependencyResolver.BeginScope();

            var expected = new TestService();
            var actual = scope.GetServices(typeof(ITestService)).ToArray();

            Assert.NotNull(actual);
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(expected.GetType(), actual[0].GetType());
        }

        private class TestService : ITestService
        {
        }

        private class FakeScopeAccessor : IScopeAccessor
        {
            private IServiceScope fakeScope;

            public FakeScopeAccessor(IServiceScope fakeScope)
            {
                this.fakeScope = fakeScope;
            }

            public IServiceScope GetScope()
            {
                return this.fakeScope;
            }
        }
    }
}
