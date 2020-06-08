// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Services.Localization;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Seo.Components
{
    [JsonObject]
    public class UrlRewritingResult
    {
        [JsonProperty("rewritingResult")]
        public string RewritingResult { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("identifiedPage")]
        public string IdentifiedPage { get; set; }

        [JsonProperty("redirectionReason")]
        public string RedirectionReason { get; set; }

        [JsonProperty("redirectionResult")]
        public string RedirectionResult { get; set; }

        [JsonProperty("operationMessages")]
        public string OperationMessages { get; set; }

        [JsonIgnore]
        public int Status { get; set; }

        public UrlRewritingResult()
        {
            var noneText = Localization.GetString("None", Localization.GlobalResourceFile);
            this.RewritingResult = noneText;
            this.Culture = noneText;
            this.IdentifiedPage = noneText;
            this.RedirectionResult = noneText;
            this.OperationMessages = noneText;
        }
    }
}
