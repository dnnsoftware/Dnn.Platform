// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Users;

    /// <summary>The Main Business layer of Taxonomy.</summary>
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
        private const CacheItemPriority CachePriority = CacheItemPriority.Normal;
        private const int CacheTimeOut = 20;
        private readonly IDataService dataService;

        /// <summary>Initializes a new instance of the <see cref="TermController"/> class.</summary>
        public TermController()
            : this(Util.GetDataService())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="TermController"/> class.</summary>
        /// <param name="dataService">The data service.</param>
        public TermController(IDataService dataService)
        {
            this.dataService = dataService;
        }

        /// <summary>Adds the term.</summary>
        /// <param name="term">The term.</param>
        /// <returns>term id.</returns>
        /// <exception cref="System.ArgumentNullException">term is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">term.VocabularyId is less than 0.</exception>
        /// <exception cref="System.ArgumentException">term.Name is empty.</exception>
        public int AddTerm(Term term)
        {
            // Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "VocabularyId", term.VocabularyId);
            Requires.PropertyNotNullOrEmpty("term", "Name", term.Name);

            term.Name = HttpUtility.HtmlEncode(term.Name);

            if (term.IsHeirarchical)
            {
                term.TermId = this.dataService.AddHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                term.TermId = this.dataService.AddSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // Clear Cache
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.TermCacheKey, term.VocabularyId));

            return term.TermId;
        }

        /// <summary>Adds the content of the term to.</summary>
        /// <param name="term">The term.</param>
        /// <param name="contentItem">The content item.</param>
        /// <exception cref="System.ArgumentNullException">term is null.</exception>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        public void AddTermToContent(Term term, ContentItem contentItem)
        {
            // Argument Contract
            Requires.NotNull("term", term);
            Requires.NotNull("contentItem", contentItem);

            this.dataService.AddTermToContent(term, contentItem);

            // We have adjusted a content item, remove it from cache
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.ContentItemsCacheKey, contentItem.ContentTypeId));
        }

        /// <summary>Deletes the term.</summary>
        /// <param name="term">The term.</param>
        /// <exception cref="System.ArgumentNullException">term is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">term.TermId is less than 0.</exception>
        public void DeleteTerm(Term term)
        {
            // Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "TermId", term.TermId);

            if (term.IsHeirarchical)
            {
                this.dataService.DeleteHeirarchicalTerm(term);
            }
            else
            {
                this.dataService.DeleteSimpleTerm(term);
            }

            // Clear Cache
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.TermCacheKey, term.VocabularyId));
        }

        /// <summary>Gets the term.</summary>
        /// <param name="termId">The term id.</param>
        /// <returns>specific term.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">termId is less than 0.</exception>
        public Term GetTerm(int termId)
        {
            // Argument Contract
            Requires.NotNegative("termId", termId);

            return CBO.FillObject<Term>(this.dataService.GetTerm(termId));
        }

        /// <summary>Retrieve usage data for the specified term ID.</summary>
        /// <param name="termId">Term ID in question.</param>
        /// <returns>A <see cref="TermUsage"/> instance or <see langword="null"/>.</returns>
        public TermUsage GetTermUsage(int termId)
        {
            Requires.NotNegative("termId", termId);

            return CBO.FillObject<TermUsage>(this.dataService.GetTermUsage(termId));
        }

        /// <summary>Gets the content of the terms by content item id.</summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>term collection.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">ContentItemId is less than 0.</exception>
        public IQueryable<Term> GetTermsByContent(int contentItemId)
        {
            // Argument Contract
            Requires.NotNegative("contentItemId", contentItemId);

            return CBO.FillQueryable<Term>(this.dataService.GetTermsByContent(contentItemId));
        }

        /// <summary>Gets the terms by vocabulary id.</summary>
        /// <param name="vocabularyId">The vocabulary id.</param>
        /// <returns>term collection.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">vocabularyId is less than 0.</exception>
        public IQueryable<Term> GetTermsByVocabulary(int vocabularyId)
        {
            // Argument Contract
            Requires.NotNegative("vocabularyId", vocabularyId);

            return CBO.GetCachedObject<List<Term>>(new CacheItemArgs(string.Format(CultureInfo.InvariantCulture, DataCache.TermCacheKey, vocabularyId), CacheTimeOut, CachePriority, vocabularyId), this.GetTermsCallBack).AsQueryable();
        }

        /// <summary>Gets the terms by vocabulary name.</summary>
        /// <param name="vocabularyName">Name of the vocabulary.</param>
        /// <returns>term collection.</returns>
        /// <exception cref="System.ArgumentException">vocabularyName is empty.</exception>
        public IQueryable<Term> GetTermsByVocabulary(string vocabularyName)
        {
            // Argument Contract
            Requires.NotNullOrEmpty("vocabularyName", vocabularyName);

            IVocabularyController vocabularyController = Util.GetVocabularyController();
            Vocabulary vocabulary = vocabularyController.GetVocabularies()
                                        .Cast<Vocabulary>().Where(v => v.Name == vocabularyName)
                                    .SingleOrDefault();

            if (vocabulary == null)
            {
                throw new ArgumentException("Vocabulary does not exist.", nameof(vocabularyName));
            }

            return this.GetTermsByVocabulary(vocabulary.VocabularyId);
        }

        /// <summary>Removes all terms from content item.</summary>
        /// <param name="contentItem">The content item.</param>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        public void RemoveTermsFromContent(ContentItem contentItem)
        {
            // Argument Contract
            Requires.NotNull("contentItem", contentItem);

            this.dataService.RemoveTermsFromContent(contentItem);

            // We have adjusted a content item, remove it from cache
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.ContentItemsCacheKey, contentItem.ContentTypeId));
        }

        /// <summary>Updates the term.</summary>
        /// <param name="term">The term.</param>
        /// <exception cref="System.ArgumentNullException">term is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">term.TermId is less than 0.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">term.VocabularyId is less than 0.</exception>
        /// <exception cref="System.ArgumentException">term.Name is empty.</exception>
        public void UpdateTerm(Term term)
        {
            // Argument Contract
            Requires.NotNull("term", term);
            Requires.PropertyNotNegative("term", "TermId", term.TermId);
            Requires.PropertyNotNegative("term", "Vocabulary.VocabularyId", term.VocabularyId);
            Requires.PropertyNotNullOrEmpty("term", "Name", term.Name);

            if (term.IsHeirarchical)
            {
                this.dataService.UpdateHeirarchicalTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }
            else
            {
                this.dataService.UpdateSimpleTerm(term, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // Clear Cache
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, DataCache.TermCacheKey, term.VocabularyId));
        }

        private object GetTermsCallBack(CacheItemArgs cacheItemArgs)
        {
            var vocabularyId = (int)cacheItemArgs.ParamList[0];
            return CBO.FillQueryable<Term>(this.dataService.GetTermsByVocabulary(vocabularyId)).ToList();
        }
    }
}
