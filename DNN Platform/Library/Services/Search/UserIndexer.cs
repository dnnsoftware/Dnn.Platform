#region Copyright
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
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Scheduling;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;

#endregion

namespace DotNetNuke.Services.Search
{
    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Services.Search
    /// Project:    DotNetNuke.Search.Index
    /// Class:      TabIndexer
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The TabIndexer is an implementation of the abstract IndexingProvider
    /// class
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class UserIndexer : IndexingProvider
    {
        internal const string UserIndexResetFlag = "UserIndexer_ReIndex";
        internal const string ValueSplitFlag = "$$$";

        internal static readonly Regex UsrFirstNameSplitRx = new Regex(Regex.Escape(ValueSplitFlag), RegexOptions.Compiled);

        #region Private Properties

        private const int BatchSize = 250;
        private const int ClauseMaxCount = 1024;

        private static readonly int UserSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

        #endregion

        #region Override Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Searches for and indexes modified users for the given portal.
        /// </summary>
        /// <returns>Count of indexed records</returns>
        /// -----------------------------------------------------------------------------
        public override int IndexSearchDocuments(int portalId,
            ScheduleHistoryItem schedule, DateTime startDateLocal, Action<IEnumerable<SearchDocument>> indexer)
        {
            Requires.NotNull("indexer", indexer);
            const int saveThreshold = BatchSize;
            var totalIndexed = 0;
            var checkpointModified = false;
            startDateLocal = GetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, startDateLocal);
            var searchDocuments = new Dictionary<string, SearchDocument>();

            var needReindex = PortalController.GetPortalSettingAsBoolean(UserIndexResetFlag, portalId, false);
            if (needReindex)
            {
                startDateLocal = SqlDateTime.MinValue.Value.AddDays(1);
            }

            var controller = new ListController();
            var textDataType = controller.GetListEntryInfo("DataType", "Text");
            var richTextDataType = controller.GetListEntryInfo("DataType", "RichText");

            var profileDefinitions = ProfileController.GetPropertyDefinitionsByPortal(portalId, false, false)
                .Cast<ProfilePropertyDefinition>()
                .Where(d => (textDataType != null && d.DataType == textDataType.EntryID)
                            || (richTextDataType != null && d.DataType == richTextDataType.EntryID))
                .ToList();

            try
            {
                int startUserId;
                var checkpointData = GetLastCheckpointData(portalId, schedule.ScheduleID);
                if (string.IsNullOrEmpty(checkpointData) || !int.TryParse(checkpointData, out startUserId))
                {
                    startUserId = Null.NullInteger;
                }

                int rowsAffected;
                IList<int> indexedUsers;
                do
                {
                    rowsAffected = FindModifiedUsers(portalId, startDateLocal,
                        searchDocuments, profileDefinitions, out indexedUsers, ref startUserId);

                    if (rowsAffected > 0 && searchDocuments.Count >= saveThreshold)
                    {
                        //remove existing indexes
                        DeleteDocuments(portalId, indexedUsers);
                        var values = searchDocuments.Values;
                        totalIndexed += IndexCollectedDocs(indexer, values);
                        SetLastCheckpointData(portalId, schedule.ScheduleID, startUserId.ToString());
                        SetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, values.Last().ModifiedTimeUtc.ToLocalTime());
                        searchDocuments.Clear();
                        checkpointModified = true;
                    }
                } while (rowsAffected > 0);

                if (searchDocuments.Count > 0)
                {
                    //remove existing indexes
                    DeleteDocuments(portalId, indexedUsers);
                    var values = searchDocuments.Values;
                    totalIndexed += IndexCollectedDocs(indexer, values);
                    checkpointModified = true;
                }

                if (needReindex)
                {
                    PortalController.DeletePortalSetting(portalId, UserIndexResetFlag);
                }
            }
            catch (Exception ex)
            {
                checkpointModified = false;
                Exceptions.Exceptions.LogException(ex);
            }

