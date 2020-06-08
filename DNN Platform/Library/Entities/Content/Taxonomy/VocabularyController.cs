// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Entities.Content.Data;
using DotNetNuke.Entities.Users;

#endregion

namespace DotNetNuke.Entities.Content.Taxonomy
{
	/// <summary>
	/// VocabularyController provides the business layer of Vocabulary and VocabularyType.
	/// </summary>
	/// <seealso cref="TermController"/>
    public class VocabularyController : IVocabularyController
    {
        private readonly IDataService _DataService;
        private const int _CacheTimeOut = 20;

        #region Constructors

        public VocabularyController() : this(Util.GetDataService())
        {
        }

        public VocabularyController(IDataService dataService)
        {
            _DataService = dataService;
        }

        #endregion

        #region Private Methods

        private object GetVocabulariesCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillQueryable<Vocabulary>(_DataService.GetVocabularies()).ToList();
        }

        #endregion

        #region Public Methods

        public int AddVocabulary(Vocabulary vocabulary)
        {
            //Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);
            Requires.PropertyNotNegative("vocabulary", "ScopeTypeId", vocabulary.ScopeTypeId);

            vocabulary.VocabularyId = _DataService.AddVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);

            //Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

            return vocabulary.VocabularyId;
        }

        public void ClearVocabularyCache()
        {
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
        }

        public void DeleteVocabulary(Vocabulary vocabulary)
        {
            //Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);

            _DataService.DeleteVocabulary(vocabulary);

            //Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);
        }

        public IQueryable<Vocabulary> GetVocabularies()
        {
            return CBO.GetCachedObject<List<Vocabulary>>(new CacheItemArgs(DataCache.VocabularyCacheKey, _CacheTimeOut), GetVocabulariesCallBack).AsQueryable();
        }

        public void UpdateVocabulary(Vocabulary vocabulary)
        {
            //Argument Contract
            Requires.NotNull("vocabulary", vocabulary);
            Requires.PropertyNotNegative("vocabulary", "VocabularyId", vocabulary.VocabularyId);
            Requires.PropertyNotNullOrEmpty("vocabulary", "Name", vocabulary.Name);

            //Refresh Cache
            DataCache.RemoveCache(DataCache.VocabularyCacheKey);

            _DataService.UpdateVocabulary(vocabulary, UserController.Instance.GetCurrentUserInfo().UserID);
        }

        #endregion
    }
}
