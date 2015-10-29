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
            TabId = -1;
            ContentItemId = -1;

            if (contentType != null)
            {
                ContentType = contentType;
                Content = new DynamicContentPart(PortalId, contentType);
            }
        }

        public int ContentItemId { get; set; }

        public DynamicContentPart Content { get; set; }

        public DynamicContentType ContentType { get; private set; }

        public int ModuleId { get; set; }

        public int TabId { get; set; }

        public int PortalId { get; set; }

        public void FromJson(string json)
        {
            Requires.NotNullOrEmpty("json", json);

            var jObject = JObject.Parse(json);
            var contentTypeId = jObject["contentTypeId"].Value<int>();

            ContentType = DynamicContentTypeManager.Instance.GetContentType(contentTypeId, PortalId, true);

            if (ContentType == null)
            {
                throw new JsonContentTypeInvalidException(contentTypeId);
            }

            var jContent = jObject["content"] as JObject;
            if (jContent == null)
            {
                throw new JsonMissingContentException();
            }

            Content = new DynamicContentPart(PortalId, ContentType);
            Content.FromJson(jContent);
        }

        public string ToJson()
        {
            var jObject = new JObject(
                                new JProperty("contentTypeId", ContentType.ContentTypeId),
                                new JProperty("content", Content.ToJson()
                                )
                            );

            return jObject.ToString();
        }

    }
}
