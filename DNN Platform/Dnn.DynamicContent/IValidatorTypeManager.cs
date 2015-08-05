// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;

namespace Dnn.DynamicContent
{
    public interface IValidatorTypeManager
    {
        /// <summary>
        /// Adds a new validator type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="validatorType">The validator type to add.</param>
        /// <returns>validator type id.</returns>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentException">validatorType.Name is empty.</exception>
        int AddValidatorType(ValidatorType validatorType);

        /// <summary>
        /// Deletes the validator type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="validatorType">The validator type to delete.</param>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">validator type id is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">validatorType is in use.</exception>
        void DeleteValidatorType(ValidatorType validatorType);

        /// <summary>
        /// Gets the validator types.
        /// </summary>
        /// <returns>validator type collection.</returns>
        IQueryable<ValidatorType> GetValidatorTypes();

        /// <summary>
        /// Updates the validator type.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">validator type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">validatorType.Name is empty.</exception>
        /// <exception cref="System.InvalidOperationException">validatorType is in use.</exception>
        void UpdateValidatorType(ValidatorType validatorType);
    }
}
