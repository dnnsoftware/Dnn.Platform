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

using DotNetNuke.Services.Search.Entities;

#endregion

namespace DotNetNuke.Services.Search.Internals
{
    /// <summary>
    /// Internal Search Controller Helper Interface.
    /// <remarks>This is an Internal interface and should not be used outside of Core.</remarks>
    /// </summary>
    public interface ISearchHelper
    {

        #region SearchType APIs

        // /// <summary>
        // /// Commits the added search documents into the search database
        // /// </summary>
        // void Commit();

        /// <summary>
        /// Returns a list of SearchTypes defined in the system
        /// </summary>
        /// <returns></returns>
        IEnumerable<SearchType> GetSearchTypes();

        /// <summary>
        /// Gets a SearchType Item for the given name.
        /// </summary>
        /// <returns></returns>
        SearchType GetSearchTypeByName(string searchTypeName);

        #endregion

        #region Synonym Management APIs
        /// <summary>
        /// Returns a list of Synonyms for a given word. E.g. leap, hop for jump
        /// </summary>
        /// <param name="term">word for which to obtain synonyms</param>
        /// <param name="portalId">portal id</param> 
        /// <param name="cultureCode">culture code</param>
        /// <returns>List of synonyms</returns>
        /// <remarks>Synonyms must be defined in system first</remarks>
        IEnumerable<string> GetSynonyms(int portalId, string cultureCode, string term);

        /// <summary>
        /// Returns a list of SynonymsGroup defined in the system
        /// </summary>
        /// <returns></returns>
        IEnumerable<SynonymsGroup> GetSynonymsGroups(int portalId, string cultureCode);

        /// <summary>
        /// Adds a synonymsgroup to system
        /// </summary>
        /// <param name="synonymsTags">synonyms tags seperated by comma, like this: dnn,dotnetnuke</param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode">culture code</param>
        /// <returns></returns>
        int AddSynonymsGroup(string synonymsTags, int portalId, string cultureCode, out string duplicateWord);

        /// <summary>
        /// Updates a sysnonymsGroup
        /// </summary>
        /// <param name="synonymsGroupId"></param>
        /// <param name="synonymsTags">synonyms tags seperated by comma, like this: dnn,dotnetnuke</param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode">culture code</param>
        /// <returns></returns>
        int UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int portalId, string cultureCode, out string duplicateWord);

        /// <summary>
        /// Deletes a synonyms group
        /// </summary>
        /// <param name="synonymsGroupId"></param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode">culture code</param>
        void DeleteSynonymsGroup(int synonymsGroupId, int portalId, string cultureCode);

        #endregion

        #region Stop Word Management APIs

        /// <summary>
        /// Gets a search stop words
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        SearchStopWords GetSearchStopWords(int portalId, string cultureCode);

        /// <summary>
        /// Adds a search stop words
        /// </summary>
        /// <param name="stopWords"></param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        int AddSearchStopWords(string stopWords, int portalId, string cultureCode);

        /// <summary>
        /// Updates a search stop words
        /// </summary>
        /// <param name="stopWordsId"></param>
        /// <param name="stopWords"></param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        int UpdateSearchStopWords(int stopWordsId, string stopWords, int portalId, string cultureCode);

        /// <summary>
        /// Deletes a search stop words
        /// </summary>
        /// <param name="stopWordsId"></param>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        void DeleteSearchStopWords(int stopWordsId, int portalId, string cultureCode);

        #endregion

        #region Reindex and Compact settings

        DateTime GetSearchReindexRequestTime(int portalId);
        DateTime SetSearchReindexRequestTime(int portalId);
        bool GetSearchCompactFlag();
        void SetSearchReindexRequestTime(bool turnOn);
        bool IsReindexRequested(int portalId, DateTime startDate);
        IEnumerable<int> GetPortalsToReindex(DateTime startDate);
        DateTime GetLastSuccessfulIndexingDateTime(int scheduleId);
        void SetLastSuccessfulIndexingDateTime(int scheduleId, DateTime startDateLocal);

        DateTime GetIndexerCheckpointUtcTime(int scheduleId, string indexerKey);
        void SetIndexerCheckpointUtcTime(int scheduleId, string indexerKey, DateTime lastUtcTime);

        string GetIndexerCheckpointData(int scheduleId, string indexerKey);
        void SetIndexerCheckpointData(int scheduleId, string indexerKey, string checkPointData);

        #endregion

        #region Other Search Helper methods
        Tuple<int, int> GetSearchMinMaxLength();
		string RephraseSearchText(string searchPhrase, bool useWildCard, bool allowLeadingWildcard = false);
        string StripTagsNoAttributes(string html, bool retainSpace);
        #endregion
    }
}
