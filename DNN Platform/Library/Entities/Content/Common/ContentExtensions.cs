﻿#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Content.Taxonomy;
using DotNetNuke.Services.FileSystem;

#endregion

namespace DotNetNuke.Entities.Content.Common
{
	/// <summary>
	/// Extension methods for Term, Vocabulary, ContentItem.
	/// </summary>
	/// <seealso cref="Term"/>
    public static class ContentExtensions
    {
        #region "Term Extensions"

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
		/// <returns>Vocabulary</returns>
        internal static Vocabulary GetVocabulary(this Term term, int vocabularyId)
        {
            IVocabularyController ctl = Util.GetVocabularyController();

            return (from v in ctl.GetVocabularies() where v.VocabularyId == vocabularyId select v).SingleOrDefault();
        }

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
                foreach (Term _Term in (from term in terms orderby term.Name ascending select term))
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
                foreach (Term _Term in (from term in terms orderby term.Name ascending select term))
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

        #endregion

        #region "Vocabulary Extensions"

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

        #endregion

        #region "ContentItem Extensions"

		/// <summary>
		/// Gets the meta data.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>meta data collection</returns>
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
		/// <returns>term collection</returns>
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

        #endregion
    }
}