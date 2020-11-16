// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Seo.Components
{
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    [JsonObject]
    public class UrlRewritingResult
    {
        public UrlRewritingResult()
        {
            var noneText = Localization.GetString("None", Localization.GlobalResourceFile);
            this.RewritingResult = noneText;
            this.Culture = noneText;
            this.IdentifiedPage = noneText;
            this.RedirectionResult = noneText;
            this.OperationMessages = noneText;
        }

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
    }
}
