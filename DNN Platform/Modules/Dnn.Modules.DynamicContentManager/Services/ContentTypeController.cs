// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using Dnn.DynamicContent;
using Dnn.DynamicContent.Exceptions;
using Dnn.DynamicContent.Localization;
using Dnn.Modules.DynamicContentManager.Services.ViewModels;
using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Web.Api;
using DotNetNuke.Services.Localization;

namespace Dnn.Modules.DynamicContentManager.Services
{
    /// <summary>
    /// ContentTypeController provides the Web Services to manage Data Types
    /// </summary>
    [SupportedModules("Dnn.DynamicContentManager")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ContentTypeController : BaseController
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
            return DeleteEntity(() => FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId),
                                contentField => FieldDefinitionManager.Instance.DeleteFieldDefinition(contentField));
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
            try
            {
                return DeleteEntity(() => DynamicContentTypeManager.Instance.GetContentType(viewModel.ContentTypeId, PortalSettings.PortalId, true),
                                contentType => DynamicContentTypeManager.Instance.DeleteContentType(contentType));
            }
            catch (ContentTypeInUseException ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateErrorResponse((HttpStatusCode)HttpStatusCodeAdditions.UnprocessableEntity, ex.Message);
            }            
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
            return GetEntity(() => FieldDefinitionManager.Instance.GetFieldDefinitions(contentTypeId).SingleOrDefault(c => c.FieldDefinitionId == contentFieldId),
                           contentField => new ContentFieldViewModel(contentField, PortalSettings));
        }

        /// <summary>
        /// GetAllContentFields retrieves all the content fields including those of enclosed content types
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetAllContentFields(int contentTypeId)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);

            var response = new
                            {
                                results = ProcessFields(contentType, String.Empty)
                            };

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// GetContentFields retrieves a page of Content Fields
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <param name="pageIndex">The page to fetch</param>
        /// <param name="pageSize">The size of page to fetch</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentFields(int contentTypeId, int pageIndex = 0, int pageSize = 999)
        {
            return GetPage<FieldDefinition, ContentFieldViewModel>(
                                () =>
                                    {
                                        var contentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true);
                                        return new PagedList<FieldDefinition>(contentType.FieldDefinitions, pageIndex, pageSize);
                                    },
                                contentField => new ContentFieldViewModel(contentField, PortalSettings));
        }

        /// <summary>
        /// Move a Field's position in the list
        /// </summary>
        /// <param name="viewModel">A ViewModel (DTO) represneting the object to be moved</param>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MoveContentField(MoveContentFieldViewModel viewModel)
        {
            FieldDefinitionManager.Instance.MoveFieldDefintion(viewModel.ContentTypeId, viewModel.SourceIndex, viewModel.TargetIndex);

            return Request.CreateResponse(HttpStatusCode.OK, new {});
        }

        /// <summary>
        /// GetContentType retrieves a single ContentType
        /// </summary>
        /// <param name="contentTypeId">The id of the ContentType</param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage GetContentType(int contentTypeId)
        {
            return GetEntity(() => DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalSettings.PortalId, true),
                           contentType => new ContentTypeViewModel(contentType, PortalSettings, true));
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
            return GetPage(() => DynamicContentTypeManager.Instance.GetContentTypes(searchTerm, PortalSettings.PortalId, pageIndex, pageSize, true),
                            contentType => new ContentTypeViewModel(contentType, PortalSettings));

        }

        private ArrayList ProcessFields(DynamicContentType contentType, string prefix)
        {
            var fields = new ArrayList();

            var locale = Thread.CurrentThread.CurrentUICulture.ToString();

            foreach (var definition in contentType.FieldDefinitions)
            {
                var key = String.Format(FieldDefinitionManager.NameKey, definition.FieldDefinitionId);
                var fieldName = definition.Name;
                var localizedName = (locale == PortalSettings.DefaultLanguage)
                                        ? fieldName
                                        : ContentTypeLocalizationManager.Instance.GetLocalizedValue(key, locale, PortalSettings.PortalId);

                if (definition.IsReferenceType)
                {
                    var newPrefix = (String.IsNullOrEmpty(prefix)) ? fieldName : prefix + fieldName;
                    newPrefix += "/";

                    var field = new
                                {
                                    name = localizedName,
                                    fields = ProcessFields(definition.ContentType, newPrefix)
                                };

                    fields.Add(field);
                }
                else
                {

                    var field = new
                                {
                                    fieldName = prefix + fieldName,
                                    name = localizedName
                                };

                    fields.Add(field);
                }
            }

            return fields;
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
            var portalId = contentType.PortalId;
            var contentFieldId = viewModel.ContentFieldId;

            var localizedNames = new List<ContentTypeLocalization>();
            var defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            var localizedLabels = new List<ContentTypeLocalization>();
            var defaultLabel = ParseLocalizations(viewModel.LocalizedLabels, localizedLabels, portalId);

            var localizedDescriptions = new List<ContentTypeLocalization>();
            var defaultDescription = ParseLocalizations(viewModel.LocalizedDescriptions, localizedDescriptions, portalId);

            string errorMessage;

            if (!IsValidContentField(portalId, viewModel, defaultName, localizedNames, out errorMessage))
            {
                return GetErrorResponse(errorMessage);
            }

            if (contentFieldId == Null.NullInteger)
            {
                contentFieldId = AddFieldDefinition(portalId, viewModel, defaultName, defaultLabel, defaultDescription);
            }
            else
            {
                UpdateFieldDefinition(viewModel, defaultName, defaultLabel, defaultDescription);

            }

            SaveFieldLocalizations(portalId, contentFieldId, localizedNames, localizedLabels, localizedDescriptions);

            return Request.CreateResponse(HttpStatusCode.OK, new {contentFieldId});
        }

        private bool IsValidContentField(int portalId, ContentFieldViewModel viewModel, string defaultName, IEnumerable<ContentTypeLocalization> localizedNames, 
            out string errorMessage)
        {
            var contentFieldId = viewModel.ContentFieldId;

            if (!IsUniqueName(defaultName, FieldDefinitionManager.Instance.GetFieldDefinitions(viewModel.ContentTypeId).
                Where(x => x.FieldDefinitionId != contentFieldId).Select(x => x.Name)))
            {
                errorMessage = LocalizeString("ContentFieldExists");
                return false;
            }

            foreach (var localizedName in localizedNames)
            {
                var otherFields = FieldDefinitionManager.Instance.GetFieldDefinitions(viewModel.ContentTypeId).Where(x => x.FieldDefinitionId != contentFieldId);
                var localizations = ContentTypeLocalizationManager.Instance.GetLocalizations(portalId);
                var otherFieldsNameTranslations = new List<string>();
                foreach (var field in otherFields)
                {
                    var localizationKey = String.Format(FieldDefinitionManager.NameKey, field.FieldDefinitionId);
                    var fieldTranslation = localizations.SingleOrDefault(x => x.CultureCode == localizedName.CultureCode && x.Key == localizationKey);
                    if (fieldTranslation != null)
                    {
                        otherFieldsNameTranslations.Add(fieldTranslation.Value);
                    }
                }

                if (!IsUniqueName(localizedName.Value, otherFieldsNameTranslations))
                {
                    var language = GetLanguage(portalId, localizedName.CultureCode);
                    errorMessage = string.Format(LocalizeString("ContentFieldTranslationExists"), language ?? localizedName.CultureCode);
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }

        private static bool IsUniqueName(string fieldName, IEnumerable<string> otherFieldsNames)
        {
            return otherFieldsNames.All(x => ! x.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
        }

        private static string GetLanguage(int portalId, string cultureCode)
        {
            var language = LocaleController.Instance.GetLocales(portalId).Values.FirstOrDefault(x => x.Code == cultureCode);
            return language != null ? language.NativeName : null;
        }

        private HttpResponseMessage GetErrorResponse(string errorMessage)
        {
            return Request.CreateErrorResponse((HttpStatusCode) HttpStatusCodeAdditions.UnprocessableEntity, errorMessage);
        }

        private static int AddFieldDefinition(int portalId, ContentFieldViewModel viewModel, string defaultName,
            string defaultLabel, string defaultDescription)
        {
            return FieldDefinitionManager.Instance.AddFieldDefinition(
                new FieldDefinition
                {
                    ContentTypeId = viewModel.ContentTypeId,
                    PortalId = portalId,
                    Name = defaultName,
                    Label = defaultLabel,
                    Description = defaultDescription,
                    FieldTypeId = viewModel.FieldTypeId,
                    IsReferenceType = viewModel.IsReferenceType,
                    IsList = viewModel.IsList
                });
        }

        private static void UpdateFieldDefinition(ContentFieldViewModel viewModel, string defaultName, string defaultLabel, string defaultDescription)
        {
            var savedField = FieldDefinitionManager.Instance.GetFieldDefinition(viewModel.ContentFieldId, viewModel.ContentTypeId);

            savedField.Name = defaultName;
            savedField.Description = defaultDescription;
            savedField.Label = defaultLabel;
            savedField.FieldTypeId = viewModel.FieldTypeId;
            savedField.IsReferenceType = viewModel.IsReferenceType;
            savedField.IsList = viewModel.IsList;

            FieldDefinitionManager.Instance.UpdateFieldDefinition(savedField);
        }

        private void SaveFieldLocalizations(int portalId, int contentFieldId, List<ContentTypeLocalization> localizedNames,
            List<ContentTypeLocalization> localizedLabels, List<ContentTypeLocalization> localizedDescriptions)
        {
            SaveContentLocalizations(localizedNames, FieldDefinitionManager.NameKey, contentFieldId, portalId);
            SaveContentLocalizations(localizedLabels, FieldDefinitionManager.LabelKey, contentFieldId, portalId);
            SaveContentLocalizations(localizedDescriptions, FieldDefinitionManager.DescriptionKey, contentFieldId, portalId);
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
            var contentTypeId = viewModel.ContentTypeId;
            var portalId = viewModel.IsSystem ? -1 : PortalSettings.PortalId;

            var localizedNames = new List<ContentTypeLocalization>();
            string defaultName = ParseLocalizations(viewModel.LocalizedNames, localizedNames, portalId);

            var localizedDescriptions = new List<ContentTypeLocalization>();
            string defaultDescription = ParseLocalizations(viewModel.LocalizedDescriptions, localizedDescriptions, portalId);

            return SaveEntity(contentTypeId,

                /*CheckEntity*/ () => DynamicContentTypeManager.Instance.GetContentTypes(portalId, true)
                                                .SingleOrDefault((t => t.Name.Equals(defaultName, StringComparison.InvariantCultureIgnoreCase))),

                /*ErrorMsg*/    LocalizeString("ContentTypeExists"),

                /*CreateEntity*/() => new DynamicContentType
                                            {
                                                Name = defaultName,
                                                Description = defaultDescription,
                                                PortalId = portalId
                                            },

                /*AddEntity*/   contentType => DynamicContentTypeManager.Instance.AddContentType(contentType),

                /*GetEntity*/   () => DynamicContentTypeManager.Instance.GetContentType(contentTypeId, portalId, true),

                /*UpdateEntity*/contentType =>
                                            {
                                                contentType.Name = defaultName;
                                                contentType.Description = defaultDescription;
                                                DynamicContentTypeManager.Instance.UpdateContentType(contentType);
                                            },

                /*SaveLocal*/   id =>
                                            {
                                                SaveContentLocalizations(localizedNames, DynamicContentTypeManager.NameKey, id, portalId);
                                                SaveContentLocalizations(localizedDescriptions, DynamicContentTypeManager.DescriptionKey, id, portalId);
                                            });

        }
    }
}
