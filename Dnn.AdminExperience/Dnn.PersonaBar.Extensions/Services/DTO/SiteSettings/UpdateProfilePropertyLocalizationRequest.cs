// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

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
