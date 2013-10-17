#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Common;
using DotNetNuke.Common.Lists;
using DotNetNuke.Data;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Services.Search.Internals;

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
        #region Private Properties

        private const int BatchSize = 500;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserIndexer));
        private static readonly int UserSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

        #endregion

        #region Override Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Returns the collection of SearchDocuments populated with Tab MetaData for the given portal.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        /// <history>
        ///     [vnguyen]   04/16/2013  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public override IEnumerable<SearchDocument> GetSearchDocuments(int portalId, DateTime startDate)
        {
            var searchDocuments = new List<SearchDocument>();

            try
            {
                var pageIndex = 0;
                while (true)
                {
                    var reader = DataProvider.Instance().GetAvailableUsersForIndex(portalId, startDate, pageIndex, BatchSize);

                    while (reader.Read())
                    {
                        var userId = Convert.ToInt32(reader["UserId"]);
                        var displayName = reader["DisplayName"].ToString();
                        var propertyName = reader["PropertyName"].ToString();
                        var propertyValue = reader["PropertyValue"].ToString();
                        var visibilityMode = ((UserVisibilityMode) Convert.ToInt32(reader["Visibility"]));
                        var modifiedTime = Convert.ToDateTime(reader["ModifiedTime"]);

                        var uniqueKey = string.Format("{0}{1}", userId, visibilityMode);
                        if (visibilityMode == UserVisibilityMode.FriendsAndGroups)
                        {
                            uniqueKey = string.Format("{0}_{1}", uniqueKey, reader["ExtendedVisibility"]);
                        }

                        if (searchDocuments.Any(d => d.UniqueKey == uniqueKey))
                        {
                            var document = searchDocuments.First(d => d.UniqueKey == uniqueKey);
                            document.Body += string.Format(" {0}", propertyValue);

                            if (modifiedTime > document.ModifiedTimeUtc)
                            {
                                document.ModifiedTimeUtc = modifiedTime;
                            }

                            if (propertyName == "FirstName")
                            {
                                document.Description = propertyValue;
                            }
                        }
                        else
                        {
                            //Need remove use exists index for all visibilities.
                            DeleteDocuments(portalId, userId);
                            var searchDoc = new SearchDocument
                                            {
                                                SearchTypeId = UserSearchTypeId,
                                                UniqueKey = uniqueKey,
                                                AuthorUserId = userId,
                                                PortalId = portalId,
                                                ModifiedTimeUtc = modifiedTime,
                                                Body = string.Format("{0}", propertyValue),
                                                Description = propertyName == "FirstName" ? propertyValue : string.Empty,
                                                Title = displayName
                                            };

                            Logger.Trace("UserIndexer: Search document for user [" + displayName + " uid:" + userId + "]");
                            searchDocuments.Add(searchDoc);
                        }
                    }

                    //Get the next result (containing the total)
                    reader.NextResult();

                    //Get the total no of records from the second result
                    var totalRecords = Globals.GetTotalRecords(ref reader);
                    if (totalRecords <= BatchSize * (pageIndex + 1))
                    {
                        break;
                    }
                    else
                    {
                        pageIndex++;
                    }
                }
                
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }

            return searchDocuments;
        }

        #endregion

        #region Private Methods

        private void DeleteDocuments(int portalId, int userId)
        {
            Array values = Enum.GetValues(typeof(UserVisibilityMode));
            foreach (var item in values)
            {
                var mode = Enum.GetName(typeof(UserVisibilityMode), item);
                var searchDocument = new SearchDocument
                                         {
                                             UniqueKey = string.Format("{0}{1}", userId, mode),
                                             SearchTypeId = UserSearchTypeId,
                                             PortalId = portalId
                                         };

                InternalSearchController.Instance.DeleteSearchDocument(searchDocument);
            }
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
}