// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the CspDirectiveNameMapper class.
    /// </summary>
    [TestClass]
    public class CspDirectiveNameMapperTests
    {
        /// <summary>
        /// Tests GetDirectiveName with all known directive types.
        /// </summary>
        [TestMethod]
        public void GetDirectiveName_AllKnownTypes_ShouldReturnCorrectNames()
        {
            // Arrange & Act & Assert
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.DefaultSrc).Should().Be("default-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ScriptSrc).Should().Be("script-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.StyleSrc).Should().Be("style-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ImgSrc).Should().Be("img-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ConnectSrc).Should().Be("connect-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FontSrc).Should().Be("font-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ObjectSrc).Should().Be("object-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.MediaSrc).Should().Be("media-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FrameSrc).Should().Be("frame-src");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.BaseUri).Should().Be("base-uri");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.PluginTypes).Should().Be("plugin-types");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.SandboxDirective).Should().Be("sandbox");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FormAction).Should().Be("form-action");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.FrameAncestors).Should().Be("frame-ancestors");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ReportUri).Should().Be("report-uri");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.ReportTo).Should().Be("report-to");
            CspDirectiveNameMapper.GetDirectiveName(CspDirectiveType.UpgradeInsecureRequests).Should().Be("upgrade-insecure-requests");
        }

        /// <summary>
        /// Tests GetDirectiveType with all known directive names.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_AllKnownNames_ShouldReturnCorrectTypes()
        {
            // Arrange & Act & Assert
            CspDirectiveNameMapper.GetDirectiveType("default-src").Should().Be(CspDirectiveType.DefaultSrc);
            CspDirectiveNameMapper.GetDirectiveType("script-src").Should().Be(CspDirectiveType.ScriptSrc);
            CspDirectiveNameMapper.GetDirectiveType("style-src").Should().Be(CspDirectiveType.StyleSrc);
            CspDirectiveNameMapper.GetDirectiveType("img-src").Should().Be(CspDirectiveType.ImgSrc);
            CspDirectiveNameMapper.GetDirectiveType("connect-src").Should().Be(CspDirectiveType.ConnectSrc);
            CspDirectiveNameMapper.GetDirectiveType("font-src").Should().Be(CspDirectiveType.FontSrc);
            CspDirectiveNameMapper.GetDirectiveType("object-src").Should().Be(CspDirectiveType.ObjectSrc);
            CspDirectiveNameMapper.GetDirectiveType("media-src").Should().Be(CspDirectiveType.MediaSrc);
            CspDirectiveNameMapper.GetDirectiveType("frame-src").Should().Be(CspDirectiveType.FrameSrc);
            CspDirectiveNameMapper.GetDirectiveType("base-uri").Should().Be(CspDirectiveType.BaseUri);
            CspDirectiveNameMapper.GetDirectiveType("plugin-types").Should().Be(CspDirectiveType.PluginTypes);
            CspDirectiveNameMapper.GetDirectiveType("sandbox").Should().Be(CspDirectiveType.SandboxDirective);
            CspDirectiveNameMapper.GetDirectiveType("form-action").Should().Be(CspDirectiveType.FormAction);
            CspDirectiveNameMapper.GetDirectiveType("frame-ancestors").Should().Be(CspDirectiveType.FrameAncestors);
            CspDirectiveNameMapper.GetDirectiveType("report-uri").Should().Be(CspDirectiveType.ReportUri);
            CspDirectiveNameMapper.GetDirectiveType("report-to").Should().Be(CspDirectiveType.ReportTo);
            CspDirectiveNameMapper.GetDirectiveType("upgrade-insecure-requests").Should().Be(CspDirectiveType.UpgradeInsecureRequests);
        }

        /// <summary>
        /// Tests GetDirectiveType with case insensitive input.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_CaseInsensitiveInput_ShouldReturnCorrectType()
        {
            // Arrange & Act & Assert
            CspDirectiveNameMapper.GetDirectiveType("DEFAULT-SRC").Should().Be(CspDirectiveType.DefaultSrc);
            CspDirectiveNameMapper.GetDirectiveType("Script-Src").Should().Be(CspDirectiveType.ScriptSrc);
            CspDirectiveNameMapper.GetDirectiveType("STYLE-SRC").Should().Be(CspDirectiveType.StyleSrc);
        }

        /// <summary>
        /// Tests GetDirectiveType with unknown directive name.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_UnknownDirectiveName_ShouldThrowArgumentException()
        {
            // Arrange
            var unknownDirective = "unknown-directive";

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(unknownDirective));
            exception.Message.Should().Contain($"Unknown directive name: {unknownDirective}");
        }

        /// <summary>
        /// Tests GetDirectiveType with null input.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_NullInput_ShouldThrowArgumentException()
        {
            // Arrange
            string directiveName = null;

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            exception.Message.Should().Contain("Directive name cannot be null or empty");
        }

        /// <summary>
        /// Tests GetDirectiveType with empty input.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_EmptyInput_ShouldThrowArgumentException()
        {
            // Arrange
            var directiveName = string.Empty;

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            exception.Message.Should().Contain("Directive name cannot be null or empty");
        }

        /// <summary>
        /// Tests GetDirectiveType with whitespace input.
        /// </summary>
        [TestMethod]
        public void GetDirectiveType_WhitespaceInput_ShouldThrowArgumentException()
        {
            // Arrange
            var directiveName = "   ";

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspDirectiveNameMapper.GetDirectiveType(directiveName));
            exception.Message.Should().Contain("Directive name cannot be null or empty");
        }

        /// <summary>
        /// Tests TryGetDirectiveType with valid directive names.
        /// </summary>
        [TestMethod]
        public void TryGetDirectiveType_ValidDirectiveNames_ShouldReturnTrueAndCorrectType()
        {
            // Arrange & Act & Assert
            CspDirectiveNameMapper.TryGetDirectiveType("default-src", out var defaultSrcType).Should().BeTrue();
            defaultSrcType.Should().Be(CspDirectiveType.DefaultSrc);

            CspDirectiveNameMapper.TryGetDirectiveType("script-src", out var scriptSrcType).Should().BeTrue();
            scriptSrcType.Should().Be(CspDirectiveType.ScriptSrc);

            CspDirectiveNameMapper.TryGetDirectiveType("upgrade-insecure-requests", out var upgradeType).Should().BeTrue();
            upgradeType.Should().Be(CspDirectiveType.UpgradeInsecureRequests);
        }

        /// <summary>
        /// Tests TryGetDirectiveType with invalid directive names.
        /// </summary>
        [TestMethod]
        public void TryGetDirectiveType_InvalidDirectiveNames_ShouldReturnFalseAndDefaultType()
        {
            // Arrange & Act & Assert
            CspDirectiveNameMapper.TryGetDirectiveType("unknown-directive", out var type1).Should().BeFalse();
            type1.Should().Be(default(CspDirectiveType));

            CspDirectiveNameMapper.TryGetDirectiveType(null, out var type2).Should().BeFalse();
            type2.Should().Be(default(CspDirectiveType));

            CspDirectiveNameMapper.TryGetDirectiveType(string.Empty, out var type3).Should().BeFalse();
            type3.Should().Be(default(CspDirectiveType));
        }

        /// <summary>
        /// Tests round-trip conversion (type to name to type).
        /// </summary>
        [TestMethod]
        public void RoundTripConversion_AllDirectiveTypes_ShouldReturnOriginalType()
        {
            // Arrange
            var allDirectiveTypes = Enum.GetValues(typeof(CspDirectiveType)).Cast<CspDirectiveType>();

            // Act & Assert
            foreach (var originalType in allDirectiveTypes)
            {
                var directiveName = CspDirectiveNameMapper.GetDirectiveName(originalType);
                var convertedType = CspDirectiveNameMapper.GetDirectiveType(directiveName);
                convertedType.Should().Be(originalType, $"Round-trip conversion failed for {originalType}");
            }
        }
    }
}
