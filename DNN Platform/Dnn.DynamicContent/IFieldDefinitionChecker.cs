﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Dnn.DynamicContent
{
    internal interface IFieldDefinitionChecker
    {
        bool IsValid(FieldDefinition fieldDefinition, out string errorMessage);
    }
}
