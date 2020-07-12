// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Contracts
{
    using System.Runtime.Serialization;

    using DotNetNuke.Entities.Portals;

    [DataContract]
    public class RegisterationDetails
    {
        public PortalSettings PortalSettings { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool Authorize { get; set; }
        public bool Notify { get; set; }
        public bool RandomPassword { get; set; }

        /// <summary>Gets of sets a value indicating whether, whatever registration mode is set, it will always add user.</summary>
        [IgnoreDataMember]
        public bool IgnoreRegistrationMode { get; set; }
    }
}
