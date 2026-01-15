// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    /// <summary>
    /// Integration tests that test the complete parsing workflow with real-world examples.
    /// Based on the CspParsingExample class examples.
    /// </summary>
    [TestFixture]
    public class IntegrationTests
    {
        /// <summary>
        /// Tests the complete example from CspParsingExample.ParseExample().
        /// </summary>
        [Test]
        public void ParseExample_CompleteWorkflow_ShouldWorkAsExpected()
        {
            // Arrange - Example CSP header string from CspParsingExample
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act - Parse the CSP header
            parser.Parse(cspHeader);

            // Assert - Access parsed directives
            Assert.That(policy, Is.Not.Null);
            Assert.That(policy.GeneratePolicy(), Is.Not.Empty);
            Assert.That(policy.Nonce, Is.Not.Empty);

            // Modify the parsed policy as shown in the example
            policy.ScriptSource.AddHost("newcdn.example.com");
            policy.StyleSource.AddHash("sha256-abc123def456");

            var modifiedPolicy = policy.GeneratePolicy();
            Assert.That(modifiedPolicy, Does.Contain("newcdn.example.com"));
            Assert.That(modifiedPolicy, Does.Contain("'sha256-abc123def456'"));
        }

        /// <summary>
        /// Tests all the various formats from CspParsingExample.ParseVariousFormats().
        /// </summary>
        [Test]
        public void ParseVariousFormats_AllExamples_ShouldParseSuccessfully()
        {
            // Arrange - All examples from CspParsingExample.ParseVariousFormats()
            var examples = new[]
            {
                // Basic policy
                "default-src 'self'",

                // Policy with multiple sources
                "script-src 'self' 'unsafe-inline' https://cdn.example.com",

                // Policy with nonce
                "script-src 'self' 'nonce-abc123def456'",

                // Policy with hash
                "style-src 'self' 'sha256-abc123def456789'",

                // Complex policy
                "default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri http://csp-report",

                // Policy with sandbox
                "sandbox allow-forms allow-scripts; script-src 'self'",

                // Policy with form-action
                "form-action 'self' https://secure.example.com",

                // Real-world complex policy with report-uri
                "default-src 'self'; img-src 'self' https://www.dnnsoftware.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests",
            };

            // Act & Assert - Each example should parse successfully
            foreach (var example in examples)
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                var result = parser.TryParse(example);
                Assert.That(result, Is.True, $"Failed to parse: {example}");
                Assert.That(policy, Is.Not.Null, $"Policy should not be null for: {example}");
                Assert.That(policy.GeneratePolicy(), Is.Not.Empty, $"Generated policy should not be empty for: {example}");
            }
        }

        /// <summary>
        /// Tests parsing and regenerating the real-world complex policy to ensure data integrity.
        /// </summary>
        [Test]
        public void ParseComplexRealWorldPolicy_ShouldPreserveAllDirectives()
        {
            // Arrange - Real-world complex policy from the example
            var complexPolicy = "default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act
            parser.Parse(complexPolicy);
            var regeneratedPolicy = policy.GeneratePolicy();

            // Assert - Check that all key elements are preserved
            Assert.That(regeneratedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(regeneratedPolicy, Does.Contain("img-src"));
            Assert.That(regeneratedPolicy, Does.Contain("https://front.satrabel.be"));
            Assert.That(regeneratedPolicy, Does.Contain("https://www.googletagmanager.com"));
            Assert.That(regeneratedPolicy, Does.Contain("https://region1.google-analytics.com"));
            Assert.That(regeneratedPolicy, Does.Contain("font-src"));
            Assert.That(regeneratedPolicy, Does.Contain("https://fonts.gstatic.com"));
            Assert.That(regeneratedPolicy, Does.Contain("style-src"));
            Assert.That(regeneratedPolicy, Does.Contain("https://fonts.googleapis.com"));
            Assert.That(regeneratedPolicy, Does.Contain("frame-ancestors 'self'"));
            Assert.That(regeneratedPolicy, Does.Contain("frame-src 'self'"));
            Assert.That(regeneratedPolicy, Does.Contain("form-action 'self'"));
            Assert.That(regeneratedPolicy, Does.Contain("object-src 'none'"));
            Assert.That(regeneratedPolicy, Does.Contain("base-uri 'self'"));
            Assert.That(regeneratedPolicy, Does.Contain("script-src"));
            Assert.That(regeneratedPolicy, Does.Contain("'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ='"));
            Assert.That(regeneratedPolicy, Does.Contain("'strict-dynamic'"));
            Assert.That(regeneratedPolicy, Does.Contain("connect-src"));
            Assert.That(regeneratedPolicy, Does.Contain("https://www.google-analytics.com"));
            Assert.That(regeneratedPolicy, Does.Contain("upgrade-insecure-requests"));
        }

        /// <summary>
        /// Tests parsing and then extending a policy with additional sources.
        /// </summary>
        [Test]
        public void ParseAndExtendPolicy_ShouldWorkCorrectly()
        {
            // Arrange
            var originalPolicy = "default-src 'self'; script-src 'self'";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act - Parse and extend
            parser.Parse(originalPolicy);

            // Add new sources
            policy.ScriptSource.AddHost("cdn.example.com");
            policy.ScriptSource.AddNonce("newNonce123");
            policy.StyleSource.AddSelf();
            policy.StyleSource.AddHost("fonts.googleapis.com");
            policy.ImgSource.AddSelf();
            policy.ImgSource.AddScheme("data:");

            var extendedPolicy = policy.GeneratePolicy();

            // Assert
            Assert.That(extendedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(extendedPolicy, Does.Contain("script-src"));
            Assert.That(extendedPolicy, Does.Contain("'self'"));
            Assert.That(extendedPolicy, Does.Contain("cdn.example.com"));
            Assert.That(extendedPolicy, Does.Contain("'nonce-newNonce123'"));
            Assert.That(extendedPolicy, Does.Contain("style-src"));
            Assert.That(extendedPolicy, Does.Contain("fonts.googleapis.com"));
            Assert.That(extendedPolicy, Does.Contain("img-src"));
            Assert.That(extendedPolicy, Does.Contain("data:"));
        }

        /// <summary>
        /// Tests parsing policies with various source combinations.
        /// </summary>
        [Test]
        public void ParsePoliciesWithVariousSourceCombinations_ShouldHandleAllCorrectly()
        {
            // Arrange - Various source combinations
            var testCases = new[]
            {
                ("script-src 'self' 'unsafe-inline' 'unsafe-eval'", new[] { "'self'", "'unsafe-inline'", "'unsafe-eval'" }),
                ("style-src 'self' 'unsafe-inline' 'sha256-abc123'", new[] { "'self'", "'unsafe-inline'", "'sha256-abc123'" }),
                ("img-src 'self' data: https: blob:", new[] { "'self'", "data:", "https:", "blob:" }),
                ("script-src 'self' https://cdn.example.com 'nonce-xyz789' 'strict-dynamic'", new[] { "'self'", "https://cdn.example.com", "'nonce-xyz789'", "'strict-dynamic'" }),
                ("connect-src 'self' wss: https://api.example.com", new[] { "'self'", "wss:", "https://api.example.com" }),
                ("font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com", new[] { "'self'", "https://fonts.gstatic.com", "https://fonts.googleapis.com" }),
            };

            // Act & Assert
            foreach (var (policyString, expectedSources) in testCases)
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                parser.Parse(policyString);
                var generatedPolicy = policy.GeneratePolicy();

                foreach (var expectedSource in expectedSources)
                {
                    Assert.That(generatedPolicy, Does.Contain(expectedSource), $"Policy '{policyString}' should contain '{expectedSource}'");
                }
            }
        }

        /// <summary>
        /// Tests that the parser handles edge cases gracefully.
        /// </summary>
        [Test]
        public void ParseEdgeCases_ShouldHandleGracefully()
        {
            // Test cases that should parse successfully even with unusual formatting
            var edgeCases = new[]
            {
                // Extra spaces
                "default-src  'self'   ;   script-src   'self'  ",

                // Single directive
                "default-src 'self'",

                // Empty directive values (should be handled gracefully)
                "default-src 'self'; ; script-src 'self'",

                // Mixed case (should work due to case-insensitive parsing)
                "DEFAULT-SRC 'self'; Script-Src 'self'",
            };

            foreach (var edgeCase in edgeCases)
            {
                // Should not throw exceptions
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                var result = parser.TryParse(edgeCase);
                if (result)
                {
                    Assert.That(policy, Is.Not.Null);
                    Assert.That(policy.GeneratePolicy(), Is.Not.Empty);
                }
            }
        }

        /// <summary>
        /// Tests performance with a large, complex policy.
        /// </summary>
        [Test]
        public void ParseLargeComplexPolicy_ShouldPerformWell()
        {
            // Arrange - Large policy with many directives and sources
            var largePolicy = string.Join("; ",
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' 'nonce-abc123' https://cdn1.example.com https://cdn2.example.com https://cdn3.example.com 'strict-dynamic'",
                "style-src 'self' 'unsafe-inline' 'sha256-hash1' 'sha256-hash2' https://fonts.googleapis.com https://cdn.example.com",
                "img-src 'self' data: https: blob: https://images.example.com https://cdn.example.com https://assets.example.com",
                "connect-src 'self' wss: https://api.example.com https://analytics.example.com https://tracking.example.com",
                "font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com https://cdn.example.com",
                "object-src 'none'",
                "media-src 'self' https://media.example.com",
                "frame-src 'self' https://trusted.example.com",
                "frame-ancestors 'self' https://parent.example.com",
                "form-action 'self' https://secure.example.com",
                "base-uri 'self'",
                "upgrade-insecure-requests");
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act - Should parse quickly
            var startTime = DateTime.UtcNow;
            parser.Parse(largePolicy);
            var parseTime = DateTime.UtcNow - startTime;

            var generatedPolicy = policy.GeneratePolicy();

            // Assert - Should complete quickly (less than 1 second)
            Assert.That(parseTime, Is.LessThan(TimeSpan.FromSeconds(1)));
            Assert.That(generatedPolicy, Is.Not.Empty);
            Assert.That(generatedPolicy, Does.Contain("default-src 'self'"));
            Assert.That(generatedPolicy, Does.Contain("upgrade-insecure-requests"));
        }
    }
}
