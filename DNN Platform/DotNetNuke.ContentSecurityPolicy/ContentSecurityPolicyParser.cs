// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utility class for parsing Content Security Policy headers into ContentSecurityPolicy objects.
    /// </summary>
    public class ContentSecurityPolicyParser
    {
        private readonly IContentSecurityPolicy policy;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentSecurityPolicyParser"/> class.
        /// </summary>
        /// <param name="policy">The ContentSecurityPolicy instance to populate with parsed directives.</param>
        public ContentSecurityPolicyParser(IContentSecurityPolicy policy)
        {
            this.policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        /// <summary>
        /// Parses a CSP header string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <exception cref="ArgumentException">Thrown when the CSP header is invalid or cannot be parsed.</exception>
        public void Parse(string cspHeader)
        {
            if (string.IsNullOrWhiteSpace(cspHeader))
            {
                throw new ArgumentException("CSP header cannot be null or empty", nameof(cspHeader));
            }

            // Split the header into individual directives
            var directives = SplitDirectives(cspHeader);

            foreach (var directive in directives)
            {
                this.ParseDirective(directive);
            }
        }

        /// <summary>
        /// Tries to parse a CSP header string into the provided ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public bool TryParse(string cspHeader)
        {
            try
            {
                this.Parse(cspHeader);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Splits the CSP header into individual directive strings.
        /// </summary>
        private static IEnumerable<string> SplitDirectives(string cspHeader)
        {
            // CSP directives are separated by semicolons
            return cspHeader.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                           .Select(d => d.Trim())
                           .Where(d => !string.IsNullOrEmpty(d));
        }

        /// <summary>
        /// Parses a source string and adds it to the contributor.
        /// </summary>
        private static void ParseAndAddSource(SourceCspContributor contributor, string source)
        {
            var trimmedSource = source.Trim();

            // Check for quoted keywords first
            if (CspSourceTypeNameMapper.IsQuotedKeyword(trimmedSource))
            {
                if (CspSourceTypeNameMapper.TryGetSourceType(trimmedSource, out var sourceType))
                {
                    switch (sourceType)
                    {
                        case CspSourceType.Self:
                            contributor.AddSelf();
                            break;

                        case CspSourceType.Inline:
                            contributor.AddInline();
                            break;

                        case CspSourceType.Eval:
                            contributor.AddEval();
                            break;

                        case CspSourceType.None:
                            contributor.AddNone();
                            break;

                        case CspSourceType.StrictDynamic:
                            contributor.AddStrictDynamic();
                            break;
                    }
                }
                else if (CspSourceTypeNameMapper.IsNonceSource(trimmedSource))
                {
                    var quotedValue = trimmedSource.Substring(1, trimmedSource.Length - 2);
                    var nonce = quotedValue.Substring(6); // Remove "nonce-" prefix
                    contributor.AddNonce(nonce);
                }
                else if (CspSourceTypeNameMapper.IsHashSource(trimmedSource))
                {
                    var quotedValue = trimmedSource.Substring(1, trimmedSource.Length - 2);
                    contributor.AddHash(quotedValue);
                }
            }
            else if (trimmedSource.Contains(":"))
            {
                // Check if it's a scheme
                if (IsScheme(trimmedSource))
                {
                    contributor.AddScheme(trimmedSource);
                }
                else
                {
                    // Treat as host
                    contributor.AddHost(trimmedSource);
                }
            }
            else
            {
                // Treat as host (domain without protocol)
                contributor.AddHost(trimmedSource);
            }
        }

        /// <summary>
        /// Checks if a string represents a scheme.
        /// </summary>
        private static bool IsScheme(string source)
        {
            string[] knownSchemes = { "http:", "https:", "data:", "blob:", "filesystem:", "wss:", "ws:" };
            return knownSchemes.Contains(source.ToLowerInvariant());
        }

        /// <summary>
        /// Parses a single directive and applies it to the policy.
        /// </summary>
        private void ParseDirective(string directive)
        {
            var parts = directive.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return;
            }

            var directiveName = parts[0].ToLowerInvariant();
            var sources = parts.Skip(1).ToArray();

            // Try to get the directive type from the name
            if (!CspDirectiveNameMapper.TryGetDirectiveType(directiveName, out var directiveType))
            {
                // Unknown directive - ignore for now
                return;
            }

            this.ApplyDirectiveToPolicy(directiveType, sources);
        }

        /// <summary>
        /// Applies a parsed directive to the policy object.
        /// </summary>
        private void ApplyDirectiveToPolicy(CspDirectiveType directiveType, string[] sources)
        {
            switch (directiveType)
            {
                case CspDirectiveType.SandboxDirective:
                    this.policy.AddSandboxDirective(string.Join(" ", sources));
                    break;

                case CspDirectiveType.PluginTypes:
                    foreach (var source in sources)
                    {
                        this.policy.AddPluginTypes(source);
                    }

                    break;

                case CspDirectiveType.UpgradeInsecureRequests:
                    this.policy.UpgradeInsecureRequests();
                    break;

                case CspDirectiveType.ReportUri:
                    foreach (var source in sources)
                    {
                        this.policy.AddReportEndpoint("default", source);
                    }

                    break;

                case CspDirectiveType.ReportTo:
                    foreach (var source in sources)
                    {
                        this.policy.AddReportTo(source);
                    }

                    break;

                // Source-based directives
                default:
                    var contributor = this.GetSourceContributor(directiveType);
                    if (contributor != null)
                    {
                        foreach (var source in sources)
                        {
                            ParseAndAddSource(contributor, source);
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Gets the appropriate source contributor for a directive type.
        /// </summary>
        private SourceCspContributor GetSourceContributor(CspDirectiveType directiveType)
        {
            return directiveType switch
            {
                CspDirectiveType.DefaultSrc => this.policy.DefaultSource,
                CspDirectiveType.ScriptSrc => this.policy.ScriptSource,
                CspDirectiveType.StyleSrc => this.policy.StyleSource,
                CspDirectiveType.ImgSrc => this.policy.ImgSource,
                CspDirectiveType.ConnectSrc => this.policy.ConnectSource,
                CspDirectiveType.FontSrc => this.policy.FontSource,
                CspDirectiveType.ObjectSrc => this.policy.ObjectSource,
                CspDirectiveType.MediaSrc => this.policy.MediaSource,
                CspDirectiveType.FrameSrc => this.policy.FrameSource,
                CspDirectiveType.FrameAncestors => this.policy.FrameAncestors,
                CspDirectiveType.FormAction => this.policy.FormAction,
                CspDirectiveType.BaseUri => this.policy.BaseUriSource,
                _ => null
            };
        }
    }
}
