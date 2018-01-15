#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework.Internal.Reflection;
using DotNetNuke.Framework.Reflections;

using Moq;
using NUnit.Framework;
using ServicesRoutingManager = DotNetNuke.Web.Api.Internal.ServicesRoutingManager;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class ServiceRoutingManagerTests
    {
        // ReSharper disable UnusedMember.Local
        private readonly List<string[]> _emptyStringArrays = new List<string[]>
                                                        {null, new string[0], new[] {""}, new string[] {null}};
        // ReSharper restore UnusedMember.Local
        private Mock<IPortalController> _mockPortalController;
        private IPortalController _portalController;

        [SetUp]
        public void Setup()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;

            _mockPortalController = new Mock<IPortalController>();
            _portalController = _mockPortalController.Object;
            PortalController.SetTestableInstance(_portalController);
        }

        [TearDown]
        public void TearDown()
        {
            PortalController.ClearInstance();
        }

        [Test]
        public void LocatesAllServiceRouteMappers()
        {
            var assemblyLocator = new Mock<IAssemblyLocator>();

            //including the assembly with object ensures that the assignabliity is done correctly
            var assembliesToReflect = new IAssembly[2];
            assembliesToReflect[0] = new AssemblyWrapper(GetType().Assembly);
            assembliesToReflect[1] = new AssemblyWrapper(typeof (Object).Assembly);

            assemblyLocator.Setup(x => x.Assemblies).Returns(assembliesToReflect);

            var locator = new TypeLocator {AssemblyLocator = assemblyLocator.Object};

            List<Type> types = locator.GetAllMatchingTypes(ServicesRoutingManager.IsValidServiceRouteMapper).ToList();

            //if new ServiceRouteMapper classes are added to the assembly they willl likely need to be added here
            CollectionAssert.AreEquivalent(
                new[]
                    {
                        typeof (FakeServiceRouteMapper),
                        typeof (ReflectedServiceRouteMappers.EmbeddedServiceRouteMapper),
                        typeof (ExceptionOnCreateInstanceServiceRouteMapper),
                        typeof (ExceptionOnRegisterServiceRouteMapper)
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
                                                              typeof (ExceptionOnRegisterServiceRouteMapper),
                                                              typeof (ExceptionOnCreateInstanceServiceRouteMapper),
                                                              typeof (FakeServiceRouteMapper)
                                                          });
            var al = new Mock<IAssemblyLocator>();
            al.Setup(x => x.Assemblies).Returns(new[] {assembly.Object});
            var tl = new TypeLocator {AssemblyLocator = al.Object};
            var srm = new ServicesRoutingManager(new RouteCollection()) {TypeLocator = tl};

            srm.RegisterRoutes();

            Assert.AreEqual(1, FakeServiceRouteMapper.RegistrationCalls);
        }

        [Test]
        public void RegisterRoutesIsCalledOnServiceRouteMappers()
        {
            FakeServiceRouteMapper.RegistrationCalls = 0;
            var assembly = new Mock<IAssembly>();
            assembly.Setup(x => x.GetTypes()).Returns(new[] {typeof (FakeServiceRouteMapper)});
            var al = new Mock<IAssemblyLocator>();
            al.Setup(x => x.Assemblies).Returns(new[] {assembly.Object});
            var tl = new TypeLocator {AssemblyLocator = al.Object};
            var srm = new ServicesRoutingManager(new RouteCollection()) {TypeLocator = tl};

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
            //Arrange
            _mockPortalController.Setup(x => x.GetPortals()).Returns(new ArrayList());
            
            //Act
            var srm = new ServicesRoutingManager(new RouteCollection());

            //Assert
            Assert.DoesNotThrow(() => srm.MapHttpRoute("name", "default", "/url", null, new[] { "foo" }));
        }

        [Test]
        public void NameIsInsertedInRouteDataTokens()
        {
            //Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            _mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            //Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });

            //Assert
            var route = (Route) routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
        }

        [Test]
        public void TwoRoutesOnTheSameFolderHaveSimilarNames()
        {
            //Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            _mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            //Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });
            srm.MapHttpRoute("folder", "another", "alt/url", new[] { "foo" });

            //Assert
            var route = (Route)routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
            route = (Route)routeCollection[1];
            Assert.AreEqual("folder-default-0-old", route.DataTokens["Name"]);
        }

        [Test]
        public void RoutesShouldHaveBackwardCompability()
        {
            //Arrange
            var portalInfo = new ArrayList { new PortalInfo { PortalID = 0 } };
            _mockPortalController.Setup(x => x.GetPortals()).Returns(portalInfo);
            var mockPac = new Mock<IPortalAliasController>();
            mockPac.Setup(x => x.GetPortalAliasesByPortalId(0)).Returns(new[] { new PortalAliasInfo { HTTPAlias = "www.foo.com" } });
            PortalAliasController.SetTestableInstance(mockPac.Object);

            var routeCollection = new RouteCollection();
            var srm = new ServicesRoutingManager(routeCollection);

            //Act
            srm.MapHttpRoute("folder", "default", "url", new[] { "foo" });

            //Assert
            var route = (Route)routeCollection[0];
            Assert.AreEqual("folder-default-0", route.DataTokens["Name"]);
            route = (Route)routeCollection[1];
            Assert.AreEqual("folder-default-0-old", route.DataTokens["Name"]);
            Assert.IsTrue(route.Url.StartsWith("DesktopModules"));
        }
    }
}