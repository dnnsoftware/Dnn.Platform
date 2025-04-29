// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace UnitTests.Subtext;

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
            Assert.That(HttpContext.Current.Session["Test"], Is.EqualTo("Success"), "Was not able to retrieve session variable.");
        }
    }

    ////[Test]
    public void CanGetSetApplicationVariables()
    {
        using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
        {
            HttpContext.Current.Application["Test"] = "Success";
            Assert.That(HttpContext.Current.Application["Test"], Is.EqualTo("Success"), "Was not able to retrieve application variable.");
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
            Assert.That(simulator.ResponseText, Is.EqualTo(expected), "The Expected Response is all wrong.");
        } // HttpContext.Current is set to null again.
    }

    ////[Test]
    public void CanDispose()
    {
        using (var simulator = new HttpSimulator())
        {
            simulator.SimulateRequest();
            Assert.That(HttpContext.Current, Is.Not.Null);
        }

        Assert.That(HttpContext.Current, Is.Null);
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
        Assert.That(simulator.ApplicationPath, Is.EqualTo(expectedAppPath));
        simulator.SimulateRequest(new Uri(url));
        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.ApplicationPath, Is.EqualTo(expectedAppPath));
            Assert.That(HttpRuntime.AppDomainAppVirtualPath, Is.EqualTo(expectedAppPath));
            Assert.That(HostingEnvironment.ApplicationVirtualPath, Is.EqualTo(expectedAppPath));
        });
    }

    /*//[RowTest]
    ////[Row("http://localhost/Test/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\default.aspx")]
    ////[Row("http://localhost/Test/Subtest/default.aspx", "/Test", @"c:\projects\test", @"c:\projects\test\", @"c:\projects\test\Subtest\default.aspx")]
    ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]
    ////[Row("http://localhost/test/default.aspx", "/", @"c:\inetpub\wwwroot", @"c:\inetpub\wwwroot\", @"c:\inetpub\wwwroot\test\default.aspx")]*/
    public void CanSetAppPhysicalPathCorrectly(string url, string appPath, string appPhysicalPath, string expectedPhysicalAppPath, string expectedPhysicalPath)
    {
        var simulator = new HttpSimulator(appPath, appPhysicalPath);
        Assert.That(simulator.PhysicalApplicationPath, Is.EqualTo(expectedPhysicalAppPath));
        simulator.SimulateRequest(new Uri(url), HttpVerb.GET);

        Assert.Multiple(() =>
        {
            Assert.That(simulator.PhysicalPath, Is.EqualTo(expectedPhysicalPath));
            Assert.That(HttpRuntime.AppDomainAppPath, Is.EqualTo(expectedPhysicalAppPath));
            Assert.That(HostingEnvironment.ApplicationPhysicalPath, Is.EqualTo(expectedPhysicalAppPath));
            Assert.That(HttpContext.Current.Request.PhysicalPath, Is.EqualTo(expectedPhysicalPath));
        });
    }

    ////[Test]
    public void CanGetQueryString()
    {
        var simulator = new HttpSimulator();
        simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=value1&param2=value2&param3=value3"));
        for (var i = 1; i <= 3; i++)
        {
            Assert.That(HttpContext.Current.Request.QueryString["param" + i], Is.EqualTo("value" + i), $"Could not find query string field 'param{i}'");
        }

        simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param1=new-value1&param2=new-value2&param3=new-value3&param4=new-value4"));
        for (var i = 1; i <= 4; i++)
        {
            Assert.That(HttpContext.Current.Request.QueryString["param" + i], Is.EqualTo("new-value" + i), $"Could not find query string field 'param{i}'");
        }

        simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?"));
        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo(string.Empty));
            Assert.That(HttpContext.Current.Request.QueryString, Is.Empty);
        });

        simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"));
        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo(string.Empty));
            Assert.That(HttpContext.Current.Request.QueryString, Is.Empty);
        });

        simulator.SimulateRequest(new Uri("http://localhost/Test.aspx?param-name"));
        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.QueryString.ToString(), Is.EqualTo("param-name"));
            Assert.That(HttpContext.Current.Request.QueryString, Has.Count.EqualTo(1));
        });
        Assert.That(HttpContext.Current.Request.QueryString["param-name"], Is.Null);
    }

    // //[Test]
    public void CanSimulateFormPost()
    {
        using (var simulator = new HttpSimulator())
        {
            var form = new NameValueCollection { { "Test1", "Value1" }, { "Test2", "Value2" } };
            simulator.SimulateRequest(new Uri("http://localhost/Test.aspx"), form);

            Assert.Multiple(() =>
            {
                Assert.That(HttpContext.Current.Request.Form["Test1"], Is.EqualTo("Value1"));
                Assert.That(HttpContext.Current.Request.Form["Test2"], Is.EqualTo("Value2"));
            });
        }

        using (var simulator = new HttpSimulator())
        {
            simulator.SetFormVariable("Test1", "Value1")
                .SetFormVariable("Test2", "Value2")
                .SimulateRequest(new Uri("http://localhost/Test.aspx"));

            Assert.Multiple(() =>
            {
                Assert.That(HttpContext.Current.Request.Form["Test1"], Is.EqualTo("Value1"));
                Assert.That(HttpContext.Current.Request.Form["Test2"], Is.EqualTo("Value2"));
            });
        }
    }

    // //[Test]
    public void CanGetResponse()
    {
        var simulator = new HttpSimulator();
        simulator.SimulateRequest();
        HttpContext.Current.Response.Write("Hello World!");
        HttpContext.Current.Response.Flush();
        Assert.That(simulator.ResponseText, Is.EqualTo("Hello World!"));
    }

    // //[Test]
    public void CanGetReferer()
    {
        var simulator = new HttpSimulator();
        simulator.SetReferer(new Uri("http://example.com/Blah.aspx")).SimulateRequest();
        Assert.That(HttpContext.Current.Request.UrlReferrer, Is.EqualTo(new Uri("http://example.com/Blah.aspx")));

        simulator = new HttpSimulator();
        simulator.SimulateRequest().SetReferer(new Uri("http://x.example.com/Blah.aspx"));
        Assert.That(HttpContext.Current.Request.UrlReferrer, Is.EqualTo(new Uri("http://x.example.com/Blah.aspx")));
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
        Assert.Multiple(() =>
        {
            Assert.That(simulator.ApplicationPath, Is.EqualTo(expectedAppPath));
            Assert.That(simulator.PhysicalApplicationPath, Is.EqualTo(expectedAppDomainAppPath));
        });
    }

    // [RowTest]
    ////[Row("http://localhost/AppPath/default.aspx", "/AppPath", "/AppPath/default.aspx")]
    ////[Row("http://localhost/AppPath/default.aspx", "/", "/AppPath/default.aspx")]
    public void CanGetLocalPathCorrectly(string url, string appPath, string expectedLocalPath)
    {
        var simulator = new HttpSimulator(appPath, @"c:\inetpub\wwwroot\AppPath\");
        simulator.SimulateRequest(new Uri(url));
        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.Path, Is.EqualTo(expectedLocalPath));
            Assert.That(HttpContext.Current.Request.Url.LocalPath, Is.EqualTo(expectedLocalPath));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(HttpContext.Current.Request.Url.Host, Is.EqualTo(expectedHost));
            Assert.That(HttpContext.Current.Request.Url.Port, Is.EqualTo(expectedPort));
            Assert.That(HttpContext.Current.Request.ApplicationPath, Is.EqualTo(expectedAppPath));
            Assert.That(HttpContext.Current.Request.PhysicalApplicationPath, Is.EqualTo(expectedPhysicalPath));
            Assert.That(HttpContext.Current.Request.Url.LocalPath, Is.EqualTo(expectedLocalPath));
        });
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
        Assert.That(vpath, Is.Not.Null);

        var environment = HttpSimulatorTester.CallGetEnvironment();

        var vpathString = ReflectionHelper.InvokeProperty<string>(vpath, "VirtualPathString");
        var appVirtPath = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_appVirtualPath", environment);
        Assert.That(appVirtPath, Is.Not.Null);
        Console.WriteLine("VPATH: " + vpath);
        Console.WriteLine("App-VPATH: " + appVirtPath);

        Console.WriteLine("vpath.VirtualPathString == '{0}'", vpathString);

        var mapping = ReflectionHelper.InvokeNonPublicMethod<string>(typeof(HostingEnvironment), "GetVirtualPathToFileMapping", vpath);
        Console.WriteLine("GetVirtualPathToFileMapping: --->{0}<---", mapping ?? "{NULL}");

        var o = ReflectionHelper.GetPrivateInstanceFieldValue<object>("_configMapPath", environment);
        Console.WriteLine("_configMapPath: {0}", o ?? "{null}");

        var mappedPath = ReflectionHelper.InvokeNonPublicMethod<string>(environment, "MapPathActual", vpath, false);
        Console.WriteLine("MAPPED: " + mappedPath);
        Assert.That(HttpContext.Current.Request.MapPath(virtualPath), Is.EqualTo(expectedMapPath));
    }

    // [Test]
    public void CanInstantiateVirtualPath()
    {
        var virtualPathType = Type.GetType("System.Web.VirtualPath, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true);
        var constructor = virtualPathType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);
        Assert.That(constructor, Is.Not.Null);
    }

    // [Test]
    public void CanGetHostingEnvironment()
    {
        var environment = HttpSimulatorTester.CallGetEnvironment();
        Assert.That(environment, Is.Not.Null);
        environment = HttpSimulatorTester.CallGetEnvironment();
        Assert.That(environment, Is.Not.Null);
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
        Assert.That(HttpSimulatorTester.CallNormalizeSlashes(s), Is.EqualTo(expected));
    }

    // [Test]
    public void CanStripTrailing()
    {
        Assert.That(HttpSimulatorTester.CallStripTrailingBackSlashes(@"c:\blah\blah2\"), Is.EqualTo(@"c:\blah\blah2"));
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
