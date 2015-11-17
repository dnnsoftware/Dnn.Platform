﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Dnn.DynamicContent
{
    public class DynamicContentPart
    {
        public DynamicContentPart(int portalId, DynamicContentType contentType)
        {
            PortalId = portalId;
            ContentType = contentType;
            Fields = new Dictionary<string, DynamicContentField>();

            foreach (var fieldDefinition in contentType.FieldDefinitions)
            {
                var field = new DynamicContentField(fieldDefinition);

                Fields.Add(fieldDefinition.Name, field);
            }
        }

        public DynamicContentType ContentType { get; private set; }

        public IDictionary<string, DynamicContentField> Fields { get; private set; }

        public int PortalId { get; set; }

        private DynamicContentField FromJson(FieldDefinition definition, JToken jToken)
        {
            DynamicContentField field;

            if (definition.IsReferenceType)
            {
                field = new DynamicContentField(definition);
                var part = new DynamicContentPart(PortalId, definition.ContentType);
                field.Value = part;
                field.FromJson(jToken);
            }
            else
            {
                var stringValue = jToken.Value<string>() ?? String.Empty;
                switch (definition.DataType.UnderlyingDataType)
                {
                    case UnderlyingDataType.Boolean:
                        field = new DynamicContentField(definition) { Value = jToken.Value<bool?>() ?? false };
                        break;
                    case UnderlyingDataType.Bytes:
                        field = (String.IsNullOrEmpty(stringValue))
                                ? new DynamicContentField(definition) { Value = new byte[] { } }
                                : new DynamicContentField(definition) { Value = Convert.FromBase64String(stringValue) };
                        break;
                    case UnderlyingDataType.DateTime:
                        DateTime dateTimeResult;
                        field = DateTime.TryParse(stringValue, out dateTimeResult)
                                ? new DynamicContentField(definition) { Value = dateTimeResult }
                                : new DynamicContentField(definition) { Value = new DateTime(2000, 1, 1) };
                        break;
                    case UnderlyingDataType.Float:
                        field = new DynamicContentField(definition) { Value = jToken.Value<double?>() ?? 0 };
                        break;
                    case UnderlyingDataType.Guid:
                        Guid guidResult;
                        field = Guid.TryParse(stringValue, out guidResult)
                                ? new DynamicContentField(definition) { Value = guidResult }
                                : new DynamicContentField(definition) { Value = Guid.NewGuid() };
                        break;
                    case UnderlyingDataType.Integer:
                        field = new DynamicContentField(definition) { Value = jToken.Value<int?>() ?? 0 };
                        break;
                    case UnderlyingDataType.TimeSpan:
                        TimeSpan timeSpanResult;
                        field = TimeSpan.TryParse(stringValue, out timeSpanResult)
                                ? new DynamicContentField(definition) { Value = timeSpanResult }
                                : new DynamicContentField(definition) { Value = new TimeSpan(0, 0, 0) };
                        break;
                    case UnderlyingDataType.Uri:
                        Uri uriResult;
                        field = Uri.TryCreate(stringValue, UriKind.Absolute, out uriResult)
                                ? new DynamicContentField(definition) { Value = uriResult }
                                : new DynamicContentField(definition) { Value = null };
                        break;
                    default:
                        field = new DynamicContentField(definition) { Value = stringValue };
                        break;
                }
            }

            return field;
        }

        public void FromJson(JToken jContent)
        {
            Fields = new Dictionary<string, DynamicContentField>();

            foreach (var fieldDefinition in ContentType.FieldDefinitions)
            {
                var jField = jContent[fieldDefinition.Name];

                DynamicContentField field;
                
                if (jField != null)
                {
                    if (fieldDefinition.IsList)
                    {
                        field = new DynamicContentField(fieldDefinition);
                        var jArray = jField as JArray;
                        if (jArray != null)
                        {
                            field.Value = jArray.Select(jObject => FromJson(fieldDefinition, jObject)).ToList();
                        }
                    }
                    else
                    {
                        field = FromJson(fieldDefinition, jField);
                    }
                }
                else
                {                    
                    field = FromJson(fieldDefinition, new JValue((object)null));
                }
                
                Fields.Add(fieldDefinition.Name, field);
            }
        }

        public JObject ToJson()
        {
            return new JObject(
                            from f in Fields.Values
                            select new JProperty(f.Definition.Name, ToJson(f))
                        );
        }

        public string GetStringValue()
        {
            var values = new List<string>();
            foreach (var field in Fields)
            {               
                values.Add(field.Value.GetStringValue());
            }
            return string.Join(", ", values);
        }


        private object ToJson(DynamicContentField field)
        {
            if (field.Definition.IsList)
            {
                if (field.Definition.IsReferenceType)
                {
                    var list = field.Value as List<DynamicContentPart>;

                    return list == null ? null 
                        : new JArray(from item in list select item.ToJson());
                }

                return new JArray(field.Value);
            }

            return field.Definition.IsReferenceType ? field.ToJson() : field.Value;
        }
    }
}
