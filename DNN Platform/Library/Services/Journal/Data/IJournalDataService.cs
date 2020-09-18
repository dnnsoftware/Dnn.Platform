// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Data;

    public interface IJournalDataService
    {
        IDataReader Journal_ListForSummary(int portalId, int moduleId, int currentUserId, int rowIndex, int maxRows);

        IDataReader Journal_ListForProfile(int portalId, int moduleId, int currentUserId, int profileId, int rowIndex,
                                           int maxRows);

        IDataReader Journal_ListForGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex,
                                         int maxRows);

        void Journal_Delete(int journalId);

        void Journal_DeleteByKey(int portalId, string objectKey);

        void Journal_DeleteByGroupId(int portalId, int groupId);

        void Journal_SoftDelete(int journalId);

        void Journal_SoftDeleteByKey(int portalId, string objectKey);

        void Journal_SoftDeleteByGroupId(int portalId, int groupId);

        IDataReader Journal_Get(int portalId, int currentUserId, int journalId);

        IDataReader Journal_Get(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck);

        IDataReader Journal_GetByKey(int portalId, string objectKey);

        IDataReader Journal_GetByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted);

        int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary,
            string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet);

        int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary,
            string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsHidden, bool commentsDisabled);

        int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary,
            string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet);

        int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary,
            string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsHidden, bool commentsDisabled);

        void Journal_UpdateContentItemId(int journalId, int contentItemId);

        void Journal_Like(int journalId, int userId, string displayName);

        IDataReader Journal_LikeList(int portalId, int journalId);

        void Journal_Comment_Delete(int journalId, int commentId);

        int Journal_Comment_Save(int journalId, int commentId, int userId, string comment, string xml, DateTime dateUpdated);

        IDataReader Journal_Comment_List(int journalId);

        IDataReader Journal_Comment_Get(int commentId);

        IDataReader Journal_Comment_ListByJournalIds(string journalIds);

        void Journal_Comment_Like(int journalId, int commentId, int userId, string displayName);

        IDataReader Journal_Comment_LikeList(int portalId, int journalId, int commentId);

        void Journal_Comments_ToggleDisable(int portalId, int journalId, bool disable);

        void Journal_Comments_ToggleHidden(int portalId, int journalId, bool hidden);

        IDataReader Journal_Types_List(int portalId);

        IDataReader Journal_Types_GetById(int journalTypeId);

        IDataReader Journal_Types_Get(string journalType);

        void Journal_Types_Delete(int journalTypeId, int portalId);

        int Journal_Types_Save(int journalTypeId, string journalType, string icon, int portalId, bool isEnabled,
                               bool appliesToProfile, bool appliesToGroup, bool appliesToStream, string options,
                               bool supportsNotify);

        IDataReader Journal_GetStatsForGroup(int portalId, int groupId);

        IDataReader Journal_TypeFilters_List(int portalId, int moduleId);

        void Journal_TypeFilters_Delete(int portalId, int moduleId);

        void Journal_TypeFilters_Save(int portalId, int moduleId, int journalTypeId);
    }
}
