// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System.Linq;

    /// <summary>
    /// Interface of TermController.
    /// </summary>
    /// <seealso cref="TermController"/>
    public interface ITermController
    {
        int AddTerm(Term term);

        void AddTermToContent(Term term, ContentItem contentItem);

        void DeleteTerm(Term term);

        Term GetTerm(int termId);

        TermUsage GetTermUsage(int termId);

        IQueryable<Term> GetTermsByContent(int contentItemId);

        IQueryable<Term> GetTermsByVocabulary(int vocabularyId);

        IQueryable<Term> GetTermsByVocabulary(string vocabularyName);

        void RemoveTermsFromContent(ContentItem contentItem);

        void UpdateTerm(Term term);
    }
}
