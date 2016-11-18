import { languagesActionTypes as ActionTypes } from "../constants/actionTypes";
import LanguageService from "services/languageService";

const languagesActions = {
    getLanguages(tabId, callback) {
        return (dispatch) => {
            LanguageService.getLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageTranslatable(tabId, callback) {
        return (dispatch) => {
            LanguageService.makePageTranslatable(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    makePageNeutral(tabId, callback) {
        return (dispatch) => {
            LanguageService.makePageNeutral(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    addMissingLanguages(tabId, callback) {
        return (dispatch) => {
            LanguageService.addMissingLanguages(tabId, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    notifyTranslators(params, callback) {
        return (dispatch) => {
            LanguageService.notifyTranslators(params, data => {
                if (callback) {
                    callback(data);
                }
            });
        };
    },
    updateTabLocalization(params, callback, failureCallback) {
        return (dispatch) => {
            LanguageService.updateTabLocalization(params, data => {
                if (callback) {
                    callback(data);
                }
            }, data => {
                if(failureCallback) {
                    failureCallback();
                }
            });
        };
    }
};

export default languagesActions;
