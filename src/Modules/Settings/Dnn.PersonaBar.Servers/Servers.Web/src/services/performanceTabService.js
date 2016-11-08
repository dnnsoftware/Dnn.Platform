import serviceFramework from "./serviceFramework";

const convertKeyValueToLabelValue = function (list) {
    return list.map((item) => {
        return {
            value: item.Value,
            label: item.Key
        };
    });
};

const getPerformanceSettings = function () {
    return serviceFramework.get("ServerSettingsPerformance", "GetPerformanceSettings")
        .then(response => {
            return {
                cachingProvider: response.CachingProvider,
                pageStatePersistence: response.PageStatePersistence,
                moduleCacheProvider: response.ModuleCacheProvider,
                pageCacheProvider: response.PageCacheProvider,
                cacheSetting: response.CacheSetting,
                authCacheability: response.AuthCacheability,
                unauthCacheability: response.UnauthCacheability,
                sslForCacheSynchronization: response.SslForCacheSynchronization,
                clientResourcesManagementMode: response.ClientResourcesManagementMode,
                
                currentHostVersion: response.CurrentHostVersion,
                hostEnableCompositeFiles: response.HostEnableCompositeFiles,
                hostMinifyCss: response.HostMinifyCss,
                HostMinifyJs: response.HostMinifyJs,
                
                currentPortalVersion: response.CurrentPortalVersion,
                portalEnableCompositeFiles: response.PortalEnableCompositeFiles,
                portalMinifyCss: response.PortalMinifyCss,
                portalMinifyJs: response.PortalMinifyJs,
                
                cachingProviderOptions: convertKeyValueToLabelValue(response.CachingProviderOptions),
                pageStatePersistenceOptions: convertKeyValueToLabelValue(response.PageStatePersistenceOptions),
                moduleCacheProviders: convertKeyValueToLabelValue(response.ModuleCacheProviders),
                pageCacheProviders: convertKeyValueToLabelValue(response.PageCacheProviders),
                cacheSettingOptions: convertKeyValueToLabelValue(response.CacheSettingOptions),
                authCacheabilityOptions: convertKeyValueToLabelValue(response.AuthCacheabilityOptions),
                unauthCacheabilityOptions: convertKeyValueToLabelValue(response.UnauthCacheabilityOptions)
            };
        }
    );
};

const performanceTabService = {
    getPerformanceSettings: getPerformanceSettings
};

export default performanceTabService; 