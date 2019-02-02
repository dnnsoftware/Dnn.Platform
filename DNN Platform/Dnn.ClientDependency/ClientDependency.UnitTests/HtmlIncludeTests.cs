using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core;
using ClientDependency.Core.Controls;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class HtmlIncludeTests
    {
        [Test]
        public void Css_Parser()
        {
            var cssTags = @"

<link href='/css/mycss1.css'  type='text/css'></link>
    <link href='/css/mycss2.css'  type='text/css' media='print,screen' rel='stylesheet'></link>
<script href='/css/mycss2.css'  type='text/javascript' media='print,screen' rel='stylesheet'></script>
        <link href='/css/mycss3.css' type='text/css' media='print' ></link>
";
            
            var includer = new HtmlInclude();
            var result = includer.GetIncludes(cssTags, ClientDependencyType.Css);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("/css/mycss1.css", result.ElementAt(0).FilePath);
            Assert.AreEqual("/css/mycss2.css", result.ElementAt(1).FilePath);
            Assert.AreEqual("/css/mycss3.css", result.ElementAt(2).FilePath);
            Assert.AreEqual(2, result.ElementAt(1).HtmlAttributes.Count);
            Assert.AreEqual(1, result.ElementAt(2).HtmlAttributes.Count);
        }

        [Test]
        public void Js_Parser()
        {
            var jsTags = @"

<script src='/js/myjs1.js' type='text/javascript'></script>
<link href='/css/mycss1.css'  type='text/css' media='print,screen' rel='stylesheet'></link>
    <script src='/js/myjs2.js'  type='text/javascript' async='true' defer='true'></script>

        <script src='/js/myjs3.js' type='text/javascript' charset='utf8' ></script>
";

            var includer = new HtmlInclude();
            var result = includer.GetIncludes(jsTags, ClientDependencyType.Javascript);

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual("/js/myjs1.js", result.ElementAt(0).FilePath);
            Assert.AreEqual("/js/myjs2.js", result.ElementAt(1).FilePath);
            Assert.AreEqual("/js/myjs3.js", result.ElementAt(2).FilePath);
            Assert.AreEqual(2, result.ElementAt(1).HtmlAttributes.Count);
            Assert.AreEqual(1, result.ElementAt(2).HtmlAttributes.Count);
        }

    }
}
