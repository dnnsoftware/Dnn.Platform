// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.FileSystem;

    public interface IJournalController
    {
        // Journal Items

        /// <summary>
        /// Get journal type by name.
        /// </summary>
        /// <param name="journalType">type name.</param>
        /// <returns>Journal type object.</returns>
        ///
        JournalTypeInfo GetJournalType(string journalType);

        /// <summary>
        /// Get journal type by type id.
        /// </summary>
        /// <param name="journalTypeId">Type id.</param>
        /// <returns>Journal type object.</returns>
        ///
        JournalTypeInfo GetJournalTypeById(int journalTypeId);

        /// <summary>
        /// Get journal item by object key.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="objectKey">The object key.</param>
        /// <returns>Journal object.</returns>
        ///
        JournalItem GetJournalItemByKey(int portalId, string objectKey);

        /// <summary>
        /// Get journal item by object key.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="objectKey">The object key.</param>
        /// <param name="includeAllItems">Whether include deleted items.</param>
        /// <returns></returns>
        ///
        JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems);

        /// <summary>
        /// Get journal item by object key.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="objectKey">The object key.</param>
        /// <param name="includeAllItems">Whether include deleted items.</param>
        /// <param name="isDeleted">Whether get deleted item.</param>
        /// <returns></returns>
        ///
        JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted);

        /// <summary>
        /// Get journal info.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user, if securityCheck set as True, will check whether this user has permission to view the journal.</param>
        /// <param name="journalId">Id of Journal.</param>
        /// <returns>Journal Object.</returns>
        ///
        JournalItem GetJournalItem(int portalId, int userId, int journalId);

        /// <summary>
        /// Get journal info.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user, if securityCheck set as True, will check whether this user has permission to view the journal.</param>
        /// <param name="journalId">Id of Journal.</param>
        /// <param name="includeAllItems">Whether include deleted items.</param>
        /// <returns>Journal Object.</returns>
        ///
        JournalItem GetJournalItem(int portalId, int userId, int journalId, bool includeAllItems);

        /// <summary>
        /// Get journal info.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user, if securityCheck set as True, will check whether this user has permission to view the journal.</param>
        /// <param name="journalId">Id of Journal.</param>
        /// <param name="includeAllItems">Whether include deleted items.</param>
        /// <param name="isDeleted">Whether get deleted item.</param>
        /// <returns>Journal Object.</returns>
        ///
        JournalItem GetJournalItem(int portalId, int userId, int journalId, bool includeAllItems, bool isDeleted);

        /// <summary>
        /// Get journal info.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user, if securityCheck set as True, will check whether this user has permission to view the journal.</param>
        /// <param name="journalId">Id of Journal.</param>
        /// <param name="includeAllItems">Whether include deleted items.</param>
        /// <param name="isDeleted">Whether get deleted item.</param>
        /// <param name="securityCheck">Whether check current user has permission to get journal.</param>
        /// <returns>Journal Object.</returns>
        ///
        JournalItem GetJournalItem(int portalId, int userId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck);

        /// <summary>
        /// Get all journal types in portal.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <returns>Enumerable JournalTypeInfo object list.</returns>
        IEnumerable<JournalTypeInfo> GetJournalTypes(int portalId);

        /// <summary>
        /// Save an uploaded file.
        /// </summary>
        /// <param name="module">Module where the file is uploaded.</param>
        /// <param name="userInfo">User who uploads the file.</param>
        /// <param name="fileName">File Name.</param>
        /// <param name="fileContent">File content.</param>
        /// <returns>A FileInfo object corresponding to the saved file.</returns>
        IFileInfo SaveJourmalFile(ModuleInfo module, UserInfo userInfo, string fileName, Stream fileContent);

        /// <summary>
        /// Save the journal object into database.
        /// </summary>
        /// <param name="journalItem">Journal object.</param>
        /// <param name="module">The module info of journal item context.</param>
        ///
        void SaveJournalItem(JournalItem journalItem, ModuleInfo module);

        /// <summary>
        /// Update the journal info in database.
        /// </summary>
        /// <param name="journalItem">Journal object.</param>
        /// <param name="module">The module info of journal item context.</param>
        ///
        void UpdateJournalItem(JournalItem journalItem, ModuleInfo module);

        /// <summary>
        /// Hard delete journal item by journal id.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user.</param>
        /// <param name="journalId">Id of the journal want to delete.</param>
        ///
        void DeleteJournalItem(int portalId, int userId, int journalId);

        /// <summary>
        /// Hard delete journal items based on group Id.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="groupId">Id of social group.</param>
        ///
        void DeleteJournalItemByGroupId(int portalId, int groupId);

        /// <summary>
        /// Hard delete journal item by object key.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="objectKey">Object key.</param>
        ///
        void DeleteJournalItemByKey(int portalId, string objectKey);

        /// <summary>
        /// Soft delete journal item by journal id.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="userId">Id of current user.</param>
        /// <param name="journalId">Id of the journal want to delete.</param>
        ///
        void SoftDeleteJournalItem(int portalId, int userId, int journalId);

        /// <summary>
        /// Soft delete journal items based on group Id.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="groupId">Id of social group.</param>
        ///
        void SoftDeleteJournalItemByGroupId(int portalId, int groupId);

        /// <summary>
        /// Soft delete journal item by object key.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="objectKey">Object key.</param>
        ///
        void SoftDeleteJournalItemByKey(int portalId, string objectKey);

        /// <summary>
        /// Disable comment on a journal item.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="journalId">Id of the journal want to disable comment.</param>
        ///
        void DisableComments(int portalId, int journalId);

        /// <summary>
        /// Hide comments on a journal item.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="journalId">Id of the journal want to hide comments.</param>
        ///
        void HideComments(int portalId, int journalId);

        /// <summary>
        /// Enable comment on a journal item.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="journalId">Id of the journal want to enable comment.</param>
        ///
        void EnableComments(int portalId, int journalId);

        /// <summary>
        /// Show comments on a  journal item.
        /// </summary>
        /// <param name="portalId">Id of portal.</param>
        /// <param name="journalId">Id of the journal want to show comment.</param>
        ///
        void ShowComments(int portalId, int journalId);

        /// <summary>
        /// Get all comments in the given journal items.
        /// </summary>
        /// <param name="journalIdList">Id list of journal items.</param>
        /// <returns>CommentInfo object list.</returns>
        ///
        IList<CommentInfo> GetCommentsByJournalIds(List<int> journalIdList);

        /// <summary>
        /// Save a like on journal item.
        /// </summary>
        /// <param name="journalId">Id of journal item.</param>
        /// <param name="userId">Id of current user.</param>
        /// <param name="displayName">User's display name.</param>
        ///
        void LikeJournalItem(int journalId, int userId, string displayName);

        /// <summary>
        /// Save comment info object into database.
        /// </summary>
        /// <param name="ci">Comment info object.</param>
        ///
        void SaveComment(CommentInfo ci);

        /// <summary>
        /// Get comment info object by comment id.
        /// </summary>
        /// <param name="commentId">Id of the comment.</param>
        /// <returns>Comment info object.</returns>
        ///
        CommentInfo GetComment(int commentId);

        /// <summary>
        /// Delete a comment by id.
        /// </summary>
        /// <param name="journalId">Id of journal.</param>
        /// <param name="commentId">Id of comment.</param>
        ///
        void DeleteComment(int journalId, int commentId);

        /// <summary>
        /// Save a like on comment.
        /// </summary>
        /// <param name="journalId">Id of journal.</param>
        /// <param name="commentId">Id of comment.</param>
        /// <param name="userId">Id of current user.</param>
        /// <param name="displayName">User's display name.</param>
        void LikeComment(int journalId, int commentId, int userId, string displayName);

        [Obsolete("Deprecated in DNN 7.2.2. Use SaveJournalItem(JournalItem, ModuleInfo). Scheduled removal in v10.0.0.")]
        void SaveJournalItem(JournalItem journalItem, int tabId);

        [Obsolete("Deprecated in DNN 7.2.2. Use UpdateJournalItem(JournalItem, ModuleInfo). Scheduled removal in v10.0.0.")]
        void UpdateJournalItem(JournalItem journalItem, int tabId);
    }
}
