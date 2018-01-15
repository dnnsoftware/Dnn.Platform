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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using DotNetNuke.Web.Api;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.Api
{
    [TestFixture]
    public class DnnHttpControllerSelectorTests
    {
        [TestCase("Sample")]
        [TestCase("SAmple")]
        [TestCase("SAMPLE")]
        public void SelectControllerIsCaseInsensitive(string controllerTypeName)
        {
            const string expectedNamespace = "ExpectedNamespace";

            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType("Sample", expectedNamespace);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            var request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = controllerTypeName;
            routeData.Route.SetNameSpaces(new[] {expectedNamespace});
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);

            // Act
            HttpControllerDescriptor descriptor = selector.SelectController(request);

            // Assert
            Assert.AreEqual(controllerType, descriptor.ControllerType);
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void SelectControllerIgnoresControllersInUnexpectedNamespaces()
        {
            const string expectedNamespace = "ExpectedNamespace";
            const string unexpectedNamespace = "UnexpectedNamespace";
            const string controllerName = "Sample";

            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);
            
            Type controllerType = GetMockControllerType(controllerName, unexpectedNamespace);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType });

            var request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = controllerName;
            routeData.Route.SetNameSpaces(new[] { expectedNamespace });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);

            // Act
            selector.SelectController(request);
        }

        [Test]
        [ExpectedException(typeof(HttpResponseException))]
        public void SelectControllerThrowsOnAmgiuousControllers()
        {
            const string expectedNamespace = "ExpectedNamespace";
            const string expectedNamespace2 = "ExpectedNamespace2";
            const string controllerName = "Sample";

            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            configuration.Services.Replace(typeof(IHttpControllerTypeResolver), controllerTypeResolver.Object);

            Type controllerType = GetMockControllerType(controllerName, expectedNamespace);
            Type controllerType2 = GetMockControllerType(controllerName, expectedNamespace2);
            controllerTypeResolver
                .Setup(c => c.GetControllerTypes(It.IsAny<IAssembliesResolver>()))
                .Returns(new Collection<Type> { controllerType, controllerType2 });

            var request = new HttpRequestMessage();
            IHttpRouteData routeData = GetRouteData();
            routeData.Values["controller"] = controllerName;
            routeData.Route.SetNameSpaces(new[] { expectedNamespace, expectedNamespace2 });
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = routeData;

            var selector = new DnnHttpControllerSelector(configuration);

            // Act
            selector.SelectController(request);
        }

        private static IHttpRouteData GetRouteData()
        {
            var mockRoute = new Mock<IHttpRoute>();
            mockRoute.SetupGet(x => x.DataTokens).Returns(new Dictionary<string, object>());
            var routeData = new HttpRouteData(mockRoute.Object);

            return routeData;
        }

        private static Type GetMockControllerType(string controllerName, string @namespace)
        {
            var mockType = new Mock<Type>();

            mockType.Setup(t => t.Name).Returns(controllerName + "Controller");
            mockType.Setup(t => t.FullName).Returns(string.Format("{0}.{1}Controller", @namespace, controllerName));
            return mockType.Object;
        }
    }
}