// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;

    public class TermHelper
    {
        public const string PageTagsVocabulary = "PageTags";

        public static List<Term> ToTabTerms(string pageSettingsTags, int tabPortalId)
        {
            var terms = new List<Term>();

            if (string.IsNullOrEmpty(pageSettingsTags))
            {
                return terms;
            }

            var termController = new TermController();
            var vocabularyController = Util.GetVocabularyController();
            var vocabulary = vocabularyController.GetVocabularies()
                                .Cast<Vocabulary>()
                                .Where(v => v.Name == PageTagsVocabulary && v.ScopeId == tabPortalId)
                                .SingleOrDefault();

            var vocabularyId = Null.NullInteger;
            if (vocabulary == null)
            {
                var scopeType = Util.GetScopeTypeController().GetScopeTypes().SingleOrDefault(s => s.ScopeType == "Portal");
                if (scopeType == null)
                {
                    throw new Exception("Can't create default vocabulary as scope type 'Portal' can't finded.");
                }

                vocabularyId = vocabularyController.AddVocabulary(
                    new Vocabulary(PageTagsVocabulary, string.Empty, VocabularyType.Simple)
                    {
                        ScopeTypeId = scopeType.ScopeTypeId,
                        ScopeId = tabPortalId,
                    });
            }
            else
            {
                vocabularyId = vocabulary.VocabularyId;
            }

            // get all terms info
            var allTerms = new List<Term>();
            var vocabularies = from v in vocabularyController.GetVocabularies()
                               where v.ScopeType.ScopeType == "Portal" && v.ScopeId == tabPortalId && !v.Name.Equals("Tags", StringComparison.OrdinalIgnoreCase)
                               select v;
            foreach (var v in vocabularies)
            {
                allTerms.AddRange(termController.GetTermsByVocabulary(v.VocabularyId));
            }

            foreach (var tag in pageSettingsTags.Trim().Split(','))
            {
                if (!string.IsNullOrEmpty(tag))
                {
                    var term = allTerms.FirstOrDefault(t => t.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                    if (term == null)
                    {
                        var termId = termController.AddTerm(new Term(tag, string.Empty, vocabularyId));
                        term = termController.GetTerm(termId);
                    }

                    terms.Add(term);
                }
            }

            return terms;
        }
    }
}
