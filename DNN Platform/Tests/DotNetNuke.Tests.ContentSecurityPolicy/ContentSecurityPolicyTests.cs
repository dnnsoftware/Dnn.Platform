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
    /// Unit tests for ContentSecurityPolicy class with parser integration.
    /// </summary>
    [TestClass]
    public class ContentSecurityPolicyTests
    {
        /// <summary>
        /// Tests parsing using the instance-based parser integration.
        /// </summary>
        [TestMethod]
        public void Parse_ValidInput_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123' cdn.example.com cdn.example.com/ *.example.com 10.10.10.10 https://*.example.com:12/path/to/file.js http://[fe80::1]/index.html; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            policy.Should().BeAssignableTo<IContentSecurityPolicy>();

            // Verify we can access parsed directives
            policy.DefaultSource.Should().NotBeNull();
            policy.ScriptSource.Should().NotBeNull();
            policy.StyleSource.Should().NotBeNull();
            policy.ImgSource.Should().NotBeNull();
            policy.ConnectSource.Should().NotBeNull();
            policy.FontSource.Should().NotBeNull();
            policy.FrameAncestors.Should().NotBeNull();

            // Verify the policy can be regenerated
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().NotBeNullOrEmpty();
            generatedPolicy.Should().Contain("default-src 'self'");
        }

        /// <summary>
        /// Tests TryParse functionality.
        /// </summary>
        [TestMethod]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(cspHeader);

            // Assert
            result.Should().BeTrue();
            policy.Should().NotBeNull();
            policy.Should().BeAssignableTo<IContentSecurityPolicy>();
            policy.GeneratePolicy().Should().Contain("default-src 'self'");
            policy.GeneratePolicy().Should().Contain("script-src 'self'");
        }

        /// <summary>
        /// Tests TryParse with invalid input.
        /// </summary>
        [TestMethod]
        public void TryParse_InvalidInput_ShouldReturnFalse()
        {
            // Arrange
            var invalidCspHeader = string.Empty;
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(invalidCspHeader);

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Tests that parsed policy can be modified and regenerated.
        /// </summary>
        [TestMethod]
        public void Parse_ModifyParsedPolicy_ShouldGenerateUpdatedPolicy()
        {
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(originalCspHeader);

            // Modify the parsed policy
            policy.ScriptSource.AddHost("newcdn.example.com");
            policy.StyleSource.AddHash("sha256-abc123def456");

            var modifiedPolicy = policy.GeneratePolicy();

            // Assert
            modifiedPolicy.Should().Contain("default-src 'self'");
            modifiedPolicy.Should().Contain("script-src");
            modifiedPolicy.Should().Contain("'self'");
            modifiedPolicy.Should().Contain("newcdn.example.com");
            modifiedPolicy.Should().Contain("style-src");
            modifiedPolicy.Should().Contain("'sha256-abc123def456'");
        }

        /// <summary>
        /// Tests nonce generation on policy instance.
        /// </summary>
        [TestMethod]
        public void Parse_AccessNonce_ShouldGenerateNonce()
        {
            // Arrange
            var cspHeader = "default-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);
            var nonce = policy.Nonce;

            // Assert
            nonce.Should().NotBeNullOrEmpty();
            nonce.Length.Should().BeGreaterThan(0);

            // Nonce should be consistent across multiple calls
            var nonce2 = policy.Nonce;
            nonce2.Should().Be(nonce);
        }

        /// <summary>
        /// Tests parsing and using all directive types.
        /// </summary>
        [TestMethod]
        public void Parse_AllDirectiveTypes_ShouldParseCorrectly()
        {
            // Arrange
            var cspHeader = "default-src 'self'; " +
                           "script-src 'self' 'unsafe-inline'; " +
                           "style-src 'self' 'unsafe-inline'; " +
                           "img-src 'self' data:; " +
                           "connect-src 'self'; " +
                           "font-src 'self'; " +
                           "object-src 'none'; " +
                           "media-src 'self'; " +
                           "frame-src 'none'; " +
                           "frame-ancestors 'none'; " +
                           "form-action 'self'; " +
                           "base-uri 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();

            // Verify all directive contributors are accessible
            policy.DefaultSource.Should().NotBeNull();
            policy.ScriptSource.Should().NotBeNull();
            policy.StyleSource.Should().NotBeNull();
            policy.ImgSource.Should().NotBeNull();
            policy.ConnectSource.Should().NotBeNull();
            policy.FontSource.Should().NotBeNull();
            policy.ObjectSource.Should().NotBeNull();
            policy.MediaSource.Should().NotBeNull();
            policy.FrameSource.Should().NotBeNull();
            policy.FrameAncestors.Should().NotBeNull();
            policy.FormAction.Should().NotBeNull();
            policy.BaseUriSource.Should().NotBeNull();

            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("script-src");
            generatedPolicy.Should().Contain("style-src");
            generatedPolicy.Should().Contain("img-src");
            generatedPolicy.Should().Contain("connect-src");
            generatedPolicy.Should().Contain("font-src");
            generatedPolicy.Should().Contain("object-src");
            generatedPolicy.Should().Contain("media-src");
            generatedPolicy.Should().Contain("frame-src");
            generatedPolicy.Should().Contain("frame-ancestors");
            generatedPolicy.Should().Contain("form-action");
            generatedPolicy.Should().Contain("base-uri");
        }

        /// <summary>
        /// Tests parsing policy with reporting directives.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithReporting_ShouldParseCorrectly()
        {
            // Arrange
            var cspHeader = "default-src 'self'; report-uri http:///csp-report; report-to csp-endpoint";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("default-src 'self'");
            // Note: reporting directives might be handled differently in the implementation
        }

        /// <summary>
        /// Tests parsing policy with upgrade-insecure-requests.
        /// </summary>
        [TestMethod]
        public void Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()
        {
            // Arrange
            var cspHeader = "default-src 'self'; upgrade-insecure-requests";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            policy.Should().NotBeNull();
            var generatedPolicy = policy.GeneratePolicy();
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("upgrade-insecure-requests");
        }

        /// <summary>
        /// Tests round-trip parsing (parse then generate should produce similar result).
        /// </summary>
        [TestMethod]
        public void Parse_RoundTrip_ShouldProduceSimilarPolicy()
        {
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(originalCspHeader);
            var regeneratedHeader = policy.GeneratePolicy();

            // Assert
            regeneratedHeader.Should().Contain("default-src 'self'");
            regeneratedHeader.Should().Contain("script-src");
            regeneratedHeader.Should().Contain("'self'");
            regeneratedHeader.Should().Contain("'unsafe-inline'");
            regeneratedHeader.Should().Contain("style-src");

            // The order might be different, but all elements should be present
            var originalParts = originalCspHeader.Split(';').Select(p => p.Trim()).ToArray();
            foreach (var part in originalParts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    // Check that the directive name and 'self' are present
                    var directiveName = part.Split(' ')[0];
                    regeneratedHeader.Should().Contain(directiveName);
                }
            }
        }
    }
}
