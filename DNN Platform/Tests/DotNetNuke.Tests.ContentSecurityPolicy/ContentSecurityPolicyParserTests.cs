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
    /// Unit tests for the ContentSecurityPolicyParser class using instance-based approach.
    /// </summary>
    [TestClass]
    public class ContentSecurityPolicyParserTests
    {
        /// <summary>
        /// Tests parsing of a basic CSP policy.
        /// </summary>
        [TestMethod]
        public void Parse_BasicPolicy_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            policy.GeneratePolicy().Should().Be("default-src 'self'");
        }

        /// <summary>
        /// Tests parsing of a policy with multiple sources.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithMultipleSources_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "script-src 'self' 'unsafe-inline' https://cdn.example.com";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("'self'");
            generatedPolicy.Should().Contain("'unsafe-inline'");
            generatedPolicy.Should().Contain("https://cdn.example.com");
        }

        /// <summary>
        /// Tests parsing of a policy with nonce values.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithNonce_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "script-src 'self' 'nonce-abc123def456'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("'self'");
            generatedPolicy.Should().Contain("'nonce-abc123def456'");
        }

        /// <summary>
        /// Tests parsing of a policy with hash values.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithHash_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "style-src 'self' 'sha256-abc123def456789'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("style-src");
            generatedPolicy.Should().Contain("'self'");
            generatedPolicy.Should().Contain("'sha256-abc123def456789'");
        }

        /// <summary>
        /// Tests parsing of a complex policy from the example.
        /// </summary>
        [TestMethod]
        public void Parse_ComplexPolicy_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri /csp-report";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();

            // Check that all expected directives are present
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("'strict-dynamic'");
            generatedPolicy.Should().Contain("style-src");
            generatedPolicy.Should().Contain("'unsafe-inline'");
            generatedPolicy.Should().Contain("img-src");
            generatedPolicy.Should().Contain("data:");
            generatedPolicy.Should().Contain("blob:");
            generatedPolicy.Should().Contain("connect-src");
            generatedPolicy.Should().Contain("wss:");
            generatedPolicy.Should().Contain("font-src");
            generatedPolicy.Should().Contain("https://fonts.googleapis.com");
            generatedPolicy.Should().Contain("frame-ancestors 'none'");
            generatedPolicy.Should().Contain("upgrade-insecure-requests");
        }

        /// <summary>
        /// Tests parsing of a policy with sandbox directive.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithSandbox_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "sandbox allow-forms allow-scripts; script-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("sandbox");
            generatedPolicy.Should().Contain("allow-forms");
            generatedPolicy.Should().Contain("allow-scripts");
            generatedPolicy.Should().Contain("script-src 'self'");
        }

        /// <summary>
        /// Tests parsing of a policy with form-action directive.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithFormAction_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "form-action 'self' https://secure.example.com";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("form-action");
            generatedPolicy.Should().Contain("'self'");
            generatedPolicy.Should().Contain("https://secure.example.com");
        }

        /// <summary>
        /// Tests parsing of the real-world complex policy from the example.
        /// </summary>
        [TestMethod]
        public void Parse_RealWorldComplexPolicy_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();

            // Check key directives
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("img-src");
            generatedPolicy.Should().Contain("https://front.satrabel.be");
            generatedPolicy.Should().Contain("font-src");
            generatedPolicy.Should().Contain("https://fonts.gstatic.com");
            generatedPolicy.Should().Contain("style-src");
            generatedPolicy.Should().Contain("https://fonts.googleapis.com");
            generatedPolicy.Should().Contain("frame-ancestors 'self'");
            generatedPolicy.Should().Contain("frame-src 'self'");
            generatedPolicy.Should().Contain("form-action 'self'");
            generatedPolicy.Should().Contain("object-src 'none'");
            generatedPolicy.Should().Contain("base-uri 'self'");
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ='");
            generatedPolicy.Should().Contain("'strict-dynamic'");
            generatedPolicy.Should().Contain("connect-src");
            generatedPolicy.Should().Contain("https://www.googletagmanager.com");
            generatedPolicy.Should().Contain("upgrade-insecure-requests");
        }

        /// <summary>
        /// Tests TryParse with valid input.
        /// </summary>
        [TestMethod]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(cspHeader);

            // Assert
            result.Should().BeTrue();
            policy.Should().NotBeNull();
            policy.GeneratePolicy().Should().Be("default-src 'self'");
        }

        /// <summary>
        /// Tests TryParse with invalid input.
        /// </summary>
        [TestMethod]
        public void TryParse_InvalidInput_ShouldReturnFalse()
        {
            // Arrange
            var cspHeader = string.Empty;
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(cspHeader);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests Parse with null input should throw exception.
        /// </summary>
        [TestMethod]
        public void Parse_NullInput_ShouldThrowArgumentException()
        {
            // Arrange
            string cspHeader = null;
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => parser.Parse(cspHeader));
            exception.Message.Should().Contain("CSP header cannot be null or empty");
        }

        /// <summary>
        /// Tests Parse with empty input should throw exception.
        /// </summary>
        [TestMethod]
        public void Parse_EmptyInput_ShouldThrowArgumentException()
        {
            // Arrange
            var cspHeader = string.Empty;
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => parser.Parse(cspHeader));
            exception.Message.Should().Contain("CSP header cannot be null or empty");
        }

        /// <summary>
        /// Tests Parse with whitespace-only input should throw exception.
        /// </summary>
        [TestMethod]
        public void Parse_WhitespaceOnlyInput_ShouldThrowArgumentException()
        {
            // Arrange
            var cspHeader = "   ";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentException>(() => parser.Parse(cspHeader));
            exception.Message.Should().Contain("CSP header cannot be null or empty");
        }

        /// <summary>
        /// Tests that parsing ignores unknown directives gracefully.
        /// </summary>
        [TestMethod]
        public void Parse_UnknownDirective_ShouldIgnoreUnknownDirectiveAndParseKnownOnes()
        {
            // Arrange
            var cspHeader = "default-src 'self'; unknown-directive something; script-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("script-src 'self'");
            generatedPolicy.Should().NotContain("unknown-directive");
        }

        /// <summary>
        /// Tests parsing with various scheme sources.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithSchemes_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "img-src 'self' data: https: blob: filesystem:";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("img-src");
            generatedPolicy.Should().Contain("'self'");
            generatedPolicy.Should().Contain("data:");
            generatedPolicy.Should().Contain("https:");
            generatedPolicy.Should().Contain("blob:");
            generatedPolicy.Should().Contain("filesystem:");
        }

        /// <summary>
        /// Tests parsing with various hash algorithms.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithDifferentHashAlgorithms_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "script-src 'sha256-abc123' 'sha384-def456' 'sha512-ghi789'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("'sha256-abc123'");
            generatedPolicy.Should().Contain("'sha384-def456'");
            generatedPolicy.Should().Contain("'sha512-ghi789'");
        }

        /// <summary>
        /// Tests that constructor with null policy throws ArgumentNullException.
        /// </summary>
        [TestMethod]
        public void Constructor_NullPolicy_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.ThrowsException<ArgumentNullException>(() => new ContentSecurityPolicyParser(null));
            exception.ParamName.Should().Be("policy");
        }
    }
}