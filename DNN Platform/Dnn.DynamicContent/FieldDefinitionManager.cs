// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Data;
using System.Linq;
using Dnn.DynamicContent.Exceptions;
using Dnn.DynamicContent.Localization;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent
{
    public class FieldDefinitionManager : ControllerBase<FieldDefinition, IFieldDefinitionManager, FieldDefinitionManager>, IFieldDefinitionManager
    {
        internal const string FieldDefinitionCacheKey = "ContentTypes_FieldDefinitions";
        internal const string FieldDefinitionScope = "ContentTypeId";
        public const string DescriptionKey = "ContentField_{0}_Description";
        public const string LabelKey = "ContentField_{0}_Label";
        public const string NameKey = "ContentField_{0}_Name";

        private IFieldDefinitionChecker _fieldDefinitionChecker;

        protected override Func<IFieldDefinitionManager> GetFactory()
        {
            return () => new FieldDefinitionManager();
        }

        public FieldDefinitionManager() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public FieldDefinitionManager(IDataContext dataContext) : base(dataContext)
        {
            _fieldDefinitionChecker = FieldDefinitionChecker.Instance;
        }

        /// <summary>
        /// Adds a new field definition for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="field">The field definition to add.</param>
        /// <returns>field definition id.</returns>
        /// <exception cref="System.ArgumentNullException">field definition is null.</exception>
        /// <exception cref="System.ArgumentException">field.Name is empty.</exception>
        /// <exception cref="Dnn.DynamicContent.Exceptions.InvalidEntityException">The field object is not valid (Inexisting content type, dead loop definition, 
        /// duplicate field names in the same content type, etc.).</exception>
        public int AddFieldDefinition(FieldDefinition field)
        {
            //Argument Contract
            Requires.PropertyNotNegative(field, "FieldTypeId");
            Requires.PropertyNotNegative(field, "ContentTypeId");
            Requires.PropertyNotNullOrEmpty(field, "Name");
            Requires.PropertyNotNullOrEmpty(field, "Label");

            field.Order = Get(field.ContentTypeId).Count();

            string errorMessage;
            if (!_fieldDefinitionChecker.IsValid(field, out errorMessage))
            {
                throw new InvalidEntityException(errorMessage);
            }

            Add(field);

            ClearContentTypeCache(field);

            //Add any new ValidationRules
            foreach (var validationRule in field.ValidationRules)
            {
                validationRule.FieldDefinitionId = field.FieldDefinitionId;
                ValidationRuleManager.Instance.AddValidationRule(validationRule);
            }

            return field.FieldDefinitionId;
        }

        private void ClearContentTypeCache(FieldDefinition field)
        {
            var contentType = DynamicContentTypeManager.Instance.GetContentType(field.ContentTypeId, field.PortalId, true);

            if (contentType != null)
            {
                contentType.ClearFieldDefinitions();
            }
        }

        /// <summary>
        /// Deletes the field definition for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="field">The field definitione to delete.</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        public void DeleteFieldDefinition(FieldDefinition field)
        {
            Delete(field);

            //Update field order of remaining fields
            var sql = @"UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                SET[Order] = [Order] - 1
                                    WHERE[Order] > @1
                                    AND ContentTypeID = @0";

            using (DataContext)
            {
                DataContext.Execute(CommandType.Text, sql, field.ContentTypeId, field.Order);
            }

            ClearContentTypeCache(field);

            //Delete any ValidationRules
            foreach (var validationRule in field.ValidationRules)
            {
                ValidationRuleManager.Instance.DeleteValidationRule(validationRule);
            }

            //Delete Localizations
            ContentTypeLocalizationManager.Instance.DeleteLocalizations(field.PortalId, String.Format(NameKey, field.FieldDefinitionId));
            ContentTypeLocalizationManager.Instance.DeleteLocalizations(field.PortalId, String.Format(LabelKey, field.FieldDefinitionId));
            ContentTypeLocalizationManager.Instance.DeleteLocalizations(field.PortalId, String.Format(DescriptionKey, field.FieldDefinitionId));
        }

        /// <summary>
        /// Gets a field definition.
        /// </summary>
        /// <param name="fieldDefinitionId">The ID of the field definition</param>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <returns>field definition collection.</returns>
        //TODO add Unit Tests for this method
        public FieldDefinition GetFieldDefinition(int fieldDefinitionId, int contentTypeId)
        {
            return Get(contentTypeId).SingleOrDefault((f) => f.FieldDefinitionId == fieldDefinitionId);
        }

        /// <summary>
        /// Gets the field definitions.
        /// </summary>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <returns>field definition collection.</returns>
        public IQueryable<FieldDefinition> GetFieldDefinitions(int contentTypeId)
        {
            return Get(contentTypeId).OrderBy(f => f.Order).AsQueryable();
        }

        /// <summary>
        /// Move a Field Definition's position in the list
        /// </summary>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <param name="sourceIndex">The index (order) of the item being moved</param>
        /// <param name="targetIndex">The target index (order) of the item being moved</param>
        public void MoveFieldDefintion(int contentTypeId, int sourceIndex, int targetIndex)
        {
            //First get the item to be moved
            var field = Get(contentTypeId).SingleOrDefault(f => f.Order == sourceIndex);

            if (field != null)
            {
                //Next update all the intermediate fields
                var sql = @"IF @1 > @2 -- Move other items down order
                            BEGIN
                                UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                    SET[Order] = [Order] + 1
                                        WHERE[Order] < @1 AND[Order] >= @2
                                        AND ContentTypeID = @0
                            END
                        ELSE --Move other items up order
                            BEGIN
                                UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                    SET[Order] = [Order] - 1
                                        WHERE[Order] > @1 AND[Order] <= @2
                                        AND ContentTypeId = @0
                            END";

                using (DataContext)
                {
                    DataContext.Execute(CommandType.Text, sql, contentTypeId, sourceIndex, targetIndex);
                }

                //Update item to be moved
                field.Order = targetIndex;
                UpdateFieldDefinition(field);

                ClearContentTypeCache(field);
            }
        }


        /// <summary>
        /// Updates the field definition.
        /// </summary>
        /// <param name="field">The field definition.</param>
        /// <exception cref="System.ArgumentNullException">field definition is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">field definition id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">field.Name is empty.</exception>
        /// <exception cref="Dnn.DynamicContent.Exceptions.InvalidEntityException">The field object is not valid (Inexisting content type, dead loop definition, 
        /// duplicate field names in the same content type, etc.).</exception>
        public void UpdateFieldDefinition(FieldDefinition field)
        {
            //Argument Contract
            Requires.PropertyNotNegative(field, "ContentTypeId");
            Requires.PropertyNotNegative(field, "FieldTypeId");
            Requires.PropertyNotNullOrEmpty(field, "Name");
            Requires.PropertyNotNullOrEmpty(field, "Label");

            string errorMessage;
            if (!_fieldDefinitionChecker.IsValid(field, out errorMessage))
            {
                throw new InvalidEntityException(errorMessage);
            }

            Update(field);

            ClearContentTypeCache(field);

            //Upsert any ValidationRules
            foreach (var validationRule in field.ValidationRules)
            {
                if (validationRule.ValidationRuleId == -1)
                {
                    validationRule.FieldDefinitionId = field.FieldDefinitionId;
                    ValidationRuleManager.Instance.AddValidationRule(validationRule);
                }
                else
                {
                    ValidationRuleManager.Instance.UpdateValidationRule(validationRule);
                }
            }
        }
    }
}
