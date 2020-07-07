// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Web;

    using DotNetNuke.Common;
    using Moq;

    public class HttpContextHelper
    {
        /// <summary>
        /// Return Response object with default values for missing ones
        /// _mockRequest = Mock.Get(_mockhttpContext.Object.Request);
        /// syntax _mockRequest.SetupGet(x => x.[PropertyName]).Returns(...);
        /// e.g. SetupGet(x => x.ServerVariables).Returns(new NameValueCollection()).
        /// </summary>
        /// <returns>HttpResponseBase.</returns>
        public static Mock<HttpContextBase> RegisterMockHttpContext()
        {
            var mock = CrateMockHttpContext();
            HttpContextSource.RegisterInstance(mock.Object);
            return mock;
        }

        private static Mock<HttpContextBase> CrateMockHttpContext()
        {
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Items).Returns(new Hashtable());
            context.SetupGet(x => x.Request).Returns(GetMockRequestBase());
            context.SetupGet(x => x.Response).Returns(GetMockResponseBase());
            context.SetupGet(x => x.Session).Returns(GetMockSessionStateBase());
            context.SetupGet(x => x.Server).Returns(GeMocktServerUtilityBase());
            return context;
        }

        private static HttpServerUtilityBase GeMocktServerUtilityBase()
        {
            var mockServerUtility = new Mock<HttpServerUtilityBase>();
            mockServerUtility.SetupAllProperties();
            return mockServerUtility.Object;
        }

        private static HttpSessionStateBase GetMockSessionStateBase()
        {
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupAllProperties();
            return mockSession.Object;
        }

        private static HttpResponseBase GetMockResponseBase()
        {
            var mockResponse = new Mock<HttpResponseBase>();
            mockResponse.SetupAllProperties();
            return mockResponse.Object;
        }

        private static HttpRequestBase GetMockRequestBase()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupAllProperties();
            request.SetupGet(x => x.Headers).Returns(new NameValueCollection());
            request.SetupGet(x => x.InputStream).Returns(new MemoryStream());
            request.SetupGet(x => x.Params).Returns(new NameValueCollection());
            request.SetupGet(x => x.QueryString).Returns(new NameValueCollection());
            request.SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());
            request.SetupGet(x => x.UserAgent).Returns(string.Empty);
            request.SetupGet(x => x.UserHostAddress).Returns(string.Empty);
            request.SetupGet(x => x.UserLanguages).Returns(new[] { string.Empty });
            return request.Object;
        }
    }
}
