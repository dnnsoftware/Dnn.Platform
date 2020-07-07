// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Prompt.Components.Models
{
    using DotNetNuke.Common;

    public class PortalModelBase
    {
        public PortalModelBase()
        {
        }

        public PortalModelBase(DotNetNuke.Entities.Portals.PortalInfo portal)
        {
            this.PortalId = portal.PortalID;
            this.PortalName = portal.PortalName;
            this.PageCount = portal.Pages;
            this.UserCount = portal.Users;
            this.Language = portal.DefaultLanguage;

            switch (portal.UserRegistration)
            {
                case (int)Globals.PortalRegistrationType.NoRegistration:
                    this.RegistrationMode = "None";
                    break;
                case (int)Globals.PortalRegistrationType.PrivateRegistration:
                    this.RegistrationMode = "Private";
                    break;
                case (int)Globals.PortalRegistrationType.PublicRegistration:
                    this.RegistrationMode = "Public";
                    break;
                case (int)Globals.PortalRegistrationType.VerifiedRegistration:
                    this.RegistrationMode = "Verified";
                    break;
                default:
                    this.RegistrationMode = "Unknown";
                    break;
            }
        }

        public int PortalId { get; set; }
        public string PortalName { get; set; }
        public string RegistrationMode { get; set; }
        public string DefaultPortalAlias { get; set; }
        public int PageCount { get; set; }
        public int UserCount { get; set; }
        public string Language { get; set; }
    }
}
