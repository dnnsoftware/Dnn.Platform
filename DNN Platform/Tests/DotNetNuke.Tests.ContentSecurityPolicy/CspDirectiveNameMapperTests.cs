// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for the CspDirectiveNameMapper class.
    /// </summary>
    [TestFixture]
    public class CspDirectiveNameMapperTests
    {
        /// <summary>
        /// Tests GetDirectiveName with all known directive types.
        /// </summary>
        [Test]
        public void GetDirectiveName_AllKnownTypes_ShouldReturnCorrectNames()
        {
            // Arrange & Act & Assert
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.DefaultSrc), Is.EqualTo("default-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ScriptSrc), Is.EqualTo("script-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.StyleSrc), Is.EqualTo("style-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ImgSrc), Is.EqualTo("img-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ConnectSrc), Is.EqualTo("connect-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FontSrc), Is.EqualTo("font-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ObjectSrc), Is.EqualTo("object-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.MediaSrc), Is.EqualTo("media-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FrameSrc), Is.EqualTo("frame-src"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.BaseUri), Is.EqualTo("base-uri"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.PluginTypes), Is.EqualTo("plugin-types"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.SandboxDirective), Is.EqualTo("sandbox"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FormAction), Is.EqualTo("form-action"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FrameAncestors), Is.EqualTo("frame-ancestors"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ReportUri), Is.EqualTo("report-uri"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ReportTo), Is.EqualTo("report-to"));
            Assert.That(CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.UpgradeInsecureRequests), Is.EqualTo("upgrade-insecure-requests"));
        }

        /// <summary>
        /// Tests GetDirectiveType with all known directive names.
        /// </summary>
        [Test]
        public void GetDirectiveType_AllKnownNames_ShouldReturnCorrectTypes()
        {
            // Arrange & Act & Assert
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("default-src"), Is.EqualTo(CspDirectiveType.DefaultSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("script-src"), Is.EqualTo(CspDirectiveType.ScriptSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("style-src"), Is.EqualTo(CspDirectiveType.StyleSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("img-src"), Is.EqualTo(CspDirectiveType.ImgSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("connect-src"), Is.EqualTo(CspDirectiveType.ConnectSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("font-src"), Is.EqualTo(CspDirectiveType.FontSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("object-src"), Is.EqualTo(CspDirectiveType.ObjectSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("media-src"), Is.EqualTo(CspDirectiveType.MediaSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("frame-src"), Is.EqualTo(CspDirectiveType.FrameSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("base-uri"), Is.EqualTo(CspDirectiveType.BaseUri));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("plugin-types"), Is.EqualTo(CspDirectiveType.PluginTypes));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("sandbox"), Is.EqualTo(CspDirectiveType.SandboxDirective));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("form-action"), Is.EqualTo(CspDirectiveType.FormAction));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("frame-ancestors"), Is.EqualTo(CspDirectiveType.FrameAncestors));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("report-uri"), Is.EqualTo(CspDirectiveType.ReportUri));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("report-to"), Is.EqualTo(CspDirectiveType.ReportTo));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("upgrade-insecure-requests"), Is.EqualTo(CspDirectiveType.UpgradeInsecureRequests));
        }

        /// <summary>
        /// Tests GetDirectiveType with case insensitive input.
        /// </summary>
        [Test]
        public void GetDirectiveType_CaseInsensitiveInput_ShouldReturnCorrectType()
        {
            // Arrange & Act & Assert
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("DEFAULT-SRC"), Is.EqualTo(CspDirectiveType.DefaultSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("Script-Src"), Is.EqualTo(CspDirectiveType.ScriptSrc));
            Assert.That(CspDirectiveNameMapper.GetDirectiveType("STYLE-SRC"), Is.EqualTo(CspDirectiveType.StyleSrc));
        }

        /// <summary>
        /// Tests GetDirectiveType with unknown directive name.
        /// </summary>
        [Test]
        public void GetDirectiveType_UnknownDirectiveName_ShouldThrowArgumentException()
        {
            // Arrange
            var unknownDirective = "unknown-directive";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(unknownDirective));
            Assert.That(exception.Message, Does.Contain($"Unknown directive name: {unknownDirective}"));
        }

        /// <summary>
        /// Tests GetDirectiveType with null input.
        /// </summary>
        [Test]
        public void GetDirectiveType_NullInput_ShouldThrowArgumentException()
        {
            // Arrange
            string directiveName = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            Assert.That(exception.Message, Does.Contain("Directive name cannot be null or empty"));
        }

        /// <summary>
        /// Tests GetDirectiveType with empty input.
        /// </summary>
        [Test]
        public void GetDirectiveType_EmptyInput_ShouldThrowArgumentException()
        {
            // Arrange
            var directiveName = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            Assert.That(exception.Message, Does.Contain("Directive name cannot be null or empty"));
        }

        /// <summary>
        /// Tests GetDirectiveType with whitespace input.
        /// </summary>
        [Test]
        public void GetDirectiveType_WhitespaceInput_ShouldThrowArgumentException()
        {
            // Arrange
            var directiveName = "   ";

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            Assert.That(exception.Message, Does.Contain("Directive name cannot be null or empty"));
        }

        /// <summary>
        /// Tests TryGetDirectiveType with valid directive names.
        /// </summary>
        [Test]
        public void TryGetDirectiveType_ValidDirectiveNames_ShouldReturnTrueAndCorrectType()
        {
            // Arrange & Act & Assert
            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType("default-src", out var defaultSrcType), Is.True);
            Assert.That(defaultSrcType, Is.EqualTo(CspDirectiveType.DefaultSrc));

            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType("script-src", out var scriptSrcType), Is.True);
            Assert.That(scriptSrcType, Is.EqualTo(CspDirectiveType.ScriptSrc));

            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType("upgrade-insecure-requests", out var upgradeType), Is.True);
            Assert.That(upgradeType, Is.EqualTo(CspDirectiveType.UpgradeInsecureRequests));
        }

        /// <summary>
        /// Tests TryGetDirectiveType with invalid directive names.
        /// </summary>
        [Test]
        public void TryGetDirectiveType_InvalidDirectiveNames_ShouldReturnFalseAndDefaultType()
        {
            // Arrange & Act & Assert
            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType("unknown-directive", out var type1), Is.False);
            Assert.That(type1, Is.EqualTo(default(CspDirectiveType)));

            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType(null, out var type2), Is.False);
            Assert.That(type2, Is.EqualTo(default(CspDirectiveType)));

            Assert.That(CspDirectiveNameMapper.TryGetDirectiveType(string.Empty, out var type3), Is.False);
            Assert.That(type3, Is.EqualTo(default(CspDirectiveType)));
        }

        /// <summary>
        /// Tests round-trip conversion (type to name to type).
        /// </summary>
        [Test]
        public void RoundTripConversion_AllDirectiveTypes_ShouldReturnOriginalType()
        {
            // Arrange
            var allDirectiveTypes = Enum.GetValues(typeof(CspDirectiveType)).Cast<CspDirectiveType>();

            // Act & Assert
            foreach (var originalType in allDirectiveTypes)
            {
                var directiveName = CspDirectiveNameMapper.GetDirectiveName(originalType);
                var convertedType = CspDirectiveNameMapper.GetDirectiveType(directiveName);
                Assert.That(convertedType, Is.EqualTo(originalType), $"Round-trip conversion failed for {originalType}");
            }
        }
    }
}
