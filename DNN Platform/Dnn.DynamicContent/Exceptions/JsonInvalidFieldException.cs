// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DotNetNuke.Services.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    public class JsonInvalidFieldException : InvalidOperationException
    {
        public JsonInvalidFieldException(string fieldName)
            : base(Localization.GetExceptionMessage("JsonInvalidField", String.Format("The content type does not specify the field - {0}.", fieldName)))
        {

        }
    }
}
