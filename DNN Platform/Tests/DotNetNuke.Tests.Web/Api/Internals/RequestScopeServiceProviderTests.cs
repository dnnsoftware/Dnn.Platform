using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System.Web.UI;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Web.Common;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    public class RequestScopeServiceProviderTests
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
            serviceCollection.AddSingleton<ITestService, TestService>();

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

            var provider = new RequestScopeServiceProvider();

            // Act
            var instance = provider.GetService(typeof(ITestService));

            // Assert
            Assert.NotNull(instance);
            Assert.AreEqual(scope.ServiceProvider.GetRequiredService<ITestService>(), instance);
        }

        [Test]
        public void CreateInstanceWithProtectedConstructor()
        {
            // Arrange
            var scope = Globals.DependencyProvider.CreateScope();

            HttpContextHelper.RegisterMockHttpContext();
            HttpContextSource.Current.SetScope(scope);

            var provider = new RequestScopeServiceProvider();

            // Act
            var instance = provider.GetService(typeof(PageHandlerFactory));

            // Assert
            Assert.NotNull(instance);
            Assert.IsInstanceOf<PageHandlerFactory>(instance);
        }

        private interface ITestService
        { }

        private class TestService : ITestService
        { }
    }
}
