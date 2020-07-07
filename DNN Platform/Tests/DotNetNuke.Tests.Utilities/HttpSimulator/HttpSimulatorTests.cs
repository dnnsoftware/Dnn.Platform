// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace UnitTests.Subtext
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Web;
    using System.Web.Hosting;

    using DotNetNuke.Tests.Instance.Utilities.HttpSimulator;
    using NUnit.Framework;

    ////[TestFixture]
    public class HttpSimulatorTests
    {
        ////[Test]
        public void CanGetSetSession()
        {
            using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
            {
                HttpContext.Current.Session["Test"] = "Success";
                Assert.AreEqual("Success", HttpContext.Current.Session["Test"], "Was not able to retrieve session variable.");
            }
        }

        ////[Test]
        public void CanGetSetApplicationVariables()
        {
            using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
            {
                HttpContext.Current.Application["Test"] = "Success";
                Assert.AreEqual("Success", HttpContext.Current.Application["Test"], "Was not able to retrieve application variable.");
            }
        }

        ////[Test]
        public void TestHttpHandlerWritesCorrectResponse()
        {
            using (var simulator = new HttpSimulator("/", @"c:\inetpub\"))
            {
                simulator.SetFormVariable("username", "phil")
                    .SetReferer(new Uri("http://example.com/1/"))
                    .SimulateRequest(new Uri("http://localhost/MyHandler.ashx?id=1234"));

                var handler = new TestHttpHandler();
                handler.ProcessRequest(HttpContext.Current);
                HttpContext.Current.Response.Flush();

                const string expected = @"c:\inetpub\MyHandler.ashx:phil:1234:http://example.com/1/";
                Assert.AreEqual(expected, simulator.ResponseText, "The Expected Response is all wrong.");
            } // HttpContext.Current is set to null again.
        }

        ////[Test]
        public void CanDispose()
        {
            using (var simulator = new HttpSimulator())
            {
                simulator.SimulateRequest();
                Assert.IsNotNull(HttpContext.Current);
            }

            Assert.IsNull(HttpContext.Current);
        }

        /*//[RowTest]
        ////[Row("http://localhost/Test/Default.aspx", "/Test", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "/Test/", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "//Test//", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "/Test", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "/Test/", "/Test")]
        ////[Row("http://localhost/Test/Subtest/Default.aspx", "//Test//", "/Test")]
        ////[Row("http://localhost/Test/Default.aspx", "", "/")]
        ////[Row("http://localhost/Test/Default.aspx", "/", "/")]
        ////[Row("http://localhost/Test/Default.aspx", null, "/")]*/
        public void CanSetApplicationPathCorrectly(string url, string appPath, string expectedAppPath)
        {
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\site1\test");
            Assert.AreEqual(expectedAppPath, simulator.ApplicationPath);
            simulator.SimulateRequest(new Uri(url));
            Assert.AreEqual(expectedAppPath, HttpContext.Current.Request.ApplicationPath);
            Assert.AreEqual(expectedAppPath, HttpRuntime.AppDomainAppVirtualPath);
            Assert.AreEqual(expectedAppPath, HostingEnvironment.ApplicationVirtualPath);
        }

        /*//[RowTest]
        ////[Row("http://localhost/Test/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\default.aspx")]
        ////[Row("http://localhost/Test/Subtest/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\Subtest\default.aspx")]
        ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]
        ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]*/
        public void CanSetAppPhysicalPathCorrectly(string url, string appPath, string appPhysicalPath, string expectedPhysicalAppPath, string expectedPhysicalPath)
        {
            var simulator = new HttpSimulator(appPath, appPhysicalPath);
            Assert.AreEqual(expectedPhysicalAppPath, simulator.PhysicalApplicationPath);
            simulator.SimulateRequest(new Uri(url), HttpVerb.GET);

            Assert.AreEqual(expectedPhysicalPath, simulator.PhysicalPath);
            Assert.AreEqual(expectedPhysicalAppPath, HttpRuntime.AppDomainAppPath);
            Assert.AreEqual(expectedPhysicalAppPath, HostingEnvironment.ApplicationPhysicalPath);
            Assert.AreEqual(expectedPhysicalPath, HttpContext.Current.Request.PhysicalPath);
        }

        ////[Test]
        public void CanGetQueryString()
        {
            var simulator = new HttpSimulator();
            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=value1&param2=value2&param3=value3"));
            for (var i = 1; i <= 3; i++)
            {
                Assert.AreEqual("value" + i, HttpContext.Current.Request.QueryString["param" + i], "Could not find query string field 'param{0}'", i);
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=new-value1&param2=new-value2&param3=new-value3&param4=new-value4"));
            for (var i = 1; i <= 4; i++)
            {
                Assert.AreEqual("new-value" + i, HttpContext.Current.Request.QueryString["param" + i], "Could not find query string field 'param{0}'", i);
            }

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?"));
            Assert.AreEqual(string.Empty, HttpContext.Current.Request.QueryString.ToString());
            Assert.AreEqual(0, HttpContext.Current.Request.QueryString.Count);

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"));
            Assert.AreEqual(string.Empty, HttpContext.Current.Request.QueryString.ToString());
            Assert.AreEqual(0, HttpContext.Current.Request.QueryString.Count);

            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param-name"));
            Assert.AreEqual("param-name", HttpContext.Current.Request.QueryString.ToString());
            Assert.AreEqual(1, HttpContext.Current.Request.QueryString.Count);
            Assert.IsNull(HttpContext.Current.Request.QueryString["param-name"]);
        }

        // //[Test]
        public void CanSimulateFormPost()
        {
            using (var simulator = new HttpSimulator())
            {
                var form = new NameValueCollection { { "Test1", "Value1" }, { "Test2", "Value2" } };
                simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"), form);

                Assert.AreEqual("Value1", HttpContext.Current.Request.Form["Test1"]);
                Assert.AreEqual("Value2", HttpContext.Current.Request.Form["Test2"]);
            }

            using (var simulator = new HttpSimulator())
            {
                simulator.SetFormVariable("Test1", "Value1")
                    .SetFormVariable("Test2", "Value2")
                    .SimulateRequest(new Uri("http://localhost/Test.aspx"));

                Assert.AreEqual("Value1", HttpContext.Current.Request.Form["Test1"]);
                Assert.AreEqual("Value2", HttpContext.Current.Request.Form["Test2"]);
            }
        }

        // //[Test]
        public void CanGetResponse()
        {
            var simulator = new HttpSimulator();
            simulator.SimulateRequest();
            HttpContext.Current.Response.Write("Hello World!");
            HttpContext.Current.Response.Flush();
            Assert.AreEqual("Hello World!", simulator.ResponseText);
        }

        // //[Test]
        public void CanGetReferer()
        {
            var simulator = new HttpSimulator();
            simulator.SetReferer(new Uri("http://example.com/Blah.aspx")).SimulateRequest();
            Assert.AreEqual(new Uri("http://example.com/Blah.aspx"), HttpContext.Current.Request.UrlReferrer);

            simulator = new HttpSimulator();
            simulator.SimulateRequest().SetReferer(new Uri("http://x.example.com/Blah.aspx"));
            Assert.AreEqual(new Uri("http://x.example.com/Blah.aspx"), HttpContext.Current.Request.UrlReferrer);
        }

        /*//[RowTest]
        ////[Row("http://localhost:60653/Test.aspx", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test.aspx?test=true", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/Test/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test/Test.aspx", @"c:\InetPub\wwwRoot\")]
        ////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        ////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath/", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]*/
        public void CanParseRequestUrl(string url, string appPath, string physicalPath, string expectedHost, int expectedPort, string expectedAppPath, string expectedPage, string expectedAppDomainAppPath)
        {
            var simulator = new HttpSimulator(appPath, physicalPath);
            Assert.AreEqual(expectedAppPath, simulator.ApplicationPath);
            Assert.AreEqual(expectedAppDomainAppPath, simulator.PhysicalApplicationPath);
        }

        // [RowTest]
        ////[Row("http://localhost/AppPath/default.aspx", "/AppPath", "/AppPath/default.aspx")]
        ////[Row("http://localhost/AppPath/default.aspx", "/", "/AppPath/default.aspx")]
        public void CanGetLocalPathCorrectly(string url, string appPath, string expectedLocalPath)
        {
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\AppPath\");
            simulator.SimulateRequest(new Uri(url));
            Assert.AreEqual(expectedLocalPath, HttpContext.Current.Request.Path);
            Assert.AreEqual(expectedLocalPath, HttpContext.Current.Request.Url.LocalPath);
        }

        // [RowTest]
        //////[Row("http://localhost:60653/Test.aspx", null, null, "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/Test/Test.aspx", "/", @"c:\InetPub\wwwRoot\", "localhost", 60653, "/", "/Test/Test.aspx", @"c:\InetPub\wwwRoot\")]
        //////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        //////[Row("http://localhost:60653/AppPath/Test.aspx", "/AppPath/", @"c:\InetPub\wwwRoot\AppPath\", "localhost", 60653, "/AppPath", "/AppPath/Test.aspx", @"c:\InetPub\wwwRoot\AppPath\")]
        public void CanSimulateRequest(string url, string appPath, string physicalPath, string expectedHost, int expectedPort, string expectedAppPath, string expectedLocalPath, string expectedPhysicalPath)
        {
            var simulator = new HttpSimulator(appPath, physicalPath);
            simulator.SimulateRequest(new Uri(url));

            Assert.AreEqual(expectedHost, HttpContext.Current.Request.Url.Host);
            Assert.AreEqual(expectedPort, HttpContext.Current.Request.Url.Port);
            Assert.AreEqual(expectedAppPath, HttpContext.Current.Request.ApplicationPath);
            Assert.AreEqual(expectedPhysicalPath, HttpContext.Current.Request.PhysicalApplicationPath);
            Assert.AreEqual(expectedLocalPath, HttpContext.Current.Request.Url.LocalPath);
        }

        // [RowTest]
        ////[Row("/", "/", @"c:\inetpub\wwwroot\")]
        ////[Row("/Test/Test.aspx", "/", @"c:\inetpub\wwwroot\Test\Test.aspx")]
        ////[Row("/Test/Blah/Test.aspx", "/", @"c:\inetpub\wwwroot\Test\Blah\Test.aspx")]
        ////[Row("/Test", "/Test", @"c:\inetpub\wwwroot")]
        ////[Row("/Test/", "/Test", @"c:\inetpub\wwwroot\")]
        public void CanMapPath(string virtualPath, string appPath, string expectedMapPath)
        {
            var url = new Uri("http://localhost/Test/Test.aspx");
            var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\");
            simulator.SimulateRequest(url);

            // Create a virtual path object.
            var vpath = ReflectionHelper.Instantiate("System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", new[] { typeof(string) }, virtualPath);
            Assert.IsNotNull(vpath);

            var environment = HttpSimulatorTester.CallGetEnvironment();

            var vpathString = ReflectionHelper.InvokeProperty<string>(vpath, "VirtualPathString");
            var appVirtPath = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_appVirtualPath", environment);
            Assert.IsNotNull(appVirtPath);
            Console.WriteLine("VPATH: " + vpath);
            Console.WriteLine("App-VPATH: " + appVirtPath);

            Console.WriteLine("vpath.VirtualPathString == '{0}'", vpathString);

            var mapping = ReflectionHelper.InvokeNonPublicMethod<string>(typeof(HostingEnvironment), "GetVirtualPathToFileMapping", vpath);
            Console.WriteLine("GetVirtualPathToFileMapping: --->{0}<---", mapping ?? "{NULL}");

            var o = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_configMapPath", environment);
            Console.WriteLine("_configMapPath: {0}", o ?? "{null}");

            var mappedPath = ReflectionHelper.InvokeNonPublicMethod<string>(environment, "MapPathActual", vpath, false);
            Console.WriteLine("MAPPED: " + mappedPath);
            Assert.AreEqual(expectedMapPath, HttpContext.Current.Request.MapPath(virtualPath));
        }

        // [Test]
        public void CanInstantiateVirtualPath()
        {
            var virtualPathType = Type.GetType("System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
            var constructor = virtualPathType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);
            Assert.IsNotNull(constructor);
        }

        // [Test]
        public void CanGetHostingEnvironment()
        {
            var environment = HttpSimulatorTester.CallGetEnvironment();
            Assert.IsNotNull(environment);
            environment = HttpSimulatorTester.CallGetEnvironment();
            Assert.IsNotNull(environment);
        }

        // [RowTest]
        ////[Row("/", "/")]
        ////[Row("", "/")]
        ////[Row("/test", "/test")]
        ////[Row("test/", "/test")]
        ////[Row("/test/", "/test")]
        ////[Row("/test/////", "/test")]
        ////[Row("////test/", "/test")]
        ////[Row("/test/test////", "/test/test")]
        ////[Row("/////test/test////", "/test/test")]
        ////[Row("/////test///test////", "/test/test")]
        public void CanNormalizeSlashes(string s, string expected)
        {
            Assert.AreEqual(expected, HttpSimulatorTester.CallNormalizeSlashes(s));
        }

        // [Test]
        public void CanStripTrailing()
        {
            Assert.AreEqual(@"c:\blah\blah2", HttpSimulatorTester.CallStripTrailingBackSlashes(@"c:\blah\blah2\"));
        }

        internal class TestHttpHandler : IHttpHandler
        {
            public bool IsReusable
            {
                get { return true; }
            }

            public void ProcessRequest(HttpContext context)
            {
                var physicalPath = context.Request.MapPath("/MyHandler.ashx");
                var username = context.Request.Form["username"];
                var id = context.Request.QueryString["id"];
                if (context.Request.UrlReferrer == null)
                {
                    return;
                }

                var referer = context.Request.UrlReferrer.ToString();

                // Imagine, if you will, a bunch of complex interesting
                // and fascinating logic here.
                context.Response.Write(physicalPath + ":" + username + ":" + id + ":" + referer);
            }
        }
    }

    internal class HttpSimulatorTester : HttpSimulator
    {
        public static string CallNormalizeSlashes(string s)
        {
            return NormalizeSlashes(s);
        }

        public static string CallStripTrailingBackSlashes(string s)
        {
            return StripTrailingBackSlashes(s);
        }

        public static HostingEnvironment CallGetEnvironment()
        {
            return GetHostingEnvironment();
        }
    }
}
