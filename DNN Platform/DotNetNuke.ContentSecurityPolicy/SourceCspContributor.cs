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
    /// Contributor for fetch directives (sources-based directives).
    /// </summary>
    public class SourceCspContributor : BaseCspContributor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceCspContributor"/> class.
        /// </summary>
        /// <param name="directiveType">The directive type to create the contributor for.</param>
        public SourceCspContributor(CspDirectiveType directiveType)
        {
            this.DirectiveType = directiveType;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the inline source is used for backward compatibility.
        /// </summary>
        public bool InlineForBackwardCompatibility { get; set; }

        /// <summary>
        /// Gets collection of allowed sources.
        /// </summary>
        private List<CspSource> Sources { get; } = new List<CspSource>();

        /// <summary>
        /// Adds a source with inline type to the contributor.
        /// </summary>
        /// <returns>The current instance for method chaining.</returns>
        public SourceCspContributor AddInline()
        {
            return this.AddSource(new CspSource(CspSourceType.Inline));
        }

        /// <summary>
        /// Ajoute une source 'self' qui autorise les ressources de la même origine.
        /// </summary>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddSelf()
        {
            return this.AddSource(new CspSource(CspSourceType.Self));
        }

        /// <summary>
        /// Ajoute une source 'unsafe-eval' qui autorise l'utilisation de eval().
        /// </summary>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddEval()
        {
            return this.AddSource(new CspSource(CspSourceType.Eval));
        }

        /// <summary>
        /// Ajoute un hôte spécifique comme source autorisée.
        /// </summary>
        /// <param name="host">L'hôte à autoriser (ex: example.com).</param>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddHost(string host)
        {
            return this.AddSource(new CspSource(CspSourceType.Host, host));
        }

        /// <summary>
        /// Ajoute un schéma comme source autorisée.
        /// </summary>
        /// <param name="scheme">Le schéma à autoriser (ex: https:, data:).</param>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddScheme(string scheme)
        {
            return this.AddSource(new CspSource(CspSourceType.Scheme, scheme));
        }

        /// <summary>
        /// Ajoute un nonce cryptographique comme source autorisée.
        /// </summary>
        /// <param name="nonce">La valeur du nonce à utiliser.</param>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddNonce(string nonce)
        {
            return this.AddSource(new CspSource(CspSourceType.Nonce, nonce));
        }

        /// <summary>
        /// Ajoute un hash cryptographique comme source autorisée.
        /// </summary>
        /// <param name="hash">La valeur du hash à utiliser.</param>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddHash(string hash)
        {
            return this.AddSource(new CspSource(CspSourceType.Hash, hash));
        }

        /// <summary>
        /// Ajoute une source 'none' qui bloque toutes les sources.
        /// </summary>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddNone()
        {
            return this.AddSource(new CspSource(CspSourceType.None));
        }

        /// <summary>
        /// Ajoute une source 'strict-dynamic' qui active le chargement dynamique strict des scripts.
        /// </summary>
        /// <returns>L'instance courante pour chaîner les méthodes.</returns>
        public SourceCspContributor AddStrictDynamic()
        {
            return this.AddSource(new CspSource(CspSourceType.StrictDynamic));
        }

        /// <summary>
        /// Adds a source to the contributor.
        /// </summary>
        /// <param name="source">The source to add.</param>
        /// <returns>The current instance for method chaining.</returns>
        public SourceCspContributor AddSource(CspSource source)
        {
            if (!this.Sources.Any(s => s.Type == source.Type && s.Value == source.Value))
            {
                this.Sources.Add(source);
            }

            return this;
        }

        /// <summary>
        /// Removes a source from the contributor.
        /// </summary>
        /// <param name="sourceType">The type of the source to remove.</param>
        public void RemoveSources(CspSourceType sourceType)
        {
            this.Sources.RemoveAll(s => s.Type == sourceType);
        }

        /// <summary>
        /// Generates the directive string.
        /// </summary>
        /// <returns>The directive string.</returns>
        public override string GenerateDirective()
        {
            if (!this.Sources.Any())
            {
                return string.Empty;
            }

            if (this.Sources.Any(s => s.Type == CspSourceType.Inline) && !this.InlineForBackwardCompatibility)
            {
                this.RemoveSources(CspSourceType.Nonce);
                this.RemoveSources(CspSourceType.StrictDynamic);
            }

            return $"{CspDirectiveNameMapper.GetDirectiveName(this.DirectiveType)} {string.Join(" ", this.Sources.Select(s => s.ToString()))}";
        }

        /// <summary>
        /// Gets sources by type.
        /// </summary>
        /// <param name="type">The type of sources to get.</param>
        /// <returns>The sources of the specified type.</returns>
        public IEnumerable<CspSource> GetSourcesByType(CspSourceType type)
        {
            return this.Sources.Where(s => s.Type == type);
        }
    }
}
