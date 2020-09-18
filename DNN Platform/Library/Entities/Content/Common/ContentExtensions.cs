// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;

namespace DotNetNuke.Entities.Content
{
    [Obsolete("Moving ContentExtensions to the DotNetNuke.Entities.Content namespace was an error. Please use DotNetNuke.Entities.Content.Common.ContentExtensions. Scheduled removal in v10.0.0.")]
    public static class ContentExtensions
    {
        // only forwarding public methods that existed as of 6.1.0
        // calls to internal methods will be fixed in the source
        public static string ToDelimittedString(this List<Term> terms, string delimiter)
        {
            return Common.ContentExtensions.ToDelimittedString(terms, delimiter);
        }

        public static string ToDelimittedString(this List<Term> terms, string format, string delimiter)
        {
            return Common.ContentExtensions.ToDelimittedString(terms, format, delimiter);
        }
    }
}

namespace DotNetNuke.Entities.Content.Common
{
    /// <summary>
    /// Extension methods for Term, Vocabulary, ContentItem.
    /// </summary>
    /// <seealso cref="Term"/>
    public static class ContentExtensions
    {
        /// <summary>
        /// Toes the delimitted string.
        /// </summary>
        /// <param name="terms">The terms.</param>
        /// <param name="delimitter">The delimitter.</param>
        /// <returns>terms' name as a string and split with the given delimitter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string delimitter)
        {
            var sb = new StringBuilder();
            if (terms != null)
            {
                foreach (Term _Term in from term in terms orderby term.Name ascending select term)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(_Term.Name);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Toes the delimitted string.
        /// </summary>
        /// <param name="terms">The terms.</param>
        /// <param name="format">The format.</param>
        /// <param name="delimitter">The delimitter.</param>
        /// <returns> formatted terms' name as a string and split with the given delimitter order by name A-Z.</returns>
        public static string ToDelimittedString(this List<Term> terms, string format, string delimitter)
        {
            var sb = new StringBuilder();
            if (terms != null)
            {
                foreach (Term _Term in from term in terms orderby term.Name ascending select term)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(delimitter);
                    }

                    sb.Append(string.Format(format, _Term.Name));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the child terms.
        /// </summary>
        /// <param name="Term">The term.</param>
        /// <param name="termId">The term id.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection which's parent is the specific term.</returns>
        internal static List<Term> GetChildTerms(this Term Term, int termId, int vocabularyId)
        {
            ITermController ctl = Util.GetTermController();

            IQueryable<Term> terms = from term in ctl.GetTermsByVocabulary(vocabularyId) where term.ParentTermId == termId select term;

            return terms.ToList();
        }

        /// <summary>
        /// Gets the vocabulary.
        /// </summary>
        /// <param name="term">The term.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>Vocabulary.</returns>
        internal static Vocabulary GetVocabulary(this Term term, int vocabularyId)
        {
            IVocabularyController ctl = Util.GetVocabularyController();

            return (from v in ctl.GetVocabularies() where v.VocabularyId == vocabularyId select v).SingleOrDefault();
        }

        /// <summary>
        /// Gets the type of the scope.
        /// </summary>
        /// <param name="voc">The voc.</param>
        /// <param name="scopeTypeId">The scope type id.</param>
        /// <returns>scope type.</returns>
        internal static ScopeType GetScopeType(this Vocabulary voc, int scopeTypeId)
        {
            IScopeTypeController ctl = Util.GetScopeTypeController();

            return ctl.GetScopeTypes().Where(s => s.ScopeTypeId == scopeTypeId).SingleOrDefault();
        }

        /// <summary>
        /// Gets the terms by vocabularyId.
        /// </summary>
        /// <param name="voc">The voc.</param>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this Vocabulary voc, int vocabularyId)
        {
            ITermController ctl = Util.GetTermController();

            return ctl.GetTermsByVocabulary(vocabularyId).ToList();
        }

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>meta data collection.</returns>
        internal static NameValueCollection GetMetaData(this ContentItem item, int contentItemId)
        {
            IContentController ctl = Util.GetContentController();

            NameValueCollection _MetaData;
            if (contentItemId == Null.NullInteger)
            {
                _MetaData = new NameValueCollection();
            }
            else
            {
                _MetaData = ctl.GetMetaData(contentItemId);
            }

            return _MetaData;
        }

        /// <summary>
        /// Gets the terms by content item id.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>term collection.</returns>
        internal static List<Term> GetTerms(this ContentItem item, int contentItemId)
        {
            ITermController ctl = Util.GetTermController();

            List<Term> _Terms = null;
            if (contentItemId == Null.NullInteger)
            {
                _Terms = new List<Term>();
            }
            else
            {
                _Terms = ctl.GetTermsByContent(contentItemId).ToList();
            }

            return _Terms;
        }
    }
}
