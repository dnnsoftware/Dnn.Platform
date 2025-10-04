// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Example class demonstrating how to parse Content Security Policy headers.
    /// </summary>
    public static class CspParsingExample
    {
        /// <summary>
        /// Demonstrates how to parse a CSP header string.
        /// </summary>
        public static void ParseExample()
        {
            // Example CSP header string
            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline' https://cdn.example.com 'nonce-abc123'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self'; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; report-uri /csp-report";
            var policy = new ContentSecurityPolicy();
            try
            {
                // Parse the CSP header
                policy.AddHeader(cspHeader);

                // Access parsed directives
                Console.WriteLine("Parsed CSP Policy:");
                Console.WriteLine($"Generated Policy: {policy.GeneratePolicy()}");
                Console.WriteLine($"Nonce: {policy.Nonce}");

                // You can now modify the parsed policy
                policy.ScriptSource.AddHost("newcdn.example.com");
                policy.StyleSource.AddHash("sha256-abc123def456");

                Console.WriteLine($"Modified Policy: {policy.GeneratePolicy()}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Failed to parse CSP header: {ex.Message}");
            }

            // Example using TryParse
            var invalidCspHeader = "invalid-directive something";
            try
            {
                policy.AddHeader(invalidCspHeader);
                Console.WriteLine("Successfully parsed invalid header");
            }
            catch (Exception)
            {
            Console.WriteLine("Failed to parse invalid header (as expected)");
            }
        }

        /// <summary>
        /// Demonstrates various CSP header formats that can be parsed.
        /// </summary>
        public static void ParseVariousFormats()
        {
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
                "default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri /csp-report",

                // Policy with sandbox
                "sandbox allow-forms allow-scripts; script-src 'self'",

                // Policy with form-action
                "form-action 'self' https://secure.example.com",

                // Policy with report-uri
                "default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests",
            };

            foreach (var example in examples)
            {
                Console.WriteLine($"\nParsing: {example}");
                var policy = new ContentSecurityPolicy();
                try
                {
                    policy.AddHeader(example);
                    Console.WriteLine($"Success: {policy.GeneratePolicy()}");
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to parse");
                }
            }
        }
    }
}
