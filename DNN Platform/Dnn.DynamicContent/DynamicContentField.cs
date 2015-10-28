// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json.Linq;

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

        internal void FromJson(JToken jToken)
        {
            var part = Value as DynamicContentPart ?? new DynamicContentPart(Definition.PortalId, Definition.ContentType);
            part.FromJson(jToken);
        }

        internal JObject ToJson()
        {
            var part = Value as DynamicContentPart ?? new DynamicContentPart(Definition.PortalId, Definition.ContentType);
            return part.ToJson();
        }
    }
}
