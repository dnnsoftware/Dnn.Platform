// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent
{
    public class ValidatorTypeController : ControllerBase<ValidatorType, IValidatorTypeController, ValidatorTypeController>, IValidatorTypeController
    {
        internal const string ValidatorTypeCacheKey = "ContentTypes_ValidationTypes";

        protected override Func<IValidatorTypeController> GetFactory()
        {
            return () => new ValidatorTypeController();
        }

        public ValidatorTypeController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ValidatorTypeController(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new validator type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="validatorType">The validator type to add.</param>
        /// <returns>validator type id.</returns>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentException">validatorType.Name is empty.</exception>
        public int AddValidatorType(ValidatorType validatorType)
        {
            //Argument Contract
            Validate(validatorType);

            Add(validatorType);

            return validatorType.ValidatorTypeId;
        }

        /// <summary>
        /// Deletes the validator type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="validatorType">The validator type to delete.</param>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">validator type id is less than 0.</exception>
        /// <exception cref="System.InvalidOperationException">validatorType is in use.</exception>
        public void DeleteValidatorType(ValidatorType validatorType)
        {
            Delete(validatorType);
        }

        /// <summary>
        /// Gets the validator types.
        /// </summary>
        /// <returns>validator type collection.</returns>
        public IQueryable<ValidatorType> GetValidatorTypes()
        {
            return Get().AsQueryable();
        }

        /// <summary>
        /// Updates the validator type.
        /// </summary>
        /// <param name="validatorType">The validator type.</param>
        /// <exception cref="System.ArgumentNullException">validator type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">validator type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">validatorType.Name is empty.</exception>
        /// <exception cref="System.InvalidOperationException">validatorType is in use.</exception>
        public void UpdateValidatorType(ValidatorType validatorType)
        {
            //Argument Contract
            Validate(validatorType);

            Update(validatorType);
        }

        private void Validate(ValidatorType validatorType)
        {
            Requires.PropertyNotNullOrEmpty(validatorType, "Name");
            Requires.PropertyNotNullOrEmpty(validatorType, "ValidatorClassName");
            if (String.IsNullOrEmpty(validatorType.ErrorKey) && String.IsNullOrEmpty(validatorType.ErrorMessage))
            {
                throw new InvalidValidationTypeException(validatorType);
            }
        }
    }
}
