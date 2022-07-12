// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Modules.Html
{
    using DotNetNuke.Modules.Html;
    using NUnit.Framework;

    [TestFixture]
    public class HtmlTextControllerTests
    {
        [Test]
        public void ManageRelativePaths_DoesNotChangePlainHtml()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<p>Hello</p>",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("<p>Hello</p>", actual);
        }

        [Test]
        public void ManageRelativePaths_AdjustsRelativeImgSrc()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"image.jpg\"/>",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("<img src=\"/portals/0/image.jpg\"/>", actual);
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithCorrectPathCaseInsensitive()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"/Portals/0/image.jpg\"/>",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("<img src=\"/portals/0/image.jpg\"/>", actual);
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrl()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"https://example.com/image.jpg\"/>",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("<img src=\"https://example.com/image.jpg\"/>", actual);
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithAbsoluteUrlInContent()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"https://example.com/image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("src=\"https://example.com/image.jpg\" is how you indicate a URL", actual);
        }

        [Test]
        public void ManageRelativePaths_DoesAdjustImgSrcWithRelativeUrlInContent()
        {
            // TODO: should we attempt to avoid making this change?
            var actual = HtmlTextController.ManageRelativePaths(
                "src=\"image.jpg\" is how you indicate a URL",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("src=\"/portals/0/image.jpg\" is how you indicate a URL", actual);
        }

        [Test]
        public void ManageRelativePaths_DoesNotAdjustImgSrcWithDataUrl()
        {
            var actual = HtmlTextController.ManageRelativePaths(
                "<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>",
                "/portals/0/",
                "src",
                0);
            Assert.AreEqual("<img src=\"data:image/gif;base64,R0lGODlhEAAQAMQAAORHHOVSKudfOulrSOp3WOyDZu6QdvCchPGolfO0o/XBs/fNwfjZ0frl3/zy7////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACH5BAkAABAALAAAAAAQABAAAAVVICSOZGlCQAosJ6mu7fiyZeKqNKToQGDsM8hBADgUXoGAiqhSvp5QAnQKGIgUhwFUYLCVDFCrKUE1lBavAViFIDlTImbKC5Gm2hB0SlBCBMQiB0UjIQA7\"/>", actual);
        }
    }
}