            if (checkpointModified)
            {
                // at last reset start user pointer
                SetLastCheckpointData(portalId, schedule.ScheduleID, Null.NullInteger.ToString());
                SetLocalTimeOfLastIndexedItem(portalId, schedule.ScheduleID, DateTime.Now);
            }
            return totalIndexed;
        }

        private int IndexCollectedDocs(Action<IEnumerable<SearchDocument>> indexer, ICollection<SearchDocument> searchDocuments)
        {
            indexer.Invoke(searchDocuments);
            var total = searchDocuments.Select(d => d.UniqueKey.Substring(0, d.UniqueKey.IndexOf("_", StringComparison.Ordinal))).Distinct().Count();
            return total;
        }

        private static int FindModifiedUsers(int portalId, DateTime startDateLocal,
            IDictionary<string, SearchDocument> searchDocuments, IList<ProfilePropertyDefinition> profileDefinitions, out IList<int> indexedUsers,
            ref int startUserId)
        {
            var rowsAffected = 0;
            indexedUsers = new List<int>();
            using (var reader = DataProvider.Instance().GetAvailableUsersForIndex(portalId, startDateLocal, startUserId, BatchSize))
            {
                while (reader.Read())
                {
                    var userSearch = GetUserSearch(reader);
                    if (userSearch == null) continue;

                    AddBasicInformation(searchDocuments, indexedUsers, userSearch, portalId);

                    //log the userid so that it can get the correct user collection next time.
                    if (userSearch.UserId > startUserId)
                    {
                        startUserId = userSearch.UserId;
                    }

                    foreach (var definition in profileDefinitions)
                    {
                        var propertyName = definition.PropertyName;

                        if (!ContainsColumn(propertyName, reader))
                        {
                            continue;
                        }

                        var propertyValue = reader[propertyName].ToString();

                        if (string.IsNullOrEmpty(propertyValue) || !propertyValue.Contains(ValueSplitFlag))
                        {
                            continue;
                        }

                        var splitValues = Regex.Split(propertyValue, Regex.Escape(ValueSplitFlag));

                        propertyValue = splitValues[0];
                        var visibilityMode = ((UserVisibilityMode)Convert.ToInt32(splitValues[1]));
                        var extendedVisibility = splitValues[2];
                        var modifiedTime = Convert.ToDateTime(splitValues[3]).ToUniversalTime();

                        if (string.IsNullOrEmpty(propertyValue))
                        {
                            continue;
                        }

                        //DNN-5740 / DNN-9040: replace split flag if it included in property value.
                        propertyValue = propertyValue.Replace("[$]", "$");
                        var uniqueKey = string.Format("{0}_{1}", userSearch.UserId, visibilityMode).ToLowerInvariant();
                        if (visibilityMode == UserVisibilityMode.FriendsAndGroups)
                        {
                            uniqueKey = string.Format("{0}_{1}", uniqueKey, extendedVisibility);
                        }

                        if (searchDocuments.ContainsKey(uniqueKey))
                        {
                            var document = searchDocuments[uniqueKey];
                            document.Keywords.Add(propertyName, propertyValue);

                            if (modifiedTime > document.ModifiedTimeUtc)
                            {
                                document.ModifiedTimeUtc = modifiedTime;
                            }
                        }
                        else
                        {
                            //Need remove use exists index for all visibilities.
                            if (!indexedUsers.Contains(userSearch.UserId))
                            {
                                indexedUsers.Add(userSearch.UserId);
                            }

                            if (!string.IsNullOrEmpty(propertyValue))
                            {
                                var searchDoc = new SearchDocument
                                {
                                    SearchTypeId = UserSearchTypeId,
                                    UniqueKey = uniqueKey,
                                    PortalId = portalId,
                                    ModifiedTimeUtc = modifiedTime,
                                    Description = userSearch.FirstName,
                                    Title = userSearch.DisplayName
                                };
                                searchDoc.Keywords.Add(propertyName, propertyValue);
                                searchDoc.NumericKeys.Add("superuser", Convert.ToInt32(userSearch.SuperUser));
                                searchDocuments.Add(uniqueKey, searchDoc);
                            }
                        }
                    }

                    rowsAffected++;
                }
            }
            return rowsAffected;
        }

        private static void AddBasicInformation(IDictionary<string, SearchDocument> searchDocuments, ICollection<int> indexedUsers, UserSearch userSearch, int portalId)
        {
            if (!searchDocuments.ContainsKey(
                            string.Format("{0}_{1}", userSearch.UserId, UserVisibilityMode.AllUsers)
                                .ToLowerInvariant()))
            {
                if (!indexedUsers.Contains(userSearch.UserId))
                {
                    indexedUsers.Add(userSearch.UserId);
                }
                //if the user doesn't exist in search collection, we need add it with ALLUsers mode,
                //so that can make sure DisplayName will be indexed
                var searchDoc = new SearchDocument
                {
                    SearchTypeId = UserSearchTypeId,
                    UniqueKey =
                        string.Format("{0}_{1}", userSearch.UserId,
                            UserVisibilityMode.AllUsers).ToLowerInvariant(),
                    PortalId = portalId,
                    ModifiedTimeUtc = userSearch.LastModifiedOnDate,
                    Body = string.Empty,
                    Description = userSearch.FirstName,
                    Title = userSearch.DisplayName
                };
                searchDoc.NumericKeys.Add("superuser", Convert.ToInt32(userSearch.SuperUser));
                searchDocuments.Add(searchDoc.UniqueKey, searchDoc);
            }

            if (!searchDocuments.ContainsKey(
                            string.Format("{0}_{1}", userSearch.UserId, UserVisibilityMode.AdminOnly)
                                .ToLowerInvariant()))
            {
                if (!indexedUsers.Contains(userSearch.UserId))
                {
                    indexedUsers.Add(userSearch.UserId);
                }
                //if the user doesn't exist in search collection, we need add it with ALLUsers mode,
                //so that can make sure DisplayName will be indexed
                var searchDoc = new SearchDocument
                {
                    SearchTypeId = UserSearchTypeId,
                    UniqueKey =
                        string.Format("{0}_{1}", userSearch.UserId,
                            UserVisibilityMode.AdminOnly).ToLowerInvariant(),
                    PortalId = portalId,
                    ModifiedTimeUtc = userSearch.LastModifiedOnDate,
                    Body = string.Empty,
                    Description = userSearch.FirstName,
                    Title = userSearch.DisplayName
                };

                searchDoc.NumericKeys.Add("superuser", Convert.ToInt32(userSearch.SuperUser));
                searchDoc.Keywords.Add("username", userSearch.UserName);
                searchDoc.Keywords.Add("email", userSearch.Email);
                searchDoc.Keywords.Add("createdondate", userSearch.CreatedOnDate.ToString(Constants.DateTimeFormat));
                searchDocuments.Add(searchDoc.UniqueKey, searchDoc);
            }
        }

        private static UserSearch GetUserSearch(IDataRecord reader)
        {
            try
            {
                var createdOn = reader["CreatedOnDate"] as DateTime?;
                var modifiedOn = reader["LastModifiedOnDate"] as DateTime? ?? createdOn;
                var userSearch = new UserSearch
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    DisplayName = reader["DisplayName"].ToString(),
                    Email = reader["Email"].ToString(),
                    UserName = reader["Username"].ToString(),
                    SuperUser = Convert.ToBoolean(reader["IsSuperUser"]),
                    LastModifiedOnDate = Convert.ToDateTime(modifiedOn).ToUniversalTime(),
                    CreatedOnDate = Convert.ToDateTime(createdOn).ToUniversalTime()
                };

                if (!string.IsNullOrEmpty(userSearch.FirstName) && userSearch.FirstName.Contains(ValueSplitFlag))
                {
                    userSearch.FirstName = UsrFirstNameSplitRx.Split(userSearch.FirstName)[0];
                }

                return userSearch;
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
                return null;
            }
        }

        #endregion

        #region Private Methods

        private static void DeleteDocuments(int portalId, ICollection<int> usersList)
        {
            if (usersList == null || usersList.Count == 0)
            {
                return;
            }

            var values = Enum.GetValues(typeof (UserVisibilityMode));

            var clauseCount = 0;
            foreach (var item in values)
            {
                var keyword = new StringBuilder("(");
                foreach (var userId in usersList)
                {
                    var mode = Enum.GetName(typeof (UserVisibilityMode), item);
                    keyword.AppendFormat("{2} {0}_{1} OR {0}_{1}* ", userId, mode,
                        keyword.Length > 1 ? "OR " : string.Empty);
                    clauseCount += 2;
                    if (clauseCount >= ClauseMaxCount)
                        //max cluaseCount is 1024, if reach the max value, perform a delete action. 
                    {
                        keyword.Append(")");
                        PerformDelete(portalId, keyword.ToString().ToLowerInvariant());
                        keyword.Clear().Append("(");
                        clauseCount = 0;
                    }
                }

                if (keyword.Length > 1)
                {
                    keyword.Append(")");
                    PerformDelete(portalId, keyword.ToString().ToLowerInvariant());
                }
            }
        }

        private static void PerformDelete(int portalId, string keyword)
        {
            var query = new BooleanQuery
            {
                {NumericRangeQuery.NewIntRange(Constants.PortalIdTag, portalId, portalId, true, true), Occur.MUST},
                {
                    NumericRangeQuery.NewIntRange(Constants.SearchTypeTag, UserSearchTypeId, UserSearchTypeId, true, true),
                    Occur.MUST
                }
            };

            var parserContent = new QueryParser(Constants.LuceneVersion, Constants.UniqueKeyTag,
                new SearchQueryAnalyzer(true));
            var parsedQueryContent = parserContent.Parse(keyword.ToLowerInvariant());
            query.Add(parsedQueryContent, Occur.MUST);

            LuceneController.Instance.Delete(query);
        }

        private static bool ContainsColumn(string col, IDataReader reader)
        {
            var schema = reader.GetSchemaTable();
            return schema != null && schema.Select("ColumnName = '" + col + "'").Length > 0;
        }

        #endregion

        #region Obsoleted Methods

        [Obsolete("Legacy Search (ISearchable) -- Deprecated in DNN 7.1. Use 'IndexSearchDocuments' instead.. Scheduled removal in v10.0.0.")]
        public override SearchItemInfoCollection GetSearchIndexItems(int portalId)
        {
            return null;
        }

        #endregion

    }

    internal class UserSearch
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public bool SuperUser { get; set; }
        public DateTime LastModifiedOnDate { get; set; }
        public DateTime CreatedOnDate { get; set; }
    }
}