// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#region Usings

using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;


#endregion

namespace DotNetNuke.Web.Validators
{
    public class ValidationResult
    {
        private readonly IEnumerable<ValidationError> _Errors;

        #region "Constructors"

        public ValidationResult()
        {
            this._Errors = Enumerable.Empty<ValidationError>();
        }

        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Requires.NotNull("errors", errors);
            this._Errors = errors;
        }

        #endregion

        #region "Public Properties"

        public IEnumerable<ValidationError> Errors
        {
            get
            {
                return this._Errors;
            }
        }

        public bool IsValid
        {
            get
            {
                return this._Errors.Count() == 0;
            }
        }

        public static ValidationResult Successful
        {
            get
            {
                return new ValidationResult();
            }
        }

        #endregion

        #region "Public Methods"

        public ValidationResult CombineWith(ValidationResult other)
        {
            Requires.NotNull("other", other);

            // Just concatenate the errors collection
            return new ValidationResult(this._Errors.Concat(other.Errors));
        }

        #endregion
    }
}
