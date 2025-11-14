// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>Represents the response from the update service with information about the latest DNN version.</summary>
    [DataContract]
    [Serializable]
    public class FrameworkQueryDTO
    {
        /// <summary>Gets or sets a value indicating whether the current framework version is up-to-date.</summary>
        [DataMember(Name = "UpToDate")]
        public bool UpToDate { get; set; } = true;

        /// <summary>Gets or sets the latest DNN version number, in <c>"XXYYZZ"</c> format.</summary>
        [DataMember(Name = "Version")]
        public string Version { get; set; } = string.Empty;

        /// <summary>Gets or sets the URL with information about the latest DNN version.</summary>
        [DataMember(Name = "Url")]
        public string Url { get; set; } = string.Empty;

        /// <summary>Gets or sets a value indicating whether the current framework version has a critical issue.</summary>
        [DataMember(Name = "Critical")]
        public bool IsCritical { get; set; } = false;

        /// <summary>Gets or sets the date the latest DNN version was published.</summary>
        [DataMember(Name = "Published")]
        public DateTime Published { get; set; }
    }
}
