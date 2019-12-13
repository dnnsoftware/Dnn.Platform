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
            RewritingResult = noneText;
            Culture = noneText;
            IdentifiedPage = noneText;
            RedirectionResult = noneText;
            OperationMessages = noneText;
        }
    }
}
