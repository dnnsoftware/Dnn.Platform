// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateBasicSearchSettingsRequest
    {
        public int MinWordLength { get; set; }

        public int MaxWordLength { get; set; }

        public bool AllowLeadingWildcard { get; set; }

        public string SearchCustomAnalyzer { get; set; }

        public int TitleBoost { get; set; }

        public int TagBoost { get; set; }

        public int ContentBoost { get; set; }

        public int DescriptionBoost { get; set; }

        public int AuthorBoost { get; set; }
    }
}
