// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
