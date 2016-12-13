#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Linq;
using System.Web;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// The Main Business layer of Taxonomy.
	/// </summary>
	/// <example>
	/// <code lang="C#">
	/// internal static List&lt;Term&gt; GetTerms(this Vocabulary voc, int vocabularyId)
	/// {
	///     ITermController ctl = Util.GetTermController();
	///     return ctl.GetTermsByVocabulary(vocabularyId).ToList();
	/// }
	/// </code>
	/// </example>
    public class TermController : ITermController
    {
        private readonly IDataService _DataService;
        private const string _CacheKey = "Terms_{0}";
        private const CacheItemPriority _CachePriority = CacheItemPriority.Normal;

        private const int _CacheTimeOut = 20;

        #region Constructors

        public TermController() : this(Util.GetDataService())
        {
        }

        public TermController(IDataService dataService)
        {
            _DataService = dataService;
        }

        #endregion

        #region Private Methods

        private object GetTermsCallBack(CacheItemArgs cacheItemArgs)
        {
            var vocabularyId = (int) cacheItemArgs.ParamList[0];
            return CBO.FillQueryable<Term>(_DataService.GetTermsByVocabulary(vocabularyId)).ToList();
        }

        #endregion

        #region Public Methods

		/// <summary>
		/// Adds the term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <returns>term id.</returns>
		/// <exception cref="System.ArgumentNullException">term is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">term.VocabularyId is less than 0.</exception>
		/// <exception cref="System.ArgumentException">term.Name is empty.</exception>
        public int AddTerm(Term term)
        {
            //Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "VocabularyId", term.VocabularyId);
            Requires.PropertyNotNullOrEmpty("term", "Name", term.Name);

		    term.Name = HttpUtility.HtmlEncode(term.Name);
            
            if ((term.IsHeirarchical))
            {
                term.TermId = _DataService.AddHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                term.TermId = _DataService.AddSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            //Clear Cache
            DataCache.RemoveCache(string.Format(_CacheKey, term.VocabularyId));

            return term.TermId;
        }

		/// <summary>
		/// Adds the content of the term to.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <param name="contentItem">The content item.</param>
		/// <exception cref="System.ArgumentNullException">term is null.</exception>
		/// <exception cref="System.ArgumentNullException">content item is null.</exception>
        public void AddTermToContent(Term term, ContentItem contentItem)
        {
            //Argument Contract
            Requires.NotNull("term", term);
            Requires.NotNull("contentItem", contentItem);

            _DataService.AddTermToContent(term, contentItem);
        }

		/// <summary>
		/// Deletes the term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <exception cref="System.ArgumentNullException">term is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">term.TermId is less than 0.</exception>
        public void DeleteTerm(Term term)
        {
            //Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "TermId", term.TermId);

            if ((term.IsHeirarchical))
            {
                _DataService.DeleteHeirarchicalTerm(term);
            }
            else
            {
                _DataService.DeleteSimpleTerm(term);
            }

            //Clear Cache
            DataCache.RemoveCache(string.Format(_CacheKey, term.VocabularyId));
        }

		/// <summary>
		/// Gets the term.
		/// </summary>
		/// <param name="termId">The term id.</param>
		/// <returns>specific term.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">termId is less than 0.</exception>
        public Term GetTerm(int termId)
        {
            //Argument Contract
            Requires.NotNegative("termId", termId);

            return CBO.FillObject<Term>(_DataService.GetTerm(termId));
        }


        /// <summary>
        /// Retrieve usage data for the specified term ID.
        /// </summary>
        /// <param name="termId">Term ID in question</param>
        public TermUsage GetTermUsage(int termId)
        {
            Requires.NotNegative("termId", termId);

            return CBO.FillObject<TermUsage>(_DataService.GetTermUsage(termId));
        }

		/// <summary>
		/// Gets the content of the terms by content item id.
		/// </summary>
		/// <param name="contentItemId">The content item id.</param>
		/// <returns>term collection</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">ContentItemId is less than 0.</exception>
        public IQueryable<Term> GetTermsByContent(int contentItemId)
        {
            //Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

            return CBO.FillQueryable<Term>(_DataService.GetTermsByContent(contentItemId));
        }

		/// <summary>
		/// Gets the terms by vocabulary id.
		/// </summary>
		/// <param name="vocabularyId">The vocabulary id.</param>
		/// <returns>term collection</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">vocabularyId is less than 0.</exception>
        public IQueryable<Term> GetTermsByVocabulary(int vocabularyId)
        {
            //Argument Contract
            Requires.NotNegative("vocabularyId", vocabularyId);

            return CBO.GetCachedObject<List<Term>>(new CacheItemArgs(string.Format(_CacheKey, vocabularyId), _CacheTimeOut, _CachePriority, vocabularyId), GetTermsCallBack).AsQueryable();
        }

		/// <summary>
		/// Gets the terms by vocabulary name.
		/// </summary>
		/// <param name="vocabularyName">Name of the vocabulary.</param>
		/// <returns>term collection</returns>
		/// <exception cref="System.ArgumentException">vocabularyName is empty.</exception>
        public IQueryable<Term> GetTermsByVocabulary(string vocabularyName)
        {
            //Argument Contract
            Requires.NotNullOrEmpty("vocabularyName", vocabularyName);

            IVocabularyController vocabularyController = Util.GetVocabularyController();
            Vocabulary vocabulary = (vocabularyController.GetVocabularies()
                                        .Cast<Vocabulary>().Where(v => v.Name == vocabularyName))
                                    .SingleOrDefault();
            
            if (vocabulary == null)
            {
                throw new ArgumentException("Vocabulary does not exist.", "vocabularyName");
            }

		    return GetTermsByVocabulary(vocabulary.VocabularyId);
        }

		/// <summary>
		/// Removes all terms from content item.
		/// </summary>
		/// <param name="contentItem">The content item.</param>
		/// <exception cref="System.ArgumentNullException">content item is null.</exception>
		public void RemoveTermsFromContent(ContentItem contentItem)
        {
            //Argument Contract
            Requires.NotNull("contentItem", contentItem);

            _DataService.RemoveTermsFromContent(contentItem);
        }

		/// <summary>
		/// Updates the term.
		/// </summary>
		/// <param name="term">The term.</param>
		/// <exception cref="System.ArgumentNullException">term is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">term.TermId is less than 0.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">term.VocabularyId is less than 0.</exception>
		/// <exception cref="System.ArgumentException">term.Name is empty.</exception>
        public void UpdateTerm(Term term)
        {
            //Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "TermId", term.TermId);
            Requires.PropertyNotNegative("term", "Vocabulary.VocabularyId", term.VocabularyId);
            Requires.PropertyNotNullOrEmpty("term", "Name", term.Name);

            if ((term.IsHeirarchical))
            {
                _DataService.UpdateHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                _DataService.UpdateSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            //Clear Cache
            DataCache.RemoveCache(string.Format(_CacheKey, term.VocabularyId));
        }

        #endregion
    }
}