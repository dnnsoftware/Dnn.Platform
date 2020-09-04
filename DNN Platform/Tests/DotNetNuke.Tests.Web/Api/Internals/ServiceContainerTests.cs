// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api.Internals
{
    using System.Web;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Tests.Instance.Utilities.HttpSimulator;
    using Moq;
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceContainerTests
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTransient<INavigationManager>(container => Mock.Of<INavigationManager>());
            serviceCollection.AddTransient<IApplicationStatusInfo>(container => new DotNetNuke.Application.ApplicationStatusInfo(Mock.Of<IApplicationInfo>()));
            
            Globals.DependencyProvider = serviceCollection.BuildServiceProvider();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Globals.DependencyProvider = null;
        }

        [Test]
        public void CreateScopeWhenRequestDoesNotExists()
        {
            Assert.Null(HttpContext.Current?.GetScope());

            var container = ServiceScopeContainer.GetRequestOrCreateScope();

            Assert.True(container.ShouldDispose);
        }

        [Test]
        public void UseRequestScopeWhenPossible()
        {
            using (var simulator = new HttpSimulator())
            {
                simulator.SimulateRequest();

                var scope = Globals.DependencyProvider.CreateScope();

                HttpContext.Current.SetScope(scope);

                Assert.NotNull(HttpContext.Current?.GetScope());

                var container = ServiceScopeContainer.GetRequestOrCreateScope();

                Assert.False(container.ShouldDispose);
                Assert.AreEqual(scope, container.ServiceScope);
            }
        }
    }
}
