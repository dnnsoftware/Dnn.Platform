// // DotNetNuke® - http://www.dotnetnuke.com
// // Copyright (c) 2002-2017
// // by DotNetNuke Corporation
// // 
// // Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// // documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// // the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// // to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// // 
// // The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// // of the Software.
// // 
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// // TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// // THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// // CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// // DEALINGS IN THE SOFTWARE.

using System.Collections;
using System.Collections.Specialized;
using System.Web;
using DotNetNuke.Common;
using Moq;
using System.IO;

namespace DotNetNuke.Tests.Utilities
{
    public class HttpContextHelper
    {
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

        /// <summary>
        /// Return Response object with default values for missing ones
        /// _mockRequest = Mock.Get(_mockhttpContext.Object.Request);
        /// syntax _mockRequest.SetupGet(x => x.[PropertyName]).Returns(...);
        /// e.g. SetupGet(x => x.ServerVariables).Returns(new NameValueCollection());
        /// </summary>
        /// <returns>HttpResponseBase</returns>
        public static Mock<HttpContextBase> RegisterMockHttpContext()
        {
            var mock = CrateMockHttpContext();
            HttpContextSource.RegisterInstance(mock.Object);
            return mock;
        }
    }
}