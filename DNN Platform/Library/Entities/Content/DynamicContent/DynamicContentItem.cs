#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Entities.Content.DynamicContent.Exceptions;
using Newtonsoft.Json.Linq;
// ReSharper disable UseStringInterpolation

namespace DotNetNuke.Entities.Content.DynamicContent
{
    public class DynamicContentItem
    {
        public DynamicContentItem(int portalId)
        {
            Requires.NotNegative("portalId", portalId);
            PortalId = portalId;
            ModuleId = -1;
        }

        public DynamicContentItem(DynamicContentType contentType)
        {
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative(contentType, "PortalId");

            PortalId = contentType.PortalId;
            ModuleId = -1;

            Initialize(contentType);
        }

        private void Initialize(DynamicContentType contentType)
        {
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

        public int ModuleId { get; set; }

        public int PortalId { get; set; }

        public void FromJson(string json)
        {
            Requires.NotNullOrEmpty("json", json);

            var jObject = JObject.Parse(json);
            var contentTypeId = jObject["contentTypeId"].Value<int>();

            ContentType = DynamicContentTypeController.Instance.GetContentTypes(PortalId)
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
