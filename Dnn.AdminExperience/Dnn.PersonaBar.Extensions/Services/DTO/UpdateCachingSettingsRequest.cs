// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class UpdateCachingSettingsRequest
    {
        public string CachingProvider { get; set; }

        public bool UseSSL { get; set; }
    }
}
