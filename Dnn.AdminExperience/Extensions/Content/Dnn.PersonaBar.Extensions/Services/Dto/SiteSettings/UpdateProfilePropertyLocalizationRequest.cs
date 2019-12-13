#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateProfilePropertyLocalizationRequest
    {
        public int? PortalId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyCategory { get; set; }

        public string Language { get; set; }

        public string PropertyNameString { get; set; }

        public string PropertyHelpString { get; set; }

        public string PropertyRequiredString { get; set; }

        public string PropertyValidationString { get; set; }

        public string CategoryNameString { get; set; }
    }
}
