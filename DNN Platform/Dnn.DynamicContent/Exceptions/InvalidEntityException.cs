// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Dnn.DynamicContent.Exceptions
{
    public class InvalidEntityException: Exception
    {
        public InvalidEntityException(Type entityType)
            : base(string.Format(DotNetNuke.Services.Localization.Localization.GetString("EntityIsNotValid"), entityType.FullName))
        {}

        public InvalidEntityException(string message)
             : base(message)
        {}
    }
}
