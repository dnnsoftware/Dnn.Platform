using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ClientDependency.Core;
using ClientDependency.Core.CompositeFiles.Providers;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class PathBasedUrlFormatValidatorTest
    {
        [Test]
        public void PathBasedUrlFormat_Parse_1()
        {
            string fileKey;
            int version;
            ClientDependencyType type;
            var output = PathBasedUrlFormatter.Parse("{dependencyId}.{version}.{type}", "123456789.10.js", out fileKey, out type, out version);
            Assert.AreEqual(true, output);
            Assert.AreEqual("123456789", fileKey);
            Assert.AreEqual(10, version);
            Assert.AreEqual(ClientDependencyType.Javascript, type);
        }

        [Test]
        public void PathBasedUrlFormat_Parse_2()
        {
            string fileKey;
            int version;
            ClientDependencyType type;
            var output = PathBasedUrlFormatter.Parse("{dependencyId}/{version}/{type}", "123456789/1234/css", out fileKey, out type, out version);
            Assert.AreEqual(true, output);
            Assert.AreEqual("123456789", fileKey);
            Assert.AreEqual(1234, version);
            Assert.AreEqual(ClientDependencyType.Css, type);
        }

        [Test]
        public void PathBasedUrlFormat_Parse_3()
        {
            string fileKey;
            int version;
            ClientDependencyType type;
            var output = PathBasedUrlFormatter.Parse("{dependencyId}|@#{version}$%^{type}", "123456789|@#1234$%^css", out fileKey, out type, out version);
            Assert.AreEqual(true, output);
            Assert.AreEqual("123456789", fileKey);
            Assert.AreEqual(1234, version);
            Assert.AreEqual(ClientDependencyType.Css, type);
        }

        [Test]
        public void PathBasedUrlFormat_Create()
        {
            var output = PathBasedUrlFormatter.CreatePath("{dependencyId}.{version}.{type}", "123456789", ClientDependencyType.Javascript, 10);
            Assert.AreEqual("123456789.10.js", output);
        }

        [Test]
        public void PathBasedUrlFormat_Create_Exceeding_Max_Path()
        {
            var output = PathBasedUrlFormatter.CreatePath("{dependencyId}.{version}.{type}", "123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100asdfghjklqwertyuiopqwertyuiopzxcvbnmasdfghjklqwertyuiopasdfghjklpoiuytrew", ClientDependencyType.Javascript, 10);
            Assert.AreEqual("123456789101112131415161718192021222324252627282930313233343536373839404142434445464748495051525354555657585960616263646566676869707172737475767778798081828384858687888990919293949596979899100asdfghjklqwertyuiopqwertyuiopzxcvbnmasdfghjklqwe/rtyuiopasdfghjklpoiuytrew.10.js", output);
        }

        [Test]        
        public void PathBasedUrlFormat_Test_Valid_Format()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}.{type}");
            PathBasedUrlFormatter.Validate("{dependencyId}/{version}/{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_EndsWith()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}.{type}a");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_StartsWith()
        {
            PathBasedUrlFormatter.Validate("a{dependencyId}.{version}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Invalid_Delimiter1()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}}{version}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Invalid_Delimiter2()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}{{version}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Invalid_Delimiter3()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}}{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Invalid_Delimiter4()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}{{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Null_Or_Empty()
        {
            PathBasedUrlFormatter.Validate("");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Missing_DependencyId_Token()
        {
            PathBasedUrlFormatter.Validate("{version}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Missing_Version_Token()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Missing_Type_Token()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Missing_DependencyId_Delimiter()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}{version}.{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Missing_Version_Delimiter()
        {
            PathBasedUrlFormatter.Validate("{dependencyId}.{version}{type}");
        }

        [Test]
        [ExpectedException(typeof(FormatException))]
        public void PathBasedUrlFormat_Test_Token_Ordering()
        {
            PathBasedUrlFormatter.Validate("{version}.{dependencyId}.{type}");
        }

    }
}
