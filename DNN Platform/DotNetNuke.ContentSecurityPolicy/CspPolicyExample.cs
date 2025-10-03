// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Démontre l'utilisation de la Content Security Policy en configurant différentes directives.
    /// </summary>
    public class CspPolicyExample
    {
        /// <summary>
        /// Démontre l'utilisation de la Content Security Policy en configurant différentes directives.
        /// </summary>
        public static void Example()
        {
            // Create a Content Security Policy
            var csp = new ContentSecurityPolicy();

            // Add a source-based contributor for script sources
            csp.ScriptSource
                    .AddSelf()
                    .AddHost("https://trusted-cdn.com");

            // Add a document-based contributor for sandbox
            csp.AddSandboxDirective("allow-scripts allow-same-origin");

            // Add a reporting contributor
            csp.AddReportEndpoint("name", "https://example.com/csp-report");

            // Generate the complete policy
            string policy = csp.GeneratePolicy();

            Console.WriteLine(policy);
        }
    }
}
