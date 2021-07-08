// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Tests.Utilities;
    using DotNetNuke.Web.Common;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class WebFormsServiceProviderTests
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
            serviceCollection.AddScoped<IScopedService, ScopedService>();
            serviceCollection.AddSingleton<SingletonService>();

            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Globals.DependencyProvider = null;
        }

        [Test]
        public void CreateInstanceFromScope()
        {
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();

            // Act
            var instance = provider.GetService(typeof(IScopedService));

            // Assert
            Assert.NotNull(instance);
            Assert.AreEqual(scope.ServiceProvider.GetRequiredService<IScopedService>(), instance);
        }

        [Test]
        public void CreateInstanceWithProtectedConstructor()
        {
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();

            // Act
            var instance = provider.GetService(typeof(PageHandlerFactory));

            // Assert
            Assert.NotNull(instance);
            Assert.IsInstanceOf<PageHandlerFactory>(instance);
        }

        [Test]
        public void CreateModuleWithSingletonService()
        {
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
            var service = Globals.DependencyProvider.GetRequiredService<SingletonService>();

            // Act
            var module = provider.GetService<TestModule<SingletonService>>();

            // Assert
            Assert.NotNull(module);
            Assert.AreEqual(module.ConstructorService, module.DependencyService);
            Assert.AreEqual(module.ConstructorService, service);
        }

        [Test]
        public void CreateModuleWithScopedService()
        {
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new WebFormsServiceProvider();
            var serviceFromRequestScope = scope.ServiceProvider.GetRequiredService<IScopedService>();
            var serviceFromGlobalScope = Globals.DependencyProvider.GetRequiredService<IScopedService>();

            // Act
            var module = provider.GetService<TestModule<IScopedService>>();

            // Assert
            Assert.NotNull(module);
            Assert.AreEqual(module.ConstructorService, module.DependencyService);
            Assert.AreEqual(module.ConstructorService, serviceFromRequestScope);
            Assert.AreNotEqual(module.ConstructorService, serviceFromGlobalScope);
        }

        private interface IScopedService
        { }

        private class ScopedService : IScopedService
        { }

        private class SingletonService
        { }

        public class TestModule<T> : PortalModuleBase
        {
            public TestModule(T service)
            {
                ConstructorService = service;
                DependencyService = DependencyProvider.GetRequiredService<T>();
            }

            public T ConstructorService { get; }

            public T DependencyService { get; }
        }
    }
}
