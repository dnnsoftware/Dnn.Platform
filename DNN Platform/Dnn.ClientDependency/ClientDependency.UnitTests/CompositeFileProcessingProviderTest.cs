using ClientDependency.Core.CompositeFiles.Providers;
using System.Linq;
using System;
using System.Web;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Moq;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
  
    [TestFixture]
    public class CompositeFileProcessingProviderTest
    {
        [TestCase(25, 25, 110)]
        [TestCase(25, 9, 220)]
        public void Split_Dependencies_On_Max_Length(int dependencyCount, int expectedResultCount, int maxLength)
        {
            //mock http server urlencode
            var server = new Mock<HttpServerUtilityBase>();
            server.Setup(s => s.UrlEncode(It.IsAny<string>())).Returns((string s) => HttpUtility.UrlEncode(s));
            var http = Mock.Of<HttpContextBase>(x => x.Server == server.Object);
            
            var provider = new TestCompositeFileProcessingProvider();
            var dependencies = new List<IClientDependencyFile>();
            for (int i = 0; i < dependencyCount; i++)
            {
                dependencies.Add(new JavascriptFile("/App_Plugins/MyPackage/js/test/test" + i + ".js"));
            }            
            var result = provider.GetCompositeFileUrls(
                ClientDependencyType.Javascript,
                dependencies.ToArray(),
                "/DependencyHandler.axd",
                http, maxLength, 43);

            Assert.AreEqual(expectedResultCount, result.Count());
            foreach (var r in result)
            {
                Assert.AreEqual(1, result.Count(x => x == r));
            }

        }

        [Test]
        public void Split_Dependencies_On_Max_Length_Throws()
        {
            //mock http server urlencode
            var server = new Mock<HttpServerUtilityBase>();
            server.Setup(s => s.UrlEncode(It.IsAny<string>())).Returns((string s) => HttpUtility.UrlEncode(s));
            var http = Mock.Of<HttpContextBase>(x => x.Server == server.Object);

            var provider = new TestCompositeFileProcessingProvider();
            var dependencies = new[]
                {
                    //this one will fit
                    new JavascriptFile("/App_Plugins/MyPackage/js/test/test1.js"),
                    //this one won't
                    new JavascriptFile("/App_Plugins/MyPackage/js/test/test10.js")
                };
            
            Assert.Throws<InvalidOperationException>(() => provider.GetCompositeFileUrls(
                ClientDependencyType.Javascript,
                dependencies.ToArray(),
                "/DependencyHandler.axd",
                http, 100, 43));
        }

        private class TestCompositeFileProcessingProvider: BaseCompositeFileProcessingProvider
        {
            public override FileInfo SaveCompositeFile(byte[] fileContents, ClientDependencyType type, HttpServerUtilityBase server)
            {
                throw new NotImplementedException();
            }

            public override byte[] CombineFiles(string[] filePaths, HttpContextBase context, ClientDependencyType type, out List<CompositeFileDefinition> fileDefs)
            {
                throw new NotImplementedException();
            }

            public override byte[] CompressBytes(CompressionType type, byte[] fileBytes)
            {
                throw new NotImplementedException();
            }
        }
    }
}
