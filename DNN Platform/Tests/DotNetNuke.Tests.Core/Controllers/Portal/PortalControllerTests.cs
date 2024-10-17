// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Portal
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Tests.Utilities.Fakes;

    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class PortalControllerTests
    {
        private const string HostMapPath = @"C:\path";
        private const string DefaultName = "Default";
        private const string DefaultDeName = "Rückstellungs-Web site";
        private const string DefaultDeDescription = "A new german description";
        private const string StaticName = "Static";

        private const string StaticDescription = "An description from a template file";
        private const string AlternateName = "Alternate";

        private const string AlternateDeName = "Alternate in German";
        private const string ResourceName = "Resource";
        private static readonly string DefaultPath = MakePath(DefaultName);
        private static readonly string DefaultDePath = MakePath(DefaultName, "de-DE");

        private static readonly Dictionary<string, string> DefaultExpectationsDe = new Dictionary<string, string>
                                                                                    {
                                                                                        { "Name", DefaultDeName },
                                                                                        { "TemplateFilePath", DefaultPath },
                                                                                        { "LanguageFilePath", DefaultDePath },
                                                                                        { "CultureCode", "de-DE" },
                                                                                        { "Description", DefaultDeDescription },
                                                                                    };

        private static readonly string DefaultUsPath = MakePath(DefaultName, "en-US");

        private static readonly Dictionary<string, string> DefaultExpectationsUs = new Dictionary<string, string>
                                                                                    {
                                                                                        { "Name", DefaultName },
                                                                                        { "TemplateFilePath", DefaultPath },
                                                                                        { "LanguageFilePath", DefaultUsPath },
                                                                                        { "CultureCode", "en-US" },
                                                                                    };

        private static readonly string CultureCode = Thread.CurrentThread.CurrentCulture.Name;
        private static readonly string StaticPath = MakePath(StaticName);

        private static readonly Dictionary<string, string> StaticExpectations = new Dictionary<string, string>
                                                                                    {
                                                                                        { "Name", StaticName },
                                                                                        { "TemplateFilePath", StaticPath },
                                                                                        { "Description", StaticDescription },
                                                                                        { "CultureCode", "en-US" },
                                                                                    };

        private static readonly string AlternatePath = MakePath(AlternateName);
        private static readonly string AlternateDePath = MakePath(AlternateName, "de-DE");

        private static readonly Dictionary<string, string> AlternateExpectationsDe = new Dictionary<string, string>
                                                                                    {
                                                                                        { "Name", AlternateDeName },
                                                                                        { "TemplateFilePath", AlternatePath },
                                                                                        { "LanguageFilePath", AlternateDePath },
                                                                                        { "CultureCode", "de-DE" },
                                                                                    };

        private static readonly string ResourcePath = MakePath(ResourceName);
        private static readonly string ResourceFilePath = ResourcePath + ".resources";

        private static readonly Dictionary<string, string> ResourceExpectations = new Dictionary<string, string>
                                                                                    {
                                                                                        { "Name", ResourceName },
                                                                                        { "TemplateFilePath", ResourcePath },
                                                                                        { "ResourceFilePath", ResourceFilePath },
                                                                                        { "CultureCode", "en-US" },
                                                                                    };

        private Mock<IPortalTemplateIO> mockPortalTemplateIO;
        private FakeServiceProvider serviceProvider;

        [SetUp]
        public void SetUp()
        {
            this.mockPortalTemplateIO = new Mock<IPortalTemplateIO>();
            PortalTemplateIO.SetTestableInstance(this.mockPortalTemplateIO.Object);

            this.serviceProvider = FakeServiceProvider.Setup(
                services =>
                {
                    services.AddSingleton(this.mockPortalTemplateIO.Object);
                });
        }

        [TearDown]
        public void TearDown()
        {
            PortalTemplateIO.ClearInstance();
        }

        [Test]
        public void NoTemplatesReturnsEmptyList()
        {
            // Arrange

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Is.Empty);
        }

        [Test]
        public void LanguageFileWithoutATemplateIsIgnored()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath));

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Is.Empty);
        }

        [Test]
        public void TemplatesWithNoLanguageFilesAreLoaded()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(StaticPath));
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(StaticPath)).Returns(this.CreateTemplateFileReader(StaticDescription));

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Has.Count.EqualTo(1));
            AssertTemplateInfo(StaticExpectations, templates[0]);
        }

        [Test]
        public void TemplateWith2Languages()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(DefaultPath));
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath, DefaultUsPath));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "de-DE")).Returns(DefaultDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultDePath)).Returns(this.CreateLanguageFileReader(DefaultDeName, DefaultDeDescription));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "en-US")).Returns(DefaultUsPath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultUsPath)).Returns(this.CreateLanguageFileReader(DefaultName));

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Has.Count.EqualTo(2));
            AssertTemplateInfo(DefaultExpectationsDe, templates[0]);
            AssertTemplateInfo(DefaultExpectationsUs, templates[1]);
        }

        [Test]
        public void TwoTemplatesAssortedLanguages()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(DefaultPath, AlternatePath));
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath, DefaultUsPath, AlternateDePath));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "de-DE")).Returns(DefaultDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultDePath)).Returns(this.CreateLanguageFileReader(DefaultDeName, DefaultDeDescription));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "en-US")).Returns(DefaultUsPath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultUsPath)).Returns(this.CreateLanguageFileReader(DefaultName));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(AlternatePath, "de-DE")).Returns(AlternateDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(AlternateDePath)).Returns(this.CreateLanguageFileReader(AlternateDeName));

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Has.Count.EqualTo(3));
            AssertTemplateInfo(DefaultExpectationsDe, templates[0]);
            AssertTemplateInfo(DefaultExpectationsUs, templates[1]);
            AssertTemplateInfo(AlternateExpectationsDe, templates[2]);
        }

        [Test]
        public void ResourceFileIsLocatedWhenPresent()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(ResourcePath));
            this.mockPortalTemplateIO.Setup(x => x.GetResourceFilePath(ResourcePath)).Returns(ResourceFilePath);

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Has.Count.EqualTo(1));
            AssertTemplateInfo(ResourceExpectations, templates[0]);
        }

        [Test]
        public void SingleTemplateAndLanguage()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(DefaultPath));
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "de-DE")).Returns(DefaultDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultDePath)).Returns(this.CreateLanguageFileReader(DefaultDeName, DefaultDeDescription));

            // Act
            var templates = CreateController().GetPortalTemplates();

            // Assert
            Assert.That(templates, Has.Count.EqualTo(1));
            AssertTemplateInfo(DefaultExpectationsDe, templates[0]);
        }

        [Test]
        public void ATemplateCanBeLoadedDirectly()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(DefaultPath));
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "de-DE")).Returns(DefaultDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultDePath)).Returns(this.CreateLanguageFileReader(DefaultDeName, DefaultDeDescription));

            // Act
            var template = CreateController().GetPortalTemplate(DefaultPath, "de-DE");

            // Assert
            AssertTemplateInfo(DefaultExpectationsDe, template);
        }

        [Test]
        public void GetPortalTemplateReturnsNullIfCultureDoesNotMatch()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(DefaultPath));
            this.mockPortalTemplateIO.Setup(x => x.EnumerateLanguageFiles()).Returns(this.ToEnumerable(DefaultDePath));
            this.mockPortalTemplateIO.Setup(x => x.GetLanguageFilePath(DefaultPath, "de-DE")).Returns(DefaultDePath);
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(DefaultDePath)).Returns(this.CreateLanguageFileReader(DefaultDeName, DefaultDeDescription));

            // Act
            var template = CreateController().GetPortalTemplate(DefaultPath, "de");

            // Assert
            Assert.That(template, Is.Null);
        }

        [Test]
        public void GetPortalTemplateCanReturnAStaticTemplate()
        {
            // Arrange
            this.mockPortalTemplateIO.Setup(x => x.EnumerateTemplates()).Returns(this.ToEnumerable(StaticPath));
            this.mockPortalTemplateIO.Setup(x => x.OpenTextReader(StaticPath)).Returns(this.CreateTemplateFileReader(StaticDescription));

            // Act
            var template = CreateController().GetPortalTemplate(StaticPath, "en-US");

            // Assert
            AssertTemplateInfo(StaticExpectations, template);
        }

        private static PortalTemplateController CreateController()
        {
            return new PortalTemplateController(Mock.Of<IBusinessControllerProvider>());
        }

        private static void AssertTemplateInfo(Dictionary<string, string> expectations, IPortalTemplateInfo templateInfo)
        {
            AssertTemplateField(expectations, "Name", templateInfo.Name);
            AssertTemplateField(expectations, "TemplateFilePath", templateInfo.TemplateFilePath);
            AssertTemplateField(expectations, "CultureCode", templateInfo.CultureCode);
            AssertTemplateField(expectations, "LanguageFilePath", templateInfo.LanguageFilePath);
            AssertTemplateField(expectations, "ResourceFilePath", templateInfo.ResourceFilePath);
            AssertTemplateField(expectations, "Description", templateInfo.Description);
        }

        private static void AssertTemplateField(Dictionary<string, string> expectations, string key, string value)
        {
            string expected;
            expectations.TryGetValue(key, out expected);
            if (string.IsNullOrEmpty(expected))
            {
                Assert.That(value, Is.Null.Or.Empty, string.Format("Checking value of " + key));
            }
            else
            {
                Assert.That(value, Is.EqualTo(expected), string.Format("Checking value of " + key));
            }
        }

        private static string MakePath(string name)
        {
            var fileName = name + ".template";
            return Path.Combine(HostMapPath, fileName);
        }

        private static string MakePath(string name, string culture)
        {
            return string.Format(@"{0}.{1}.resx", MakePath(name), culture);
        }

        private TextReader CreateLanguageFileReader(string name)
        {
            return this.CreateLanguageFileReader(name, null);
        }

        private TextReader CreateLanguageFileReader(string name, string description)
        {
            if (description != null)
            {
                description = string.Format("<data name=\"PortalDescription.Text\" xml:space=\"preserve\"><value>{0}</value></data>", description);
            }

            var xml = string.Format("<root><data name=\"LocalizedTemplateName.Text\" xml:space=\"preserve\"><value>{0}</value></data>{1}</root>", name, description);
            return new StringReader(xml);
        }

        private TextReader CreateTemplateFileReader(string description)
        {
            var xml = string.Format("<portal><description>{0}</description></portal>", description);
            return new StringReader(xml);
        }

        private IEnumerable<T> ToEnumerable<T>(params T[] input)
        {
            return input;
        }
    }
}
