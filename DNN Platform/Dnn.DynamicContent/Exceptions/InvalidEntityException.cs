// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an entity definition is not valid and therefore cannot be created or updated
    /// </summary>
    public class InvalidEntityException : InvalidOperationException
    {
        public InvalidEntityException(Type entityType)
            : base(string.Format(DotNetNuke.Services.Localization.Localization.GetString("EntityIsNotValid"), entityType.FullName))
        {}

        public InvalidEntityException(string message)
             : base(message)
        {}
    }
}
