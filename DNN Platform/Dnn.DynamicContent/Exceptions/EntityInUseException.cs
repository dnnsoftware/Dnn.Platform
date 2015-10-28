// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using DnnLocalization = DotNetNuke.Services.Localization.Localization;

namespace Dnn.DynamicContent.Exceptions
{
    /// <summary>
    /// Base Exception used when an entity is in use
    /// </summary>
    public class EntityInUseException : InvalidOperationException
    {
        public EntityInUseException(string entityName, int entityId)
            : base(String.Format(
                DnnLocalization.GetString("DynamicContentEntityInUse",
                DnnLocalization.ExceptionsResourceFile), entityName, entityId)) { }
    }
}
