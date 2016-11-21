import LanguageService from "services/languageService";

const languagesActions = {
    getLanguages(tabId, callback) {
        return () => {
            LanguageService.getLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageTranslatable(tabId, callback) {
        return () => {
            LanguageService.makePageTranslatable(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageNeutral(tabId, callback) {
        return () => {
            LanguageService.makePageNeutral(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addMissingLanguages(tabId, callback) {
        return () => {
            LanguageService.addMissingLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    notifyTranslators(params, callback) {
        return () => {
            LanguageService.notifyTranslators(params, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateTabLocalization(params, callback, failureCallback) {
        return () => {
            LanguageService.updateTabLocalization(params, data => {
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
    deleteModule(params, callback, failureCallback) {
        return () => {
            LanguageService.deleteModule(params, data => {
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
    restoreModule(params, callback, failureCallback) {
        return () => {
            LanguageService.restoreModule(params, data => {
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

export default languagesActions;
