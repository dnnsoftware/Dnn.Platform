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
    /// Integration tests that test the complete parsing workflow with real-world examples.
    /// Based on the CspParsingExample class examples.
    /// </summary>
    [TestClass]
    public class IntegrationTests
    {
        /// <summary>
        /// Tests the complete example from CspParsingExample.ParseExample().
        /// </summary>
        [TestMethod]
        public void ParseExample_CompleteWorkflow_ShouldWorkAsExpected()
        {
            // Arrange - Example CSP header string from CspParsingExample
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri http://csp-report";
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act - Parse the CSP header
            parser.Parse(cspHeader);

            // Assert - Access parsed directives
            policy.Should().NotBeNull();
            policy.GeneratePolicy().Should().NotBeNullOrEmpty();
            policy.Nonce.Should().NotBeNullOrEmpty();

            // Modify the parsed policy as shown in the example
            policy.ScriptSource.AddHost("newcdn.example.com");
            policy.StyleSource.AddHash("sha256-abc123def456");

            var modifiedPolicy = policy.GeneratePolicy();
            modifiedPolicy.Should().Contain("newcdn.example.com");
            modifiedPolicy.Should().Contain("'sha256-abc123def456'");
        }

        /// <summary>
        /// Tests all the various formats from CspParsingExample.ParseVariousFormats().
        /// </summary>
        [TestMethod]
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
                result.Should().BeTrue($"Failed to parse: {example}");
                policy.Should().NotBeNull($"Policy should not be null for: {example}");
                policy.GeneratePolicy().Should().NotBeNullOrEmpty($"Generated policy should not be empty for: {example}");
            }
        }

        /// <summary>
        /// Tests parsing and regenerating the real-world complex policy to ensure data integrity.
        /// </summary>
        [TestMethod]
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
            regeneratedPolicy.Should().Contain("default-src 'self'");
            regeneratedPolicy.Should().Contain("img-src");
            regeneratedPolicy.Should().Contain("https://front.satrabel.be");
            regeneratedPolicy.Should().Contain("https://www.googletagmanager.com");
            regeneratedPolicy.Should().Contain("https://region1.google-analytics.com");
            regeneratedPolicy.Should().Contain("font-src");
            regeneratedPolicy.Should().Contain("https://fonts.gstatic.com");
            regeneratedPolicy.Should().Contain("style-src");
            regeneratedPolicy.Should().Contain("https://fonts.googleapis.com");
            regeneratedPolicy.Should().Contain("frame-ancestors 'self'");
            regeneratedPolicy.Should().Contain("frame-src 'self'");
            regeneratedPolicy.Should().Contain("form-action 'self'");
            regeneratedPolicy.Should().Contain("object-src 'none'");
            regeneratedPolicy.Should().Contain("base-uri 'self'");
            regeneratedPolicy.Should().Contain("script-src");
            regeneratedPolicy.Should().Contain("'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ='");
            regeneratedPolicy.Should().Contain("'strict-dynamic'");
            regeneratedPolicy.Should().Contain("connect-src");
            regeneratedPolicy.Should().Contain("https://www.google-analytics.com");
            regeneratedPolicy.Should().Contain("upgrade-insecure-requests");
        }

        /// <summary>
        /// Tests parsing and then extending a policy with additional sources.
        /// </summary>
        [TestMethod]
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
            extendedPolicy.Should().Contain("default-src 'self'");
            extendedPolicy.Should().Contain("script-src");
            extendedPolicy.Should().Contain("'self'");
            extendedPolicy.Should().Contain("cdn.example.com");
            extendedPolicy.Should().Contain("'nonce-newNonce123'");
            extendedPolicy.Should().Contain("style-src");
            extendedPolicy.Should().Contain("fonts.googleapis.com");
            extendedPolicy.Should().Contain("img-src");
            extendedPolicy.Should().Contain("data:");
        }

        /// <summary>
        /// Tests parsing policies with various source combinations.
        /// </summary>
        [TestMethod]
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
                    generatedPolicy.Should().Contain(expectedSource, $"Policy '{policyString}' should contain '{expectedSource}'");
                }
            }
        }

        /// <summary>
        /// Tests that the parser handles edge cases gracefully.
        /// </summary>
        [TestMethod]
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
                    policy.Should().NotBeNull();
                    policy.GeneratePolicy().Should().NotBeNullOrEmpty();
                }
            }
        }

        /// <summary>
        /// Tests performance with a large, complex policy.
        /// </summary>
        [TestMethod]
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
                "upgrade-insecure-requests"
            );
            var policy = new ContentSecurityPolicy();
            var parser = new ContentSecurityPolicyParser(policy);

            // Act - Should parse quickly
            var startTime = DateTime.UtcNow;
            parser.Parse(largePolicy);
            var parseTime = DateTime.UtcNow - startTime;

            var generatedPolicy = policy.GeneratePolicy();

            // Assert - Should complete quickly (less than 1 second)
            parseTime.Should().BeLessThan(TimeSpan.FromSeconds(1));
            generatedPolicy.Should().NotBeNullOrEmpty();
            generatedPolicy.Should().Contain("default-src 'self'");
            generatedPolicy.Should().Contain("upgrade-insecure-requests");
        }
    }
}
