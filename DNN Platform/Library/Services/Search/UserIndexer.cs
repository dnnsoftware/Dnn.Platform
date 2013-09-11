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
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UserIndexer));
        private static readonly int TabSearchTypeId = SearchHelper.Instance.GetSearchTypeByName("user").SearchTypeId;

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
            var users = UserController.GetUsers(portalId).Cast<UserInfo>().Where(u => u.LastModifiedOnDate > startDate);
            try
            {
                foreach (var user in users)
                {
                    Array values = Enum.GetValues(typeof(UserVisibilityMode));
                    foreach (var val in values)
                    {
                        var body = "";
                        var mode = Enum.GetName(typeof(UserVisibilityMode), val);
                        var props = user.Profile.ProfileProperties.Cast<ProfilePropertyDefinition>().Where(p => p.ProfileVisibility.VisibilityMode.ToString() == mode);
                        foreach (var prop in props)
                        {
                            body += prop.PropertyValue + " ";
                        }
                        var searchDoc = new SearchDocument
                        {
                            SearchTypeId = TabSearchTypeId,
                            UniqueKey = user.UserID.ToString() + val,
                            AuthorUserId = user.UserID,
                            PortalId = portalId,
                            ModifiedTimeUtc = user.LastModifiedOnDate,
                            Body = body,
                            Description = user.Profile.FirstName,
                            Title = user.DisplayName
                        };

                        Logger.Trace("UserIndexer: Search document for user [" + user.DisplayName + " uid:" + user.UserID + "]");
                        searchDocuments.Add(searchDoc);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.Exceptions.LogException(ex);
            }

            return searchDocuments;
        }

        [Obsolete("Legacy Search (ISearchable) -- Depricated in DNN 7.1. Use 'GetSearchDocuments' instead.")]
        public override SearchItemInfoCollection GetSearchIndexItems(int portalId)
        {
            return null;
        }
    }
}