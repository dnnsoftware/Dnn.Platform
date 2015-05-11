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

        public DynamicContentItem(DynamicContentType contentType)
        {
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative(contentType, "PortalId");

            PortalId = contentType.PortalId;

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

        public int PortalId { get; set; }

        public void FromJson(string json)
        {
            Requires.NotNullOrEmpty("json", json);

            var jObject = JObject.Parse(json);
            var contentTypeId = jObject["contentTypeId"].Value<int>();

            ContentType = DynamicContentTypeManager.Instance.GetContentTypes(PortalId)
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

                    var value = jField["value"];
                    DynamicContentField field;
                    switch (value.Type)
                    {
                        case JTokenType.Boolean:
                            field = new DynamicContentField(definition) {Value = jField["value"].Value<bool>() };
                            break;
                        case JTokenType.Bytes:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<Byte[]>() };
                            break;
                        case JTokenType.Date:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<DateTime>() };
                            break;
                        case JTokenType.Float:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<float>() };
                            break;
                        case JTokenType.Guid:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<Guid>() };
                            break;
                        case JTokenType.Integer:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<int>() };
                            break;
                        case JTokenType.TimeSpan:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<TimeSpan>() };
                            break;
                        case JTokenType.Uri:
                            field = new DynamicContentField(definition) { Value = jField["value"].Value<Uri>() };
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
