// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent.Common;
using Dnn.DynamicContent.Exceptions;
using Dnn.DynamicContent.Localization;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;

namespace Dnn.DynamicContent
{
    public class DynamicContentTypeManager : ControllerBase<DynamicContentType, IDynamicContentTypeManager, DynamicContentTypeManager>, IDynamicContentTypeManager
    {
        internal const string StructuredWhereClause = "WHERE PortalID = @0 AND IsStructured = 1";
        public const string NameKey = "ContentType_{0}_Name";
        public const string DescriptionKey = "ContentType_{0}_Description";

        protected override Func<IDynamicContentTypeManager> GetFactory()
        {
            return () => new DynamicContentTypeManager();
        }

        public DynamicContentTypeManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public DynamicContentTypeManager(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        /// <exception cref="SystemContentTypeSecurityException">system content types can only be added by Super Users</exception>
        public int AddContentType(DynamicContentType contentType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "Name");

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (contentType.IsSystem && !currentUser.IsSuperUser)
            {
                throw new SystemContentTypeSecurityException();
            }

            contentType.CreatedByUserId = currentUser.UserID;
            contentType.CreatedOnDate = DateUtilitiesManager.Instance.GetDatabaseTime();

            Add(contentType);

            //Save Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                definition.ContentTypeId = contentType.ContentTypeId;
                FieldDefinitionManager.Instance.AddFieldDefinition(definition);
            }

            //Save Content Templates
            foreach (var template in contentType.Templates)
            {
                template.ContentTypeId = contentType.ContentTypeId;
                ContentTemplateManager.Instance.AddContentTemplate(template);
            }

            return contentType.ContentTypeId;
        }

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="SystemContentTypeSecurityException">system content types can only be deleted by Super Users</exception>
        /// <exception cref="DynamicContentTypeDoesNotExistException">requested content type by ContentTypeId and PortalId does not exist</exception>
        public void DeleteContentType(DynamicContentType contentType)
        {
            Requires.NotNull(contentType);
            Requires.PropertyNotNegative(contentType, "ContentTypeId");
            
            var storedContentType = GetContentType(contentType.ContentTypeId, contentType.PortalId, true);
            if (storedContentType == null)
            {
                throw new DynamicContentTypeDoesNotExistException();
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (storedContentType.IsSystem && !currentUser.IsSuperUser)
            {
                throw new SystemContentTypeSecurityException();
            }

            //Delete Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                FieldDefinitionManager.Instance.DeleteFieldDefinition(definition);
            }

            //Delete Content Templates
            foreach (var template in contentType.Templates)
            {
                ContentTemplateManager.Instance.DeleteContentTemplate(template);
            }

            //Delete Localizations
            ContentTypeLocalizationManager.Instance.DeleteLocalizations(contentType.PortalId, String.Format(NameKey, contentType.ContentTypeId));
            ContentTypeLocalizationManager.Instance.DeleteLocalizations(contentType.PortalId, String.Format(DescriptionKey, contentType.ContentTypeId));

            Delete(contentType);
        }

        /// <summary>
        /// GetContentType retrieves a dynamic content type for a portal, optionally including system types
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System content types (ie. content types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type</returns>
        public DynamicContentType GetContentType(int contentTypeId, int portalId, bool includeSystem = false)
        {
            DynamicContentType contentType = Get(portalId).SingleOrDefault(ct => ct.ContentTypeId == contentTypeId);

            if (contentType == null && includeSystem)
            {
                contentType = Get(-1).SingleOrDefault(dt => dt.ContentTypeId == contentTypeId);
            }
            return contentType;
        }

        /// <summary>
        /// Gets the content types for a specific portal.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <param name="includeSystem">A flag to determine if System Content Types (ie. Content Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type collection.</returns>
        public IQueryable<DynamicContentType> GetContentTypes(int portalId, bool includeSystem = false)
        {
            var contentTypes = portalId > -1 
                                ? Get(portalId).ToList() 
                                : Get(portalId).Where(t => t.IsDynamic).ToList();

            if (includeSystem && portalId > -1)
            {
                contentTypes.AddRange(Get(-1).Where(t => t.IsDynamic));
            }
            return contentTypes.AsQueryable();
        }

        /// <summary>
        /// Gets a page of content types for a specific portal.
        /// </summary>
        /// <param name="searchTerm">The search term to use</param>
        /// <param name="portalId">The portalId</param>
        /// <param name="pageIndex">The page index to return</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeSystem">A flag to determine if System Content Types (ie. Content Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type collection.</returns>
        public IPagedList<DynamicContentType> GetContentTypes(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false)
        {
            var contentTypes = GetContentTypes(portalId, includeSystem);

            if (!String.IsNullOrEmpty(searchTerm))
            {
                contentTypes = contentTypes.Where(dt => dt.Name.ToLowerInvariant().Contains(searchTerm.ToLowerInvariant()));
            }

            return new PagedList<DynamicContentType>(contentTypes, pageIndex, pageSize);
        }

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        /// <exception cref="SystemContentTypeSecurityException">system content types can only be modified by Super Users</exception>
        /// <exception cref="DynamicContentTypeDoesNotExistException">requested content type by ContentTypeId and PortalId does not exist</exception>
        public void UpdateContentType(DynamicContentType contentType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "Name");
            Requires.PropertyNotNegative(contentType, "ContentTypeId");
            
            var storedContentType = GetContentType(contentType.ContentTypeId, contentType.PortalId, true);
            if (storedContentType == null)
            {
                throw new DynamicContentTypeDoesNotExistException();
            }

            var currentUser = UserController.Instance.GetCurrentUserInfo();
            if (storedContentType.IsSystem && !currentUser.IsSuperUser)
            {
                throw new SystemContentTypeSecurityException();
            }

            contentType.LastModifiedByUserId = currentUser.UserID;
            contentType.LastModifiedOnDate = DateUtilitiesManager.Instance.GetDatabaseTime();

            Update(contentType);

            //Save Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                if (definition.FieldDefinitionId == -1)
                {
                    definition.ContentTypeId = contentType.ContentTypeId;
                    FieldDefinitionManager.Instance.AddFieldDefinition(definition);
                }
                else
                {
                    FieldDefinitionManager.Instance.UpdateFieldDefinition(definition);
                }
            }

            //Save Content Templates
            foreach (var template in contentType.Templates)
            {
                if (template.TemplateId == -1)
                {
                    template.ContentTypeId = contentType.ContentTypeId;
                    ContentTemplateManager.Instance.AddContentTemplate(template);
                }
                else
                {
                    ContentTemplateManager.Instance.UpdateContentTemplate(template);
                }
            }

        }
    }
}
