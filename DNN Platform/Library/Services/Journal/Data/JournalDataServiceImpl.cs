// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal;

using System;
using System.Data;

using DotNetNuke.Data;

internal class JournalDataServiceImpl : IJournalDataService
{
    private readonly DataProvider provider = DataProvider.Instance();

    /// <inheritdoc/>
    public IDataReader Journal_ListForSummary(int portalId, int moduleId, int currentUserId, int rowIndex, int maxRows)
    {
        return this.provider.ExecuteReader(
            "Journal_ListForSummary",
            portalId,
            moduleId,
            currentUserId,
            rowIndex,
            maxRows);
    }

    /// <inheritdoc/>
    public IDataReader Journal_ListForProfile(int portalId, int moduleId, int currentUserId, int profileId, int rowIndex, int maxRows)
    {
        return this.provider.ExecuteReader(
            "Journal_ListForProfile",
            portalId,
            moduleId,
            currentUserId,
            profileId,
            rowIndex,
            maxRows);
    }

    /// <inheritdoc/>
    public IDataReader Journal_ListForGroup(int portalId, int moduleId, int currentUserId, int groupId, int rowIndex, int maxRows)
    {
        return this.provider.ExecuteReader(
            "Journal_ListForGroup",
            portalId,
            moduleId,
            currentUserId,
            groupId,
            rowIndex,
            maxRows);
    }

    /// <inheritdoc/>
    public void Journal_Delete(int journalId)
    {
        this.provider.ExecuteNonQuery("Journal_Delete", journalId);
    }

