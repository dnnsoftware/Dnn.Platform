#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Common.Lists;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
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
    /// <history>
    ///     [vnguyen]   05/27/2013 
    /// </history>
    /// -----------------------------------------------------------------------------
    public class UserIndexer : IndexingProvider
    {
        internal const string UserIndexResetFlag = "UserIndexer_ReIndex";
        internal const string ValueSplitFlag = "$$$";

        #region Private Properties

        private const int BatchSize = 250;
        private const int ClauseMaxCount = 1024;

        private static readonly int UserSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

        #endregion

        #region Override Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the collection of SearchDocuments populated with Tab MetaData for the given portal.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="startDateLocal"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/16/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override IEnumerable<SearchDocument> GetSearchDocuments(int portalId, DateTime startDateLocal)
        {
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
                            || (richTextDataType != null && d.DataType == richTextDataType.EntryID)).ToList();

            try
            {
                var startUserId = Null.NullInteger;
                while (true)
                {
                    var reader = DataProvider.Instance()
                        .GetAvailableUsersForIndex(portalId, startDateLocal, startUserId, BatchSize);
                    int rowsAffected = 0;
                    var indexedUsers = new List<int>();

                    while (reader.Read())
                    {
                        var userSearch = GetUserSearch(reader);
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
                            var visibilityMode = ((UserVisibilityMode) Convert.ToInt32(splitValues[1]));
                            var extendedVisibility = splitValues[2];
                            var modifiedTime = Convert.ToDateTime(splitValues[3]).ToUniversalTime();

                            if (string.IsNullOrEmpty(propertyValue))
                            {
                                continue;
                            }

							//DNN-5740: replace split flag if it included in property value.
	                        propertyValue = propertyValue.Replace("[$][$][$]", "$$$");
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

                    //close the data reader
                    reader.Close();

                    //remov exists indexes
                    DeleteDocuments(portalId, indexedUsers);

                    if (rowsAffected == 0)
                    {
                        break;
                    }
                }

                if (needReindex)
                {
                    PortalController.DeletePortalSetting(portalId, UserIndexResetFlag);
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }

            return searchDocuments.Values;
        }

        private void AddBasicInformation(Dictionary<string, SearchDocument> searchDocuments, List<int> indexedUsers, UserSearch userSearch, int portalId)
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
                //searchDoc.NumericKeys.Add("superuser", Convert.ToInt32(userSearch.SuperUser));
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

        private UserSearch GetUserSearch(IDataReader reader)
        {
            var userSearch = new UserSearch
            {
                UserId = Convert.ToInt32(reader["UserId"]),
                DisplayName = reader["DisplayName"].ToString(),
                
                Email = reader["Email"].ToString(),
                UserName = reader["Username"].ToString(),
                SuperUser = Convert.ToBoolean(reader["IsSuperUser"]),
                LastModifiedOnDate = Convert.ToDateTime(reader["LastModifiedOnDate"]).ToUniversalTime(),
                CreatedOnDate = Convert.ToDateTime(reader["CreatedOnDate"]).ToUniversalTime()
            };
            if (!string.IsNullOrEmpty(userSearch.FirstName) && userSearch.FirstName.Contains(ValueSplitFlag))
            {
                userSearch.FirstName = Regex.Split(userSearch.FirstName, Regex.Escape(ValueSplitFlag))[0];
            }

            return userSearch;
        }

        #endregion

        #region Private Methods

        private void DeleteDocuments(int portalId, IList<int> usersList)
        {
            if (usersList.Count == 0)
            {
                return;
            }

            Array values = Enum.GetValues(typeof (UserVisibilityMode));

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

        private void PerformDelete(int portalId, string keyword)
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

        private bool ContainsColumn(string col, IDataReader reader)
        {
            var schema = reader.GetSchemaTable();
            return schema != null && schema.Select("ColumnName = '" + col + "'").Length > 0;
        }

        #endregion

        #region Obsoleted Methods

        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.")]
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