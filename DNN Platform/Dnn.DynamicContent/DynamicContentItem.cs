// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Common;
using Newtonsoft.Json.Linq;

// ReSharper disable UseStringInterpolation

namespace Dnn.DynamicContent
{
    public class DynamicContentItem
    {
        public DynamicContentItem(int portalId)
        {
            Requires.NotNegative("portalId", portalId);

            PortalId = portalId;

            Initialize(null);
        }

        public DynamicContentItem(int portalId, DynamicContentType contentType)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNull("contentType", contentType);

            PortalId = portalId;

            Initialize(contentType);
        }

        private void Initialize(DynamicContentType contentType)
        {
            ModuleId = -1;
            ContentItemId = -1;

            if (contentType != null)
            {
                ContentType = contentType;

                Fields = new Dictionary<string, DynamicContentField>();

                foreach (var fieldDefinition in contentType.FieldDefinitions)
                {
                    var field = new DynamicContentField(fieldDefinition);

                    Fields.Add(fieldDefinition.Name, field);
                }
            }
        }

        public int ContentItemId { get; set; }

        public DynamicContentType ContentType { get; private set; }

        public IDictionary<string, DynamicContentField> Fields { get; private set; }

        public int ModuleId { get; set; }

        public int TabId { get; set; }

        public int PortalId { get; set; }

        public void FromJson(string json)
        {
            Requires.NotNullOrEmpty("json", json);

            var jObject = JObject.Parse(json);
            var contentTypeId = jObject["contentTypeId"].Value<int>();

            ContentType = DynamicContentTypeManager.Instance.GetContentTypes(PortalId, true)
                                    .SingleOrDefault(t => t.ContentTypeId == contentTypeId);

            if (ContentType == null)
            {
                throw new JsonContentTypeInvalidException(contentTypeId);
            }

            var jContent = jObject["content"] as JObject;
            if (jContent == null)
            {
                throw new JsonMissingContentException();
            }

            var jFields = jContent["field"] as JArray;

            Fields = new Dictionary<string, DynamicContentField>();
            if (jFields != null)
            {
                foreach (var jField in jFields)
                {
                    var fieldName = jField["name"].Value<string>();
                    var definition = ContentType.FieldDefinitions.SingleOrDefault(d => d.Name.ToLowerInvariant() == fieldName.ToLowerInvariant());

                    if (definition == null)
                    {
                        throw new JsonInvalidFieldException(fieldName);
                    }

                    DynamicContentField field;
                    var stringValue = jField["value"].Value<string>();
                    switch (definition.DataType.UnderlyingDataType)
                    {

                        case UnderlyingDataType.Boolean:
                            field = new DynamicContentField(definition) {Value = jField["value"].Value<bool>() };
                            break;
                        case UnderlyingDataType.Bytes:
                            field = new DynamicContentField(definition) { Value = Convert.FromBase64String(stringValue) };
                            break;
                        case UnderlyingDataType.DateTime:
                            DateTime dateTimeResult;
                            field = DateTime.TryParse(stringValue, out dateTimeResult)
                                    ? new DynamicContentField(definition) { Value = dateTimeResult }
                                    : new DynamicContentField(definition) { Value = stringValue };
                            break;
                        case UnderlyingDataType.Float:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<float>() };
                            break;
                        case UnderlyingDataType.Guid:
                            Guid guidResult;
                            field = Guid.TryParse(stringValue, out guidResult) 
                                    ? new DynamicContentField(definition) { Value = guidResult } 
                                    : new DynamicContentField(definition) { Value = stringValue };
                            break;
                        case UnderlyingDataType.Integer:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<int>() };
                            break;
                        case UnderlyingDataType.TimeSpan:
                            TimeSpan timeSpanResult;
                            field = TimeSpan.TryParse(stringValue, out timeSpanResult)
                                    ? new DynamicContentField(definition) { Value = timeSpanResult }
                                    : new DynamicContentField(definition) { Value = stringValue };
                            break;
                        case UnderlyingDataType.Uri:
                            Uri uriResult = null;
                            field = Uri.TryCreate(stringValue, UriKind.Absolute, out uriResult) 
                                    ? new DynamicContentField(definition) {Value = uriResult} 
                                    : new DynamicContentField(definition) { Value = stringValue};
                            break;
                        default:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<string>() };
                            break;
                    }

                    Fields.Add(definition.Name, field);
                }
            }
        }

        public string ToJson()
        {
            var jObject = new JObject(
                                new JProperty("contentTypeId", ContentType.ContentTypeId),
                                new JProperty("content",
                                    new JObject(
                                        new JProperty("field",
                                              new JArray(
                                                  from f in Fields.Values
                                                  select new JObject(
                                                    new JProperty("name", f.Definition.Name),
                                                    new JProperty("value", f.Value)
                                                    )
                                                )
                                            )
                                        )
                                    )
                            );


            return jObject.ToString();
        }
    }
}
