// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Dnn.DynamicContent
{
    /// <summary>
    /// Class for validating FieldDefinition entities
    /// </summary>
    internal interface IFieldDefinitionChecker
    {
        /// <summary>
        /// Checks that a FieldDefinition entity is valid or not.
        /// </summary>
        /// <param name="fieldDefinition">Entity to check.</param>
        /// <param name="errorMessage">Error message that indicates the reason of the non-validity of the entity.</param>
        /// <returns>True if the FieldDefinition is valid, false if it is not.</returns>
        bool IsValid(FieldDefinition fieldDefinition, out string errorMessage);
    }
}
