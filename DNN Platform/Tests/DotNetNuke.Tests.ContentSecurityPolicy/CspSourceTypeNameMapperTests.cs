// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the CspSourceTypeNameMapper class.
    /// </summary>
    [TestClass]
    public class CspSourceTypeNameMapperTests
    {
        /// <summary>
        /// Tests GetSourceTypeName with all known source types.
        /// </summary>
        [TestMethod]
        public void GetSourceTypeName_AllKnownTypes_ShouldReturnCorrectNames()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Host).Should().Be("host");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Scheme).Should().Be("scheme");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Self).Should().Be("'self'");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Inline).Should().Be("'unsafe-inline'");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Eval).Should().Be("'unsafe-eval'");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Nonce).Should().Be("nonce");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.Hash).Should().Be("hash");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.None).Should().Be("'none'");
            CspSourceTypeNameMapper.GetSourceTypeName(CspSourceType.StrictDynamic).Should().Be("'strict-dynamic'");
        }

        /// <summary>
        /// Tests GetSourceType with all known source names.
        /// </summary>
        [TestMethod]
        public void GetSourceType_AllKnownNames_ShouldReturnCorrectTypes()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.GetSourceType("'self'").Should().Be(CspSourceType.Self);
            CspSourceTypeNameMapper.GetSourceType("'unsafe-inline'").Should().Be(CspSourceType.Inline);
            CspSourceTypeNameMapper.GetSourceType("'unsafe-eval'").Should().Be(CspSourceType.Eval);
            CspSourceTypeNameMapper.GetSourceType("'none'").Should().Be(CspSourceType.None);
            CspSourceTypeNameMapper.GetSourceType("'strict-dynamic'").Should().Be(CspSourceType.StrictDynamic);
        }

        /// <summary>
        /// Tests GetSourceType with case insensitive input.
        /// </summary>
        [TestMethod]
        public void GetSourceType_CaseInsensitiveInput_ShouldReturnCorrectType()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.GetSourceType("'SELF'").Should().Be(CspSourceType.Self);
            CspSourceTypeNameMapper.GetSourceType("'Unsafe-Inline'").Should().Be(CspSourceType.Inline);
            CspSourceTypeNameMapper.GetSourceType("'UNSAFE-EVAL'").Should().Be(CspSourceType.Eval);
            CspSourceTypeNameMapper.GetSourceType("'NONE'").Should().Be(CspSourceType.None);
            CspSourceTypeNameMapper.GetSourceType("'STRICT-DYNAMIC'").Should().Be(CspSourceType.StrictDynamic);
        }

        /// <summary>
        /// Tests GetSourceType with unknown source name.
        /// </summary>
        [TestMethod]
        public void GetSourceType_UnknownSourceName_ShouldThrowArgumentException()
        {
            // Arrange
            var unknownSource = "'unknown-source'";

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(unknownSource));
            exception.Message.Should().Contain($"Unknown source name: {unknownSource}");
        }

        /// <summary>
        /// Tests GetSourceType with null input.
        /// </summary>
        [TestMethod]
        public void GetSourceType_NullInput_ShouldThrowArgumentException()
        {
            // Arrange
            string sourceName = null;

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
            exception.Message.Should().Contain("Source name cannot be null or empty");
        }

        /// <summary>
        /// Tests GetSourceType with empty input.
        /// </summary>
        [TestMethod]
        public void GetSourceType_EmptyInput_ShouldThrowArgumentException()
        {
            // Arrange
            var sourceName = string.Empty;

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
            exception.Message.Should().Contain("Source name cannot be null or empty");
        }

        /// <summary>
        /// Tests TryGetSourceType with valid source names.
        /// </summary>
        [TestMethod]
        public void TryGetSourceType_ValidSourceNames_ShouldReturnTrueAndCorrectType()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.TryGetSourceType("'self'", out var selfType).Should().BeTrue();
            selfType.Should().Be(CspSourceType.Self);

            CspSourceTypeNameMapper.TryGetSourceType("'unsafe-inline'", out var inlineType).Should().BeTrue();
            inlineType.Should().Be(CspSourceType.Inline);

            CspSourceTypeNameMapper.TryGetSourceType("'none'", out var noneType).Should().BeTrue();
            noneType.Should().Be(CspSourceType.None);
        }

        /// <summary>
        /// Tests TryGetSourceType with invalid source names.
        /// </summary>
        [TestMethod]
        public void TryGetSourceType_InvalidSourceNames_ShouldReturnFalseAndDefaultType()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.TryGetSourceType("'unknown-source'", out var type1).Should().BeFalse();
            type1.Should().Be(default(CspSourceType));

            CspSourceTypeNameMapper.TryGetSourceType(null, out var type2).Should().BeFalse();
            type2.Should().Be(default(CspSourceType));

            CspSourceTypeNameMapper.TryGetSourceType(string.Empty, out var type3).Should().BeFalse();
            type3.Should().Be(default(CspSourceType));

            CspSourceTypeNameMapper.TryGetSourceType("example.com", out var type4).Should().BeFalse();
            type4.Should().Be(default(CspSourceType));
        }

        /// <summary>
        /// Tests IsQuotedKeyword with various inputs.
        /// </summary>
        [TestMethod]
        public void IsQuotedKeyword_VariousInputs_ShouldReturnCorrectResults()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.IsQuotedKeyword("'self'").Should().BeTrue();
            CspSourceTypeNameMapper.IsQuotedKeyword("'unsafe-inline'").Should().BeTrue();
            CspSourceTypeNameMapper.IsQuotedKeyword("'none'").Should().BeTrue();
            CspSourceTypeNameMapper.IsQuotedKeyword("'nonce-abc123'").Should().BeTrue();
            CspSourceTypeNameMapper.IsQuotedKeyword("'sha256-abc123'").Should().BeTrue();

            CspSourceTypeNameMapper.IsQuotedKeyword("example.com").Should().BeFalse();
            CspSourceTypeNameMapper.IsQuotedKeyword("https:").Should().BeFalse();
            CspSourceTypeNameMapper.IsQuotedKeyword("self").Should().BeFalse(); // Missing quotes
            CspSourceTypeNameMapper.IsQuotedKeyword("'self").Should().BeFalse(); // Missing closing quote
            CspSourceTypeNameMapper.IsQuotedKeyword("self'").Should().BeFalse(); // Missing opening quote
            CspSourceTypeNameMapper.IsQuotedKeyword(null).Should().BeFalse();
            CspSourceTypeNameMapper.IsQuotedKeyword(string.Empty).Should().BeFalse();
            CspSourceTypeNameMapper.IsQuotedKeyword("   ").Should().BeFalse();
        }

        /// <summary>
        /// Tests IsNonceSource with various inputs.
        /// </summary>
        [TestMethod]
        public void IsNonceSource_VariousInputs_ShouldReturnCorrectResults()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.IsNonceSource("'nonce-abc123'").Should().BeTrue();
            CspSourceTypeNameMapper.IsNonceSource("'nonce-xyz789def'").Should().BeTrue();
            CspSourceTypeNameMapper.IsNonceSource("'nonce-'").Should().BeTrue(); // Edge case: empty nonce value

            CspSourceTypeNameMapper.IsNonceSource("'self'").Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource("'unsafe-inline'").Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource("'sha256-abc123'").Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource("nonce-abc123").Should().BeFalse(); // Missing quotes
            CspSourceTypeNameMapper.IsNonceSource("'nonce-abc123").Should().BeFalse(); // Missing closing quote
            CspSourceTypeNameMapper.IsNonceSource("nonce-abc123'").Should().BeFalse(); // Missing opening quote
            CspSourceTypeNameMapper.IsNonceSource("example.com").Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource(null).Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource(string.Empty).Should().BeFalse();
            CspSourceTypeNameMapper.IsNonceSource("   ").Should().BeFalse();
        }

        /// <summary>
        /// Tests IsHashSource with various inputs.
        /// </summary>
        [TestMethod]
        public void IsHashSource_VariousInputs_ShouldReturnCorrectResults()
        {
            // Arrange & Act & Assert
            CspSourceTypeNameMapper.IsHashSource("'sha256-abc123'").Should().BeTrue();
            CspSourceTypeNameMapper.IsHashSource("'sha384-def456'").Should().BeTrue();
            CspSourceTypeNameMapper.IsHashSource("'sha512-ghi789'").Should().BeTrue();

            CspSourceTypeNameMapper.IsHashSource("'self'").Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource("'unsafe-inline'").Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource("'nonce-abc123'").Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource("sha256-abc123").Should().BeFalse(); // Missing quotes
            CspSourceTypeNameMapper.IsHashSource("'sha256-abc123").Should().BeFalse(); // Missing closing quote
            CspSourceTypeNameMapper.IsHashSource("sha256-abc123'").Should().BeFalse(); // Missing opening quote
            CspSourceTypeNameMapper.IsHashSource("'md5-abc123'").Should().BeFalse(); // Unsupported hash algorithm
            CspSourceTypeNameMapper.IsHashSource("example.com").Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource(null).Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource(string.Empty).Should().BeFalse();
            CspSourceTypeNameMapper.IsHashSource("   ").Should().BeFalse();
        }

        /// <summary>
        /// Tests round-trip conversion for supported source types.
        /// </summary>
        [TestMethod]
        public void RoundTripConversion_SupportedSourceTypes_ShouldReturnOriginalType()
        {
            // Note: Only testing source types that have direct string representations
            var supportedTypes = new[]
            {
                CspSourceType.Self,
                CspSourceType.Inline,
                CspSourceType.Eval,
                CspSourceType.None,
                CspSourceType.StrictDynamic,
            };

            // Act & Assert
            foreach (var originalType in supportedTypes)
            {
                var sourceName = CspSourceTypeNameMapper.GetSourceTypeName(originalType);
                var convertedType = CspSourceTypeNameMapper.GetSourceType(sourceName);
                convertedType.Should().Be(originalType, $"Round-trip conversion failed for {originalType}");
            }
        }

        /// <summary>
        /// Tests that non-quoted source types throw exceptions when passed to GetSourceType.
        /// </summary>
        [TestMethod]
        public void GetSourceType_NonQuotedSourceTypes_ShouldThrowException()
        {
            // These types don't have direct string representations that can be parsed back
            var nonQuotedTypes = new[]
            {
                CspSourceType.Host,
                CspSourceType.Scheme,
                CspSourceType.Nonce,
                CspSourceType.Hash,
            };

            foreach (var sourceType in nonQuotedTypes)
            {
                var sourceName = CspSourceTypeNameMapper.GetSourceTypeName(sourceType);
                
                // These should throw exceptions when trying to parse them back
                Assert.ThrowsException<ArgumentException>(() => CspSourceTypeNameMapper.GetSourceType(sourceName));
            }
        }
    }
}
