#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
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
