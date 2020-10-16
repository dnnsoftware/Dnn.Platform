﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content.Taxonomy
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;
    using DotNetNuke.Entities.Users;

    /// <summary>
    /// VocabularyController provides the business layer of Vocabulary and VocabularyType.
    /// </summary>
    /// <seealso cref="TermController"/>
    public class VocabularyController : IVocabularyController
    {
        private const int _CacheTimeOut = 20;
        private readonly IDataService _DataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="VocabularyController"/> class.
        /// </summary>
        public VocabularyController()
            : this(Util.GetDataService())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VocabularyController"/> class.
        /// </summary>
        /// <param name="dataService"></param>
        public VocabularyController(IDataService dataService)
        {
            this._DataService = dataService;
        }

        /// <inheritdoc/>
        public int AddVocabulary(Vocabulary vocabulary)
        {
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);
            Requires.PropertyNotNegative("vocabulary", "ScopeTypeId", vocabulary.ScopeTypeId);

            vocabulary.VocabularyId = this._DataService.AddVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

            return vocabulary.VocabularyId;
        }

        /// <inheritdoc/>
        public void ClearVocabularyCache()
        {
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
        }

        /// <inheritdoc/>
        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);

            this._DataService.DeleteVocabulary(vocabulary);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
        }

        /// <inheritdoc/>
        public IQueryable<Vocabulary> GetVocabularies()
        {
            return CBO.GetCachedObject<List<Vocabulary>>(new CacheItemArgs(DataCache.VocabularyCacheKey, _CacheTimeOut), this.GetVocabulariesCallBack).AsQueryable();
        }

        /// <inheritdoc/>
        public void UpdateVocabulary(Vocabulary vocabulary)
        {
            // Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);

            // Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

            this._DataService.UpdateVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        private object GetVocabulariesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillQueryable<Vocabulary>(this._DataService.GetVocabularies()).ToList();
        }
    }
}
