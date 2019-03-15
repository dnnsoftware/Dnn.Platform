using DotNetNuke.Entities.Urls;
using NUnit.Framework;

namespace DotNetNuke.Tests.Web.UrlRewrite
{
    [TestFixture]
    class RewriteControllerUnitTests
    {
        [TestCase("http://www.dnn.com/en-us/aboutus", "en-us", "http://www.dnn.com/aboutus")]
        [TestCase("http://www.dnn.com/en-Us/aboutus", "en-Us", "http://www.dnn.com/aboutus")]
        [TestCase("http://www.dnn.com/aboutus", "en-Us", "http://www.dnn.com/aboutus")]
        [TestCase("https://dnn.com/en", "en-Us", "https://dnn.com/en")]
        [TestCase("https://www.dnn.com/en", "en-Us", "https://www.dnn.com/en")]
        [TestCase("http://en.dnn.com/", "en-Us", "http://en.dnn.com/")]
        [TestCase("http://www.a.b.c.com", "en-Us", "http://www.a.b.c.com")]
        public void RemoveLanguageCodeFromAbsoluteURL(string absoluteURL, string languageCode, string expectedURL)
        {
            // Act
            var result = RewriteController.GetUrlWithoutLanguageCode(absoluteURL, languageCode);

            // Assert
            Assert.AreEqual(result, expectedURL);
        }
    }
}
