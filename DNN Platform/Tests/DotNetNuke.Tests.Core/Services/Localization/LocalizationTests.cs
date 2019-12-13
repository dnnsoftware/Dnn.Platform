// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
