using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Rhino.Mocks;

namespace ClientDependency.UnitTests
{
    public class FakeHttpContextFactory
    {

        public FakeHttpContextFactory(string url)
        {
            //Http Request

            Request = MockRepository.GenerateMock<HttpRequestBase>();
            Request.Stub(x => x.AppRelativeCurrentExecutionFilePath).Return(url);
            Request.Stub(x => x.PathInfo).Return(string.Empty);
            Request.Stub(x => x.RawUrl).Return(VirtualPathUtility.ToAbsolute(url, "/"));
            Request.Stub(x => x.Url).Return(new Uri("http://MyTestWebsite"));
            Request.Stub(x => x.ApplicationPath).Return("/");

            //HTTP Context

            Context = MockRepository.GenerateMock<HttpContextBase>();
            Context.Stub(x => x.Cache).Return(HttpRuntime.Cache);
            Context.Stub(x => x.Items).Return(new Dictionary<string, object>());
            Context.Stub(x => x.Request).Return(Request);

            //HTTP server

            Context.Stub(x => x.Server).Return(new FakeServer());
        }

        public HttpContextBase Context { get; private set; }
        public HttpRequestBase Request { get; private set; }

        private class FakeServer : HttpServerUtilityBase
        {
            public override string MapPath(string path)
            {
                return path;
            }
        }

    }
}
