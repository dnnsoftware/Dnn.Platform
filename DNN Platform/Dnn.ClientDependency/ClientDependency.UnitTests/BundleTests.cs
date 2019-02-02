using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class BundleTests
    {

        [TearDown]
        public void TearDown()
        {
            BundleManager.ClearBundles();
        }

        [Test]
        public void Can_Add_And_Retreive_Css_Bundle()
        {
            var a1 = new[]
                {
                    new CssFile("/css/test1.css"),
                    new CssFile("/css/test2.css"),
                    new CssFile("/css/test3.css")
                };
            var a2 = new[]
                {
                    new CssFile("/css/test4.css"),
                    new CssFile("/css/test5.css"),
                    new CssFile("/css/test6.css")
                };
            BundleManager.CreateCssBundle("Css1", a1);
            BundleManager.CreateCssBundle("Css2", a2);

            Assert.AreEqual(BundleManager.GetCssBundles().Count, 2);
        }

        [Test]
        public void Can_Add_And_Retreive_Js_Bundle()
        {
            var a1 = new[]
                {
                    new JavascriptFile("/css/test1.css"),
                    new JavascriptFile("/css/test2.css"),
                    new JavascriptFile("/css/test3.css")
                };
            var a2 = new[]
                {
                    new JavascriptFile("/css/test4.css"),
                    new JavascriptFile("/css/test5.css"),
                    new JavascriptFile("/css/test6.css")
                };
            var a3 = new[]
                {
                    new JavascriptFile("/css/test7.css"),
                    new JavascriptFile("/css/test8.css"),
                    new JavascriptFile("/css/test9.css")
                };
            BundleManager.CreateJsBundle("Js1", a1);
            BundleManager.CreateJsBundle("Js2", a2);
            BundleManager.CreateJsBundle("Js3", a2);

            Assert.AreEqual(BundleManager.GetJsBundles().Count, 3);
        }

        [Test]
        public void Can_Update_Css_Bundle()
        {
            var a1 = new[]
                {
                    new CssFile("/css/test1.css"),
                    new CssFile("/css/test2.css"),
                    new CssFile("/css/test3.css")
                };
            var a2 = new[]
                {
                    new CssFile("/css/test4.css"),
                    new CssFile("/css/test5.css"),
                    new CssFile("/css/test6.css")
                };
            BundleManager.CreateCssBundle("Css1", a1);
            //this will replace the previous one
            BundleManager.CreateCssBundle("Css1", a2);

            Assert.AreEqual(BundleManager.GetCssBundles().Count, 1);
        }

        [Test]
        public void Ensure_Order_Correct()
        {
            var a1 = new List<CssFile>
                {
                    new CssFile("/css/test1.css"),
                    new CssFile("/css/test3.css"),
                    new CssFile("/css/test2.css"),
                    new CssFile("/css/test4.css"){ Priority = 1},
                    new CssFile("/css/test5.css")
                };            
            BundleManager.CreateCssBundle("Css1", a1.ToArray());

            var bundle = BundleManager.GetCssBundles().First();

            Assert.AreEqual(a1[3], (CssFile)bundle.Value.First());

            var currentIndex = -1;
            //iterate except for the one with the priority
            foreach (CssFile b in bundle.Value.Except(new[] {(CssFile) a1[3]}))
            {
                var newIndex = a1.IndexOf(b);
                Assert.Greater(newIndex, currentIndex);
                currentIndex = newIndex;
            }
        }

    }
}
