// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContentTypeController : DnnApiController
    {
        /// <summary>
        /// DeleteContentField deletes a single ContentField
        /// </summary>
        /// <param name="viewModel">The ContentField to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentField(ContentFieldViewModel viewModel)
        {
            var contentField = FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId);

            if (contentField != null)
            {
                FieldDefinitionManager.Instance.DeleteFieldDefinition(contentField);
            }

            var response = new
                            {
                                success = true
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// DeleteContentType deletes a single ContentType
        /// </summary>
        /// <param name="viewModel">The ContentType to delete</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteContentType(ContentTypeViewModel viewModel)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true);

            if (contentType != null)
            {
                DynamicContentTypeManager.Instance.DeleteContentType(contentType);
            }

            var response = new
                            {
                                success = true
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetContentField retrieves a single Content Field
        /// </summary>
        /// <param name="contentTypeId">The id of the Content Type</param>
        /// <param name="contentFieldId">The id of the Content Field</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentField(int contentTypeId, int contentFieldId)
        {
            var contentField = FieldDefinitionManager.Instance.GetFieldDefinitions(contentTypeId).SingleOrDefault((c) => c.FieldDefinitionId == contentFieldId);

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            contentField = new ContentFieldViewModel(contentField)
                                        }
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetContentFields retrieves a page of Content Fields
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <param name="pageIndex">The page to fetch</param>
        /// <param name="pageSize">The size of page to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentFields(int contentTypeId, int pageIndex, int pageSize)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);

            var response = new
            {
                success = true,
                data = new
                        {
                            contentFields = new ContentFieldsViewModel(contentType.FieldDefinitions, pageIndex, pageSize)
                        }
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetContentType retrieves a single ContentType
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentType(int contentTypeId)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);

            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            contentType = new ContentTypeViewModel(contentType, PortalSettings.UserInfo.IsSuperUser, true)
                                        }
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// GetContentTypes retrieves a page of ContentTypes that satisfy the searchTerm
        /// </summary>
        /// <param name="searchTerm">The text to use to search for ContentTypes</param>
        /// <param name="pageIndex">The page to return, where 0 is the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentTypes(string searchTerm, int pageIndex, int pageSize)
        {
            var contentTypeList = DynamicContentTypeManager.Instance.GetContentTypes(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true);
            var contentTypes = contentTypeList
                                .Select(contentType => new ContentTypeViewModel(contentType, PortalSettings.UserInfo.IsSuperUser))
                                .ToList();

            var response = new
                            {
                                success = true,
                                data = new
                                {
                                    results = contentTypes,
                                    totalResults = contentTypeList.TotalCount
                                }
                            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// SaveContentField saves the content field
        /// </summary>
        /// <param name="viewModel">The ViewModel for the Content Field to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveContentField(ContentFieldViewModel viewModel)
        {
            DynamicContentType contentType = DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true);

            FieldDefinition contentField;

            if (viewModel.ContentFieldId == -1)
            {
                contentField = new FieldDefinition()
                                                {
                                                    ContentTypeId = contentType.ContentTypeId,
                                                    DataTypeId = viewModel.DataTypeId,
                                                    Label = viewModel.Label,
                                                    Name = viewModel.Name,
                                                    Description = viewModel.Description,
                                                    PortalId = contentType.PortalId
                                                };
                FieldDefinitionManager.Instance.AddFieldDefinition(contentField);
            }
            else
            {
                //Update
                contentField = FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId);

                if (contentField != null)
                {
                    contentField.Name = viewModel.Name;
                    contentField.Description = viewModel.Description;
                    contentField.Label = viewModel.Label;
                    contentField.DataTypeId = viewModel.DataTypeId;
                    FieldDefinitionManager.Instance.UpdateFieldDefinition(contentField);
                }
            }
            var response = new
            {
                success = true,
                data = new
                        {
                            contentFieldId = contentField.FieldDefinitionId
                        }
            };

            return Request.CreateResponse(response);
        }

        /// <summary>
        /// SaveContentType saves the content type
        /// </summary>
        /// <param name="viewModel">The ViewModel for the DataType to save</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SaveContentType(ContentTypeViewModel viewModel)
        {
            DynamicContentType contentType;

            if (viewModel.ContentTypeId == -1)
            {
                contentType = new DynamicContentType()
                                        {
                                            Name = viewModel.Name,
                                            Description = viewModel.Description,
                                            PortalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId
                                        };
                DynamicContentTypeManager.Instance.AddContentType(contentType);
            }
            else
            {
                //Update
                contentType = DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true);

                if (contentType != null)
                {
                    contentType.Name = viewModel.Name;
                    contentType.Description = viewModel.Description;
                    DynamicContentTypeManager.Instance.UpdateContentType(contentType);
                }
            }
            var response = new
                            {
                                success = true,
                                data = new
                                        {
                                            contentTypeId = contentType.ContentTypeId
                                        }
                            };

            return Request.CreateResponse(response);
        }
    }
}
