// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Web.Validators
{
    public class ValidationError
    {
        #region "Public Properties"

        public string ErrorMessage { get; set; }

        public string PropertyName { get; set; }

        public object Validator { get; set; }

        #endregion
    }
}
