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
using System;
using System.Collections.Generic;

namespace DotNetNuke.Services.Journal
{
    public interface IJournalController
    {
        // Journal Items
        JournalTypeInfo GetJournalType(string journalType);
        JournalTypeInfo GetJournalTypeById(int journalTypeId);
        JournalItem GetJournalItemByKey(int portalId, string objectKey);
        JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems);
        JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted);
        JournalItem GetJournalItem(int portalId, int userId, int journalId);
        JournalItem GetJournalItem(int portalId, int userId, int journalId, bool includeAllItems);
        JournalItem GetJournalItem(int portalId, int userId, int journalId, bool includeAllItems, bool isDeleted);
        IEnumerable<JournalTypeInfo> GetJournalTypes(int portalId);
        void SaveJournalItem(JournalItem journalItem, int tabId);
        void UpdateJournalItem(JournalItem journalItem, int tabId);

        // Delete Journal Items
        void DeleteJournalItem(int portalId, int userId, int journalId);
        void DeleteJournalItemByGroupId(int portalId, int groupId);
        void DeleteJournalItemByKey(int portalId, string objectKey);
        void SoftDeleteJournalItem(int portalId, int userId, int journalId);
        void SoftDeleteJournalItemByGroupId(int portalId, int groupId);
        void SoftDeleteJournalItemByKey(int portalId, string objectKey);
        
        // Journal Comments
        void DisableComments(int portalId, int journalId);
        void HideComments(int portalId, int journalId);
        void EnableComments(int portalId, int journalId);
        void ShowComments(int portalId, int journalId);
        IList<CommentInfo> GetCommentsByJournalIds(List <int> journalIdList);
        void LikeJournalItem(int journalId, int userId, string displayName);
        void SaveComment(CommentInfo ci);
        CommentInfo GetComment(int commentId);
        void DeleteComment(int journalId, int commentId);
        void LikeComment(int journalId, int commentId, int userId, string displayName);
    }
}