// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System.Linq;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
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
