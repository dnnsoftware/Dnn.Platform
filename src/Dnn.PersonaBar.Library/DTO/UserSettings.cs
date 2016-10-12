#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Library.DTO
{
    /// <summary>
    /// Persona Bar User Settings
    /// </summary>
    [DataContract]
    public class UserSettings
    {
        [DataMember(Name = "expandPersonaBar")]
        public bool ExpandPersonaBar{ get; set; }

        [DataMember(Name = "activePath")]
        public string ActivePath { get; set; }

        [DataMember(Name = "activeIdentifier")]
        public string ActiveIdentifier { get; set; }

        [DataMember(Name = "expandTasksPane")]
        public bool ExpandTasksPane { get; set; }

        [DataMember(Name = "comparativeTerm")]
        public string ComparativeTerm { get; set; }

        [DataMember(Name = "endDate")]
        public DateTime EndDate { get; set; }

        [DataMember(Name = "legends")]
        public string[] Legends { get; set; }

        [DataMember(Name = "period")]
        public string Period { get; set; }

        [DataMember(Name = "startDate")]
        public DateTime StartDate { get; set; }
    }
}