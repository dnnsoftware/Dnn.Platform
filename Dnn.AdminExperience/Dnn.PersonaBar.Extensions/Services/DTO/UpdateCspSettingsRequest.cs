// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Services.Dto
{
    using DotNetNuke.Entities.Portals;

    public class UpdateCspSettingsRequest
    {
        public PortalSettings.CspMode CspHeaderMode { get; set; }

        public string CspHeader { get; set; }

        public string CspReportingHeader { get; set; }
    }
}
