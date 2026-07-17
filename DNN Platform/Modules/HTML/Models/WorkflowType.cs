// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html.Models
{
    /// <summary>
    /// Définit les différents types de workflows disponibles pour la gestion du contenu HTML.
    /// </summary>
    public enum WorkflowType
    {
        /// <summary>
        /// Le contenu est publié directement sans validation intermédiaire.
        /// </summary>
        DirectPublish = 1,

        /// <summary>
        /// Le contenu est sauvegardé comme brouillon avant publication.
        /// </summary>
        SaveDraft = 2,

        /// <summary>
        /// Le contenu doit passer par un processus d'approbation avant publication.
        /// </summary>
        ContentApproval = 3,
    }
}
