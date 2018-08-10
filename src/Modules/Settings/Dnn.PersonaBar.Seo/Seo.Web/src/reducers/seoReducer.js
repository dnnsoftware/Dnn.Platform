import { seo as ActionTypes } from "../constants/actionTypes";

export default function seoSettings(state = {
}, action) {
    switch (action.type) {
        case ActionTypes.RETRIEVED_SEO_GENERAL_SETTINGS:
            return { ...state,
                generalSettings: action.data.generalSettings,
                replacementCharacterList: action.data.replacementCharacterList,
                deletedPageHandlingTypes: action.data.deletedPageHandlingTypes,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SEO_GENERAL_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                generalSettings: action.data.generalSettings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SEO_GENERAL_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        case ActionTypes.RETRIEVED_SEO_REGEX_SETTINGS:
            return { ...state,
                regexSettings: action.data.regexSettings,
                regexClientModified: action.data.regexClientModified
            };
        case ActionTypes.SEO_REGEX_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                regexSettings: action.data.regexSettings,
                regexClientModified: action.data.regexClientModified
            };
        case ActionTypes.UPDATED_SEO_REGEX_SETTINGS:
            return { ...state,
                regexClientModified: action.data.regexClientModified
            };
        case ActionTypes.TESTED_SEO_PAGE_URL:
        case ActionTypes.CLEARED_SEO_TEST_PAGE_URL_RESULTS:
            return { ...state,
                urls: action.data.urls
            };
        case ActionTypes.TESTED_SEO_URL_REWRITING:
        case ActionTypes.CLEARED_SEO_TEST_URL_REWRITING_RESULTS:
            return { ...state,
                rewritingResult: action.data.rewritingResult,
                culture: action.data.culture,
                identifiedPage: action.data.identifiedPage,
                redirectionReason: action.data.redirectionReason,
                redirectionResult: action.data.redirectionResult,
                operationMessages: action.data.operationMessages
            };
        case ActionTypes.RETRIEVED_SEO_SITEMAP_SETTINGS:
            return { ...state,
                sitemapSettings: action.data.sitemapSettings,
                searchEngineUrls: action.data.searchEngineUrls,
                clientModified: action.data.clientModified
            };
        case ActionTypes.UPDATED_SEO_SITEMAP_SETTINGS:
            return { ...state,
                clientModified: action.data.clientModified
            };
        case ActionTypes.SEO_SITEMAP_SETTINS_CLIENT_MODIFIED:
            return { ...state,
                sitemapSettings: action.data.sitemapSettings,
                clientModified: action.data.clientModified
            };
        case ActionTypes.RETRIEVED_SEO_SITEMAP_PROVIDERS:
            return { ...state,
                sitemapProviders: action.data.providers
            };
        case ActionTypes.RETRIEVED_SEO_EXTENSION_URL_PROVIDERS:
            return { ...state,
                extensionUrlProviders: action.data.providers
            };
        default:
            return { ...state
            };
    }
}
