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

namespace DotNetNuke.Tests.Utilities
{
    public class HttpContextHelper
    {
        private static Mock<HttpContextBase> CrateMockHttpContext()
        {
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Items).Returns(new Hashtable());

            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(new NameValueCollection());
            request.SetupGet(x => x.Params).Returns(new NameValueCollection());
            context.SetupGet(x => x.Request).Returns(request.Object);

            context.SetupGet(x => x.Response).Returns(new Mock<HttpResponseBase>().Object);
            context.SetupGet(x => x.Session).Returns(new Mock<HttpSessionStateBase>().Object);
            context.SetupGet(x => x.Server).Returns(new Mock<HttpServerUtilityBase>().Object);

            return context;
        }

        public static Mock<HttpContextBase> RegisterMockHttpContext()
        {
            var mock = CrateMockHttpContext();
            HttpContextSource.RegisterInstance(mock.Object);
            return mock;
        }
    }
}