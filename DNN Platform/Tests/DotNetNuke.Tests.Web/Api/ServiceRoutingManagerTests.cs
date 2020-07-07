// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Web.Api
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Routing;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.DependencyInjection;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    using ServicesRoutingManager = DotNetNuke.Web.Api.Internal.ServicesRoutingManager;

    [TestFixture]
    public class ServiceRoutingManagerTests
    {
        // ReSharper disable UnusedMember.Local
        private readonly List<string[]> _emptyStringArrays = new List<string[]>
                                                        { null, new string[0], new[] { string.Empty }, new string[] { null } };

        // ReSharper restore UnusedMember.Local
        private Mock<IPortalController> _mockPortalController;
        private IPortalController _portalController;

        [SetUp]
        public void Setup()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;

            this._mockPortalController = new Mock<IPortalController>();
            this._portalController = this._mockPortalController.Object;
            PortalController.SetTestableInstance(this._portalController);

            var navigationManagerMock = new Mock<INavigationManager>();
            var services = new ServiceCollection();
            services.AddScoped(typeof(INavigationManager), (x) => navigationManagerMock.Object);
            Globals.DependencyProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();

            if (Globals.DependencyProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Globals.DependencyProvider = null;
        }

        [Test]
        public void LocatesAllServiceRouteMappers()
        {
            var assemblyLocator = new Mock<IAssemblyLocator>();

            // including the assembly with object ensures that the assignabliity is done correctly
            var assembliesToReflect = new IAssembly[2];
            assembliesToReflect[0] = new AssemblyWrapper(this.GetType().Assembly);
            assembliesToReflect[1] = new AssemblyWrapper(typeof(object).Assembly);

            assemblyLocator.Setup(x => x.Assemblies).Returns(assembliesToReflect);

            var locator = new TypeLocator { AssemblyLocator = assemblyLocator.Object };

            List<Type> types = locator.GetAllMatchingTypes(ServicesRoutingManager.IsValidServiceRouteMapper).ToList();

            // if new ServiceRouteMapper classes are added to the assembly they willl likely need to be added here
            CollectionAssert.AreEquivalent(
                new[]
                    {
                        typeof(FakeServiceRouteMapper),
                        typeof(ReflectedServiceRouteMappers.EmbeddedServiceRouteMapper),
                        typeof(ExceptionOnCreateInstanceServiceRouteMapper),
                        typeof(ExceptionOnRegisterServiceRouteMapper),
                    }, types);
        }

        [Test]
        public void NameSpaceRequiredOnMapRouteCalls([ValueSource("_emptyStringArrays")] string[] namespaces)
        {
            var srm = new ServicesRoutingManager(new RouteCollection());

            Assert.Throws<ArgumentException>(() => srm.MapHttpRoute("usm", "default", "url", null, namespaces));
        }

        [Test]
        public void RegisterRoutesIsCalledOnAllServiceRouteMappersEvenWhenSomeThrowExceptions()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[]
                                                          {
                                                              typeof(ExceptionOnRegisterServiceRouteMapper),
                                                              typeof(ExceptionOnCreateInstanceServiceRouteMapper),
                                                              typeof(FakeServiceRouteMapper),
                                                          });
            var al = new Mock<IAssemblyLocator>();
            al.Setup(x => x.Assemblies).Returns(new[] { assembly.Object });
            var tl = new TypeLocator { AssemblyLocator = al.Object };
            var srm = new ServicesRoutingManager(new RouteCollection()) { TypeLocator = tl };

            srm.RegisterRoutes();

            Assert.AreEqual(1, FakeServiceRouteMapper.RegistrationCalls);
        }

        [Test]
        public void RegisterRoutesIsCalledOnServiceRouteMappers()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[] { typeof(FakeServiceRouteMapper) });
            var al = new Mock<IAssemblyLocator>();
            al.Setup(x => x.Assemblies).Returns(new[] { assembly.Object });
            var tl = new TypeLocator { AssemblyLocator = al.Object };
            var srm = new ServicesRoutingManager(new RouteCollection()) { TypeLocator = tl };

            srm.RegisterRoutes();

            Assert.AreEqual(1, FakeServiceRouteMapper.RegistrationCalls);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void UniqueNameRequiredOnMapRouteCalls(string uniqueName)
        {
            var srm = new ServicesRoutingManager(new RouteCollection());

            Assert.Throws<ArgumentException>(() => srm.MapHttpRoute(uniqueName, "default", "url", null, new[] { "foo" }));
        }

        [Test]
        public void UrlCanStartWithSlash()
        {
            // Arrange
            this._mockPortalController.Setup(x => x.GetPortals()).Returns(new ArrayList());

            // Act
            var srm = new ServicesRoutingManager(new RouteCollection());

            // Assert
            Assert.DoesNotThrow(() => srm.MapHttpRoute("name", "default", "/url", null, new[] { "foo" }));
        }

        [Test]
        public void NameIsInsertedInRouteDataTokens()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this._mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });

            // Assert
            var route = (Route)routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
        }

        [Test]
        public void TwoRoutesOnTheSameFolderHaveSimilarNames()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this._mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });
            srm.MapHttpRoute("folder", "another", "alt/url", new[] { "foo" });

            // Assert
            var route = (Route)routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
            route = (Route)routeCollection[1];
            Assert.AreEqual("folder-default-0-old", route.DataTokens["Name"]);
        }

        [Test]
        public void RoutesShouldHaveBackwardCompability()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this._mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });

            // Assert
            var route = (Route)routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
            route = (Route)routeCollection[1];
            Assert.AreEqual("folder-default-0-old", route.DataTokens["Name"]);
            Assert.IsTrue(route.Url.StartsWith("DesktopModules"));
        }
    }
}
