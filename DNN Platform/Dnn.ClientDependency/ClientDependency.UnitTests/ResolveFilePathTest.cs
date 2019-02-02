using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    using System;
    using System.Web;

    using Moq;

    [TestFixture]
    public class ResolveFilePathTest
    {
        [Test]
        public void EmptyPath_Throws()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "", };

            Assert.Throws<ArgumentException>(() => file.ResolveFilePath(mockHttp));
        }

        [Test]
        public void WhiteSpacePath_Throws()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "  ", };

            Assert.Throws<ArgumentException>(() => file.ResolveFilePath(mockHttp));
        }

        [Test]
        public void TildePath_IsResolved()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            Mock.Get(mockHttp).DefaultValue = DefaultValue.Mock;
            Mock.Get(mockHttp.Request).Setup(r => r.ApplicationPath).Returns("/");

            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "~/file.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("/file.js", resolvedPath);
        }

        [Test]
        public void AbsoluteUrl_IsUnaltered()
        {
            var mockHttp = Mock.Of<HttpContextBase>();

            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "https://cdn.example.com/file.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("https://cdn.example.com/file.js", resolvedPath);
        }

        [Test]
        public void AbsolutePath_IsUnaltered()
        {
            var mockHttp = Mock.Of<HttpContextBase>();

            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "/file.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("/file.js", resolvedPath);
        }

        [Test]
        public void RelativePath_HasCurrentExecutionPathPrefixed()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            Mock.Get(mockHttp).DefaultValue = DefaultValue.Mock;
            Mock.Get(mockHttp.Request).Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns("/the/path/page.aspx");

            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "file.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("/the/path/file.js", resolvedPath);
        }

        [Test]
        public void NonCanonicalRelativePath_IsCanonicalized()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            Mock.Get(mockHttp).DefaultValue = DefaultValue.Mock;
            Mock.Get(mockHttp.Request).Setup(r => r.AppRelativeCurrentExecutionFilePath).Returns("/the/path/page.aspx");

            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "../file.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("/the/file.js", resolvedPath);
        }

        [Test]
        public void NonCanonicalAbsolutePath_IsCanonicalized()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "/website/folder/../js.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("/website/js.js", resolvedPath);
        }

        [Test]
        public void Schema_Relative_Path()
        {
            var mockHttp = Mock.Of<HttpContextBase>();
            var file = new BasicFile(ClientDependencyType.Javascript) { FilePath = "//website/js.js", };

            var resolvedPath = file.ResolveFilePath(mockHttp);

            Assert.AreEqual("//website/js.js", resolvedPath);
        }
    }
}