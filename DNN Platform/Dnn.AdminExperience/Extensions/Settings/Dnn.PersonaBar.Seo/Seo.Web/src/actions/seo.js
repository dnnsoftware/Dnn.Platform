import { seo as ActionTypes } from "../constants/actionTypes";
import ApplicationService from "../services/applicationService";
const siteInfoActions = {
    getGeneralSettings(callback) {
        return (dispatch) => {
            ApplicationService.getGeneralSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS,
                    data: {
                        generalSettings: data.Settings,
                        replacementCharacterList: data.ReplacementCharacterList,
                        deletedPageHandlingTypes: data.DeletedPageHandlingTypes,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateGeneralSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateGeneralSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_GENERAL_SETTINGS,
                    data: {
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    generalSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED,
                data: {
                    generalSettings: parameter,
                    clientModified: true
                }
            });
        };
    },
    getRegexSettings(callback) {
        return (dispatch) => {
            ApplicationService.getRegexSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_REGEX_SETTINGS,
                    data: {
                        regexSettings: data.Settings,
                        regexClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateRegexSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateRegexSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_REGEX_SETTINGS,
                    data: {
                        regexClientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    regexSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_REGEX_SETTINS_CLIENT_MODIFIED,
                data: {
                    regexSettings: parameter,
                    regexClientModified: true
                }
            });
        };
    },
    testUrl(pageId, queryString, customPageName, callback) {
        return (dispatch) => {
            ApplicationService.testUrl(pageId, queryString, customPageName, data => {
                dispatch({
                    type: ActionTypes.TESTED_SEO_PAGE_URL,
                    data: {
                        urls: data.Urls.join("\n")
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    testUrlRewrite(uri, callback) {
        return (dispatch) => {
            ApplicationService.testUrlRewrite(uri, data => {
                dispatch({
                    type: ActionTypes.TESTED_SEO_URL_REWRITING,
                    data: {
                        rewritingResult: data.RewritingResult.rewritingResult,
                        culture: data.RewritingResult.culture,
                        identifiedPage: data.RewritingResult.identifiedPage,
                        redirectionReason: data.RewritingResult.redirectionReason,
                        redirectionResult: data.RewritingResult.redirectionResult,
                        operationMessages: data.RewritingResult.operationMessages
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    clearUrlTestResults() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEARED_SEO_TEST_PAGE_URL_RESULTS,
                data: {
                    urls: ""
                }
            });
        };
    },
    clearUrlRewritingTestResults() {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.CLEARED_SEO_TEST_URL_REWRITING_RESULTS,
                data: {
                    rewritingResult: "",
                    culture: "",
                    identifiedPage: "",
                    redirectionReason: "",
                    redirectionResult: "",
                    operationMessages: ""
                }
            });
        };
    },
    getSitemapSettings(callback) {
        return (dispatch) => {
            ApplicationService.getSitemapSettings(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_SITEMAP_SETTINGS,
                    data: {
                        sitemapSettings: data.Settings,
                        searchEngineUrls: data.SearchEngineUrls,
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSitemapSettings(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateSitemapSettings(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_SITEMAP_SETTINGS,
                    data: {
                        clientModified: false
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    sitemapSettingsClientModified(parameter) {
        return (dispatch) => {
            dispatch({
                type: ActionTypes.SEO_SITEMAP_SETTINS_CLIENT_MODIFIED,
                data: {
                    sitemapSettings: parameter,
                    clientModified: true
                }
            });
        };
    },
    clearCache(callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.clearCache(data => {
                dispatch({
                    type: ActionTypes.CLEARED_SEO_SITEMAP_CACHE,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getSitemapProviders(callback) {
        return (dispatch) => {
            ApplicationService.getSitemapProviders(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_SITEMAP_PROVIDERS,
                    data: {
                        providers: data.Providers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateSitemapProvider(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateSitemapProvider(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_SITEMAP_PROVIDER,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    getExtensionUrlProviders(callback) {
        return (dispatch) => {
            ApplicationService.getExtensionUrlProviders(data => {
                dispatch({
                    type: ActionTypes.RETRIEVED_SEO_EXTENSION_URL_PROVIDERS,
                    data: {
                        providers: data.Providers
                    }
                });
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateExtensionUrlProviderStatus(payload, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.updateExtensionUrlProviderStatus(payload, data => {
                dispatch({
                    type: ActionTypes.UPDATED_SEO_EXTENSION_URL_PROVIDER,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    },
    createVerification(verification, callback, failureCallback) {
        return (dispatch) => {
            ApplicationService.createVerification(verification, data => {
                dispatch({
                    type: ActionTypes.CREATED_SEO_SITEMAP_VERIFICATION,
                    data: {
                        
                    }
                });
                if (callback) {
                    callback(data);
                }
            }, data => {
                if (failureCallback) {
                    failureCallback(data);
                }
            });
        };
    }
};

export default siteInfoActions;
