// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class JsonMissingContentException : InvalidOperationException
    {
        public JsonMissingContentException()
            : base(DotNetNuke.Services.Localization.Localization.GetExceptionMessage("JsonMissingContent", "There is no content node in the json document."))
        {

        }
    }
}
