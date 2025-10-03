// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    /// <summary>
    /// Represents different types of Content Security Policy source types.
    /// </summary>
    public enum CspSourceType
    {
        /// <summary>
        /// Permet de spécifier des domaines spécifiques comme source.
        /// </summary>
        Host,

        /// <summary>
        /// Permet de spécifier des protocoles (ex: https:, data:) comme source.
        /// </summary>
        Scheme,

        /// <summary>
        /// Autorise les ressources de la même origine ('self').
        /// </summary>
        Self,

        /// <summary>
        /// Autorise l'utilisation de code inline ('unsafe-inline').
        /// </summary>
        Inline,

        /// <summary>
        /// Autorise l'utilisation de eval() ('unsafe-eval').
        /// </summary>
        Eval,

        /// <summary>
        /// Utilise un nonce cryptographique pour valider les ressources.
        /// </summary>
        Nonce,

        /// <summary>
        /// Utilise un hash cryptographique pour valider les ressources.
        /// </summary>
        Hash,

        /// <summary>
        /// N'autorise aucune source ('none').
        /// </summary>
        None,

        /// <summary>
        /// Active le mode strict-dynamic pour le chargement des scripts.
        /// </summary>
        StrictDynamic,
    }
}
