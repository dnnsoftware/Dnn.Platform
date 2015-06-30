// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class JsonContentTypeInvalidException : InvalidOperationException
    {
        public JsonContentTypeInvalidException(int contentTypeId)
            : base(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("JsonContentTypeInvalid", String.Format("The contentTypeId specified in the json document - {0} - does not exist.", contentTypeId)))
        {
            
        }
    }
}
