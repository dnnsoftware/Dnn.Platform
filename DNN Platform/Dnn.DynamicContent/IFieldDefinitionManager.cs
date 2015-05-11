// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

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
        void DeleteFieldDefinition(FieldDefinition field);

        /// <summary>
        /// Gets the field definitions.
        /// </summary>
        /// <param name="contentTypeId">The Id of the parent Content Type</param>
        /// <returns>field definition collection.</returns>
        IQueryable<FieldDefinition> GetFieldDefinitions(int contentTypeId);

        /// <summary>
        /// Updates the field definition.
        /// </summary>
        /// <param name="field">The field definition.</param>
        /// <exception cref="System.ArgumentNullException">field definition is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">field definition id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">field.Name is empty.</exception>
        void UpdateFieldDefinition(FieldDefinition field);
    }
}
