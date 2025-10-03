// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ContentSecurityPolicy
{
    /// <summary>
    /// Represents different types of Content Security Policy directives.
    /// </summary>
    public enum CspDirectiveType
    {
        /// <summary>
        /// Directive qui définit la politique par défaut pour les types de ressources non spécifiés.
        /// </summary>
        DefaultSrc,

        /// <summary>
        /// Directive qui contrôle les sources de scripts autorisées.
        /// </summary>
        ScriptSrc,

        /// <summary>
        /// Directive qui contrôle les sources de styles autorisées.
        /// </summary>
        StyleSrc,

        /// <summary>
        /// Directive qui contrôle les sources d'images autorisées.
        /// </summary>
        ImgSrc,

        /// <summary>
        /// Directive qui contrôle les destinations de connexion autorisées.
        /// </summary>
        ConnectSrc,

        /// <summary>
        /// Directive qui contrôle les sources de polices autorisées.
        /// </summary>
        FontSrc,

        /// <summary>
        /// Directive qui contrôle les sources d'objets autorisées.
        /// </summary>
        ObjectSrc,

        /// <summary>
        /// Directive qui contrôle les sources de médias autorisées.
        /// </summary>
        MediaSrc,

        /// <summary>
        /// Directive qui contrôle les sources de frames autorisées.
        /// </summary>
        FrameSrc,

        /// <summary>
        /// Directive qui restreint les URLs pouvant être utilisées dans la base URI du document.
        /// </summary>
        BaseUri,

        /// <summary>
        /// Directive qui restreint les types de plugins pouvant être chargés.
        /// </summary>
        PluginTypes,

        /// <summary>
        /// Directive qui active un bac à sable pour la ressource demandée.
        /// </summary>
        SandboxDirective,

        /// <summary>
        /// Directive qui restreint les URLs pouvant être utilisées comme cible de formulaire.
        /// </summary>
        FormAction,

        /// <summary>
        /// Directive qui spécifie les parents autorisés à intégrer une page dans un frame.
        /// </summary>
        FrameAncestors,

        /// <summary>
        /// Directive qui spécifie l'URI où envoyer les rapports de violation.
        /// </summary>
        ReportUri,

        /// <summary>
        /// Directive qui spécifie où envoyer les rapports de violation au format JSON.
        /// </summary>
        ReportTo,

        /// <summary>
        /// Directive qui spécifie UpgradeInsecureRequests.
        /// </summary>
        UpgradeInsecureRequests,
}
}
