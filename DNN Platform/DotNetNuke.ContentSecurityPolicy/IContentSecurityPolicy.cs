// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    using System;

    /// <summary>
    /// Interface définissant les opérations de gestion de la Content Security Policy.
    /// </summary>
    public interface IContentSecurityPolicy
    {
        /// <summary>
        /// Gets a cryptographically secure nonce value for the CSP policy.
        /// </summary>
        string Nonce { get; }

        /// <summary>
        /// Gets the default source contributor.
        /// </summary>
        SourceCspContributor DefaultSource { get; }

        /// <summary>
        /// Gets the script source contributor.
        /// </summary>
        SourceCspContributor ScriptSource { get; }

        /// <summary>
        /// Gets the style source contributor.
        /// </summary>
        SourceCspContributor StyleSource { get; }

        /// <summary>
        /// Gets the image source contributor.
        /// </summary>
        SourceCspContributor ImgSource { get; }

        /// <summary>
        /// Gets the connect source contributor.
        /// </summary>
        SourceCspContributor ConnectSource { get; }

        /// <summary>
        /// Gets the font source contributor.
        /// </summary>
        SourceCspContributor FontSource { get; }

        /// <summary>
        /// Gets the object source contributor.
        /// </summary>
        SourceCspContributor ObjectSource { get; }

        /// <summary>
        /// Gets the media source contributor.
        /// </summary>
        SourceCspContributor MediaSource { get; }

        /// <summary>
        /// Gets the frame source contributor.
        /// </summary>
        SourceCspContributor FrameSource { get; }

        /// <summary>
        /// Gets the frame ancestors contributor.
        /// </summary>
        SourceCspContributor FrameAncestors { get; }

        /// <summary>
        /// Gets the Form action source contributor.
        /// </summary>
        SourceCspContributor FormAction { get; }

        /// <summary>
        /// Gets the base URI source contributor.
        /// </summary>
        SourceCspContributor BaseUriSource { get; }

        /// <summary>
        /// Supprimer une source de script à la politique.
        /// </summary>
        /// <param name="cspSourceType">Le type de source CSP à supprimer.</param>
        void RemoveScriptSources(CspSourceType cspSourceType);

        /// <summary>
        /// Ajoute des types de plugins à la politique.
        /// </summary>
        /// <param name="value">Le type de plugin à autoriser.</param>
        void AddPluginTypes(string value);

        /// <summary>
        /// Ajoute une directive sandbox à la politique.
        /// </summary>
        /// <param name="value">Les options de la directive sandbox.</param>
        void AddSandboxDirective(string value);

        /// <summary>
        /// Ajoute une action de formulaire à la politique.
        /// </summary>
        /// <param name="sourceType">Le type de source CSP à ajouter.</param>
        /// <param name="value">L'URL autorisée pour la soumission du formulaire.</param>
        void AddFormAction(CspSourceType sourceType, string value);

        /// <summary>
        /// Ajoute des ancêtres de frame à la politique.
        /// </summary>
        /// <param name="sourceType">Le type de source CSP à ajouter.</param>
        /// <param name="value">L'URL autorisée comme ancêtre de frame.</param>
        void AddFrameAncestors(CspSourceType sourceType, string value);

        /// <summary>
        /// Ajoute une URI de rapport à la politique.
        /// </summary>
        /// <param name="name">Le nom où les rapports de violation seront envoyés.</param>
        /// <param name="value">L'URI où les rapports de violation seront envoyés.</param>
        public void AddReportEndpoint(string name, string value);

        /// <summary>
        /// Ajoute une destination de rapport à la politique.
        /// </summary>
        /// <param name="value">L'endpoint où envoyer les rapports.</param>
        void AddReportTo(string value);

        /// <summary>
        /// Parses a CSP header string into a ContentSecurityPolicy object.
        /// </summary>
        /// <param name="cspHeader">The CSP header string to parse.</param>
        /// <returns>A ContentSecurityPolicy object representing the parsed header.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the CSP header is invalid or cannot be parsed.</exception>
        IContentSecurityPolicy AddHeaders(string cspHeader);

        /// <summary>
        /// Génère la politique de sécurité complète.
        /// </summary>
        /// <returns>La politique de sécurité complète sous forme de chaîne.</returns>
        string GeneratePolicy();

        /// <summary>
        /// Génère la politique de sécurité complète.
        /// </summary>
        /// <returns>Reporting Endpoints sous forme de chaîne.</returns>
        string GenerateReportingEndpoints();

        /// <summary>
        /// Upgrade Insecure Requests.
        /// </summary>
        void UpgradeInsecureRequests();
    }
}
