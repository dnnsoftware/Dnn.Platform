using Newtonsoft.Json;

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    [JsonObject]
    public class LocalizationProgress
    {
        public string CurrentOperationText { get; set; }
        public bool InProgress { get; set; }
        public int PrimaryTotal { get; set; }
        public int PrimaryValue { get; set; }
        public int PrimaryPercent { get; set; }
        public int SecondaryTotal { get; set; }
        public int SecondaryValue { get; set; }
        public int SecondaryPercent { get; set; }
        public int TimeEstimated { get; set; }
        public string Error { get; set; }

        public LocalizationProgress Reset()
        {
            InProgress = false;
            CurrentOperationText = "";
            return this;
        }
    }
}