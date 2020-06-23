﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Validators
{
    public class ValidationError
    {
        public string ErrorMessage { get; set; }

        public string PropertyName { get; set; }

        public object Validator { get; set; }
    }
}
