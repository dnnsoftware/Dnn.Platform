// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.RichTextEditor
{
    using System.Linq;

    using Dnn.RichTextEditorProvider.Components.PageLinks;
    using DotNetNuke.Entities.Tabs;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PageLinkServiceTests
    {
        private const int PortalId = 0;

        [TestCase("http://example.com/about-us", "/about-us")]
        [TestCase("https://example.com/child/Home/tabid/5/Default.aspx?language=de-DE", "/child/Home/tabid/5/Default.aspx?language=de-DE")]
        [TestCase("https://example.com/about#team", "/about#team")]
        [TestCase("/about-us", "/about-us")]
        [TestCase("~/about-us", "~/about-us")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void ToSiteRelativeUrl_StripsSchemeAndHost(string url, string expected)
        {
            Assert.That(PageLinkService.ToSiteRelativeUrl(url), Is.EqualTo(expected));
        }

        [Test]
        public void GetPageLinks_MapsPagesInTreeOrder_WithRelativeUrls()
        {
            var home = new TabInfo { TabID = 1, TabName = "Home", Level = 0, };
            var team = new TabInfo { TabID = 2, TabName = "Team", Level = 1, };
            var service = CreateService(home, team);

            var links = service.GetPageLinks(PortalId);

            Assert.Multiple(() =>
            {
                Assert.That(links.Select(l => l.Id), Is.EqualTo(new[] { 1, 2, }));
                Assert.That(links.Select(l => l.Name), Is.EqualTo(new[] { "Home", "Team", }));
                Assert.That(links.Select(l => l.Level), Is.EqualTo(new[] { 0, 1, }));
                Assert.That(links.Select(l => l.Url), Is.EqualTo(new[] { "/page-1", "/page-2", }));
                Assert.That(links.All(l => l.Selectable), Is.True);
            });
        }

        [Test]
        public void GetPageLinks_SkipsDeletedPages()
        {
            var service = CreateService(new TabInfo { TabID = 1, TabName = "Home", }, new TabInfo { TabID = 2, TabName = "Old", IsDeleted = true, });

            Assert.That(service.GetPageLinks(PortalId).Select(l => l.Id), Is.EqualTo(new[] { 1, }));
        }

        [Test]
        public void GetPageLinks_ListsDisabledPagesAsNotSelectable()
        {
            var service = CreateService(new TabInfo { TabID = 1, TabName = "Section", DisableLink = true, });

            Assert.That(service.GetPageLinks(PortalId).Single().Selectable, Is.False);
        }

        private static PageLinkService CreateService(params TabInfo[] pages)
        {
            var source = new Mock<IPageSource>();
            source.Setup(s => s.GetViewablePages(PortalId)).Returns(pages);
            source.Setup(s => s.GetUrl(It.IsAny<TabInfo>())).Returns<TabInfo>(page => "https://example.com/page-" + page.TabID);
            return new PageLinkService(source.Object);
        }
    }
}
