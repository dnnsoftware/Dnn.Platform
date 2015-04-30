#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System.Linq;

namespace Dnn.DynamicContent
{
    public interface IValidatorTypeController
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
