// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent
{
    public class DynamicContentTypeController : ControllerBase<DynamicContentType, IDynamicContentTypeController, DynamicContentTypeController>, IDynamicContentTypeController
    {
        internal const string StructuredWhereClause = "WHERE PortalID = @0 AND IsStructured = 1";

        protected override Func<IDynamicContentTypeController> GetFactory()
        {
            return () => new DynamicContentTypeController();
        }

        public DynamicContentTypeController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public DynamicContentTypeController(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public int AddContentType(DynamicContentType contentType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "ContentType");

            Add(contentType);

            //Save Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                definition.ContentTypeId = contentType.ContentTypeId;
                FieldDefinitionController.Instance.AddFieldDefinition(definition);
            }

            //Save Content Templates
            foreach (var template in contentType.Templates)
            {
                template.ContentTypeId = contentType.ContentTypeId;
                ContentTemplateController.Instance.AddContentTemplate(template);
            }

            return contentType.ContentTypeId;
        }

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        public void DeleteContentType(DynamicContentType contentType)
        {
            Delete(contentType);

            //Delete Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                FieldDefinitionController.Instance.DeleteFieldDefinition(definition);
            }

            //Delete Content Templates
            foreach (var template in contentType.Templates)
            {
                ContentTemplateController.Instance.DeleteContentTemplate(template);
            }
        }

        /// <summary>
        /// Gets the content types for a specific portal.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <returns>content type collection.</returns>
        public IQueryable<DynamicContentType> GetContentTypes(int portalId)
        {
            IQueryable<DynamicContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<DynamicContentType>();

                contentTypes = rep.Get(portalId).AsQueryable();
            }

            return contentTypes;
        }

        /// <summary>
        /// Gets a page of content types for a specific portal.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <param name="pageIndex">The page index to return</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>content type collection.</returns>
        public IPagedList<DynamicContentType> GetContentTypes(int portalId, int pageIndex, int pageSize)
        {
            IPagedList<DynamicContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<DynamicContentType>();

                contentTypes = rep.GetPage(portalId, pageIndex, pageSize);
            }

            return contentTypes;
        }

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public void UpdateContentType(DynamicContentType contentType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "ContentType");

            Update(contentType);

            //Save Field Definitions
            foreach (var definition in contentType.FieldDefinitions)
            {
                if (definition.FieldDefinitionId == -1)
                {
                    definition.ContentTypeId = contentType.ContentTypeId;
                    FieldDefinitionController.Instance.AddFieldDefinition(definition);
                }
                else
                {
                    FieldDefinitionController.Instance.UpdateFieldDefinition(definition);
                }
            }

            //Save Content Templates
            foreach (var template in contentType.Templates)
            {
                if (template.TemplateId == -1)
                {
                    template.ContentTypeId = contentType.ContentTypeId;
                    ContentTemplateController.Instance.AddContentTemplate(template);
                }
                else
                {
                    ContentTemplateController.Instance.UpdateContentTemplate(template);
                }
            }

        }
    }
}
