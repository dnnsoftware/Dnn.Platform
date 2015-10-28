// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DnnLocalization = DotNetNuke.Services.Localization.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    /// <summary>
    /// Exception thrown when a ContentType is in use and it is tried to be deleted
    /// </summary>
    public class ContentTypeInUseException : EntityInUseException
    {
        public ContentTypeInUseException(DynamicContentType dataType)
            : base(DnnLocalization.GetString("ContentType",
                DnnLocalization.SharedResourceFile), dataType.ContentTypeId)            
        {
        }
    }
}
