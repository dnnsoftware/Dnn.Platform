﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
            _Errors = Enumerable.Empty<ValidationError>();
        }

        public ValidationResult(IEnumerable<ValidationError> errors)
        {
            Requires.NotNull("errors", errors);
            _Errors = errors;
        }

        #endregion

        #region "Public Properties"

        public IEnumerable<ValidationError> Errors
        {
            get
            {
                return _Errors;
            }
        }

        public bool IsValid
        {
            get
            {
                return (_Errors.Count() == 0);
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

            //Just concatenate the errors collection
            return new ValidationResult(_Errors.Concat(other.Errors));
        }

        #endregion
    }
}
