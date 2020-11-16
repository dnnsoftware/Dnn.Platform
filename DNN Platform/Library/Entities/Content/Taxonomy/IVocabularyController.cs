// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System.Linq;

    /// <summary>
    /// Interface of VocabularyController.
    /// </summary>
    /// <seealso cref="VocabularyController"/>
    public interface IVocabularyController
    {
        int AddVocabulary(Vocabulary vocabulary);

        void ClearVocabularyCache();

        void DeleteVocabulary(Vocabulary vocabulary);

        IQueryable<Vocabulary> GetVocabularies();

        void UpdateVocabulary(Vocabulary vocabulary);
    }
}
