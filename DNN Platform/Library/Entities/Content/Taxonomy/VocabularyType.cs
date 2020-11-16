// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Taxonomy
{
    /// <summary>
    /// Enumeration of VocabularyType.
    /// </summary>
    public enum VocabularyType
    {
        /// <summary>
        /// Simple Vocabulary
        /// </summary>
        Simple = 1,

        /// <summary>
        /// The Vocabulary can have parent or child nodes.
        /// </summary>
        Hierarchy = 2,
    }
}
