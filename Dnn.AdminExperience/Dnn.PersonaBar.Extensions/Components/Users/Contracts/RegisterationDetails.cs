// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;
using DotNetNuke.Entities.Portals;

namespace Dnn.PersonaBar.Users.Components.Contracts
{
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
        /// <summary>
        /// When set to true, whatever registration mode is set, it will always add user
        /// Please check below issue for this property
        /// https://dnntracker.atlassian.net/browse/SOCIAL-3158
        /// </summary>
        [IgnoreDataMember]
        public bool IgnoreRegistrationMode { get; set; }
    }
}
