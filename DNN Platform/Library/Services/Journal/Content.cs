// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System.Linq;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Common;

    public class Content
    {
        /// <summary>
        /// This is used to determine the ContentTypeID (part of the Core API) based on this module's content type. If the content type doesn't exist yet for the module, it is created.
        /// </summary>
        /// <returns>The primary key value (ContentTypeID) from the core API's Content Types table.</returns>
        internal static int GetContentTypeID(string ContentTypeName)
        {
            var typeController = new ContentTypeController();
            var colContentTypes = from t in typeController.GetContentTypes() where t.ContentType == ContentTypeName select t;
            int contentTypeId;

            if (colContentTypes.Count() > 0)
            {
                var contentType = colContentTypes.Single();
                contentTypeId = contentType == null ? CreateContentType(ContentTypeName) : contentType.ContentTypeId;
            }
            else
            {
                contentTypeId = CreateContentType(ContentTypeName);
            }

            return contentTypeId;
        }

        /// <summary>
        /// This should only run after the Post exists in the data store.
        /// </summary>
        /// <returns>The newly created ContentItemID from the data store.</returns>
        /// <remarks>This is for the first question in the thread. Not for replies or items with ParentID > 0.</remarks>
        internal ContentItem CreateContentItem(JournalItem objJournalItem, int tabId, int moduleId)
        {
            var typeController = new ContentTypeController();
            string contentTypeName = "DNNCorp_JournalProfile";
            if (objJournalItem.SocialGroupId > 0)
            {
                contentTypeName = "DNNCorp_JournalGroup";
            }

            var colContentTypes = from t in typeController.GetContentTypes() where t.ContentType == contentTypeName select t;
            int contentTypeID;

            if (colContentTypes.Count() > 0)
            {
                var contentType = colContentTypes.Single();
                contentTypeID = contentType == null ? CreateContentType(contentTypeName) : contentType.ContentTypeId;
            }
            else
            {
                contentTypeID = CreateContentType(contentTypeName);
            }

            var objContent = new ContentItem
            {
                Content = GetContentBody(objJournalItem),
                ContentTypeId = contentTypeID,
                Indexed = false,
                ContentKey = "journalid=" + objJournalItem.JournalId,
                ModuleID = moduleId,
                TabID = tabId,
            };

            objContent.ContentItemId = Util.GetContentController().AddContentItem(objContent);

            // Add Terms
            // var cntTerm = new Terms();
            // cntTerm.ManageQuestionTerms(objPost, objContent);
            return objContent;
        }

        /// <summary>
        /// This is used to update the content in the ContentItems table. Should be called when a question is updated.
        /// </summary>
        internal void UpdateContentItem(JournalItem objJournalItem, int tabId, int moduleId)
        {
            var objContent = Util.GetContentController().GetContentItem(objJournalItem.ContentItemId);

            if (objContent == null)
            {
                return;
            }

            // Only update content the contentitem if it was created by the journal
            if ((objContent.ContentTypeId == GetContentTypeID("DNNCorp_JournalProfile") && objJournalItem.ProfileId > 0)
                    || (objContent.ContentTypeId == GetContentTypeID("DNNCorp_JournalGroup") && objJournalItem.SocialGroupId > 0))
            {
                objContent.Content = GetContentBody(objJournalItem);
                objContent.TabID = tabId;
                objContent.ModuleID = moduleId;
                objContent.ContentKey = "journalid=" + objJournalItem.JournalId; // we reset this just in case the page changed.

                Util.GetContentController().UpdateContentItem(objContent);
            }

            // Update Terms
            // var cntTerm = new Terms();
            // cntTerm.ManageQuestionTerms(objPost, objContent);
        }

        /// <summary>
        /// This removes a content item associated with a question/thread from the data store. Should run every time an entire thread is deleted.
        /// </summary>
        /// <param name="contentItemID"></param>
        internal void DeleteContentItem(int contentItemID)
        {
            if (contentItemID <= Null.NullInteger)
            {
                return;
            }

            var objContent = Util.GetContentController().GetContentItem(contentItemID);
            if (objContent == null)
            {
                return;
            }

            // remove any metadata/terms associated first (perhaps we should just rely on ContentItem cascade delete here?)
            // var cntTerms = new Terms();
            // cntTerms.RemoveQuestionTerms(objContent);
            Util.GetContentController().DeleteContentItem(objContent);
        }

        /// <summary>
        /// Creates a Content Type (for taxonomy) in the data store.
        /// </summary>
        /// <returns>The primary key value of the new ContentType.</returns>
        private static int CreateContentType(string ContentTypeName)
        {
            var typeController = new ContentTypeController();
            var objContentType = new ContentType { ContentType = ContentTypeName };

            return typeController.AddContentType(objContentType);
        }

        /// <summary>
        /// Creates the content text.
        /// </summary>
        /// <param name="objJournalItem"></param>
        /// <returns></returns>
        private static string GetContentBody(JournalItem objJournalItem)
        {
            if (!string.IsNullOrEmpty(objJournalItem.Title))
            {
                return objJournalItem.Title;
            }
            else if (!string.IsNullOrEmpty(objJournalItem.Summary))
            {
                return objJournalItem.Summary;
            }
            else if (!string.IsNullOrEmpty(objJournalItem.Body))
            {
                return objJournalItem.Body;
            }
            else
            {
                return null;
            }
        }
    }
}
