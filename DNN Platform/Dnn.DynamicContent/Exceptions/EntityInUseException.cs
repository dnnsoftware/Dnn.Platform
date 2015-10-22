// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DnnLocalization = DotNetNuke.Services.Localization.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    public class EntityInUseException : InvalidOperationException
    {
        public EntityInUseException(string entityName, int entityId)
            : base(String.Format(
                DnnLocalization.GetString("DynamicContentEntityInUse",
                DnnLocalization.ExceptionsResourceFile), entityName, entityId)) { }
    }
}
