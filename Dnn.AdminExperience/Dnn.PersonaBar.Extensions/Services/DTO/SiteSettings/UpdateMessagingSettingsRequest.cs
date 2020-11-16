// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    using Newtonsoft.Json;

    public class UpdateMessagingSettingsRequest
    {
        public int? PortalId { get; set; }

        public string CultureCode { get; set; }

        public bool DisablePrivateMessage { get; set; }

        public double ThrottlingInterval { get; set; }

        public int RecipientLimit { get; set; }

        public bool AllowAttachments { get; set; }

        public bool ProfanityFilters { get; set; }

        public bool IncludeAttachments { get; set; }

        public bool SendEmail { get; set; }
    }
}
