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
                portalName: response.PortalName,
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
                hostMinifyJs: response.HostMinifyJs,
                
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
        });
};

const save = function (performanceSettings) {
    const request = {
        CachingProvider: performanceSettings.cachingProvider, 
        PageStatePersistence: performanceSettings.pageStatePersistence, 
        ModuleCacheProvider: performanceSettings.moduleCacheProvider, 
        PageCacheProvider: performanceSettings.pageCacheProvider, 
        CacheSetting: performanceSettings.cacheSetting, 
        AuthCacheability: performanceSettings.authCacheability, 
        UnauthCacheability: performanceSettings.unauthCacheability, 
        SslForCacheSynchronization: performanceSettings.sslForCacheSynchronization, 
        ClientResourcesManagementMode: performanceSettings.clientResourcesManagementMode
    };
    
    if (performanceSettings.clientResourcesManagementMode === "h") {
        request.CurrentHostVersion = performanceSettings.currentHostVersion; 
        request.HostEnableCompositeFiles = performanceSettings.hostEnableCompositeFiles; 
        request.HostMinifyCss = performanceSettings.hostMinifyCss; 
        request.HostMinifyJs = performanceSettings.hostMinifyJs; 
        
    } else {
        request.CurrentPortalVersion = performanceSettings.currentPortalVersion; 
        request.PortalEnableCompositeFiles = performanceSettings.portalEnableCompositeFiles; 
        request.PortalMinifyCss = performanceSettings.portalMinifyCss; 
        request.PortalMinifyJs = performanceSettings.portalMinifyJs;
    }
    
    return serviceFramework.post("ServerSettingsPerformance", "UpdatePerformanceSettings", request);
};

const incrementVersion = function (version, isGlobalSetting) {
    if (isGlobalSetting) {
        return serviceFramework.post("ServerSettingsPerformance", "IncrementHostVersion");
    }
    
    return serviceFramework.post("ServerSettingsPerformance", "IncrementPortalVersion");
};

const performanceTabService = {
    getPerformanceSettings: getPerformanceSettings,
    save: save,
    incrementVersion: incrementVersion
};

export default performanceTabService; 