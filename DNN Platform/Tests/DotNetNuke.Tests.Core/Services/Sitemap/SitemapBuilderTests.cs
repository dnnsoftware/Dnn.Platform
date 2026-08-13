// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    using DotNetNuke.Services.Sitemap;
    using NUnit.Framework;

    [TestFixture]
    public class SitemapBuilderTests
    {
        private const string SitemapNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private const string XhtmlNamespace = "http://www.w3.org/1999/xhtml";

        [Test]
        public void WriteSitemap_WithoutAlternateUrls_WritesStandardFields()
        {
            var sitemapUrl = new SitemapUrl
            {
                Url = "https://example.com/en-us/page",
                LastModified = new DateTime(2026, 8, 4),
                ChangeFrequency = SitemapChangeFrequency.Daily,
                Priority = 0.5F,
            };

            var document = WriteSitemap(sitemapUrl);
            XNamespace sitemap = SitemapNamespace;
            XNamespace xhtml = XhtmlNamespace;
            var url = document.Root?.Element(sitemap + "url");

            Assert.Multiple(() =>
            {
                Assert.That(document.Root?.GetNamespaceOfPrefix("xhtml"), Is.EqualTo(xhtml));
                Assert.That(url, Is.Not.Null);
                Assert.That(url?.Element(sitemap + "loc")?.Value, Is.EqualTo("https://example.com/en-us/page"));
                Assert.That(url?.Element(sitemap + "lastmod")?.Value, Is.EqualTo("2026-08-04"));
                Assert.That(url?.Element(sitemap + "changefreq")?.Value, Is.EqualTo("daily"));
                Assert.That(url?.Element(sitemap + "priority")?.Value, Is.EqualTo("0.5"));
                Assert.That(url?.Elements(xhtml + "link"), Is.Empty);
            });
        }

        [Test]
        public void WriteSitemap_WithAlternateUrls_WritesHreflangLinks()
        {
            var sitemapUrl = new SitemapUrl
            {
                Url = "https://example.com/en-us/page",
                LastModified = new DateTime(2026, 8, 4),
                ChangeFrequency = SitemapChangeFrequency.Daily,
                Priority = 0.5F,
                AlternateUrls = new List<AlternateUrl>
                {
                    new AlternateUrl
                    {
                        Language = "en-US",
                        Url = "https://example.com/en-us/page",
                    },
                    new AlternateUrl
                    {
                        Language = "fr-FR",
                        Url = "https://example.fr/fr-fr/page",
                    },
                    new AlternateUrl
                    {
                        Language = "x-default",
                        Url = "https://example.com/en-us/page",
                    },
                },
            };

            var document = WriteSitemap(sitemapUrl);
            XNamespace sitemap = SitemapNamespace;
            XNamespace xhtml = XhtmlNamespace;
            var links = document.Root?
                .Element(sitemap + "url")?
                .Elements(xhtml + "link")
                .ToList();

            Assert.Multiple(() =>
            {
                Assert.That(links, Has.Count.EqualTo(3));
                Assert.That(links?[0].Attribute("rel")?.Value, Is.EqualTo("alternate"));
                Assert.That(links?[0].Attribute("hreflang")?.Value, Is.EqualTo("en-US"));
                Assert.That(links?[0].Attribute("href")?.Value, Is.EqualTo("https://example.com/en-us/page"));
                Assert.That(links?[1].Attribute("rel")?.Value, Is.EqualTo("alternate"));
                Assert.That(links?[1].Attribute("hreflang")?.Value, Is.EqualTo("fr-FR"));
                Assert.That(links?[1].Attribute("href")?.Value, Is.EqualTo("https://example.fr/fr-fr/page"));
                Assert.That(links?[2].Attribute("rel")?.Value, Is.EqualTo("alternate"));
                Assert.That(links?[2].Attribute("hreflang")?.Value, Is.EqualTo("x-default"));
                Assert.That(links?[2].Attribute("href")?.Value, Is.EqualTo("https://example.com/en-us/page"));
            });
        }

        private static XDocument WriteSitemap(SitemapUrl sitemapUrl)
        {
            var builder = (SitemapBuilder)FormatterServices.GetUninitializedObject(typeof(SitemapBuilder));
            var writeSitemap = typeof(SitemapBuilder).GetMethod("WriteSitemap", BindingFlags.Instance | BindingFlags.NonPublic);
            using var output = new StringWriter(CultureInfo.InvariantCulture);

            Assert.That(writeSitemap, Is.Not.Null);
            writeSitemap.Invoke(builder, new object[] { false, output, 0, new List<SitemapUrl> { sitemapUrl } });

            return XDocument.Parse(output.ToString());
        }
    }
}
