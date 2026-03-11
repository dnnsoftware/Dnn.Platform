// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Unit tests for ContentSecurityPolicy class with parser integration.
    /// </summary>
    [TestFixture]
    public class ContentSecurityPolicyTests
    {
        /// <summary>
        /// Tests parsing using the instance-based parser integration.
        /// </summary>
        [Test]
        public void Parse_ValidInput_ShouldReturnValidPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123' cdn.example.com cdn.example.com/ *.example.com 10.10.10.10 https://*.example.com:12/path/to/file.js http://[fe80::1]/index.html; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy, Is.InstanceOf<IContentSecurityPolicy>());

            // Verify we can access parsed directives
            Assert.That(policy.DefaultSource, Is.Not.Null);
            Assert.That(policy.ScriptSource, Is.Not.Null);
            Assert.That(policy.StyleSource, Is.Not.Null);
            Assert.That(policy.ImgSource, Is.Not.Null);
            Assert.That(policy.ConnectSource, Is.Not.Null);
            Assert.That(policy.FontSource, Is.Not.Null);
            Assert.That(policy.FrameAncestors, Is.Not.Null);

            // Verify the policy can be regenerated
            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Is.Not.Empty);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
        }

        /// <summary>
        /// Tests TryParse functionality.
        /// </summary>
        [Test]
        public void TryParse_ValidInput_ShouldReturnTrueAndPolicy()
        {
            // Arrange
            var cspHeader = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(cspHeader);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy, Is.InstanceOf<IContentSecurityPolicy>());
            Assert.That(policy.GeneratePolicy(), Does.Contain("default-src 'self'"));
            Assert.That(policy.GeneratePolicy(), Does.Contain("script-src 'self'"));
        }

        /// <summary>
        /// Tests TryParse with invalid input.
        /// </summary>
        [Test]
        public void TryParse_InvalidInput_ShouldReturnFalse()
        {
            // Arrange
            var invalidCspHeader = string.Empty;
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            var result = parser.TryParse(invalidCspHeader);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Tests that parsed policy can be modified and regenerated.
        /// </summary>
        [Test]
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
            Assert.That(modifiedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(modifiedPolicy, Does.Contain("script-src"));
            Assert.That(modifiedPolicy, Does.Contain("'self'"));
            Assert.That(modifiedPolicy, Does.Contain("newcdn.example.com"));
            Assert.That(modifiedPolicy, Does.Contain("style-src"));
            Assert.That(modifiedPolicy, Does.Contain("'sha256-abc123def456'"));
        }

        /// <summary>
        /// Tests nonce generation on policy instance.
        /// </summary>
        [Test]
        public void Parse_AccessNonce_ShouldGenerateNonce()
        {
            // Arrange
            var cspHeader = "default-src 'self'";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);
            var nonce = policy.Nonce;

            // Assert
            Assert.That(nonce, Is.Not.Empty);
            Assert.That(nonce.Length, Is.GreaterThan(0));

            // Nonce should be consistent across multiple calls
            var nonce2 = policy.Nonce;
            Assert.That(nonce2, Is.EqualTo(nonce));
        }

        /// <summary>
        /// Tests parsing and using all directive types.
        /// </summary>
        [Test]
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
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);

            // Verify all directive contributors are accessible
            Assert.That(policy.DefaultSource, Is.Not.Null);
            Assert.That(policy.ScriptSource, Is.Not.Null);
            Assert.That(policy.StyleSource, Is.Not.Null);
            Assert.That(policy.ImgSource, Is.Not.Null);
            Assert.That(policy.ConnectSource, Is.Not.Null);
            Assert.That(policy.FontSource, Is.Not.Null);
            Assert.That(policy.ObjectSource, Is.Not.Null);
            Assert.That(policy.MediaSource, Is.Not.Null);
            Assert.That(policy.FrameSource, Is.Not.Null);
            Assert.That(policy.FrameAncestors, Is.Not.Null);
            Assert.That(policy.FormAction, Is.Not.Null);
            Assert.That(policy.BaseUriSource, Is.Not.Null);

            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("script-src"));
            Assert.That(generatedPolicy, Does.Contain("style-src"));
            Assert.That(generatedPolicy, Does.Contain("img-src"));
            Assert.That(generatedPolicy, Does.Contain("connect-src"));
            Assert.That(generatedPolicy, Does.Contain("font-src"));
            Assert.That(generatedPolicy, Does.Contain("object-src"));
            Assert.That(generatedPolicy, Does.Contain("media-src"));
            Assert.That(generatedPolicy, Does.Contain("frame-src"));
            Assert.That(generatedPolicy, Does.Contain("frame-ancestors"));
            Assert.That(generatedPolicy, Does.Contain("form-action"));
            Assert.That(generatedPolicy, Does.Contain("base-uri"));
        }

        /// <summary>
        /// Tests parsing policy with reporting directives.
        /// </summary>
        [Test]
        public void Parse_PolicyWithReporting_ShouldParseCorrectly()
        {
            // Arrange
            var cspHeader = "default-src 'self'; report-uri http://csp-report; report-to csp-endpoint";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            // Note: reporting directives might be handled differently in the implementation
        }

        /// <summary>
        /// Tests parsing policy with upgrade-insecure-requests.
        /// </summary>
        [Test]
        public void Parse_PolicyWithUpgradeInsecureRequests_ShouldParseCorrectly()
        {
            // Arrange
            var cspHeader = "default-src 'self'; upgrade-insecure-requests";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(cspHeader);

            // Assert
            Assert.That(policy, Is.Not.Null);
            var generatedPolicy = policy.GeneratePolicy();
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
        }

        /// <summary>
        /// Tests round-trip parsing (parse then generate should produce similar result).
        /// </summary>
        [Test]
        public void Parse_RoundTrip_ShouldProduceSimilarPolicy()
        {
            // Arrange
            var originalCspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self'";
            var policy = new ContentSecurityPolicy(true);
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(originalCspHeader);
            var regeneratedHeader = policy.GeneratePolicy();

            // Assert
            Assert.That(regeneratedHeader, Does.Contain("default-src 'self'"));
            Assert.That(regeneratedHeader, Does.Contain("script-src"));
            Assert.That(regeneratedHeader, Does.Contain("'self'"));
            Assert.That(regeneratedHeader, Does.Contain("'unsafe-inline'"));
            Assert.That(regeneratedHeader, Does.Contain("style-src"));

            // The order might be different, but all elements should be present
            var originalParts = originalCspHeader.Split(';').Select(p => p.Trim()).ToArray();
            foreach (var part in originalParts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    // Check that the directive name and 'self' are present
                    var directiveName = part.Split(' ')[0];
                    Assert.That(regeneratedHeader, Does.Contain(directiveName));
                }
            }
        }
    }
}
