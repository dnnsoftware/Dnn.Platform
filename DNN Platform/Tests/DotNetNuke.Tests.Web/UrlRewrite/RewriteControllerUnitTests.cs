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
        public void RemoveLanguageCodeFromAbsoluteURL(string absoluteURL, string languageCode, string expectedURL)
        {
            // Act
            var result = RewriteController.GetUrlWithLanguageCode(absoluteURL, languageCode);

            // Assert
            Assert.AreEqual(result, expectedURL);
        }
    }
}
