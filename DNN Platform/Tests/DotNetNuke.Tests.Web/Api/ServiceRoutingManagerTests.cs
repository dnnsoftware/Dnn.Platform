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

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.Internal.Reflection;
    using DotNetNuke.Framework.Reflections;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;

    using Moq;

    using NUnit.Framework;

    using ServicesRoutingManager = DotNetNuke.Web.Api.Internal.ServicesRoutingManager;

    [TestFixture]
    public class ServiceRoutingManagerTests
    {
        private static readonly List<string[]> EmptyStringArrays = new List<string[]>
        {
            null, Array.Empty<string>(), new[] { string.Empty }, new string[] { null },
        };

        private Mock<IPortalController> mockPortalController;
        private IPortalController portalController;
        private Mock<IPortalAliasService> mockPortalAliasService;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void Setup()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;

            this.mockPortalController = new Mock<IPortalController>();
            this.portalController = this.mockPortalController.Object;
            PortalController.SetTestableInstance(this.portalController);

            this.mockPortalAliasService = new Mock<IPortalAliasService>();
            this.mockPortalAliasService.As<IPortalAliasController>();

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockPortalAliasService.Object);
                    services.AddSingleton((IPortalAliasController)this.mockPortalAliasService.Object);
                    services.AddSingleton(this.mockPortalController.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();

            this.serviceProvider.Dispose();
            this.mockPortalAliasService = null;
        }

        [Test]
        public void LocatesAllServiceRouteMappers()
        {
            var assemblyLocator = new Mock<IAssemblyLocator>();

            // including the assembly with object ensures that the assignability is done correctly
            var assembliesToReflect = new IAssembly[2];
            assembliesToReflect[0] = new AssemblyWrapper(this.GetType().Assembly);
            assembliesToReflect[1] = new AssemblyWrapper(typeof(object).Assembly);

            assemblyLocator.Setup(x => x.Assemblies).Returns(assembliesToReflect);

            var locator = new TypeLocator { AssemblyLocator = assemblyLocator.Object };

            List<Type> types = locator.GetAllMatchingTypes(ServicesRoutingManager.IsValidServiceRouteMapper).ToList();

            // if new ServiceRouteMapper classes are added to the assembly they will likely need to be added here
            Assert.That(
                types, Is.EquivalentTo(new[]
                    {
                        typeof(FakeServiceRouteMapper),
                        typeof(ReflectedServiceRouteMappers.EmbeddedServiceRouteMapper),
                        typeof(ExceptionOnCreateInstanceServiceRouteMapper),
                        typeof(ExceptionOnRegisterServiceRouteMapper),
                    }));
        }

        [Test]
        public void NameSpaceRequiredOnMapRouteCalls([ValueSource(nameof(EmptyStringArrays))] string[] namespaces)
        {
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, new RouteCollection());

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
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, new RouteCollection()) { TypeLocator = tl };

            srm.RegisterRoutes();

            Assert.That(FakeServiceRouteMapper.RegistrationCalls, Is.EqualTo(1));
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
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, new RouteCollection()) { TypeLocator = tl };

            srm.RegisterRoutes();

            Assert.That(FakeServiceRouteMapper.RegistrationCalls, Is.EqualTo(1));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void UniqueNameRequiredOnMapRouteCalls(string uniqueName)
        {
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, new RouteCollection());

            Assert.Throws<ArgumentException>(() => srm.MapHttpRoute(uniqueName, "default", "url", null, new[] { "foo" }));
        }

        [Test]
        public void UrlCanStartWithSlash()
        {
            // Arrange
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(new ArrayList());

            // Act
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, new RouteCollection());

            // Assert
            Assert.DoesNotThrow(() => srm.MapHttpRoute("name", "default", "/url", null, new[] { "foo" }));
        }

        [Test]
        public void NameIsInsertedInRouteDataTokens()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });

            // Assert
            var route = (Route)routeCollection[0];
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
        }

        [Test]
        public void TwoRoutesOnTheSameFolderHaveSimilarNames()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });
            srm.MapHttpRoute("folder", "another", "alt/url", new[] { "foo" });

            // Assert
            var route = (Route)routeCollection[0];
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
            route = (Route)routeCollection[1];
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0-old"));
        }

        [Test]
        public void RoutesShouldHaveBackwardCompability()
        {
            // Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            this.mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);

            this.mockPortalAliasService.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com" }]);
            this.mockPortalAliasService.As<IPortalAliasController>().Setup(x => x.GetPortalAliasesByPortalId(0)).Returns([new PortalAliasInfo { HTTPAlias = "www.foo.com" }]);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(Globals.DependencyProvider, routeCollection);

            // Act
            srm.MapHttpRoute("folder", "default", "url", ["foo"]);

            // Assert
            var route = (Route)routeCollection[0];
            Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0"));
            route = (Route)routeCollection[1];
            Assert.Multiple(() =>
            {
                Assert.That(route.DataTokens["Name"], Is.EqualTo("folder-default-0-old"));
                Assert.That(route.Url, Does.StartWith("DesktopModules"));
            });
        }
    }
}
