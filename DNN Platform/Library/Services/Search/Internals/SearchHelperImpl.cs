// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Search.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Caching;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Scheduling;
    using DotNetNuke.Services.Search.Entities;
    using Lucene.Net.Analysis;
    using Lucene.Net.Analysis.Standard;
    using Lucene.Net.Analysis.Tokenattributes;
    using Lucene.Net.Util;

    internal class SearchHelperImpl : ISearchHelper
    {
        private const string SearchTypesCacheKey = "SearchTypes";
        private const string SynonymTermsCacheKey = "SynonymTerms";
        private const string SynonymGroupsCacheKey = "SynonymsGroups";
        private const string CacheKeyFormat = "{0}_{1}_{2}";
        private const string LastIndexKeyFormat = "{0}_{1}";
        private const string SearchStopWordsCacheKey = "SearchStopWords";
        private const string ResourceFileRelativePathWithoutExt = "/App_GlobalResources/GlobalResources";
        private readonly IList<string> _emptySynonums = new List<string>(0);

        public IEnumerable<SearchType> GetSearchTypes()
        {
            var cachArg = new CacheItemArgs(SearchTypesCacheKey, 120, CacheItemPriority.Default);
            return CBO.GetCachedObject<IList<SearchType>>(
                cachArg,
                dataArgs =>
                {
                    return CBO.FillCollection<SearchType>(DataProvider.Instance().GetAllSearchTypes());
                });
        }

        public SearchType GetSearchTypeByName(string searchTypeName)
        {
            return this.GetSearchTypes().Single(t => t.SearchTypeName == searchTypeName);
        }

        public IEnumerable<string> GetSynonyms(int portalId, string cultureCode, string term)
        {
            var terms = this.GetSynonymTerms(portalId, cultureCode);
            IList<string> synonyms;
            if (terms == null || !terms.TryGetValue((term ?? string.Empty).ToLowerInvariant(), out synonyms))
            {
                synonyms = this._emptySynonums;
            }

            return synonyms;
        }

        public IEnumerable<SynonymsGroup> GetSynonymsGroups(int portalId, string cultureCode)
        {
            var cacheKey = string.Format(CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
            var cachArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default);
            return CBO.GetCachedObject<IList<SynonymsGroup>>(cachArg, this.GetSynonymsGroupsCallBack);
        }

        public int AddSynonymsGroup(string synonymsTags, int portalId, string cultureCode, out string duplicateWord)
        {
            duplicateWord = null;
            if (string.IsNullOrEmpty(synonymsTags))
            {
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
            var list = this.GetSynonymsGroups(portalId, cultureCode);
            var tags = synonymsTags.ToLowerInvariant().Split(',');

            if (!tags.Any())
            {
                return 0;
            }

            foreach (var group in list)
            {
                var groupTags = group.SynonymsTags.ToLowerInvariant().Split(',');
                duplicateWord = tags.FirstOrDefault(groupTags.Contains);
                if (!string.IsNullOrEmpty(duplicateWord))
                {
                    return 0;
                }
            }

            var newId = DataProvider.Instance().AddSynonymsGroup(synonymsTags, userId, portalId, cultureCode);
            var cacheKey = string.Format(CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);

            return newId;
        }

        public int UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int portalId, string cultureCode, out string duplicateWord)
        {
            duplicateWord = null;
            if (synonymsGroupId <= 0)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(synonymsTags))
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
            var list = this.GetSynonymsGroups(portalId, cultureCode);
            var tags = synonymsTags.ToLowerInvariant().Split(',');

            if (!tags.Any())
            {
                return 0;
            }

            foreach (var group in list)
            {
                if (group.SynonymsGroupId != synonymsGroupId)
                {
                    var groupTags = group.SynonymsTags.ToLowerInvariant().Split(',');
                    duplicateWord = tags.FirstOrDefault(groupTags.Contains);
                    if (!string.IsNullOrEmpty(duplicateWord))
                    {
                        return 0;
                    }
                }
            }

            DataProvider.Instance().UpdateSynonymsGroup(synonymsGroupId, synonymsTags, userId);
            var cacheKey = string.Format(CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
            return synonymsGroupId;
        }

        public void DeleteSynonymsGroup(int synonymsGroupId, int portalId, string cultureCode)
        {
            if (synonymsGroupId <= 0)
            {
                return;
            }

            DataProvider.Instance().DeleteSynonymsGroup(synonymsGroupId);
            var cacheKey = string.Format(CacheKeyFormat, SynonymGroupsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);

            cacheKey = string.Format(CacheKeyFormat, SynonymTermsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
        }

        public SearchStopWords GetSearchStopWords(int portalId, string cultureCode)
        {
            var cacheKey = string.Format(CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
            var cachArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default);
            var list = CBO.GetCachedObject<IList<SearchStopWords>>(cachArg, this.GetSearchStopWordsCallBack);
            return list == null ? null : list.FirstOrDefault();
        }

        public int AddSearchStopWords(string stopWords, int portalId, string cultureCode)
        {
            if (string.IsNullOrEmpty(stopWords))
            {
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                return 0;
            }

            var tags = stopWords.ToLowerInvariant().Split(',');
            if (!tags.Any())
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
            var newId = DataProvider.Instance().AddSearchStopWords(stopWords, userId, portalId, cultureCode);
            var cacheKey = string.Format(CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
            return newId;
        }

        public int UpdateSearchStopWords(int stopWordsId, string stopWords, int portalId, string cultureCode)
        {
            if (string.IsNullOrEmpty(stopWords))
            {
                return 0;
            }

            if (portalId < 0)
            {
                return 0;
            }

            if (stopWordsId <= 0)
            {
                return 0;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                return 0;
            }

            var tags = stopWords.ToLowerInvariant().Split(',');
            if (!tags.Any())
            {
                return 0;
            }

            var userId = PortalSettings.Current.UserId;
            DataProvider.Instance().UpdateSearchStopWords(stopWordsId, stopWords, userId);
            var cacheKey = string.Format(CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
            return stopWordsId;
        }

        public void DeleteSearchStopWords(int stopWordsId, int portalId, string cultureCode)
        {
            if (stopWordsId <= 0)
            {
                return;
            }

            DataProvider.Instance().DeleteSearchStopWords(stopWordsId);
            var cacheKey = string.Format(CacheKeyFormat, SearchStopWordsCacheKey, portalId, cultureCode);
            DataCache.ClearCache(cacheKey);
        }

        public DateTime GetSearchReindexRequestTime(int portalId)
        {
            var requestedOn = SqlDateTime.MinValue.Value;

            var reindexRequest = portalId < 0
                ? HostController.Instance.GetString(Constants.SearchReindexSettingName, Null.NullString) // host level setting
                : PortalController.GetPortalSetting(Constants.SearchReindexSettingName, portalId, Null.NullString); // portal level setting

            if (reindexRequest != Null.NullString)
            {
                DateTime.TryParseExact(reindexRequest, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out requestedOn);
            }

            return requestedOn;
        }

        public DateTime SetSearchReindexRequestTime(int portalId)
        {
            var now = DateTime.Now;
            var text = now.ToString(Constants.ReindexDateTimeFormat);

            if (portalId < 0)
            {
                // host level setting
                HostController.Instance.Update(Constants.SearchReindexSettingName, text, true);
            }
            else
            {
                // portal level setting
                PortalController.UpdatePortalSetting(portalId, Constants.SearchReindexSettingName, text, true);
            }

            return now;
        }

        public bool GetSearchCompactFlag()
        {
            return HostController.Instance.GetString(Constants.SearchOptimizeFlagName, Null.NullString) == "1";
        }

        public void SetSearchReindexRequestTime(bool turnOn)
        {
            HostController.Instance.Update(Constants.SearchOptimizeFlagName, turnOn ? "1" : "0", true);
        }

        /// <summary>
        /// Determines whether there was a request to re-index the site since a specific date/time.
        /// </summary>
        /// <returns></returns>
        public bool IsReindexRequested(int portalId, DateTime startDate)
        {
            var reindexDateTime = this.GetSearchReindexRequestTime(portalId);
            return reindexDateTime > startDate;
        }

        /// <summary>
        /// Returns a collection of portal ID's to reindex if it was requested since last indexing.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public IEnumerable<int> GetPortalsToReindex(DateTime startDate)
        {
            var portals2Reindex = PortalController.Instance.GetPortals().Cast<PortalInfo>()
                .Where(portal => this.IsReindexRequested(portal.PortalID, startDate))
                .Select(portal => portal.PortalID);

            if (this.IsReindexRequested(-1, startDate))
            {
                // Include Host Level
                portals2Reindex = portals2Reindex.Concat(new[] { -1 });
            }

            return portals2Reindex.ToArray();
        }

        /// <summary>
        /// Returns the last time search indexing was completed successfully.
        /// The returned value in local server time (not UTC).
        /// Beware that the value stored in teh database is converted to UTC time.
        /// </summary>
        /// <returns></returns>
        public DateTime GetLastSuccessfulIndexingDateTime(int scheduleId)
        {
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            var lastValue = settings[Constants.SearchLastSuccessIndexName] as string;

            if (string.IsNullOrEmpty(lastValue))
            {
                // try to fallback to old location where this was stored
                var name = string.Format(LastIndexKeyFormat, Constants.SearchLastSuccessIndexName, scheduleId);
                lastValue = HostController.Instance.GetString(name, Null.NullString);
            }

            DateTime lastTime;
            if (!string.IsNullOrEmpty(lastValue) &&
                DateTime.TryParseExact(lastValue, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out lastTime))
            {
                // retrieves the date as UTC but returns to caller as local
                lastTime = FixSqlDateTime(lastTime).ToLocalTime().ToLocalTime();
                if (lastTime > DateTime.Now)
                {
                    lastTime = DateTime.Now;
                }
            }
            else
            {
                lastTime = SqlDateTime.MinValue.Value.AddDays(1);
            }

            return lastTime;
        }

        /// <summary>
        /// Stores the last successful time of the system search indexer.
        /// The passed value should be in local system time; not UTC time.
        /// Beware that the value stored in teh database is converted to UTC time.
        /// </summary>
        public void SetLastSuccessfulIndexingDateTime(int scheduleId, DateTime startDateLocal)
        {
            SchedulingProvider.Instance().AddScheduleItemSetting(
                scheduleId,
                Constants.SearchLastSuccessIndexName, startDateLocal.ToUniversalTime().ToString(Constants.ReindexDateTimeFormat));
        }

        public DateTime GetIndexerCheckpointUtcTime(int scheduleId, string indexerKey)
        {
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            var lastValue = settings[indexerKey] as string;

            DateTime lastUtcTime;
            if (!string.IsNullOrEmpty(lastValue) &&
                DateTime.TryParseExact(lastValue, Constants.ReindexDateTimeFormat, null, DateTimeStyles.None, out lastUtcTime))
            {
                lastUtcTime = FixSqlDateTime(lastUtcTime);
            }
            else
            {
                lastUtcTime = DateTime.UtcNow;
            }

            return lastUtcTime;
        }

        public void SetIndexerCheckpointUtcTime(int scheduleId, string indexerKey, DateTime lastUtcTime)
        {
            SchedulingProvider.Instance().AddScheduleItemSetting(scheduleId, indexerKey, lastUtcTime.ToString(Constants.ReindexDateTimeFormat));
        }

        public string GetIndexerCheckpointData(int scheduleId, string indexerKey)
        {
            var settings = SchedulingProvider.Instance().GetScheduleItemSettings(scheduleId);
            return settings[indexerKey] as string;
        }

        public void SetIndexerCheckpointData(int scheduleId, string indexerKey, string checkPointData)
        {
            SchedulingProvider.Instance().AddScheduleItemSetting(scheduleId, indexerKey, checkPointData);
        }

        public Tuple<int, int> GetSearchMinMaxLength()
        {
            var hostController = HostController.Instance;
            var minWordLength = hostController.GetInteger(Constants.SearchMinLengthKey, Constants.DefaultMinLen);
            var maxWordLength = hostController.GetInteger(Constants.SearchMaxLengthKey, Constants.DefaultMaxLen);

            if (minWordLength < Constants.MinimumMinLen)
            {
                minWordLength = Constants.MinimumMinLen;
            }

            if (maxWordLength < Constants.MinimumMaxLen)
            {
                maxWordLength = Constants.MinimumMaxLen;
            }

            if (minWordLength > Constants.MaximumMinLen)
            {
                minWordLength = Constants.MaximumMinLen;
            }

            if (maxWordLength > Constants.MaximumMaxLen)
            {
                maxWordLength = Constants.MaximumMaxLen;
            }

            if (minWordLength > maxWordLength)
            {
                var exceptionMessage = Localization.GetExceptionMessage("SearchAnalyzerMinWordLength", "Search Analyzer: min word length ({0}) is greater than max word length ({1}) value");
                throw new InvalidDataException(
                    string.Format(exceptionMessage, minWordLength, maxWordLength));
            }

            return new Tuple<int, int>(minWordLength, maxWordLength);
        }

        /// <summary>
        /// Processes and re-phrases the search text by looking into exact-match and wildcard option.
        /// </summary>
        /// <param name="searchPhrase"></param>
        /// <param name="useWildCard"></param>
        /// <param name="allowLeadingWildcard"></param>
        /// <returns>cleaned and pre-processed search phrase.</returns>
        public string RephraseSearchText(string searchPhrase, bool useWildCard, bool allowLeadingWildcard = false)
        {
            searchPhrase = this.CleanSearchPhrase(HttpUtility.HtmlDecode(searchPhrase));

            if (!useWildCard && !searchPhrase.Contains("\""))
            {
                return searchPhrase;
            }

            // we have a quotation marks and/or wildcard search, adjust accordingly
            var chars = this.FoldToASCII(searchPhrase).ToCharArray();
            var insideQuote = false;
            var newPhraseBulder = new StringBuilder();
            var currentWord = new StringBuilder();

            foreach (var c in chars)
            {
                currentWord.Append(c);
                switch (c)
                {
                    case '"':
                        insideQuote = !insideQuote;
                        if (!insideQuote)
                        {
                            newPhraseBulder.Append(currentWord + " ");
                            currentWord.Clear();
                        }

                        break;
                    case ' ':
                        if (!insideQuote && useWildCard)
                        {
                            // end of a word; we need to append a wild card to search when needed
                            newPhraseBulder.Append(this.FixLastWord(currentWord.ToString().Trim(), allowLeadingWildcard) + " ");
                            currentWord.Clear();
                        }

                        break;
                }
            }

            // in case the a double quote was not closed
            if (insideQuote)
            {
                currentWord.Append('"');
                newPhraseBulder.Append(currentWord);
            }
            else if (useWildCard)
            {
                newPhraseBulder.Append(this.FixLastWord(currentWord.ToString().Trim(), allowLeadingWildcard));
            }
            else
            {
                newPhraseBulder.Append(currentWord);
            }

            return newPhraseBulder.ToString().Trim().Replace("  ", " ");
        }

        public string StripTagsNoAttributes(string html, bool retainSpace)
        {
            var strippedString = !string.IsNullOrEmpty(html) ? HtmlUtils.StripTags(html, retainSpace) : html;

            // Encode and Strip again
            strippedString = !string.IsNullOrEmpty(strippedString) ? HtmlUtils.StripTags(html, retainSpace) : html;

            return strippedString;
        }

        private static DateTime FixSqlDateTime(DateTime datim)
        {
            if (datim <= SqlDateTime.MinValue.Value)
            {
                datim = SqlDateTime.MinValue.Value.AddDays(1);
            }
            else if (datim >= SqlDateTime.MaxValue.Value)
            {
                datim = SqlDateTime.MaxValue.Value.AddDays(-1);
            }

            return datim;
        }

        private IDictionary<string, IList<string>> GetSynonymTerms(int portalId, string cultureCode)
        {
            var cacheKey = string.Format("{0}_{1}_{2}", SynonymTermsCacheKey, portalId, cultureCode);
            var cachArg = new CacheItemArgs(cacheKey, 120, CacheItemPriority.Default);
            return CBO.GetCachedObject<IDictionary<string, IList<string>>>(cachArg, this.SynonymTermsCallBack);
        }

        private string FixLastWord(string lastWord, bool allowLeadingWildcard)
        {
            if (string.IsNullOrEmpty(lastWord))
            {
                return string.Empty;
            }

            if (lastWord.IndexOfAny(new[] { '~', '*' }) < 0)
            {
                var beginIsGroup = false;
                var endIsGroup = false;
                var wordStartPos = 0;
                var wordEndPos = lastWord.Length;

                var c1 = lastWord[0];
                var c2 = lastWord[wordEndPos - 1];

                if (c1 == '(' || c1 == '{' || c1 == '[')
                {
                    wordStartPos++;
                    beginIsGroup = true;
                }

                if (c2 == ')' || c2 == '}' || c2 == ']')
                {
                    wordEndPos--;
                    endIsGroup = true;
                }

                // pos points to next character after space
                var wordLen = wordEndPos - wordStartPos;
                lastWord = lastWord.Substring(wordStartPos, wordLen);

                // reserved words used for logical operations are case sensitive
                if (lastWord.Length > 0 && lastWord != "AND" && lastWord != "OR")
                {
                    lastWord = (beginIsGroup && endIsGroup)
                        ? string.Format("{0} OR {1}{0}*", lastWord, allowLeadingWildcard ? "*" : string.Empty)
                        : string.Format("({0} OR {1}{0}*)", lastWord, allowLeadingWildcard ? "*" : string.Empty);
                }

                if (beginIsGroup)
                {
                    lastWord = c1 + lastWord;
                }

                if (endIsGroup)
                {
                    lastWord += c2;
                }
            }

            return lastWord;
        }

        private string CleanSearchPhrase(string searchPhrase)
        {
            var chars = searchPhrase.ToCharArray();
            var hasExactMatch = false;
            int pos;

            // replace all forms of double quotes or whitespace with the normal ones
            for (pos = 0; pos < chars.Length; pos++)
            {
                var c = chars[pos];
                switch (c)
                {
                    case '\u02BA': // 'ʺ' Modifier Letter Double Prime
                    case '\u02EE': // 'ˮ' Modifier Letter Double Apostrophe
                    case '\u00AB': // [LEFT-POINTING DOUBLE ANGLE QUOTATION MARK]
                    case '\u00BB': // [RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK]
                    case '\u201C': // [LEFT DOUBLE QUOTATION MARK]
                    case '\u201D': // [RIGHT DOUBLE QUOTATION MARK]
                    case '\u201E': // [DOUBLE LOW-9 QUOTATION MARK]
                    case '\u2033': // [DOUBLE PRIME]
                    case '\u2036': // [REVERSED DOUBLE PRIME]
                    case '\u275D': // [HEAVY DOUBLE TURNED COMMA QUOTATION MARK ORNAMENT]
                    case '\u275E': // [HEAVY DOUBLE COMMA QUOTATION MARK ORNAMENT]
                    case '\u276E': // [HEAVY LEFT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                    case '\u276F': // [HEAVY RIGHT-POINTING ANGLE QUOTATION MARK ORNAMENT]
                    case '\uFF02': // [FULLWIDTH QUOTATION MARK]
                        chars[pos] = '"';  // Quotation Mark
                        hasExactMatch = true;
                        break;
                    case '\u0009': // HT, Horizontal Tab
                    case '\u000A': // LF, Line feed
                    case '\u000B': // VT, Vertical Tab
                    case '\u000C': // FF, Form feed
                    case '\u000D': // CR, Carriage return
                    case '\u00A0': // NBSP, No-break space
                        chars[pos] = ' ';  // space
                        break;
                }
            }

            // remove double apaces
            var cleaned = new string(chars).Replace("  ", " ");

            if (hasExactMatch)
            {
                // remove empty double quotes
                cleaned = cleaned.Replace("\"\"", string.Empty).Replace("\" \"", string.Empty);
            }

            return cleaned;
        }

        private object SynonymTermsCallBack(CacheItemArgs cacheItem)
        {
            var parts = cacheItem.CacheKey.Split('_');
            var allTerms = new Dictionary<string, IList<string>>();
            var portalId = int.Parse(parts[1]);
            var cultureCode = parts[2];
            var groups = this.GetSynonymsGroups(portalId, cultureCode);
            if (groups == null)
            {
                return allTerms;
            }

            foreach (var synonymsGroup in groups)
            {
                var terms = new Dictionary<string, IList<string>>();
                var groupTags = synonymsGroup.SynonymsTags.ToLowerInvariant().Split(',');

                // add the first key first
                foreach (var tag in groupTags)
                {
                    if (!terms.ContainsKey(tag))
                    {
                        terms.Add(tag, new List<string>());
                    }
                }

                // add synonyms
                foreach (var term in terms)
                {
                    foreach (var syn in terms)
                    {
                        if (term.Key != syn.Key)
                        {
                            term.Value.Add(syn.Key);
                        }
                    }
                }

                foreach (var term in terms)
                {
                    allTerms.Add(term.Key, term.Value);
                }
            }

            return allTerms;
        }

        private object GetSearchStopWordsCallBack(CacheItemArgs cacheItem)
        {
            var splittedKeys = cacheItem.CacheKey.Split('_');
            var portalId = int.Parse(splittedKeys[1]);
            var cultureCode = splittedKeys[2];

            this.EnsurePortalDefaultsAreSet(portalId);

            return CBO.FillCollection<SearchStopWords>(DataProvider.Instance().GetSearchStopWords(portalId, cultureCode));
        }

        private void EnsurePortalDefaultsAreSet(int portalId)
        {
            const string setting = "SearchAdminInitialization";

            // check portal settings first
            if (PortalController.GetPortalSetting(setting, portalId, "false") != "false")
            {
                return;
            }

            // Portal may not be present, especially during installation
            if (PortalController.Instance.GetPortal(portalId) == null)
            {
                return;
            }

            foreach (var locale in LocaleController.Instance.GetLocales(portalId).Values)
            {
                var resourceFile = this.GetResourceFile(locale.Code);

                var currentStopWords = CBO.FillCollection<SearchStopWords>(DataProvider.Instance().GetSearchStopWords(portalId, locale.Code));
                if (currentStopWords == null || currentStopWords.Count == 0)
                {
                    // Add Default StopWord
                    var defaultStopWords = Localization.GetString("DefaultStopwordGroup", resourceFile);
                    if (!string.IsNullOrEmpty(defaultStopWords))
                    {
                        DataProvider.Instance().AddSearchStopWords(defaultStopWords, 1, portalId, locale.Code);
                    }
                }

                var currentSynonymGroups = CBO.FillCollection<SynonymsGroup>(DataProvider.Instance().GetAllSynonymsGroups(portalId, locale.Code));
                if (currentSynonymGroups == null || currentSynonymGroups.Count == 0)
                {
                    // Add Default Synonym
                    var defaultSynonymsGroup = Localization.GetString("DefaultSynonymGroup", resourceFile);
                    if (!string.IsNullOrEmpty(defaultSynonymsGroup))
                    {
                        DataProvider.Instance().AddSynonymsGroup(defaultSynonymsGroup, 1, portalId, locale.Code);
                    }
                }
            }

            // Update Portal Settings
            PortalController.UpdatePortalSetting(portalId, setting, "true", true);
        }

        private string GetResourceFile(string cultureCode)
        {
            var cultureRelativePath = "~" + ResourceFileRelativePathWithoutExt + "." + cultureCode + ".resx";
            const string regularRelativePath = "~" + ResourceFileRelativePathWithoutExt + ".resx";
            return File.Exists(Path.Combine(Globals.ApplicationMapPath, ResourceFileRelativePathWithoutExt + "." + cultureCode + ".resx")) ? cultureRelativePath : regularRelativePath;
        }

        private object GetSynonymsGroupsCallBack(CacheItemArgs cacheItem)
        {
            var portalId = int.Parse(cacheItem.CacheKey.Split('_')[1]);
            var cultureCode = cacheItem.CacheKey.Split('_')[2];

            this.EnsurePortalDefaultsAreSet(portalId);

            return CBO.FillCollection<SynonymsGroup>(DataProvider.Instance().GetAllSynonymsGroups(portalId, cultureCode));
        }

        private string FoldToASCII(string searchPhrase)
        {
            var sb = new StringBuilder();

            var cleanedPhrase = searchPhrase.Trim('\0');

            var asciiFilter = new ASCIIFoldingFilter(new WhitespaceTokenizer((TextReader)new StringReader(cleanedPhrase)));

            string space = string.Empty;
            while (asciiFilter.IncrementToken())
            {
                sb.AppendFormat("{0}{1}", space ?? string.Empty, asciiFilter.GetAttribute<ITermAttribute>().Term);
                if (string.IsNullOrEmpty(space))
                {
                    space = " ";
                }
            }

            return sb.ToString();
        }
    }
}
