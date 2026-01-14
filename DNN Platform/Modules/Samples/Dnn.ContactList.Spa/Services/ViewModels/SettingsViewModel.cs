// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using Newtonsoft.Json;

namespace Dnn.ContactList.Spa.Services.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SettingsViewModel
    {
        public SettingsViewModel()
        {
            
        }

        [JsonProperty("isFormEnabled")]
        public bool IsFormEnabled { get; set; }
    }
}