    /// <inheritdoc/>
    public void Journal_DeleteByKey(int portalId, string objectKey)
    {
        this.provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey);
    }

    /// <inheritdoc/>
    public void Journal_DeleteByGroupId(int portalId, int groupId)
    {
        this.provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId);
    }

    /// <inheritdoc/>
    public void Journal_SoftDelete(int journalId)
    {
        this.provider.ExecuteNonQuery("Journal_Delete", journalId, true);
    }

    /// <inheritdoc/>
    public void Journal_SoftDeleteByKey(int portalId, string objectKey)
    {
        this.provider.ExecuteNonQuery("Journal_DeleteByKey", portalId, objectKey, true);
    }

    /// <inheritdoc/>
    public void Journal_SoftDeleteByGroupId(int portalId, int groupId)
    {
        this.provider.ExecuteNonQuery("Journal_DeleteByGroupId", portalId, groupId, true);
    }

    /// <inheritdoc/>
    public void Journal_Like(int journalId, int userId, string displayName)
    {
        this.provider.ExecuteNonQuery("Journal_Like", journalId, userId, displayName);
    }

    /// <inheritdoc/>
    public IDataReader Journal_LikeList(int portalId, int journalId)
    {
        return this.provider.ExecuteReader("Journal_LikeList", portalId, journalId);
    }

    /// <inheritdoc/>
    public void Journal_UpdateContentItemId(int journalId, int contentItemId)
    {
        this.provider.ExecuteNonQuery("Journal_UpdateContentItemId", journalId, contentItemId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Get(int portalId, int currentUserId, int journalId)
    {
        return this.Journal_Get(portalId, currentUserId, journalId, false, false, false);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Get(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
    {
        return this.provider.ExecuteReader("Journal_Get", portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck);
    }

    /// <inheritdoc/>
    public IDataReader Journal_GetByKey(int portalId, string objectKey)
    {
        return this.Journal_GetByKey(portalId, objectKey, false, false);
    }

    /// <inheritdoc/>
    public IDataReader Journal_GetByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
    {
        return this.provider.ExecuteReader("Journal_GetByKey", portalId, objectKey, includeAllItems, isDeleted);
    }

    /// <inheritdoc/>
    public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet)
    {
        journalId = this.provider.ExecuteScalar<int>(
            "Journal_Save",
            portalId,
            journalId,
            journalTypeId,
            currentUserId,
            profileId,
            groupId,
            title,
            summary,
            itemData,
            xml,
            objectKey,
            accessKey,
            securitySet,
            false,
            false);
        return journalId;
    }

    /// <inheritdoc/>
    public int Journal_Save(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
    {
        journalId = this.provider.ExecuteScalar<int>(
            "Journal_Save",
            portalId,
            journalId,
            journalTypeId,
            currentUserId,
            profileId,
            groupId,
            title,
            summary,
            itemData,
            xml,
            objectKey,
            accessKey,
            securitySet,
            commentsDisabled,
            commentsHidden);
        return journalId;
    }

    /// <inheritdoc/>
    public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet)
    {
        journalId = this.provider.ExecuteScalar<int>(
            "Journal_Update",
            portalId,
            journalId,
            journalTypeId,
            currentUserId,
            profileId,
            groupId,
            title,
            summary,
            itemData,
            xml,
            objectKey,
            accessKey,
            securitySet,
            false,
            false);

        return journalId;
    }

    /// <inheritdoc/>
    public int Journal_Update(int portalId, int currentUserId, int profileId, int groupId, int journalId, int journalTypeId, string title, string summary, string body, string itemData, string xml, string objectKey, Guid accessKey, string securitySet, bool commentsDisabled, bool commentsHidden)
    {
        journalId = this.provider.ExecuteScalar<int>(
            "Journal_Update",
            portalId,
            journalId,
            journalTypeId,
            currentUserId,
            profileId,
            groupId,
            title,
            summary,
            itemData,
            xml,
            objectKey,
            accessKey,
            securitySet,
            commentsDisabled,
            commentsHidden);
        return journalId;
    }

    /// <inheritdoc/>
    public void Journal_Comment_Delete(int journalId, int commentId)
    {
        this.provider.ExecuteNonQuery("Journal_Comment_Delete", journalId, commentId);
    }

    /// <inheritdoc/>
    public int Journal_Comment_Save(int journalId, int commentId, int userId, string comment, string xml, DateTime dateUpdated)
    {
        commentId = this.provider.ExecuteScalar<int>("Journal_Comment_Save", journalId, commentId, userId, comment, xml, DataProvider.Instance().GetNull(dateUpdated));
        return commentId;
    }

    /// <inheritdoc/>
    public IDataReader Journal_Comment_List(int journalId)
    {
        return this.provider.ExecuteReader("Journal_Comment_List", journalId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Comment_Get(int commentId)
    {
        return this.provider.ExecuteReader("Journal_Comment_Get", commentId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Comment_ListByJournalIds(string journalIds)
    {
        return this.provider.ExecuteReader("Journal_Comment_ListByJournalIds", journalIds);
    }

    /// <inheritdoc/>
    public void Journal_Comment_Like(int journalId, int commentId, int userId, string displayName)
    {
        this.provider.ExecuteNonQuery("Journal_Comment_Like", journalId, commentId, userId, displayName);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Comment_LikeList(int portalId, int journalId, int commentId)
    {
        return this.provider.ExecuteReader("Journal_Comment_LikeList", portalId, journalId, commentId);
    }

    /// <inheritdoc/>
    public void Journal_Comments_ToggleDisable(int portalId, int journalId, bool disable)
    {
        this.provider.ExecuteNonQuery("Journal_Comments_ToggleDisable", portalId, journalId, disable);
    }

    /// <inheritdoc/>
    public void Journal_Comments_ToggleHidden(int portalId, int journalId, bool hidden)
    {
        this.provider.ExecuteNonQuery("Journal_Comments_ToggleHidden", portalId, journalId, hidden);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Types_List(int portalId)
    {
        return this.provider.ExecuteReader("Journal_Types_List", portalId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Types_GetById(int journalTypeId)
    {
        return this.provider.ExecuteReader("Journal_Types_GetById", journalTypeId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_Types_Get(string journalType)
    {
        return this.provider.ExecuteReader("Journal_Types_Get", journalType);
    }

    /// <inheritdoc/>
    public void Journal_Types_Delete(int journalTypeId, int portalId)
    {
        this.provider.ExecuteNonQuery("Journal_Types_Delete", journalTypeId, portalId);
    }

    /// <inheritdoc/>
    public int Journal_Types_Save(int journalTypeId, string journalType, string icon, int portalId, bool isEnabled, bool appliesToProfile, bool appliesToGroup, bool appliesToStream, string options, bool supportsNotify)
    {
        journalTypeId = this.provider.ExecuteScalar<int>(
            "Journal_Types_Save",
            journalTypeId,
            journalType,
            icon,
            portalId,
            isEnabled,
            appliesToProfile,
            appliesToGroup,
            appliesToStream,
            options,
            supportsNotify);
        return journalTypeId;
    }

    /// <inheritdoc/>
    public IDataReader Journal_GetStatsForGroup(int portalId, int groupId)
    {
        return this.provider.ExecuteReader("Journal_GetStatsForGroup", portalId, groupId);
    }

    /// <inheritdoc/>
    public IDataReader Journal_TypeFilters_List(int portalId, int moduleId)
    {
        return this.provider.ExecuteReader("Journal_TypeFilters_List", portalId, moduleId);
    }

    /// <inheritdoc/>
    public void Journal_TypeFilters_Delete(int portalId, int moduleId)
    {
        this.provider.ExecuteNonQuery("Journal_TypeFilters_Delete", portalId, moduleId);
    }

    /// <inheritdoc/>
    public void Journal_TypeFilters_Save(int portalId, int moduleId, int journalTypeId)
    {
        this.provider.ExecuteNonQuery("Journal_TypeFilters_Save", portalId, moduleId, journalTypeId);
    }
}
