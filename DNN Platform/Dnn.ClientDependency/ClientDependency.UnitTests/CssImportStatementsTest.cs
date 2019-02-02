using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{
    [TestFixture]
    public class CssImportStatementsTest
    {
        [Test]
        public void Ensure_Non_Temp_Search_Strings_Are_Resolved_From_Stream()
        {
            var css = @"@font-face{
font-family:'Open Sans';
font-style:normal;
font-weight:400;
src:url('../fonts/opensans/OpenSans-Regular-webfont.eot');
src:local('Open Sans'),
	local('OpenSans'),
	url('../fonts/opensans/OpenSans-Regular-webfont.eot?#iefix') format('embedded-opentype'),
	url('../fonts/opensans/OpenSans-Regular-webfont.ttf') format('truetype'),
	url('../fonts/opensans/OpenSans-Regular-webfont.svg#open_sansregular') format('svg')}";

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(css);
                writer.Flush();

                string externalImports;
                IEnumerable<string> importPaths;
                CssHelper.ParseImportStatements(ms, out importPaths);
                
                Assert.AreEqual("", externalImports);
            }            
        }

        [Test]
        public void Retain_External_Imports()
        {
            var cssWithImport = @"@import url(""//fonts.googleapis.com/css?subset=latin,cyrillic-ext,latin-ext,cyrillic&family=Open+Sans+Condensed:300|Open+Sans:400,600,400italic,600italic|Merriweather:400,300,300italic,400italic,700,700italic|Roboto+Slab:400,300"");
@import url(""//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css"");";

            IEnumerable<string> importPaths;
            var output = CssHelper.ParseImportStatements(cssWithImport, out importPaths);

            Assert.AreEqual(cssWithImport, output);
        }

        [Test]
        public void Retain_External_Imports_From_Stream()
        {
            var cssWithImport = @"@import url(""//fonts.googleapis.com/css?subset=latin,cyrillic-ext,latin-ext,cyrillic&family=Open+Sans+Condensed:300|Open+Sans:400,600,400italic,600italic|Merriweather:400,300,300italic,400italic,700,700italic|Roboto+Slab:400,300"");
@import url(""//netdna.bootstrapcdn.com/font-awesome/4.0.3/css/font-awesome.css"");";

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(cssWithImport);
                writer.Flush();

                string externalImports;
                IEnumerable<string> importPaths;
                var position = CssHelper.ParseImportStatements(ms, out importPaths);

                Assert.AreEqual(ms.Length, position);
                Assert.AreEqual(cssWithImport, externalImports);
            }            
        }

        [Test]
        public void Can_Parse_Import_Statements()
        {
            var css = @"@import url('/css/typography.css');
@import url('/css/layout.css');
@import url('http://mysite/css/color.css');
@import url(/css/blah.css);

body { color: black; }
div {display: block;}";

            IEnumerable<string> importPaths;
            var output = CssHelper.ParseImportStatements(css, out importPaths);

            Assert.AreEqual(@"@import url('http://mysite/css/color.css');


body { color: black; }
div {display: block;}", output);

            Assert.AreEqual(3, importPaths.Count());
            Assert.AreEqual("/css/typography.css", importPaths.ElementAt(0));
            Assert.AreEqual("/css/layout.css", importPaths.ElementAt(1));
            //Assert.AreEqual("http://mysite/css/color.css", importPaths.ElementAt(2));
            Assert.AreEqual("/css/blah.css", importPaths.ElementAt(2));
        }

        [Test]
        public void Can_Parse_Import_Statements_From_Stream_2()
        {
            var css = @"@-ms-viewport {
  width: device-width;
}
.visible-xs,
tr.visible-xs,
th.visible-xs,
td.visible-xs {
  display: none !important;
}
@media (max-width: 767px) {
  .visible-xs {
    display: block !important;
  }
  table.visible-xs {
    display: table;
  }
  tr.visible-xs {
    display: table-row !important;
  }
  th.visible-xs,
  td.visible-xs {
    display: table-cell !important;
  }
}
@media (min-width: 768px) and (max-width: 991px) {
  .visible-xs.visible-sm {
    display: block !important;
  }
  table.visible-xs.visible-sm {
    display: table;
  }
  tr.visible-xs.visible-sm {
    display: table-row !important;
  }
  th.visible-xs.visible-sm,
  td.visible-xs.visible-sm {
    display: table-cell !important;
  }
}
";
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(css);
                writer.Flush();

                IEnumerable<string> importPaths;
                string externalImports;
                var position = CssHelper.ParseImportStatements(ms, out importPaths);

                Assert.AreEqual(string.Empty, externalImports);

                //Assert.AreEqual(10, importPaths.Count());
                //Assert.AreEqual("/css/typography.css", importPaths.ElementAt(0));
                //Assert.AreEqual("/css/layout.css", importPaths.ElementAt(1));
                ////Assert.AreEqual("http://mysite/css/color.css", importPaths.ElementAt(2));
                //Assert.AreEqual("/css/blah.css", importPaths.ElementAt(2));
            }



        }

        [Test]
        public void Can_Parse_Import_Statements_From_Stream()
        {
            var css = @"@import url('/css/typography.css');
@import url('/css/layout.css');
@import url('http://mysite/css/color.css');
@import url(/css/blah.css);

body { color: black; }
div {display: block;}";

            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.Write(css);
                writer.Flush();

                IEnumerable<string> importPaths;
                string externalImports;
                var position = CssHelper.ParseImportStatements(ms, out importPaths);

                Assert.AreEqual(@"@import url('http://mysite/css/color.css');", externalImports);

                Assert.AreEqual(3, importPaths.Count());
                Assert.AreEqual("/css/typography.css", importPaths.ElementAt(0));
                Assert.AreEqual("/css/layout.css", importPaths.ElementAt(1));
                //Assert.AreEqual("http://mysite/css/color.css", importPaths.ElementAt(2));
                Assert.AreEqual("/css/blah.css", importPaths.ElementAt(2));

            }

            
        }
    }
}