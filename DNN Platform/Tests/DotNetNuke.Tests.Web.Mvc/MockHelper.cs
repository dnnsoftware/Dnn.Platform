#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Collections.Specialized;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace DotNetNuke.Tests.Web.Mvc
{
    public class MockHelper
    {
        public static HttpContextBase CreateMockHttpContext()
        {
            return CreateMockHttpContext(new Dictionary<string, object>());
        }

        public static HttpContextBase CreateMockHttpContext(Dictionary<string, object> items)
        {
            var mockContext = new Mock<HttpContextBase>();
            mockContext.SetupGet(c => c.Items)
                       .Returns(items);

            var mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(r => r.QueryString)
                       .Returns(new NameValueCollection());
            mockRequest.Setup(r => r.RequestContext)
                       .Returns(new RequestContext());

            var mockResponse = new Mock<HttpResponseBase>();

            mockContext.SetupGet(c => c.Request)
                       .Returns(mockRequest.Object);
            mockContext.SetupGet(c => c.Response)
                       .Returns(mockResponse.Object);
            
            return mockContext.Object;
        }

        public static HttpContextBase CreateMockHttpContext(string requestUrl)
        {
            HttpContextBase httpContext = CreateMockHttpContext();

            var mockRequest = Mock.Get(httpContext.Request);
            mockRequest.Setup(r => r.Url)
                       .Returns(new Uri(requestUrl));
            mockRequest.Setup(r => r.ApplicationPath)
                       .Returns(new Uri(requestUrl).AbsolutePath);
            mockRequest.Setup(r => r.RawUrl)
                       .Returns(requestUrl);

            var mockResponse = new Mock<HttpResponseBase>();
            mockResponse.Setup(r => r.ApplyAppPathModifier(It.IsAny<string>()))
                        .Returns<string>(s => s);

            Mock.Get(httpContext)
                .SetupGet(c => c.Response)
                .Returns(mockResponse.Object);

            return httpContext;
        }

        public static ControllerContext CreateMockControllerContext(ControllerBase controller)
        {
            return new ControllerContext(CreateMockHttpContext(), new RouteData(), controller);
        }

        public static ControllerContext CreateMockControllerContext(HttpContextBase httpContext)
        {
            return new ControllerContext(httpContext, new RouteData(), new Mock<ControllerBase>().Object);
        }

        public static ControllerContext CreateMockControllerContext()
        {
            return CreateMockControllerContext(new Mock<ControllerBase>().Object);
        }

        public static ControllerContext CreateMockControllerContext(RouteData routeDatae)
        {
            return new ControllerContext(CreateMockHttpContext(), routeDatae, new Mock<ControllerBase>().Object);
        }

        public static ControllerContext CreateMockControllerContext(HttpContextBase httpContext, RouteData routeData)
        {
            return new ControllerContext(httpContext?? CreateMockHttpContext(), routeData, new Mock<ControllerBase>().Object);
        }

        public static ViewContext CreateViewContext(string url)
        {
            var routeData = new RouteData();
            routeData.Values["controller"] = "Controller";
            routeData.Values["action"] = "Action";
            routeData.Values["id"] = "Id";
            var controllerContext = new ControllerContext(CreateMockHttpContext(url),
                                                                        routeData,
                                                                        new Mock<ControllerBase>().Object);
            return new ViewContext(controllerContext,
                                   new Mock<IView>().Object,
                                   new ViewDataDictionary(),
                                   new TempDataDictionary(),
                                   new StringWriter());
        }
    }
}
