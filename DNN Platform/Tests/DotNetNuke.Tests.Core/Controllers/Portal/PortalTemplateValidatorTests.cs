// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Tests.Core.Controllers.Portal
{
    using System;
    using System.IO;
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    using NUnit.Framework;

    [TestFixture]
    public class PortalTemplateValidatorTests
    {
        [TestCase("Blank Website.template")]
        [TestCase("Default Website.template")]
        public void DefaultPortalTemplatesShouldMatchSchema(string templateFileName)
        {
            // Arrange
            var basePath = GetPortalTemplateResourcesPath();
            var schemaFilePath = Path.Combine(basePath, "portal.template.xsd");
            var templateFilePath = Path.Combine(basePath, templateFileName);
            using var validator = new PortalTemplateValidator();

            // Act
            var isValid = validator.Validate(templateFilePath, schemaFilePath);

            // Assert
            Assert.That(File.Exists(schemaFilePath), Is.True, $"Missing schema file: {schemaFilePath}");
            Assert.That(File.Exists(templateFilePath), Is.True, $"Missing template file: {templateFilePath}");
            Assert.That(isValid, Is.True, string.Join(Environment.NewLine, validator.Errors.Cast<string>()));
        }

        [Test]
        public void InvalidPortalTemplateShouldFailSchemaValidation()
        {
            // Arrange
            var basePath = GetPortalTemplateResourcesPath();
            var schemaFilePath = Path.Combine(basePath, "portal.template.xsd");
            var templateFilePath = Path.GetTempFileName();
            var invalidTemplate = "<?xml version=\"1.0\"?><portal><settings><unknownsetting>True</unknownsetting></settings></portal>";
            File.WriteAllText(templateFilePath, invalidTemplate);
            using var validator = new PortalTemplateValidator();

            try
            {
                // Act
                var isValid = validator.Validate(templateFilePath, schemaFilePath);

                // Assert
                Assert.That(isValid, Is.False);
                Assert.That(validator.Errors.Cast<string>(), Has.Some.Contains("unknownsetting"));
            }
            finally
            {
                File.Delete(templateFilePath);
            }
        }

        private static string GetPortalTemplateResourcesPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "PortalTemplates");
        }
    }
}
