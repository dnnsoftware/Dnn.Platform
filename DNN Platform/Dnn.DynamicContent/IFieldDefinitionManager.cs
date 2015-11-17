// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent.Exceptions;

namespace Dnn.DynamicContent
{
    public interface IFieldDefinitionManager
    {
        /// <summary>
        /// Adds a new field definition for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="field">The field definition to add.</param>
        /// <returns>field definition id.</returns>
        /// <exception cref="System.ArgumentNullException">field definition is null.</exception>
        /// <exception cref="System.ArgumentException">field.Name is empty.</exception>
        int AddFieldDefinition(FieldDefinition field);

        /// <summary>
        /// Deletes the field definition for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="field">The field definitione to delete.</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        /// <exception cref="FieldDefinitionDoesNotExistException">requested data type by FieldDefinitionId and PortalId does not exist</exception>
        void DeleteFieldDefinition(FieldDefinition field);

        /// <summary>
        /// Gets a field definition.
        /// </summary>
        /// <param name="fieldDefinitionId">The ID of the field definition</param>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <returns>field definition collection.</returns>
        FieldDefinition GetFieldDefinition(int fieldDefinitionId, int contentTypeId);

        /// <summary>
        /// Gets the field definitions.
        /// </summary>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <returns>field definition collection.</returns>
        IQueryable<FieldDefinition> GetFieldDefinitions(int contentTypeId);

        /// <summary>
        /// Move a Field Definition's position in the list
        /// </summary>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <param name="sourceIndex">The index (order) of the item being moved</param>
        /// <param name="targetIndex">The target index (order) of the item being moved</param>
        void MoveFieldDefinition(int contentTypeId, int sourceIndex, int targetIndex);

        /// <summary>
        /// Updates the field definition.
        /// </summary>
        /// <param name="field">The field definition.</param>
        /// <exception cref="System.ArgumentNullException">field definition is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">field definition id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">field.Name is empty.</exception>
        /// <exception cref="FieldDefinitionDoesNotExistException">requested data type by FieldDefinitionId and PortalId does not exist</exception>
        /// <exception cref="InvalidOperationException">portal id is different than the stored portal id</exception>
        void UpdateFieldDefinition(FieldDefinition field);
    }
}
