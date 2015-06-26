// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent.Common;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;


namespace Dnn.DynamicContent
{
    public class ContentTemplateManager : ControllerBase<ContentTemplate, IContentTemplateManager, ContentTemplateManager>, IContentTemplateManager
    {
        internal const string ContentTemplateCacheKey = "ContentTypes_Templates";
        internal const string PortalScope = "PortalId";
        public const string TemplateNameKey = "Template_{0}_Name";

        protected override Func<IContentTemplateManager> GetFactory()
        {
            return () => new ContentTemplateManager();
        }

        public ContentTemplateManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ContentTemplateManager(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to add.</param>
        /// <returns>content template id.</returns>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        public int AddContentTemplate(ContentTemplate contentTemplate)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentTemplate, "Name");
            Requires.PropertyNotNegative(contentTemplate, "ContentTypeId");

            contentTemplate.CreatedByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            contentTemplate.CreatedOnDate = DateUtilitiesManager.Instance.GetDatabaseTime();

            Add(contentTemplate);

            ClearContentTypeCache(contentTemplate);

            return contentTemplate.TemplateId;
        }

        private void ClearContentTypeCache(ContentTemplate contentTemplate)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTemplate.ContentTypeId, contentTemplate.PortalId, true);

            if (contentType != null)
            {
                contentType.ClearTemplates();
            }
        }


        /// <summary>
        /// Deletes the content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to delete.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        public void DeleteContentTemplate(ContentTemplate contentTemplate)
        {
            Delete(contentTemplate);

            ClearContentTypeCache(contentTemplate);
        }

        /// <summary>
        /// GetContentTemplate overloads retrieves a singe content template
        /// </summary>
        /// <param name="templateId">The Id of the template</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be searched. Defaults to false</param>
        /// <returns>content template</returns>
        //TODO add Unit Tests for this method
        public ContentTemplate GetContentTemplate(int templateId, int portalId, bool includeSystem = false)
        {
            return GetContentTemplates(portalId, includeSystem).SingleOrDefault((t) => t.TemplateId == templateId);
        }

        /// <summary>
        /// This GetContentTemplates overloads retrieves all the content templates for a portal, optionally including system types
        /// </summary>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content template collection.</returns>
        public IQueryable<ContentTemplate> GetContentTemplates(int portalId, bool includeSystem = false)
        {
            List<ContentTemplate> templates = Get(portalId).ToList();
            if (includeSystem)
            {
                templates.AddRange(Get(-1));
            }
            return templates.AsQueryable();
        }

        /// <summary>
        /// Gets the content templates for a content Type
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type which this template is for</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content template collection.</returns>
        public IQueryable<ContentTemplate> GetContentTemplates(int contentTypeId, int portalId, bool includeSystem = false)
        {
            return GetContentTemplates(portalId, includeSystem).Where(ct => ct.ContentTypeId == contentTypeId);
        }

        /// <summary>
        /// This GetContentTemplates overload retrieves a page of content templates for a given portal, based on a Search Term that can appear anywhere in the Name.
        /// </summary>
        /// <param name="searchTerm">The search term to use</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page number - 0 represents the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>a PagedList of ContentTemplates</returns>
        public IPagedList<ContentTemplate> GetContentTemplates(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false)
        {
            var templates = GetContentTemplates(portalId, includeSystem);

            if (!String.IsNullOrEmpty(searchTerm))
            {
                templates = templates.Where(ct => ct.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));
            }

            return new PagedList<ContentTemplate>(templates, pageIndex, pageSize);
        }

        /// <summary>
        /// Updates the content template.
        /// </summary>
        /// <param name="contentTemplate">The content template.</param>
        /// <exception cref="System.ArgumentNullException">content template is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content template id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentTemplate.Name is empty.</exception>
        /// <exception cref="System.InvalidOperationException">contentTemplate is in use.</exception>
        public void UpdateContentTemplate(ContentTemplate contentTemplate)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentTemplate, "Name");
            Requires.PropertyNotNegative(contentTemplate, "ContentTypeId");

            contentTemplate.LastModifiedByUserId = UserController.Instance.GetCurrentUserInfo().UserID;
            contentTemplate.LastModifiedOnDate = DateUtilitiesManager.Instance.GetDatabaseTime();

            Update(contentTemplate);

            ClearContentTypeCache(contentTemplate);
        }
    }
}
