using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Content;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Content.Common;
using DotNetNuke.Common.Utilities;

namespace DotNetNuke.Entities.Groups {
    public class Content {
        /// <summary>
        /// This should only run after the Post exists in the data store. 
        /// </summary>
        /// <returns>The newly created ContentItemID from the data store.</returns>
        /// <remarks>This is for the first question in the thread. Not for replies or items with ParentID > 0.</remarks>
        internal ContentItem CreateContentItem(RoleInfo objItem, int tabId) {
            var typeController = new ContentTypeController();
            string contentTypeName = "DNNCorp_SocialGroup";
            if (objItem.RoleID > 0) {
                contentTypeName = "DNNCorp_SocialGroup";
            }
            var colContentTypes = (from t in typeController.GetContentTypes() where t.ContentType == contentTypeName select t);
            int contentTypeID;

            if (colContentTypes.Count() > 0) {
                var contentType = colContentTypes.Single();
                contentTypeID = contentType == null ? CreateContentType(contentTypeName) : contentType.ContentTypeId;
            } else {
                contentTypeID = CreateContentType(contentTypeName);
            }

            var objContent = new ContentItem {
                Content = objItem.RoleName,
                ContentTypeId = contentTypeID,
                Indexed = false,
                ContentKey = "GroupId=" + objItem.RoleID,
                ModuleID = -1,
                TabID = tabId
            };

            objContent.ContentItemId = Util.GetContentController().AddContentItem(objContent);

            // Add Terms
            //var cntTerm = new Terms();
            //cntTerm.ManageQuestionTerms(objPost, objContent);

            return objContent;
        }

        /// <summary>
        /// This is used to update the content in the ContentItems table. Should be called when a question is updated.
        /// </summary>
        internal void UpdateContentItem(RoleInfo objItem, int tabId) {
            ContentItem objContent = null; //Util.GetContentController().;

            if (objContent == null) return;
            objContent.Content = objItem.RoleName;
            objContent.TabID = tabId;
            objContent.ContentKey = "GroupId=" + objItem.RoleID; // we reset this just in case the page changed.

            Util.GetContentController().UpdateContentItem(objContent);

            // Update Terms
            //var cntTerm = new Terms();
            //cntTerm.ManageQuestionTerms(objPost, objContent);
        }

        /// <summary>
        /// This removes a content item associated with a question/thread from the data store. Should run every time an entire thread is deleted.
        /// </summary>
        /// <param name="contentItemID"></param>
        internal void DeleteContentItem(int contentItemID) {
            if (contentItemID <= Null.NullInteger) return;
            var objContent = Util.GetContentController().GetContentItem(contentItemID);
            if (objContent == null) return;

            // remove any metadata/terms associated first (perhaps we should just rely on ContentItem cascade delete here?)
            //var cntTerms = new Terms();
            //cntTerms.RemoveQuestionTerms(objContent);

            Util.GetContentController().DeleteContentItem(objContent);
        }

        /// <summary>
        /// This is used to determine the ContentTypeID (part of the Core API) based on this module's content type. If the content type doesn't exist yet for the module, it is created.
        /// </summary>
        /// <returns>The primary key value (ContentTypeID) from the core API's Content Types table.</returns>
        internal static int GetContentTypeID(string ContentTypeName) {
            var typeController = new ContentTypeController();
            var colContentTypes = (from t in typeController.GetContentTypes() where t.ContentType == ContentTypeName select t);
            int contentTypeId;

            if (colContentTypes.Count() > 0) {
                var contentType = colContentTypes.Single();
                contentTypeId = contentType == null ? CreateContentType(ContentTypeName) : contentType.ContentTypeId;
            } else {
                contentTypeId = CreateContentType(ContentTypeName);
            }

            return contentTypeId;
        }

        #region Private Methods

        /// <summary>
        /// Creates a Content Type (for taxonomy) in the data store.
        /// </summary>
        /// <returns>The primary key value of the new ContentType.</returns>
        private static int CreateContentType(string ContentTypeName) {
            var typeController = new ContentTypeController();
            var objContentType = new ContentType { ContentType = ContentTypeName };

            return typeController.AddContentType(objContentType);
        }
      
        #endregion

    }
}