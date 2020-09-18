// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

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
            this.InProgress = false;
            this.CurrentOperationText = "";
            return this;
        }
    }
}
