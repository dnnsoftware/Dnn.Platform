// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Common;
using Newtonsoft.Json.Linq;

namespace Dnn.DynamicContent
{
    public class DynamicContentItem
    {
        public DynamicContentItem(int portalId, DynamicContentType contentType)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNull("contentType", contentType);

            PortalId = portalId;

            ModuleId = -1;
            TabId = -1;
            ContentItemId = -1;

            ContentType = contentType;
            Content = new DynamicContentPart(PortalId, contentType);
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

            var jContent = JObject.Parse(json);
            Content = new DynamicContentPart(PortalId, ContentType);
            Content.FromJson(jContent);
        }

        public string ToJson()
        {
            return Content.ToJson().ToString();
        }
    }
}
