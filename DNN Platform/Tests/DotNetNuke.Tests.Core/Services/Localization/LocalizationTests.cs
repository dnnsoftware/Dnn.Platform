#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Web;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Localization.Internal;
using DotNetNuke.Tests.Utilities;
using Moq;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Services.Localization
{
    [TestFixture]
    public class LocalizationTests
    {
        private Mock<HttpContextBase> _mockHttpContext;
        private HttpContextBase _httpContext;
        private readonly string[] _standardCultureCodes = new[] { "en-US", "de-DE", "fr-CA", "Lt-sr-SP", "kok-IN"};

        [SetUp]
        public void Setup()
        {
            _mockHttpContext = HttpContextHelper.RegisterMockHttpContext();
            _httpContext = _mockHttpContext.Object;
        }

        [Test]
        public void NoMatchReturnsFallback()
        {
            const string fallback = "fallback";
            //Arrange
            
            //Act
            var ret  = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(new string[0], fallback);

            //Assert
            Assert.AreEqual(ret, fallback);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CultureCodesCannotBeNull()
        {
            //Arrange
            

            //Act
            TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(null);

            //Assert
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void FallBackCannotBeNull()
        {
            //Arrange
            

            //Act
            TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(new string[0], null);

            //Assert
        }

        [Test]
        public void PerfectMatchPossible()
        {
            //Arrange
            _mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] {"de-DE"});

            //Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(_standardCultureCodes);

            //Assert
            Assert.AreEqual(ret, "de-DE");
        }

        [Test]
        public void QParamsAreIgnored()
        {
            //Arrange
            _mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "de-DE;q=0.8" });

            //Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(_standardCultureCodes);

            //Assert
            Assert.AreEqual(ret, "de-DE");
        }

        [Test]
        public void MatchOnOnlyLanguage()
        {
            //Arrange
            _mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            //Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(_standardCultureCodes);

            //Assert
            Assert.AreEqual(ret, "fr-CA");
        }

        [Test]
        public void PerfectMatchPreferredToFirstMatch()
        {
            //Arrange
            _mockHttpContext.Setup(x => x.Request.UserLanguages).Returns(new[] { "fr-FR" });

            //Act
            var ret = TestableLocalization.Instance.BestCultureCodeBasedOnBrowserLanguages(new[] {"fr-CA", "fr-FR"});

            //Assert
            Assert.AreEqual(ret, "fr-FR");
        }


        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseLocaleFromResxFileName(string fileName)
        {
            foreach (var standardCultureCode in _standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
                Assert.AreEqual(f.GetLocaleCodeFromFileName(), standardCultureCode);               
            }
        }

        [Test]
        [TestCase("My/Path/To/File with.locale-extension")]
        [TestCase("My\\Path\\To\\File with.locale-extension")]
        public void ParseFileNameFromResxFile(string fileName)
        {
            foreach (var standardCultureCode in _standardCultureCodes)
            {
                var f = fileName + "." + standardCultureCode + ".resx";
                Assert.AreEqual(f.GetFileNameFromLocalizedResxFile(), fileName);
            }
        }
    }
}