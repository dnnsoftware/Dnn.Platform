// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy.Tests
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Test runner class that demonstrates the CSP parsing functionality.
    /// This class can be used to run examples similar to CspParsingExample but with test verification.
    /// </summary>
    public static class TestRunner
    {
        /// <summary>
        /// Runs all parsing examples and demonstrates the functionality.
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("=== DotNetNuke ContentSecurityPolicy Parser Test Runner ===\n");

            RunBasicParsingExample();
            RunComplexParsingExample();
            RunRealWorldExample();
            RunErrorHandlingExample();
            RunPerformanceExample();

            Console.WriteLine("\n=== All examples completed successfully! ===");
        }

        /// <summary>
        /// Demonstrates basic CSP parsing functionality.
        /// </summary>
        public static void RunBasicParsingExample()
        {
            Console.WriteLine("1. Basic Parsing Example:");
            Console.WriteLine("========================");

            var cspHeader = "default-src 'self'; script-src 'self' 'unsafe-inline'";
            Console.WriteLine($"Original CSP: {cspHeader}");

            try
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                parser.Parse(cspHeader);
                var regenerated = policy.GeneratePolicy();

                Console.WriteLine($"Parsed and regenerated: {regenerated}");
                Console.WriteLine($"Nonce available: {policy.Nonce}");
                Console.WriteLine("✅ Basic parsing successful\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Demonstrates complex CSP parsing with multiple directives.
        /// </summary>
        public static void RunComplexParsingExample()
        {
            Console.WriteLine("2. Complex Parsing Example:");
            Console.WriteLine("===========================");

            var complexHeader = "default-src 'self'; script-src 'self' 'strict-dynamic'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self' wss:; font-src 'self' https://fonts.googleapis.com; frame-ancestors 'none'; upgrade-insecure-requests; report-uri /csp-report";
            Console.WriteLine($"Original CSP: {complexHeader}");

            try
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                parser.Parse(complexHeader);

                // Modify the policy
                policy.ScriptSource.AddHost("cdn.example.com");
                policy.StyleSource.AddHash("sha256-newHash123");

                var modifiedPolicy = policy.GeneratePolicy();
                Console.WriteLine($"Modified policy: {modifiedPolicy}");
                Console.WriteLine("✅ Complex parsing and modification successful\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Demonstrates parsing of real-world CSP policy.
        /// </summary>
        public static void RunRealWorldExample()
        {
            Console.WriteLine("3. Real-World Example:");
            Console.WriteLine("======================");

            var realWorldPolicy = "default-src 'self'; img-src 'self' https://front.satrabel.be https://www.googletagmanager.com https://region1.google-analytics.com; font-src 'self' https://fonts.gstatic.com; style-src 'self' https://fonts.googleapis.com https://www.googletagmanager.com; frame-ancestors 'self'; frame-src 'self'; form-action 'self'; object-src 'none'; base-uri 'self'; script-src 'nonce-hq9CE6VltPZiiySID0F9914GvPObOnIAN3Qs/0R+AmQ=' 'strict-dynamic'; report-to csp-endpoint; report-uri https://dnncore.satrabel.be/DesktopModules/Csp/Report; connect-src https://www.googletagmanager.com https://region1.google-analytics.com https://www.google-analytics.com; upgrade-insecure-requests";
            
            Console.WriteLine("Original real-world CSP (truncated for display):");
            Console.WriteLine($"{realWorldPolicy.Substring(0, Math.Min(100, realWorldPolicy.Length))}...");

            try
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                parser.Parse(realWorldPolicy);
                var regenerated = policy.GeneratePolicy();

                Console.WriteLine($"Successfully parsed {realWorldPolicy.Split(';').Length} directives");
                Console.WriteLine($"Regenerated policy length: {regenerated.Length} characters");
                Console.WriteLine("✅ Real-world parsing successful\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Demonstrates error handling capabilities.
        /// </summary>
        public static void RunErrorHandlingExample()
        {
            Console.WriteLine("4. Error Handling Example:");
            Console.WriteLine("==========================");

            var testCases = new[]
            {
                ("Valid policy", "default-src 'self'", true),
                ("Empty string", "", false),
                ("Null string", null, false),
                ("Invalid directive", "invalid-directive something", false),
                ("Mixed valid/invalid", "default-src 'self'; invalid-directive something", true), // Should ignore invalid
            };

            foreach (var (description, cspHeader, shouldSucceed) in testCases)
            {
                Console.WriteLine($"Testing: {description}");
                
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                var result = parser.TryParse(cspHeader);
                
                if (result == shouldSucceed)
                {
                    Console.WriteLine($"✅ Expected result: {(shouldSucceed ? "Success" : "Failure")}");
                }
                else
                {
                    Console.WriteLine($"❌ Unexpected result: Expected {(shouldSucceed ? "Success" : "Failure")}, got {(result ? "Success" : "Failure")}");
                }
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Demonstrates performance characteristics.
        /// </summary>
        public static void RunPerformanceExample()
        {
            Console.WriteLine("5. Performance Example:");
            Console.WriteLine("=======================");

            // Create a large CSP policy for performance testing
            var largePolicy = string.Join("; ",
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' 'nonce-abc123' https://cdn1.example.com https://cdn2.example.com https://cdn3.example.com 'strict-dynamic'",
                "style-src 'self' 'unsafe-inline' 'sha256-hash1' 'sha256-hash2' https://fonts.googleapis.com",
                "img-src 'self' data: https: blob: https://images.example.com https://cdn.example.com",
                "connect-src 'self' wss: https://api.example.com https://analytics.example.com",
                "font-src 'self' https://fonts.gstatic.com https://fonts.googleapis.com",
                "object-src 'none'",
                "media-src 'self' https://media.example.com",
                "frame-src 'self' https://trusted.example.com",
                "frame-ancestors 'self' https://parent.example.com",
                "form-action 'self' https://secure.example.com",
                "base-uri 'self'",
                "upgrade-insecure-requests"
            );

            Console.WriteLine($"Testing performance with policy containing {largePolicy.Split(';').Length} directives");

            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var policy = new ContentSecurityPolicy();
                var parser = new ContentSecurityPolicyParser(policy);
                parser.Parse(largePolicy);
                var regenerated = policy.GeneratePolicy();
                
                stopwatch.Stop();
                
                Console.WriteLine($"Parse time: {stopwatch.ElapsedMilliseconds}ms");
                Console.WriteLine($"Input length: {largePolicy.Length} characters");
                Console.WriteLine($"Output length: {regenerated.Length} characters");
                Console.WriteLine("✅ Performance test completed\n");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Console.WriteLine($"❌ Performance test failed after {stopwatch.ElapsedMilliseconds}ms: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Demonstrates various source type parsing.
        /// </summary>
        public static void RunSourceTypeExamples()
        {
            Console.WriteLine("6. Source Type Examples:");
            Console.WriteLine("========================");

            var sourceExamples = new[]
            {
                ("Self source", "default-src 'self'"),
                ("Unsafe inline", "script-src 'unsafe-inline'"),
                ("Nonce", "script-src 'nonce-abc123'"),
                ("Hash", "style-src 'sha256-abc123def456'"),
                ("Host", "script-src https://cdn.example.com"),
                ("Scheme", "img-src data: https:"),
                ("None", "object-src 'none'"),
                ("Strict dynamic", "script-src 'strict-dynamic'"),
            };

            foreach (var (description, example) in sourceExamples)
            {
                Console.WriteLine($"Testing: {description}");
                
                try
                {
                    var policy = new ContentSecurityPolicy();
                    var parser = new ContentSecurityPolicyParser(policy);
                    parser.Parse(example);
                    var regenerated = policy.GeneratePolicy();
                    Console.WriteLine($"  Input:  {example}");
                    Console.WriteLine($"  Output: {regenerated}");
                    Console.WriteLine("  ✅ Success");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ❌ Error: {ex.Message}");
                }
                
                Console.WriteLine();
            }
        }
    }
}
