// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace Dnn.PersonaBar.Servers.Services.Dto
{
    public class UpdatePerfSettingsRequest
    {
        public string CachingProvider { get; set; }
        public string PageStatePersistence { get; set; }

        public string ModuleCacheProvider { get; set; }

        public string PageCacheProvider { get; set; }

        public string CacheSetting { get; set; }

        public string AuthCacheability { get; set; }

        public string UnauthCacheability { get; set; }
        public bool SslForCacheSynchronization { get; set; }
        public string ClientResourcesManagementMode { get; set; }
        public string CurrentHostVersion { get; set; }
        public bool HostEnableCompositeFiles { get; set; }
        public bool HostMinifyCss { get; set; }
        public bool HostMinifyJs { get; set; }
public string CurrentPortalVersion { get; set; }
        public bool PortalEnableCompositeFiles { get; set; }
        public bool PortalMinifyCss { get; set; }
        public bool PortalMinifyJs { get; set; }
    }
}
