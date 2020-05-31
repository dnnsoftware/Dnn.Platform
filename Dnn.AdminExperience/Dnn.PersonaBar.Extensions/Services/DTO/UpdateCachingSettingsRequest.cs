// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class UpdateCachingSettingsRequest
    {
        public string CachingProvider { get; set; }

        public bool UseSSL { get; set; }
    }
}
