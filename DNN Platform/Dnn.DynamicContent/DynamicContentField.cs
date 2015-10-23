// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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

        internal void FromJson(JObject jObject)
        {
            var part = Value as DynamicContentPart ??
                       new DynamicContentPart(Definition.PortalId, Definition.ContentType);
            part.FromJson(jObject);
        }

        internal JObject ToJson()
        {
            var part = Value as DynamicContentPart ??
                       new DynamicContentPart(Definition.PortalId, Definition.ContentType);
            return part.ToJson();
        }

        public string GetStringValue()
        {
            if (Value == null)
            {
                return String.Empty;
            }
            if (!Definition.IsReferenceType)
            {
                return Value.ToString();
            }
            var part = Value as DynamicContentPart ??
                       new DynamicContentPart(Definition.PortalId, Definition.ContentType);
            return part.GetStringValue();
        }
    }
}
