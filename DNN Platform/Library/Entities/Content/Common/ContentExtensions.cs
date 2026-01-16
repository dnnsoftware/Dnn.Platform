// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>Extension methods for Term, Vocabulary, ContentItem.</summary>
    /// <seealso cref="Term"/>
    public static class ContentExtensions
    {
        /// <summary>Toes the delimitted string.</summary>
        /// <param name="terms">The terms.</param>
        /// <param name="delimitter">The delimitter.</param>
        /// <returns>terms' name as a string and split with the given delimitter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string delimitter)
        {
            var sb = new StringBuilder();
            if (terms != null)
            {
                foreach (Term term in terms.OrderBy(term => term.Name))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(term.Name);
                }
            }

            return sb.ToString();
        }

        /// <summary>Toes the delimited string.</summary>
        /// <param name="terms">The terms.</param>
        /// <param name="format">The format.</param>
        /// <param name="delimitter">The delimiter.</param>
        /// <returns> formatted terms' name as a string and split with the given delimiter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string format, string delimitter)
        {
            var sb = new StringBuilder();
            if (terms != null)
            {
                foreach (var term in terms.OrderBy(term => term.Name))
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(string.Format(CultureInfo.InvariantCulture, format, term.Name));
                }
            }

            return sb.ToString();
        }

        /// <summary>Gets the child terms.</summary>
        /// <param name="term1">The term.</param>
        /// <param name="termId">The term id.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection which's parent is the specific term.</returns>
        internal static List<Term> GetChildTerms(this Term term1, int termId, int vocabularyId)
        {
            ITermController ctl = Util.GetTermController();

            IQueryable<Term> terms = from term in ctl.GetTermsByVocabulary(vocabularyId) where term.ParentTermId == termId select term;

            return terms.ToList();
        }

        /// <summary>Gets the vocabulary.</summary>
        /// <param name="term">The term.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>Vocabulary.</returns>
        internal static Vocabulary GetVocabulary(this Term term, int vocabularyId)
        {
            IVocabularyController ctl = Util.GetVocabularyController();

            return (from v in ctl.GetVocabularies() where v.VocabularyId == vocabularyId select v).SingleOrDefault();
        }

        /// <summary>Gets the type of the scope.</summary>
        /// <param name="voc">The voc.</param>
        /// <param name="scopeTypeId">The scope type id.</param>
        /// <returns>scope type.</returns>
        internal static ScopeType GetScopeType(this Vocabulary voc, int scopeTypeId)
        {
            IScopeTypeController ctl = Util.GetScopeTypeController();

            return ctl.GetScopeTypes().Where(s => s.ScopeTypeId == scopeTypeId).SingleOrDefault();
        }

        /// <summary>Gets the terms by vocabularyId.</summary>
        /// <param name="voc">The voc.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this Vocabulary voc, int vocabularyId)
        {
            ITermController ctl = Util.GetTermController();

            return ctl.GetTermsByVocabulary(vocabularyId).ToList();
        }

        /// <summary>Gets the meta data.</summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>meta data collection.</returns>
        internal static NameValueCollection GetMetaData(this ContentItem item, int contentItemId)
        {
            IContentController ctl = Util.GetContentController();

            NameValueCollection metaData;
            if (contentItemId == Null.NullInteger)
            {
                metaData = new NameValueCollection();
            }
            else
            {
                metaData = ctl.GetMetaData(contentItemId);
            }

            return metaData;
        }

        /// <summary>Gets the terms by content item id.</summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this ContentItem item, int contentItemId)
        {
            ITermController ctl = Util.GetTermController();

            List<Term> terms = null;
            if (contentItemId == Null.NullInteger)
            {
                terms = new List<Term>();
            }
            else
            {
                terms = ctl.GetTermsByContent(contentItemId).ToList();
            }

            return terms;
        }
    }
}
