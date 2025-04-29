// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals;

using System;
using System.Collections.Generic;

using DotNetNuke.Services.Search.Entities;

/// <summary> Internal Search Controller Helper Interface.</summary>
/// <remarks>This is an Internal interface and should not be used outside of Core.</remarks>
public interface ISearchHelper
{
    // /// <summary>
    // /// Commits the added search documents into the search database
    // /// </summary>
    // void Commit();

    /// <summary>Returns a list of SearchTypes defined in the system.</summary>
    /// <returns>A sequence of <see cref="SearchType"/> instances.</returns>
    IEnumerable<SearchType> GetSearchTypes();

    /// <summary>Gets a SearchType Item for the given name.</summary>
    /// <returns>A <see cref="SearchType"/> instance.</returns>
    SearchType GetSearchTypeByName(string searchTypeName);

    /// <summary>Returns a list of Synonyms for a given word. E.g. leap, hop for jump.</summary>
    /// <param name="portalId">portal id.</param>
    /// <param name="cultureCode">culture code.</param>
    /// <param name="term">word for which to obtain synonyms.</param>
    /// <returns>List of synonyms.</returns>
    /// <remarks>Synonyms must be defined in system first.</remarks>
    IEnumerable<string> GetSynonyms(int portalId, string cultureCode, string term);

    /// <summary>Returns a list of SynonymsGroup defined in the system.</summary>
    /// <returns>A sequence of <see cref="SynonymsGroup"/> instances.</returns>
    IEnumerable<SynonymsGroup> GetSynonymsGroups(int portalId, string cultureCode);

    /// <summary>Adds a synonyms group to system.</summary>
    /// <param name="synonymsTags">synonyms tags separated by comma, like this: <c>"dnn,dotnetnuke"</c>.</param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode">culture code.</param>
    /// <returns>The new synonyms group ID, or <c>0</c> if the input was invalid.</returns>
    int AddSynonymsGroup(string synonymsTags, int portalId, string cultureCode, out string duplicateWord);

    /// <summary>Updates a synonyms group.</summary>
    /// <param name="synonymsGroupId"></param>
    /// <param name="synonymsTags">synonyms tags separated by comma, like this: <c>"dnn,dotnetnuke"</c>.</param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode">culture code.</param>
    /// <returns><paramref name="synonymsGroupId"/> or <c>0</c> if the input was invalid.</returns>
    int UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int portalId, string cultureCode, out string duplicateWord);

    /// <summary>Deletes a synonyms group.</summary>
    /// <param name="synonymsGroupId"></param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode">culture code.</param>
    void DeleteSynonymsGroup(int synonymsGroupId, int portalId, string cultureCode);

    /// <summary>Gets a search stop words.</summary>
    /// <param name="portalId"></param>
    /// <param name="cultureCode"></param>
    /// <returns>A <see cref="SearchStopWords"/> instance or <see langword="null"/>.</returns>
    SearchStopWords GetSearchStopWords(int portalId, string cultureCode);

    /// <summary>Adds a search stop words.</summary>
    /// <param name="stopWords"></param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode"></param>
    /// <returns>The new stop-words ID, or <c>0</c> if the input was invalid.</returns>
    int AddSearchStopWords(string stopWords, int portalId, string cultureCode);

    /// <summary>Updates a search stop words.</summary>
    /// <param name="stopWordsId"></param>
    /// <param name="stopWords"></param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode"></param>
    /// <returns><paramref name="stopWordsId"/>, or <c>0</c> if the input was invalid.</returns>
    int UpdateSearchStopWords(int stopWordsId, string stopWords, int portalId, string cultureCode);

    /// <summary>Deletes a search stop words.</summary>
    /// <param name="stopWordsId"></param>
    /// <param name="portalId"></param>
    /// <param name="cultureCode"></param>
    void DeleteSearchStopWords(int stopWordsId, int portalId, string cultureCode);

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

    Tuple<int, int> GetSearchMinMaxLength();

    string RephraseSearchText(string searchPhrase, bool useWildCard, bool allowLeadingWildcard = false);

    string StripTagsNoAttributes(string html, bool retainSpace);
}
