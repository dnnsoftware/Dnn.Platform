// Copyright (c) DNN Software. All rights reserved.
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

        private DynamicContentField FromJson(FieldDefinition definition, JObject jObject)
        {
            DynamicContentField field;

            if (definition.IsReferenceType)
            {
                field = new DynamicContentField(definition);
                var part = new DynamicContentPart(PortalId, definition.ContentType);
                field.Value = part;
                if(jObject["value"] == null)
                {
                    field.FromJson(jObject);
                }
                else
                {
                    field.FromJson(jObject["value"] as JObject);
                }
            }
            else
            {
                var stringValue = jObject["value"].Value<string>() ?? String.Empty;
                switch (definition.DataType.UnderlyingDataType)
                {

                    case UnderlyingDataType.Boolean:
                        Boolean boolResult;
                        field = Boolean.TryParse(stringValue, out boolResult)
                                ? new DynamicContentField(definition) { Value = boolResult }
                                : new DynamicContentField(definition) { Value = false };
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
                        Double dblResult;
                        field = Double.TryParse(stringValue, out dblResult)
                                ? new DynamicContentField(definition) { Value = dblResult }
                                : new DynamicContentField(definition) { Value = 0.0 };
                        break;
                    case UnderlyingDataType.Guid:
                        Guid guidResult;
                        field = Guid.TryParse(stringValue, out guidResult)
                                ? new DynamicContentField(definition) { Value = guidResult }
                                : new DynamicContentField(definition) { Value = Guid.NewGuid() };
                        break;
                    case UnderlyingDataType.Integer:
                        Int32 intResult;
                        field = Int32.TryParse(stringValue, out intResult)
                                ? new DynamicContentField(definition) { Value = intResult }
                                : new DynamicContentField(definition) { Value = 0 };
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

        public void FromJson(JObject jContent)
        {
            var jFields = jContent["field"] as JArray;
            Fields = new Dictionary<string, DynamicContentField>();

            if (jFields == null)
            {
                return;
            }

            foreach (var fieldDefinition in ContentType.FieldDefinitions)
            {
                var jField = FindJFieldByName(jFields, fieldDefinition.Name);

                DynamicContentField field;
                
                if (jField != null)
                {
                    if (fieldDefinition.IsList)
                    {
                        field = new DynamicContentField(fieldDefinition);
                        var jArray = jField["value"] as JArray;
                        if (jArray != null)
                        {
                            field.Value = jArray.Select(jObject => FromJson(fieldDefinition, jObject as JObject)).ToList();
                        }
                    }
                    else
                    {
                        field = FromJson(fieldDefinition, jField as JObject);
                    }
                }
                else
                {
                    field = FromJson(fieldDefinition, new JObject(
                                            new JProperty("name", fieldDefinition.Name),
                                            new JProperty("value", null)
                                        ));
                }
                
                Fields.Add(fieldDefinition.Name, field);
            }
        }

        private static JToken FindJFieldByName(JArray jFields, string name)
        {
            return jFields.SingleOrDefault(
                j => j["name"].Value<string>().Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public JObject ToJson()
        {
            var jObject = new JObject(
                                new JProperty("field",
                                    new JArray(
                                        from f in Fields.Values
                                        select new JObject(
                                            new JProperty("name", f.Definition.Name),
                                            new JProperty("value", ToJson(f))
                                        )
                                    )
                                )
                            );

            return jObject;
        }

        private object ToJson(DynamicContentField field)
        {
            object value = null;

            if (field.Definition.IsList)
            {
                if (field.Definition.IsReferenceType)
                {
                    var list = field.Value as List<DynamicContentPart>;
                    if (list != null)
                    {
                        var array = new JArray();
                        foreach (var item in list)
                        {
                            array.Add(item.ToJson());
                        }
                        value = array;
                    }
                }
                else
                {
                    var list = field.Value as List<DynamicContentField>;
                    if (list != null)
                    {
                        var array = new JArray();
                        foreach (var item in list)
                        {
                            array.Add(new JObject(
                                            new JProperty("name", item.Definition.Name),
                                            new JProperty("value", ToJson(item))
                                        ));
                        }
                        value = array;
                    }
                }
            }
            else
            {
                value = field.Definition.IsReferenceType ? field.ToJson() : field.Value;
            }
            
            return value;
        }
    }
}
