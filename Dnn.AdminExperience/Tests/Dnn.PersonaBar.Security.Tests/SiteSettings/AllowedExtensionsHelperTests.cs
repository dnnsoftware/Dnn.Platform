// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Tests.SiteSettings
{
    using Dnn.PersonaBar.SiteSettings.Components;
    using DotNetNuke.Common.Utilities;

    using NUnit.Framework;

    [TestFixture]
    public class AllowedExtensionsHelperTests
    {
        private const string HostPermitted = "jpg,jpeg,jpe,gif,bmp,png,svg,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,ico,avi,mpg,mpeg,mp3,wmv,mov,wav,mp4,webm,ogv,export";
        private const string HostDefault = "jpg,jpeg,jpe,gif,bmp,png,svg,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,ico,avi,mpg,mpeg,mp3,wmv,mov,wav,mp4,webm,ogv,export";
        private const string ImageTypes = "bmp,gif,ico,jpeg,jpg,jpe,png,svg";

        [TestCase("jpg,png,gif", "gif,jpg,png")]
        [TestCase("JPG, png ,gif", "gif,jpg,png")]
        [TestCase("jpg,png,png", "png,jpg")]
        [TestCase("", "")]
        public void AreEquivalent_IgnoresOrderCaseWhitespaceAndDuplicates(string first, string second)
        {
            Assert.That(AllowedExtensionsHelper.AreEquivalent(new FileExtensionWhitelist(first), new FileExtensionWhitelist(second)), Is.True);
        }

        [TestCase("jpg,png", "jpg,png,gif")]
        [TestCase("jpg", "")]
        public void AreEquivalent_DetectsDifferentLists(string first, string second)
        {
            Assert.That(AllowedExtensionsHelper.AreEquivalent(new FileExtensionWhitelist(first), new FileExtensionWhitelist(second)), Is.False);
        }

        [Test]
        public void AreEquivalent_HandlesNull()
        {
            Assert.Multiple(() =>
            {
                Assert.That(AllowedExtensionsHelper.AreEquivalent(null, null), Is.True);
                Assert.That(AllowedExtensionsHelper.AreEquivalent(new FileExtensionWhitelist("jpg"), null), Is.False);
            });
        }

        [Test]
        public void GetImageAllowList_IsRestrictedByHostPermittedList()
        {
            var images = AllowedExtensionsHelper.GetImageAllowList(ImageTypes, new FileExtensionWhitelist("jpg,png,pdf"));

            Assert.That(images.ToStorageString(), Is.EqualTo("jpg,png"));
        }

        [Test]
        public void GetImageAllowList_HandlesNullImageTypes()
        {
            var images = AllowedExtensionsHelper.GetImageAllowList(null, new FileExtensionWhitelist(HostPermitted));

            Assert.That(images.AllowedExtensions, Is.Empty);
        }

        [TestCase(HostDefault, AllowedExtensionsHelper.DefaultOption)]
        [TestCase("export,ogv,webm,mp4,wav,mov,wmv,mp3,mpeg,mpg,avi,ico,txt,pdf,pptx,ppt,xlsx,xls,docx,doc,svg,png,bmp,gif,jpe,jpeg,jpg", AllowedExtensionsHelper.DefaultOption)]
        [TestCase(ImageTypes, AllowedExtensionsHelper.OnlyImagesOption)]
        [TestCase("jpg,jpeg,jpe,gif,bmp,png,svg,ico", AllowedExtensionsHelper.OnlyImagesOption)]
        [TestCase("jpg,pdf", AllowedExtensionsHelper.CustomOption)]
        public void GetWhitelistOption_ComparesListsRegardlessOfOrder(string siteList, int expectedOption)
        {
            var imageList = AllowedExtensionsHelper.GetImageAllowList(ImageTypes, new FileExtensionWhitelist(HostPermitted));

            var option = AllowedExtensionsHelper.GetWhitelistOption(new FileExtensionWhitelist(siteList), new FileExtensionWhitelist(HostDefault), imageList);

            Assert.That(option, Is.EqualTo(expectedOption));
        }

        [Test]
        public void GetWhitelistOption_EmptyImageListIsNeverOnlyImages()
        {
            var option = AllowedExtensionsHelper.GetWhitelistOption(new FileExtensionWhitelist(string.Empty), new FileExtensionWhitelist(HostDefault), new FileExtensionWhitelist(string.Empty));

            Assert.That(option, Is.EqualTo(AllowedExtensionsHelper.CustomOption));
        }

        [Test]
        public void GetRejectedExtensions_ReturnsExtensionsNotPermittedByHost()
        {
            var requested = new FileExtensionWhitelist("jpg,webp,png,WEBP,exe");
            var effective = requested.RestrictBy(new FileExtensionWhitelist(HostPermitted));

            var rejected = AllowedExtensionsHelper.GetRejectedExtensions(requested, effective);

            Assert.That(rejected, Is.EqualTo(new[] { "webp", "exe" }));
        }

        [Test]
        public void GetRejectedExtensions_ReturnsEmptyWhenAllPermitted()
        {
            var requested = new FileExtensionWhitelist("jpg,png");
            var effective = requested.RestrictBy(new FileExtensionWhitelist(HostPermitted));

            Assert.That(AllowedExtensionsHelper.GetRejectedExtensions(requested, effective), Is.Empty);
        }
    }
}
