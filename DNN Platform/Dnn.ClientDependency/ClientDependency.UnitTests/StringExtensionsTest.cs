using ClientDependency.Core;
using NUnit.Framework;

namespace ClientDependency.UnitTests
{

    [TestFixture]
    public class StringExtensionsTest
    {
        [Test]
        public void Non_Alphanumeric()
        {
            var name = "Hello@#b[sdfsd/sde=+1!";
            var safe = name.ReplaceNonAlphanumericChars('-');
            foreach (var c in safe)
            {
                Assert.IsTrue(char.IsLetterOrDigit(c) || c == '-');
            }
        }

        [Test]
        public void Decode_From_64_Url()
        {
            var files = "/VirtualFolderTest/Pages/relative.css;/VirtualFolderTest/Css/Site.css;/VirtualFolderTest/Css/ColorScheme.css;/VirtualFolderTest/Css/Controls.css;/VirtualFolderTest/Css/CustomControl.css;/VirtualFolderTest/Css/Content.css;";
            var encodedFiles = files.EncodeTo64Url();

            var decoded = encodedFiles.DecodeFrom64Url();

            Assert.AreEqual(files, decoded);
        }
    }
}