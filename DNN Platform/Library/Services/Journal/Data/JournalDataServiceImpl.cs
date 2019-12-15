// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;

using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Journal
{
    internal class JournalDataServiceImpl : IJournalDataService
    {
        private readonly DataProvider _provider = DataProvider.Instance();

        #region IJournalDataService Members

        public IDataReader Journal_ListForSummary(int portalId, int moduleId, int currentUserId, int rowIndex,
                                                  int maxRows)
        {
            return _provider.ExecuteReader("Journal_ListForSummary", portalId, moduleId, currentUserId, rowIndex,
                                           maxRows);
        }

        public IDataReader Journal_ListForProfile(int portalId, int moduleId, int currentUserId, int profileId,
                                                  int rowIndex, int maxRows)
        {
            return _provider.ExecuteReader("Journal_ListForProfile", portalId, moduleId, currentUserId, profileId,
                                           rowIndex, maxRows);
        }

        public IDataReader Journal_ListForGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex,
                                                int maxRows)
        {
            return _provider.ExecuteReader("Journal_ListForGroup", portalId, moduleId, currentUserId, groupId, rowIndex,
                                           maxRows);
        }

        public void Journal_Delete(int journalId)
        {
            _provider.ExecuteNonQuery("Journal_Delete", journalId);
        }

        public void Journal_DeleteByKey(int portalId, string objectKey)
        {
            _provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey);
        }

        public void Journal_DeleteByGroupId(int portalId, int groupId)
        {
            _provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId);
        }

        public void Journal_SoftDelete(int journalId)
        {
            _provider.ExecuteNonQuery("Journal_Delete", journalId, true);
        }
        
        public void Journal_SoftDeleteByKey(int portalId, string objectKey)
        {
            _provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey, true);
        }
        
        public void Journal_SoftDeleteByGroupId(int portalId, int groupId)
        {
            _provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId, true);
        }
        
        public void Journal_Like(int journalId, int userId, string displayName)
        {
            _provider.ExecuteNonQuery("Journal_Like", journalId, userId, displayName);
        }

        public IDataReader Journal_LikeList(int portalId, int journalId)
        {
            return _provider.ExecuteReader("Journal_LikeList", portalId, journalId);
        }

        public void Journal_UpdateContentItemId(int journalId, int contentItemId)
        {
            _provider.ExecuteNonQuery("Journal_UpdateContentItemId", journalId, contentItemId);
        }
        public IDataReader Journal_Get(int portalId, int currentUserId, int journalId) 
        {
            return Journal_Get(portalId, currentUserId, journalId, false, false, false);
        }
        public IDataReader Journal_Get(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
        {
            return _provider.ExecuteReader("Journal_Get", portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck);
        }
        public IDataReader Journal_GetByKey(int portalId, string objectKey) {
            return Journal_GetByKey(portalId, objectKey, false, false);
        }
        public IDataReader Journal_GetByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
        {
            return _provider.ExecuteReader("Journal_GetByKey", portalId, objectKey, includeAllItems, isDeleted);
        }
        public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title,
                                string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet) 
        {  
            journalId = _provider.ExecuteScalar<int>("Journal_Save", portalId, journalId, journalTypeId, currentUserId, profileId, 
                                                    groupId, title, summary, itemData, xml, objectKey, accessKey, securitySet, false, false);
            return journalId;
         }
        public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title,
                        string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
        {
            journalId = _provider.ExecuteScalar<int>("Journal_Save", portalId, journalId, journalTypeId, currentUserId, profileId,
                                                    groupId, title, summary, itemData, xml, objectKey, accessKey, securitySet, commentsDisabled, commentsHidden);
            return journalId;
        }
        public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title,
                        string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet)
        {
            journalId = _provider.ExecuteScalar<int>("Journal_Update", portalId, journalId, journalTypeId, currentUserId, profileId,
                                                    groupId, title, summary, itemData, xml, objectKey, accessKey, securitySet, false, false);

            return journalId;
        }
        public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title,
                        string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
        {
            journalId = _provider.ExecuteScalar<int>("Journal_Update", portalId, journalId, journalTypeId, currentUserId, profileId,
                                                    groupId, title, summary, itemData, xml, objectKey, accessKey, securitySet, commentsDisabled, commentsHidden);
            return journalId;
        }
        public void Journal_Comment_Delete(int journalId, int commentId) {
            _provider.ExecuteNonQuery("Journal_Comment_Delete", journalId, commentId);
        }

        public int Journal_Comment_Save(int journalId, int commentId, int userId, string comment, string xml, DateTime dateUpdated)
        {
            commentId = _provider.ExecuteScalar<int>("Journal_Comment_Save", journalId, commentId, userId, comment, xml, DataProvider.Instance().GetNull(dateUpdated));
            return commentId;
        }
       
        public IDataReader Journal_Comment_List(int journalId)
        {
            return _provider.ExecuteReader("Journal_Comment_List", journalId);
        }

        public IDataReader Journal_Comment_Get(int commentId)
        {
            return _provider.ExecuteReader("Journal_Comment_Get", commentId);
        }

        public IDataReader Journal_Comment_ListByJournalIds(string journalIds)
        {
            return _provider.ExecuteReader("Journal_Comment_ListByJournalIds", journalIds);
        }

        public void Journal_Comment_Like(int journalId, int commentId, int userId, string displayName)
        {
            _provider.ExecuteNonQuery("Journal_Comment_Like", journalId, commentId, userId, displayName);
        }
        public IDataReader Journal_Comment_LikeList(int portalId, int journalId, int commentId) {
            return _provider.ExecuteReader("Journal_Comment_LikeList", portalId, journalId, commentId);
        }
        public void Journal_Comments_ToggleDisable(int portalId, int journalId, bool disable)
        {
            _provider.ExecuteNonQuery("Journal_Comments_ToggleDisable", portalId, journalId, disable);
        }

        public void Journal_Comments_ToggleHidden(int portalId, int journalId, bool hidden)
        {
            _provider.ExecuteNonQuery("Journal_Comments_ToggleHidden", portalId, journalId, hidden);
        }
        public IDataReader Journal_Types_List(int portalId) {
            return _provider.ExecuteReader("Journal_Types_List", portalId);
        }

        public IDataReader Journal_Types_GetById(int journalTypeId)
        {
            return _provider.ExecuteReader("Journal_Types_GetById", journalTypeId);
        }

        public IDataReader Journal_Types_Get(string journalType)
        {
            return _provider.ExecuteReader("Journal_Types_Get", journalType);
        }

        public void Journal_Types_Delete(int journalTypeId, int portalId)
        {
            _provider.ExecuteNonQuery("Journal_Types_Delete", journalTypeId, portalId);
        }

        public int Journal_Types_Save(int journalTypeId, string journalType, string icon, int portalId, bool isEnabled,
                                      bool appliesToProfile, bool appliesToGroup, bool appliesToStream, string options,
                                      bool supportsNotify)
        {
            journalTypeId = _provider.ExecuteScalar<int>("Journal_Types_Save", journalTypeId, journalType, icon,
                                                         portalId, isEnabled, appliesToProfile, appliesToGroup,
                                                         appliesToStream, options, supportsNotify);
            return journalTypeId;
        }

        public IDataReader Journal_GetStatsForGroup(int portalId, int groupId)
        {
            return _provider.ExecuteReader("Journal_GetStatsForGroup", portalId, groupId);
        }

        public IDataReader Journal_TypeFilters_List(int portalId, int moduleId)
        {
            return _provider.ExecuteReader("Journal_TypeFilters_List", portalId, moduleId);
        }

        public void Journal_TypeFilters_Delete(int portalId, int moduleId)
        {
            _provider.ExecuteNonQuery("Journal_TypeFilters_Delete", portalId, moduleId);
        }

        public void Journal_TypeFilters_Save(int portalId, int moduleId, int journalTypeId)
        {
            _provider.ExecuteNonQuery("Journal_TypeFilters_Save", portalId, moduleId, journalTypeId);
        }

        #endregion
    }
}
