#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using System;
using System.Collections.Generic;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Services.Journal
{
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
        /// Hard delete journal items based on group Id
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
        /// Soft delete journal items based on group Id
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
        IList<CommentInfo> GetCommentsByJournalIds(List <int> journalIdList);

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

        [Obsolete("Deprecated in DNN 7.2.2. Use SaveJournalItem(JournalItem, ModuleInfo)")]
        void SaveJournalItem(JournalItem journalItem, int tabId);

        [Obsolete("Deprecated in DNN 7.2.2. Use UpdateJournalItem(JournalItem, ModuleInfo)")]
        void UpdateJournalItem(JournalItem journalItem, int tabId);
    }
}