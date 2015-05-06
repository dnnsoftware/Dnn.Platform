// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Dnn.DynamicContent
{
    public class DynamicContentField
    {
        public DynamicContentField(FieldDefinition definition)
        {
            Definition = definition;
        }

        public FieldDefinition Definition { get; set; }

        public object Value { get; set; }
    }
}
