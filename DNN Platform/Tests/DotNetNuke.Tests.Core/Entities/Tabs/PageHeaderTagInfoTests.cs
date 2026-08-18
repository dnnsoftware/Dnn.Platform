// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Entities.Tabs
{
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Tests.Utilities.Fakes;

    using NUnit.Framework;

    [TestFixture]
    public class PageHeaderTagInfoTests
    {
        [SetUp]
        public void SetUp()
        {
            // Normalize sanitizes tag names through PortalSecurity, which resolves its
            // instance from the dependency provider on first access.
            FakeServiceProvider.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            FakeServiceProvider.Reset();
        }

        [Test]
        public void FromSettings_Returns_Only_PageHeaderTag_Settings_Ordered_By_Name()
        {
            // Arrange
            var settings = new Hashtable
            {
                { "PageHeaderTag_OgTitle", "<meta property=\"og:title\" content=\"Test\" />" },
                { "PageHeaderTag_Default", "<meta name=\"description\" content=\"Test\" />" },
                { "PageHeadText", "<meta name=\"legacy\" content=\"ignored\" />" },
                { "SkinSrc", "[G]Skins/default" },
            };

            // Act
            var items = PageHeaderTagInfo.FromSettings(settings);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items.Count, Is.EqualTo(2), "Only the PageHeaderTag_* settings should be returned");
                Assert.That(items[0].Name, Is.EqualTo("Default"), "Items should be ordered by name");
                Assert.That(items[1].Name, Is.EqualTo("OgTitle"), "Items should be ordered by name");
                Assert.That(items[0].Content, Is.EqualTo("<meta name=\"description\" content=\"Test\" />"));
                Assert.That(items[1].Content, Is.EqualTo("<meta property=\"og:title\" content=\"Test\" />"));
            }
        }

        [Test]
        public void FromSettings_Skips_Empty_Names_And_Returns_Empty_For_Null_Settings()
        {
            // Arrange
            var settings = new Hashtable
            {
                { "PageHeaderTag_", "content without a name" },
                { "AnotherKey", "another value" },
            };

            // Act
            var items = PageHeaderTagInfo.FromSettings(settings);
            var itemsFromNull = PageHeaderTagInfo.FromSettings(null);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(items, Is.Empty, "An entry without a name should be skipped");
                Assert.That(itemsFromNull, Is.Empty, "Null settings should produce an empty list");
            }
        }

        [Test]
        public void Render_Joins_Non_Empty_Contents_With_NewLine()
        {
            // Arrange
            var items = new List<PageHeaderTagInfo>
            {
                new PageHeaderTagInfo { Name = "Default", Content = "<meta name=\"description\" content=\"Test\" />" },
                new PageHeaderTagInfo { Name = "Empty", Content = "   " },
                new PageHeaderTagInfo { Name = "OgTitle", Content = "<meta property=\"og:title\" content=\"Test\" />" },
            };

            // Act
            var rendered = PageHeaderTagInfo.Render(items);

            // Assert
            Assert.That(rendered, Is.EqualTo("<meta name=\"description\" content=\"Test\" />" + System.Environment.NewLine + "<meta property=\"og:title\" content=\"Test\" />"), "Blank contents should be skipped");
        }

        [Test]
        public void Render_Returns_Empty_For_Null_Items()
        {
            // Act
            var rendered = PageHeaderTagInfo.Render(null);

            // Assert
            Assert.That(rendered, Is.Empty);
        }

        [Test]
        public void Normalize_Trims_Names_And_Removes_Empty_Items()
        {
            // Arrange
            var items = new List<PageHeaderTagInfo>
            {
                new PageHeaderTagInfo { Name = "  Default  ", Content = "<meta name=\"description\" content=\"Test\" />" },
                new PageHeaderTagInfo { Name = "  ", Content = "<meta name=\"skipped\" content=\"no name\" />" },
                new PageHeaderTagInfo { Name = "Blank", Content = "   " },
                null,
            };

            // Act
            var normalized = PageHeaderTagInfo.Normalize(items);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalized.Count, Is.EqualTo(1), "Items without a name or content should be removed");
                Assert.That(normalized[0].Name, Is.EqualTo("Default"), "The name should be trimmed");
            }
        }

        [Test]
        public void Normalize_Keeps_Last_Item_For_Duplicate_Names_Ignoring_Case()
        {
            // Arrange
            var items = new List<PageHeaderTagInfo>
            {
                new PageHeaderTagInfo { Name = "Default", Content = "<meta name=\"first\" content=\"1\" />" },
                new PageHeaderTagInfo { Name = "default", Content = "<meta name=\"last\" content=\"2\" />" },
            };

            // Act
            var normalized = PageHeaderTagInfo.Normalize(items);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalized.Count, Is.EqualTo(1), "Duplicate names should collapse to a single item");
                Assert.That(normalized[0].Content, Is.EqualTo("<meta name=\"last\" content=\"2\" />"), "The last duplicate should win");
            }
        }

        [Test]
        public void Normalize_Encodes_Markup_In_Name()
        {
            // Arrange - a tag name must never contain markup, it is used as a setting key
            // and rendered in the management UI.
            var items = new List<PageHeaderTagInfo>
            {
                new PageHeaderTagInfo { Name = "<script>alert('xss')</script>", Content = "<meta name=\"description\" content=\"Test\" />" },
                new PageHeaderTagInfo { Name = "OgTitle", Content = "<meta property=\"og:title\" content=\"Test\" />" },
            };

            // Act
            var normalized = PageHeaderTagInfo.Normalize(items);

            // Assert
            using (Assert.EnterMultipleScope())
            {
                Assert.That(normalized.Count, Is.EqualTo(2));
                Assert.That(normalized[0].Name, Does.Not.Contain("<"), "The name must not contain raw markup");
                Assert.That(normalized[0].Name, Does.Not.Contain(">"), "The name must not contain raw markup");
                Assert.That(normalized[1].Name, Is.EqualTo("OgTitle"), "Names without markup should stay unchanged");
            }
        }

        [Test]
        public void Normalize_Returns_Empty_List_For_Null_Items()
        {
            // Act
            var normalized = PageHeaderTagInfo.Normalize(null);

            // Assert
            Assert.That(normalized, Is.Not.Null.And.Empty);
        }
    }
}
